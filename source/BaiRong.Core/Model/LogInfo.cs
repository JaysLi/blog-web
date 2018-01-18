﻿using System;

namespace BaiRong.Core.Model
{
    public class LogInfo
    {
        public const string AdminLogin = "后台管理员登录";

        public LogInfo()
        {
            Id = 0;
            UserName = string.Empty;
            IpAddress = string.Empty;
            AddDate = DateTime.Now;
            Action = string.Empty;
            Summary = string.Empty;
        }

        public LogInfo(int id, string userName, string ipAddress, DateTime addDate, string action, string summary)
        {
            Id = id;
            UserName = userName;
            IpAddress = ipAddress;
            AddDate = addDate;
            Action = action;
            Summary = summary;
        }

        public int Id { get; set; }

        public string UserName { get; set; }

        public string IpAddress { get; set; }

        public DateTime AddDate { get; set; }

        public string Action { get; set; }

        public string Summary { get; set; }
    }
}
