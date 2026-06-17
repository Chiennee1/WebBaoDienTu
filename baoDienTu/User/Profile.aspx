<%@ Page Title="Hồ sơ cá nhân" Language="C#" MasterPageFile="~/MasterPages/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="baoDienTu.User.Profile" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <meta name="description" content="Cập nhật thông tin cá nhân: họ tên, số điện thoại và ảnh đại diện." />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <%= RenderPage() %>
</asp:Content>
