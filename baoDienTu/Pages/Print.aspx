<%@ Page Title="In bài viết" Language="C#" MasterPageFile="~/MasterPages/PrintMasterPage.master" AutoEventWireup="true" CodeBehind="Print.aspx.cs" Inherits="baoDienTu.Pages.Print" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="PrintContent" runat="server">
    <%= RenderPage() %>
</asp:Content>
