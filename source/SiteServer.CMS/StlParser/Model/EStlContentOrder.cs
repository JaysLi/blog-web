using System;
using System.Web.UI.WebControls;

namespace SiteServer.CMS.StlParser.Model
{
	public enum EStlContentOrder
	{
        Default,				    //Ĭ�����򣬼����ݹ�������е�����
        Back,			            //Ĭ��������෴����
        AddDate,				    //�����ʱ������
        AddDateBack,			    //�����ʱ����෴��������
        LastEditDate,			    //��������ʱ������
        LastEditDateBack,		    //��������ʱ����෴��������
        Hits,	                    //�����������
        HitsByDay,                  //���յ��������
        HitsByWeek,			        //���ܵ��������
        HitsByMonth,		        //���µ��������
        Stars,                      //������������
        Digg,                       //������������
        Comments,                   //������������
        Random                      //�����ʾ����
	}


	public class EStlContentOrderUtils
	{
		public static string GetValue(EStlContentOrder type)
		{
            if (type == EStlContentOrder.Default)
			{
                return "Default";
			}
            else if (type == EStlContentOrder.Back)
			{
                return "Back";
			}
            else if (type == EStlContentOrder.AddDate)
			{
                return "AddDate";
			}
            else if (type == EStlContentOrder.AddDateBack)
			{
                return "AddDateBack";
			}
            else if (type == EStlContentOrder.LastEditDate)
			{
                return "LastEditDate";
			}
            else if (type == EStlContentOrder.LastEditDateBack)
			{
                return "LastEditDateBack";
			}
            else if (type == EStlContentOrder.Hits)
			{
                return "Hits";
			}
            else if (type == EStlContentOrder.HitsByDay)
			{
                return "HitsByDay";
			}
            else if (type == EStlContentOrder.HitsByWeek)
			{
                return "HitsByWeek";
			}
            else if (type == EStlContentOrder.HitsByMonth)
			{
                return "HitsByMonth";
            }
            else if (type == EStlContentOrder.Stars)
            {
                return "Stars";
            }
            else if (type == EStlContentOrder.Digg)
            {
                return "Digg";
            }
            else if (type == EStlContentOrder.Comments)
            {
                return "Comments";
            }
            else if (type == EStlContentOrder.Random)
            {
                return "Random";
            }
			else
			{
				throw new Exception();
			}
		}

		public static string GetText(EStlContentOrder type)
		{
            if (type == EStlContentOrder.Default)
			{
                return "Ĭ�����򣬼����ݹ�������е�����";
			}
            else if (type == EStlContentOrder.Back)
			{
                return "Ĭ��������෴����";
			}
            else if (type == EStlContentOrder.AddDate)
			{
                return "�����ʱ������";
			}
            else if (type == EStlContentOrder.AddDateBack)
			{
                return "�����ʱ����෴��������";
			}
            else if (type == EStlContentOrder.LastEditDate)
			{
                return "��������ʱ������";
			}
            else if (type == EStlContentOrder.LastEditDateBack)
			{
                return "��������ʱ����෴��������";
			}
            else if (type == EStlContentOrder.Hits)
			{
                return "�����������";
			}
            else if (type == EStlContentOrder.HitsByDay)
			{
                return "���յ��������";
			}
            else if (type == EStlContentOrder.HitsByWeek)
			{
                return "���ܵ��������";
			}
            else if (type == EStlContentOrder.HitsByMonth)
			{
                return "���µ��������";
            }
            else if (type == EStlContentOrder.Stars)
            {
                return "������������";
            }
            else if (type == EStlContentOrder.Digg)
            {
                return "������������";
            }
            else if (type == EStlContentOrder.Comments)
            {
                return "������������";
            }
            else if (type == EStlContentOrder.Random)
            {
                return "�����ʾ����";
            }
			else
			{
				throw new Exception();
			}
		}

		public static EStlContentOrder GetEnumType(string typeStr)
		{
            var retval = EStlContentOrder.Default;

            if (Equals(EStlContentOrder.Default, typeStr))
			{
                retval = EStlContentOrder.Default;
			}
            else if (Equals(EStlContentOrder.Back, typeStr))
			{
                retval = EStlContentOrder.Back;
			}
            else if (Equals(EStlContentOrder.AddDate, typeStr))
			{
                retval = EStlContentOrder.AddDate;
			}
            else if (Equals(EStlContentOrder.AddDateBack, typeStr))
			{
                retval = EStlContentOrder.AddDateBack;
			}
            else if (Equals(EStlContentOrder.LastEditDate, typeStr))
			{
                retval = EStlContentOrder.LastEditDate;
			}
            else if (Equals(EStlContentOrder.LastEditDateBack, typeStr))
			{
                retval = EStlContentOrder.LastEditDateBack;
			}
            else if (Equals(EStlContentOrder.Hits, typeStr))
			{
                retval = EStlContentOrder.Hits;
			}
            else if (Equals(EStlContentOrder.HitsByDay, typeStr))
			{
                retval = EStlContentOrder.HitsByDay;
			}
            else if (Equals(EStlContentOrder.HitsByWeek, typeStr))
			{
                retval = EStlContentOrder.HitsByWeek;
			}
            else if (Equals(EStlContentOrder.HitsByMonth, typeStr))
			{
                retval = EStlContentOrder.HitsByMonth;
            }
            else if (Equals(EStlContentOrder.Stars, typeStr))
            {
                retval = EStlContentOrder.Stars;
            }
            else if (Equals(EStlContentOrder.Digg, typeStr))
            {
                retval = EStlContentOrder.Digg;
            }
            else if (Equals(EStlContentOrder.Comments, typeStr))
            {
                retval = EStlContentOrder.Comments;
            }
            else if (Equals(EStlContentOrder.Random, typeStr))
            {
                retval = EStlContentOrder.Random;
            }

			return retval;
		}

		public static bool Equals(EStlContentOrder type, string typeStr)
		{
			if (string.IsNullOrEmpty(typeStr)) return false;
			if (string.Equals(GetValue(type).ToLower(), typeStr.ToLower()))
			{
				return true;
			}
			return false;
		}

        public static bool Equals(string typeStr, EStlContentOrder type)
        {
            return Equals(type, typeStr);
        }

		public static ListItem GetListItem(EStlContentOrder type, bool selected)
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
                listControl.Items.Add(GetListItem(EStlContentOrder.Default, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.Back, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.AddDate, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.AddDateBack, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.LastEditDate, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.LastEditDateBack, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.Hits, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.HitsByDay, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.HitsByWeek, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.HitsByMonth, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.Stars, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.Digg, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.Comments, false));
                listControl.Items.Add(GetListItem(EStlContentOrder.Random, false));
			}
		}

	}
}
