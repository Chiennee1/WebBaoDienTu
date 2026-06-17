<%@ Page Title="Đổi mật khẩu" Language="C#" MasterPageFile="~/MasterPages/Site.Master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="baoDienTu.User.ChangePassword" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <meta name="description" content="Thay đổi mật khẩu tài khoản của bạn một cách an toàn." />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <%= RenderPage() %>
</asp:Content>
