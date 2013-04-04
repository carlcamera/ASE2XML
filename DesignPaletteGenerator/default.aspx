<%@ Page Language="C#" CodeBehind="default.aspx.cs" Inherits="DesignPaletteGenerator.PaletteGen" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" lang="en" xml:lang="en">
<head runat="server">
      <title>Online ASE File Converter</title>
      <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
      <meta name="description" content="" />
      <meta name="keywords" content="" />
      <link rel="icon" href="favicon.ico" type="image/x-icon" />
      <link rel="shortcut icon" href="favicon.ico" type="image/x-icon" />
      <link rel="stylesheet" type="text/css" media="Screen, projection" href="blueprint/screen.css" />
      <!--[if IE]>
	  <link rel="stylesheet" type="text/css" media="Screen, projection" href="blueprint/ie.css" />
      <![endif]-->
      <link rel="stylesheet" type="text/css" media="Screen, projection" href="screen.css" />
</head>
<body>
    <div id="masthead" class="container">
        <div class="column span-23 last">
            <h1 id="siteheading">ASE to Expression<span class="trademark">®</span> Converter <span>Converts Adobe<span class="trademark">®</span> ASE Palette files to Expression<span class="trademark">®</span> Palette files.</span></h1>
        </div>
        <div class="column span-14 prepend-5 last">
        <p class="warnmsg"><asp:Literal id="usermsg" runat="server"></asp:Literal></p>
        </div>
        <div id="theform" class="column span-23 last">
            <form id="form1" runat="server">
                <div>
                  <label for="uploadbox">Locate your ASE Palette File</label>
                </div>
                <div>
                    <input id="asefile" type="file" runat="server" />
                    <input id="filesubmit" type="button" value="Convert" onserverclick="button_upload" runat="server" />
                </div>
            </form>
        </div>
        <div class="column span-14 prepend-5 last">
          	<p id="partingcomments">
			<em>Adobe</em> is a registered trademark of Adobe Systems Incorporated. <em>Expression</em> is a registered trademark of Microsoft Incorporated.
 			By using this utility you agree to the following terms and conditions: the utility is provided as-is with no warranties nor guarantees. This utility
			is not associated nor endorsed by Adobe Systems, Microsoft, or any of their affiliates.
    		</p>
        </div>
    </div>
</body>
</html>
