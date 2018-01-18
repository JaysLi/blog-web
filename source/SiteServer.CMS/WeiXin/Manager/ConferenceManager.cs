﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using BaiRong.Core;
using SiteServer.CMS.Core;
using SiteServer.CMS.Model;
using SiteServer.CMS.WeiXin.Data;
using SiteServer.CMS.WeiXin.Model;
using SiteServer.CMS.WeiXin.Model.Enumerations;
using SiteServer.CMS.WeiXin.WeiXinMP.Entities.Response;

namespace SiteServer.CMS.WeiXin.Manager
{
	public class ConferenceManager
    {
        public static string GetImageUrl(PublishmentSystemInfo publishmentSystemInfo, string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return PageUtils.AddProtocolToUrl(SiteFilesAssets.GetUrl(publishmentSystemInfo.Additional.ApiUrl, "weixin/conference/img/start.jpg"));
            }
            else
            {
                return PageUtils.AddProtocolToUrl(PageUtility.ParseNavigationUrl(publishmentSystemInfo, imageUrl));
            }
        }

        public static string GetEndImageUrl(PublishmentSystemInfo publishmentSystemInfo, string endImageUrl)
        {
            if (string.IsNullOrEmpty(endImageUrl))
            {
                return PageUtils.AddProtocolToUrl(SiteFilesAssets.GetUrl(publishmentSystemInfo.Additional.ApiUrl, "weixin/conference/img/end.jpg"));
            }
            else
            {
                return PageUtils.AddProtocolToUrl(PageUtility.ParseNavigationUrl(publishmentSystemInfo, endImageUrl));
            }
        }

        private static string GetConferenceUrl(PublishmentSystemInfo publishmentSystemInfo)
        {
            return PageUtils.AddProtocolToUrl(SiteFilesAssets.GetUrl(publishmentSystemInfo.Additional.ApiUrl, "weixin/conference/index.html"));
        }

        public static string GetConferenceUrl(PublishmentSystemInfo publishmentSystemInfo, ConferenceInfo conferenceInfo, string wxOpenID)
        {
            var attributes = new NameValueCollection();
            attributes.Add("publishmentSystemID", conferenceInfo.PublishmentSystemId.ToString());
            attributes.Add("conferenceID", conferenceInfo.Id.ToString());
            attributes.Add("wxOpenID", wxOpenID);
            return PageUtils.AddQueryString(GetConferenceUrl(publishmentSystemInfo), attributes);
        }

        public static void AddContent(int publishmentSystemID, int conferenceID, string realName, string mobile, string email, string company, string position, string note, string ipAddress, string cookieSN, string wxOpenID, string userName)
        {
            DataProviderWx.ConferenceDao.AddUserCount(conferenceID);
            var contentInfo = new ConferenceContentInfo { Id = 0, PublishmentSystemId = publishmentSystemID, ConferenceId = conferenceID, IpAddress = ipAddress, CookieSn = cookieSN, WxOpenId = wxOpenID, UserName = userName, RealName = realName, Mobile = mobile, Email = email, Company = company, Position = position, Note = note, AddDate = DateTime.Now };
            DataProviderWx.ConferenceContentDao.Insert(contentInfo);
        }

        public static List<Article> Trigger(PublishmentSystemInfo publishmentSystemInfo, Model.KeywordInfo keywordInfo, string wxOpenID)
        {
            var articleList = new List<Article>();

            DataProviderWx.CountDao.AddCount(keywordInfo.PublishmentSystemId, ECountType.RequestNews);

            var conferenceInfoList = DataProviderWx.ConferenceDao.GetConferenceInfoListByKeywordId(keywordInfo.PublishmentSystemId, keywordInfo.KeywordId);

            foreach (var conferenceInfo in conferenceInfoList)
            {
                Article article = null;

                if (conferenceInfo != null && conferenceInfo.StartDate < DateTime.Now)
                {
                    var isEnd = false;

                    if (conferenceInfo.EndDate < DateTime.Now)
                    {
                        isEnd = true;
                    }

                    if (isEnd)
                    {
                        var endImageUrl = GetEndImageUrl(publishmentSystemInfo, conferenceInfo.EndImageUrl);

                        article = new Article()
                        {
                            Title = conferenceInfo.EndTitle,
                            Description = conferenceInfo.EndSummary,
                            PicUrl = endImageUrl
                        };
                    }
                    else
                    {
                        var imageUrl = GetImageUrl(publishmentSystemInfo, conferenceInfo.ImageUrl);
                        var pageUrl = GetConferenceUrl(publishmentSystemInfo, conferenceInfo, wxOpenID);

                        article = new Article()
                        {
                            Title = conferenceInfo.Title,
                            Description = conferenceInfo.Summary,
                            PicUrl = imageUrl,
                            Url = pageUrl
                        };
                    }
                }

                if (article != null)
                {
                    articleList.Add(article);
                }
            }

            return articleList;
        }
	}
}
