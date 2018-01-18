﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using BaiRong.Core;
using SiteServer.CMS.StlParser.Model;
using SiteServer.CMS.StlParser.Utility;

namespace SiteServer.CMS.StlParser.StlElement
{
    [Stl(Usage = "页签切换", Description = "通过 stl:tabs 标签在模板中显示页签切换")]
    public class StlTabs
    {
        private StlTabs() { }
        public const string ElementName = "stl:tabs";

        public const string AttributeTabName = "tabName";
        public const string AttributeIsHeader = "isHeader";
        public const string AttributeAction = "action";
        public const string AttributeClassActive = "classActive";
        public const string AttributeClassNormal = "classNormal";
        public const string AttributeCurrent = "current";

        public static SortedList<string, string> AttributeList => new SortedList<string, string>
        {
            {AttributeTabName, "页签名称"},
            {AttributeIsHeader, "是否为页签头部"},
            {AttributeAction, StringUtils.SortedListToAttributeValueString("页签切换方式", ActionList)},
            {AttributeClassActive, "当前显示页签头部的CSS类"},
            {AttributeClassNormal, "当前隐藏页签头部的CSS类"},
            {AttributeCurrent, "当前页签"}
        };

        public const string ActionClick = "Click";
        public const string ActionMouseOver = "MouseOver";

        public static SortedList<string, string> ActionList => new SortedList<string, string>
        {
            {ActionClick, "点击"},
            {ActionMouseOver, "鼠标移动"}
        };

        internal static string Parse(PageInfo pageInfo, ContextInfo contextInfo)
        {
            var tabName = string.Empty;
            var isHeader = true;
            var action = ActionMouseOver;
            var classActive = "active";
            var classNormal = string.Empty;
            var current = 0;

            foreach (var name in contextInfo.Attributes.Keys)
            {
                var value = contextInfo.Attributes[name];

                if (StringUtils.EqualsIgnoreCase(name, AttributeTabName))
                {
                    tabName = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeIsHeader))
                {
                    isHeader = TranslateUtils.ToBool(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeAction))
                {
                    action = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeClassActive))
                {
                    classActive = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeClassNormal))
                {
                    classNormal = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeCurrent))
                {
                    current = TranslateUtils.ToInt(value, 1);
                }
            }

            return ParseImpl(pageInfo, contextInfo, tabName, isHeader, action, classActive, classNormal, current);
        }

        private static string ParseImpl(PageInfo pageInfo, ContextInfo contextInfo, string tabName, bool isHeader, string action, string classActive, string classNormal, int current)
        {
            pageInfo.AddPageScriptsIfNotExists(PageInfo.Components.Jquery);

            var builder = new StringBuilder();
            var uniqueId = pageInfo.UniqueId;

            if (contextInfo.ChildNodes.Count > 0)
            {
                if (isHeader)
                {
                    builder.Append($@"
<SCRIPT language=javascript>
function stl_tab_{uniqueId}(tabName, no){{
	for ( i = 1; i <= {contextInfo.ChildNodes.Count}; i++){{
		var el = jQuery('#{tabName}_tabContent_' + i);
		var li = $('#{tabName}_tabHeader_' + i);
		if (i == no){{
            try{{
			    el.show();
            }}catch(e){{}}
            li.removeClass('{classNormal}');
            li.addClass('{classActive}');
		}}else{{
            try{{
			    el.hide();
            }}catch(e){{}}
            li.removeClass('{classActive}');
            li.addClass('{classNormal}');
		}}
	}}
}}
</SCRIPT>
");
                }

                var count = 0;
                foreach (XmlNode xmlNode in contextInfo.ChildNodes)
                {
                    if (xmlNode.NodeType != XmlNodeType.Element) continue;
                    var attributes = new NameValueCollection();
                    if (xmlNode.Attributes != null)
                    {
                        var xmlIe = xmlNode.Attributes.GetEnumerator();
                        while (xmlIe.MoveNext())
                        {
                            var attr = (XmlAttribute)xmlIe.Current;
                            var attributeName = attr.Name.ToLower();
                            if (!StringUtils.EqualsIgnoreCase(attr.Name, "id") && !StringUtils.EqualsIgnoreCase(attr.Name, "onmouseover") && !StringUtils.EqualsIgnoreCase(attr.Name, "onclick"))
                            {
                                attributes[attributeName] = attr.Value;
                            }
                        }
                    }

                    count++;
                    if (isHeader)
                    {
                        attributes["id"] = $"{tabName}_tabHeader_{count}";
                        if (StringUtils.EqualsIgnoreCase(action, ActionMouseOver))
                        {
                            attributes["onmouseover"] = $"stl_tab_{uniqueId}('{tabName}', {count});return false;";
                        }
                        else
                        {
                            attributes["onclick"] = $"stl_tab_{uniqueId}('{tabName}', {count});return false;";
                        }
                        if (current != 0)
                        {
                            if (count == current)
                            {
                                attributes["class"] = classActive;
                            }
                            else
                            {
                                attributes["class"] = classNormal;
                            }
                        }
                    }
                    else
                    {
                        attributes["id"] = $"{tabName}_tabContent_{count}";
                        if (current != 0)
                        {
                            if (count != current)
                            {
                                attributes["style"] = $"display:none;{attributes["style"]}";
                            }
                        }
                    }

                    var innerXml = string.Empty;
                    if (!string.IsNullOrEmpty(xmlNode.InnerXml))
                    {
                        var innerBuilder = new StringBuilder(xmlNode.InnerXml);
                        StlParserManager.ParseInnerContent(innerBuilder, pageInfo, contextInfo);
                        StlParserUtility.XmlToHtml(innerBuilder);
                        innerXml = innerBuilder.ToString();
                    }

                    builder.Append(
                        $"<{xmlNode.Name} {TranslateUtils.ToAttributesString(attributes)}>{innerXml}</{xmlNode.Name}>");
                }
            }

            return builder.ToString();
        }
    }
}
