﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.HtmlControls;
using BaiRong.Core;
using SiteServer.CMS.StlParser.Model;
using SiteServer.CMS.StlParser.Parsers;
using SiteServer.CMS.StlParser.Utility;

namespace SiteServer.CMS.StlParser.StlElement
{
    [Stl(Usage = "执行动作", Description = "通过 stl:action 标签在模板中创建链接，点击链接后将执行相应的动作")]
    public class StlAction
    {
        private StlAction() { }
        public const string ElementName = "stl:action";

        public const string AttributeType = "type";
        public const string AttributeReturnUrl = "returnUrl";

        public static SortedList<string, string> AttributeList => new SortedList<string, string>
        {
            {AttributeType, StringUtils.SortedListToAttributeValueString("动作类型", TypeList)},
            {AttributeReturnUrl, "动作完成后的返回地址"}
        };

        public const string TypeLogin = "Login";
        public const string TypeRegister = "Register";
        public const string TypeLogout = "Logout";
        public const string TypeAddFavorite = "AddFavorite";
        public const string TypeSetHomePage = "SetHomePage";
        public const string TypeTranslate = "Translate";
        public const string TypeClose = "Close";

        public static SortedList<string, string> TypeList => new SortedList<string, string>
        {
            {TypeLogin, "登录"},
            {TypeRegister, "注册"},
            {TypeLogout, "退出"},
            {TypeAddFavorite, "将页面添加至收藏夹"},
            {TypeSetHomePage, "将页面设置为首页"},
            {TypeTranslate, "繁体/简体转换"},
            {TypeClose, "关闭页面"}
        };

        public static string Parse(PageInfo pageInfo, ContextInfo contextInfo)
        {
            var type = string.Empty;
            var returnUrl = string.Empty;

            foreach (var name in contextInfo.Attributes.Keys)
            {
                var value = contextInfo.Attributes[name];
                if (StringUtils.EqualsIgnoreCase(name, AttributeType))
                {
                    type = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeReturnUrl))
                {
                    returnUrl = StlEntityParser.ReplaceStlEntitiesForAttributeValue(value, pageInfo, contextInfo);
                }
            }

            //var ie = node.Attributes?.GetEnumerator();
            //if (ie != null)
            //{
            //    while (ie.MoveNext())
            //    {
            //        var attr = (XmlAttribute)ie.Current;

            //        if (StringUtils.EqualsIgnoreCase(attr.Name, AttributeType))
            //        {
            //            type = attr.Value;
            //        }
            //        else if (StringUtils.EqualsIgnoreCase(attr.Name, AttributeReturnUrl))
            //        {
            //            returnUrl = StlEntityParser.ReplaceStlEntitiesForAttributeValue(attr.Value, pageInfo, contextInfo);
            //        }
            //        else
            //        {
            //            attributes.Add(attr.Name, attr.Value);
            //        }
            //    }
            //}

            return ParseImpl(pageInfo, contextInfo, type, returnUrl);
        }

        private static string ParseImpl(PageInfo pageInfo, ContextInfo contextInfo, string type, string returnUrl)
        {
            var stlAnchor = new HtmlAnchor();

            foreach (var attributeName in contextInfo.Attributes.Keys)
            {
                stlAnchor.Attributes.Add(attributeName, contextInfo.Attributes[attributeName]);
            }

            var url = PageUtils.UnclickedUrl;
            var onclick = string.Empty;

            var innerBuilder = new StringBuilder(contextInfo.InnerXml);
            StlParserManager.ParseInnerContent(innerBuilder, pageInfo, contextInfo);
            stlAnchor.InnerHtml = innerBuilder.ToString();

            //计算动作开始
            if (!string.IsNullOrEmpty(type))
            {
                if (StringUtils.EqualsIgnoreCase(type, TypeLogin))
                {
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        returnUrl = StlUtility.GetStlCurrentUrl(pageInfo.PublishmentSystemInfo, contextInfo.ChannelId, contextInfo.ContentId, contextInfo.ContentInfo, pageInfo.TemplateInfo.TemplateType, pageInfo.TemplateInfo.TemplateId);
                    }

                    url = HomeUtils.GetLoginUrl(pageInfo.HomeUrl, returnUrl);
                }
                else if (StringUtils.EqualsIgnoreCase(type, TypeRegister))
                {
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        returnUrl = StlUtility.GetStlCurrentUrl(pageInfo.PublishmentSystemInfo, contextInfo.ChannelId, contextInfo.ContentId, contextInfo.ContentInfo, pageInfo.TemplateInfo.TemplateType, pageInfo.TemplateInfo.TemplateId);
                    }

                    url = HomeUtils.GetRegisterUrl(pageInfo.HomeUrl, returnUrl);
                }
                else if (StringUtils.EqualsIgnoreCase(type, TypeLogout))
                {
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        returnUrl = StlUtility.GetStlCurrentUrl(pageInfo.PublishmentSystemInfo, contextInfo.ChannelId, contextInfo.ContentId, contextInfo.ContentInfo, pageInfo.TemplateInfo.TemplateType, pageInfo.TemplateInfo.TemplateId);
                    }

                    url = HomeUtils.GetLogoutUrl(pageInfo.HomeUrl, returnUrl);
                }
                else if (StringUtils.EqualsIgnoreCase(type, TypeAddFavorite))
                {
                    pageInfo.SetPageScripts(TypeAddFavorite, @"
<script type=""text/javascript""> 
    function AddFavorite(){  
        if (document.all) {
            window.external.addFavorite(window.location.href, document.title);
        } 
        else if (window.sidebar) {
            window.sidebar.addPanel(document.title, window.location.href, """");
        }
    }
</script>
", true);
                    stlAnchor.Attributes["onclick"] = "AddFavorite();";
                }
                else if (StringUtils.EqualsIgnoreCase(type, TypeSetHomePage))
                {
                    url = pageInfo.PublishmentSystemInfo.PublishmentSystemUrl;
                    pageInfo.AddPageEndScriptsIfNotExists(TypeAddFavorite, $@"
<script type=""text/javascript""> 
    function SetHomepage(){{   
        if (document.all) {{
            document.body.style.behavior = 'url(#default#homepage)';
            document.body.setHomePage(""{url}"");
        }}
        else if (window.sidebar) {{
            if (window.netscape) {{
                try {{
                    netscape.security.PrivilegeManager.enablePrivilege(""UniversalXPConnect"");
                 }}
                catch(e) {{
                    alert(""该操作被浏览器拒绝，如果想启用该功能，请在地址栏内输入 about:config,然后将项 signed.applets.codebase_principal_support 值该为true"");
                }}
             }}
            var prefs = Components.classes['@mozilla.org/preferences-service;1'].getService(Components.interfaces.nsIPrefBranch);
            prefs.setCharPref('browser.startup.homepage', ""{url}"");
        }}
    }}
</script>
");
                    stlAnchor.Attributes["onclick"] = "SetHomepage();";
                }
                else if (StringUtils.EqualsIgnoreCase(type, TypeTranslate))
                {
                    pageInfo.AddPageScriptsIfNotExists(PageInfo.JsAhTranslate);

                    var msgToTraditionalChinese = "繁體";
                    var msgToSimplifiedChinese = "简体";
                    if (!string.IsNullOrEmpty(stlAnchor.InnerHtml))
                    {
                        if (stlAnchor.InnerHtml.IndexOf(",", StringComparison.Ordinal) != -1)
                        {
                            msgToTraditionalChinese = stlAnchor.InnerHtml.Substring(0, stlAnchor.InnerHtml.IndexOf(",", StringComparison.Ordinal));
                            msgToSimplifiedChinese = stlAnchor.InnerHtml.Substring(stlAnchor.InnerHtml.IndexOf(",", StringComparison.Ordinal) + 1);
                        }
                        else
                        {
                            msgToTraditionalChinese = stlAnchor.InnerHtml;
                        }
                    }
                    stlAnchor.InnerHtml = msgToTraditionalChinese;

                    if (string.IsNullOrEmpty(stlAnchor.ID))
                    {
                        stlAnchor.ID = "translateLink";
                    }

                    pageInfo.SetPageEndScripts(TypeTranslate, $@"
<script type=""text/javascript""> 
var defaultEncoding = 0;
var translateDelay = 0;
var cookieDomain = ""/"";
var msgToTraditionalChinese = ""{msgToTraditionalChinese}"";
var msgToSimplifiedChinese = ""{msgToSimplifiedChinese}"";
var translateButtonId = ""{stlAnchor.ClientID}"";
translateInitilization();
</script>
");
                }
                else if (StringUtils.EqualsIgnoreCase(type, TypeClose))
                {
                    url = "javascript:window.close()";
                }
            }
            //计算动作结束

            stlAnchor.HRef = url;

            if (!string.IsNullOrEmpty(onclick))
            {
                stlAnchor.Attributes.Add("onclick", onclick);
            }

            // 如果是实体标签，则只返回url
            if (contextInfo.IsCurlyBrace)
            {
                return stlAnchor.HRef;
            }
            else
            {
                return ControlUtils.GetControlRenderHtml(stlAnchor);
            }
        }
    }
}
