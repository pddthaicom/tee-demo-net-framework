<%@ Page Title="Excel EPG Importer" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ExcelImporter.aspx.cs" Inherits="EpgImport.ExcelImporter" Culture="en-GB" UICulture="en-GB" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

	<div style="text-align: center">
		<ul>
			<li>Inport program</li>
			<li>Create epg data</li>
		</ul>
    
        <p class="validation-summary-errors">
            <asp:Literal runat="server" ID="FailureText" />
        </p>
		<fieldset>
			<legend>Excel Import Form</legend>
            <ol>
                <li>
					<asp:Label runat="server" AssociatedControlID="fileExcel">Excel File</asp:Label>
					<asp:FileUpload ID="fileExcel" runat="server" /><br/>
					<asp:RequiredFieldValidator runat="server" ControlToValidate="fileExcel" CssClass="field-validation-error" ErrorMessage="The excel file is required." />
				</li>
			</ol>
			<asp:Button ID="btnImport" runat="server" Text="Import from Excel" OnClick="btnImport_Click" />
		</fieldset>

		<asp:HyperLink ID="lnkExcel" runat="server" NavigateUrl="~/Upload/Epg Template.xlsx">Download Excel Template</asp:HyperLink> <br/>

		<asp:Label ID="lblProgress" runat="server"></asp:Label>
	</div>
	<p>Since:2014-10-30<br />
		Last Modified: 2014-10-30</p>
</asp:Content>