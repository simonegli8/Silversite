<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Register assembly="SCS.MenuExtender" namespace="SCS.Web.UI.WebControls" tagprefix="cc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>MenuExtender Sample</title>
    <link href="styles/menu-v.css" rel="stylesheet" type="text/css" />
    <link href="styles/menu-h.css" rel="stylesheet" type="text/css" />
    <script src="scripts/jquery-1.5.2.min.js" type="text/javascript"></script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
    
        <div style="height: 60px;">                        
            <ul id="MediaOutline1" runat="server" class="menu">
                <li><a href="/">Television</a>
                    <ul>
                        <li><a href="/">24</a></li>
                        <li><a href="/">Friends</a></li>
                        <li><a href="/">Simpsons</a></li>                    
                        <li><a href="/">Seinfeld</a></li>                    
                    </ul>
                </li>
                <li><a href="/">Movies</a>
                    <ul>
                        <li><a href="/">Scream</a></li>
                        <li><a href="/">Terminator</a>
                            <ul>
                                <li><a href="/">The Terminator</a></li>
                                <li><a href="/">Judgment Day</a></li>
                                <li><a href="/">Rise of the Machines</a></li>
                                <li><a href="/">Terminator Salvation</a></li>                            
                            </ul>    
                        </li>
                        <li><a href="/">The Matrix</a></li>
                    </ul>
                </li>
                <li><a href="/">Books</a>
                    <ul>
                        <li><a href="/">The Firm</a></li>
                        <li><a href="/">Jurassic Park</a></li>
                        <li><a href="/">Life Expectancy</a></li>
                        <li><a href="/">Prince of Tides</a></li>
                    </ul>
                </li>
            </ul>
        </div>
    
        <cc1:MenuExtender ID="MediaOutlineExtender1" runat="server" 
            HideDelaySeconds="1.2" 
            TargetControlID="MediaOutline1"
            MenuCssClass="menu-h" />

        <div style="clear: both;">                        
            <ul id="MediaOutline2" runat="server" class="menu">
                <li><a href="/">Television</a>
                    <ul>
                        <li><a href="/">24</a></li>
                        <li><a href="/">Friends</a></li>
                        <li><a href="/">Simpsons</a></li>                    
                        <li><a href="/">Seinfeld</a></li>                    
                    </ul>
                </li>
                <li><a href="/">Movies</a>
                    <ul>
                        <li><a href="/">Scream</a></li>
                        <li><a href="/">Terminator</a>
                            <ul>
                                <li><a href="/">The Terminator</a></li>
                                <li><a href="/">Judgment Day</a></li>
                                <li><a href="/">Rise of the Machines</a></li>
                                <li><a href="/">Terminator Salvation</a></li>                            
                            </ul>    
                        </li>
                        <li><a href="/">The Matrix</a></li>
                    </ul>
                </li>
                <li><a href="/">Books</a>
                    <ul>
                        <li><a href="/">The Firm</a></li>
                        <li><a href="/">Jurassic Park</a></li>
                        <li><a href="/">Life Expectancy</a></li>
                        <li><a href="/">Prince of Tides</a></li>
                    </ul>
                </li>
            </ul>
        </div>
    
        <cc1:MenuExtender ID="MediaOutlineExtender2" runat="server" 
            HideDelaySeconds="1.2" 
            TargetControlID="MediaOutline2" 
            MenuCssClass="menu-v" />
        
        <div style="clear: both;margin-top: 50px">
            Download more samples and source code from <a href="http://menuextender.codeplex.com" target="_blank">MenuExtender on CodePlex</a>.</div>
    </form>
</body>
</html>
