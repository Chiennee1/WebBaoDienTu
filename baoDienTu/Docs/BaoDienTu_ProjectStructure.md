# THIẾT KẾ HỆ THỐNG DỰ ÁN
## Bài Tập Lớn Môn Lập Trình Web — Đề 1: Xây dựng Trang Báo Điện Tử

> **Trường Đại học Mở Hà Nội — Khoa Công nghệ Thông tin**
> Công nghệ: ASP.NET Web Forms (.NET 4.8) | CSDL: MS SQL Server 2019 | Mô hình: 3-Layer Architecture

---

## MỤC LỤC

1. [Tổng quan hệ thống](#1-tổng-quan-hệ-thống)
2. [Công nghệ sử dụng](#2-công-nghệ-sử-dụng)
3. [Kiến trúc 3 lớp](#3-kiến-trúc-3-lớp)
4. [Cấu trúc thư mục dự án](#4-cấu-trúc-thư-mục-dự-án)
5. [Thiết kế Database](#5-thiết-kế-database)
6. [Thiết kế các lớp C#](#6-thiết-kế-các-lớp-c)
7. [Stored Procedures & Views đã triển khai](#7-stored-procedures--views-đã-triển-khai)
8. [Luồng xử lý nghiệp vụ](#8-luồng-xử-lý-nghiệp-vụ)
9. [Giao diện người dùng](#9-giao-diện-người-dùng)
10. [Bảo mật hệ thống](#10-bảo-mật-hệ-thống)
11. [Hướng dẫn triển khai](#11-hướng-dẫn-triển-khai)
12. [Checklist nộp bài](#12-checklist-nộp-bài)

---

## 1. TỔNG QUAN HỆ THỐNG

### 1.1 Mô tả bài toán

Xây dựng hệ thống **Báo Điện Tử** đầy đủ chức năng:

| STT | Chức năng | Mô tả | Vai trò thực hiện |
|-----|-----------|-------|-------------------|
| 1 | Quản lý tin tức | Tin tức phân theo chuyên mục, hỗ trợ danh mục con | Admin / Editor |
| 2 | Soạn thảo rich text | CKEditor 5, upload ảnh, nội dung dài | Editor |
| 3 | Kiểm duyệt bài | Tin chỉ hiển thị sau khi Admin duyệt | Admin |
| 4 | Đọc tin / In tin | Trang in riêng với layout tối giản | Reader / Khách |
| 5 | Gửi tin cho bạn | "Tell your friend" — gửi email kèm tin nhắn | Reader / Khách |
| 6 | Newsletter | Đăng ký → xác nhận email → nhận bản tin | Reader / Khách |
| 7 | Tìm kiếm | Tìm theo từ khóa, lọc theo chuyên mục / tag | Tất cả |
| 8 | Tags / Tin liên quan | Gắn thẻ từ khóa, liên kết bài liên quan | Editor |
| 9 | Bình luận | Bình luận theo dõi phê duyệt (Admin duyệt) | Reader / Khách |
| 10 | Phân quyền | Admin / Editor / Reader, khóa tài khoản khi sai PW | Admin |

### 1.2 Các đối tượng người dùng

```
┌─────────────────────────────────────────────────────────────────┐
│                      HỆ THỐNG BÁO ĐIỆN TỬ                       │
├──────────────────┬──────────────────────┬───────────────────────┤
│      ADMIN       │       EDITOR         │        READER         │
│    (RoleID=1)    │     (RoleID=2)       │      (RoleID=3)       │
├──────────────────┼──────────────────────┼───────────────────────┤
│ Duyệt/từ chối   │ Viết bài mới         │ Đọc tin               │
│ bài viết        │ Sửa bài của mình     │ In tin tức            │
│ Quản lý Users   │ Upload ảnh thumbnail │ Gửi tin cho bạn       │
│ Quản lý chuyên  │ Dùng CKEditor rich   │ Đăng ký Newsletter    │
│ mục & Tags      │ text                 │ Bình luận bài viết    │
│ Gửi Newsletter  │ Gán tags, liên quan  │ Tìm kiếm tin          │
│ Cấu hình hệ     │ Xem thống kê bài     │ Xem theo chuyên mục   │
│ thống           │ của mình             │ Cập nhật hồ sơ        │
│ Xem Dashboard   │                      │                       │
│ thống kê        │                      │                       │
└──────────────────┴──────────────────────┴───────────────────────┘
  + KHÁCH (chưa đăng nhập): Đọc tin, Tìm kiếm, Đăng ký Newsletter,
    Gửi tin cho bạn, Bình luận (nhập tên + email)
```

---

## 2. CÔNG NGHỆ SỬ DỤNG

### 2.1 Stack kỹ thuật

```
┌──────────────────────────────────────────────────────────────┐
│                       CLIENT SIDE                            │
│  HTML5 + CSS3 + Bootstrap 5.3 + JavaScript ES6 + jQuery 3.7 │
│  CKEditor 5 (soạn thảo rich text)                           │
│  AJAX (gọi API không reload trang)                          │
│  DataTables 1.13 (bảng phân trang Admin)                    │
│  SweetAlert2 (hộp thoại thông báo)                          │
│  FontAwesome 6 (bộ icon)                                    │
├──────────────────────────────────────────────────────────────┤
│                       SERVER SIDE                            │
│  ASP.NET Web Forms — .NET Framework 4.8 — C#                │
│  ADO.NET + SqlCommand + Stored Procedures                    │
│  System.Net.Mail (gửi email SMTP)                           │
│  FormsAuthentication (quản lý phiên đăng nhập)             │
│  SHA-256 + Salt (mã hóa mật khẩu)                          │
├──────────────────────────────────────────────────────────────┤
│                       DATABASE                               │
│  Microsoft SQL Server 2019 (Express hoặc cao hơn)           │
│  Collation: Vietnamese_CI_AS                                 │
│  T-SQL: Tables, Indexes, Views, Stored Procedures,          │
│          Functions, Triggers, Seed Data                      │
├──────────────────────────────────────────────────────────────┤
│                      DEPLOYMENT                              │
│  IIS 10.0+ (Internet Information Services)                  │
│  Windows 10/11 hoặc Windows Server 2019+                    │
│  Visual Studio 2022 Community (IDE)                         │
│  SQL Server Management Studio 19+ (SSMS)                    │
└──────────────────────────────────────────────────────────────┘
```

### 2.2 Thư viện & Package

| Thư viện | Phiên bản | Mục đích |
|---------|-----------|---------|
| Bootstrap | 5.3 | Responsive UI framework |
| jQuery | 3.7 | DOM manipulation, AJAX |
| CKEditor | 5 | Rich text editor soạn thảo bài viết |
| FontAwesome | 6 | Bộ biểu tượng icon |
| SweetAlert2 | latest | Hộp thoại xác nhận / thông báo |
| DataTables | 1.13 | Bảng dữ liệu phân trang trang Admin |
| System.Net.Mail | built-in .NET | Gửi email SMTP (Newsletter, Tell friend) |
| RNGCryptoServiceProvider | built-in .NET | Tạo Salt ngẫu nhiên an toàn |
| SHA256 | built-in .NET | Hash mật khẩu |

---

## 3. KIẾN TRÚC 3 LỚP

### 3.1 Sơ đồ tổng quan

```
┌──────────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER — Lớp 1                        │
│                                                                      │
│  ┌─────────────────────────┐    ┌────────────────────────────────┐  │
│  │   Trang công khai       │    │   Khu vực Admin (/Admin/)      │  │
│  │   Default.aspx          │    │   Default.aspx  (Dashboard)    │  │
│  │   NewsList.aspx         │    │   ManageNews.aspx              │  │
│  │   NewsDetail.aspx       │    │   AddEditNews.aspx             │  │
│  │   Search.aspx           │    │   PendingNews.aspx             │  │
│  │   Login.aspx            │    │   ManageCategory.aspx          │  │
│  │   Register.aspx         │    │   ManageUser.aspx              │  │
│  │   NewsletterSub.aspx    │    │   ManageComment.aspx           │  │
│  └──────────┬──────────────┘    │   ManageNewsletter.aspx        │  │
│             │                   │   SendNewsletter.aspx          │  │
│             │   Code-Behind (.aspx.cs) — xử lý sự kiện UI       │  │
│             │   Nhận dữ liệu từ Form → Gọi BLL → Bind kết quả  │  │
└─────────────┼────────────────────────────────────────────────────────┘
              │ Gọi phương thức BLL
┌─────────────▼────────────────────────────────────────────────────────┐
│                  BUSINESS LOGIC LAYER — Lớp 2                        │
│                                                                      │
│  NewsBLL.cs        │ Validate, tạo Slug, quy tắc duyệt bài         │
│  UserBLL.cs        │ Hash PW + Salt, kiểm tra quyền, khóa TK       │
│  CategoryBLL.cs    │ Validate cấu trúc cây chuyên mục              │
│  CommentBLL.cs     │ Kiểm tra AllowComment, chống spam             │
│  NewsletterBLL.cs  │ Tạo token xác nhận / hủy đăng ký             │
│  EmailBLL.cs       │ Build email template, gửi SMTP                │
│  TagBLL.cs         │ Parse chuỗi tag, cập nhật UseCount            │
└─────────────┬────────────────────────────────────────────────────────┘
              │ Gọi phương thức DAL
┌─────────────▼────────────────────────────────────────────────────────┐
│                   DATA ACCESS LAYER — Lớp 3                          │
│                                                                      │
│  DBConnection.cs   │ Tạo SqlConnection từ connectionString          │
│  NewsDAL.cs        │ Gọi sp_GetNewsList, sp_AddNews, sp_ApproveNews │
│  UserDAL.cs        │ Gọi sp_Login, sp_VerifyLogin, sp_RegisterUser  │
│  CategoryDAL.cs    │ Gọi sp_GetCategories, sp_AddCategory           │
│  CommentDAL.cs     │ Gọi sp_AddComment, sp_ApproveComment          │
│  NewsletterDAL.cs  │ Gọi sp_SubscribeNewsletter, sp_GetSubscribers  │
│  TagDAL.cs         │ Gọi sp_AddTagsToNews, sp_GetPopularTags        │
│  ShareLogDAL.cs    │ Gọi sp_AddShareLog                            │
│  SettingDAL.cs     │ Gọi sp_GetSetting, sp_SetSetting              │
└─────────────┬────────────────────────────────────────────────────────┘
              │ ADO.NET — SqlCommand + SqlParameter (chống SQL Injection)
┌─────────────▼────────────────────────────────────────────────────────┐
│                  DATABASE — MS SQL Server 2019                        │
│                                                                      │
│  12 Tables  │ Roles, Users, Categories, News, Tags, News_Tags,      │
│             │ RelatedNews, Comments, Newsletter, Newsletter_Sends,   │
│             │ ShareLog, Settings                                     │
│  8 Views    │ vw_NewsDetail, vw_FeaturedNews, vw_LatestNews,        │
│             │ vw_MostViewedNews, vw_NewsByCategory,                  │
│             │ vw_PendingNews, vw_CommentDetails, vw_AdminDashboard  │
│  28+ SP     │ Nhóm A (News) + B (Users) + C (Categories) +          │
│             │ D (Comments) + E (Newsletter) + F (ShareLog) +         │
│             │ G (Tags) + H (Settings)                               │
│  2 Funcs    │ fn_GenerateSlug, fn_GetCategoryBreadcrumb             │
│  2 Triggers │ trg_News_UpdatedAt, trg_Tags_UseCount                 │
│  9 Indexes  │ Tối ưu truy vấn tin tức, comments, newsletter         │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 4. CẤU TRÚC THƯ MỤC DỰ ÁN

```
BaoDienTu/                                  ← Thư mục gốc Solution
│
├── 📄 BaoDienTu.sln                        ← File Solution Visual Studio
│
├── 📁 App_Code/                            ← Code dùng chung (tự compile)
│   │
│   ├── 📁 DAL/                             ═══ DATA ACCESS LAYER ═══
│   │   ├── DBConnection.cs                 ← Quản lý kết nối SQL Server
│   │   ├── NewsDAL.cs                      ← Thao tác bảng News
│   │   ├── CategoryDAL.cs                  ← Thao tác bảng Categories
│   │   ├── UserDAL.cs                      ← Thao tác bảng Users
│   │   ├── CommentDAL.cs                   ← Thao tác bảng Comments
│   │   ├── NewsletterDAL.cs                ← Thao tác bảng Newsletter
│   │   ├── TagDAL.cs                       ← Thao tác bảng Tags + News_Tags
│   │   ├── ShareLogDAL.cs                  ← Thao tác bảng ShareLog
│   │   └── SettingDAL.cs                   ← Thao tác bảng Settings
│   │
│   ├── 📁 BLL/                             ═══ BUSINESS LOGIC LAYER ═══
│   │   ├── NewsBLL.cs                      ← Nghiệp vụ tin tức
│   │   ├── CategoryBLL.cs                  ← Nghiệp vụ chuyên mục
│   │   ├── UserBLL.cs                      ← Nghiệp vụ người dùng & auth
│   │   ├── CommentBLL.cs                   ← Nghiệp vụ bình luận
│   │   ├── NewsletterBLL.cs                ← Nghiệp vụ newsletter
│   │   ├── EmailBLL.cs                     ← Gửi email SMTP
│   │   ├── TagBLL.cs                       ← Nghiệp vụ tags
│   │   └── SettingBLL.cs                   ← Đọc/ghi cấu hình hệ thống
│   │
│   ├── 📁 Models/                          ═══ ENTITY MODELS (DTO) ═══
│   │   ├── NewsModel.cs                    ← DTO bảng News
│   │   ├── CategoryModel.cs                ← DTO bảng Categories
│   │   ├── UserModel.cs                    ← DTO bảng Users
│   │   ├── CommentModel.cs                 ← DTO bảng Comments
│   │   ├── NewsletterModel.cs              ← DTO bảng Newsletter
│   │   ├── TagModel.cs                     ← DTO bảng Tags
│   │   └── DashboardModel.cs               ← DTO thống kê Admin
│   │
│   └── 📁 Helpers/                         ← Các lớp tiện ích
│       ├── SlugHelper.cs                   ← Tạo slug tiếng Việt
│       ├── SecurityHelper.cs               ← Hash PW, tạo Salt, token GUID
│       ├── FileUploadHelper.cs             ← Upload & validate file ảnh
│       └── PagingHelper.cs                 ← Tính toán phân trang
│
├── 📁 Admin/                               ═══ PRESENTATION — Admin ═══
│   ├── 📄 Web.config                       ← Chặn truy cập (chỉ Admin)
│   ├── AdminMasterPage.master              ← Layout sidebar Admin
│   ├── AdminMasterPage.master.cs
│   ├── Default.aspx                        ← Dashboard: thống kê tổng quan
│   ├── Default.aspx.cs
│   ├── ManageNews.aspx                     ← Danh sách tất cả tin tức
│   ├── ManageNews.aspx.cs
│   ├── AddEditNews.aspx                    ← Thêm / Sửa bài viết
│   ├── AddEditNews.aspx.cs
│   ├── PendingNews.aspx                    ← Danh sách bài chờ duyệt
│   ├── PendingNews.aspx.cs
│   ├── ManageCategory.aspx                 ← Quản lý chuyên mục (cây)
│   ├── ManageCategory.aspx.cs
│   ├── ManageUser.aspx                     ← Quản lý tài khoản
│   ├── ManageUser.aspx.cs
│   ├── ManageComment.aspx                  ← Duyệt bình luận
│   ├── ManageComment.aspx.cs
│   ├── ManageNewsletter.aspx               ← Danh sách đăng ký
│   ├── ManageNewsletter.aspx.cs
│   ├── SendNewsletter.aspx                 ← Soạn & gửi bản tin
│   ├── SendNewsletter.aspx.cs
│   ├── ManageSettings.aspx                 ← Cấu hình hệ thống
│   └── ManageSettings.aspx.cs
│
├── 📁 Editor/                              ═══ PRESENTATION — Editor ═══
│   ├── 📄 Web.config                       ← Chặn: chỉ Admin + Editor
│   ├── MyNews.aspx                         ← Bài viết của tôi
│   ├── MyNews.aspx.cs
│   ├── WriteNews.aspx                      ← Viết bài mới (CKEditor)
│   ├── WriteNews.aspx.cs
│   ├── EditNews.aspx                       ← Sửa bài của mình
│   └── EditNews.aspx.cs
│
├── 📁 User/                                ═══ PRESENTATION — Người dùng ═══
│   ├── Profile.aspx                        ← Trang cá nhân
│   ├── Profile.aspx.cs
│   ├── ChangePassword.aspx                 ← Đổi mật khẩu
│   └── ChangePassword.aspx.cs
│
├── 📁 Pages/                               ═══ PRESENTATION — Trang công khai ═══
│   ├── SiteMasterPage.master               ← Layout Header + Footer + Nav
│   ├── SiteMasterPage.master.cs
│   ├── PrintMasterPage.master              ← Layout trang in (tối giản)
│   ├── Default.aspx                        ← Trang chủ
│   ├── Default.aspx.cs
│   ├── NewsList.aspx                       ← Danh sách tin theo chuyên mục
│   ├── NewsList.aspx.cs
│   ├── NewsDetail.aspx                     ← Chi tiết bài viết
│   ├── NewsDetail.aspx.cs
│   ├── NewsTag.aspx                        ← Tin theo tag
│   ├── NewsTag.aspx.cs
│   ├── Search.aspx                         ← Trang tìm kiếm
│   ├── Search.aspx.cs
│   ├── Login.aspx                          ← Đăng nhập
│   ├── Login.aspx.cs
│   ├── Register.aspx                       ← Đăng ký tài khoản
│   ├── Register.aspx.cs
│   ├── NewsletterSubscribe.aspx            ← Đăng ký nhận bản tin
│   ├── NewsletterSubscribe.aspx.cs
│   ├── NewsletterConfirm.aspx              ← Xác nhận email đăng ký
│   ├── NewsletterConfirm.aspx.cs
│   ├── Unsubscribe.aspx                    ← Hủy đăng ký newsletter
│   ├── Unsubscribe.aspx.cs
│   ├── Print.aspx                          ← Trang in bài viết
│   ├── Print.aspx.cs
│   ├── 404.aspx                            ← Trang lỗi 404
│   └── Error.aspx                          ← Trang lỗi chung
│
├── 📁 Handlers/                            ← HTTP Handlers (xử lý nhanh)
│   ├── ShareNews.ashx                      ← Gửi "Tell your friend"
│   ├── UploadImage.ashx                    ← Upload ảnh từ CKEditor
│   └── IncreaseView.ashx                   ← Tăng lượt xem (AJAX)
│
├── 📁 Database/                            ═══ SCRIPTS SQL ═══
│   └── BaoDienTu_Database.sql              ← ✅ FILE DUY NHẤT — Chứa toàn bộ:
│                                              • DROP + CREATE DATABASE
│                                              • 12 Tables + Constraints
│                                              • 9 Indexes tối ưu
│                                              • 8 Views
│                                              • 2 Functions
│                                              • 28+ Stored Procedures
│                                              • 2 Triggers
│                                              • Seed Data (Roles, Users,
│                                                Categories, Tags, News mẫu,
│                                                Comments, Newsletter, Settings)
│
├── 📁 Static/                              ← Tài nguyên tĩnh
│   ├── 📁 css/
│   │   ├── bootstrap.min.css               ← Bootstrap 5.3
│   │   ├── all.min.css                     ← FontAwesome 6
│   │   ├── site.css                        ← Style chính trang công khai
│   │   ├── admin.css                       ← Style khu vực Admin
│   │   └── print.css                       ← Style trang in
│   ├── 📁 js/
│   │   ├── jquery.min.js                   ← jQuery 3.7
│   │   ├── bootstrap.bundle.min.js         ← Bootstrap JS
│   │   ├── sweetalert2.all.min.js          ← SweetAlert2
│   │   ├── datatables.min.js               ← DataTables
│   │   ├── ckeditor.js                     ← CKEditor 5
│   │   ├── site.js                         ← Script trang công khai
│   │   └── admin.js                        ← Script trang Admin
│   ├── 📁 images/
│   │   ├── logo.png
│   │   ├── favicon.ico
│   │   └── default-thumbnail.jpg           ← Ảnh mặc định khi không có thumbnail
│   └── 📁 uploads/
│       └── 📁 news/                        ← Ảnh thumbnail bài viết (upload)
│
├── 📄 Web.config                           ← Cấu hình chính ứng dụng
├── 📄 Global.asax                          ← Sự kiện Application_Start/End
└── 📄 Global.asax.cs
```

---

## 5. THIẾT KẾ DATABASE

### 5.1 Danh sách đối tượng database (đã tạo trong BaoDienTu_Database.sql)

#### Bảng (Tables) — 12 bảng

| # | Tên bảng | Mô tả | Khóa chính | Ghi chú |
|---|----------|-------|------------|---------|
| 1 | `Roles` | Vai trò người dùng | RoleID | Admin=1, Editor=2, Reader=3 |
| 2 | `Users` | Tài khoản người dùng | UserID | Có Salt, LoginFailCount, LockUntil |
| 3 | `Categories` | Chuyên mục đa cấp | CatID | ParentID tự tham chiếu |
| 4 | `News` | Bài viết / Tin tức | NewsID | Status: 0=Draft,1=Pending,2=Approved,3=Rejected |
| 5 | `Tags` | Thẻ từ khóa | TagID | Có UseCount tự cập nhật qua Trigger |
| 6 | `News_Tags` | Quan hệ News ↔ Tags | (NewsID, TagID) | Nhiều-nhiều, CASCADE DELETE |
| 7 | `RelatedNews` | Tin tức liên quan | (NewsID, RelatedNewsID) | Có SortOrder |
| 8 | `Comments` | Bình luận | CmtID | Hỗ trợ khách (GuestName/Email) và reply |
| 9 | `Newsletter` | Đăng ký bản tin | SubID | ConfirmToken + UnsubscribeToken (GUID) |
| 10 | `Newsletter_Sends` | Lịch sử gửi bản tin | SendID | Lưu nội dung + số lượng gửi |
| 11 | `ShareLog` | Log gửi tin cho bạn | ShareID | CASCADE DELETE khi xóa News |
| 12 | `Settings` | Cấu hình hệ thống | SettingKey | Key-Value, có 15 cấu hình mặc định |

#### Views — 8 views

| # | Tên view | Mô tả |
|---|----------|-------|
| 1 | `vw_NewsDetail` | Tin đã duyệt kèm Author, Category, Breadcrumb |
| 2 | `vw_FeaturedNews` | TOP 6 tin nổi bật (IsFeatured=1) cho trang chủ |
| 3 | `vw_LatestNews` | TOP 10 tin mới nhất |
| 4 | `vw_MostViewedNews` | TOP 10 tin đọc nhiều nhất |
| 5 | `vw_NewsByCategory` | Số lượng tin theo từng chuyên mục |
| 6 | `vw_PendingNews` | Tất cả bài đang chờ duyệt (Status=1) |
| 7 | `vw_CommentDetails` | Bình luận kèm tên hiển thị (User hoặc Guest) |
| 8 | `vw_AdminDashboard` | Thống kê tổng: tin duyệt, chờ duyệt, user, subscriber |

#### Functions — 2 hàm

| # | Tên function | Mô tả |
|---|--------------|-------|
| 1 | `fn_GenerateSlug` | Chuẩn hóa slug (loại ký tự đặc biệt) |
| 2 | `fn_GetCategoryBreadcrumb` | Lấy đường dẫn breadcrumb chuyên mục cha → con |

#### Triggers — 2 trigger

| # | Tên trigger | Sự kiện | Mô tả |
|---|-------------|---------|-------|
| 1 | `trg_News_UpdatedAt` | AFTER UPDATE ON News | Tự cập nhật cột UpdatedAt |
| 2 | `trg_Tags_UseCount` | AFTER INSERT/DELETE ON News_Tags | Tự cập nhật UseCount của Tag |

#### Indexes — 9 index tối ưu

| Index | Bảng | Mục đích |
|-------|------|---------|
| `IX_News_Status_Published` | News | Lấy tin đã duyệt sắp xếp mới nhất |
| `IX_News_CatID` | News | Lọc tin theo chuyên mục |
| `IX_News_AuthorID` | News | Lọc tin theo tác giả |
| `IX_News_Featured` | News | Lọc tin nổi bật (Filtered Index) |
| `IX_News_Hot` | News | Lọc tin hot (Filtered Index) |
| `IX_Comments_NewsID` | Comments | Lấy comment theo bài viết |
| `IX_News_Tags_TagID` | News_Tags | Lọc tin theo tag |
| `IX_Newsletter_IsActive` | Newsletter | Lấy subscriber đang hoạt động |

### 5.2 Sơ đồ quan hệ (ERD tóm tắt)

```
Roles ──────< Users >──────────< News >──────< Categories
                │                  │                │
                │              News_Tags         (ParentID tự tham chiếu)
                │                  │
                │               Tags
                │
              Users >────────< Comments >──── News
                │
              Users >──────── News (ApprovedBy)
                │
              Users >──────── Newsletter_Sends

News >────── RelatedNews ──────< News
News >────── ShareLog
News >────── Comments
Newsletter (độc lập với Users — khách cũng đăng ký được)
Settings (độc lập — key-value store)
```

### 5.3 Cấu trúc bảng quan trọng

#### Bảng `News` — Trung tâm hệ thống

```sql
News (
    NewsID       INT IDENTITY PK,
    Title        NVARCHAR(500)  NOT NULL,
    Slug         VARCHAR(550)   NOT NULL UNIQUE,  -- SEO URL
    Summary      NVARCHAR(1000),                  -- Tóm tắt hiển thị danh sách
    Content      NVARCHAR(MAX)  NOT NULL,          -- Nội dung rich text (CKEditor)
    Thumbnail    VARCHAR(255),                     -- Đường dẫn ảnh đại diện
    AuthorID     INT  → Users.UserID,
    CatID        INT  → Categories.CatID,
    Status       TINYINT: 0=Draft | 1=Pending | 2=Approved | 3=Rejected,
    IsApproved   BIT  DEFAULT 0,
    ApprovedBy   INT  → Users.UserID (NULL nếu chưa duyệt),
    ApprovedAt   DATETIME,
    RejectReason NVARCHAR(500),                   -- Lý do từ chối
    ViewCount    INT  DEFAULT 0,
    AllowComment BIT  DEFAULT 1,
    IsFeatured   BIT  DEFAULT 0,                  -- Tin nổi bật trang chủ
    IsHot        BIT  DEFAULT 0,                  -- Nhãn "Hot"
    CreatedAt    DATETIME DEFAULT GETDATE(),
    UpdatedAt    DATETIME DEFAULT GETDATE(),       -- Tự cập nhật qua Trigger
    PublishedAt  DATETIME                          -- Thời điểm được duyệt lần đầu
)
```

#### Bảng `Users` — Tài khoản & bảo mật

```sql
Users (
    UserID         INT IDENTITY PK,
    Username       VARCHAR(50)   NOT NULL UNIQUE,
    Password       VARCHAR(256)  NOT NULL,   -- SHA-256(password + salt)
    Salt           VARCHAR(100)  NOT NULL,   -- Ngẫu nhiên, khác nhau mỗi user
    Email          VARCHAR(150)  NOT NULL UNIQUE,
    FullName       NVARCHAR(150) NOT NULL,
    RoleID         INT → Roles.RoleID,
    IsActive       BIT DEFAULT 1,
    Avatar         VARCHAR(255),
    Phone          VARCHAR(20),
    LoginFailCount INT DEFAULT 0,            -- Đếm lần đăng nhập sai
    LockUntil      DATETIME,                 -- Khóa TK đến thời điểm này
    CreatedAt      DATETIME DEFAULT GETDATE(),
    LastLogin      DATETIME
)
```

### 5.4 Seed data đã có sẵn sau khi chạy script

| Dữ liệu | Số lượng | Chi tiết |
|---------|----------|---------|
| Roles | 3 | Admin, Editor, Reader |
| Users | 4 | admin, editor01, editor02, reader01 |
| Categories | 18 | 10 chuyên mục gốc + 8 danh mục con |
| Tags | 10 | Các tag phổ biến tiếng Việt |
| News | 5 | Bài viết mẫu đã duyệt, có IsFeatured và IsHot |
| Comments | 5 | Bình luận đã duyệt |
| Newsletter | 4 | 3 đã xác nhận, 1 chưa xác nhận |
| Settings | 15 | Cấu hình đầy đủ SMTP, phân trang, upload |

> ⚠ **Quan trọng:** Password trong seed data là placeholder. Phải chạy đoạn code C# để hash và UPDATE vào DB trước khi dùng (xem Phần 11).

---

## 6. THIẾT KẾ CÁC LỚP C#

### 6.1 Models (DTO)

```csharp
// Models/NewsModel.cs
public class NewsModel
{
    public int      NewsID        { get; set; }
    public string   Title         { get; set; }
    public string   Slug          { get; set; }
    public string   Summary       { get; set; }
    public string   Content       { get; set; }
    public string   Thumbnail     { get; set; }
    public int      AuthorID      { get; set; }
    public string   AuthorName    { get; set; }
    public string   AuthorAvatar  { get; set; }
    public int      CatID         { get; set; }
    public string   CatName       { get; set; }
    public string   CatSlug       { get; set; }
    public string   ParentCatName { get; set; }
    public int      Status        { get; set; }
    public bool     IsApproved    { get; set; }
    public string   RejectReason  { get; set; }
    public int      ViewCount     { get; set; }
    public bool     AllowComment  { get; set; }
    public bool     IsFeatured    { get; set; }
    public bool     IsHot         { get; set; }
    public DateTime CreatedAt     { get; set; }
    public DateTime UpdatedAt     { get; set; }
    public DateTime? PublishedAt  { get; set; }

    // Computed
    public string StatusLabel => Status switch {
        0 => "Bản nháp", 1 => "Chờ duyệt",
        2 => "Đã duyệt",  3 => "Từ chối", _ => ""
    };
    public List<TagModel> Tags         { get; set; } = new();
    public List<NewsModel> RelatedNews { get; set; } = new();
}

// Models/UserModel.cs
public class UserModel
{
    public int      UserID    { get; set; }
    public string   Username  { get; set; }
    public string   Email     { get; set; }
    public string   FullName  { get; set; }
    public int      RoleID    { get; set; }
    public string   RoleName  { get; set; }
    public bool     IsActive  { get; set; }
    public string   Avatar    { get; set; }
    public string   Phone     { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsAdmin  => RoleID == 1;
    public bool IsEditor => RoleID == 1 || RoleID == 2;
}

// Models/CategoryModel.cs
public class CategoryModel
{
    public int      CatID       { get; set; }
    public string   CatName     { get; set; }
    public string   Slug        { get; set; }
    public int?     ParentID    { get; set; }
    public string   ParentName  { get; set; }
    public string   Breadcrumb  { get; set; }
    public int      SortOrder   { get; set; }
    public bool     IsActive    { get; set; }
    public int      NewsCount   { get; set; }
    public List<CategoryModel> Children { get; set; } = new();
}
```

### 6.2 Helpers/SecurityHelper.cs

```csharp
public static class SecurityHelper
{
    // Tạo salt ngẫu nhiên 32 bytes
    public static string GenerateSalt()
    {
        byte[] saltBytes = new byte[32];
        using var rng = new RNGCryptoServiceProvider();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    // Hash mật khẩu: SHA256(password + salt)
    public static string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(
            Encoding.UTF8.GetBytes(password + salt));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    // Tạo token GUID cho xác nhận newsletter
    public static string GenerateToken()
        => Guid.NewGuid().ToString("N");  // 32 ký tự hex

    // Mã hóa HTML để chống XSS
    public static string SafeHtml(string input)
        => HttpUtility.HtmlEncode(input ?? string.Empty);
}
```

### 6.3 Helpers/SlugHelper.cs

```csharp
public static class SlugHelper
{
    private static readonly Dictionary<string, string> _vietMap = new()
    {
        {"à","a"},{"á","a"},{"ả","a"},{"ã","a"},{"ạ","a"},
        {"ă","a"},{"ằ","a"},{"ắ","a"},{"ẳ","a"},{"ẵ","a"},{"ặ","a"},
        {"â","a"},{"ầ","a"},{"ấ","a"},{"ẩ","a"},{"ẫ","a"},{"ậ","a"},
        {"đ","d"},
        {"è","e"},{"é","e"},{"ẻ","e"},{"ẽ","e"},{"ẹ","e"},
        {"ê","e"},{"ề","e"},{"ế","e"},{"ể","e"},{"ễ","e"},{"ệ","e"},
        {"ì","i"},{"í","i"},{"ỉ","i"},{"ĩ","i"},{"ị","i"},
        {"ò","o"},{"ó","o"},{"ỏ","o"},{"õ","o"},{"ọ","o"},
        {"ô","o"},{"ồ","o"},{"ố","o"},{"ổ","o"},{"ỗ","o"},{"ộ","o"},
        {"ơ","o"},{"ờ","o"},{"ớ","o"},{"ở","o"},{"ỡ","o"},{"ợ","o"},
        {"ù","u"},{"ú","u"},{"ủ","u"},{"ũ","u"},{"ụ","u"},
        {"ư","u"},{"ừ","u"},{"ứ","u"},{"ử","u"},{"ữ","u"},{"ự","u"},
        {"ỳ","y"},{"ý","y"},{"ỷ","y"},{"ỹ","y"},{"ỵ","y"}
    };

    public static string Generate(string title, int newsId = 0)
    {
        string s = title.ToLower().Trim();
        foreach (var kv in _vietMap)
            s = s.Replace(kv.Key, kv.Value);
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", "-");
        s = s.Trim('-');
        // Thêm ID để đảm bảo unique
        if (newsId > 0)
            s += "-" + newsId;
        return s;
    }
}
```

### 6.4 DAL/DBConnection.cs

```csharp
public static class DBConnection
{
    private static readonly string _connStr =
        ConfigurationManager.ConnectionStrings["BaoDienTuDB"].ConnectionString;

    public static SqlConnection GetConnection()
        => new SqlConnection(_connStr);

    // Thực thi SP trả về nhiều dòng
    public static SqlDataReader ExecuteReader(
        string spName, params SqlParameter[] parameters)
    {
        var conn = GetConnection();
        conn.Open();
        var cmd = new SqlCommand(spName, conn)
            { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddRange(parameters);
        return cmd.ExecuteReader(CommandBehavior.CloseConnection);
    }

    // Thực thi SP không trả dữ liệu
    public static int ExecuteNonQuery(
        string spName, params SqlParameter[] parameters)
    {
        using var conn = GetConnection();
        conn.Open();
        var cmd = new SqlCommand(spName, conn)
            { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddRange(parameters);
        return cmd.ExecuteNonQuery();
    }

    // Thực thi SP trả về 1 giá trị
    public static object ExecuteScalar(
        string spName, params SqlParameter[] parameters)
    {
        using var conn = GetConnection();
        conn.Open();
        var cmd = new SqlCommand(spName, conn)
            { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddRange(parameters);
        return cmd.ExecuteScalar();
    }
}
```

### 6.5 BLL/UserBLL.cs (trích đoạn quan trọng)

```csharp
public class UserBLL
{
    private readonly UserDAL _dal = new UserDAL();

    public UserModel Login(string username, string password)
    {
        // Bước 1: Lấy Salt của user (gọi sp_Login)
        string salt = _dal.GetSalt(username);
        if (salt == null) return null; // User không tồn tại

        // Bước 2: Hash password với salt
        string hashedPw = SecurityHelper.HashPassword(password, salt);

        // Bước 3: Xác thực (gọi sp_VerifyLogin)
        return _dal.VerifyLogin(username, hashedPw);
        // Trả về null nếu sai, UserModel nếu đúng
    }

    public int Register(string username, string password,
                        string email, string fullName)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(username) || username.Length < 4)
            throw new ArgumentException("Username tối thiểu 4 ký tự.");
        if (!Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d).{8,}$"))
            throw new ArgumentException("Mật khẩu tối thiểu 8 ký tự, có chữ hoa và số.");
        if (!Regex.IsMatch(email, @"^[^@]+@[^@]+\.[^@]+$"))
            throw new ArgumentException("Email không hợp lệ.");

        // Hash password
        string salt     = SecurityHelper.GenerateSalt();
        string hashedPw = SecurityHelper.HashPassword(password, salt);

        return _dal.Register(username, hashedPw, salt, email, fullName);
    }

    public void SetSession(UserModel user, HttpSessionState session)
    {
        session["UserID"]   = user.UserID;
        session["Username"] = user.Username;
        session["FullName"] = user.FullName;
        session["RoleID"]   = user.RoleID;
        session["RoleName"] = user.RoleName;
    }

    public bool IsAdmin(int roleId)  => roleId == 1;
    public bool IsEditor(int roleId) => roleId == 1 || roleId == 2;
}
```

---

## 7. STORED PROCEDURES & VIEWS ĐÃ TRIỂN KHAI

### 7.1 Danh sách Stored Procedures (28 SP)

#### Nhóm A — News (12 SP)

| SP | Mô tả | Parameters chính |
|----|-------|-----------------|
| `sp_GetNewsList` | Danh sách tin phân trang | @CatID, @Page, @PageSize, @Total OUTPUT |
| `sp_GetNewsDetail` | Chi tiết 1 bài theo Slug | @Slug |
| `sp_GetNewsById` | Lấy bài theo ID (kể cả chưa duyệt) | @NewsID |
| `sp_AddNews` | Thêm bài mới | @Title, @Slug, @Content, @AuthorID, @CatID... |
| `sp_UpdateNews` | Cập nhật bài (reset về Pending) | @NewsID, @Title, @Content... |
| `sp_DeleteNews` | Xóa bài (cascade) | @NewsID |
| `sp_ApproveNews` | Duyệt / Từ chối | @NewsID, @AdminID, @IsApproved, @RejectReason |
| `sp_IncreaseViewCount` | Tăng lượt xem +1 | @NewsID |
| `sp_SearchNews` | Tìm kiếm toàn văn | @Keyword, @Page, @PageSize, @Total OUTPUT |
| `sp_GetNewsByTag` | Tin theo Tag slug | @TagSlug, @Page, @PageSize, @Total OUTPUT |
| `sp_GetRelatedNews` | Tin liên quan (thủ công + cùng mục) | @NewsID, @Top |
| `sp_GetAdminNewsList` | Danh sách tin Admin (tất cả status) | @Status, @AuthorID, @CatID, @Keyword... |

#### Nhóm B — Users (7 SP)

| SP | Mô tả |
|----|-------|
| `sp_Login` | Lấy Salt theo Username |
| `sp_VerifyLogin` | Xác thực hash PW, xử lý LockUntil |
| `sp_RegisterUser` | Đăng ký tài khoản, kiểm tra trùng |
| `sp_GetUserById` | Lấy thông tin user kèm RoleName |
| `sp_UpdateUserProfile` | Cập nhật FullName, Phone, Avatar |
| `sp_ChangePassword` | Đổi mật khẩu (kiểm tra PW cũ) |
| `sp_GetAllUsers` | Danh sách user Admin (lọc, phân trang) |

#### Nhóm C — Categories (3 SP)

| SP | Mô tả |
|----|-------|
| `sp_GetCategories` | Tất cả chuyên mục kèm NewsCount, Breadcrumb |
| `sp_AddCategory` | Thêm chuyên mục, kiểm tra Slug trùng |
| `sp_UpdateCategory` | Cập nhật, kiểm tra không tự làm cha của mình |

#### Nhóm D — Comments (3 SP)

| SP | Mô tả |
|----|-------|
| `sp_GetCommentsByNews` | Bình luận đã duyệt theo bài viết |
| `sp_AddComment` | Thêm bình luận (kiểm tra AllowComment) |
| `sp_ApproveComment` | Duyệt / ẩn bình luận |

#### Nhóm E — Newsletter (4 SP)

| SP | Mô tả |
|----|-------|
| `sp_SubscribeNewsletter` | Đăng ký, tạo ConfirmToken + UnsubToken |
| `sp_ConfirmNewsletter` | Xác nhận qua token email |
| `sp_UnsubscribeNewsletter` | Hủy đăng ký qua UnsubToken |
| `sp_GetActiveSubscribers` | Danh sách subscriber để gửi mail |

#### Nhóm F — ShareLog (1 SP)

| SP | Mô tả |
|----|-------|
| `sp_AddShareLog` | Lưu log gửi tin cho bạn |

#### Nhóm G — Tags (3 SP)

| SP | Mô tả |
|----|-------|
| `sp_AddTagsToNews` | Gán tags vào bài (parse chuỗi phân cách bằng dấu phẩy) |
| `sp_GetTagsByNews` | Lấy danh sách tag của bài viết |
| `sp_GetPopularTags` | TOP N tag phổ biến nhất |

#### Nhóm H — Settings (2 SP)

| SP | Mô tả |
|----|-------|
| `sp_GetSetting` | Lấy giá trị theo key |
| `sp_SetSetting` | Upsert (INSERT hoặc UPDATE) giá trị |

---

## 8. LUỒNG XỬ LÝ NGHIỆP VỤ

### 8.1 Luồng đăng bài → duyệt → xuất bản

```
[EDITOR]                         [ADMIN]                    [READER]
   │                                │                           │
   ▼                                │                           │
Soạn bài (WriteNews.aspx)          │                           │
CKEditor + Upload ảnh               │                           │
   │                                │                           │
   ▼                                │                           │
Submit → NewsBLL.AddNews()          │                           │
  • Validate tiêu đề, nội dung      │                           │
  • SlugHelper.Generate(title)      │                           │
  • Status = 1 (Pending)            │                           │
  • sp_AddNews → DB                 │                           │
   │                                │                           │
   ├──── Email thông báo ──────────▶│                           │
   │                                ▼                           │
   │                     PendingNews.aspx                       │
   │                     Xem trước bài viết                     │
   │                                │                           │
   │                    ┌───────────┴──────────┐                │
   │                    │                      │                │
   │                 Duyệt ✅              Từ chối ❌            │
   │                    │                      │                │
   │                    ▼                      ▼                │
   │           sp_ApproveNews          sp_ApproveNews           │
   │           IsApproved=1            IsApproved=0             │
   │           Status=2                Status=3                 │
   │           PublishedAt=NOW()       RejectReason=...         │
   │                    │                      │                │
   │◀── Email kết quả ──┘                      │                │
   │                                           │                │
   │                                           │                │
   │              Bài xuất hiện                │                │
   │              trên website ────────────────────────────────▶│
   │                                           │             Đọc tin
```

### 8.2 Luồng đọc tin → In / Chia sẻ

```
Reader truy cập trang chủ (Default.aspx)
   │
   ├── Slider: vw_FeaturedNews (IsFeatured=1)
   ├── Tin mới: vw_LatestNews
   └── Tin theo chuyên mục
          │
          ▼
   Chọn bài → NewsDetail.aspx?slug=...
          │
          ├── sp_GetNewsDetail(@Slug)
          ├── sp_IncreaseViewCount(@NewsID)  ← AJAX call
          ├── sp_GetRelatedNews(@NewsID)
          └── sp_GetCommentsByNews(@NewsID)
                │
         ┌──────┼──────────────────┐
         │      │                  │
         ▼      ▼                  ▼
      Bình   In tin            Gửi cho bạn
      luận   Print.aspx        (Modal popup)
         │      │                  │
         ▼      ▼                  ▼
   sp_Add   PrintMaster       ShareNews.ashx
   Comment  .master           → EmailBLL
            (CSS print)         .SendTellFriend()
                                → sp_AddShareLog
```

### 8.3 Luồng Newsletter

```
Khách nhập email vào form "Đăng ký bản tin"
   │
   ▼
NewsletterSubscribe.aspx → NewsletterBLL.Subscribe()
   │
   ├── sp_SubscribeNewsletter(email, fullName, confirmToken, unsubToken)
   │      ├── Nếu đã đăng ký & xác nhận → thông báo "Đã đăng ký rồi"
   │      └── Nếu chưa → INSERT vào DB, IsActive=0, IsConfirmed=0
   │
   ▼
EmailBLL.SendConfirmEmail(email, confirmToken)
   │
   └── Link: /Pages/NewsletterConfirm.aspx?token=xxxxx
              │
              ▼
         sp_ConfirmNewsletter(@Token)
         IsActive=1, IsConfirmed=1, ConfirmedAt=NOW()
              │
              ▼
         "Đăng ký thành công! Bạn sẽ nhận bản tin sớm."

[ADMIN] SendNewsletter.aspx
   │
   ├── Soạn Subject + HtmlContent (CKEditor)
   ├── sp_GetActiveSubscribers → danh sách email
   └── EmailBLL.SendNewsletter() → vòng lặp gửi từng subscriber
         │
         ├── Mỗi email nhúng link: /Pages/Unsubscribe.aspx?token=unsubToken
         └── sp_AddNewsletterSend(subject, content, adminId, totalSent)
```

---

## 9. GIAO DIỆN NGƯỜI DÙNG

### 9.1 Sitemap toàn bộ trang web

```
BÁO ĐIỆN TỬ — SITEMAP
│
├── 🏠  /                          Trang chủ
│        Slider tin nổi bật (IsFeatured)
│        Tin mới nhất (vw_LatestNews)
│        Tin theo từng chuyên mục
│        Form đăng ký Newsletter
│
├── 📰  /Pages/NewsList.aspx?cat={slug}     Danh sách tin theo chuyên mục
│        Phân trang 10 tin/trang
│        Breadcrumb: Trang chủ > Chuyên mục > Chuyên mục con
│
├── 📄  /Pages/NewsDetail.aspx?slug={slug}  Chi tiết bài viết
│        Nội dung đầy đủ (rich text)
│        Tags của bài
│        Tin liên quan
│        Bình luận (nếu AllowComment=1)
│        Button: [🖨 In bài]  [📧 Gửi cho bạn]
│
├── 🏷️  /Pages/NewsTag.aspx?tag={slug}      Tin theo tag
│
├── 🔍  /Pages/Search.aspx?q={keyword}      Tìm kiếm
│
├── 🖨️  /Pages/Print.aspx?id={newsId}       Trang in (PrintMasterPage)
│
├── 👤  Tài khoản
│   ├── /Pages/Login.aspx                  Đăng nhập
│   ├── /Pages/Register.aspx               Đăng ký
│   ├── /User/Profile.aspx                 Hồ sơ cá nhân
│   └── /User/ChangePassword.aspx          Đổi mật khẩu
│
├── 📧  Newsletter
│   ├── /Pages/NewsletterSubscribe.aspx    Đăng ký nhận bản tin
│   ├── /Pages/NewsletterConfirm.aspx      Xác nhận email
│   └── /Pages/Unsubscribe.aspx            Hủy đăng ký
│
├── ✏️  /Editor/                            Khu vực Biên tập viên
│   ├── MyNews.aspx                        Bài viết của tôi
│   ├── WriteNews.aspx                     Viết bài mới
│   └── EditNews.aspx                      Sửa bài
│
└── 🔐  /Admin/                            Khu vực Quản trị
    ├── Default.aspx                       Dashboard thống kê
    ├── PendingNews.aspx                   Duyệt bài chờ
    ├── ManageNews.aspx                    Tất cả tin tức
    ├── AddEditNews.aspx                   Thêm/Sửa bài
    ├── ManageCategory.aspx                Quản lý chuyên mục
    ├── ManageUser.aspx                    Quản lý tài khoản
    ├── ManageComment.aspx                 Duyệt bình luận
    ├── ManageNewsletter.aspx              Danh sách đăng ký
    ├── SendNewsletter.aspx                Gửi bản tin
    └── ManageSettings.aspx                Cấu hình hệ thống
```

### 9.2 Wireframe trang chủ (Default.aspx)

```
┌─────────────────────────────────────────────────────────────────┐
│  🗞 LOGO BÁO ĐIỆN TỬ      [Tìm kiếm...]        [Đăng nhập]    │
├─────┬──────┬──────┬───────┬──────┬────────┬──────┬─────────────┤
│Chính│Kinh  │Xã   │Thế   │Thể  │Giải   │Công  │ Du lịch     │
│ trị │ tế   │ hội  │ giới  │ thao │ trí    │ nghệ │             │
├─────┴──────┴──────┴───────┴──────┴────────┴──────┴─────────────┤
│                                                                  │
│  ┌─────────────────────────────┐  ┌──────────────────────────┐ │
│  │                             │  │  TIN MỚI NHẤT             │ │
│  │   SLIDER TIN NỔI BẬT        │  ├──────────────────────────┤ │
│  │   (IsFeatured = 1)          │  │ ■ [ảnh] Tiêu đề bài 1    │ │
│  │   Tự động chuyển 5 giây     │  │         12:30 | 1.2k views│ │
│  │   ◀  ●●○○○  ▶              │  │ ■ [ảnh] Tiêu đề bài 2    │ │
│  │                             │  │ ■ [ảnh] Tiêu đề bài 3    │ │
│  └─────────────────────────────┘  │ ■ [ảnh] Tiêu đề bài 4    │ │
│                                   │ ■ [ảnh] Tiêu đề bài 5    │ │
│                                   └──────────────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│  KINH TẾ                                         [Xem thêm →]  │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐          │
│  │ [ảnh]   │  │ [ảnh]   │  │ [ảnh]   │  │ [ảnh]   │          │
│  │ Tiêu đề │  │ Tiêu đề │  │ Tiêu đề │  │ Tiêu đề │          │
│  │ tóm tắt │  │ tóm tắt │  │ tóm tắt │  │ tóm tắt │          │
│  │ 2 dòng  │  │ 2 dòng  │  │ 2 dòng  │  │ 2 dòng  │          │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘          │
├─────────────────────────────────────────────────────────────────┤
│  📧 ĐĂNG KÝ NHẬN BẢN TIN                                       │
│  Nhận tin tức mới nhất mỗi ngày ngay trong hộp thư của bạn    │
│  [     Họ và tên     ] [      Email của bạn      ] [ĐĂNG KÝ]  │
├─────────────────────────────────────────────────────────────────┤
│  FOOTER: © 2024 Báo Điện Tử | Liên hệ | Về chúng tôi | RSS  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 10. BẢO MẬT HỆ THỐNG

### 10.1 Phân quyền theo thư mục

```xml
<!-- /Admin/Web.config — Chỉ Admin truy cập -->
<configuration>
  <system.web>
    <authorization>
      <allow roles="Admin"/>
      <deny users="*"/>
    </authorization>
  </system.web>
</configuration>

<!-- /Editor/Web.config — Admin và Editor truy cập -->
<configuration>
  <system.web>
    <authorization>
      <allow roles="Admin,Editor"/>
      <deny users="*"/>
    </authorization>
  </system.web>
</configuration>

<!-- Web.config gốc — Cấu hình FormsAuthentication -->
<system.web>
  <authentication mode="Forms">
    <forms loginUrl="~/Pages/Login.aspx" timeout="30"
           cookieless="UseCookies" slidingExpiration="true"/>
  </authentication>
</system.web>
```

### 10.2 Bảng biện pháp bảo mật

| Nguy cơ | Biện pháp áp dụng | Nơi thực hiện |
|---------|------------------|---------------|
| SQL Injection | SqlParameter + Stored Procedures — KHÔNG nối chuỗi SQL | Toàn bộ DAL |
| XSS | `HtmlEncode()` tất cả output; `ValidateRequest="true"` | Code-behind + ASPX |
| CSRF | ViewState ASP.NET built-in | Tự động |
| Brute-force | Khóa TK 15 phút sau 5 lần sai (`sp_VerifyLogin`) | DB + UserBLL |
| Mật khẩu yếu | Validate regex (min 8 ký tự, có HOA + số) | UserBLL.Register() |
| Password lưu DB | SHA-256 hash + Salt ngẫu nhiên riêng mỗi user | SecurityHelper |
| Upload độc hại | Chỉ jpg/png/gif/webp, kiểm tra extension + magic bytes, đổi tên GUID | FileUploadHelper |
| Directory Traversal | Lưu file upload ra ngoài webroot, validate path | FileUploadHelper |
| Session Hijacking | Session timeout 30 phút, tái tạo Session ID sau login | Global.asax |
| Truy cập trái phép | Kiểm tra Session trong Page_Load mỗi trang cần auth | Code-behind |

### 10.3 Code mẫu bảo mật

```csharp
// SecurityHelper.cs
public static string HashPassword(string password, string salt)
{
    using var sha256 = SHA256.Create();
    byte[] bytes = sha256.ComputeHash(
        Encoding.UTF8.GetBytes(password + salt));
    return BitConverter.ToString(bytes).Replace("-", "").ToLower();
}

public static string GenerateSalt()
{
    byte[] saltBytes = new byte[32];
    using var rng = new RNGCryptoServiceProvider();
    rng.GetBytes(saltBytes);
    return Convert.ToBase64String(saltBytes);
}

// FileUploadHelper.cs — Kiểm tra file upload
public static bool IsValidImage(HttpPostedFile file, out string error)
{
    error = string.Empty;
    var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    string ext = Path.GetExtension(file.FileName).ToLower();

    if (!allowedExt.Contains(ext))
    { error = "Chỉ cho phép ảnh JPG, PNG, GIF, WEBP."; return false; }

    if (file.ContentLength > 5 * 1024 * 1024)
    { error = "Ảnh không được vượt quá 5MB."; return false; }

    // Kiểm tra magic bytes (header file)
    byte[] header = new byte[4];
    file.InputStream.Read(header, 0, 4);
    file.InputStream.Seek(0, SeekOrigin.Begin);

    bool isJpg  = header[0]==0xFF && header[1]==0xD8;
    bool isPng  = header[0]==0x89 && header[1]==0x50;
    bool isGif  = header[0]==0x47 && header[1]==0x49;
    bool isWebp = header[0]==0x52 && header[1]==0x49; // RIFF

    if (!isJpg && !isPng && !isGif && !isWebp)
    { error = "File không phải định dạng ảnh hợp lệ."; return false; }

    return true;
}

public static string SaveImage(HttpPostedFile file, string uploadFolder)
{
    string fileName = Guid.NewGuid().ToString("N")
                    + Path.GetExtension(file.FileName).ToLower();
    string savePath = Path.Combine(uploadFolder, fileName);
    file.SaveAs(savePath);
    return "/Static/uploads/news/" + fileName;
}
```

---

## 11. HƯỚNG DẪN TRIỂN KHAI

### 11.1 Yêu cầu môi trường

| Thành phần | Yêu cầu | Tải về |
|------------|---------|--------|
| OS | Windows 10/11 hoặc Windows Server 2019+ | — |
| .NET Framework | 4.8 | microsoft.com |
| Visual Studio | 2022 Community (miễn phí) | visualstudio.microsoft.com |
| SQL Server | 2019 Express (miễn phí) | microsoft.com/sql-server |
| SSMS | 19+ | aka.ms/ssmsfullsetup |
| IIS | 10.0+ (tích hợp Windows) | Bật trong Windows Features |

### 11.2 Các bước triển khai chi tiết

```
════════════════════════════════════════════════════════
  BƯỚC 1 — Cài đặt môi trường
════════════════════════════════════════════════════════
1. Cài Visual Studio 2022 Community
   Workload cần chọn: "ASP.NET and web development"

2. Cài SQL Server 2019 Express
   Chọn "Basic" installation
   Ghi nhớ tên instance (thường: localhost\SQLEXPRESS)

3. Cài SSMS 19+

4. Bật IIS:
   Control Panel → Programs → Windows Features
   ✅ Internet Information Services
      ✅ Web Management Tools → IIS Management Console
      ✅ World Wide Web Services
         ✅ Application Development Features
            ✅ ASP.NET 4.8
         ✅ Common HTTP Features (tất cả)

════════════════════════════════════════════════════════
  BƯỚC 2 — Tạo Database (1 file duy nhất)
════════════════════════════════════════════════════════
1. Mở SSMS, kết nối: localhost\SQLEXPRESS
2. File → Open → Database/BaoDienTu_Database.sql
3. Nhấn F5 (Execute)
   Script sẽ tự động:
   ✅ Xóa DB cũ nếu có
   ✅ Tạo BaoDienTuDB với collation Vietnamese_CI_AS
   ✅ Tạo 12 bảng + Constraints
   ✅ Tạo 9 Indexes
   ✅ Tạo 8 Views
   ✅ Tạo 2 Functions
   ✅ Tạo 28+ Stored Procedures
   ✅ Tạo 2 Triggers
   ✅ Insert Seed Data (Roles, Users, Categories, Tags,
      5 bài News mẫu, Comments, Newsletter, Settings)

════════════════════════════════════════════════════════
  BƯỚC 3 — Cập nhật mật khẩu tài khoản seed data
════════════════════════════════════════════════════════
Password trong seed data là placeholder, cần chạy
script C# sau để hash và cập nhật vào DB:

Tạo file InitPasswords.cs (chạy 1 lần):

    string[] users = { "admin", "editor01", "editor02", "reader01" };
    string[] passwords = { "Admin@123", "Editor@123",
                           "Editor@123", "Reader@123" };

    for (int i = 0; i < users.Length; i++)
    {
        string salt = SecurityHelper.GenerateSalt();
        string hash = SecurityHelper.HashPassword(passwords[i], salt);

        using var conn = DBConnection.GetConnection();
        conn.Open();
        var cmd = new SqlCommand(
            "UPDATE Users SET Password=@pw, Salt=@salt WHERE Username=@un",
            conn);
        cmd.Parameters.AddWithValue("@pw",   hash);
        cmd.Parameters.AddWithValue("@salt", salt);
        cmd.Parameters.AddWithValue("@un",   users[i]);
        cmd.ExecuteNonQuery();
    }

════════════════════════════════════════════════════════
  BƯỚC 4 — Cấu hình Web.config
════════════════════════════════════════════════════════
Mở Web.config, điền thông tin thực tế:

<connectionStrings>
  <add name="BaoDienTuDB"
       connectionString="Data Source=localhost\SQLEXPRESS;
                         Initial Catalog=BaoDienTuDB;
                         Integrated Security=True;"
       providerName="System.Data.SqlClient"/>
</connectionStrings>

Trong bảng Settings (đã có sẵn), cập nhật:
  SMTP_User → email Gmail của bạn
  SMTP_Pass → App Password (16 ký tự từ Google Account)

Cách tạo App Password Gmail:
  myaccount.google.com → Security → 2-Step Verification
  → App passwords → Tạo mới → Copy 16 ký tự

════════════════════════════════════════════════════════
  BƯỚC 5 — Mở và Build dự án
════════════════════════════════════════════════════════
1. Mở Visual Studio 2022
2. File → Open → Project/Solution → BaoDienTu.sln
3. Build → Build Solution (Ctrl+Shift+B)
4. Kiểm tra: không có lỗi đỏ trong Output window

════════════════════════════════════════════════════════
  BƯỚC 6 — Chạy dự án
════════════════════════════════════════════════════════
Cách A — IIS Express (dễ nhất, dùng khi debug):
  F5 hoặc Ctrl+F5 trong Visual Studio
  Mở tại: http://localhost:{port}/

Cách B — IIS Local (giống môi trường thực):
  1. Mở IIS Manager
  2. Sites → Add Website
     Physical path: thư mục dự án
     Port: 8080
  3. Application Pool → .NET 4.0 + Integrated
  4. Mở: http://localhost:8080/

════════════════════════════════════════════════════════
  BƯỚC 7 — Kiểm tra sau khi chạy
════════════════════════════════════════════════════════
□ Trang chủ hiển thị 5 bài tin mẫu
□ Đăng nhập admin/Admin@123 → vào được /Admin/
□ Đăng nhập editor01/Editor@123 → vào được /Editor/
□ Đăng nhập reader01/Reader@123 → không vào được Admin
□ Xem chi tiết bài viết, lượt xem tăng
□ Gửi bình luận (hiển thị "chờ duyệt")
□ Form "Gửi cho bạn" gửi được email
□ Đăng ký Newsletter nhận được email xác nhận
□ Admin duyệt bình luận → hiển thị
□ Editor viết bài → Admin duyệt → xuất hiện trang chủ
```

### 11.3 File Web.config hoàn chỉnh

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <connectionStrings>
    <add name="BaoDienTuDB"
         connectionString="Data Source=localhost\SQLEXPRESS;
                           Initial Catalog=BaoDienTuDB;
                           Integrated Security=True;
                           MultipleActiveResultSets=True;"
         providerName="System.Data.SqlClient"/>
  </connectionStrings>

  <appSettings>
    <add key="SiteName"        value="Báo Điện Tử"/>
    <add key="SiteUrl"         value="http://localhost:8080"/>
    <add key="UploadPath"      value="~/Static/uploads/news/"/>
    <add key="MaxFileSizeMB"   value="5"/>
  </appSettings>

  <system.web>
    <compilation debug="true" targetFramework="4.8"/>
    <httpRuntime targetFramework="4.8"
                 maxRequestLength="10240"
                 executionTimeout="120"/>
    <globalization culture="vi-VN" uiCulture="vi-VN"
                   requestEncoding="utf-8"
                   responseEncoding="utf-8"/>
    <authentication mode="Forms">
      <forms loginUrl="~/Pages/Login.aspx"
             timeout="30"
             cookieless="UseCookies"
             slidingExpiration="true"
             name=".BaoDienTuAuth"/>
    </authentication>
    <authorization>
      <allow users="*"/>
    </authorization>
    <sessionState mode="InProc" timeout="30"/>
    <pages validateRequest="false" enableViewState="true">
      <!-- validateRequest=false vì CKEditor gửi HTML -->
    </pages>
    <customErrors mode="On" defaultRedirect="~/Pages/Error.aspx">
      <error statusCode="404" redirect="~/Pages/404.aspx"/>
    </customErrors>
  </system.web>

  <system.webServer>
    <defaultDocument>
      <files><add value="Default.aspx"/></files>
    </defaultDocument>
    <staticContent>
      <mimeMap fileExtension=".webp" mimeType="image/webp"/>
    </staticContent>
  </system.webServer>
</configuration>
```

### 11.4 Tài khoản mặc định (sau Bước 3)

| Vai trò | Username | Password | Truy cập |
|---------|----------|----------|---------|
| Admin | `admin` | `Admin@123` | Toàn bộ hệ thống, kể cả /Admin/ |
| Editor | `editor01` | `Editor@123` | /Editor/ + trang công khai |
| Editor | `editor02` | `Editor@123` | /Editor/ + trang công khai |
| Reader | `reader01` | `Reader@123` | Trang công khai + /User/ |

> ⚠ **Đổi mật khẩu admin ngay sau khi triển khai thực tế!**

---

## 12. CHECKLIST NỘP BÀI

### 12.1 Checklist Database

```
✅ File BaoDienTu_Database.sql chạy thành công không lỗi
✅ 12 bảng được tạo đầy đủ với Constraints
✅ 8 Views hoạt động (SELECT * FROM vw_NewsDetail có kết quả)
✅ 28+ Stored Procedures đã tạo
✅ 2 Triggers hoạt động (sửa News → UpdatedAt tự cập nhật)
✅ Seed data: Roles, Users, Categories, Tags, News, Comments, Newsletter
✅ Mật khẩu đã được hash (không còn "placeholder_hash_...")
```

### 12.2 Checklist Chức năng

```
✅ Trang chủ hiển thị tin nổi bật (slider) + tin mới nhất
✅ Xem tin tức theo chuyên mục (phân trang)
✅ Xem chi tiết bài viết đầy đủ
✅ Lượt xem tăng mỗi khi vào trang chi tiết
✅ In tin tức (trang in riêng, layout tối giản)
✅ Chức năng "Gửi cho bạn" gửi được email
✅ Tìm kiếm tin tức theo từ khóa
✅ Xem tin theo Tag
✅ Bình luận (khách + tài khoản đã đăng nhập)
✅ Đăng ký Newsletter → nhận email xác nhận → xác nhận OK
✅ Hủy đăng ký Newsletter qua link trong email
✅ Đăng ký tài khoản Reader
✅ Đăng nhập / Đăng xuất
✅ Phân quyền: Reader không vào được /Admin/
✅ Editor: viết bài mới bằng CKEditor, upload ảnh thumbnail
✅ Editor: sửa bài → tự động trở về Pending
✅ Admin: xem Dashboard thống kê (tin, user, subscriber)
✅ Admin: duyệt bài / từ chối (nhập lý do)
✅ Admin: quản lý chuyên mục (thêm/sửa/ẩn)
✅ Admin: quản lý người dùng (kích hoạt/vô hiệu)
✅ Admin: duyệt bình luận
✅ Admin: gửi Newsletter đến tất cả subscriber đã xác nhận
✅ Admin: cấu hình hệ thống (Settings)
```

### 12.3 Checklist Kỹ thuật

```
✅ Mô hình 3 lớp rõ ràng: DAL / BLL / Presentation
✅ Toàn bộ truy vấn DB dùng SqlParameter (không nối chuỗi SQL)
✅ Mật khẩu lưu dưới dạng SHA-256 + Salt (không plaintext)
✅ Giao diện responsive (Bootstrap 5) trên mobile
✅ URL thân thiện: ?slug=ten-bai-viet
✅ Xử lý lỗi 404, lỗi chung (customErrors)
✅ Upload ảnh: kiểm tra extension + magic bytes + giới hạn 5MB
✅ Session timeout 30 phút
✅ Validate input phía server (không chỉ client)
```

### 12.4 Checklist Nộp bài

```
✅ Quyển báo cáo PDF (đúng cấu trúc tại https://bit.ly/3h8itvt)
✅ Trình bày theo qui định tại mẫu QuidinhTrinhbayBaocao_v4_5_2_1.pdf
✅ File ZIP chứa:
   ├── Source code đầy đủ (BaoDienTu/)
   ├── Database/BaoDienTu_Database.sql
   └── README.md hướng dẫn cài đặt
✅ Nộp đúng hạn theo quy định của lớp
```

---

*Tài liệu thiết kế hệ thống — Bài Tập Lớn Lập Trình Web*
*Trường Đại học Mở Hà Nội — Khoa Công nghệ Thông tin*
*Đề 1: Xây dựng Trang Báo Điện Tử — ASP.NET + SQL Server — Mô hình 3 lớp*
