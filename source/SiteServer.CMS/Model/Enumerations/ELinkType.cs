using System;
using System.Web.UI.WebControls;

namespace SiteServer.CMS.Model.Enumerations
{
	
	public enum ELinkType
	{
		LinkNoRelatedToChannelAndContent,	                                //默认
		NoLinkIfContentNotExists,			                                //无内容时不可链接
		LinkToOnlyOneContent,				                                //仅一条内容时链接到此内容
		NoLinkIfContentNotExistsAndLinkToOnlyOneContent,					//无内容时不可链接，仅一条内容时链接到此内容
		LinkToFirstContent,				                                    //链接到第一条内容
		NoLinkIfContentNotExistsAndLinkToFirstContent,						//无内容时不可链接，有内容时链接到第一条内容
		NoLinkIfChannelNotExists,			                                //无栏目时不可链接
		LinkToLastAddChannel,				                                //链接到最近增加的子栏目
		LinkToFirstChannel,					                                //链接到第一个子栏目
		NoLinkIfChannelNotExistsAndLinkToLastAddChannel,					//无栏目时不可链接，有栏目时链接到最近增加的子栏目
		NoLinkIfChannelNotExistsAndLinkToFirstChannel,						//无栏目时不可链接，有栏目时链接到第一个子栏目
		NoLink								                                //不可链接
	}

	public class ELinkTypeUtils
	{
		public static string GetValue(ELinkType type)
		{
		    if (type == ELinkType.LinkNoRelatedToChannelAndContent)
			{
				return "LinkNoRelatedToChannelAndContent";
			}
		    if (type == ELinkType.NoLinkIfContentNotExists)
		    {
		        return "NoLinkIfContentNotExists";
		    }
		    if (type == ELinkType.LinkToOnlyOneContent)
		    {
		        return "LinkToOnlyOneContent";
		    }
		    if (type == ELinkType.NoLinkIfContentNotExistsAndLinkToOnlyOneContent)
		    {
		        return "NoLinkIfContentNotExistsAndLinkToOnlyOneContent";
		    }
		    if (type == ELinkType.LinkToFirstContent)
		    {
		        return "LinkToFirstContent";
		    }
		    if (type == ELinkType.NoLinkIfContentNotExistsAndLinkToFirstContent)
		    {
		        return "NoLinkIfContentNotExistsAndLinkToFirstContent";
		    }
		    if (type == ELinkType.NoLinkIfChannelNotExists)
		    {
		        return "NoLinkIfChannelNotExists";
		    }
		    if (type == ELinkType.LinkToLastAddChannel)
		    {
		        return "LinkToLastAddChannel";
		    }
		    if (type == ELinkType.LinkToFirstChannel)
		    {
		        return "LinkToFirstChannel";
		    }
		    if (type == ELinkType.NoLinkIfChannelNotExistsAndLinkToLastAddChannel)
		    {
		        return "NoLinkIfChannelNotExistsAndLinkToLastAddChannel";
		    }
		    if (type == ELinkType.NoLinkIfChannelNotExistsAndLinkToFirstChannel)
		    {
		        return "NoLinkIfChannelNotExistsAndLinkToFirstChannel";
		    }
		    if (type == ELinkType.NoLink)
		    {
		        return "NoLink";
		    }
		    throw new Exception();
		}

		public static string GetText(ELinkType type)
		{
		    if (type == ELinkType.NoLinkIfContentNotExists)
			{
				return "无内容时不可链接";
			}
		    if (type == ELinkType.LinkToOnlyOneContent)
		    {
		        return "仅一条内容时链接到此内容";
		    }
		    if (type == ELinkType.NoLinkIfContentNotExistsAndLinkToOnlyOneContent)
		    {
		        return "无内容时不可链接，仅一条内容时链接到此内容";
		    }
		    if (type == ELinkType.LinkToFirstContent)
		    {
		        return "链接到第一条内容";
		    }
		    if (type == ELinkType.NoLinkIfContentNotExistsAndLinkToFirstContent)
		    {
		        return "无内容时不可链接，有内容时链接到第一条内容";
		    }
		    if (type == ELinkType.NoLinkIfChannelNotExists)
		    {
		        return "无栏目时不可链接";
		    }
		    if (type == ELinkType.LinkToLastAddChannel)
		    {
		        return "链接到最近增加的子栏目";
		    }
		    if (type == ELinkType.LinkToFirstChannel)
		    {
		        return "链接到第一个子栏目";
		    }
		    if (type == ELinkType.NoLinkIfChannelNotExistsAndLinkToLastAddChannel)
		    {
		        return "无栏目时不可链接，有栏目时链接到最近增加的子栏目";
		    }
		    if (type == ELinkType.NoLinkIfChannelNotExistsAndLinkToFirstChannel)
		    {
		        return "无栏目时不可链接，有栏目时链接到第一个子栏目";
		    }
		    if (type == ELinkType.NoLink)
		    {
		        return "不可链接";
		    }
		    if (type == ELinkType.LinkNoRelatedToChannelAndContent)
		    {
		        return "默认";
		    }
		    throw new Exception();
		}

		public static ELinkType GetEnumType(string typeStr)
		{
			var retval = ELinkType.LinkNoRelatedToChannelAndContent;

			if (Equals(ELinkType.NoLinkIfContentNotExists, typeStr))
			{
				retval = ELinkType.NoLinkIfContentNotExists;
			}
			else if (Equals(ELinkType.LinkToOnlyOneContent, typeStr))
			{
				retval = ELinkType.LinkToOnlyOneContent;
			}
			else if (Equals(ELinkType.NoLinkIfContentNotExistsAndLinkToOnlyOneContent, typeStr))
			{
				retval = ELinkType.NoLinkIfContentNotExistsAndLinkToOnlyOneContent;
			}
			else if (Equals(ELinkType.LinkToFirstContent, typeStr))
			{
				retval = ELinkType.LinkToFirstContent;
			}
			else if (Equals(ELinkType.NoLinkIfContentNotExistsAndLinkToFirstContent, typeStr))
			{
				retval = ELinkType.NoLinkIfContentNotExistsAndLinkToFirstContent;
			}
			else if (Equals(ELinkType.NoLinkIfChannelNotExists, typeStr))
			{
				retval = ELinkType.NoLinkIfChannelNotExists;
			}
			else if (Equals(ELinkType.LinkToLastAddChannel, typeStr))
			{
				retval = ELinkType.LinkToLastAddChannel;
			}
			else if (Equals(ELinkType.LinkToFirstChannel, typeStr))
			{
				retval = ELinkType.LinkToFirstChannel;
			}
			else if (Equals(ELinkType.NoLinkIfChannelNotExistsAndLinkToLastAddChannel, typeStr))
			{
				retval = ELinkType.NoLinkIfChannelNotExistsAndLinkToLastAddChannel;
			}
			else if (Equals(ELinkType.NoLinkIfChannelNotExistsAndLinkToFirstChannel, typeStr))
			{
				retval = ELinkType.NoLinkIfChannelNotExistsAndLinkToFirstChannel;
			}
			else if (Equals(ELinkType.NoLink, typeStr))
			{
				retval = ELinkType.NoLink;
			}
			else if (Equals(ELinkType.LinkNoRelatedToChannelAndContent, typeStr))
			{
				retval = ELinkType.LinkNoRelatedToChannelAndContent;
			}

			return retval;
		}

		public static bool Equals(ELinkType type, string typeStr)
		{
			if (string.IsNullOrEmpty(typeStr)) return false;
			if (string.Equals(GetValue(type).ToLower(), typeStr.ToLower()))
			{
				return true;
			}
			return false;
		}

        public static bool Equals(string typeStr, ELinkType type)
        {
            return Equals(type, typeStr);
        }

		public static ListItem GetListItem(ELinkType type, bool selected)
		{
			var item = new ListItem(GetText(type), GetValue(type));
			if (selected)
			{
				item.Selected = true;
			}
			return item;
		}

		public static void AddListItems(ListControl listControl)
		{
			if (listControl != null)
			{
				listControl.Items.Add(GetListItem(ELinkType.LinkNoRelatedToChannelAndContent, false));
				listControl.Items.Add(GetListItem(ELinkType.NoLinkIfContentNotExists, false));
				listControl.Items.Add(GetListItem(ELinkType.LinkToOnlyOneContent, false));
				listControl.Items.Add(GetListItem(ELinkType.NoLinkIfContentNotExistsAndLinkToOnlyOneContent, false));
				listControl.Items.Add(GetListItem(ELinkType.LinkToFirstContent, false));
				listControl.Items.Add(GetListItem(ELinkType.NoLinkIfContentNotExistsAndLinkToFirstContent, false));
				listControl.Items.Add(GetListItem(ELinkType.NoLinkIfChannelNotExists, false));
				listControl.Items.Add(GetListItem(ELinkType.LinkToLastAddChannel, false));
				listControl.Items.Add(GetListItem(ELinkType.LinkToFirstChannel, false));
				listControl.Items.Add(GetListItem(ELinkType.NoLinkIfChannelNotExistsAndLinkToLastAddChannel, false));
				listControl.Items.Add(GetListItem(ELinkType.NoLinkIfChannelNotExistsAndLinkToFirstChannel, false));
				listControl.Items.Add(GetListItem(ELinkType.NoLink, false));
			}
		}

        public static bool IsCreatable(NodeInfo nodeInfo)
        {
            var isCreatable = false;

            if (nodeInfo.LinkType == ELinkType.LinkNoRelatedToChannelAndContent)
            {
                isCreatable = true;
            }
            else if (nodeInfo.LinkType == ELinkType.NoLinkIfContentNotExists)
            {
                isCreatable = nodeInfo.ContentNum != 0;
            }
            else if (nodeInfo.LinkType == ELinkType.LinkToOnlyOneContent)
            {
                isCreatable = nodeInfo.ContentNum != 1;
            }
            else if (nodeInfo.LinkType == ELinkType.NoLinkIfContentNotExistsAndLinkToOnlyOneContent)
            {
                if (nodeInfo.ContentNum != 0 && nodeInfo.ContentNum != 1)
                {
                    isCreatable = true;
                }
            }
            else if (nodeInfo.LinkType == ELinkType.LinkToFirstContent)
            {
                isCreatable = nodeInfo.ContentNum < 1;
            }
            else if (nodeInfo.LinkType == ELinkType.NoLinkIfChannelNotExists)
            {
                isCreatable = nodeInfo.ChildrenCount != 0;
            }
            else if (nodeInfo.LinkType == ELinkType.LinkToLastAddChannel)
            {
                isCreatable = nodeInfo.ChildrenCount <= 0;
            }
            else if (nodeInfo.LinkType == ELinkType.LinkToFirstChannel)
            {
                isCreatable = nodeInfo.ChildrenCount <= 0;
            }

            return isCreatable;
        }

	}
}
