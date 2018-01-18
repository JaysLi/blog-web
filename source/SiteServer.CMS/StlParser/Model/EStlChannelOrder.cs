using System;
using System.Web.UI.WebControls;

namespace SiteServer.CMS.StlParser.Model
{
	public enum EStlChannelOrder
	{
        Default,				    //Ĭ�����򣬼���Ŀ��������е�����
        Back,			            //Ĭ��������෴����
        AddDate,				    //�����ʱ������
        AddDateBack,			    //�����ʱ����෴��������
        Hits,	                    //�����������
        Random                      //�����ʾ����
	}


	public class EStlChannelOrderUtils
	{
		public static string GetValue(EStlChannelOrder type)
		{
            if (type == EStlChannelOrder.Default)
			{
                return "Default";
			}
            else if (type == EStlChannelOrder.Back)
			{
                return "Back";
			}
            else if (type == EStlChannelOrder.AddDate)
			{
                return "AddDate";
			}
            else if (type == EStlChannelOrder.AddDateBack)
			{
                return "AddDateBack";
			}
            else if (type == EStlChannelOrder.Hits)
			{
                return "Hits";
			}
            else if (type == EStlChannelOrder.Random)
            {
                return "Random";
            }
			else
			{
				throw new Exception();
			}
		}

		public static string GetText(EStlChannelOrder type)
		{
            if (type == EStlChannelOrder.Default)
			{
                return "Ĭ�����򣬼���Ŀ��������е�����";
			}
            else if (type == EStlChannelOrder.Back)
			{
                return "Ĭ��������෴����";
			}
            else if (type == EStlChannelOrder.AddDate)
			{
                return "�����ʱ������";
			}
            else if (type == EStlChannelOrder.AddDateBack)
			{
                return "�����ʱ����෴��������";
			}
            else if (type == EStlChannelOrder.Hits)
			{
                return "�����������";
			}
            else if (type == EStlChannelOrder.Random)
            {
                return "�����ʾ����";
            }
			else
			{
				throw new Exception();
			}
		}

		public static EStlChannelOrder GetEnumType(string typeStr)
		{
            var retval = EStlChannelOrder.Default;

            if (Equals(EStlChannelOrder.Default, typeStr))
			{
                retval = EStlChannelOrder.Default;
			}
            else if (Equals(EStlChannelOrder.Back, typeStr))
			{
                retval = EStlChannelOrder.Back;
			}
            else if (Equals(EStlChannelOrder.AddDate, typeStr))
			{
                retval = EStlChannelOrder.AddDate;
			}
            else if (Equals(EStlChannelOrder.AddDateBack, typeStr))
			{
                retval = EStlChannelOrder.AddDateBack;
			}
            else if (Equals(EStlChannelOrder.Hits, typeStr))
			{
                retval = EStlChannelOrder.Hits;
			}
            else if (Equals(EStlChannelOrder.Random, typeStr))
            {
                retval = EStlChannelOrder.Random;
            }

			return retval;
		}

		public static bool Equals(EStlChannelOrder type, string typeStr)
		{
			if (string.IsNullOrEmpty(typeStr)) return false;
			if (string.Equals(GetValue(type).ToLower(), typeStr.ToLower()))
			{
				return true;
			}
			return false;
		}

        public static bool Equals(string typeStr, EStlChannelOrder type)
        {
            return Equals(type, typeStr);
        }

		public static ListItem GetListItem(EStlChannelOrder type, bool selected)
		{
			var item = new ListItem(GetValue(type) + " (" + GetText(type) + ")", GetValue(type));
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
                listControl.Items.Add(GetListItem(EStlChannelOrder.Default, false));
                listControl.Items.Add(GetListItem(EStlChannelOrder.Back, false));
                listControl.Items.Add(GetListItem(EStlChannelOrder.AddDate, false));
                listControl.Items.Add(GetListItem(EStlChannelOrder.AddDateBack, false));
                listControl.Items.Add(GetListItem(EStlChannelOrder.Hits, false));
                listControl.Items.Add(GetListItem(EStlChannelOrder.Random, false));
			}
		}

	}
}
