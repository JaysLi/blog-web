﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;
using BaiRong.Core;
using BaiRong.Core.IO;
using BaiRong.Core.Net;
using SiteServer.BackgroundPages.Core;
using SiteServer.CMS.Core;
using SiteServer.CMS.Core.Security;
using SiteServer.CMS.Model.Enumerations;

namespace SiteServer.BackgroundPages.Ajax
{
    public class AjaxOtherService : Page
    {
        public const string CacheTotalCount = "_TotalCount";
        public const string CacheCurrentCount = "_CurrentCount";
        public const string CacheMessage = "_Message";

        private const string TypeGetCountArray = "GetCountArray";
        private const string TypeSiteTemplateDownload = "SiteTemplateDownload";
        private const string TypeSiteTemplateZip = "SiteTemplateZip";
        private const string TypeSiteTemplateUnZip = "SiteTemplateUnZip";
        private const string TypeGetLoadingChannels = "GetLoadingChannels";
        private const string TypePluginDownload = "PluginDownload";

        public static string GetCountArrayUrl()
        {
            return PageUtils.GetAjaxUrl(nameof(AjaxOtherService), new NameValueCollection
            {
                {"type", TypeGetCountArray }
            });
        }

        public static string GetSiteTemplateDownloadUrl()
        {
            return PageUtils.GetAjaxUrl(nameof(AjaxOtherService), new NameValueCollection
            {
                {"type", TypeSiteTemplateDownload }
            });
        }

        public static string GetPluginDownloadUrl()
        {
            return PageUtils.GetAjaxUrl(nameof(AjaxOtherService), new NameValueCollection
            {
                {"type", TypePluginDownload }
            });
        }

        public static string GetSiteTemplateDownloadParameters(string downloadUrl, string directoryName, string userKeyPrefix)
        {
            return TranslateUtils.NameValueCollectionToString(new NameValueCollection
            {
                {"downloadUrl", TranslateUtils.EncryptStringBySecretKey(downloadUrl)},
                {"directoryName", directoryName},
                {"userKeyPrefix", userKeyPrefix},
            });
        }

        public static string GetPluginDownloadParameters(string downloadUrl, string userKeyPrefix)
        {
            return TranslateUtils.NameValueCollectionToString(new NameValueCollection
            {
                {"downloadUrl", TranslateUtils.EncryptStringBySecretKey(downloadUrl)},
                {"userKeyPrefix", userKeyPrefix}
            });
        }

        public static string GetSiteTemplateZipUrl()
        {
            return PageUtils.GetAjaxUrl(nameof(AjaxOtherService), new NameValueCollection
            {
                {"type", TypeSiteTemplateZip }
            });
        }

        public static string GetSiteTemplateUnZipUrl()
        {
            return PageUtils.GetAjaxUrl(nameof(AjaxOtherService), new NameValueCollection
            {
                {"type", TypeSiteTemplateUnZip }
            });
        }

        public static string GetSiteTemplateZipParameters(string directoryName, string userKeyPrefix)
        {
            return TranslateUtils.NameValueCollectionToString(new NameValueCollection
            {
                {"directoryName", directoryName},
                {"userKeyPrefix", userKeyPrefix}
            });
        }

        public static string GetSiteTemplateUnZipParameters(string fileName, string userKeyPrefix)
        {
            return TranslateUtils.NameValueCollectionToString(new NameValueCollection
            {
                {"fileName", fileName},
                {"userKeyPrefix", userKeyPrefix}
            });
        }

        public static string GetGetLoadingChannelsUrl()
        {
            return PageUtils.GetAjaxUrl(nameof(AjaxOtherService), new NameValueCollection
            {
                {"type", TypeGetLoadingChannels }
            });
        }

        public static string GetGetLoadingChannelsParameters(int publishmentSystemId, ELoadingType loadingType, NameValueCollection additional)
        {
            return TranslateUtils.NameValueCollectionToString(new NameValueCollection
            {
                {"publishmentSystemID", publishmentSystemId.ToString() },
                {"loadingType", ELoadingTypeUtils.GetValue(loadingType)},
                {"additional", TranslateUtils.EncryptStringBySecretKey(TranslateUtils.NameValueCollectionToString(additional))}
            });
        }

        public void Page_Load(object sender, EventArgs e)
        {
            var type = Request["type"];
            var retval = new NameValueCollection();
            string retString = null;
            var body = new RequestBody();
            if (!body.IsAdministratorLoggin) return;

            if (type == TypeGetCountArray)
            {
                var userKeyPrefix = Request["userKeyPrefix"];
                retval = GetCountArray(userKeyPrefix);
            }
            else if (type == TypeSiteTemplateDownload)
            {
                var userKeyPrefix = Request["userKeyPrefix"];
                var downloadUrl = TranslateUtils.DecryptStringBySecretKey(Request["downloadUrl"]);
                var directoryName = Request["directoryName"];
                retval = SiteTemplateDownload(downloadUrl, directoryName, userKeyPrefix);
            }
            else if (type == TypeSiteTemplateZip)
            {
                var userKeyPrefix = Request["userKeyPrefix"];
                var directoryName = Request["directoryName"];
                retval = SiteTemplateZip(directoryName, userKeyPrefix);
            }
            else if (type == TypeSiteTemplateUnZip)
            {
                var userKeyPrefix = Request["userKeyPrefix"];
                var fileName = Request["fileName"];
                retval = SiteTemplateUnZip(fileName, userKeyPrefix);
            }
            else if (type == TypeGetLoadingChannels)
            {
                var publishmentSystemId = TranslateUtils.ToInt(Request["publishmentSystemID"]);
                var parentId = TranslateUtils.ToInt(Request["parentID"]);
                var loadingType = Request["loadingType"];
                var additional = Request["additional"];
                retString = GetLoadingChannels(publishmentSystemId, parentId, loadingType, additional, body);
            }
            else if (type == TypePluginDownload)
            {
                var userKeyPrefix = Request["userKeyPrefix"];
                var downloadUrl = TranslateUtils.DecryptStringBySecretKey(Request["downloadUrl"]);
                retval = PluginDownload(downloadUrl, userKeyPrefix);
            }
            //else if (type == "GetLoadingGovPublicCategories")
            //{
            //    string classCode = base.Request["classCode"];
            //    int publishmentSystemID = TranslateUtils.ToInt(base.Request["publishmentSystemID"]);
            //    int parentID = TranslateUtils.ToInt(base.Request["parentID"]);
            //    string loadingType = base.Request["loadingType"];
            //    string additional = base.Request["additional"];
            //    retString = GetLoadingGovPublicCategories(classCode, publishmentSystemID, parentID, loadingType, additional);
            //}
            //else if (type == "GetLoadingTemplates")
            //{
            //    int publishmentSystemID = TranslateUtils.ToInt(base.Request["publishmentSystemID"]);
            //    string templateType = base.Request["templateType"];
            //    retString = GetLoadingTemplates(publishmentSystemID, templateType);
            //}
            //else if (type == "StlTemplate")
            //{
            //    int publishmentSystemID = TranslateUtils.ToInt(base.Request["publishmentSystemID"]);
            //    int templateID = TranslateUtils.ToInt(base.Request["templateID"]);
            //    string includeUrl = base.Request["includeUrl"];
            //    string operation = base.Request["operation"];
            //    retval = TemplateDesignOperation.Operate(publishmentSystemID, templateID, includeUrl, operation, base.Request.Form);
            //}

            if (retString != null)
            {
                Page.Response.Write(retString);
                Page.Response.End();
            }
            else
            {
                var jsonString = TranslateUtils.NameValueCollectionToJsonString(retval);
                Page.Response.Write(jsonString);
                Page.Response.End();
            }
        }

        public NameValueCollection GetCountArray(string userKeyPrefix)//进度及显示
        {
            var retval = new NameValueCollection();
            if (CacheUtils.Get(userKeyPrefix + CacheTotalCount) != null && CacheUtils.Get(userKeyPrefix + CacheCurrentCount) != null && CacheUtils.Get(userKeyPrefix + CacheMessage) != null)
            {
                var totalCount = TranslateUtils.ToInt((string)CacheUtils.Get(userKeyPrefix + CacheTotalCount));
                var currentCount = TranslateUtils.ToInt((string)CacheUtils.Get(userKeyPrefix + CacheCurrentCount));
                var message = (string)CacheUtils.Get(userKeyPrefix + CacheMessage);
                retval = AjaxManager.GetCountArrayNameValueCollection(totalCount, currentCount, message);
            }
            return retval;
        }

        public NameValueCollection SiteTemplateDownload(string downloadUrl, string directoryName, string userKeyPrefix)
        {
            var cacheTotalCountKey = userKeyPrefix + CacheTotalCount;
            var cacheCurrentCountKey = userKeyPrefix + CacheCurrentCount;
            var cacheMessageKey = userKeyPrefix + CacheMessage;

            CacheUtils.Insert(cacheTotalCountKey, "5");//存储需要的页面总数
            CacheUtils.Insert(cacheCurrentCountKey, "0");//存储当前的页面总数
            CacheUtils.Insert(cacheMessageKey, string.Empty);//存储消息

            //返回“运行结果”和“错误信息”的字符串数组
            NameValueCollection retval;

            try
            {
                CacheUtils.Insert(cacheCurrentCountKey, "1");
                CacheUtils.Insert(cacheMessageKey, "开始下载模板压缩包，可能需要几分钟，请耐心等待");

                var filePath = PathUtility.GetSiteTemplatesPath(directoryName + ".zip");
                FileUtils.DeleteFileIfExists(filePath);
                WebClientUtils.SaveRemoteFileToLocal(downloadUrl, filePath);

                CacheUtils.Insert(cacheCurrentCountKey, "4");
                CacheUtils.Insert(cacheMessageKey, "模板压缩包下载成功，开始解压缩");

                var directoryPath = PathUtility.GetSiteTemplatesPath(directoryName);
                if (!DirectoryUtils.IsDirectoryExists(directoryPath))
                {
                    ZipUtils.UnpackFiles(filePath, directoryPath);
                }

                CacheUtils.Insert(cacheCurrentCountKey, "5");
                CacheUtils.Insert(cacheMessageKey, string.Empty);

                retval = AjaxManager.GetProgressTaskNameValueCollection("站点模板下载成功，请到站点模板管理中查看。", string.Empty);
            }
            catch (Exception ex)
            {
                retval = AjaxManager.GetProgressTaskNameValueCollection(string.Empty,
                    $@"<br />下载失败！<br />{ex.Message}");
            }

            CacheUtils.Remove(cacheTotalCountKey);//取消存储需要的页面总数
            CacheUtils.Remove(cacheCurrentCountKey);//取消存储当前的页面总数
            CacheUtils.Remove(cacheMessageKey);//取消存储消息

            return retval;
        }

        public NameValueCollection PluginDownload(string downloadUrl, string userKeyPrefix)
        {
            var cacheTotalCountKey = userKeyPrefix + CacheTotalCount;
            var cacheCurrentCountKey = userKeyPrefix + CacheCurrentCount;
            var cacheMessageKey = userKeyPrefix + CacheMessage;

            CacheUtils.Insert(cacheTotalCountKey, "5");//存储需要的页面总数
            CacheUtils.Insert(cacheCurrentCountKey, "0");//存储当前的页面总数
            CacheUtils.Insert(cacheMessageKey, string.Empty);//存储消息

            //返回“运行结果”和“错误信息”的字符串数组
            NameValueCollection retval;

            try
            {
                CacheUtils.Insert(cacheCurrentCountKey, "1");
                CacheUtils.Insert(cacheMessageKey, "开始下载插件压缩包，可能需要几分钟，请耐心等待");

                var fileName = PageUtils.GetFileNameFromUrl(downloadUrl);
                var filePath = PathUtils.GetPluginsPath(fileName);
                FileUtils.DeleteFileIfExists(filePath);
                WebClientUtils.SaveRemoteFileToLocal(downloadUrl, filePath);

                CacheUtils.Insert(cacheCurrentCountKey, "4");
                CacheUtils.Insert(cacheMessageKey, "插件压缩包下载成功，开始安装");

                ZipUtils.UnpackFiles(filePath, PathUtils.GetPluginsPath(fileName.Substring(0, fileName.IndexOf(".", StringComparison.Ordinal))));

                CacheUtils.Insert(cacheCurrentCountKey, "5");
                CacheUtils.Insert(cacheMessageKey, string.Empty);

                retval = AjaxManager.GetProgressTaskNameValueCollection("插件安装成功，请刷新页面查看。", string.Empty);
            }
            catch (Exception ex)
            {
                retval = AjaxManager.GetProgressTaskNameValueCollection(string.Empty,
                    $@"<br />下载失败！<br />{ex.Message}");
            }

            CacheUtils.Remove(cacheTotalCountKey);//取消存储需要的页面总数
            CacheUtils.Remove(cacheCurrentCountKey);//取消存储当前的页面总数
            CacheUtils.Remove(cacheMessageKey);//取消存储消息

            return retval;
        }

        public NameValueCollection SiteTemplateZip(string directoryName, string userKeyPrefix)
        {
            var cacheTotalCountKey = userKeyPrefix + CacheTotalCount;
            var cacheCurrentCountKey = userKeyPrefix + CacheCurrentCount;
            var cacheMessageKey = userKeyPrefix + CacheMessage;

            CacheUtils.Insert(cacheTotalCountKey, "1");//存储需要的页面总数
            CacheUtils.Insert(cacheCurrentCountKey, "0");//存储当前的页面总数
            CacheUtils.Insert(cacheMessageKey, string.Empty);//存储消息

            //返回“运行结果”和“错误信息”的字符串数组
            NameValueCollection retval;

            try
            {
                directoryName = PathUtils.RemoveParentPath(directoryName);
                var fileName = directoryName + ".zip";
                var filePath = PathUtility.GetSiteTemplatesPath(fileName);
                var directoryPath = PathUtility.GetSiteTemplatesPath(directoryName);

                FileUtils.DeleteFileIfExists(filePath);

                ZipUtils.PackFiles(filePath, directoryPath);

                CacheUtils.Insert(cacheCurrentCountKey, "1");//存储当前的页面总数

                retval = AjaxManager.GetProgressTaskNameValueCollection(
                    $"站点模板压缩成功，<a href='{PageUtility.GetSiteTemplatesUrl(fileName)}' target=_blank>点击下载</a>。", string.Empty);
            }
            catch (Exception ex)
            {
                retval = AjaxManager.GetProgressTaskNameValueCollection(string.Empty,
                    $@"<br />站点模板压缩失败！<br />{ex.Message}");
            }

            CacheUtils.Remove(cacheTotalCountKey);//取消存储需要的页面总数
            CacheUtils.Remove(cacheCurrentCountKey);//取消存储当前的页面总数
            CacheUtils.Remove(cacheMessageKey);//取消存储消息

            return retval;
        }

        public NameValueCollection SiteTemplateUnZip(string fileName, string userKeyPrefix)
        {
            var cacheTotalCountKey = userKeyPrefix + CacheTotalCount;
            var cacheCurrentCountKey = userKeyPrefix + CacheCurrentCount;
            var cacheMessageKey = userKeyPrefix + CacheMessage;

            CacheUtils.Insert(cacheTotalCountKey, "1");//存储需要的页面总数
            CacheUtils.Insert(cacheCurrentCountKey, "0");//存储当前的页面总数
            CacheUtils.Insert(cacheMessageKey, string.Empty);//存储消息

            //返回“运行结果”和“错误信息”的字符串数组
            NameValueCollection retval;

            try
            {
                var directoryPath = PathUtility.GetSiteTemplatesPath(PathUtils.GetFileNameWithoutExtension(fileName));
                var zipFilePath = PathUtility.GetSiteTemplatesPath(fileName);

                ZipUtils.UnpackFiles(zipFilePath, directoryPath);

                CacheUtils.Insert(cacheCurrentCountKey, "1");//存储当前的页面总数

                retval = AjaxManager.GetProgressTaskNameValueCollection("站点模板解压成功", string.Empty);
            }
            catch (Exception ex)
            {
                retval = AjaxManager.GetProgressTaskNameValueCollection(string.Empty,
                    $@"<br />站点模板解压失败！<br />{ex.Message}");
            }

            CacheUtils.Remove(cacheTotalCountKey);//取消存储需要的页面总数
            CacheUtils.Remove(cacheCurrentCountKey);//取消存储当前的页面总数
            CacheUtils.Remove(cacheMessageKey);//取消存储消息

            return retval;
        }

        public string GetLoadingChannels(int publishmentSystemId, int parentId, string loadingType, string additional, RequestBody body)
        {
            var list = new List<string>();

            var eLoadingType = ELoadingTypeUtils.GetEnumType(loadingType);

            var nodeIdList = DataProvider.NodeDao.GetNodeIdListByParentId(publishmentSystemId, parentId);

            var publishmentSystemInfo = PublishmentSystemManager.GetPublishmentSystemInfo(publishmentSystemId);

            var nameValueCollection = TranslateUtils.ToNameValueCollection(TranslateUtils.DecryptStringBySecretKey(additional));

            foreach (var nodeId in nodeIdList)
            {
                var enabled = AdminUtility.IsOwningNodeId(body.AdministratorName, nodeId);
                if (!enabled)
                {
                    if (!AdminUtility.IsHasChildOwningNodeId(body.AdministratorName, nodeId))
                    {
                        continue;
                    }
                }
                var nodeInfo = NodeManager.GetNodeInfo(publishmentSystemId, nodeId);

                list.Add(ChannelLoading.GetChannelRowHtml(publishmentSystemInfo, nodeInfo, enabled, eLoadingType, nameValueCollection, body.AdministratorName));
            }

            //arraylist.Reverse();

            var builder = new StringBuilder();
            foreach (var html in list)
            {
                builder.Append(html);
            }
            return builder.ToString();
        }
    }
}
