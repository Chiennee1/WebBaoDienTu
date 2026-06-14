<%@ Page Title="Giới thiệu" Language="C#" MasterPageFile="~/MasterPages/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="baoDienTu.Pages.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-shell">
        <div class="container-xl content-layout">
            <section class="article-detail">
                <div class="article-meta"><span>Báo Điện Tử</span></div>
                <h1>Nền tảng tin tức Web Forms</h1>
                <p class="article-summary">Dự án MVP mô phỏng một tòa soạn điện tử với luồng đọc tin, kiểm duyệt và quản trị nội dung.</p>
                <div class="article-body">
                    <p>Ứng dụng được xây dựng bằng ASP.NET Web Forms, ADO.NET và SQL Server theo hướng 3 lớp: giao diện, nghiệp vụ và truy cập dữ liệu.</p>
                    <p>Admin quản lý toàn bộ hệ thống, Editor viết và gửi duyệt bài, Reader đọc tin và tương tác qua bình luận/newsletter.</p>
                </div>
            </section>
            <aside class="panel">
                <h2>Điểm nổi bật</h2>
                <p class="muted">Giao diện responsive, phân quyền rõ ràng, dữ liệu lấy từ stored procedure và view trong SQL Server.</p>
            </aside>
        </div>
    </div>
</asp:Content>
