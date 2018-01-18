﻿using System;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using BaiRong.Core;
using SiteServer.CMS.Core;
using SiteServer.CMS.Core.Create;

namespace SiteServer.BackgroundPages.Cms
{
    public class ModalCreateChannels : BasePageCms
    {
		protected RadioButtonList IsIncludeChildren;
        protected RadioButtonList IsCreateContents;

        private string _channelIdCollection;

        public static string GetOpenWindowString(int publishmentSystemId)
        {
            return PageUtils.GetOpenWindowStringWithCheckBoxValue("生成栏目页", PageUtils.GetCmsUrl(nameof(ModalCreateChannels), new NameValueCollection
            {
                {"PublishmentSystemID", publishmentSystemId.ToString()}
            }), "ChannelIDCollection", "请选择需要生成页面的栏目!", 450, 300);
        }

		public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PageUtils.CheckRequestParameter("PublishmentSystemID", "ChannelIDCollection");

            _channelIdCollection = Body.GetQueryString("ChannelIDCollection");
		}

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            var isIncludeChildren = TranslateUtils.ToBool(IsIncludeChildren.SelectedValue);
            var isCreateContents = TranslateUtils.ToBool(IsCreateContents.SelectedValue);

            foreach (var channelId in TranslateUtils.StringCollectionToIntList(_channelIdCollection))
            {
                CreateManager.CreateChannel(PublishmentSystemId, channelId);
                if (isCreateContents)
                {
                    CreateManager.CreateAllContent(PublishmentSystemId, channelId);
                }
                if (isIncludeChildren)
                {
                    foreach (var childChannelId in DataProvider.NodeDao.GetNodeIdListForDescendant(channelId))
                    {
                        CreateManager.CreateChannel(PublishmentSystemId, childChannelId);
                        if (isCreateContents)
                        {
                            CreateManager.CreateAllContent(PublishmentSystemId, channelId);
                        }
                    }
                }
            }

            PageUtils.CloseModalPageWithoutRefresh(Page);
		}
	}
}
