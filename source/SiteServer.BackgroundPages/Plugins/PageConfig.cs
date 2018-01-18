﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using BaiRong.Core;
using BaiRong.Core.Model.Enumerations;
using SiteServer.CMS.Plugin;

namespace SiteServer.BackgroundPages.Plugins
{
    public class PageConfig : BasePage
    {
        private string _pluginId;

        public DropDownList DdlIsDefault;
        public PlaceHolder PhCustom;
        public DropDownList DdlSqlDatabaseType;
        public PlaceHolder PhSql1;
        public TextBox TbSqlServer;
        public DropDownList DdlIsTrustedConnection;
        public PlaceHolder PhSqlUserNamePassword;
        public TextBox TbSqlUserName;
        public TextBox TbSqlPassword;
        public HtmlInputHidden HihSqlHiddenPassword;
        public Button BtnConnect;
        public PlaceHolder PhSql2;
        public DropDownList DdlSqlDatabaseName;

        public static string GetRedirectUrl(string pluginId)
        {
            return PageUtils.GetPluginsUrl(nameof(PageConfig), new NameValueCollection
            {
                {"pluginId", pluginId}
            });
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            _pluginId = Body.GetQueryString("pluginId");

            if (Page.IsPostBack) return;

            BreadCrumbPlugins("插件配置", AppManager.Permissions.Plugins.Management);

            var metadata = PluginCache.GetMetadata(_pluginId);
            var isDefault = string.IsNullOrEmpty(metadata.DatabaseType) &&
                            string.IsNullOrEmpty(metadata.ConnectionString);

            EBooleanUtils.AddListItems(DdlIsDefault, "默认数据库连接", "自定义数据库连接");
            ControlUtils.SelectListItemsIgnoreCase(DdlIsDefault, isDefault.ToString());
            PhCustom.Visible = !isDefault;

            DdlSqlDatabaseType.Items.Add(new ListItem
            {
                Text = "SqlServer",
                Value = "SqlServer"
            });
            DdlSqlDatabaseType.Items.Add(new ListItem
            {
                Text = "MySql",
                Value = "MySql"
            });

            EBooleanUtils.AddListItems(DdlIsTrustedConnection, "Windows 身份验证", "用户名密码验证");
            ControlUtils.SelectListItemsIgnoreCase(DdlIsTrustedConnection, false.ToString());

            if (!isDefault)
            {
                var databaseType = metadata.DatabaseType;
                var connectionString = metadata.ConnectionString;
                if (WebConfigUtils.IsProtectData)
                {
                    databaseType = TranslateUtils.DecryptStringBySecretKey(databaseType);
                    connectionString = TranslateUtils.DecryptStringBySecretKey(connectionString);
                }
                ControlUtils.SelectListItemsIgnoreCase(DdlSqlDatabaseType, databaseType);
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var server = string.Empty;
                    var isTrustedConnection = false;
                    var uid = string.Empty;
                    foreach (var str in connectionString.Split(';'))
                    {
                        var arr = str.Split('=');
                        if (arr.Length == 2)
                        {
                            var name = StringUtils.Trim(arr[0]);
                            var value = StringUtils.Trim(arr[1]);
                            if (StringUtils.EqualsIgnoreCase(name, "server"))
                            {
                                server = value;
                            }
                            else if (StringUtils.EqualsIgnoreCase(name, "Trusted_Connection"))
                            {
                                isTrustedConnection = TranslateUtils.ToBool(value);
                            }
                            else if (StringUtils.EqualsIgnoreCase(name, "uid"))
                            {
                                uid = value;
                            }
                        }
                    }

                    TbSqlServer.Text = server;
                    ControlUtils.SelectListItemsIgnoreCase(DdlIsTrustedConnection, isTrustedConnection.ToString());
                    TbSqlUserName.Text = uid;
                }
            }

            DdlIsTrustedConnection_SelectedIndexChanged(null, EventArgs.Empty);
        }

        protected void DdlIsDefault_SelectedIndexChanged(object sender, EventArgs e)
        {
            PhCustom.Visible = !TranslateUtils.ToBool(DdlIsDefault.SelectedValue);
        }

        protected void DdlSqlDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            PhSql1.Visible = true;
            PhSql2.Visible = false;
            DdlSqlDatabaseName.Items.Clear();
        }

        protected void DdlIsTrustedConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            PhSqlUserNamePassword.Visible = !TranslateUtils.ToBool(DdlIsTrustedConnection.SelectedValue);
        }

        protected void Connect_Click(object sender, EventArgs e)
        {
            HihSqlHiddenPassword.Value = TbSqlPassword.Text;
            string errorMessage;
            List<string> databaseNameList;
            var connectionStringWithoutDatabaseName = GetConnectionString(false);
            var isMySql = StringUtils.EqualsIgnoreCase(DdlSqlDatabaseType.SelectedValue, "MySql");
            var isConnectValid = BaiRongDataProvider.DatabaseDao.ConnectToServer(isMySql, connectionStringWithoutDatabaseName, out databaseNameList, out errorMessage);

            if (isConnectValid)
            {
                DdlSqlDatabaseName.Items.Clear();

                foreach (var databaseName in databaseNameList)
                {
                    DdlSqlDatabaseName.Items.Add(databaseName);
                }

                DdlIsDefault.Enabled = PhSql1.Visible = false;
                PhSql2.Visible = true;
            }
            else
            {
                FailMessage(errorMessage);
            }
        }

        private string GetConnectionString(bool isDatabaseName)
        {
            string connectionString = $"server={TbSqlServer.Text};";
            if (TranslateUtils.ToBool(DdlIsTrustedConnection.SelectedValue))
            {
                connectionString += "Trusted_Connection=True;";
            }
            else
            {
                connectionString += $"uid={TbSqlUserName.Text};pwd={HihSqlHiddenPassword.Value};";
            }
            if (isDatabaseName)
            {
                connectionString += $"database={DdlSqlDatabaseName.SelectedValue};";
            }
            return connectionString;
        }

        protected void Submit_Click(object sender, EventArgs e)
        {
            if (!Page.IsPostBack || !Page.IsValid) return;

            var databaseType = string.Empty;
            var connectionString = string.Empty;

            if (!TranslateUtils.ToBool(DdlIsDefault.SelectedValue))
            {
                if (string.IsNullOrEmpty(DdlSqlDatabaseName.SelectedValue))
                {
                    FailMessage("配置失败，需要选择数据库");
                    return;
                }

                databaseType = DdlSqlDatabaseType.SelectedValue;
                connectionString = GetConnectionString(true);
            }

            var metadata = PluginManager.UpdateDatabase(_pluginId, databaseType, connectionString);

            Body.AddAdminLog("配置插件数据库连接", $"插件:{metadata.DisplayName}");
            SuccessMessage("插件配置成功");
        }
    }
}
