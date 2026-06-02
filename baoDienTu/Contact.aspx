<%@ Page Title="Liên hệ" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="baoDienTu.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-shell">
        <div class="container-xl content-layout">
            <section class="article-detail">
                <div class="article-meta"><span>Liên hệ</span></div>
                <h1>Kết nối với tòa soạn</h1>
                <p class="article-summary">Gửi góp ý, đề xuất chủ đề hoặc thông tin hợp tác cho nhóm phát triển Báo Điện Tử.</p>
                <div class="form-panel">
                    <div class="form-grid">
                        <div class="field"><label>Họ tên</label><input placeholder="Nguyễn Văn A" /></div>
                        <div class="field"><label>Email</label><input type="email" placeholder="ban@example.com" /></div>
                        <div class="field full"><label>Nội dung</label><textarea placeholder="Nội dung liên hệ"></textarea></div>
                    </div>
                    <div class="btn-row" style="margin-top:16px">
                        <button class="btn-main" type="button">Gửi thông tin</button>
                    </div>
                </div>
            </section>
            <aside class="panel">
                <h2>Thông tin</h2>
                <p><strong>Email:</strong> contact@baodientu.vn</p>
                <p><strong>Hotline:</strong> 024 0000 0000</p>
                <p class="muted">Form liên hệ là giao diện MVP; tích hợp gửi mail thật sẽ bật ở giai đoạn sau.</p>
            </aside>
        </div>
    </div>
</asp:Content>
