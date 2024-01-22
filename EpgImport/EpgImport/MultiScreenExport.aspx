<%@ Page Title="Multi Screen Exporter" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MultiScreenExport.aspx.cs" Inherits="EpgImport.MultiScreenExport" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

	<div style="text-align: center">
		<ul>
			<li>Import channel information</li>
			<li>Sync program information</li>
			<li>Import program information</li>
			<li>Import EPG information (Episode)</li>
			<li>Sync EPG information (Episode)</li>
		</ul>
    
		<asp:Button ID="btnSync" runat="server" Text="Sync EPG Data to Multi Screen Database" OnClick="btnSync_Click" /><br />
	</div>
	<asp:UpdatePanel ID="pnlStatus" runat="server" UpdateMode="Conditional">
		<ContentTemplate>
			<asp:Label ID="lblStatus" runat="server" Text="Progress"></asp:Label>
		</ContentTemplate>
	</asp:UpdatePanel>

	<p>Since:2014-10-28<br />
		Last Modified: 2014-10-28</p>
</asp:Content>
