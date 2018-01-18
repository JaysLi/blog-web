﻿using System.Collections.Generic;
using System.Text;
using BaiRong.Core;
using SiteServer.CMS.Controllers.Stl;
using SiteServer.CMS.StlParser.Model;
using SiteServer.CMS.StlParser.Utility;

namespace SiteServer.CMS.StlParser.StlElement
{
    [Stl(Usage = "动态显示", Description = "通过 stl:dynamic 标签在模板中实现动态显示功能")]
    public class StlDynamic
    {
        private StlDynamic() { }
        public const string ElementName = "stl:dynamic";

        public const string AttributeContext = "context";
        public const string AttributeIsPageRefresh = "isPageRefresh";

        public static SortedList<string, string> AttributeList => new SortedList<string, string>
        {
            {AttributeContext, "所处上下文"},
            {AttributeIsPageRefresh, "翻页时是否刷新页面"}
        };

        internal static string Parse(PageInfo pageInfo, ContextInfo contextInfo)
        {
            // 如果是实体标签则返回空
            if (contextInfo.IsCurlyBrace)
            {
                return string.Empty;
            }

            var isPageRefresh = false;

            foreach (var name in contextInfo.Attributes.Keys)
            {
                var value = contextInfo.Attributes[name];

                if (StringUtils.EqualsIgnoreCase(name, AttributeContext))
                {
                    contextInfo.ContextType = EContextTypeUtils.GetEnumType(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeIsPageRefresh))
                {
                    isPageRefresh = TranslateUtils.ToBool(value);
                }
            }

            return ParseImpl(pageInfo, contextInfo, contextInfo.InnerXml, isPageRefresh);
        }

        private static string ParseImpl(PageInfo pageInfo, ContextInfo contextInfo, string templateContent, bool isPageRefresh)
        {
            pageInfo.AddPageScriptsIfNotExists(PageInfo.Components.StlClient);

            var ajaxDivId = StlParserUtility.GetAjaxDivId(pageInfo.UniqueId);

            var functionName = $"stlDynamic_{ajaxDivId}";

            if (string.IsNullOrEmpty(templateContent))
            {
                return string.Empty;
            }

            var apiUrl = ActionsDynamic.GetUrl(pageInfo.ApiUrl);
            var currentPageUrl = StlUtility.GetStlCurrentUrl(pageInfo.PublishmentSystemInfo, contextInfo.ChannelId, contextInfo.ContentId, contextInfo.ContentInfo, pageInfo.TemplateInfo.TemplateType, pageInfo.TemplateInfo.TemplateId);
            currentPageUrl = PageUtils.AddQuestionOrAndToUrl(currentPageUrl);
            var apiParameters = ActionsDynamic.GetParameters(pageInfo.PublishmentSystemId, contextInfo.ChannelId, contextInfo.ContentId, pageInfo.TemplateInfo.TemplateId, currentPageUrl, ajaxDivId, isPageRefresh, templateContent);

            var builder = new StringBuilder();
            builder.Append($@"<span id=""{ajaxDivId}""></span>");

            builder.Append($@"
<script type=""text/javascript"" language=""javascript"">
function {functionName}(pageNum)
{{
    var url = ""{apiUrl}?"" + StlClient.getQueryString();
    var data = {apiParameters};
    if (pageNum && pageNum > 0)
    {{
        data.pageNum = pageNum;
    }}

    stlClient.post(url, data, function (err, data, status) {{
        if (!err) document.getElementById(""{ajaxDivId}"").innerHTML = data.html;
    }});
}}
{functionName}(0);
</script>
");

            return builder.ToString();
        }

        internal static string ParseDynamicElement(string stlElement, PageInfo pageInfo, ContextInfo contextInfo)
        {
            stlElement = StringUtils.ReplaceIgnoreCase(stlElement, "isdynamic=\"true\"", string.Empty);
            return ParseImpl(pageInfo, contextInfo, stlElement, false);
        }
    }
}
