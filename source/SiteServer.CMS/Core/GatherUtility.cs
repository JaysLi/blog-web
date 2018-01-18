﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using BaiRong.Core;
using BaiRong.Core.AuxiliaryTable;
using BaiRong.Core.Model.Attributes;
using BaiRong.Core.Model.Enumerations;
using BaiRong.Core.Net;
using SiteServer.CMS.Core.Create;
using SiteServer.CMS.Model;
using System.Collections.Generic;
using BaiRong.Core.Model;
using SiteServer.Plugin;
using SiteServer.Plugin.Models;

namespace SiteServer.CMS.Core
{
    public class GatherUtility
    {
        public static string GetRegexString(string normalString)
        {
            var retval = normalString;
            if (!string.IsNullOrEmpty(normalString))
            {
                var replaceChar = new[] { '\\', '^', '$', '.', '{', '[', '(', ')', ']', '}', '+', '?', '!', '#' };
                foreach (var theChar in replaceChar)
                {
                    retval = retval.Replace(theChar.ToString(), "\\" + theChar);
                }
                retval = retval.Replace("*", ".*?");
                retval = RegexUtils.Replace("\\s+", retval, "\\s+");
            }
            return retval;
        }

        public static string GetRegexArea(string normalAreaStart, string normalAreaEnd)
        {
            if (!string.IsNullOrEmpty(normalAreaStart) && !string.IsNullOrEmpty(normalAreaEnd))
            {
                return $"{GetRegexString(normalAreaStart)}\\s*(?<area>[\\s\\S]+?)\\s*{GetRegexString(normalAreaEnd)}";
            }
            return string.Empty;
        }

        public static string GetRegexUrl(string normalUrlStart, string normalUrlEnd)
        {
            if (!string.IsNullOrEmpty(normalUrlStart) && !string.IsNullOrEmpty(normalUrlEnd))
            {
                return
                    $"{GetRegexString(normalUrlStart)}(?:\"(?<url>[^\"]*)\"|'(?<url>[^']*)'|(?<url>\\S+)){GetRegexString(normalUrlEnd)}";
            }
            return string.Empty;
        }

        public static string GetRegexChannel(string normalChannelStart, string normalChannelEnd)
        {
            if (!string.IsNullOrEmpty(normalChannelStart) && !string.IsNullOrEmpty(normalChannelEnd))
            {
                return
                    $"{GetRegexString(normalChannelStart)}\\s*(?<channel>[\\s\\S]+?)\\s*{GetRegexString(normalChannelEnd)}";
            }
            return string.Empty;
        }

        public static string GetRegexTitle(string normalTitleStart, string normalTitleEnd)
        {
            if (!string.IsNullOrEmpty(normalTitleStart) && !string.IsNullOrEmpty(normalTitleEnd))
            {
                return $"{GetRegexString(normalTitleStart)}\\s*(?<title>[\\s\\S]+?)\\s*{GetRegexString(normalTitleEnd)}";
            }
            return string.Empty;
        }

        public static string GetRegexContent(string normalContentStart, string normalContentEnd)
        {
            if (!string.IsNullOrEmpty(normalContentStart) && !string.IsNullOrEmpty(normalContentEnd))
            {
                return
                    $"{GetRegexString(normalContentStart)}\\s*(?<content>[\\s\\S]+?)\\s*{GetRegexString(normalContentEnd)}";
            }
            return string.Empty;
        }

        public static string GetRegexAttributeName(string attributeName, string normalAuthorStart, string normalAuthorEnd)
        {
            if (!string.IsNullOrEmpty(normalAuthorStart) && !string.IsNullOrEmpty(normalAuthorEnd))
            {
                return
                    $"{GetRegexString(normalAuthorStart)}\\s*(?<{attributeName}>[\\s\\S]+?)\\s*{GetRegexString(normalAuthorEnd)}";
            }
            return string.Empty;
        }

        public static ArrayList GetGatherUrlArrayList(GatherRuleInfo gatherRuleInfo)
        {
            var gatherUrls = new ArrayList();
            if (gatherRuleInfo.GatherUrlIsCollection)
            {
                gatherUrls.AddRange(TranslateUtils.StringCollectionToStringList(gatherRuleInfo.GatherUrlCollection, '\n'));
            }

            if (gatherRuleInfo.GatherUrlIsSerialize)
            {
                if (gatherRuleInfo.SerializeFrom <= gatherRuleInfo.SerializeTo)
                {
                    var count = 1;
                    for (var i = gatherRuleInfo.SerializeFrom; i <= gatherRuleInfo.SerializeTo; i = i + gatherRuleInfo.SerializeInterval)
                    {
                        count++;
                        if (count > 200) break;
                        var thePageNumber = i.ToString();
                        if (gatherRuleInfo.SerializeIsAddZero && thePageNumber.Length == 1)
                        {
                            thePageNumber = "0" + i;
                        }
                        gatherUrls.Add(gatherRuleInfo.GatherUrlSerialize.Replace("*", thePageNumber));
                    }
                }

                if (gatherRuleInfo.SerializeIsOrderByDesc)
                {
                    gatherUrls.Reverse();
                }
            }

            return gatherUrls;
        }

        public static ArrayList GetContentUrlArrayList(GatherRuleInfo gatherRuleInfo, string regexListArea, string regexUrlInclude, bool isCache, string cacheMessageKey, StringBuilder errorBuilder)
        {
            var gatherUrls = GetGatherUrlArrayList(gatherRuleInfo);
            var contentUrls = new ArrayList();
            foreach (string gatherUrl in gatherUrls)
            {
                if (isCache)
                {
                    CacheUtils.Insert(cacheMessageKey, "获取链接" + gatherUrl);//存储消息
                }
                contentUrls.AddRange(GetContentUrls(gatherUrl, gatherRuleInfo.Charset, gatherRuleInfo.CookieString, regexListArea, regexUrlInclude, errorBuilder));
            }

            if (gatherRuleInfo.Additional.IsOrderByDesc)
            {
                contentUrls.Reverse();
            }
            return contentUrls;
        }

        public static ArrayList GetContentUrls(string gatherUrl, ECharset charset, string cookieString, string regexListArea, string regexUrlInclude, StringBuilder errorBuilder)
        {
            var contentUrls = new ArrayList();
            try
            {
                var listHtml = WebClientUtils.GetRemoteFileSource(gatherUrl, charset, cookieString);
                var areaHtml = string.Empty;

                if (!string.IsNullOrEmpty(regexListArea))
                {
                    areaHtml = RegexUtils.GetContent("area", regexListArea, listHtml);
                }
                var urlsList = RegexUtils.GetUrls(!string.IsNullOrEmpty(areaHtml) ? areaHtml : listHtml, gatherUrl);

                var isInclude = !string.IsNullOrEmpty(regexUrlInclude);

                foreach (var url in urlsList)
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        var contentUrl = url.Replace("&amp;", "&");
                        if (isInclude && !RegexUtils.IsMatch(regexUrlInclude, contentUrl))
                        {
                            continue;
                        }
                        if (!contentUrls.Contains(contentUrl))
                        {
                            contentUrls.Add(contentUrl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorBuilder.Append("<br/>").Append(ex.Message);
                LogUtils.AddErrorLog(ex);
            }
            return contentUrls;
        }

        public static NameValueCollection GetContentNameValueCollection(ECharset charset, string url, string cookieString, string regexContentExclude, string contentHtmlClearCollection, string contentHtmlClearTagCollection, string regexTitle, string regexContent, string regexContent2, string regexContent3, string regexNextPage, string regexChannel, List<string> contentAttributes, NameValueCollection contentAttributesXml)
        {
            var attributes = new NameValueCollection();

            var contentHtml = WebClientUtils.GetRemoteFileSource(url, charset, cookieString);
            var title = RegexUtils.GetContent("title", regexTitle, contentHtml);
            var content = RegexUtils.GetContent("content", regexContent, contentHtml);
            if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(regexContent2))
            {
                content = RegexUtils.GetContent("content", regexContent2, contentHtml);
            }
            if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(regexContent3))
            {
                content = RegexUtils.GetContent("content", regexContent3, contentHtml);
            }

            if (!string.IsNullOrEmpty(regexContentExclude))
            {
                content = RegexUtils.Replace(regexContentExclude, content, string.Empty);
            }
            if (!string.IsNullOrEmpty(contentHtmlClearCollection))
            {
                var htmlClearArrayList = TranslateUtils.StringCollectionToStringList(contentHtmlClearCollection);
                foreach (string htmlClear in htmlClearArrayList)
                {
                    string clearRegex = $@"<{htmlClear}[^>]*>.*?<\/{htmlClear}>";
                    content = RegexUtils.Replace(clearRegex, content, string.Empty);
                }
            }
            if (!string.IsNullOrEmpty(contentHtmlClearTagCollection))
            {
                var htmlClearTagArrayList = TranslateUtils.StringCollectionToStringList(contentHtmlClearTagCollection);
                foreach (string htmlClearTag in htmlClearTagArrayList)
                {
                    string clearRegex = $@"<{htmlClearTag}[^>]*>";
                    content = RegexUtils.Replace(clearRegex, content, string.Empty);
                    clearRegex = $@"<\/{htmlClearTag}>";
                    content = RegexUtils.Replace(clearRegex, content, string.Empty);
                }
            }

            var contentNextPageUrl = RegexUtils.GetUrl(regexNextPage, contentHtml, url);
            if (!string.IsNullOrEmpty(contentNextPageUrl))
            {
                content = GetPageContent(content, charset, contentNextPageUrl, cookieString, regexContentExclude, contentHtmlClearCollection, contentHtmlClearTagCollection, regexContent, regexContent2, regexContent3, regexNextPage);
            }

            var channel = RegexUtils.GetContent("channel", regexChannel, contentHtml);

            attributes.Add("title", title);
            attributes.Add("channel", channel);
            attributes.Add("content", StringUtils.HtmlEncode(content));

            foreach (string attributeName in contentAttributes)
            {
                var normalStart = StringUtils.ValueFromUrl(contentAttributesXml[attributeName + "_ContentStart"]);
                var normalEnd = StringUtils.ValueFromUrl(contentAttributesXml[attributeName + "_ContentEnd"]);
                var regex = GetRegexAttributeName(attributeName, normalStart, normalEnd);
                var value = RegexUtils.GetContent(attributeName, regex, contentHtml);
                attributes.Set(attributeName, value);
            }

            return attributes;
        }

        public static bool GatherOneByUrl(string administratorName, PublishmentSystemInfo publishmentSystemInfo, NodeInfo nodeInfo, bool isSaveImage, bool isSetFirstImageAsImageUrl, bool isEmptyContentAllowed, bool isSameTitleAllowed, bool isChecked, ECharset charset, string url, string cookieString, string regexTitleInclude, string regexContentExclude, string contentHtmlClearCollection, string contentHtmlClearTagCollection, string contentReplaceFrom, string contentReplaceTo, string regexTitle, string regexContent, string regexContent2, string regexContent3, string regexNextPage, string regexChannel, List<string> contentAttributes, NameValueCollection contentAttributesXml, Hashtable contentTitleHashtable, List<int[]> nodeIdAndContentIdList, bool isCache, string cacheMessageKey)
        {
            try
            {
                // TODO:采集文件、链接标题为内容标题、链接提示为内容标题
                //string extension = PathUtils.GetExtension(url);
                //if (!EFileSystemTypeUtils.IsTextEditable(extension))
                //{
                //    if (EFileSystemTypeUtils.IsImageOrFlashOrPlayer(extension))
                //    {

                //    }
                //}
                var tableName = NodeManager.GetTableName(publishmentSystemInfo, nodeInfo);
                var contentHtml = WebClientUtils.GetRemoteFileSource(url, charset, cookieString);
                var title = RegexUtils.GetContent("title", regexTitle, contentHtml);
                var content = RegexUtils.GetContent("content", regexContent, contentHtml);
                if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(regexContent2))
                {
                    content = RegexUtils.GetContent("content", regexContent2, contentHtml);
                }
                if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(regexContent3))
                {
                    content = RegexUtils.GetContent("content", regexContent3, contentHtml);
                }

                //如果标题或内容为空，返回false并退出
                if (string.IsNullOrEmpty(title))
                {
                    return false;
                }
                if (isEmptyContentAllowed == false && string.IsNullOrEmpty(content))
                {
                    return false;
                }

                title = StringUtils.StripTags(title);

                if (!string.IsNullOrEmpty(regexTitleInclude))
                {
                    if (RegexUtils.IsMatch(regexTitleInclude, title) == false)
                    {
                        return false;
                    }
                }
                if (!string.IsNullOrEmpty(regexContentExclude))
                {
                    content = RegexUtils.Replace(regexContentExclude, content, string.Empty);
                }
                if (!string.IsNullOrEmpty(contentHtmlClearCollection))
                {
                    var htmlClearArrayList = TranslateUtils.StringCollectionToStringList(contentHtmlClearCollection);
                    foreach (string htmlClear in htmlClearArrayList)
                    {
                        string clearRegex = $@"<{htmlClear}[^>]*>.*?<\/{htmlClear}>";
                        content = RegexUtils.Replace(clearRegex, content, string.Empty);
                    }
                }
                if (!string.IsNullOrEmpty(contentHtmlClearTagCollection))
                {
                    var htmlClearTagArrayList = TranslateUtils.StringCollectionToStringList(contentHtmlClearTagCollection);
                    foreach (string htmlClearTag in htmlClearTagArrayList)
                    {
                        string clearRegex = $@"<{htmlClearTag}[^>]*>";
                        content = RegexUtils.Replace(clearRegex, content, string.Empty);
                        clearRegex = $@"<\/{htmlClearTag}>";
                        content = RegexUtils.Replace(clearRegex, content, string.Empty);
                    }
                }

                if (!string.IsNullOrEmpty(contentReplaceFrom))
                {
                    var froms = TranslateUtils.StringCollectionToStringCollection(contentReplaceFrom);
                    var isMulti = false;
                    if (!string.IsNullOrEmpty(contentReplaceTo) && contentReplaceTo.IndexOf(',') != -1)
                    {
                        if (StringUtils.GetCount(",", contentReplaceTo) + 1 == froms.Count)
                        {
                            isMulti = true;
                        }
                    }
                    if (isMulti == false)
                    {
                        foreach (var from in froms)
                        {
                            title = RegexUtils.Replace($"({from.Replace(" ", "\\s")})(?!</a>)(?![^><]*>)", title, contentReplaceTo);
                            content = RegexUtils.Replace($"({from.Replace(" ", "\\s")})(?!</a>)(?![^><]*>)", content, contentReplaceTo);
                        }
                    }
                    else
                    {
                        var tos = TranslateUtils.StringCollectionToStringCollection(contentReplaceTo);
                        for (var i = 0; i < froms.Count; i++)
                        {
                            title = RegexUtils.Replace($"({froms[i].Replace(" ", "\\s")})(?!</a>)(?![^><]*>)", title, tos[i]);
                            content = RegexUtils.Replace($"({froms[i].Replace(" ", "\\s")})(?!</a>)(?![^><]*>)", content, tos[i]);
                        }
                    }
                }

                var contentNextPageUrl = RegexUtils.GetUrl(regexNextPage, contentHtml, url);
                if (!string.IsNullOrEmpty(contentNextPageUrl))
                {
                    try
                    {
                        content = GetPageContent(content, charset, contentNextPageUrl, cookieString, regexContentExclude, contentHtmlClearCollection, contentHtmlClearTagCollection, regexContent, regexContent2, regexContent3, regexNextPage);
                    }
                    catch
                    {
                        return false;
                    }
                }

                var channel = RegexUtils.GetContent("channel", regexChannel, contentHtml);
                var channelId = nodeInfo.NodeId;
                if (!string.IsNullOrEmpty(channel))
                {
                    var nodeIdByNodeName = DataProvider.NodeDao.GetNodeIdByParentIdAndNodeName(publishmentSystemInfo.PublishmentSystemId, nodeInfo.NodeId, channel, false);
                    channelId = nodeIdByNodeName == 0 ? DataProvider.NodeDao.InsertNodeInfo(publishmentSystemInfo.PublishmentSystemId, nodeInfo.NodeId, channel, string.Empty, nodeInfo.ContentModelId) : nodeIdByNodeName;
                }

                if (!isSameTitleAllowed)
                {
                    var contentTitles = contentTitleHashtable[channelId] as List<string> ??
                                        BaiRongDataProvider.ContentDao.GetValueList(tableName, channelId, ContentAttribute.Title);

                    if (contentTitles.Contains(title))
                    {
                        return false;
                    }

                    contentTitles.Add(title);
                    contentTitleHashtable[channelId] = contentTitles;
                }

                var contentInfo = new BackgroundContentInfo
                {
                    PublishmentSystemId = publishmentSystemInfo.PublishmentSystemId,
                    NodeId = channelId,
                    AddUserName = administratorName,
                    AddDate = DateTime.Now
                };
                contentInfo.LastEditUserName = contentInfo.AddUserName;
                contentInfo.LastEditDate = contentInfo.AddDate;
                contentInfo.IsChecked = isChecked;
                contentInfo.CheckedLevel = 0;

                contentInfo.Title = title;

                foreach (string attributeName in contentAttributes)
                {
                    if (!StringUtils.EqualsIgnoreCase(attributeName, ContentAttribute.Title) && !StringUtils.EqualsIgnoreCase(attributeName, BackgroundContentAttribute.Content))
                    {
                        var normalStart = StringUtils.ValueFromUrl(contentAttributesXml[attributeName + "_ContentStart"]);
                        var normalEnd = StringUtils.ValueFromUrl(contentAttributesXml[attributeName + "_ContentEnd"]);

                        //采集为空时的默认值
                        var normalDefault = StringUtils.ValueFromUrl(contentAttributesXml[attributeName + "_ContentDefault"]);

                        var regex = GetRegexAttributeName(attributeName, normalStart, normalEnd);
                        var value = RegexUtils.GetContent(attributeName, regex, contentHtml);

                        //采集为空时的默认值
                        if (string.IsNullOrEmpty(value))
                        {
                            value = normalDefault;
                        }

                        if (BackgroundContentAttribute.SystemAttributes.Contains(attributeName))
                        {
                            if (StringUtils.EqualsIgnoreCase(ContentAttribute.AddDate, attributeName))
                            {
                                contentInfo.AddDate = TranslateUtils.ToDateTime(value, DateTime.Now);
                            }
                            else if (StringUtils.EqualsIgnoreCase(ContentAttribute.IsColor, attributeName))
                            {
                                contentInfo.IsColor = TranslateUtils.ToBool(value, false);
                            }
                            else if (StringUtils.EqualsIgnoreCase(ContentAttribute.IsHot, attributeName))
                            {
                                contentInfo.IsHot = TranslateUtils.ToBool(value, false);
                            }
                            else if (StringUtils.EqualsIgnoreCase(ContentAttribute.IsRecommend, attributeName))
                            {
                                contentInfo.IsRecommend = TranslateUtils.ToBool(value, false);
                            }
                            else if (StringUtils.EqualsIgnoreCase(ContentAttribute.IsTop, attributeName))
                            {
                                contentInfo.IsTop = TranslateUtils.ToBool(value, false);
                            }
                            else if (StringUtils.EqualsIgnoreCase(BackgroundContentAttribute.ImageUrl, attributeName))
                            {
                                if (!string.IsNullOrEmpty(value))
                                {
                                    var attachmentUrl = PageUtils.GetUrlByBaseUrl(value, url);
                                    var fileName = PathUtility.GetUploadFileName(publishmentSystemInfo, attachmentUrl);
                                    var fileExtension = PageUtils.GetExtensionFromUrl(attachmentUrl);
                                    var directoryPath = PathUtility.GetUploadDirectoryPath(publishmentSystemInfo, fileExtension);
                                    DirectoryUtils.CreateDirectoryIfNotExists(directoryPath);
                                    var filePath = PathUtils.Combine(directoryPath, fileName);
                                    try
                                    {
                                        WebClientUtils.SaveRemoteFileToLocal(attachmentUrl, filePath);
                                        contentInfo.ImageUrl = PageUtility.GetPublishmentSystemVirtualUrlByPhysicalPath(publishmentSystemInfo, filePath);
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                }
                            }
                            else if (StringUtils.EqualsIgnoreCase(BackgroundContentAttribute.VideoUrl, attributeName))
                            {
                                if (!string.IsNullOrEmpty(value))
                                {
                                    var attachmentUrl = PageUtils.GetUrlByBaseUrl(value, url);
                                    var fileExtName = PageUtils.GetExtensionFromUrl(attachmentUrl);
                                    var fileName = PathUtility.GetUploadFileName(publishmentSystemInfo, attachmentUrl);
                                    var directoryPath = PathUtility.GetUploadDirectoryPath(publishmentSystemInfo, fileExtName);
                                    DirectoryUtils.CreateDirectoryIfNotExists(directoryPath);
                                    var filePath = PathUtils.Combine(directoryPath, fileName);
                                    try
                                    {
                                        WebClientUtils.SaveRemoteFileToLocal(attachmentUrl, filePath);
                                        contentInfo.VideoUrl = PageUtility.GetPublishmentSystemVirtualUrlByPhysicalPath(publishmentSystemInfo, filePath);
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                }
                            }
                            else if (StringUtils.EqualsIgnoreCase(BackgroundContentAttribute.FileUrl, attributeName))
                            {
                                if (!string.IsNullOrEmpty(value))
                                {
                                    var attachmentUrl = PageUtils.GetUrlByBaseUrl(value, url);
                                    var fileExtName = PageUtils.GetExtensionFromUrl(attachmentUrl);
                                    var fileName = PathUtility.GetUploadFileName(publishmentSystemInfo, attachmentUrl);
                                    var directoryPath = PathUtility.GetUploadDirectoryPath(publishmentSystemInfo, fileExtName);
                                    DirectoryUtils.CreateDirectoryIfNotExists(directoryPath);
                                    var filePath = PathUtils.Combine(directoryPath, fileName);
                                    try
                                    {
                                        WebClientUtils.SaveRemoteFileToLocal(attachmentUrl, filePath);
                                        contentInfo.FileUrl = PageUtility.GetPublishmentSystemVirtualUrlByPhysicalPath(publishmentSystemInfo, filePath);
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                }
                            }
                            else if (StringUtils.EqualsIgnoreCase(ContentAttribute.Hits, attributeName))
                            {
                                contentInfo.Hits = TranslateUtils.ToInt(value);
                            }
                            else
                            {
                                contentInfo.SetExtendedAttribute(attributeName, value);
                            }
                        }
                        else
                        {
                            var styleInfo = TableStyleManager.GetTableStyleInfo(ETableStyle.BackgroundContent, publishmentSystemInfo.AuxiliaryTableForContent, attributeName, null);
                            value = InputParserUtility.GetContentByTableStyle(value, publishmentSystemInfo, ETableStyle.BackgroundContent, styleInfo);

                            if (InputTypeUtils.EqualsAny(styleInfo.InputType, InputType.Image, InputType.Video, InputType.File))
                            {
                                if (!string.IsNullOrEmpty(value))
                                {
                                    var attachmentUrl = PageUtils.GetUrlByBaseUrl(value, url);
                                    var fileExtension = PathUtils.GetExtension(attachmentUrl);
                                    var fileName = PathUtility.GetUploadFileName(publishmentSystemInfo, attachmentUrl);
                                    var directoryPath = PathUtility.GetUploadDirectoryPath(publishmentSystemInfo, fileExtension);
                                    DirectoryUtils.CreateDirectoryIfNotExists(directoryPath);
                                    var filePath = PathUtils.Combine(directoryPath, fileName);
                                    try
                                    {
                                        WebClientUtils.SaveRemoteFileToLocal(attachmentUrl, filePath);
                                        value = PageUtility.GetPublishmentSystemVirtualUrlByPhysicalPath(publishmentSystemInfo, filePath);
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                }
                            }

                            contentInfo.SetExtendedAttribute(attributeName, value);
                        }
                    }
                }

                if (string.IsNullOrEmpty(contentInfo.ImageUrl))
                {
                    var firstImageUrl = string.Empty;
                    if (isSaveImage)
                    {
                        var originalImageSrcs = RegexUtils.GetOriginalImageSrcs(content);
                        var imageSrcs = RegexUtils.GetImageSrcs(url, content);
                        if (originalImageSrcs.Count == imageSrcs.Count)
                        {
                            for (var i = 0; i < originalImageSrcs.Count; i++)
                            {
                                var originalImageSrc = originalImageSrcs[i];
                                var imageSrc = imageSrcs[i];
                                var fileName = PathUtility.GetUploadFileName(publishmentSystemInfo, imageSrc);
                                var fileExtName = PathUtils.GetExtension(originalImageSrc);
                                var directoryPath = PathUtility.GetUploadDirectoryPath(publishmentSystemInfo, contentInfo.AddDate, fileExtName);
                                DirectoryUtils.CreateDirectoryIfNotExists(directoryPath);
                                var filePath = PathUtils.Combine(directoryPath, fileName);
                                try
                                {
                                    WebClientUtils.SaveRemoteFileToLocal(imageSrc, filePath);
                                    var fileUrl = PageUtility.GetPublishmentSystemVirtualUrlByPhysicalPath(publishmentSystemInfo, filePath);
                                    content = content.Replace(originalImageSrc, fileUrl);
                                    if (firstImageUrl == string.Empty)
                                    {
                                        firstImageUrl = fileUrl;
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                    }
                    else if (isSetFirstImageAsImageUrl)
                    {
                        var imageSrcs = RegexUtils.GetImageSrcs(url, content);
                        if (imageSrcs.Count > 0)
                        {
                            firstImageUrl = imageSrcs[0];
                        }
                    }

                    if (isSetFirstImageAsImageUrl)
                    {
                        contentInfo.ImageUrl = firstImageUrl;
                    }
                }
                //contentInfo.Content = StringUtility.TextEditorContentEncode(content, publishmentSystemInfo, false);
                contentInfo.Content = content;

                contentInfo.SourceId = SourceManager.CaiJi;

                var theContentId = DataProvider.ContentDao.Insert(tableName, publishmentSystemInfo, contentInfo);
                nodeIdAndContentIdList.Add(new[] { contentInfo.NodeId, theContentId });

                if (isCache)
                {
                    CacheUtils.Insert(cacheMessageKey, "采集内容：" + title);//存储消息
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        private static string GetPageContent(string previousPageContent, ECharset charset, string url, string cookieString, string regexContentExclude, string contentHtmlClearCollection, string contentHtmlClearTagCollection, string regexContent, string regexContent2, string regexContent3, string regexNextPage)
        {
            var content = previousPageContent;
            var contentHtml = WebClientUtils.GetRemoteFileSource(url, charset, cookieString);
            var nextPageContent = RegexUtils.GetContent("content", regexContent, contentHtml);
            if (string.IsNullOrEmpty(nextPageContent) && !string.IsNullOrEmpty(regexContent2))
            {
                nextPageContent = RegexUtils.GetContent("content", regexContent2, contentHtml);
            }
            if (string.IsNullOrEmpty(nextPageContent) && !string.IsNullOrEmpty(regexContent3))
            {
                nextPageContent = RegexUtils.GetContent("content", regexContent3, contentHtml);
            }

            if (!string.IsNullOrEmpty(nextPageContent))
            {
                if (string.IsNullOrEmpty(content))
                {
                    content += nextPageContent;
                }
                else
                {
                    content += ContentUtility.PagePlaceHolder + nextPageContent;
                }
            }

            if (!string.IsNullOrEmpty(regexContentExclude))
            {
                content = RegexUtils.Replace(regexContentExclude, content, string.Empty);
            }
            if (!string.IsNullOrEmpty(contentHtmlClearCollection))
            {
                var htmlClearArrayList = TranslateUtils.StringCollectionToStringList(contentHtmlClearCollection);
                foreach (string htmlClear in htmlClearArrayList)
                {
                    string clearRegex = $@"<{htmlClear}[^>]*>.*?<\/{htmlClear}>";
                    content = RegexUtils.Replace(clearRegex, content, string.Empty);
                }
            }
            if (!string.IsNullOrEmpty(contentHtmlClearTagCollection))
            {
                var htmlClearTagArrayList = TranslateUtils.StringCollectionToStringList(contentHtmlClearTagCollection);
                foreach (string htmlClearTag in htmlClearTagArrayList)
                {
                    string clearRegex = $@"<{htmlClearTag}[^>]*>";
                    content = RegexUtils.Replace(clearRegex, content, string.Empty);
                    clearRegex = $@"<\/{htmlClearTag}>";
                    content = RegexUtils.Replace(clearRegex, content, string.Empty);
                }
            }

            var contentNextPageUrl = RegexUtils.GetUrl(regexNextPage, contentHtml, url);
            if (!string.IsNullOrEmpty(contentNextPageUrl))
            {
                if (StringUtils.EqualsIgnoreCase(url, contentNextPageUrl))
                {
                    contentNextPageUrl = string.Empty;
                }
            }
            if (!string.IsNullOrEmpty(contentNextPageUrl))
            {
                return GetPageContent(content, charset, contentNextPageUrl, cookieString, regexContentExclude, contentHtmlClearCollection, contentHtmlClearTagCollection, regexContent, regexContent2, regexContent3, regexNextPage);
            }
            else
            {
                return content;
            }
        }


        #region 外部调用

        public const string CacheTotalCount = "_TotalCount";
        public const string CacheCurrentCount = "_CurrentCount";
        public const string CacheMessage = "_Message";

        public static void GatherWeb(int publishmentSystemId, string gatherRuleName, StringBuilder resultBuilder, StringBuilder errorBuilder, bool isCache, string userKeyPrefix, string administratorName)
        {
            var cacheTotalCountKey = userKeyPrefix + CacheTotalCount;
            var cacheCurrentCountKey = userKeyPrefix + CacheCurrentCount;
            var cacheMessageKey = userKeyPrefix + CacheMessage;

            if (isCache)
            {
                CacheUtils.Insert(cacheTotalCountKey, "0");//存储需要的页面总数
                CacheUtils.Insert(cacheCurrentCountKey, "0");//存储当前的页面总数
                CacheUtils.Insert(cacheMessageKey, "开始获取链接...");//存储消息
            }

            var currentCount = 0;

            var gatherRuleInfo = DataProvider.GatherRuleDao.GetGatherRuleInfo(gatherRuleName, publishmentSystemId);

            if (!DataProvider.NodeDao.IsExists(gatherRuleInfo.NodeId))
            {
                gatherRuleInfo.NodeId = publishmentSystemId;
            }

            var regexUrlInclude = GetRegexString(gatherRuleInfo.UrlInclude);
            var regexTitleInclude = GetRegexString(gatherRuleInfo.TitleInclude);
            var regexContentExclude = GetRegexString(gatherRuleInfo.ContentExclude);
            var regexListArea = GetRegexArea(gatherRuleInfo.ListAreaStart, gatherRuleInfo.ListAreaEnd);
            var regexChannel = GetRegexChannel(gatherRuleInfo.ContentChannelStart, gatherRuleInfo.ContentChannelEnd);
            var regexContent = GetRegexContent(gatherRuleInfo.ContentContentStart, gatherRuleInfo.ContentContentEnd);
            var regexContent2 = string.Empty;
            if (!string.IsNullOrEmpty(gatherRuleInfo.Additional.ContentContentStart2) && !string.IsNullOrEmpty(gatherRuleInfo.Additional.ContentContentEnd2))
            {
                regexContent2 = GetRegexContent(gatherRuleInfo.Additional.ContentContentStart2, gatherRuleInfo.Additional.ContentContentEnd2);
            }
            var regexContent3 = string.Empty;
            if (!string.IsNullOrEmpty(gatherRuleInfo.Additional.ContentContentStart3) && !string.IsNullOrEmpty(gatherRuleInfo.Additional.ContentContentEnd3))
            {
                regexContent3 = GetRegexContent(gatherRuleInfo.Additional.ContentContentStart3, gatherRuleInfo.Additional.ContentContentEnd3);
            }
            var regexNextPage = GetRegexUrl(gatherRuleInfo.ContentNextPageStart, gatherRuleInfo.ContentNextPageEnd);
            var regexTitle = GetRegexTitle(gatherRuleInfo.ContentTitleStart, gatherRuleInfo.ContentTitleEnd);
            var contentAttributes = TranslateUtils.StringCollectionToStringList(gatherRuleInfo.ContentAttributes);
            var contentAttributesXml = TranslateUtils.ToNameValueCollection(gatherRuleInfo.ContentAttributesXml);

            var contentUrls = GetContentUrlArrayList(gatherRuleInfo, regexListArea, regexUrlInclude, isCache, cacheMessageKey, errorBuilder);

            var totalCount = gatherRuleInfo.Additional.GatherNum > 0 ? gatherRuleInfo.Additional.GatherNum : contentUrls.Count;

            if (isCache)
            {
                CacheUtils.Insert(cacheTotalCountKey, totalCount.ToString());//存储需要的页面总数
                CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                CacheUtils.Insert(cacheMessageKey, "开始采集内容...");//存储消息
            }

            var contentTitleHashtable = new Hashtable();

            var publishmentSystemInfo = PublishmentSystemManager.GetPublishmentSystemInfo(publishmentSystemId);
            var nodeInfo = NodeManager.GetNodeInfo(publishmentSystemId, gatherRuleInfo.NodeId);
            var nodeIdAndContentIdList = new List<int[]>();

            foreach (string contentUrl in contentUrls)
            {
                if (GatherOneByUrl(administratorName, publishmentSystemInfo, nodeInfo, gatherRuleInfo.Additional.IsSaveImage, gatherRuleInfo.Additional.IsSetFirstImageAsImageUrl, gatherRuleInfo.Additional.IsEmptyContentAllowed, gatherRuleInfo.Additional.IsSameTitleAllowed, gatherRuleInfo.Additional.IsChecked, gatherRuleInfo.Charset, contentUrl, gatherRuleInfo.CookieString, regexTitleInclude, regexContentExclude, gatherRuleInfo.ContentHtmlClearCollection, gatherRuleInfo.ContentHtmlClearTagCollection, gatherRuleInfo.Additional.ContentReplaceFrom, gatherRuleInfo.Additional.ContentReplaceTo, regexTitle, regexContent, regexContent2, regexContent3, regexNextPage, regexChannel, contentAttributes, contentAttributesXml, contentTitleHashtable, nodeIdAndContentIdList, isCache, cacheMessageKey))
                {
                    currentCount++;
                    if (isCache)
                    {
                        CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                    }
                }
                if (currentCount == totalCount) break;
            }

            if (gatherRuleInfo.Additional.IsChecked)
            {
                foreach (var nodeIdAndContentId in nodeIdAndContentIdList)
                {
                    try
                    {
                        CreateManager.CreateContentAndTrigger(publishmentSystemId, nodeIdAndContentId[0], nodeIdAndContentId[1]);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            DataProvider.GatherRuleDao.UpdateLastGatherDate(gatherRuleName, publishmentSystemId);

            resultBuilder.Append(
                $"任务完成，<strong> {nodeInfo.NodeName} </strong>栏目共采集内容<strong> {currentCount} </strong>篇。请手动生成页面。<br/>");

            if (isCache)
            {
                CacheUtils.Remove(cacheTotalCountKey);//取消存储需要的页面总数
                CacheUtils.Remove(cacheCurrentCountKey);//取消存储当前的页面总数
                CacheUtils.Remove(cacheMessageKey);//取消存储消息
            }
        }

        public static void GatherDatabase(int publishmentSystemId, string gatherRuleName, StringBuilder resultBuilder, StringBuilder errorBuilder, bool isCache, string userKeyPrefix, string administratorName)
        {
            var cacheTotalCountKey = userKeyPrefix + CacheTotalCount;
            var cacheCurrentCountKey = userKeyPrefix + CacheCurrentCount;
            var cacheMessageKey = userKeyPrefix + CacheMessage;

            if (isCache)
            {
                CacheUtils.Insert(cacheTotalCountKey, "0");//存储需要的页面总数
                CacheUtils.Insert(cacheCurrentCountKey, "0");//存储当前的页面总数
                CacheUtils.Insert(cacheMessageKey, "开始连接数据库...");//存储消息
            }

            var publishmentSystemInfo = PublishmentSystemManager.GetPublishmentSystemInfo(publishmentSystemId);

            try
            {
                int totalCount;
                var currentCount = 0;

                var gatherDatabaseRuleInfo = DataProvider.GatherDatabaseRuleDao.GetGatherDatabaseRuleInfo(gatherRuleName, publishmentSystemId);
                var tableMatchInfo = BaiRongDataProvider.TableMatchDao.GetTableMatchInfo(gatherDatabaseRuleInfo.TableMatchId);

                if (!DataProvider.NodeDao.IsExists(gatherDatabaseRuleInfo.NodeId))
                {
                    gatherDatabaseRuleInfo.NodeId = publishmentSystemId;
                }

                if (gatherDatabaseRuleInfo.GatherNum > 0)
                {
                    totalCount = gatherDatabaseRuleInfo.GatherNum;
                }
                else
                {
                    totalCount = BaiRongDataProvider.DatabaseDao.GetIntResult(gatherDatabaseRuleInfo.ConnectionString,
                        $"SELECT COUNT(*) FROM {gatherDatabaseRuleInfo.RelatedTableName}");
                }

                if (isCache)
                {
                    CacheUtils.Insert(cacheTotalCountKey, totalCount.ToString());//存储需要的页面总数
                    CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                    CacheUtils.Insert(cacheMessageKey, "开始采集内容...");//存储消息
                }

                var nodeIdAndContentIdList = new List<int[]>();

                var whereString = string.Empty;
                if (!string.IsNullOrEmpty(gatherDatabaseRuleInfo.WhereString))
                {
                    if (gatherDatabaseRuleInfo.WhereString.Trim().ToLower().StartsWith("where "))
                    {
                        whereString = gatherDatabaseRuleInfo.WhereString;
                    }
                    else
                    {
                        whereString = "WHERE " + gatherDatabaseRuleInfo.WhereString;
                    }
                }
                string sqlString = $"SELECT * FROM {gatherDatabaseRuleInfo.RelatedTableName} {whereString}";

                var tableName = NodeManager.GetTableName(publishmentSystemInfo, gatherDatabaseRuleInfo.NodeId);

                var titleList = BaiRongDataProvider.ContentDao.GetValueList(tableName, gatherDatabaseRuleInfo.NodeId, ContentAttribute.Title);

                using (var rdr = BaiRongDataProvider.DatabaseDao.GetDataReader(gatherDatabaseRuleInfo.ConnectionString, sqlString))
                {
                    while (rdr.Read())
                    {
                        try
                        {
                            var collection = new NameValueCollection();
                            BaiRongDataProvider.DatabaseDao.ReadResultsToNameValueCollection(rdr, collection);
                            var contentInfo = Converter.ToBackgroundContentInfo(collection, tableMatchInfo.ColumnsMap);
                            if (!string.IsNullOrEmpty(contentInfo?.Title) && !titleList.Contains(contentInfo.Title))
                            {
                                contentInfo.PublishmentSystemId = publishmentSystemId;
                                contentInfo.NodeId = gatherDatabaseRuleInfo.NodeId;
                                if (contentInfo.AddDate == DateTime.MinValue || contentInfo.AddDate == DateUtils.SqlMinValue)
                                {
                                    contentInfo.AddDate = DateTime.Now;
                                }
                                contentInfo.AddUserName = administratorName;
                                contentInfo.LastEditDate = contentInfo.AddDate;
                                contentInfo.LastEditUserName = contentInfo.AddUserName;
                                contentInfo.IsChecked = gatherDatabaseRuleInfo.IsChecked;
                                contentInfo.CheckedLevel = 0;

                                contentInfo.SourceId = SourceManager.CaiJi;

                                var theContentId = DataProvider.ContentDao.Insert(tableName, publishmentSystemInfo, contentInfo);
                                nodeIdAndContentIdList.Add(new[] { contentInfo.NodeId, theContentId });

                                currentCount++;
                                if (isCache)
                                {
                                    CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                                }
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                        if (currentCount == totalCount) break;
                    }
                    rdr.Close();
                }

                if (gatherDatabaseRuleInfo.IsChecked)
                {
                    foreach (var nodeIdAndContentId in nodeIdAndContentIdList)
                    {
                        CreateManager.CreateContentAndTrigger(publishmentSystemId, nodeIdAndContentId[0], nodeIdAndContentId[1]);
                    }
                }

                DataProvider.GatherRuleDao.UpdateLastGatherDate(gatherRuleName, publishmentSystemId);
                var nodeName = NodeManager.GetNodeName(gatherDatabaseRuleInfo.PublishmentSystemId, gatherDatabaseRuleInfo.NodeId);
                resultBuilder.Append(
                    $"任务完成，<strong> {nodeName} </strong>栏目共采集内容<strong> {currentCount} </strong>篇。请手动生成页面。<br />");
            }
            catch (Exception ex)
            {
                errorBuilder.Append(ex.Message);
                LogUtils.AddErrorLog(ex);
            }

            if (isCache)
            {
                CacheUtils.Remove(cacheTotalCountKey);//取消存储需要的页面总数
                CacheUtils.Remove(cacheCurrentCountKey);//取消存储当前的页面总数
                CacheUtils.Remove(cacheMessageKey);//取消存储消息
            }
        }


        public static void GatherFile(int publishmentSystemId, string gatherRuleName, StringBuilder resultBuilder, StringBuilder errorBuilder, bool isCache, string userKeyPrefix, string administratorName)
        {
            var cacheTotalCountKey = userKeyPrefix + CacheTotalCount;
            var cacheCurrentCountKey = userKeyPrefix + CacheCurrentCount;
            var cacheMessageKey = userKeyPrefix + CacheMessage;

            if (isCache)
            {
                CacheUtils.Insert(cacheTotalCountKey, "0");//存储需要的页面总数
                CacheUtils.Insert(cacheCurrentCountKey, "0");//存储当前的页面总数
                CacheUtils.Insert(cacheMessageKey, "开始获取内容...");//存储消息
            }

            try
            {
                int totalCount;
                var currentCount = 0;

                var gatherFileRuleInfo = DataProvider.GatherFileRuleDao.GetGatherFileRuleInfo(gatherRuleName, publishmentSystemId);
                var publishmentSystemInfo = PublishmentSystemManager.GetPublishmentSystemInfo(publishmentSystemId);

                if (gatherFileRuleInfo.IsToFile)
                {
                    var gatherFilePath = PathUtility.MapPath(publishmentSystemInfo, gatherFileRuleInfo.FilePath);
                    var publishmentSystemPath = PathUtility.GetPublishmentSystemPath(publishmentSystemInfo);
                    var level = StringUtils.GetCount(PathUtils.SeparatorChar.ToString(), PathUtils.GetPathDifference(publishmentSystemPath, gatherFilePath));
                    var fileContent = WebClientUtils.GetRemoteFileSource(gatherFileRuleInfo.GatherUrl, gatherFileRuleInfo.Charset);
                    if (gatherFileRuleInfo.IsRemoveScripts)
                    {
                        fileContent = RegexUtils.RemoveScripts(fileContent);
                    }
                    if (gatherFileRuleInfo.IsSaveRelatedFiles)
                    {
                        var styleDirectoryPath = PathUtility.MapPath(publishmentSystemInfo, gatherFileRuleInfo.StyleDirectoryPath);
                        var scriptDirectoryPath = PathUtility.MapPath(publishmentSystemInfo, gatherFileRuleInfo.ScriptDirectoryPath);
                        var imageDirectoryPath = PathUtility.MapPath(publishmentSystemInfo, gatherFileRuleInfo.ImageDirectoryPath);
                        DirectoryUtils.CreateDirectoryIfNotExists(styleDirectoryPath);
                        DirectoryUtils.CreateDirectoryIfNotExists(scriptDirectoryPath);
                        DirectoryUtils.CreateDirectoryIfNotExists(imageDirectoryPath);

                        var originalCssHrefs = RegexUtils.GetOriginalCssHrefs(fileContent);
                        var originalScriptSrcs = RegexUtils.GetOriginalScriptSrcs(fileContent);
                        var originalImageSrcs = RegexUtils.GetOriginalImageSrcs(fileContent);
                        var originalFlashSrcs = RegexUtils.GetOriginalFlashSrcs(fileContent);
                        var originalStyleImageUrls = RegexUtils.GetOriginalStyleImageUrls(fileContent);
                        var originalBackgroundImageSrcs = RegexUtils.GetOriginalBackgroundImageSrcs(fileContent);

                        totalCount = originalCssHrefs.Count + originalScriptSrcs.Count + originalImageSrcs.Count + originalFlashSrcs.Count + originalStyleImageUrls.Count + originalBackgroundImageSrcs.Count;
                        if (isCache)
                        {
                            CacheUtils.Insert(cacheTotalCountKey, totalCount.ToString());//存储需要的页面总数
                            CacheUtils.Insert(cacheMessageKey, "保存文件：" + gatherFileRuleInfo.FilePath);//存储消息
                        }

                        var cssHrefs = RegexUtils.GetCssHrefs(gatherFileRuleInfo.GatherUrl, fileContent);
                        for (var i = 0; i < originalCssHrefs.Count; i++)
                        {
                            try
                            {
                                var originalLinkHref = originalCssHrefs[i];
                                var cssHref = cssHrefs[i];

                                var fileUrl = GatherCss(publishmentSystemInfo, publishmentSystemPath, imageDirectoryPath, styleDirectoryPath, level, gatherFileRuleInfo.Charset, cssHref);
                                fileContent = fileContent.Replace(originalLinkHref, fileUrl);
                                currentCount++;
                                if (isCache)
                                {
                                    CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                                    CacheUtils.Insert(cacheMessageKey, "保存Css样式文件：" + PathUtils.GetFileName(cssHref));//存储消息
                                }

                                //string originalLinkHref = (string)originalCssHrefs[i];
                                //string cssHref = (string)cssHrefs[i];

                                //string fileName = PageUtils.UrlDecode(PathUtils.GetFileName(cssHref));
                                //string filePath = PathUtils.Combine(styleDirectoryPath, fileName);

                                //string styleContent = WebClientUtils.GetRemoteFileSource(cssHref, gatherFileRuleInfo.Charset);
                                //ArrayList originalStyleImageUrls_i = RegexUtils.GetOriginalStyleImageUrls(styleContent);
                                //ArrayList styleImageUrls_i = RegexUtils.GetStyleImageUrls(PageUtils.GetUrlWithoutFileName(cssHref), styleContent);
                                //for (int j = 0; j < originalStyleImageUrls_i.Count; j++)
                                //{
                                //    string originalStyleImageUrl = (string)originalStyleImageUrls_i[j];
                                //    string styleImageUrl = (string)styleImageUrls_i[j];
                                //    string fileName_j = PageUtils.UrlDecode(PathUtils.GetFileName(styleImageUrl));
                                //    string filePath_j = PathUtils.Combine(imageDirectoryPath, fileName_j);
                                //    try
                                //    {
                                //        WebClientUtils.SaveRemoteFileToLocal(styleImageUrl, filePath_j);
                                //        string fileUrl_j = PageUtility.GetPublishmentSystemUrlOfRelatedByPhysicalPath(publishmentSystemInfo, filePath_j, level);
                                //        styleContent = styleContent.Replace(originalStyleImageUrl, fileUrl_j);
                                //    }
                                //    catch { }
                                //}

                                //FileUtils.WriteText(filePath, gatherFileRuleInfo.Charset, styleContent);

                                //string fileUrl = PageUtility.GetPublishmentSystemUrlOfRelatedByPhysicalPath(publishmentSystemInfo, filePath, level);
                                //fileContent = fileContent.Replace(originalLinkHref, fileUrl);
                                //currentCount++;
                                //if (isCache)
                                //{
                                //    CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                                //    CacheUtils.Insert(cacheMessageKey, "保存Css样式文件：" + fileName);//存储消息
                                //}
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        var scriptSrcs = RegexUtils.GetScriptSrcs(gatherFileRuleInfo.GatherUrl, fileContent);
                        for (var i = 0; i < originalScriptSrcs.Count; i++)
                        {
                            try
                            {
                                var originalScriptSrc = originalScriptSrcs[i];
                                var scriptSrc = scriptSrcs[i];
                                var fileName = PageUtils.UrlDecode(PathUtils.GetFileName(scriptSrc));
                                var filePath = PathUtils.Combine(scriptDirectoryPath, fileName);

                                WebClientUtils.SaveRemoteFileToLocal(scriptSrc, filePath);
                                var fileUrl = PageUtility.GetPublishmentSystemUrlOfRelatedByPhysicalPath(publishmentSystemInfo, filePath, level);
                                fileContent = fileContent.Replace(originalScriptSrc, fileUrl);
                                currentCount++;
                                if (isCache)
                                {
                                    CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                                    CacheUtils.Insert(cacheMessageKey, "保存Js脚本文件：" + fileName);//存储消息
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        var imageSrcs = RegexUtils.GetImageSrcs(gatherFileRuleInfo.GatherUrl, fileContent);
                        for (var i = 0; i < originalImageSrcs.Count; i++)
                        {
                            try
                            {
                                var originalImageSrc = originalImageSrcs[i];
                                var imageSrc = imageSrcs[i];
                                var fileName = PageUtils.UrlDecode(PathUtils.GetFileName(imageSrc));
                                var filePath = PathUtils.Combine(imageDirectoryPath, fileName);

                                WebClientUtils.SaveRemoteFileToLocal(imageSrc, filePath);
                                var fileUrl = PageUtility.GetPublishmentSystemUrlOfRelatedByPhysicalPath(publishmentSystemInfo, filePath, level);
                                fileContent = fileContent.Replace(originalImageSrc, fileUrl);
                                currentCount++;
                                if (isCache)
                                {
                                    CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                                    CacheUtils.Insert(cacheMessageKey, "保存图片文件：" + fileName);//存储消息
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        var flashSrcs = RegexUtils.GetFlashSrcs(gatherFileRuleInfo.GatherUrl, fileContent);
                        for (var i = 0; i < originalFlashSrcs.Count; i++)
                        {
                            try
                            {
                                var originalFlashSrc = originalFlashSrcs[i];
                                var flashSrc = flashSrcs[i];
                                var fileName = PageUtils.UrlDecode(PathUtils.GetFileName(flashSrc));
                                var filePath = PathUtils.Combine(imageDirectoryPath, fileName);

                                WebClientUtils.SaveRemoteFileToLocal(flashSrc, filePath);
                                var fileUrl = PageUtility.GetPublishmentSystemUrlOfRelatedByPhysicalPath(publishmentSystemInfo, filePath, level);
                                fileContent = fileContent.Replace(originalFlashSrc, fileUrl);
                                currentCount++;
                                if (isCache)
                                {
                                    CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                                    CacheUtils.Insert(cacheMessageKey, "保存Flash文件：" + fileName);//存储消息
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        var styleImageUrls = RegexUtils.GetStyleImageUrls(gatherFileRuleInfo.GatherUrl, fileContent);
                        for (var j = 0; j < originalStyleImageUrls.Count; j++)
                        {
                            try
                            {
                                var originalStyleImageUrl = originalStyleImageUrls[j];
                                var styleImageUrl = styleImageUrls[j];
                                var fileNameJ = PageUtils.UrlDecode(PathUtils.GetFileName(styleImageUrl));
                                var filePathJ = PathUtils.Combine(imageDirectoryPath, fileNameJ);

                                WebClientUtils.SaveRemoteFileToLocal(styleImageUrl, filePathJ);
                                var fileUrlJ = PageUtility.GetPublishmentSystemUrlOfRelatedByPhysicalPath(publishmentSystemInfo, filePathJ, level);
                                fileContent = fileContent.Replace(originalStyleImageUrl, fileUrlJ);
                                currentCount++;
                                if (isCache)
                                {
                                    CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                                    CacheUtils.Insert(cacheMessageKey, "保存图片文件：" + fileNameJ);//存储消息
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        var backgroundImageSrcs = RegexUtils.GetBackgroundImageSrcs(gatherFileRuleInfo.GatherUrl, fileContent);
                        for (var j = 0; j < originalBackgroundImageSrcs.Count; j++)
                        {
                            try
                            {
                                var originalBackgroundImageSrc = originalBackgroundImageSrcs[j];
                                var backgroundImageSrc = backgroundImageSrcs[j];
                                var fileNameJ = PageUtils.UrlDecode(PathUtils.GetFileName(backgroundImageSrc));
                                var filePathJ = PathUtils.Combine(imageDirectoryPath, fileNameJ);

                                WebClientUtils.SaveRemoteFileToLocal(backgroundImageSrc, filePathJ);
                                var fileUrlJ = PageUtility.GetPublishmentSystemUrlOfRelatedByPhysicalPath(publishmentSystemInfo, filePathJ, level);
                                fileContent = fileContent.Replace(originalBackgroundImageSrc, fileUrlJ);
                                currentCount++;
                                if (isCache)
                                {
                                    CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                                    CacheUtils.Insert(cacheMessageKey, "保存图片文件：" + fileNameJ);//存储消息
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                    else
                    {
                        totalCount = 1;
                        if (isCache)
                        {
                            CacheUtils.Insert(cacheTotalCountKey, totalCount.ToString());//存储需要的页面总数
                        }
                    }

                    FileUtils.WriteText(gatherFilePath, gatherFileRuleInfo.Charset, fileContent);

                    resultBuilder.Append("任务完成。");
                }
                else
                {

                    if (!DataProvider.NodeDao.IsExists(gatherFileRuleInfo.NodeId))
                    {
                        gatherFileRuleInfo.NodeId = publishmentSystemId;
                    }

                    var regexContentExclude = GetRegexString(gatherFileRuleInfo.ContentExclude);
                    var regexContent = GetRegexContent(gatherFileRuleInfo.ContentContentStart, gatherFileRuleInfo.ContentContentEnd);
                    var regexTitle = GetRegexTitle(gatherFileRuleInfo.ContentTitleStart, gatherFileRuleInfo.ContentTitleEnd);
                    var contentAttributes = TranslateUtils.StringCollectionToStringList(gatherFileRuleInfo.ContentAttributes);
                    var contentAttributesXml = TranslateUtils.ToNameValueCollection(gatherFileRuleInfo.ContentAttributesXml);

                    totalCount = 1;

                    if (isCache)
                    {
                        CacheUtils.Insert(cacheTotalCountKey, totalCount.ToString());//存储需要的页面总数
                        CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                        CacheUtils.Insert(cacheMessageKey, "开始采集内容...");//存储消息
                    }

                    var contentTitleHashtable = new Hashtable();
                    var nodeInfo = NodeManager.GetNodeInfo(publishmentSystemId, gatherFileRuleInfo.NodeId);
                    var nodeIdAndContentIdList = new List<int[]>();

                    if (GatherOneByUrl(administratorName, publishmentSystemInfo, nodeInfo, gatherFileRuleInfo.IsSaveImage, false, true, true, gatherFileRuleInfo.IsChecked, gatherFileRuleInfo.Charset, gatherFileRuleInfo.GatherUrl, string.Empty, string.Empty, regexContentExclude, gatherFileRuleInfo.ContentHtmlClearCollection, gatherFileRuleInfo.ContentHtmlClearTagCollection, string.Empty, string.Empty, regexTitle, regexContent, string.Empty, string.Empty, string.Empty, string.Empty, contentAttributes, contentAttributesXml, contentTitleHashtable, nodeIdAndContentIdList, isCache, cacheMessageKey))
                    {
                        currentCount++;
                        if (isCache)
                        {
                            CacheUtils.Insert(cacheCurrentCountKey, currentCount.ToString());//存储当前的页面总数
                        }
                    }

                    if (gatherFileRuleInfo.IsChecked)
                    {
                        foreach (var nodeIdAndContentId in nodeIdAndContentIdList)
                        {
                            CreateManager.CreateContentAndTrigger(publishmentSystemId, nodeIdAndContentId[0], nodeIdAndContentId[1]);
                        }
                    }

                    resultBuilder.Append(
                        $"任务完成，<strong> {nodeInfo.NodeName} </strong>栏目共采集内容<strong> {currentCount} </strong>篇。请手动生成页面。<br />");
                }
                DataProvider.GatherFileRuleDao.UpdateLastGatherDate(gatherRuleName, publishmentSystemId);
            }
            catch (Exception ex)
            {
                errorBuilder.Append(ex.Message);
                LogUtils.AddErrorLog(ex);
            }

            if (isCache)
            {
                CacheUtils.Remove(cacheTotalCountKey);//取消存储需要的页面总数
                CacheUtils.Remove(cacheCurrentCountKey);//取消存储当前的页面总数
                CacheUtils.Remove(cacheMessageKey);//取消存储消息
            }
        }

        private static string GatherCss(PublishmentSystemInfo publishmentSystemInfo, string publishmentSystemPath, string imageDirectoryPath, string styleDirectoryPath, int topLevel, ECharset charset, string cssUrl)
        {
            var fileUrl = cssUrl;
            try
            {
                var fileName = PageUtils.UrlDecode(PathUtils.GetFileName(cssUrl));
                var filePath = PathUtils.Combine(styleDirectoryPath, fileName);

                var level = StringUtils.GetCount(PathUtils.SeparatorChar.ToString(), PathUtils.GetPathDifference(publishmentSystemPath, filePath));

                var styleContent = WebClientUtils.GetRemoteFileSource(cssUrl, charset);

                //开始采集CSS内部导入的CSS
                var originalCssHrefs = RegexUtils.GetOriginalCssHrefs(styleContent);
                var cssHrefs = RegexUtils.GetCssHrefs(cssUrl, styleContent);
                for (var i = 0; i < originalCssHrefs.Count; i++)
                {
                    try
                    {
                        var originalLinkHref = originalCssHrefs[i];
                        var cssHref = cssHrefs[i];

                        var fileUrlI = GatherCss(publishmentSystemInfo, publishmentSystemPath, imageDirectoryPath, styleDirectoryPath, level, charset, cssHref);
                        styleContent = styleContent.Replace(originalLinkHref, fileUrlI);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                //开始采集CSS内部图片
                var originalStyleImageUrls = RegexUtils.GetOriginalStyleImageUrls(styleContent);
                var styleImageUrls = RegexUtils.GetStyleImageUrls(cssUrl, styleContent);
                for (var j = 0; j < originalStyleImageUrls.Count; j++)
                {
                    var originalStyleImageUrl = originalStyleImageUrls[j];
                    var styleImageUrl = styleImageUrls[j];
                    var fileNameJ = PageUtils.UrlDecode(PathUtils.GetFileName(styleImageUrl));
                    var filePathJ = PathUtils.Combine(imageDirectoryPath, fileNameJ);
                    try
                    {
                        WebClientUtils.SaveRemoteFileToLocal(styleImageUrl, filePathJ);
                        var fileUrlJ = PageUtility.GetPublishmentSystemUrlOfRelatedByPhysicalPath(publishmentSystemInfo, filePathJ, level);
                        styleContent = styleContent.Replace(originalStyleImageUrl, fileUrlJ);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                FileUtils.WriteText(filePath, charset, styleContent);

                fileUrl = PageUtility.GetPublishmentSystemUrlOfRelatedByPhysicalPath(publishmentSystemInfo, filePath, topLevel);
            }
            catch
            {
                // ignored
            }
            return fileUrl;
        }

        #endregion

    }
}
