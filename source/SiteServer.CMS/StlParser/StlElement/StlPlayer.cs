﻿using System.Collections.Generic;
using System.Text;
using BaiRong.Core;
using BaiRong.Core.Model.Attributes;
using BaiRong.Core.Model.Enumerations;
using SiteServer.CMS.Core;
using SiteServer.CMS.StlParser.Cache;
using SiteServer.CMS.StlParser.Model;
using SiteServer.CMS.StlParser.Parsers;
using SiteServer.CMS.StlParser.Utility;

namespace SiteServer.CMS.StlParser.StlElement
{
    [Stl(Usage = "播放视频", Description = "通过 stl:player 标签在模板中播放视频")]
    public class StlPlayer
	{
        private StlPlayer() { }
		public const string ElementName = "stl:player";

		public const string AttributeChannelIndex = "channelIndex";
		public const string AttributeChannelName = "channelName";
		public const string AttributeParent = "parent";
		public const string AttributeUpLevel = "upLevel";
        public const string AttributeTopLevel = "topLevel";
        public const string AttributeType = "type";
		public const string AttributePlayUrl = "playUrl";
        public const string AttributeImageUrl = "imageUrl";
        public const string AttributePlayBy = "playBy";
        public const string AttributeStretching = "stretching";
		public const string AttributeWidth = "width";
		public const string AttributeHeight = "height";
        public const string AttributeIsAutoPlay = "isAutoPlay";

        public static SortedList<string, string> AttributeList => new SortedList<string, string>
        {
            {AttributeChannelIndex, "栏目索引"},
            {AttributeChannelName, "栏目名称"},
            {AttributeParent, "显示父栏目"},
            {AttributeUpLevel, "上级栏目的级别"},
            {AttributeTopLevel, "从首页向下的栏目级别"},
            {AttributeType, "指定存储媒体的字段"},
            {AttributePlayUrl, "视频地址"},
            {AttributeImageUrl, "图片地址"},
            {AttributePlayBy, StringUtils.SortedListToAttributeValueString("指定播放器", PlayByList)},
            {AttributeStretching, "拉伸"},
            {AttributeWidth, "宽度"},
            {AttributeHeight, "高度"},
            {AttributeIsAutoPlay, "是否自动播放"}
        };

        public const string PlayByBrPlayer = "BRPlayer";
        public const string PlayByFlowPlayer = "FlowPlayer";
        public const string PlayByJwPlayer = "JWPlayer";

        public static SortedList<string, string> PlayByList => new SortedList<string, string>
        {
            {PlayByBrPlayer, "BRPlayer"},
            {PlayByFlowPlayer, "FlowPlayer"},
            {PlayByJwPlayer, "JWPlayer"}
        };

        public static string Parse(PageInfo pageInfo, ContextInfo contextInfo)
		{
		    var isGetPicUrlFromAttribute = false;
            var channelIndex = string.Empty;
            var channelName = string.Empty;
            var upLevel = 0;
            var topLevel = -1;
            var type = BackgroundContentAttribute.VideoUrl;
            var playUrl = string.Empty;
            var stretching = string.Empty;
            var imageUrl = string.Empty;
            var playBy = string.Empty;
            var width = 450;
            var height = 350;
            var isAutoPlay = true;

            foreach (var name in contextInfo.Attributes.Keys)
            {
                var value = contextInfo.Attributes[name];

                if (StringUtils.EqualsIgnoreCase(name, AttributeChannelIndex))
                {
                    channelIndex = StlEntityParser.ReplaceStlEntitiesForAttributeValue(value, pageInfo, contextInfo);
                    if (!string.IsNullOrEmpty(channelIndex))
                    {
                        isGetPicUrlFromAttribute = true;
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeChannelName))
                {
                    channelName = StlEntityParser.ReplaceStlEntitiesForAttributeValue(value, pageInfo, contextInfo);
                    if (!string.IsNullOrEmpty(channelName))
                    {
                        isGetPicUrlFromAttribute = true;
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeParent))
                {
                    if (TranslateUtils.ToBool(value))
                    {
                        upLevel = 1;
                        isGetPicUrlFromAttribute = true;
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeUpLevel))
                {
                    upLevel = TranslateUtils.ToInt(value);
                    if (upLevel > 0)
                    {
                        isGetPicUrlFromAttribute = true;
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeTopLevel))
                {
                    topLevel = TranslateUtils.ToInt(value);
                    if (topLevel >= 0)
                    {
                        isGetPicUrlFromAttribute = true;
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeType))
                {
                    type = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributePlayUrl))
                {
                    playUrl = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeImageUrl))
                {
                    imageUrl = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributePlayBy))
                {
                    playBy = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeStretching))
                {
                    stretching = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeWidth))
                {
                    width = TranslateUtils.ToInt(value, width);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeHeight))
                {
                    height = TranslateUtils.ToInt(value, height);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeIsAutoPlay))
                {
                    isAutoPlay = TranslateUtils.ToBool(value, true);
                }
            }

            return ParseImpl(pageInfo, contextInfo, isGetPicUrlFromAttribute, channelIndex, channelName, upLevel, topLevel, playUrl, imageUrl, playBy, stretching, width, height, type, isAutoPlay);
		}

        private static string ParseImpl(PageInfo pageInfo, ContextInfo contextInfo, bool isGetPicUrlFromAttribute, string channelIndex, string channelName, int upLevel, int topLevel, string playUrl, string imageUrl, string playBy, string stretching, int width, int height, string type, bool isAutoPlay)
        {
            var parsedContent = string.Empty;

            var contentId = 0;
            //判断是否图片地址由标签属性获得
            if (!isGetPicUrlFromAttribute)
            {
                contentId = contextInfo.ContentId;
            }

            if (string.IsNullOrEmpty(playUrl))
            {
                if (contentId != 0)//获取内容视频
                {
                    if (contextInfo.ContentInfo == null)
                    {
                        //playUrl = BaiRongDataProvider.ContentDao.GetValue(pageInfo.PublishmentSystemInfo.AuxiliaryTableForContent, contentId, type);
                        playUrl = Content.GetValue(pageInfo.PublishmentSystemInfo.AuxiliaryTableForContent, contentId, type);
                        if (string.IsNullOrEmpty(playUrl))
                        {
                            if (!StringUtils.EqualsIgnoreCase(type, BackgroundContentAttribute.VideoUrl))
                            {
                                //playUrl = BaiRongDataProvider.ContentDao.GetValue(pageInfo.PublishmentSystemInfo.AuxiliaryTableForContent, contentId, BackgroundContentAttribute.VideoUrl);
                                playUrl = Content.GetValue(pageInfo.PublishmentSystemInfo.AuxiliaryTableForContent, contentId, BackgroundContentAttribute.VideoUrl);
                            }
                        }
                        if (string.IsNullOrEmpty(playUrl))
                        {
                            if (!StringUtils.EqualsIgnoreCase(type, BackgroundContentAttribute.FileUrl))
                            {
                                //playUrl = BaiRongDataProvider.ContentDao.GetValue(pageInfo.PublishmentSystemInfo.AuxiliaryTableForContent, contentId, BackgroundContentAttribute.FileUrl);
                                playUrl = Content.GetValue(pageInfo.PublishmentSystemInfo.AuxiliaryTableForContent, contentId, BackgroundContentAttribute.FileUrl);
                            }
                        }
                    }
                    else
                    {
                        playUrl = contextInfo.ContentInfo.GetExtendedAttribute(type);
                        if (string.IsNullOrEmpty(playUrl))
                        {
                            playUrl = contextInfo.ContentInfo.GetExtendedAttribute(BackgroundContentAttribute.VideoUrl);
                        }
                        if (string.IsNullOrEmpty(playUrl))
                        {
                            playUrl = contextInfo.ContentInfo.GetExtendedAttribute(BackgroundContentAttribute.FileUrl);
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                if (contentId != 0)
                {
                    //imageUrl = contextInfo.ContentInfo == null ? BaiRongDataProvider.ContentDao.GetValue(pageInfo.PublishmentSystemInfo.AuxiliaryTableForContent, contentId, BackgroundContentAttribute.ImageUrl) : contextInfo.ContentInfo.GetExtendedAttribute(BackgroundContentAttribute.ImageUrl);
                    imageUrl = contextInfo.ContentInfo == null ? Content.GetValue(pageInfo.PublishmentSystemInfo.AuxiliaryTableForContent, contentId, BackgroundContentAttribute.ImageUrl) : contextInfo.ContentInfo.GetExtendedAttribute(BackgroundContentAttribute.ImageUrl);
                }
            }
            if (string.IsNullOrEmpty(imageUrl))
            {
                var channelId = StlDataUtility.GetNodeIdByLevel(pageInfo.PublishmentSystemId, contextInfo.ChannelId, upLevel, topLevel);
                channelId = StlDataUtility.GetNodeIdByChannelIdOrChannelIndexOrChannelName(pageInfo.PublishmentSystemId, channelId, channelIndex, channelName);
                var channel = NodeManager.GetNodeInfo(pageInfo.PublishmentSystemId, channelId);
                imageUrl = channel.ImageUrl;
            }

            if (!string.IsNullOrEmpty(playUrl))
            {
                var extension = PathUtils.GetExtension(playUrl);
                if (EFileSystemTypeUtils.IsFlash(extension))
                {
                    parsedContent = StlFlash.Parse(pageInfo, contextInfo);
                }
                else if (EFileSystemTypeUtils.IsImage(extension))
                {
                    parsedContent = StlImage.Parse(pageInfo, contextInfo);
                }
                else
                {
                    var uniqueId = pageInfo.UniqueId;
                    playUrl = PageUtility.ParseNavigationUrl(pageInfo.PublishmentSystemInfo, playUrl);
                    imageUrl = PageUtility.ParseNavigationUrl(pageInfo.PublishmentSystemInfo, imageUrl);

                    var fileType = EFileSystemTypeUtils.GetEnumType(extension);
                    if (fileType == EFileSystemType.Avi)
                    {
                        parsedContent = $@"
<object id=""palyer_{uniqueId}"" width=""{width}"" height=""{height}"" border=""0"" classid=""clsid:CFCDAA03-8BE4-11cf-B84B-0020AFBBCCFA"">
<param name=""ShowDisplay"" value=""0"">
<param name=""ShowControls"" value=""1"">
<param name=""AutoStart"" value=""{(isAutoPlay ? "1" : "0")}"">
<param name=""AutoRewind"" value=""0"">
<param name=""PlayCount"" value=""0"">
<param name=""Appearance"" value=""0"">
<param name=""BorderStyle"" value=""0"">
<param name=""MovieWindowHeight"" value=""240"">
<param name=""MovieWindowWidth"" value=""320"">
<param name=""FileName"" value=""{playUrl}"">
<embed width=""{width}"" height=""{height}"" border=""0"" showdisplay=""0"" showcontrols=""1"" autostart=""{(isAutoPlay
                            ? "1"
                            : "0")}"" autorewind=""0"" playcount=""0"" moviewindowheight=""240"" moviewindowwidth=""320"" filename=""{playUrl}"" src=""{playUrl}"">
</embed>
</object>
";
                    }
                    else if (fileType == EFileSystemType.Mpg)
                    {
                        parsedContent = $@"
<object classid=""clsid:05589FA1-C356-11CE-BF01-00AA0055595A"" id=""palyer_{uniqueId}"" width=""{width}"" height=""{height}"">
<param name=""Appearance"" value=""0"">
<param name=""AutoStart"" value=""{(isAutoPlay ? "true" : "false")}"">
<param name=""AllowChangeDisplayMode"" value=""-1"">
<param name=""AllowHideDisplay"" value=""0"">
<param name=""AllowHideControls"" value=""-1"">
<param name=""AutoRewind"" value=""-1"">
<param name=""Balance"" value=""0"">
<param name=""CurrentPosition"" value=""0"">
<param name=""DisplayBackColor"" value=""0"">
<param name=""DisplayForeColor"" value=""16777215"">
<param name=""DisplayMode"" value=""0"">
<param name=""Enabled"" value=""-1"">
<param name=""EnableContextMenu"" value=""-1"">
<param name=""EnablePositionControls"" value=""-1"">
<param name=""EnableSelectionControls"" value=""0"">
<param name=""EnableTracker"" value=""-1"">
<param name=""Filename"" value=""{playUrl}"" valuetype=""ref"">
<param name=""FullScreenMode"" value=""0"">
<param name=""MovieWindowSize"" value=""0"">
<param name=""PlayCount"" value=""1"">
<param name=""Rate"" value=""1"">
<param name=""SelectionStart"" value=""-1"">
<param name=""SelectionEnd"" value=""-1"">
<param name=""ShowControls"" value=""-1"">
<param name=""ShowDisplay"" value=""-1"">
<param name=""ShowPositionControls"" value=""0"">
<param name=""ShowTracker"" value=""-1"">
<param name=""Volume"" value=""-480"">
</object>
";
                    }
                    else if (fileType == EFileSystemType.Mpg)
                    {
                        parsedContent = $@"
<OBJECT id=""palyer_{uniqueId}"" classid=""clsid:CFCDAA03-8BE4-11cf-B84B-0020AFBBCCFA"" width=""{width}"" height=""{height}"">
<param name=""_ExtentX"" value=""6350"">
<param name=""_ExtentY"" value=""4763"">
<param name=""AUTOSTART"" value=""{(isAutoPlay ? "true" : "false")}"">
<param name=""SHUFFLE"" value=""0"">
<param name=""PREFETCH"" value=""0"">
<param name=""NOLABELS"" value=""-1"">
<param name=""SRC"" value=""{playUrl}"">
<param name=""CONTROLS"" value=""ImageWindow"">
<param name=""CONSOLE"" value=""console1"">
<param name=""LOOP"" value=""0"">
<param name=""NUMLOOP"" value=""0"">
<param name=""CENTER"" value=""0"">
<param name=""MAINTAINASPECT"" value=""0"">
<param name=""BACKGROUNDCOLOR"" value=""#000000"">
<embed src=""{playUrl}"" type=""audio/x-pn-realaudio-plugin"" console=""Console1"" controls=""ImageWindow"" width=""{width}"" height=""{height}"" autostart=""{(isAutoPlay
                            ? "true"
                            : "false")}""></OBJECT>
";
                    }
                    else if (fileType == EFileSystemType.Rm)
                    {
                        parsedContent = $@"
<OBJECT id=""palyer_{uniqueId}"" CLASSID=""clsid:CFCDAA03-8BE4-11cf-B84B-0020AFBBCCFA"" WIDTH=""{width}"" HEIGHT=""{height}"">
<param name=""_ExtentX"" value=""9313"">
<param name=""_ExtentY"" value=""7620"">
<param name=""AUTOSTART"" value=""{(isAutoPlay ? "true" : "false")}"">
<param name=""SHUFFLE"" value=""0"">
<param name=""PREFETCH"" value=""0"">
<param name=""NOLABELS"" value=""0"">
<param name=""SRC"" value=""{playUrl}"">
<param name=""CONTROLS"" value=""ImageWindow"">
<param name=""CONSOLE"" value=""Clip1"">
<param name=""LOOP"" value=""0"">
<param name=""NUMLOOP"" value=""0"">
<param name=""CENTER"" value=""0"">
<param name=""MAINTAINASPECT"" value=""0"">
<param name=""BACKGROUNDCOLOR"" value=""#000000"">
<embed SRC type=""audio/x-pn-realaudio-plugin"" CONSOLE=""Clip1"" CONTROLS=""ImageWindow"" WIDTH=""{width}"" HEIGHT=""{height}"" AUTOSTART=""{(isAutoPlay
                            ? "true"
                            : "false")}"">
</OBJECT>
";
                    }
                    else if (fileType == EFileSystemType.Wmv)
                    {
                        parsedContent = $@"
<object id=""palyer_{uniqueId}"" WIDTH=""{width}"" HEIGHT=""{height}"" classid=""CLSID:22d6f312-b0f6-11d0-94ab-0080c74c7e95"" codebase=""http://activex.microsoft.com/activex/controls/mplayer/en/nsmp2inf.cab#Version=6,4,5,715"" standby=""Loading Microsoft Windows Media Player components..."" type=""application/x-oleobject"" align=""right"" hspace=""5"">
<param name=""AutoRewind"" value=""1"">
<param name=""ShowControls"" value=""1"">
<param name=""ShowPositionControls"" value=""0"">
<param name=""ShowAudioControls"" value=""1"">
<param name=""ShowTracker"" value=""0"">
<param name=""ShowDisplay"" value=""0"">
<param name=""ShowStatusBar"" value=""0"">
<param name=""ShowGotoBar"" value=""0"">
<param name=""ShowCaptioning"" value=""0"">
<param name=""AutoStart"" value=""{(isAutoPlay ? "1" : "0")}"">
<param name=""FileName"" value=""{playUrl}"">
<param name=""Volume"" value=""-2500"">
<param name=""AnimationAtStart"" value=""0"">
<param name=""TransparentAtStart"" value=""0"">
<param name=""AllowChangeDisplaySize"" value=""0"">
<param name=""AllowScan"" value=""0"">
<param name=""EnableContextMenu"" value=""0"">
<param name=""ClickToPlay"" value=""0"">
</object>
";
                    }
                    else if (fileType == EFileSystemType.Wma)
                    {
                        parsedContent = $@"
<object classid=""clsid:22D6F312-B0F6-11D0-94AB-0080C74C7E95"" id=""palyer_{uniqueId}"">
<param name=""Filename"" value=""{playUrl}"">
<param name=""PlayCount"" value=""1"">
<param name=""AutoStart"" value=""{(isAutoPlay ? "1" : "0")}"">
<param name=""ClickToPlay"" value=""1"">
<param name=""DisplaySize"" value=""0"">
<param name=""EnableFullScreen Controls"" value=""1"">
<param name=""ShowAudio Controls"" value=""1"">
<param name=""EnableContext Menu"" value=""1"">
<param name=""ShowDisplay"" value=""1"">
</object>
";
                    }
                    else if (fileType == EFileSystemType.Rm || fileType == EFileSystemType.Rmb || fileType == EFileSystemType.Rmvb)
                    {
                        if (!contextInfo.Attributes.ContainsKey("ShowDisplay"))
                        {
                            contextInfo.Attributes["ShowDisplay"] = "0";
                        }
                        if (!contextInfo.Attributes.ContainsKey("ShowControls"))
                        {
                            contextInfo.Attributes["ShowControls"] = "1";
                        }
                        contextInfo.Attributes["AutoStart"] = isAutoPlay ? "1" : "0";
                        if (!contextInfo.Attributes.ContainsKey("AutoRewind"))
                        {
                            contextInfo.Attributes["AutoRewind"] = "0";
                        }
                        if (!contextInfo.Attributes.ContainsKey("PlayCount"))
                        {
                            contextInfo.Attributes["PlayCount"] = "0";
                        }
                        if (!contextInfo.Attributes.ContainsKey("Appearance"))
                        {
                            contextInfo.Attributes["Appearance"] = "0";
                        }
                        if (!contextInfo.Attributes.ContainsKey("BorderStyle"))
                        {
                            contextInfo.Attributes["BorderStyle"] = "0";
                        }
                        if (!contextInfo.Attributes.ContainsKey("Controls"))
                        {
                            contextInfo.Attributes["ImageWindow"] = "0";
                        }
                        contextInfo.Attributes["moviewindowheight"] = height.ToString();
                        contextInfo.Attributes["moviewindowwidth"] = width.ToString();
                        contextInfo.Attributes["filename"] = playUrl;
                        contextInfo.Attributes["src"] = playUrl;

                        var paramBuilder = new StringBuilder();
                        var embedBuilder = new StringBuilder();
                        foreach (string key in contextInfo.Attributes.Keys)
                        {
                            paramBuilder.Append($@"<param name=""{key}"" value=""{contextInfo.Attributes[key]}"">").Append(StringUtils.Constants.ReturnAndNewline);
                            embedBuilder.Append($@" {key}=""{contextInfo.Attributes[key]}""");
                        }

                        parsedContent = $@"
<object id=""video_{uniqueId}"" width=""{width}"" height=""{height}"" border=""0"" classid=""clsid:CFCDAA03-8BE4-11cf-B84B-0020AFBBCCFA"">
{paramBuilder}
<embed{embedBuilder}>
</embed>
</object>
";
                    }
                    else
                    {
                        if (StringUtils.EqualsIgnoreCase(playBy, PlayByFlowPlayer))
                        {
                            var ajaxElementId = StlParserUtility.GetAjaxDivId(pageInfo.UniqueId);
                            pageInfo.AddPageScriptsIfNotExists(PageInfo.JsAcFlowPlayer);

                            var swfUrl = SiteFilesAssets.GetUrl(pageInfo.ApiUrl, SiteFilesAssets.FlowPlayer.Swf);
                            parsedContent = $@"
<a href=""{playUrl}"" style=""display:block;width:{width}px;height:{height}px;"" id=""player_{ajaxElementId}""></a>
<script language=""javascript"">
    flowplayer(""player_{ajaxElementId}"", ""{swfUrl}"", {{
        clip:  {{
            autoPlay: {isAutoPlay.ToString().ToLower()}
        }}
    }});
</script>
";
                        }
                        else if (StringUtils.EqualsIgnoreCase(playBy, PlayByJwPlayer))
                        {
                            pageInfo.AddPageScriptsIfNotExists(PageInfo.JsAcJwPlayer6);
                            var ajaxElementId = StlParserUtility.GetAjaxDivId(pageInfo.UniqueId);
                            parsedContent = $@"
<div id='{ajaxElementId}'></div>
<script type='text/javascript'>
	jwplayer('{ajaxElementId}').setup({{
        autostart: {isAutoPlay.ToString().ToLower()},
		file: ""{playUrl}"",
		width: ""{width}"",
		height: ""{height}"",
		image: ""{imageUrl}""
	}});
</script>
";
                        }
                        else
                        {
                            var additional = string.Empty;
                            if (!string.IsNullOrEmpty(stretching))
                            {
                                additional = "&stretching=" + stretching;
                            }
                            parsedContent = $@"
<embed src=""{SiteFilesAssets.GetUrl(pageInfo.ApiUrl, SiteFilesAssets.BrPlayer.Swf)}"" allowfullscreen=""true"" flashvars=""controlbar=over{additional}&autostart={isAutoPlay.ToString().ToLower()}&image={imageUrl}&file={playUrl}"" width=""{width}"" height=""{height}""/>
";
                        }
                    }
                }
            }

            return parsedContent;
        }
	}
}
