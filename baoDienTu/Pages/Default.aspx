<%@ Page Title="Trang chủ" Language="C#" MasterPageFile="~/MasterPages/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="baoDienTu.Pages.DefaultPage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <%= RenderPage() %>
</asp:Content>
