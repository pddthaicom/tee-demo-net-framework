<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="EpgImport.About" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
        <h2>Epg Importer</h2>
    </hgroup>

    <article>
        <p>        
           Import and Sync Program, EPG from Kapook API
        </p>

        <p>        
            Import and Sync Program, EPG from Excel Template File
        </p>

        <p>        
            Export Channel, Program, Episode to Multi Screen Database
        </p>

        <p>        
            Web Service to get EPG Data (Json, Xml)
        </p>
    </article>

    <aside>
        <h3>Site Map</h3>
        <p>        
            Link to Programs
        </p>
        <ul>
            <li><a runat="server" href="~/">Home</a></li>
            <li><a runat="server" href="~/About">About</a></li>
            <li><a runat="server" href="~/Contact">Contact</a></li>
        </ul>
    </aside>
</asp:Content>