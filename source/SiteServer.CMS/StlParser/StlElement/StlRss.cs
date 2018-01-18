﻿using System;
using System.Collections.Generic;
using System.Text;
using BaiRong.Core;
using BaiRong.Core.IO;
using BaiRong.Core.Model.Enumerations;
using BaiRong.Core.Rss;
using SiteServer.CMS.Core;
using SiteServer.CMS.StlParser.Model;
using SiteServer.CMS.StlParser.Utility;

namespace SiteServer.CMS.StlParser.StlElement
{
    [Stl(Usage = "Rss订阅", Description = "通过 stl:rss 标签在模板中生成Rss阅读器能够浏览的Rss订阅")]
    public class StlRss
    {
        private StlRss() { }
        public const string ElementName = "stl:rss";

        public const string AttributeChannelIndex = "channelIndex";
        public const string AttributeChannelName = "channelName";
        public const string AttributeScope = "scope";
        public const string AttributeGroupChannel = "groupChannel";
        public const string AttributeGroupChannelNot = "groupChannelNot";
        public const string AttributeGroupContent = "groupContent";
        public const string AttributeGroupContentNot = "groupContentNot";
        public const string AttributeTags = "tags";
        public const string AttributeTitle = "title";
        public const string AttributeDescription = "description";
        public const string AttributeTotalNum = "totalNum";
        public const string AttributeStartNum = "startNum";
        public const string AttributeOrder = "order";
        public const string AttributeIsTop = "isTop";
        public const string AttributeIsRecommend = "isRecommend";
        public const string AttributeIsHot = "isHot";
        public const string AttributeIsColor = "isColor";

        public static SortedList<string, string> AttributeList => new SortedList<string, string>
        {
            {AttributeChannelIndex, "栏目索引"},
            {AttributeChannelName, "栏目名称"},
            {AttributeScope, "内容范围"},
            {AttributeGroupChannel, "指定显示的栏目组"},
            {AttributeGroupChannelNot, "指定不显示的栏目组"},
            {AttributeGroupContent, "指定显示的内容组"},
            {AttributeGroupContentNot, "指定不显示的内容组"},
            {AttributeTags, "指定标签"},
            {AttributeTitle, "Rss订阅标题"},
            {AttributeDescription, "Rss订阅摘要"},
            {AttributeTotalNum, "显示内容数目"},
            {AttributeStartNum, "从第几条信息开始显示"},
            {AttributeOrder, "排序"},
            {AttributeIsTop, "仅显示置顶内容"},
            {AttributeIsRecommend, "仅显示推荐内容"},
            {AttributeIsHot, "仅显示热点内容"},
            {AttributeIsColor, "仅显示醒目内容"}
        };

        public static string Parse(PageInfo pageInfo, ContextInfo contextInfo)
        {
            var title = string.Empty;
            var description = string.Empty;
            var scopeTypeString = string.Empty;
            var groupChannel = string.Empty;
            var groupChannelNot = string.Empty;
            var groupContent = string.Empty;
            var groupContentNot = string.Empty;
            var tags = string.Empty;
            var channelIndex = string.Empty;
            var channelName = string.Empty;
            var totalNum = 0;
            var startNum = 1;
            var orderByString = ETaxisTypeUtils.GetContentOrderByString(ETaxisType.OrderByTaxisDesc);
            var isTop = false;
            var isTopExists = false;
            var isRecommend = false;
            var isRecommendExists = false;
            var isHot = false;
            var isHotExists = false;
            var isColor = false;
            var isColorExists = false;

            foreach (var name in contextInfo.Attributes.Keys)
            {
                var value = contextInfo.Attributes[name];

                if (StringUtils.EqualsIgnoreCase(name, AttributeTitle))
                {
                    title = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeDescription))
                {
                    description = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeScope))
                {
                    scopeTypeString = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeChannelIndex))
                {
                    channelIndex = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeChannelName))
                {
                    channelName = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeGroupChannel))
                {
                    groupChannel = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeGroupChannelNot))
                {
                    groupChannelNot = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeGroupContent))
                {
                    groupContent = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeGroupContentNot))
                {
                    groupContentNot = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeTags))
                {
                    tags = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeTotalNum))
                {
                    totalNum = TranslateUtils.ToInt(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeStartNum))
                {
                    startNum = TranslateUtils.ToInt(value, 1);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeOrder))
                {
                    orderByString = StlDataUtility.GetOrderByString(pageInfo.PublishmentSystemId, value, ETableStyle.BackgroundContent, ETaxisType.OrderByTaxisDesc);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeIsTop))
                {
                    isTopExists = true;
                    isTop = TranslateUtils.ToBool(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeIsRecommend))
                {
                    isRecommendExists = true;
                    isRecommend = TranslateUtils.ToBool(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeIsHot))
                {
                    isHotExists = true;
                    isHot = TranslateUtils.ToBool(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeIsColor))
                {
                    isColorExists = true;
                    isColor = TranslateUtils.ToBool(value);
                }
            }

            return ParseImpl(pageInfo, contextInfo, title, description, scopeTypeString, groupChannel, groupChannelNot, groupContent, groupContentNot, tags, channelIndex, channelName, totalNum, startNum, orderByString, isTop, isTopExists, isRecommend, isRecommendExists, isHot, isHotExists, isColor, isColorExists);
        }

        private static string ParseImpl(PageInfo pageInfo, ContextInfo contextInfo, string title, string description, string scopeTypeString, string groupChannel, string groupChannelNot, string groupContent, string groupContentNot, string tags, string channelIndex, string channelName, int totalNum, int startNum, string orderByString, bool isTop, bool isTopExists, bool isRecommend, bool isRecommendExists, bool isHot, bool isHotExists, bool isColor, bool isColorExists)
        {
            var feed = new RssFeed
            {
                Encoding = ECharsetUtils.GetEncoding(pageInfo.TemplateInfo.Charset),
                Version = RssVersion.RSS20
            };

            var channel = new RssChannel
            {
                Title = title,
                Description = description
            };

            var scopeType = !string.IsNullOrEmpty(scopeTypeString) ? EScopeTypeUtils.GetEnumType(scopeTypeString) : EScopeType.All;

            var channelId = StlDataUtility.GetNodeIdByChannelIdOrChannelIndexOrChannelName(pageInfo.PublishmentSystemId, contextInfo.ChannelId, channelIndex, channelName);

            var nodeInfo = NodeManager.GetNodeInfo(pageInfo.PublishmentSystemId, channelId);
            if (string.IsNullOrEmpty(channel.Title))
            {
                channel.Title = nodeInfo.NodeName;
            }
            if (string.IsNullOrEmpty(channel.Description))
            {
                channel.Description = nodeInfo.Content;
                channel.Description = string.IsNullOrEmpty(channel.Description) ? nodeInfo.NodeName : StringUtils.MaxLengthText(channel.Description, 200);
            }
            channel.Link = new Uri(PageUtils.AddProtocolToUrl(PageUtility.GetChannelUrl(pageInfo.PublishmentSystemInfo, nodeInfo)));

            var dataSource = StlDataUtility.GetContentsDataSource(pageInfo.PublishmentSystemInfo, channelId, 0, groupContent, groupContentNot, tags, false, false, false, false, false, false, false, false, startNum, totalNum, orderByString, isTopExists, isTop, isRecommendExists, isRecommend, isHotExists, isHot, isColorExists, isColor, string.Empty, scopeType, groupChannel, groupChannelNot, null);

            if (dataSource != null)
            {
                //foreach (var dataItem in dataSource)
                //{
                //    var item = new RssItem();

                //    var contentInfo = new BackgroundContentInfo(dataItem);
                //    item.Title = StringUtils.Replace("&", contentInfo.Title, "&amp;");
                //    item.Description = contentInfo.Summary;
                //    if (string.IsNullOrEmpty(item.Description))
                //    {
                //        item.Description = StringUtils.StripTags(contentInfo.Content);
                //        item.Description = string.IsNullOrEmpty(item.Description) ? contentInfo.Title : StringUtils.MaxLengthText(item.Description, 200);
                //    }
                //    item.Description = StringUtils.Replace("&", item.Description, "&amp;");
                //    item.PubDate = contentInfo.AddDate;
                //    item.Link = new Uri(PageUtils.AddProtocolToUrl(PageUtility.GetContentUrl(pageInfo.PublishmentSystemInfo, contentInfo)));

                //    channel.Items.Add(item);
                //}
            }

            feed.Channels.Add(channel);

            var builder = new StringBuilder();
            var textWriter = new EncodedStringWriter(builder, ECharsetUtils.GetEncoding(pageInfo.TemplateInfo.Charset));
            feed.Write(textWriter);

            return builder.ToString();
        }
    }
}
