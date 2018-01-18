﻿using System;
using System.Collections;
using System.Web.UI.WebControls;
using BaiRong.Core;
using BaiRong.Core.Model.Enumerations;
using SiteServer.BackgroundPages.Cms;
using SiteServer.CMS.Core;

namespace SiteServer.BackgroundPages.Plugins
{
	public class PageTrackerMonth : BasePageCms
    {
        public Button ExportTracking;
	    readonly Hashtable accessNumHashtable = new Hashtable();
		int maxAccessNum = 0;
	    readonly Hashtable uniqueAccessNumHashtable = new Hashtable();
		int uniqueMaxAccessNum = 0;

        public int count = 12;
        public EStatictisXType xType = EStatictisXType.Month;

        public string GetGraphicX(int index)
        {
            var xNum = 0;
            var datetime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
            if (Equals(xType, EStatictisXType.Day))
            {
                datetime = datetime.AddDays(-(count - index));
                xNum = datetime.Day;
            }
            else if (Equals(xType, EStatictisXType.Month))
            {
                datetime = datetime.AddMonths(-(count - index));
                xNum = datetime.Month;
            }
            else if (Equals(xType, EStatictisXType.Year))
            {
                datetime = datetime.AddYears(-(count - index));
                xNum = datetime.Year;
            }
            else if (Equals(xType, EStatictisXType.Hour))
            {
                datetime = datetime.AddHours(-(count - index));
                xNum = datetime.Hour;
            }
            return xNum.ToString();
        }

        public string GetGraphicY(int index)
        {
            if (index <= 0 || index > count) return string.Empty;
            var accessNum = (int)accessNumHashtable[index];
            return accessNum.ToString();
        }

        public string GetUniqueGraphicY(int index)
        {
            if (index <= 0 || index > count) return string.Empty;
            var accessNum = (int)uniqueAccessNumHashtable[index];
            return accessNum.ToString();
        }

		public double GetAccessNum(int index)
		{
			double accessNum = 0;
			if (maxAccessNum > 0)
			{
				accessNum = (Convert.ToDouble(maxAccessNum) * Convert.ToDouble(index)) / 8;
				accessNum = Math.Round(accessNum, 2);
			}
			return accessNum;
		}

		public string GetGraphicHtml(int index)
		{
			if (index <= 0 || index > 12) return string.Empty;
			var accessNum = (int)accessNumHashtable[index];
			var datetime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
			datetime = datetime.AddMonths(-(12 - index));
			double height = 0;
			if (maxAccessNum >0)
			{
				height = (Convert.ToDouble(accessNum) / Convert.ToDouble(maxAccessNum)) * 200.0;
			}
            string html =
                $"<IMG title=访问量：{accessNum} height={height} style=height:{height}px src=../pic/tracker_bar.gif width=20><BR>{datetime.Month}";
			return html;
		}

		public double GetUniqueAccessNum(int index)
		{
			double uniqueAccessNum = 0;
			if (uniqueMaxAccessNum > 0)
			{
				uniqueAccessNum = (Convert.ToDouble(uniqueMaxAccessNum) * Convert.ToDouble(index)) / 8;
				uniqueAccessNum = Math.Round(uniqueAccessNum, 2);
			}
			return uniqueAccessNum;
		}

		public string GetUniqueGraphicHtml(int index)
		{
			if (index <= 0 || index > 12) return string.Empty;
			var uniqueAccessNum = (int)uniqueAccessNumHashtable[index];
			var datetime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
			datetime = datetime.AddMonths(-(12 - index));
			double height = 0;
			if (uniqueMaxAccessNum >0)
			{
				height = (Convert.ToDouble(uniqueAccessNum) / Convert.ToDouble(uniqueMaxAccessNum)) * 200.0;
			}
            string html =
                $"<IMG title=访客数：{uniqueAccessNum} height={height} style=height:{height}px src=../pic/tracker_bar.gif width=20><BR>{datetime.Month}";
			return html;
		}

		public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PageUtils.CheckRequestParameter("PublishmentSystemID");

            if (!IsPostBack)
            {
                var trackingMonthHashtable = DataProvider.TrackingDao.GetTrackingMonthHashtable(PublishmentSystemId);
                var uniqueTrackingMonthHashtable = DataProvider.TrackingDao.GetUniqueTrackingMonthHashtable(PublishmentSystemId);
                var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
                for (var i = 0; i < 12; i++)
                {
                    var datetime = now.AddMonths(-i);
                    var accessNum = 0;
                    if (trackingMonthHashtable[datetime] != null)
                    {
                        accessNum = (int)trackingMonthHashtable[datetime];
                    }
                    accessNumHashtable.Add(12 - i, accessNum);
                    if (accessNum > maxAccessNum)
                    {
                        maxAccessNum = accessNum;
                    }

                    var uniqueAccessNum = 0;
                    if (uniqueTrackingMonthHashtable[datetime] != null)
                    {
                        uniqueAccessNum = (int)uniqueTrackingMonthHashtable[datetime];
                    }
                    uniqueAccessNumHashtable.Add(12 - i, uniqueAccessNum);
                    if (uniqueAccessNum > uniqueMaxAccessNum)
                    {
                        uniqueMaxAccessNum = uniqueAccessNum;
                    }
                }

                ExportTracking.Attributes.Add("onclick", ModalExportMessage.GetOpenWindowStringToExport(PublishmentSystemId, ModalExportMessage.ExportTypeTrackerMonth));
            }
		}

	}
}
