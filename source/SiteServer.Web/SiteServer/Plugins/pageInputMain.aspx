﻿<%@ Page Language="C#" Inherits="SiteServer.BackgroundPages.BasePageCms" %>

<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
</head>
<frameset id="frame" framespacing="0" border="false" cols="180,*" frameborder="0" scrolling="yes">
	<frame name="tree" scrolling="auto" marginwidth="0" marginheight="0" src="pageInputTree.aspx?PublishmentSystemID=<%=PublishmentSystemId%>&RightPageURL=<%= BaiRong.Core.StringUtils.ValueToUrl(string.Format("pageInputMainContent.aspx?PublishmentSystemID={0}",PublishmentSystemID)) %> " >
	<frame name="content" scrolling="auto" marginwidth="0" marginheight="0" src="../pageBlank.html">
</frameset>
<noframes>
<body>
<p>This page uses frames, but your browser doesn't support them.</p>
</body>
</noframes>
</html>
