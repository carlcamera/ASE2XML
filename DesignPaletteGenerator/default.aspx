<%@ Page Language="C#" CodeBehind="default.aspx.cs" Inherits="DesignPaletteGenerator.PaletteGen" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Online ASE File Converter</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="description" content="" />
    <meta name="keywords" content="" />
    <link rel="icon" href="favicon.ico" type="image/x-icon" />
    <link rel="shortcut icon" href="favicon.ico" type="image/x-icon" />
    <link href="https://fonts.googleapis.com/css?family=Open+Sans:400,600|Khand:400" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" media="Screen, projection" href="screen.css" />
</head>
<body>
    <div class="formwrap">
        <header>
            <h1 id="siteheading">ASE Color Decoder <span>Extracts hex values from Adobe<span class="trademark">®</span> ASE Palette files.</span></h1>
        </header>
        <p class="warnmsg">
            <asp:Literal ID="usermsg" runat="server"></asp:Literal>
        </p>
        <div id="theform">
            <form id="form1" runat="server">
                <div>
                    <label for="uploadbox">Locate your ASE Palette File</label>
                </div>
                <div>
                    <input id="asefile" type="file" runat="server" />
                    <input id="filesubmit" type="button" value="Decode" onserverclick="ButtonUpload" runat="server" />
                </div>
            </form>
        </div>
        <h2><asp:Label runat="server" ID="Pname" /></h2>
        <div class="swatches">
            <asp:Repeater ID="colorlist" runat="server">
                <ItemTemplate>
                    <div class="colorcol">
                        <p class="swatchname">
                            <asp:Label runat="server" ID="Sname" Text='<%# Eval("name") %>' />
                        </p>
                        <span
                            class="sampleswatch"
                            style="background-color: #<%# Eval("hexcolor") %>"></span>
                        <p class="hexval">
                            #<%# Eval("hexcolor") %>
                        </p>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <div id="partingcomments">
            <p>
                <em>Adobe</em> is a registered trademark of Adobe Systems Incorporated. <em>Expression</em> is a registered trademark of Microsoft Incorporated.
 		        By using this utility you agree to the following terms and conditions: the utility is provided as-is with no warranties nor guarantees. This utility
		        is not associated nor endorsed by Adobe Systems, Microsoft, or any of their affiliates.
            </p>
        </div>
    </div>
</body>
</html>
