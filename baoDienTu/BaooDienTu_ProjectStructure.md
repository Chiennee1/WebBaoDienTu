# BÁO CÁO THIẾT KẾ HỆ THỐNG
## Bài Tập Lớn Môn Lập Trình Web — Đề 1: Xây dựng Trang Báo Điện Tử

> **Trường Đại học Mở Hà Nội — Khoa Công nghệ Thông tin**
> Công nghệ: ASP.NET | CSDL: MS SQL Server | Mô hình: 3-Layer Architecture

---

## MỤC LỤC

1. [Tổng quan hệ thống](#1-tổng-quan-hệ-thống)
2. [Công nghệ sử dụng](#2-công-nghệ-sử-dụng)
3. [Kiến trúc 3 lớp](#3-kiến-trúc-3-lớp)
4. [Cấu trúc thư mục dự án](#4-cấu-trúc-thư-mục-dự-án)
5. [Thiết kế Database](#5-thiết-kế-database)
6. [Thiết kế các lớp (Class Design)](#6-thiết-kế-các-lớp-class-design)
7. [Luồng xử lý nghiệp vụ](#7-luồng-xử-lý-nghiệp-vụ)
8. [Giao diện người dùng](#8-giao-diện-người-dùng)
9. [Bảo mật hệ thống](#9-bảo-mật-hệ-thống)
10. [Hướng dẫn triển khai](#10-hướng-dẫn-triển-khai)

---

## 1. TỔNG QUAN HỆ THỐNG

### 1.1 Mô tả bài toán

Xây dựng hệ thống **Báo Điện Tử** với các chức năng:

| STT | Chức năng | Mô tả |
|-----|-----------|-------|
| 1 | Quản lý tin tức | Tin tức phân theo chủ đề, có thể liên quan nhau |
| 2 | Soạn thảo nội dung | Hỗ trợ nội dung dài, định dạng rich text |
| 3 | Kiểm duyệt | Tin tức chỉ hiển thị sau khi Admin duyệt |
| 4 | Đọc tin / In tin | Người dùng đọc và in bài viết |
| 5 | Gửi tin cho bạn | Chức năng "Tell your friend" qua email |
| 6 | Newsletter | Người dùng đăng ký nhận bản tin |
| 7 | Phân quyền | Admin / Editor / Reader |

### 1.2 Các đối tượng người dùng

```
┌──────────────────────────────────────────────────────────┐
│                    HỆ THỐNG BÁO ĐIỆN TỬ                  │
├────────────┬──────────────────┬──────────────────────────┤
│   ADMIN    │     EDITOR       │        READER            │
├────────────┼──────────────────┼──────────────────────────┤
│ Duyệt bài  │ Viết bài         │ Đọc tin                  │
│ Quản lý    │ Chỉnh sửa bài    │ In tin                   │
│ user       │ Quản lý chủ đề   │ Gửi tin cho bạn          │
│ Cấu hình   │ Upload ảnh       │ Đăng ký Newsletter       │
│ hệ thống   │ Quản lý bài mình │                          │
└────────────┴──────────────────┴──────────────────────────┘
```

---

## 2. CÔNG NGHỆ SỬ DỤNG

### 2.1 Stack kỹ thuật

```
┌─────────────────────────────────────────────────────────┐
│                     CLIENT SIDE                         │
│  HTML5 + CSS3 + Bootstrap 5 + JavaScript + jQuery      │
│  CKEditor 5 (Rich Text Editor) + AJAX                  │
├─────────────────────────────────────────────────────────┤
│                     SERVER SIDE                         │
│  ASP.NET Web Forms / MVC (.NET Framework 4.8)          │
│  C# | LINQ | ADO.NET / Entity Framework                │
├─────────────────────────────────────────────────────────┤
│                     DATABASE                            │
│  Microsoft SQL Server 2019                             │
│  T-SQL | Stored Procedures | Views | Triggers          │
├─────────────────────────────────────────────────────────┤
│                   DEPLOYMENT                            │
│  IIS (Internet Information Services)                   │
│  Windows Server / localhost                            │
└─────────────────────────────────────────────────────────┘
```

### 2.2 Thư viện & Package

| Thư viện | Phiên bản | Mục đích |
|---------|-----------|---------|
| Bootstrap | 5.3 | Responsive UI framework |
| jQuery | 3.7 | DOM manipulation, AJAX |
| CKEditor | 5 | Rich text editor soạn thảo bài |
| FontAwesome | 6 | Icon bộ biểu tượng |
| SweetAlert2 | latest | Hộp thoại thông báo đẹp |
| DataTables | 1.13 | Bảng dữ liệu phân trang Admin |
| Entity Framework | 6.4 | ORM tương tác database |
| SMTP (System.Net.Mail) | built-in | Gửi email Newsletter |

---

## 3. KIẾN TRÚC 3 LỚP

### 3.1 Sơ đồ tổng quan

```
┌─────────────────────────────────────────────────────────────────┐
│                   PRESENTATION LAYER (Layer 1)                  │
│                                                                 │
│   ┌──────────────────────┐    ┌───────────────────────────┐    │
│   │  Web Forms (.aspx)   │    │  Admin Pages (.aspx)      │    │
│   │  - Default.aspx      │    │  - Dashboard.aspx         │    │
│   │  - NewsList.aspx     │    │  - ManageNews.aspx        │    │
│   │  - NewsDetail.aspx   │    │  - ManageUser.aspx        │    │
│   │  - Category.aspx     │    │  - ManageCategory.aspx    │    │
│   └──────────────────────┘    └───────────────────────────┘    │
│              │                              │                   │
│   ┌──────────┴──────────────────────────────┤                   │
│   │         Code-Behind (.aspx.cs)          │                   │
│   └─────────────────────────────────────────┘                   │
└─────────────────────┬───────────────────────────────────────────┘
                      │ Gọi phương thức
┌─────────────────────▼───────────────────────────────────────────┐
│                  BUSINESS LOGIC LAYER (Layer 2)                 │
│                                                                 │
│   ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐   │
│   │  NewsBLL.cs  │  │  UserBLL.cs  │  │  CategoryBLL.cs    │   │
│   │  - GetAll()  │  │  - Login()   │  │  - GetAll()        │   │
│   │  - GetById() │  │  - Register()│  │  - Add()           │   │
│   │  - Add()     │  │  - Update()  │  │  - Update()        │   │
│   │  - Approve() │  │  - GetById() │  │  - Delete()        │   │
│   └──────────────┘  └──────────────┘  └────────────────────┘   │
│                                                                 │
│   ┌──────────────────────┐   ┌────────────────────────────┐    │
│   │  NewsletterBLL.cs    │   │  CommentBLL.cs             │    │
│   │  - Subscribe()       │   │  - Add()                   │    │
│   │  - SendNewsletter()  │   │  - GetByNewsId()           │    │
│   │  - Unsubscribe()     │   │  - Delete()                │    │
│   └──────────────────────┘   └────────────────────────────┘    │
└─────────────────────┬───────────────────────────────────────────┘
                      │ Gọi phương thức
┌─────────────────────▼───────────────────────────────────────────┐
│                   DATA ACCESS LAYER (Layer 3)                   │
│                                                                 │
│   ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐   │
│   │  NewsDAL.cs  │  │  UserDAL.cs  │  │  CategoryDAL.cs    │   │
│   │  SQL Queries │  │  SQL Queries │  │  SQL Queries       │   │
│   │  Stored Proc │  │  Stored Proc │  │  Stored Proc       │   │
│   └──────────────┘  └──────────────┘  └────────────────────┘   │
│                                                                 │
│   ┌────────────────────────────────────────────────────────┐   │
│   │              DBConnection.cs                           │   │
│   │         (Quản lý kết nối SQL Server)                   │   │
│   └────────────────────────────────────────────────────────┘   │
└─────────────────────┬───────────────────────────────────────────┘
                      │ ADO.NET / Entity Framework
┌─────────────────────▼───────────────────────────────────────────┐
│                    DATABASE (MS SQL Server)                      │
│     Tables | Views | Stored Procedures | Triggers               │
└─────────────────────────────────────────────────────────────────┘
```

---

## 4. CẤU TRÚC THƯ MỤC DỰ ÁN

```
BaoDienTu/
│
├── 📁 App_Code/                        # Code dùng chung
│   ├── 📁 DAL/                         # DATA ACCESS LAYER
│   │   ├── DBConnection.cs             # Quản lý kết nối DB
│   │   ├── NewsDAL.cs                  # Thao tác DB bảng News
│   │   ├── CategoryDAL.cs              # Thao tác DB bảng Category
│   │   ├── UserDAL.cs                  # Thao tác DB bảng Users
│   │   ├── CommentDAL.cs               # Thao tác DB bảng Comments
│   │   ├── NewsletterDAL.cs            # Thao tác DB bảng Newsletter
│   │   └── TagDAL.cs                   # Thao tác DB bảng Tags
│   │
│   ├── 📁 BLL/                         # BUSINESS LOGIC LAYER
│   │   ├── NewsBLL.cs                  # Nghiệp vụ tin tức
│   │   ├── CategoryBLL.cs              # Nghiệp vụ chuyên mục
│   │   ├── UserBLL.cs                  # Nghiệp vụ người dùng
│   │   ├── CommentBLL.cs               # Nghiệp vụ bình luận
│   │   ├── NewsletterBLL.cs            # Nghiệp vụ newsletter
│   │   └── EmailBLL.cs                 # Nghiệp vụ gửi email
│   │
│   └── 📁 Models/                      # Entity Models (DTO)
│       ├── NewsModel.cs                # Model tin tức
│       ├── CategoryModel.cs            # Model chuyên mục
│       ├── UserModel.cs                # Model người dùng
│       ├── CommentModel.cs             # Model bình luận
│       ├── NewsletterModel.cs          # Model newsletter
│       └── TagModel.cs                 # Model thẻ tag
│
├── 📁 Admin/                           # Khu vực Admin (yêu cầu đăng nhập)
│   ├── 📁 css/
│   │   └── admin.css
│   ├── 📁 js/
│   │   └── admin.js
│   ├── Default.aspx                    # Dashboard admin
│   ├── ManageNews.aspx                 # Danh sách quản lý tin
│   ├── AddEditNews.aspx                # Thêm/sửa tin tức
│   ├── PendingNews.aspx                # Duyệt bài chờ duyệt
│   ├── ManageCategory.aspx             # Quản lý chuyên mục
│   ├── ManageUser.aspx                 # Quản lý người dùng
│   ├── ManageComment.aspx              # Quản lý bình luận
│   ├── ManageNewsletter.aspx           # Quản lý newsletter
│   ├── SendNewsletter.aspx             # Gửi bản tin email
│   └── AdminMasterPage.master          # Layout admin
│
├── 📁 User/                            # Khu vực người dùng đăng nhập
│   ├── Profile.aspx                    # Trang cá nhân
│   ├── MyNews.aspx                     # Bài viết của tôi
│   └── ChangePassword.aspx             # Đổi mật khẩu
│
├── 📁 Content/                         # PRESENTATION LAYER - Pages chính
│   ├── Default.aspx                    # Trang chủ
│   ├── NewsList.aspx                   # Danh sách tin theo chuyên mục
│   ├── NewsDetail.aspx                 # Chi tiết tin tức
│   ├── Search.aspx                     # Tìm kiếm
│   ├── Login.aspx                      # Đăng nhập
│   ├── Register.aspx                   # Đăng ký
│   └── NewsletterSubscribe.aspx        # Đăng ký nhận Newsletter
│
├── 📁 MasterPages/                     # Layout chung
│   ├── SiteMaster.master               # Layout trang chính
│   └── PrintMaster.master              # Layout trang in
│
├── 📁 Handlers/                        # HTTP Handlers
│   ├── ShareNews.ashx                  # Gửi tin cho bạn (Tell your friend)
│   ├── PrintNews.ashx                  # In tin tức
│   └── NewsletterHandler.ashx          # Xử lý đăng ký newsletter
│
├── 📁 WebServices/                     # Web Services / API
│   ├── NewsService.asmx                # Web service tin tức
│   └── NewsletterService.asmx          # Web service newsletter
│
├── 📁 Static/                          # Tài nguyên tĩnh
│   ├── 📁 css/
│   │   ├── bootstrap.min.css
│   │   ├── site.css                    # Style chính
│   │   └── responsive.css              # Style responsive
│   ├── 📁 js/
│   │   ├── jquery.min.js
│   │   ├── bootstrap.bundle.min.js
│   │   ├── ckeditor.js
│   │   └── site.js                     # Script chính
│   ├── 📁 images/
│   │   ├── logo.png
│   │   └── default-thumb.jpg
│   └── 📁 uploads/                     # Ảnh upload (bài viết)
│       └── news/
│
├── 📁 Database/                        # Scripts SQL
│   ├── CreateDatabase.sql              # Tạo database & tables
│   ├── StoredProcedures.sql            # Stored procedures
│   ├── Views.sql                       # SQL Views
│   ├── Triggers.sql                    # Triggers
│   └── SeedData.sql                    # Dữ liệu mẫu
│
├── Web.config                          # Cấu hình ứng dụng
├── Global.asax                         # Sự kiện Application
└── README.md                           # Tài liệu dự án
```

---

## 5. THIẾT KẾ DATABASE

### 5.1 Sơ đồ ERD

```
┌─────────────┐       ┌──────────────────┐       ┌─────────────┐
│   USERS     │       │      NEWS        │       │  CATEGORIES │
├─────────────┤       ├──────────────────┤       ├─────────────┤
│ UserID (PK) │──┐    │ NewsID (PK)      │──┬──▶ │ CatID (PK)  │
│ Username    │  └───▶│ Title            │  │    │ CatName     │
│ Password    │       │ Summary          │  │    │ ParentID FK │
│ Email       │       │ Content          │  │    │ Slug        │
│ FullName    │       │ Thumbnail        │  │    │ Description │
│ RoleID (FK) │       │ AuthorID (FK)    │  │    │ IsActive    │
│ IsActive    │       │ CatID (FK) ──────┘  │    └─────────────┘
│ CreatedAt   │       │ Status           │  │
│ Avatar      │       │ IsApproved       │  │    ┌─────────────┐
└─────────────┘       │ ApprovedBy (FK)  │  │    │    TAGS     │
                      │ ViewCount        │  │    ├─────────────┤
┌─────────────┐       │ AllowComment     │  │    │ TagID (PK)  │
│    ROLES    │       │ CreatedAt        │  │    │ TagName     │
├─────────────┤       │ UpdatedAt        │  │    │ Slug        │
│ RoleID (PK) │       │ PublishedAt      │  │    └─────────────┘
│ RoleName    │       └──────────────────┘  │           │
│ Description │                │            │    ┌──────▼──────┐
└─────────────┘                │            │    │  NEWS_TAGS  │
                               │            │    ├─────────────┤
                      ┌────────▼──────┐     │    │ NewsID (FK) │
                      │   COMMENTS    │     │    │ TagID (FK)  │
                      ├───────────────┤     │    └─────────────┘
                      │ CmtID (PK)    │     │
                      │ NewsID (FK)───┘     │    ┌─────────────┐
                      │ UserID (FK)         │    │  RELATED    │
                      │ Content             │    │   NEWS      │
                      │ CreatedAt           │    ├─────────────┤
                      │ IsApproved          │    │ NewsID (FK) │
                      └───────────────┘     │    │RelNewsID FK │
                                            └───▶└─────────────┘

┌─────────────────────────┐     ┌──────────────────────────────┐
│       NEWSLETTER        │     │         SHARE_LOG            │
├─────────────────────────┤     ├──────────────────────────────┤
│ SubID (PK)              │     │ ShareID (PK)                 │
│ Email                   │     │ NewsID (FK)                  │
│ FullName                │     │ SenderEmail                  │
│ IsActive                │     │ ReceiverEmail                │
│ SubscribedAt            │     │ Message                      │
│ UnsubscribeToken        │     │ SentAt                       │
└─────────────────────────┘     └──────────────────────────────┘
```

### 5.2 Script tạo Database

```sql
-- =============================================
-- TẠO DATABASE BÁO ĐIỆN TỬ
-- =============================================
CREATE DATABASE BaoDienTuDB;
GO
USE BaoDienTuDB;
GO

-- BẢNG PHÂN QUYỀN
CREATE TABLE Roles (
    RoleID   INT           PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50)  NOT NULL,  -- Admin, Editor, Reader
    Description NVARCHAR(200)
);

-- BẢNG NGƯỜI DÙNG
CREATE TABLE Users (
    UserID      INT           PRIMARY KEY IDENTITY(1,1),
    Username    VARCHAR(50)   NOT NULL UNIQUE,
    Password    VARCHAR(256)  NOT NULL,  -- SHA256 hash
    Email       VARCHAR(100)  NOT NULL UNIQUE,
    FullName    NVARCHAR(100) NOT NULL,
    RoleID      INT           NOT NULL REFERENCES Roles(RoleID),
    IsActive    BIT           DEFAULT 1,
    Avatar      VARCHAR(255),
    CreatedAt   DATETIME      DEFAULT GETDATE(),
    LastLogin   DATETIME
);

-- BẢNG CHUYÊN MỤC (hỗ trợ danh mục con)
CREATE TABLE Categories (
    CatID       INT           PRIMARY KEY IDENTITY(1,1),
    CatName     NVARCHAR(100) NOT NULL,
    Slug        VARCHAR(150)  NOT NULL UNIQUE,
    ParentID    INT           REFERENCES Categories(CatID),  -- NULL = chuyên mục gốc
    Description NVARCHAR(500),
    SortOrder   INT           DEFAULT 0,
    IsActive    BIT           DEFAULT 1,
    CreatedAt   DATETIME      DEFAULT GETDATE()
);

-- BẢNG TIN TỨC
CREATE TABLE News (
    NewsID      INT            PRIMARY KEY IDENTITY(1,1),
    Title       NVARCHAR(300)  NOT NULL,
    Slug        VARCHAR(350)   NOT NULL UNIQUE,
    Summary     NVARCHAR(500),
    Content     NVARCHAR(MAX)  NOT NULL,
    Thumbnail   VARCHAR(255),
    AuthorID    INT            NOT NULL REFERENCES Users(UserID),
    CatID       INT            NOT NULL REFERENCES Categories(CatID),
    Status      TINYINT        DEFAULT 0,  -- 0=Draft, 1=Pending, 2=Approved, 3=Rejected
    IsApproved  BIT            DEFAULT 0,
    ApprovedBy  INT            REFERENCES Users(UserID),
    ApprovedAt  DATETIME,
    ViewCount   INT            DEFAULT 0,
    AllowComment BIT           DEFAULT 1,
    IsFeatured  BIT            DEFAULT 0,  -- Tin nổi bật
    CreatedAt   DATETIME       DEFAULT GETDATE(),
    UpdatedAt   DATETIME       DEFAULT GETDATE(),
    PublishedAt DATETIME
);

-- BẢNG TAGS
CREATE TABLE Tags (
    TagID       INT           PRIMARY KEY IDENTITY(1,1),
    TagName     NVARCHAR(100) NOT NULL UNIQUE,
    Slug        VARCHAR(150)  NOT NULL UNIQUE,
    CreatedAt   DATETIME      DEFAULT GETDATE()
);

-- BẢNG QUAN HỆ TIN TỨC - TAG (nhiều-nhiều)
CREATE TABLE News_Tags (
    NewsID  INT NOT NULL REFERENCES News(NewsID) ON DELETE CASCADE,
    TagID   INT NOT NULL REFERENCES Tags(TagID)  ON DELETE CASCADE,
    PRIMARY KEY (NewsID, TagID)
);

-- BẢNG TIN TỨC LIÊN QUAN
CREATE TABLE RelatedNews (
    NewsID        INT NOT NULL REFERENCES News(NewsID),
    RelatedNewsID INT NOT NULL REFERENCES News(NewsID),
    PRIMARY KEY (NewsID, RelatedNewsID),
    CHECK (NewsID <> RelatedNewsID)
);

-- BẢNG BÌNH LUẬN
CREATE TABLE Comments (
    CmtID       INT            PRIMARY KEY IDENTITY(1,1),
    NewsID      INT            NOT NULL REFERENCES News(NewsID) ON DELETE CASCADE,
    UserID      INT            REFERENCES Users(UserID),
    GuestName   NVARCHAR(100), -- Khách không đăng nhập
    GuestEmail  VARCHAR(100),
    Content     NVARCHAR(1000) NOT NULL,
    IsApproved  BIT            DEFAULT 0,
    CreatedAt   DATETIME       DEFAULT GETDATE()
);

-- BẢNG NEWSLETTER SUBSCRIBERS
CREATE TABLE Newsletter (
    SubID            INT          PRIMARY KEY IDENTITY(1,1),
    Email            VARCHAR(100) NOT NULL UNIQUE,
    FullName         NVARCHAR(100),
    IsActive         BIT          DEFAULT 1,
    SubscribedAt     DATETIME     DEFAULT GETDATE(),
    UnsubscribeToken VARCHAR(100) NOT NULL  -- Token để hủy đăng ký
);

-- BẢNG LOG GỬI TIN CHO BẠN (Tell your friend)
CREATE TABLE ShareLog (
    ShareID       INT          PRIMARY KEY IDENTITY(1,1),
    NewsID        INT          NOT NULL REFERENCES News(NewsID),
    SenderEmail   VARCHAR(100) NOT NULL,
    SenderName    NVARCHAR(100),
    ReceiverEmail VARCHAR(100) NOT NULL,
    Message       NVARCHAR(500),
    SentAt        DATETIME     DEFAULT GETDATE()
);
GO
```

### 5.3 Stored Procedures chính

```sql
-- =============================================
-- SP: Lấy tin tức đã duyệt phân trang
-- =============================================
CREATE PROCEDURE sp_GetApprovedNews
    @CatID   INT = NULL,
    @Page    INT = 1,
    @PageSize INT = 10
AS
BEGIN
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    SELECT
        n.NewsID, n.Title, n.Slug, n.Summary,
        n.Thumbnail, n.ViewCount, n.PublishedAt,
        u.FullName AS AuthorName,
        c.CatName, c.Slug AS CatSlug
    FROM News n
    INNER JOIN Users u ON n.AuthorID = u.UserID
    INNER JOIN Categories c ON n.CatID = c.CatID
    WHERE n.IsApproved = 1
      AND (@CatID IS NULL OR n.CatID = @CatID)
    ORDER BY n.PublishedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- =============================================
-- SP: Duyệt / Từ chối bài viết
-- =============================================
CREATE PROCEDURE sp_ApproveNews
    @NewsID     INT,
    @AdminID    INT,
    @IsApproved BIT  -- 1 = duyệt, 0 = từ chối
AS
BEGIN
    UPDATE News
    SET IsApproved = @IsApproved,
        Status     = CASE WHEN @IsApproved = 1 THEN 2 ELSE 3 END,
        ApprovedBy = @AdminID,
        ApprovedAt = GETDATE(),
        PublishedAt = CASE WHEN @IsApproved = 1 AND PublishedAt IS NULL
                          THEN GETDATE() ELSE PublishedAt END
    WHERE NewsID = @NewsID;
END;
GO

-- =============================================
-- SP: Tăng lượt xem
-- =============================================
CREATE PROCEDURE sp_IncreaseViewCount
    @NewsID INT
AS
BEGIN
    UPDATE News SET ViewCount = ViewCount + 1 WHERE NewsID = @NewsID;
END;
GO

-- =============================================
-- SP: Tìm kiếm tin tức
-- =============================================
CREATE PROCEDURE sp_SearchNews
    @Keyword  NVARCHAR(200),
    @Page     INT = 1,
    @PageSize INT = 10
AS
BEGIN
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    SELECT
        n.NewsID, n.Title, n.Slug, n.Summary,
        n.Thumbnail, n.PublishedAt, u.FullName AS AuthorName
    FROM News n
    INNER JOIN Users u ON n.AuthorID = u.UserID
    WHERE n.IsApproved = 1
      AND (n.Title LIKE N'%' + @Keyword + '%'
           OR n.Summary LIKE N'%' + @Keyword + '%'
           OR n.Content LIKE N'%' + @Keyword + '%')
    ORDER BY n.PublishedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
```

### 5.4 Views

```sql
-- View: Tin tức với đầy đủ thông tin
CREATE VIEW vw_NewsDetail AS
SELECT
    n.NewsID, n.Title, n.Slug, n.Summary, n.Content,
    n.Thumbnail, n.ViewCount, n.PublishedAt, n.AllowComment, n.IsFeatured,
    u.FullName AS AuthorName, u.Avatar AS AuthorAvatar,
    c.CatName, c.CatID, c.Slug AS CatSlug,
    approver.FullName AS ApprovedByName
FROM News n
INNER JOIN Users u ON n.AuthorID = u.UserID
INNER JOIN Categories c ON n.CatID = c.CatID
LEFT JOIN Users approver ON n.ApprovedBy = approver.UserID
WHERE n.IsApproved = 1;
GO

-- View: Tin nổi bật trang chủ
CREATE VIEW vw_FeaturedNews AS
SELECT TOP 5 *
FROM vw_NewsDetail
WHERE IsFeatured = 1
ORDER BY PublishedAt DESC;
GO
```

---

## 6. THIẾT KẾ CÁC LỚP (CLASS DESIGN)

### 6.1 Models (DTO)

```csharp
// NewsModel.cs
public class NewsModel
{
    public int     NewsID       { get; set; }
    public string  Title        { get; set; }
    public string  Slug         { get; set; }
    public string  Summary      { get; set; }
    public string  Content      { get; set; }
    public string  Thumbnail    { get; set; }
    public int     AuthorID     { get; set; }
    public string  AuthorName   { get; set; }
    public int     CatID        { get; set; }
    public string  CatName      { get; set; }
    public int     Status       { get; set; }  // 0=Draft,1=Pending,2=Approved,3=Rejected
    public bool    IsApproved   { get; set; }
    public int     ViewCount    { get; set; }
    public bool    AllowComment { get; set; }
    public bool    IsFeatured   { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime? PublishedAt { get; set; }
}

// UserModel.cs
public class UserModel
{
    public int    UserID   { get; set; }
    public string Username { get; set; }
    public string Email    { get; set; }
    public string FullName { get; set; }
    public int    RoleID   { get; set; }
    public string RoleName { get; set; }
    public bool   IsActive { get; set; }
    public string Avatar   { get; set; }
}

// CategoryModel.cs
public class CategoryModel
{
    public int    CatID       { get; set; }
    public string CatName     { get; set; }
    public string Slug        { get; set; }
    public int?   ParentID    { get; set; }
    public string ParentName  { get; set; }
    public int    SortOrder   { get; set; }
    public bool   IsActive    { get; set; }
    public List<CategoryModel> Children { get; set; } = new List<CategoryModel>();
}
```

### 6.2 Data Access Layer

```csharp
// DBConnection.cs — Quản lý kết nối SQL Server
public class DBConnection
{
    private static readonly string _connStr =
        ConfigurationManager.ConnectionStrings["BaoDienTuDB"].ConnectionString;

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(_connStr);
    }
}

// NewsDAL.cs — Thao tác DB bảng News
public class NewsDAL
{
    public List<NewsModel> GetApprovedNews(int? catId, int page, int pageSize)
    {
        var list = new List<NewsModel>();
        using (var conn = DBConnection.GetConnection())
        {
            conn.Open();
            var cmd = new SqlCommand("sp_GetApprovedNews", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@CatID",    (object)catId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Page",     page);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new NewsModel
                    {
                        NewsID      = (int)reader["NewsID"],
                        Title       = reader["Title"].ToString(),
                        Slug        = reader["Slug"].ToString(),
                        Summary     = reader["Summary"].ToString(),
                        Thumbnail   = reader["Thumbnail"].ToString(),
                        AuthorName  = reader["AuthorName"].ToString(),
                        CatName     = reader["CatName"].ToString(),
                        ViewCount   = (int)reader["ViewCount"],
                        PublishedAt = reader["PublishedAt"] as DateTime?
                    });
                }
            }
        }
        return list;
    }

    public int AddNews(NewsModel model)
    {
        using (var conn = DBConnection.GetConnection())
        {
            conn.Open();
            var cmd = new SqlCommand(@"
                INSERT INTO News (Title, Slug, Summary, Content, Thumbnail,
                                  AuthorID, CatID, Status, AllowComment, CreatedAt)
                VALUES (@Title, @Slug, @Summary, @Content, @Thumbnail,
                        @AuthorID, @CatID, 1, @AllowComment, GETDATE());
                SELECT SCOPE_IDENTITY();", conn);

            cmd.Parameters.AddWithValue("@Title",       model.Title);
            cmd.Parameters.AddWithValue("@Slug",        model.Slug);
            cmd.Parameters.AddWithValue("@Summary",     model.Summary);
            cmd.Parameters.AddWithValue("@Content",     model.Content);
            cmd.Parameters.AddWithValue("@Thumbnail",   (object)model.Thumbnail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AuthorID",    model.AuthorID);
            cmd.Parameters.AddWithValue("@CatID",       model.CatID);
            cmd.Parameters.AddWithValue("@AllowComment",model.AllowComment);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    public bool ApproveNews(int newsId, int adminId, bool approve)
    {
        using (var conn = DBConnection.GetConnection())
        {
            conn.Open();
            var cmd = new SqlCommand("sp_ApproveNews", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@NewsID",     newsId);
            cmd.Parameters.AddWithValue("@AdminID",    adminId);
            cmd.Parameters.AddWithValue("@IsApproved", approve);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
```

### 6.3 Business Logic Layer

```csharp
// NewsBLL.cs — Nghiệp vụ tin tức
public class NewsBLL
{
    private readonly NewsDAL _dal = new NewsDAL();

    // Lấy danh sách tin tức đã duyệt
    public List<NewsModel> GetApprovedNews(int? catId = null, int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;
        return _dal.GetApprovedNews(catId, page, pageSize);
    }

    // Thêm bài viết mới (tự động tạo slug)
    public int AddNews(NewsModel model)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(model.Title))
            throw new ArgumentException("Tiêu đề không được để trống.");
        if (string.IsNullOrWhiteSpace(model.Content))
            throw new ArgumentException("Nội dung không được để trống.");

        // Tạo slug từ tiêu đề
        model.Slug = GenerateSlug(model.Title);
        model.Status = 1; // Pending - chờ duyệt

        return _dal.AddNews(model);
    }

    // Duyệt bài (chỉ Admin)
    public bool ApproveNews(int newsId, int adminId, bool approve)
    {
        var userBll = new UserBLL();
        if (!userBll.IsAdmin(adminId))
            throw new UnauthorizedAccessException("Bạn không có quyền duyệt bài.");

        return _dal.ApproveNews(newsId, adminId, approve);
    }

    // Tạo URL slug thân thiện SEO
    private string GenerateSlug(string title)
    {
        // Chuyển tiếng Việt có dấu → không dấu
        string slug = RemoveVietnameseDiacritics(title.ToLower().Trim());
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = slug.Trim('-');
        return slug + "-" + DateTime.Now.ToString("yyyyMMddHHmm");
    }

    private string RemoveVietnameseDiacritics(string text)
    {
        string normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}

// EmailBLL.cs — Gửi email
public class EmailBLL
{
    private readonly string _smtpHost   = ConfigurationManager.AppSettings["SMTP_Host"];
    private readonly int    _smtpPort   = int.Parse(ConfigurationManager.AppSettings["SMTP_Port"]);
    private readonly string _smtpUser   = ConfigurationManager.AppSettings["SMTP_User"];
    private readonly string _smtpPass   = ConfigurationManager.AppSettings["SMTP_Pass"];
    private readonly string _senderName = "Báo Điện Tử";

    // Gửi tin cho bạn (Tell your friend)
    public bool SendTellFriend(string from, string fromName, string to,
                               string newsTitle, string newsUrl, string message)
    {
        try
        {
            var mail = new MailMessage
            {
                From       = new MailAddress(_smtpUser, _senderName),
                Subject    = $"{fromName} chia sẻ bài viết: {newsTitle}",
                IsBodyHtml = true,
                Body       = BuildTellFriendBody(fromName, newsTitle, newsUrl, message)
            };
            mail.To.Add(to);
            mail.ReplyToList.Add(new MailAddress(from, fromName));

            using (var smtp = new SmtpClient(_smtpHost, _smtpPort))
            {
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                smtp.Send(mail);
            }
            return true;
        }
        catch { return false; }
    }

    // Gửi Newsletter đến tất cả subscribers
    public int SendNewsletter(string subject, string htmlContent)
    {
        var subscribers = new NewsletterDAL().GetActiveSubscribers();
        int sent = 0;
        foreach (var sub in subscribers)
        {
            try
            {
                var mail = new MailMessage
                {
                    From       = new MailAddress(_smtpUser, _senderName),
                    Subject    = subject,
                    IsBodyHtml = true,
                    Body       = htmlContent.Replace("{UNSUBSCRIBE_TOKEN}", sub.UnsubscribeToken)
                };
                mail.To.Add(sub.Email);
                using (var smtp = new SmtpClient(_smtpHost, _smtpPort))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                    smtp.Send(mail);
                }
                sent++;
            }
            catch { /* log error, tiếp tục gửi */ }
        }
        return sent;
    }

    private string BuildTellFriendBody(string fromName, string title, string url, string msg)
    {
        return $@"
        <div style='font-family: Arial; max-width: 600px;'>
            <h2>Bạn của bạn - <strong>{fromName}</strong> muốn chia sẻ bài viết với bạn</h2>
            <h3><a href='{url}'>{title}</a></h3>
            {(string.IsNullOrEmpty(msg) ? "" : $"<p><em>Lời nhắn: {msg}</em></p>")}
            <a href='{url}' style='background:#0066cc; color:#fff; padding:10px 20px;
                                   text-decoration:none; border-radius:4px;'>
                Đọc bài viết
            </a>
        </div>";
    }
}
```

### 6.4 Presentation Layer (Code-Behind mẫu)

```csharp
// NewsDetail.aspx.cs
public partial class NewsDetail : System.Web.UI.Page
{
    private readonly NewsBLL _newsBll     = new NewsBLL();
    private readonly CommentBLL _cmmtBll  = new CommentBLL();

    protected void Page_Load(object sender, EventArgs e)
    {
        string slug = Request.QueryString["slug"];
        if (string.IsNullOrEmpty(slug)) { Response.Redirect("~/"); return; }

        var news = _newsBll.GetBySlug(slug);
        if (news == null) { Response.Redirect("~/404.aspx"); return; }

        // Tăng lượt xem
        _newsBll.IncreaseViewCount(news.NewsID);

        // Bind dữ liệu
        BindNewsDetail(news);
        BindRelatedNews(news.NewsID, news.CatID);
        if (news.AllowComment)
            BindComments(news.NewsID);
    }

    protected void btnShare_Click(object sender, EventArgs e)
    {
        var emailBll = new EmailBLL();
        bool ok = emailBll.SendTellFriend(
            txtSenderEmail.Text, txtSenderName.Text,
            txtFriendEmail.Text,
            hdnNewsTitle.Value, hdnNewsUrl.Value,
            txtMessage.Text);

        lblShareResult.Text = ok
            ? "✅ Đã gửi thành công!"
            : "❌ Gửi thất bại. Vui lòng thử lại.";
    }

    protected void btnPrint_Click(object sender, EventArgs e)
    {
        // Redirect đến trang in với layout riêng
        Response.Redirect($"~/Handlers/PrintNews.ashx?id={hdnNewsId.Value}");
    }
}
```

---

## 7. LUỒNG XỬ LÝ NGHIỆP VỤ

### 7.1 Luồng đăng bài và duyệt bài

```
Editor soạn bài                    Admin kiểm duyệt
      │                                    │
      ▼                                    │
 Nhập nội dung                             │
 (CKEditor rich text)                      │
      │                                    │
      ▼                                    │
 Upload ảnh thumbnail                      │
      │                                    │
      ▼                                    │
 Submit → Status = "Pending"               │
      │                                    │
      │        Email thông báo             │
      ├──────────────────────────────────▶ │
      │                                    ▼
      │                            Vào trang PendingNews
      │                                    │
      │                             Xem xét bài viết
      │                                    │
      │                        ┌───────────┴──────────────┐
      │                        │                          │
      │                    Duyệt (Approve)         Từ chối (Reject)
      │                        │                          │
      │                        ▼                          ▼
      │               Status = "Approved"        Status = "Rejected"
      │               IsApproved = true          Gửi thông báo Editor
      │               PublishedAt = NOW()
      │                        │
      │                        ▼
      │               Bài xuất hiện trên trang
      │
      └───────────────────────────────────
              (Editor nhận thông báo kết quả)
```

### 7.2 Luồng đọc tin và chia sẻ

```
Người dùng truy cập
        │
        ▼
   Trang chủ (Default.aspx)
        │
        ├── Xem tin nổi bật
        ├── Xem tin mới nhất
        └── Chọn chuyên mục
                │
                ▼
        NewsList.aspx?cat=slug
                │
                ▼
        NewsDetail.aspx?slug=...
                │
         ┌──────┼───────────────┐
         │      │               │
         ▼      ▼               ▼
     Đọc tin  In tin      Tell your friend
               │               │
               ▼               ▼
         PrintMaster       Nhập email bạn
         layout            + tin nhắn
                               │
                               ▼
                          Gửi email SMTP
```

### 7.3 Luồng Newsletter

```
Người dùng nhấn "Đăng ký nhận bản tin"
        │
        ▼
 Nhập Email + Họ tên
        │
        ▼
 Kiểm tra email đã tồn tại?
   ├── CÓ → Thông báo "Đã đăng ký rồi"
   └── KHÔNG → Lưu DB + Tạo UnsubscribeToken
                    │
                    ▼
             Gửi email xác nhận
             (có link confirm)
                    │
                    ▼ (click link)
             IsActive = true
                    │
                    ▼
             Nhận bản tin định kỳ
                    │
             (Click "Hủy đăng ký")
                    │
                    ▼
             IsActive = false
```

---

## 8. GIAO DIỆN NGƯỜI DÙNG

### 8.1 Sitemap trang web

```
BÁO ĐIỆN TỬ
├── 🏠 Trang chủ (/)
│   ├── Slider tin nổi bật
│   ├── Tin mới nhất
│   ├── Tin theo chuyên mục
│   └── Form đăng ký Newsletter
│
├── 📰 Chuyên mục (/category/{slug})
│   ├── Chính trị
│   ├── Kinh tế
│   ├── Thể thao
│   ├── Giải trí
│   └── Công nghệ
│
├── 🔍 Tìm kiếm (/search?q=...)
│
├── 📄 Chi tiết bài viết (/news/{slug})
│   ├── Nội dung bài
│   ├── Tags
│   ├── Tin liên quan
│   ├── Bình luận
│   ├── [IN TIN] button
│   └── [CHIA SẺ CHO BẠN] button
│
├── 👤 Tài khoản
│   ├── Đăng nhập (/login)
│   ├── Đăng ký (/register)
│   └── Hồ sơ cá nhân (/user/profile)
│
└── 🔐 Admin (/admin)
    ├── Dashboard
    ├── Quản lý tin tức
    │   ├── Danh sách bài
    │   ├── Thêm/Sửa bài
    │   └── Duyệt bài chờ
    ├── Quản lý chuyên mục
    ├── Quản lý người dùng
    ├── Quản lý bình luận
    └── Quản lý Newsletter
        ├── Danh sách đăng ký
        └── Gửi bản tin
```

### 8.2 Layout trang chủ (Wireframe)

```
┌──────────────────────────────────────────────────────────────────┐
│                    HEADER / NAVBAR                               │
│  [LOGO]  Chính trị | Kinh tế | Thể thao | CN | Giải trí [🔍]  │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│   ┌────────────────────────┐    ┌───────────────────────────┐   │
│   │                        │    │   TIN MỚI NHẤT            │   │
│   │   SLIDER TIN NỔI BẬT   │    ├───────────────────────────┤   │
│   │                        │    │ ■ Tiêu đề bài 1 ...       │   │
│   │   (5 tin nổi bật)      │    │ ■ Tiêu đề bài 2 ...       │   │
│   │                        │    │ ■ Tiêu đề bài 3 ...       │   │
│   └────────────────────────┘    │ ■ Tiêu đề bài 4 ...       │   │
│                                 └───────────────────────────┘   │
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│              TIN THEO CHUYÊN MỤC (responsive grid)              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐       │
│  │ [ảnh]   │  │ [ảnh]   │  │ [ảnh]   │  │ [ảnh]   │       │
│  │ Tiêu đề  │  │ Tiêu đề  │  │ Tiêu đề  │  │ Tiêu đề  │       │
│  │ tóm tắt  │  │ tóm tắt  │  │ tóm tắt  │  │ tóm tắt  │       │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘       │
├──────────────────────────────────────────────────────────────────┤
│  📧 ĐĂNG KÝ NHẬN BẢN TIN — [Nhập email của bạn] [ĐĂNG KÝ]    │
├──────────────────────────────────────────────────────────────────┤
│                         FOOTER                                   │
│  © 2024 Báo Điện Tử | Liên hệ | Về chúng tôi | RSS Feed      │
└──────────────────────────────────────────────────────────────────┘
```

---

## 9. BẢO MẬT HỆ THỐNG

### 9.1 Phân quyền (Role-based Authorization)

```csharp
// Sử dụng FormsAuthentication + Session
// Web.config
/*
<system.web>
  <authentication mode="Forms">
    <forms loginUrl="~/Content/Login.aspx" timeout="30"/>
  </authentication>
  <authorization>
    <deny users="?"/>
  </authorization>
</system.web>
*/

// Trong Admin folder: Web.config riêng
/*
<configuration>
  <system.web>
    <authorization>
      <allow roles="Admin"/>
      <deny users="*"/>
    </authorization>
  </system.web>
</configuration>
*/
```

### 9.2 Các biện pháp bảo mật áp dụng

| Nguy cơ | Biện pháp |
|---------|-----------|
| SQL Injection | Dùng SqlParameter, Stored Procedures — KHÔNG concatenate SQL string trực tiếp |
| XSS (Cross-site Scripting) | `HtmlEncode()` tất cả output, `ValidateRequest="true"` trong ASP.NET |
| CSRF | Dùng ViewState + `__RequestVerificationToken` |
| Brute-force Login | Giới hạn 5 lần đăng nhập sai, khóa tài khoản 15 phút |
| Mật khẩu | Hash SHA-256 + Salt trước khi lưu DB, không lưu plaintext |
| File Upload | Chỉ cho phép jpg/png/gif, kiểm tra magic bytes, đổi tên file ngẫu nhiên |
| Session Hijacking | Session timeout 30 phút, regenerate Session ID sau đăng nhập |
| Directory Traversal | Validate đường dẫn file upload, lưu ngoài webroot |

```csharp
// Hàm hash mật khẩu với salt
public static string HashPassword(string password, string salt)
{
    using (var sha256 = SHA256.Create())
    {
        byte[] bytes = sha256.ComputeHash(
            Encoding.UTF8.GetBytes(password + salt));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}

// Tạo salt ngẫu nhiên
public static string GenerateSalt()
{
    byte[] saltBytes = new byte[32];
    using (var rng = new RNGCryptoServiceProvider())
        rng.GetBytes(saltBytes);
    return Convert.ToBase64String(saltBytes);
}
```

---

## 10. HƯỚNG DẪN TRIỂN KHAI

### 10.1 Yêu cầu môi trường

| Thành phần | Yêu cầu tối thiểu |
|------------|------------------|
| OS | Windows 10/11 hoặc Windows Server 2019+ |
| .NET Framework | 4.8 |
| Visual Studio | 2022 (Community edition miễn phí) |
| SQL Server | 2019 Express (miễn phí) hoặc cao hơn |
| SQL Server Management Studio | SSMS 19+ |
| IIS | 10.0+ (bật trong Windows Features) |

### 10.2 Các bước cài đặt

```
BƯỚC 1 — Cài đặt môi trường
══════════════════════════════
1. Cài Visual Studio 2022 Community
   → Chọn workload: "ASP.NET and web development"
2. Cài SQL Server 2019 Express
3. Cài SQL Server Management Studio (SSMS)
4. Bật IIS: Control Panel → Windows Features
   → Internet Information Services → Web Management Tools + World Wide Web Services

BƯỚC 2 — Tạo Database
══════════════════════════════
1. Mở SSMS, kết nối localhost\SQLEXPRESS
2. Mở file Database/CreateDatabase.sql → Execute (F5)
3. Mở file Database/StoredProcedures.sql → Execute
4. Mở file Database/Views.sql → Execute
5. Mở file Database/SeedData.sql → Execute (dữ liệu mẫu)

BƯỚC 3 — Cấu hình Connection String
══════════════════════════════
Mở Web.config, chỉnh sửa:

<connectionStrings>
  <add name="BaoDienTuDB"
       connectionString="Data Source=localhost\SQLEXPRESS;
                         Initial Catalog=BaoDienTuDB;
                         Integrated Security=True;"
       providerName="System.Data.SqlClient"/>
</connectionStrings>

BƯỚC 4 — Cấu hình Email SMTP (Gmail)
══════════════════════════════
<appSettings>
  <add key="SMTP_Host" value="smtp.gmail.com"/>
  <add key="SMTP_Port" value="587"/>
  <add key="SMTP_User" value="youremail@gmail.com"/>
  <add key="SMTP_Pass" value="app_password_here"/>
  <add key="SiteUrl"   value="http://localhost:8080"/>
</appSettings>

⚠ Dùng Gmail App Password (không phải mật khẩu thường):
   Google Account → Security → 2-Step Verification → App passwords

BƯỚC 5 — Chạy dự án
══════════════════════════════
1. Mở BaoDienTu.sln bằng Visual Studio 2022
2. Build Solution (Ctrl+Shift+B)
3. Chạy bằng IIS Express (F5) hoặc deploy lên IIS local

BƯỚC 6 — Tài khoản mặc định sau seed data
══════════════════════════════
┌─────────────────┬────────────┬──────────────┬──────────┐
│ Vai trò         │ Username   │ Password     │ RoleID   │
├─────────────────┼────────────┼──────────────┼──────────┤
│ Admin           │ admin      │ Admin@123    │ 1        │
│ Editor          │ editor01   │ Editor@123   │ 2        │
│ Reader          │ reader01   │ Reader@123   │ 3        │
└─────────────────┴────────────┴──────────────┴──────────┘
⚠ Đổi mật khẩu sau khi triển khai thực tế!
```

### 10.3 Cấu hình Web.config hoàn chỉnh

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <connectionStrings>
    <add name="BaoDienTuDB"
         connectionString="Data Source=localhost\SQLEXPRESS;
                           Initial Catalog=BaoDienTuDB;
                           Integrated Security=True;"
         providerName="System.Data.SqlClient"/>
  </connectionStrings>

  <appSettings>
    <!-- SMTP Email -->
    <add key="SMTP_Host"    value="smtp.gmail.com"/>
    <add key="SMTP_Port"    value="587"/>
    <add key="SMTP_User"    value="youremail@gmail.com"/>
    <add key="SMTP_Pass"    value="your_app_password"/>
    <!-- Cài đặt chung -->
    <add key="SiteName"     value="Báo Điện Tử"/>
    <add key="SiteUrl"      value="http://localhost:8080"/>
    <add key="PageSize"     value="10"/>
    <add key="UploadPath"   value="~/Static/uploads/news/"/>
    <add key="MaxFileSize"  value="5242880"/><!-- 5MB -->
  </appSettings>

  <system.web>
    <compilation debug="true" targetFramework="4.8"/>
    <httpRuntime targetFramework="4.8" maxRequestLength="10240"/>
    <globalization culture="vi-VN" uiCulture="vi-VN" requestEncoding="utf-8"
                   responseEncoding="utf-8"/>
    <authentication mode="Forms">
      <forms loginUrl="~/Content/Login.aspx" timeout="30"
             cookieless="UseCookies" slidingExpiration="true"/>
    </authentication>
    <sessionState mode="InProc" timeout="30"/>
    <pages validateRequest="true">
      <namespaces>
        <add namespace="System.Web.Optimization"/>
      </namespaces>
    </pages>
  </system.web>
</configuration>
```

### 10.4 Checklist trước khi nộp bài

```
□ Database đã có đủ bảng, stored procedures, views
□ Chức năng đăng nhập / đăng xuất hoạt động
□ Phân quyền Admin / Editor / Reader đúng
□ Trang chủ hiển thị tin tức đã duyệt
□ Xem tin theo chuyên mục
□ Xem chi tiết tin tức
□ In tin tức (trang in riêng)
□ Chức năng "Tell your friend" gửi được email
□ Đăng ký Newsletter hoạt động
□ Editor: thêm/sửa bài, upload ảnh, rich text CKEditor
□ Admin: duyệt/từ chối bài
□ Admin: gửi Newsletter đến subscribers
□ Giao diện responsive trên mobile
□ Không có SQL Injection, XSS cơ bản
□ Mật khẩu được hash trước khi lưu
□ Báo cáo PDF đúng cấu trúc theo mẫu
□ Source code nén ZIP đầy đủ
```

---

*Tài liệu thiết kế hệ thống — Bài Tập Lớn Lập Trình Web*
*Trường Đại học Mở Hà Nội — Khoa Công nghệ Thông tin*
