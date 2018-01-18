using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using BaiRong.Core;
using BaiRong.Core.AuxiliaryTable;
using BaiRong.Core.Model;
using BaiRong.Core.Model.Enumerations;

namespace SiteServer.BackgroundPages.Settings
{
    public class PageTableStyle : BasePage
    {
        public DataGrid DgContents;

        public Button AddStyle;
        public Button AddStyles;
        public Button Import;
        public Button Export;

        private string _tableName;
        private EAuxiliaryTableType _tableType;
        private ETableStyle _tableStyle;
        private List<string> _attributeNames;
        private string _redirectUrl;

        public static string GetRedirectUrl(string tableName, EAuxiliaryTableType tableType)
        {
            return PageUtils.GetSettingsUrl(nameof(PageTableStyle), new NameValueCollection
            {
                {"tableName", tableName},
                {"tableType", EAuxiliaryTableTypeUtils.GetValue(tableType)}
            });
        }

        public string GetReturnUrl()
        {
            return PageAuxiliaryTable.GetRedirectUrl();
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            _tableName = Body.GetQueryString("tableName");
            _tableType = EAuxiliaryTableTypeUtils.GetEnumType(Body.GetQueryString("tableType"));
            _tableStyle = ETableStyleUtils.GetStyleType(_tableType);
            _redirectUrl = GetRedirectUrl(_tableName, _tableType);
            _attributeNames = TableManager.GetAttributeNameList(_tableStyle, _tableName);

            if (IsPostBack) return;

            BreadCrumbSettings($"�����ֶι���{_tableName}��", AppManager.Permissions.Settings.SiteManagement);

            //ɾ����ʽ
            if (Body.IsQueryExists("DeleteStyle"))
            {
                DeleteStyle();
            }
            else if (Body.IsQueryExists("SetTaxis"))
            {
                SetTaxis();
            }

            var styleInfoList = TableStyleManager.GetTableStyleInfoList(_tableStyle, _tableName, new List<int> {0});

            DgContents.DataSource = styleInfoList;
            DgContents.ItemDataBound += dgContents_ItemDataBound;
            DgContents.DataBind();

            AddStyle.Attributes.Add("onclick", ModalTableStyleAdd.GetOpenWindowString(0, _tableName, string.Empty, _tableStyle, _redirectUrl));
            AddStyles.Attributes.Add("onclick", ModalTableStylesAdd.GetOpenWindowString(_tableName, _tableStyle, _redirectUrl));
            Import.Attributes.Add("onclick", ModalTableStyleImport.GetOpenWindowString(_tableName, _tableStyle));
            Export.Attributes.Add("onclick", ModalExportMessage.GetOpenWindowStringToSingleTableStyle(_tableStyle, _tableName));
        }

        private void DeleteStyle()
        {
            var attributeName = Body.GetQueryString("AttributeName");
            if (TableStyleManager.IsExists(0, _tableName, attributeName))
            {
                try
                {
                    TableStyleManager.Delete(0, _tableName, attributeName);
                    Body.AddAdminLog("ɾ�����ݱ���ʽ", $"��:{_tableName},�ֶ�:{attributeName}");
                    SuccessDeleteMessage();
                }
                catch (Exception ex)
                {
                    FailDeleteMessage(ex);
                }
            }
        }

        private void SetTaxis()
        {
            var tableStyleId = Body.GetQueryInt("TableStyleID");
            var styleInfo = BaiRongDataProvider.TableStyleDao.GetTableStyleInfo(tableStyleId);
            if (styleInfo != null && styleInfo.RelatedIdentity == 0)
            {
                var direction = Body.GetQueryString("Direction");

                switch (direction.ToUpper())
                {
                    case "UP":
                        BaiRongDataProvider.TableStyleDao.TaxisDown(tableStyleId);
                        break;
                    case "DOWN":
                        BaiRongDataProvider.TableStyleDao.TaxisUp(tableStyleId);
                        break;
                }
                SuccessMessage("����ɹ���");
            }
            else
            {
                var direction = Body.GetQueryString("Direction");
                var tableMetadataId = Body.GetQueryInt("TableMetadataId");
                switch (direction.ToUpper())
                {
                    case "UP":
                        BaiRongDataProvider.TableMetadataDao.TaxisDown(tableMetadataId, _tableName);
                        break;
                    case "DOWN":
                        BaiRongDataProvider.TableMetadataDao.TaxisUp(tableMetadataId, _tableName);
                        break;
                }
                SuccessMessage("����ɹ���");
            }
        }

        public void Redirect(object sender, EventArgs e)
        {
            PageUtils.Redirect(GetRedirectUrl(_tableName, _tableType));
        }

        void dgContents_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var styleInfo = (TableStyleInfo)e.Item.DataItem;
                if (_attributeNames.Contains(styleInfo.AttributeName))
                {
                    e.Item.Visible = false;
                    return;
                }

                var ltlAttributeName = (Literal)e.Item.FindControl("ltlAttributeName");
                var ltlDisplayName = (Literal)e.Item.FindControl("ltlDisplayName");
                var ltlInputType = (Literal)e.Item.FindControl("ltlInputType");
                var ltlFieldType = (Literal)e.Item.FindControl("ltlFieldType");
                var ltlIsVisible = (Literal)e.Item.FindControl("ltlIsVisible");
                var ltlValidate = (Literal)e.Item.FindControl("ltlValidate");
                var ltlEditStyle = (Literal)e.Item.FindControl("ltlEditStyle");
                var ltlEditValidate = (Literal)e.Item.FindControl("ltlEditValidate");
                var upLinkButton = (HyperLink)e.Item.FindControl("UpLinkButton");
                var downLinkButton = (HyperLink)e.Item.FindControl("DownLinkButton");

                var showPopWinString = ModalTableMetadataView.GetOpenWindowString(_tableType, _tableName, styleInfo.AttributeName);
                ltlAttributeName.Text = $"<a href=\"javascript:void 0;\" onClick=\"{showPopWinString}\">{styleInfo.AttributeName}</a>";

                ltlDisplayName.Text = styleInfo.DisplayName;
                ltlInputType.Text = InputTypeUtils.GetText(InputTypeUtils.GetEnumType(styleInfo.InputType));
                ltlFieldType.Text = TableManager.IsAttributeNameExists(_tableStyle, _tableName, styleInfo.AttributeName) ? $"��ʵ {TableManager.GetTableMetadataDataType(_tableName, styleInfo.AttributeName)}" : "�����ֶ�";

                ltlIsVisible.Text = StringUtils.GetTrueOrFalseImageHtml(styleInfo.IsVisible.ToString());
                ltlValidate.Text = EValidateTypeUtils.GetValidateInfo(styleInfo);

                showPopWinString = ModalTableStyleAdd.GetOpenWindowString(styleInfo.TableStyleId, _tableName, styleInfo.AttributeName, _tableStyle, _redirectUrl);
                var editText = "���";
                if (styleInfo.TableStyleId != 0)//���ݿ�������ʽ
                {
                    editText = "�޸�";
                }
                ltlEditStyle.Text = $"<a href=\"javascript:void 0;\" onClick=\"{showPopWinString}\">{editText}</a>";

                showPopWinString = ModalTableStyleValidateAdd.GetOpenWindowString(styleInfo.TableStyleId, _tableName, styleInfo.AttributeName, _tableStyle, _redirectUrl);
                ltlEditValidate.Text = $"<a href=\"javascript:void 0;\" onClick=\"{showPopWinString}\">����</a>";

                if (styleInfo.TableStyleId != 0)//���ݿ�������ʽ
                {
                    var urlStyle = PageUtils.GetSettingsUrl(nameof(PageTableStyle), new NameValueCollection
                    {
                        {"tableName", _tableName},
                        {"tableType", EAuxiliaryTableTypeUtils.GetValue(_tableType)},
                        {"DeleteStyle", true.ToString()},
                        {"AttributeName", styleInfo.AttributeName}
                    });
                    ltlEditStyle.Text +=
                        $@"&nbsp;&nbsp;<a href=""{urlStyle}"" onClick=""javascript:return confirm('�˲�����ɾ����Ӧ��ʾ��ʽ��ȷ����');"">ɾ��</a>";
                }

                //if (TableStyleManager.IsMetadata(this.tableStyle, styleInfo.AttributeName) || styleInfo.RelatedIdentity != this.nodeInfo.NodeID)
                //{
                //    isTaxisVisible = false;
                //}
                //else
                //{
                var isTaxisVisible = !TableStyleManager.IsExistsInParents(new List<int>{0}, _tableName, styleInfo.AttributeName);
                //}

                if (!isTaxisVisible)
                {
                    upLinkButton.Visible = downLinkButton.Visible = false;
                }
                else
                {
                    var tableMetadataId = BaiRongDataProvider.TableMetadataDao.GetTableMetadataId(styleInfo.TableName, styleInfo.AttributeName);
                    upLinkButton.NavigateUrl = PageUtils.GetSettingsUrl(nameof(PageTableStyle), new NameValueCollection
                    {
                        {"tableName", _tableName},
                        {"tableType", EAuxiliaryTableTypeUtils.GetValue(_tableType)},
                        {"SetTaxis", true.ToString()},
                        {"TableStyleID", styleInfo.TableStyleId.ToString()},
                        {"Direction", "UP"},
                        {"TableMetadataId", tableMetadataId.ToString()}
                    });
                    downLinkButton.NavigateUrl = PageUtils.GetSettingsUrl(nameof(PageTableStyle), new NameValueCollection
                    {
                        {"tableName", _tableName},
                        {"tableType", EAuxiliaryTableTypeUtils.GetValue(_tableType)},
                        {"SetTaxis", true.ToString()},
                        {"TableStyleID", styleInfo.TableStyleId.ToString()},
                        {"Direction", "DOWN"},
                        {"TableMetadataId", tableMetadataId.ToString()}
                    });
                }
            }
        }
    }
}
