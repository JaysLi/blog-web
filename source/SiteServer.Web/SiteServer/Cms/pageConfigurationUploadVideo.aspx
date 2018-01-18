﻿<%@ Page Language="C#" Inherits="SiteServer.BackgroundPages.Cms.PageConfigurationUploadVideo" %>
<%@ Register TagPrefix="bairong" Namespace="SiteServer.BackgroundPages.Controls" Assembly="SiteServer.BackgroundPages" %>
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<!--#include file="../inc/header.aspx"-->
</head>

<body>
<!--#include file="../inc/openWindow.html"-->
<form class="form-inline" runat="server">
  <asp:Literal id="LtlBreadCrumb" runat="server" />
  <bairong:alerts runat="server" />

  <div class="popover popover-static">
    <h3 class="popover-title">视频上传设置</h3>
    <div class="popover-content">
    
      <table class="table noborder table-hover">
        <tr>
          <td width="220">视频上传文件夹：</td>
          <td>
            <asp:TextBox Columns="25" MaxLength="50" id="tbVideoUploadDirectoryName" runat="server" />
            <asp:RequiredFieldValidator ControlToValidate="tbVideoUploadDirectoryName" errorMessage=" *" foreColor="red" display="Dynamic" runat="server" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="tbVideoUploadDirectoryName" ValidationExpression="[^']+" errorMessage=" *" foreColor="red" display="Dynamic" />
          </td>
        </tr>
        <tr>
          <td>视频上传保存方式：</td>
          <td>
            <asp:RadioButtonList ID="rblVideoUploadDateFormatString" class="noborder" runat="server"></asp:RadioButtonList>
            <span>本设置只影响新上传的视频, 设置更改之前的视频仍存放在原来位置</span>
          </td>
        </tr>
        <tr>
          <td>是否按时间重命名上传的视频：</td>
          <td>
            <asp:RadioButtonList ID="rblIsVideoUploadChangeFileName" class="noborder" runat="server"></asp:RadioButtonList>
            <span>本设置只影响新上传的视频, 设置更改之前的视频名仍保持不变</span>
          </td>
        </tr>
        <tr>
          <td>上传视频类型：</td>
          <td>
            <asp:TextBox TextMode="MultiLine" Width="260px" Height="100" id="tbVideoUploadTypeCollection" runat="server" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="tbVideoUploadTypeCollection"
            ValidationExpression="[^']+" errorMessage=" *" foreColor="red" display="Dynamic" />
            <br>
            <span>类型之间用“,”分割</span>
          </td>
        </tr>
        <tr>
          <td>上传视频最大大小：</td>
          <td>
            <asp:TextBox class="input-mini" Columns="10" MaxLength="50" id="tbVideoUploadTypeMaxSize" runat="server" />
            <asp:DropDownList id="ddlVideoUploadTypeUnit" class="input-small" runat="server">
              <asp:ListItem Value="KB" Text="KB" Selected="true"></asp:ListItem>
              <asp:ListItem Value="MB" Text="MB"></asp:ListItem>
            </asp:DropDownList>
            <asp:RequiredFieldValidator ControlToValidate="tbVideoUploadTypeMaxSize" errorMessage=" *" foreColor="red" display="Dynamic" runat="server" />
            <asp:RegularExpressionValidator
              ControlToValidate="tbVideoUploadTypeMaxSize"
              ValidationExpression="\d+"
              Display="Dynamic"
              ErrorMessage="上传视频最大大小必须为整数"
              foreColor="red"
              runat="server"/>
            </td>
        </tr>
      </table>
  
      <hr />
      <table class="table noborder">
        <tr>
          <td class="center">
            <asp:Button class="btn btn-primary" id="Submit" text="确 定" onclick="Submit_OnClick" runat="server" />
          </td>
        </tr>
      </table>
  
    </div>
  </div>

</form>
</body>
</html>
