﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using BaiRong.Core;
using BaiRong.Core.Auth;
using BaiRong.Core.Auth.JWT;
using BaiRong.Core.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SiteServer.Plugin.Models;

namespace SiteServer.CMS.Core
{
    public class RequestBody : IRequestContext
    {
        private const string UserAccessToken = "ss_user_access_token";
        private const string AdministratorAccessToken = "ss_administrator_access_token";
        private const int AccessTokenExpireDays = 7;

        private JObject _postData;
        public JObject PostData
        {
            get
            {
                if (_postData != null) return _postData;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
                var raw = bodyStream.ReadToEnd();
                _postData = !string.IsNullOrEmpty(raw) ? JObject.Parse(raw) : new JObject();
                return _postData;
            }
        }

        public HttpRequest Request => HttpContext.Current.Request;

        public string UserName { get; private set; }

        public string AdministratorName { get; private set; }

        public bool IsUserLoggin => !string.IsNullOrEmpty(UserName);

        public bool IsAdministratorLoggin => !string.IsNullOrEmpty(AdministratorName);

        public int PublishmentSystemId => GetQueryInt("publishmentSystemId");

        public int ChannelId => GetQueryInt("channelId");

        public int ContentId => GetQueryInt("contentId");

        public string ReturnUrl => GetQueryString("ReturnUrl");

        private UserInfo _userInfo;
        public UserInfo UserInfo
        {
            get
            {
                if (_userInfo != null) return _userInfo;

                if (!string.IsNullOrEmpty(UserName))
                {
                    _userInfo = BaiRongDataProvider.UserDao.GetUserInfoByUserName(UserName);
                }
                return _userInfo ?? (_userInfo = new UserInfo());
            }
        }

        private AdministratorInfo _administratorInfo;
        public AdministratorInfo AdministratorInfo
        {
            get
            {
                if (_administratorInfo != null) return _administratorInfo;

                if (!string.IsNullOrEmpty(AdministratorName))
                {
                    _administratorInfo = AdminManager.GetAdminInfo(AdministratorName);
                }
                return _administratorInfo ?? (_administratorInfo = new AdministratorInfo());
            }
        }

        public RequestBody()
        {
            var request = HttpContext.Current.Request;

            UserAuthentication(request);
            AdministratorAuthentication(request);
        }

        public bool IsQueryExists(string name)
        {
            return HttpContext.Current.Request.QueryString[name] != null;
        }

        public string GetQueryString(string name)
        {
            return !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[name]) ? PageUtils.FilterSql(HttpContext.Current.Request.QueryString[name]) : null;
        }

        public int GetQueryInt(string name, int defaultValue = 0)
        {
            return !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[name]) ? TranslateUtils.ToInt(HttpContext.Current.Request.QueryString[name]) : defaultValue;
        }

        public decimal GetQueryDecimal(string name, decimal defaultValue = 0)
        {
            return !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[name]) ? TranslateUtils.ToDecimal(HttpContext.Current.Request.QueryString[name]) : defaultValue;
        }

        public bool GetQueryBool(string name, bool defaultValue = false)
        {
            var str = HttpContext.Current.Request.QueryString[name];
            var retval = !string.IsNullOrEmpty(str) ? TranslateUtils.ToBool(str) : defaultValue;
            return retval;
        }

        public T GetPostObject<T>(string name = "")
        {
            string json;
            if (string.IsNullOrEmpty(name))
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
                json = bodyStream.ReadToEnd();
            }
            else
            {
                json = GetPostString(name);
            }
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var timeFormat = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
            };
            settings.Converters.Add(timeFormat);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public string GetPostString(string name)
        {
            return PostData[name]?.ToString();
        }

        public int GetPostInt(string name, int defaultValue = 0)
        {
            return TranslateUtils.ToInt(PostData[name]?.ToString(), defaultValue);
        }

        public decimal GetPostDecimal(string name, decimal defaultValue = 0)
        {
            return TranslateUtils.ToDecimal(PostData[name]?.ToString(), defaultValue);
        }

        public bool GetPostBool(string name, bool defaultValue = false)
        {
            return TranslateUtils.ToBool(PostData[name]?.ToString(), defaultValue);
        }

        public NameValueCollection GetPostCollection()
        {
            var formCollection = new NameValueCollection();
            foreach (var item in PostData)
            {
                formCollection[item.Key] = item.Value.ToString();
            }
            return formCollection;
        }

        protected void UserAuthentication(HttpRequest request)
        {
            var userTokenStr = string.Empty;
            if (!string.IsNullOrEmpty(CookieUtils.GetCookie(UserAccessToken)))
            {
                userTokenStr = CookieUtils.GetCookie(UserAccessToken);
            }
            else if (!string.IsNullOrEmpty(request.Headers.Get(UserAccessToken)))
            {
                userTokenStr = request.Headers.Get(UserAccessToken);
            }
            else if (!string.IsNullOrEmpty(request.QueryString[UserAccessToken]))
            {
                userTokenStr = request.QueryString[UserAccessToken];
            }

            if (string.IsNullOrEmpty(userTokenStr)) return;

            UserName = GetUserToken(userTokenStr).UserName;
        }

        protected void AdministratorAuthentication(HttpRequest request)
        {
            var administratorTokenStr = string.Empty;
            if (!string.IsNullOrEmpty(CookieUtils.GetCookie(AdministratorAccessToken)))
            {
                administratorTokenStr = CookieUtils.GetCookie(AdministratorAccessToken);
            }
            else if (!string.IsNullOrEmpty(request.Headers.Get(AdministratorAccessToken)))
            {
                administratorTokenStr = request.Headers.Get(AdministratorAccessToken);
            }
            else if (!string.IsNullOrEmpty(request.QueryString[AdministratorAccessToken]))
            {
                administratorTokenStr = request.QueryString[AdministratorAccessToken];
            }

            if (string.IsNullOrEmpty(administratorTokenStr)) return;

            AdministratorName = GetAdministratorToken(administratorTokenStr).AdministratorName;
        }

        public static UserToken GetUserToken(string tokenStr)
        {
            if (string.IsNullOrEmpty(tokenStr)) return new UserToken();

            try
            {
                var userToken = JsonWebToken.DecodeToObject<UserToken>(tokenStr, WebConfigUtils.SecretKey);

                if (userToken.AddDate.AddDays(AccessTokenExpireDays) > DateTime.Now)
                {
                    return userToken;
                }
            }
            catch
            {
                // ignored
            }
            return new UserToken();
        }

        public static AdministratorToken GetAdministratorToken(string tokenStr)
        {
            if (string.IsNullOrEmpty(tokenStr)) return new AdministratorToken();

            try
            {
                var userToken = JsonWebToken.DecodeToObject<AdministratorToken>(tokenStr, WebConfigUtils.SecretKey);

                if (userToken.AddDate.AddDays(AccessTokenExpireDays) > DateTime.Now)
                {
                    return userToken;
                }
            }
            catch
            {
                // ignored
            }
            return new AdministratorToken();
        }

        public static string GetUserTokenStr(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return null;

            var userToken = new UserToken
            {
                UserName = userName,
                AddDate = DateTime.Now
            };

            return JsonWebToken.Encode(userToken, WebConfigUtils.SecretKey, JwtHashAlgorithm.HS256);
        }

        public static string GetAdministratorTokenStr(string administratorName)
        {
            if (string.IsNullOrEmpty(administratorName)) return null;

            var administratorToken = new AdministratorToken()
            {
                AdministratorName = administratorName,
                AddDate = DateTime.Now
            };

            return JsonWebToken.Encode(administratorToken, WebConfigUtils.SecretKey, JwtHashAlgorithm.HS256);
        }

        public static string CurrentUserName
        {
            get
            {
                var userTokenStr = string.Empty;
                if (!string.IsNullOrEmpty(CookieUtils.GetCookie(UserAccessToken)))
                {
                    userTokenStr = CookieUtils.GetCookie(UserAccessToken);
                }

                return string.IsNullOrEmpty(userTokenStr) ? string.Empty : GetUserToken(userTokenStr).UserName;
            }
        }

        public void UserLogin(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return;

            UserName = userName;
            LogUtils.AddUserLoginLog(userName);
            CookieUtils.SetCookie(UserAccessToken, GetUserTokenStr(userName), DateTime.Now.AddDays(AccessTokenExpireDays));
        }

        public void UserLogout()
        {
            UserName = null;
            CookieUtils.Erase(UserAccessToken);
        }

        public static string CurrentAdministratorName
        {
            get
            {
                var administratorTokenStr = string.Empty;
                if (!string.IsNullOrEmpty(CookieUtils.GetCookie(AdministratorAccessToken)))
                {
                    administratorTokenStr = CookieUtils.GetCookie(AdministratorAccessToken);
                }

                return string.IsNullOrEmpty(administratorTokenStr) ? string.Empty : GetAdministratorToken(administratorTokenStr).AdministratorName;
            }
        }

        public void AdministratorLogin(string administratorName)
        {
            if (string.IsNullOrEmpty(administratorName)) return;

            AdministratorName = administratorName;
            LogUtils.AddAdminLog(administratorName, "管理员登录");
            CookieUtils.SetCookie(AdministratorAccessToken, GetAdministratorTokenStr(administratorName), DateTime.Now.AddDays(AccessTokenExpireDays));
        }

        public static void AdministratorLogout()
        {
            CookieUtils.Erase(AdministratorAccessToken);
        }

        public void AddAdminLog(string action, string summary)
        {
            LogUtils.AddAdminLog(AdministratorName, action, summary);
        }

        public void AddAdminLog(string action)
        {
            LogUtils.AddAdminLog(AdministratorName, action);
        }

        public void AddSiteLog(int publishmentSystemId, string action)
        {
            AddSiteLog(publishmentSystemId, 0, 0, action, string.Empty);
        }

        public void AddSiteLog(int publishmentSystemId, string action, string summary)
        {
            AddSiteLog(publishmentSystemId, 0, 0, action, summary);
        }

        public void AddSiteLog(int publishmentSystemId, int channelId, int contentId, string action, string summary)
        {
            if (publishmentSystemId <= 0)
            {
                LogUtils.AddAdminLog(AdministratorName, action, summary);
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(action))
                    {
                        action = StringUtils.MaxLengthText(action, 250);
                    }
                    if (!string.IsNullOrEmpty(summary))
                    {
                        summary = StringUtils.MaxLengthText(summary, 250);
                    }
                    if (channelId < 0)
                    {
                        channelId = -channelId;
                    }
                    var logInfo = new Model.LogInfo(0, publishmentSystemId, channelId, contentId, AdministratorName, PageUtils.GetIpAddress(), DateTime.Now, action, summary);
                    DataProvider.LogDao.Insert(logInfo);
                }
                catch
                {
                    // ignored
                }
            }
        }

        public void SetCookie(string name, string value, DateTime expires)
        {
            CookieUtils.SetCookie(name, value, expires);
        }

        public string GetCookie(string name)
        {
            return CookieUtils.GetCookie(name);
        }

        public bool IsCookieExists(string name)
        {
            return CookieUtils.IsExists(name);
        }
    }
}