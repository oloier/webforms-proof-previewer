<%@ page title="site active switch" masterpagefile="~/masterpage.master" autoeventwireup="false" codefile="siteactive.aspx.vb" inherits="siteactive"%>

<asp:content contentplaceholderid="pageContent" runat="server">
    <asp:linkbutton id="enable" runat="server" cssclass="button" style="background-color:seagreen">Put site online</asp:linkbutton>
    <asp:linkbutton id="disable" runat="server" cssclass="button" style="background-color:firebrick">Take site offline</asp:linkbutton>
</asp:content>