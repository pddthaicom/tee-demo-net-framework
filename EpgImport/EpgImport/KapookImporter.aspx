<%@ Page Title="Kapook EPG Impoter" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="KapookImporter.aspx.cs" Inherits="EpgImport.KapookImporter" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

	<div style="text-align: center">
		<ul>
			<li>Sync channel information</li>
			<li>Sync program information</li>
			<li>Sync episode information</li>
			<li>Create epg data</li>
		</ul>
    
		<asp:Button ID="btnImport" runat="server" Text="Import from Kapook" OnClick="btnImport_Click" /><br />
	</div>

	<p>Since:2014-10-13<br />
		Last Modified: 2014-10-13</p>
</asp:Content>
