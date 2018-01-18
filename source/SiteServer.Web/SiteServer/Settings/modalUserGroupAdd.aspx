﻿<%@ Page Language="C#" Inherits="SiteServer.BackgroundPages.Settings.ModalUserGroupAdd" Trace="false"%>
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
<asp:Button id="btnSubmit" useSubmitBehavior="false" OnClick="Submit_OnClick" runat="server" style="display:none" />
<bairong:alerts runat="server"></bairong:alerts>

  <table class="table table-noborder table-hover">
    <tr>
      <td width="160">用户组名称：</td>
      <td>
        <asp:TextBox id="GroupName" MaxLength="50" Size="50" runat="server"/>
        <asp:RequiredFieldValidator
          ControlToValidate="GroupName"
          ErrorMessage=" *" foreColor="red"
          Display="Dynamic"
          runat="server"
          />
      </td>
    </tr>
    <tr>
      <td width="160">用户组说明：</td>
      <td>
        <asp:TextBox TextMode="MultiLine" Columns="50" Rows="3" id="Description" runat="server" />
      </td>
    </tr>
  </table>

</form>
</body>
</html>
