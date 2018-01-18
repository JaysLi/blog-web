﻿<%@ Page Language="C#" Inherits="SiteServer.BackgroundPages.Settings.PageLogUser" %>
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

  <div class="well well-small">
    <table class="table table-noborder">
      <tr>
        <td>
          时间：从
          <bairong:DateTimeTextBox id="DateFrom" class="input-small" runat="server" />
          &nbsp;到&nbsp;
          <bairong:DateTimeTextBox id="DateTo" class="input-small" runat="server" />
          用户：
          <asp:TextBox ID="UserName" MaxLength="500" size="20" runat="server"/>
          关键字：
          <asp:TextBox id="Keyword" MaxLength="500" Size="37" runat="server"/>
          <asp:Button class="btn" OnClick="Search_OnClick" id="Search" text="搜 索"  runat="server"/>
        </td>
      </tr>
    </table>
  </div>

  <table class="table table-bordered table-hover">
    <tr class="info thead">
      <td width="100">用户</td>
      <td width="100">IP地址</td>
      <td width="150">日期</td>
      <td width="160">动作</td>
      <td>描述</td>
      <td width="20">
        <input onclick="_checkFormAll(this.checked)" type="checkbox" />
      </td>
    </tr>
    <asp:Repeater ID="rptContents" runat="server">
      <itemtemplate>
          <tr>
            <td class="center"><asp:Literal ID="ltlUserName" runat="server"></asp:Literal></td>
            <td class="center"><asp:Literal ID="ltlIPAddress" runat="server"></asp:Literal></td>
            <td class="center"><asp:Literal ID="ltlAddDate" runat="server"></asp:Literal></td>
            <td>
              <asp:Literal ID="ltlAction" runat="server"></asp:Literal>
            </td>
            <td>
              <asp:Literal ID="ltlSummary" runat="server"></asp:Literal></td>
            <td class="center">
              <input type="checkbox" name="IDCollection" value='<%#DataBinder.Eval(Container.DataItem, "ID")%>' />
            </td>
          </tr>
      </itemtemplate>
    </asp:Repeater>
  </table>

  <bairong:sqlPager id="spContents" runat="server" class="table table-pager" />

  <ul class="breadcrumb breadcrumb-button">
    <table width="100%">
      <tr>
        <td>
          <asp:Button class="btn" id="Delete" Text="删 除" runat="server" />
          <asp:Button class="btn" id="DeleteAll" Text="删除全部" runat="server" />
        </td>
        <td align="right"><asp:Literal ID="ltlState" runat="server"></asp:Literal></td>
        <td width="180" align="right"><asp:Button class="btn" ID="Setting" runat="server" /></td>
      </tr>
    </table>
  </ul>

</form>
</body>
</html>
