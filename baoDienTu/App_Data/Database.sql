
CREATE DATABASE BaoDienTuDB
    COLLATE Vietnamese_CI_AS;  -- Hỗ trợ tiếng Việt
GO

USE BaoDienTuDB;
GO


-- 1.1  ROLES — Vai trò người dùng

CREATE TABLE Roles (
    RoleID      INT            NOT NULL IDENTITY(1,1),
    RoleName    NVARCHAR(50)   NOT NULL,
    Description NVARCHAR(200)  NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (RoleID),
    CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
);
GO

-- 1.2  USERS — Tài khoản người dùng
CREATE TABLE Users (
    UserID      INT            NOT NULL IDENTITY(1,1),
    Username    VARCHAR(50)    NOT NULL,
    Password    VARCHAR(256)   NOT NULL,   -- SHA-256 hash
    Salt        VARCHAR(100)   NOT NULL,   -- Password salt
    Email       VARCHAR(150)   NOT NULL,
    FullName    NVARCHAR(150)  NOT NULL,
    RoleID      INT            NOT NULL,
    IsActive    BIT            NOT NULL DEFAULT 1,
    Avatar      VARCHAR(255)   NULL,
    Phone       VARCHAR(20)    NULL,
    LoginFailCount INT         NOT NULL DEFAULT 0,
    LockUntil   DATETIME       NULL,
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
    LastLogin   DATETIME       NULL,
    CONSTRAINT PK_Users PRIMARY KEY (UserID),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email    UNIQUE (Email),
    CONSTRAINT FK_Users_Roles    FOREIGN KEY (RoleID)
        REFERENCES Roles(RoleID)
);
GO

-- 1.3  CATEGORIES — Chuyên mục tin tức (hỗ trợ đa cấp)
CREATE TABLE Categories (
    CatID       INT            NOT NULL IDENTITY(1,1),
    CatName     NVARCHAR(150)  NOT NULL,
    Slug        VARCHAR(200)   NOT NULL,
    ParentID    INT            NULL,       -- NULL = chuyên mục gốc
    Description NVARCHAR(500)  NULL,
    Thumbnail   VARCHAR(255)   NULL,
    SortOrder   INT            NOT NULL DEFAULT 0,
    IsActive    BIT            NOT NULL DEFAULT 1,
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Categories    PRIMARY KEY (CatID),
    CONSTRAINT UQ_Categories_Slug UNIQUE (Slug),
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentID)
        REFERENCES Categories(CatID)
);
GO

-- 1.4  NEWS — Bài viết / Tin tức
CREATE TABLE News (
    NewsID       INT            NOT NULL IDENTITY(1,1),
    Title        NVARCHAR(500)  NOT NULL,
    Slug         VARCHAR(550)   NOT NULL,
    Summary      NVARCHAR(1000) NULL,
    Content      NVARCHAR(MAX)  NOT NULL,
    Thumbnail    VARCHAR(255)   NULL,
    AuthorID     INT            NOT NULL,
    CatID        INT            NOT NULL,
    -- 0=Draft | 1=Pending (chờ duyệt) | 2=Approved | 3=Rejected
    Status       TINYINT        NOT NULL DEFAULT 0,
    IsApproved   BIT            NOT NULL DEFAULT 0,
    ApprovedBy   INT            NULL,
    ApprovedAt   DATETIME       NULL,
    RejectReason NVARCHAR(500)  NULL,
    ViewCount    INT            NOT NULL DEFAULT 0,
    AllowComment BIT            NOT NULL DEFAULT 1,
    IsFeatured   BIT            NOT NULL DEFAULT 0,
    IsHot        BIT            NOT NULL DEFAULT 0,
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    PublishedAt  DATETIME       NULL,
    CONSTRAINT PK_News        PRIMARY KEY (NewsID),
    CONSTRAINT UQ_News_Slug   UNIQUE (Slug),
    CONSTRAINT FK_News_Author FOREIGN KEY (AuthorID)
        REFERENCES Users(UserID),
    CONSTRAINT FK_News_Cat    FOREIGN KEY (CatID)
        REFERENCES Categories(CatID),
    CONSTRAINT FK_News_Approver FOREIGN KEY (ApprovedBy)
        REFERENCES Users(UserID),
    CONSTRAINT CK_News_Status CHECK (Status IN (0, 1, 2, 3))
);
GO

-- 1.5  TAGS — Thẻ từ khóa
CREATE TABLE Tags (
    TagID     INT           NOT NULL IDENTITY(1,1),
    TagName   NVARCHAR(100) NOT NULL,
    Slug      VARCHAR(150)  NOT NULL,
    UseCount  INT           NOT NULL DEFAULT 0,
    CreatedAt DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Tags      PRIMARY KEY (TagID),
    CONSTRAINT UQ_Tags_Name UNIQUE (TagName),
    CONSTRAINT UQ_Tags_Slug UNIQUE (Slug)
);
GO

-- 1.6  NEWS_TAGS — Quan hệ nhiều-nhiều News <-> Tags
CREATE TABLE News_Tags (
    NewsID INT NOT NULL,
    TagID  INT NOT NULL,
    CONSTRAINT PK_News_Tags   PRIMARY KEY (NewsID, TagID),
    CONSTRAINT FK_NT_News     FOREIGN KEY (NewsID)
        REFERENCES News(NewsID) ON DELETE CASCADE,
    CONSTRAINT FK_NT_Tags     FOREIGN KEY (TagID)
        REFERENCES Tags(TagID)  ON DELETE CASCADE
);
GO

-- 1.7  RELATED_NEWS — Tin tức liên quan
CREATE TABLE RelatedNews (
    NewsID        INT NOT NULL,
    RelatedNewsID INT NOT NULL,
    SortOrder     INT NOT NULL DEFAULT 0,
    CONSTRAINT PK_RelatedNews  PRIMARY KEY (NewsID, RelatedNewsID),
    CONSTRAINT FK_RN_News      FOREIGN KEY (NewsID)
        REFERENCES News(NewsID) ON DELETE CASCADE,
    CONSTRAINT FK_RN_Related   FOREIGN KEY (RelatedNewsID)
        REFERENCES News(NewsID),
    CONSTRAINT CK_RN_NoSelf    CHECK (NewsID <> RelatedNewsID)
);
GO

-- 1.8  COMMENTS — Bình luận bài viết
CREATE TABLE Comments (
    CmtID      INT            NOT NULL IDENTITY(1,1),
    NewsID     INT            NOT NULL,
    UserID     INT            NULL,        -- NULL nếu là khách
    GuestName  NVARCHAR(100)  NULL,
    GuestEmail VARCHAR(150)   NULL,
    Content    NVARCHAR(2000) NOT NULL,
    IsApproved BIT            NOT NULL DEFAULT 0,
    ParentID   INT            NULL,        -- Trả lời bình luận khác
    CreatedAt  DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Comments      PRIMARY KEY (CmtID),
    CONSTRAINT FK_Cmt_News      FOREIGN KEY (NewsID)
        REFERENCES News(NewsID) ON DELETE CASCADE,
    CONSTRAINT FK_Cmt_User      FOREIGN KEY (UserID)
        REFERENCES Users(UserID),
    CONSTRAINT FK_Cmt_Parent    FOREIGN KEY (ParentID)
        REFERENCES Comments(CmtID)
);
GO

-- 1.9  NEWSLETTER — Đăng ký nhận bản tin
CREATE TABLE Newsletter (
    SubID            INT          NOT NULL IDENTITY(1,1),
    Email            VARCHAR(150) NOT NULL,
    FullName         NVARCHAR(150) NULL,
    IsActive         BIT          NOT NULL DEFAULT 0,  -- 0 = chờ xác nhận email
    IsConfirmed      BIT          NOT NULL DEFAULT 0,
    ConfirmToken     VARCHAR(100) NOT NULL,
    UnsubscribeToken VARCHAR(100) NOT NULL,
    SubscribedAt     DATETIME     NOT NULL DEFAULT GETDATE(),
    ConfirmedAt      DATETIME     NULL,
    CONSTRAINT PK_Newsletter      PRIMARY KEY (SubID),
    CONSTRAINT UQ_Newsletter_Email UNIQUE (Email)
);
GO

-- 1.10 NEWSLETTER_SENDS — Lịch sử gửi bản tin
CREATE TABLE Newsletter_Sends (
    SendID      INT            NOT NULL IDENTITY(1,1),
    Subject     NVARCHAR(300)  NOT NULL,
    HtmlContent NVARCHAR(MAX)  NOT NULL,
    SentBy      INT            NOT NULL,
    TotalSent   INT            NOT NULL DEFAULT 0,
    SentAt      DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_NL_Sends    PRIMARY KEY (SendID),
    CONSTRAINT FK_NLS_User    FOREIGN KEY (SentBy)
        REFERENCES Users(UserID)
);
GO

-- 1.11 SHARE_LOG — Log gửi tin cho bạn (Tell your friend)
CREATE TABLE ShareLog (
    ShareID       INT          NOT NULL IDENTITY(1,1),
    NewsID        INT          NOT NULL,
    SenderName    NVARCHAR(100) NULL,
    SenderEmail   VARCHAR(150) NOT NULL,
    ReceiverEmail VARCHAR(150) NOT NULL,
    Message       NVARCHAR(500) NULL,
    IsSent        BIT          NOT NULL DEFAULT 0,
    SentAt        DATETIME     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_ShareLog   PRIMARY KEY (ShareID),
    CONSTRAINT FK_SL_News    FOREIGN KEY (NewsID)
        REFERENCES News(NewsID) ON DELETE CASCADE
);
GO

-- 1.12 SETTINGS — Cấu hình hệ thống
CREATE TABLE Settings (
    SettingKey   VARCHAR(100)   NOT NULL,
    SettingValue NVARCHAR(1000) NULL,
    Description  NVARCHAR(300)  NULL,
    UpdatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Settings PRIMARY KEY (SettingKey)
);
GO

--  INDEX (Tối ưu truy vấn)

-- Index cho tìm kiếm tin tức
CREATE NONCLUSTERED INDEX IX_News_Status_Published
    ON News (IsApproved, Status, PublishedAt DESC)
    INCLUDE (Title, Slug, Thumbnail, Summary, ViewCount, CatID, AuthorID);

CREATE NONCLUSTERED INDEX IX_News_CatID
    ON News (CatID, IsApproved, PublishedAt DESC);

CREATE NONCLUSTERED INDEX IX_News_AuthorID
    ON News (AuthorID, Status);

CREATE NONCLUSTERED INDEX IX_News_Featured
    ON News (IsFeatured, IsApproved, PublishedAt DESC)
    WHERE IsFeatured = 1;

CREATE NONCLUSTERED INDEX IX_News_Hot
    ON News (IsHot, IsApproved, PublishedAt DESC)
    WHERE IsHot = 1;

--   VIEWS

-- 3.1  vw_NewsDetail — Tin tức đầy đủ thông tin (đã duyệt)
CREATE VIEW vw_NewsDetail
AS
SELECT
    n.NewsID,
    n.Title,
    n.Slug,
    n.Summary,
    n.Content,
    n.Thumbnail,
    n.ViewCount,
    n.AllowComment,
    n.IsFeatured,
    n.IsHot,
    n.PublishedAt,
    n.CreatedAt,
    n.UpdatedAt,
    n.CatID,
    c.CatName,
    c.Slug        AS CatSlug,
    c.ParentID    AS CatParentID,
    pc.CatName    AS ParentCatName,
    pc.Slug       AS ParentCatSlug,
    n.AuthorID,
    u.FullName    AS AuthorName,
    u.Avatar      AS AuthorAvatar,
    u.Email       AS AuthorEmail,
    n.ApprovedBy,
    ap.FullName   AS ApprovedByName
FROM News n
INNER JOIN Categories c  ON n.CatID     = c.CatID
LEFT  JOIN Categories pc ON c.ParentID  = pc.CatID
INNER JOIN Users u        ON n.AuthorID  = u.UserID
LEFT  JOIN Users ap       ON n.ApprovedBy = ap.UserID
WHERE n.IsApproved = 1
  AND n.Status     = 2;
GO

-- 3.2  vw_FeaturedNews — 6 tin nổi bật mới nhất (trang chủ)
CREATE VIEW vw_FeaturedNews
AS
SELECT TOP 6
    NewsID, Title, Slug, Summary, Thumbnail,
    CatID, CatName, CatSlug,
    AuthorName, PublishedAt, ViewCount
FROM vw_NewsDetail
WHERE IsFeatured = 1
ORDER BY PublishedAt DESC;
GO

-- 3.3  vw_LatestNews — 10 tin mới nhất
CREATE VIEW vw_LatestNews
AS
SELECT TOP 10
    NewsID, Title, Slug, Summary, Thumbnail,
    CatID, CatName, CatSlug,
    AuthorName, PublishedAt, ViewCount
FROM vw_NewsDetail
ORDER BY PublishedAt DESC;
GO

-- 3.4  vw_MostViewedNews — 10 tin được đọc nhiều nhất
CREATE VIEW vw_MostViewedNews
AS
SELECT TOP 10
    NewsID, Title, Slug, Thumbnail,
    CatName, CatSlug, PublishedAt, ViewCount
FROM vw_NewsDetail
ORDER BY ViewCount DESC;
GO

-- 3.5  vw_NewsByCategory — Số lượng tin theo chuyên mục
CREATE VIEW vw_NewsByCategory
AS
SELECT
    c.CatID,
    c.CatName,
    c.Slug AS CatSlug,
    c.ParentID,
    COUNT(n.NewsID) AS NewsCount
FROM Categories c
LEFT JOIN News n ON c.CatID = n.CatID
                AND n.IsApproved = 1
                AND n.Status = 2
GROUP BY c.CatID, c.CatName, c.Slug, c.ParentID;
GO

-- 3.6  vw_PendingNews — Bài chờ duyệt (dành cho Admin)
CREATE VIEW vw_PendingNews
AS
SELECT
    n.NewsID,
    n.Title,
    n.Summary,
    n.Thumbnail,
    n.CreatedAt,
    n.AuthorID,
    u.FullName  AS AuthorName,
    u.Email     AS AuthorEmail,
    n.CatID,
    c.CatName
FROM News n
INNER JOIN Users      u ON n.AuthorID = u.UserID
INNER JOIN Categories c ON n.CatID    = c.CatID
WHERE n.Status = 1;  -- Pending
GO

-- 3.7  vw_CommentDetails — Bình luận kèm thông tin
CREATE VIEW vw_CommentDetails
AS
SELECT
    cm.CmtID,
    cm.NewsID,
    n.Title     AS NewsTitle,
    n.Slug      AS NewsSlug,
    cm.UserID,
    COALESCE(u.FullName, cm.GuestName, N'Khách')  AS DisplayName,
    COALESCE(u.Email,    cm.GuestEmail)            AS DisplayEmail,
    COALESCE(u.Avatar,   NULL)                     AS DisplayAvatar,
    cm.Content,
    cm.ParentID,
    cm.IsApproved,
    cm.CreatedAt
FROM Comments cm
INNER JOIN News  n ON cm.NewsID = n.NewsID
LEFT  JOIN Users u ON cm.UserID = u.UserID;
GO

-- 3.8  vw_AdminDashboard — Thống kê tổng hợp cho Admin
CREATE VIEW vw_AdminDashboard
AS
SELECT
    (SELECT COUNT(*) FROM News WHERE IsApproved = 1)  AS TotalApprovedNews,
    (SELECT COUNT(*) FROM News WHERE Status = 1)       AS TotalPendingNews,
    (SELECT COUNT(*) FROM Users WHERE IsActive = 1)    AS TotalActiveUsers,
    (SELECT COUNT(*) FROM Newsletter
     WHERE IsActive = 1 AND IsConfirmed = 1)           AS TotalSubscribers,
    (SELECT COUNT(*) FROM Comments WHERE IsApproved = 0) AS TotalPendingComments,
    (SELECT SUM(ViewCount) FROM News WHERE IsApproved = 1) AS TotalViews;
GO

--   FUNCTIONS

-- 4.1  fn_GenerateSlug — Tạo slug từ chuỗi tiếng Việt
--       (Lưu ý: xử lý dấu tiếng Việt nên làm ở tầng C# tốt hơn,
--        hàm này chỉ chuẩn hoá slug đã bỏ dấu)
CREATE FUNCTION fn_GenerateSlug (@input NVARCHAR(500))
RETURNS VARCHAR(550)
AS
BEGIN
    DECLARE @result VARCHAR(550);
    SET @result = LOWER(@input);
    -- Thay khoảng trắng bằng dấu gạch ngang
    SET @result = REPLACE(@result, N' ', '-');
    -- Loại ký tự đặc biệt (giữ a-z, 0-9, -)
    SET @result = (
        SELECT STRING_AGG(c, '')
        FROM (
            SELECT SUBSTRING(@result, n, 1) AS c
            FROM (VALUES(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),
                        (11),(12),(13),(14),(15),(16),(17),(18),(19),(20),
                        (21),(22),(23),(24),(25),(26),(27),(28),(29),(30),
                        (31),(32),(33),(34),(35),(36),(37),(38),(39),(40),
                        (41),(42),(43),(44),(45),(46),(47),(48),(49),(50),
                        (51),(52),(53),(54),(55),(56),(57),(58),(59),(60),
                        (61),(62),(63),(64),(65),(66),(67),(68),(69),(70),
                        (71),(72),(73),(74),(75),(76),(77),(78),(79),(80),
                        (81),(82),(83),(84),(85),(86),(87),(88),(89),(90),
                        (91),(92),(93),(94),(95),(96),(97),(98),(99),(100)) AS nums(n)
            WHERE n <= LEN(@result)
        ) AS chars
        WHERE c LIKE '[a-z0-9-]'
    );
    -- Loại dấu -- liên tiếp
    WHILE CHARINDEX('--', @result) > 0
        SET @result = REPLACE(@result, '--', '-');
    -- Cắt dấu - ở đầu và cuối
    SET @result = LTRIM(RTRIM(@result));
    RETURN @result;
END;
GO

-- 4.2  fn_GetCategoryBreadcrumb — Lấy đường dẫn chuyên mục
CREATE FUNCTION fn_GetCategoryBreadcrumb (@CatID INT)
RETURNS NVARCHAR(500)
AS
BEGIN
    DECLARE @result  NVARCHAR(500) = '';
    DECLARE @current INT = @CatID;
    DECLARE @name    NVARCHAR(150);
    DECLARE @parent  INT;

    WHILE @current IS NOT NULL
    BEGIN
        SELECT @name = CatName, @parent = ParentID
        FROM Categories WHERE CatID = @current;

        IF @result = ''
            SET @result = @name;
        ELSE
            SET @result = @name + N' > ' + @result;

        SET @current = @parent;
    END;
    RETURN @result;
END;
GO

--   STORED PROCEDURES

--  NHÓM A: STORED PROCEDURES — NEWS

-- ------------------------------------------------------------
-- A1. sp_GetNewsList — Danh sách tin phân trang
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetNewsList
    @CatID    INT      = NULL,   -- NULL = tất cả chuyên mục
    @Page     INT      = 1,
    @PageSize INT      = 10,
    @Total    INT      OUTPUT    -- Tổng số bản ghi
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Đếm tổng
    SELECT @Total = COUNT(*)
    FROM vw_NewsDetail
    WHERE (@CatID IS NULL OR CatID = @CatID);

    -- Lấy dữ liệu phân trang
    SELECT
        NewsID, Title, Slug, Summary, Thumbnail,
        CatID, CatName, CatSlug,
        AuthorName, PublishedAt, ViewCount,
        IsFeatured, IsHot
    FROM vw_NewsDetail
    WHERE (@CatID IS NULL OR CatID = @CatID)
    ORDER BY PublishedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- ------------------------------------------------------------
-- A2. sp_GetNewsDetail — Chi tiết 1 bài viết theo Slug
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetNewsDetail
    @Slug VARCHAR(550)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM vw_NewsDetail
    WHERE Slug = @Slug;
END;
GO

-- ------------------------------------------------------------
-- A3. sp_GetNewsById — Lấy bài viết theo ID (kể cả chưa duyệt)
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetNewsById
    @NewsID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        n.*,
        u.FullName  AS AuthorName,
        u.Email     AS AuthorEmail,
        u.Avatar    AS AuthorAvatar,
        c.CatName,
        c.Slug      AS CatSlug,
        ap.FullName AS ApprovedByName
    FROM News n
    INNER JOIN Users      u  ON n.AuthorID   = u.UserID
    INNER JOIN Categories c  ON n.CatID      = c.CatID
    LEFT  JOIN Users      ap ON n.ApprovedBy = ap.UserID
    WHERE n.NewsID = @NewsID;
END;
GO

-- ------------------------------------------------------------
-- A4. sp_AddNews — Thêm bài viết mới
-- ------------------------------------------------------------
CREATE PROCEDURE sp_AddNews
    @Title       NVARCHAR(500),
    @Slug        VARCHAR(550),
    @Summary     NVARCHAR(1000),
    @Content     NVARCHAR(MAX),
    @Thumbnail   VARCHAR(255),
    @AuthorID    INT,
    @CatID       INT,
    @AllowComment BIT,
    @NewNewsID   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO News
        (Title, Slug, Summary, Content, Thumbnail,
         AuthorID, CatID, Status, IsApproved, AllowComment, CreatedAt, UpdatedAt)
    VALUES
        (@Title, @Slug, @Summary, @Content, @Thumbnail,
         @AuthorID, @CatID, 1, 0, @AllowComment, GETDATE(), GETDATE());

    SET @NewNewsID = SCOPE_IDENTITY();
END;
GO

-- ------------------------------------------------------------
-- A5. sp_UpdateNews — Cập nhật bài viết
-- ------------------------------------------------------------
CREATE PROCEDURE sp_UpdateNews
    @NewsID      INT,
    @Title       NVARCHAR(500),
    @Slug        VARCHAR(550),
    @Summary     NVARCHAR(1000),
    @Content     NVARCHAR(MAX),
    @Thumbnail   VARCHAR(255),
    @CatID       INT,
    @AllowComment BIT,
    @IsFeatured  BIT,
    @IsHot       BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE News
    SET Title        = @Title,
        Slug         = @Slug,
        Summary      = @Summary,
        Content      = @Content,
        Thumbnail    = ISNULL(@Thumbnail, Thumbnail),
        CatID        = @CatID,
        AllowComment = @AllowComment,
        IsFeatured   = @IsFeatured,
        IsHot        = @IsHot,
        Status       = 1,       -- Reset về Pending sau khi sửa
        IsApproved   = 0,
        UpdatedAt    = GETDATE()
    WHERE NewsID = @NewsID;
END;
GO

-- ------------------------------------------------------------
-- A6. sp_DeleteNews — Xoá bài viết
-- ------------------------------------------------------------
CREATE PROCEDURE sp_DeleteNews
    @NewsID INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Cascade sẽ tự xoá Comments, News_Tags, RelatedNews, ShareLog
    DELETE FROM News WHERE NewsID = @NewsID;
END;
GO

-- ------------------------------------------------------------
-- A7. sp_ApproveNews — Duyệt / Từ chối bài viết
-- ------------------------------------------------------------
CREATE PROCEDURE sp_ApproveNews
    @NewsID       INT,
    @AdminID      INT,
    @IsApproved   BIT,          -- 1 = duyệt, 0 = từ chối
    @RejectReason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE News
    SET IsApproved   = @IsApproved,
        Status       = CASE WHEN @IsApproved = 1 THEN 2 ELSE 3 END,
        ApprovedBy   = @AdminID,
        ApprovedAt   = GETDATE(),
        RejectReason = CASE WHEN @IsApproved = 0 THEN @RejectReason ELSE NULL END,
        PublishedAt  = CASE
                           WHEN @IsApproved = 1 AND PublishedAt IS NULL
                           THEN GETDATE()
                           ELSE PublishedAt
                       END
    WHERE NewsID = @NewsID;
END;
GO

-- ------------------------------------------------------------
-- A8. sp_IncreaseViewCount — Tăng lượt xem
-- ------------------------------------------------------------
CREATE PROCEDURE sp_IncreaseViewCount
    @NewsID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE News
    SET ViewCount = ViewCount + 1
    WHERE NewsID = @NewsID;
END;
GO

-- ------------------------------------------------------------
-- A9. sp_SearchNews — Tìm kiếm tin tức phân trang
-- ------------------------------------------------------------
CREATE PROCEDURE sp_SearchNews
    @Keyword  NVARCHAR(300),
    @Page     INT = 1,
    @PageSize INT = 10,
    @Total    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    DECLARE @SearchKw NVARCHAR(304) = N'%' + @Keyword + N'%';

    SELECT @Total = COUNT(*)
    FROM vw_NewsDetail
    WHERE Title   LIKE @SearchKw
       OR Summary LIKE @SearchKw
       OR Content LIKE @SearchKw;

    SELECT
        NewsID, Title, Slug, Summary, Thumbnail,
        CatName, CatSlug, AuthorName, PublishedAt, ViewCount
    FROM vw_NewsDetail
    WHERE Title   LIKE @SearchKw
       OR Summary LIKE @SearchKw
       OR Content LIKE @SearchKw
    ORDER BY PublishedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- ------------------------------------------------------------
-- A10. sp_GetNewsByTag — Lấy tin tức theo Tag
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetNewsByTag
    @TagSlug  VARCHAR(150),
    @Page     INT = 1,
    @PageSize INT = 10,
    @Total    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT @Total = COUNT(*)
    FROM vw_NewsDetail nd
    INNER JOIN News_Tags nt ON nd.NewsID = nt.NewsID
    INNER JOIN Tags t       ON nt.TagID  = t.TagID
    WHERE t.Slug = @TagSlug;

    SELECT
        nd.NewsID, nd.Title, nd.Slug, nd.Summary, nd.Thumbnail,
        nd.CatName, nd.CatSlug, nd.AuthorName, nd.PublishedAt, nd.ViewCount
    FROM vw_NewsDetail nd
    INNER JOIN News_Tags nt ON nd.NewsID = nt.NewsID
    INNER JOIN Tags t       ON nt.TagID  = t.TagID
    WHERE t.Slug = @TagSlug
    ORDER BY nd.PublishedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- ------------------------------------------------------------
-- A11. sp_GetRelatedNews — Lấy tin liên quan
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetRelatedNews
    @NewsID INT,
    @Top    INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    -- Tin được gắn liên quan thủ công
    SELECT TOP (@Top)
        nd.NewsID, nd.Title, nd.Slug, nd.Thumbnail, nd.PublishedAt, nd.CatName
    FROM vw_NewsDetail nd
    INNER JOIN RelatedNews rn ON nd.NewsID = rn.RelatedNewsID
    WHERE rn.NewsID = @NewsID
    ORDER BY rn.SortOrder

    -- Bổ sung thêm bằng cùng chuyên mục nếu chưa đủ
    SELECT TOP (@Top)
        nd2.NewsID, nd2.Title, nd2.Slug, nd2.Thumbnail, nd2.PublishedAt, nd2.CatName
    FROM vw_NewsDetail nd2
    WHERE nd2.CatID = (SELECT CatID FROM News WHERE NewsID = @NewsID)
      AND nd2.NewsID <> @NewsID
      AND nd2.NewsID NOT IN (
            SELECT RelatedNewsID FROM RelatedNews WHERE NewsID = @NewsID)
    ORDER BY nd2.PublishedAt DESC;
END;
GO

-- ------------------------------------------------------------
-- A12. sp_GetAdminNewsList — Danh sách tin cho Admin (tất cả trạng thái)
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetAdminNewsList
    @Status   TINYINT  = NULL,   -- NULL = tất cả
    @AuthorID INT      = NULL,   -- NULL = tất cả tác giả
    @CatID    INT      = NULL,
    @Keyword  NVARCHAR(200) = NULL,
    @Page     INT      = 1,
    @PageSize INT      = 20,
    @Total    INT      OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT  = (@Page - 1) * @PageSize;
    DECLARE @Kw     NVARCHAR(204) = CASE WHEN @Keyword IS NOT NULL
                                         THEN N'%' + @Keyword + N'%'
                                         ELSE NULL END;

    SELECT @Total = COUNT(*)
    FROM News n
    WHERE (@Status   IS NULL OR n.Status   = @Status)
      AND (@AuthorID IS NULL OR n.AuthorID = @AuthorID)
      AND (@CatID    IS NULL OR n.CatID    = @CatID)
      AND (@Kw       IS NULL OR n.Title LIKE @Kw);

    SELECT
        n.NewsID, n.Title, n.Slug, n.Thumbnail, n.Status, n.IsApproved,
        n.IsFeatured, n.IsHot, n.ViewCount, n.CreatedAt, n.PublishedAt,
        u.FullName  AS AuthorName,
        c.CatName
    FROM News n
    INNER JOIN Users      u ON n.AuthorID = u.UserID
    INNER JOIN Categories c ON n.CatID    = c.CatID
    WHERE (@Status   IS NULL OR n.Status   = @Status)
      AND (@AuthorID IS NULL OR n.AuthorID = @AuthorID)
      AND (@CatID    IS NULL OR n.CatID    = @CatID)
      AND (@Kw       IS NULL OR n.Title LIKE @Kw)
    ORDER BY n.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

--  NHÓM B: STORED PROCEDURES — USERS

-- ------------------------------------------------------------
-- B1. sp_Login — Đăng nhập (trả về thông tin user nếu hợp lệ)
-- ------------------------------------------------------------
CREATE PROCEDURE sp_Login
    @Username VARCHAR(50),
    @Password VARCHAR(256)   -- SHA-256 hash đã kết hợp salt
AS
BEGIN
    SET NOCOUNT ON;
    -- Lấy Salt trước
    SELECT Salt FROM Users WHERE Username = @Username AND IsActive = 1;
END;
GO

-- ------------------------------------------------------------
-- B2. sp_VerifyLogin — Xác thực đăng nhập sau khi hash mật khẩu
-- ------------------------------------------------------------
CREATE PROCEDURE sp_VerifyLogin
    @Username VARCHAR(50),
    @HashedPw VARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra tài khoản bị khóa
    IF EXISTS (
        SELECT 1 FROM Users
        WHERE Username = @Username
          AND LockUntil > GETDATE()
    )
    BEGIN
        SELECT -1 AS Result;  -- Tài khoản bị khóa
        RETURN;
    END;

    DECLARE @UserID INT, @Stored VARCHAR(256), @IsActive BIT;
    SELECT @UserID   = UserID,
           @Stored   = Password,
           @IsActive = IsActive
    FROM Users
    WHERE Username = @Username;

    IF @UserID IS NULL
    BEGIN
        SELECT -2 AS Result;  -- Không tìm thấy tài khoản
        RETURN;
    END;

    IF @IsActive = 0
    BEGIN
        SELECT -3 AS Result;  -- Tài khoản bị vô hiệu hóa
        RETURN;
    END;

    IF @Stored = @HashedPw
    BEGIN
        -- Đăng nhập thành công: reset LoginFailCount, cập nhật LastLogin
        UPDATE Users
        SET LoginFailCount = 0, LastLogin = GETDATE(), LockUntil = NULL
        WHERE UserID = @UserID;

        SELECT u.UserID, u.Username, u.FullName, u.Email, u.Avatar,
               u.RoleID, r.RoleName
        FROM Users u
        INNER JOIN Roles r ON u.RoleID = r.RoleID
        WHERE u.UserID = @UserID;
    END
    ELSE
    BEGIN
        -- Sai mật khẩu: tăng LoginFailCount, khóa nếu >= 5 lần
        UPDATE Users
        SET LoginFailCount = LoginFailCount + 1,
            LockUntil = CASE
                            WHEN LoginFailCount + 1 >= 5
                            THEN DATEADD(MINUTE, 15, GETDATE())
                            ELSE NULL
                        END
        WHERE UserID = @UserID;

        SELECT -4 AS Result;  -- Sai mật khẩu
    END;
END;
GO

-- ------------------------------------------------------------
-- B3. sp_RegisterUser — Đăng ký tài khoản mới
-- ------------------------------------------------------------
CREATE PROCEDURE sp_RegisterUser
    @Username  VARCHAR(50),
    @Password  VARCHAR(256),
    @Salt      VARCHAR(100),
    @Email     VARCHAR(150),
    @FullName  NVARCHAR(150),
    @NewUserID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra trùng username hoặc email
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
    BEGIN
        SET @NewUserID = -1;  -- Username đã tồn tại
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        SET @NewUserID = -2;  -- Email đã tồn tại
        RETURN;
    END;

    INSERT INTO Users (Username, Password, Salt, Email, FullName, RoleID, IsActive)
    VALUES (@Username, @Password, @Salt, @Email, @FullName, 3, 1); -- RoleID=3: Reader

    SET @NewUserID = SCOPE_IDENTITY();
END;
GO

-- ------------------------------------------------------------
-- B4. sp_GetUserById — Lấy thông tin user theo ID
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetUserById
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserID, u.Username, u.Email, u.FullName,
           u.RoleID, r.RoleName, u.IsActive, u.Avatar,
           u.Phone, u.CreatedAt, u.LastLogin
    FROM Users u
    INNER JOIN Roles r ON u.RoleID = r.RoleID
    WHERE u.UserID = @UserID;
END;
GO

-- ------------------------------------------------------------
-- B5. sp_UpdateUserProfile — Cập nhật hồ sơ cá nhân
-- ------------------------------------------------------------
CREATE PROCEDURE sp_UpdateUserProfile
    @UserID   INT,
    @FullName NVARCHAR(150),
    @Phone    VARCHAR(20),
    @Avatar   VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users
    SET FullName = @FullName,
        Phone    = @Phone,
        Avatar   = ISNULL(@Avatar, Avatar)
    WHERE UserID = @UserID;
END;
GO

-- ------------------------------------------------------------
-- B6. sp_ChangePassword — Đổi mật khẩu
-- ------------------------------------------------------------
CREATE PROCEDURE sp_ChangePassword
    @UserID      INT,
    @OldHashedPw VARCHAR(256),
    @NewHashedPw VARCHAR(256),
    @NewSalt     VARCHAR(100),
    @Result      INT OUTPUT   -- 1=OK, -1=Sai mật khẩu cũ
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Users
               WHERE UserID = @UserID AND Password = @OldHashedPw)
    BEGIN
        UPDATE Users
        SET Password = @NewHashedPw, Salt = @NewSalt
        WHERE UserID = @UserID;
        SET @Result = 1;
    END
    ELSE
        SET @Result = -1;
END;
GO

-- ------------------------------------------------------------
-- B7. sp_GetAllUsers — Danh sách user cho Admin
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetAllUsers
    @RoleID   INT  = NULL,
    @IsActive BIT  = NULL,
    @Keyword  NVARCHAR(150) = NULL,
    @Page     INT  = 1,
    @PageSize INT  = 20,
    @Total    INT  OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    DECLARE @Kw NVARCHAR(154) = CASE WHEN @Keyword IS NOT NULL
                                     THEN N'%' + @Keyword + N'%'
                                     ELSE NULL END;

    SELECT @Total = COUNT(*)
    FROM Users u
    WHERE (@RoleID   IS NULL OR u.RoleID   = @RoleID)
      AND (@IsActive IS NULL OR u.IsActive = @IsActive)
      AND (@Kw       IS NULL OR u.FullName LIKE @Kw OR u.Username LIKE @Kw
                              OR u.Email LIKE @Kw);

    SELECT u.UserID, u.Username, u.Email, u.FullName,
           u.RoleID, r.RoleName, u.IsActive, u.CreatedAt, u.LastLogin
    FROM Users u
    INNER JOIN Roles r ON u.RoleID = r.RoleID
    WHERE (@RoleID   IS NULL OR u.RoleID   = @RoleID)
      AND (@IsActive IS NULL OR u.IsActive = @IsActive)
      AND (@Kw       IS NULL OR u.FullName LIKE @Kw OR u.Username LIKE @Kw
                              OR u.Email LIKE @Kw)
    ORDER BY u.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- ============================================================
--  NHÓM C: STORED PROCEDURES — CATEGORIES
-- ============================================================

-- ------------------------------------------------------------
-- C1. sp_GetCategories — Danh sách tất cả chuyên mục
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetCategories
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        c.CatID, c.CatName, c.Slug, c.ParentID,
        p.CatName  AS ParentName,
        c.SortOrder, c.IsActive, c.CreatedAt,
        dbo.fn_GetCategoryBreadcrumb(c.CatID) AS Breadcrumb,
        (SELECT COUNT(*) FROM News n
         WHERE n.CatID = c.CatID AND n.IsApproved = 1) AS NewsCount
    FROM Categories c
    LEFT JOIN Categories p ON c.ParentID = p.CatID
    WHERE (@IsActive IS NULL OR c.IsActive = @IsActive)
    ORDER BY ISNULL(c.ParentID, c.CatID), c.SortOrder, c.CatName;
END;
GO

-- ------------------------------------------------------------
-- C2. sp_AddCategory — Thêm chuyên mục
-- ------------------------------------------------------------
CREATE PROCEDURE sp_AddCategory
    @CatName     NVARCHAR(150),
    @Slug        VARCHAR(200),
    @ParentID    INT = NULL,
    @Description NVARCHAR(500) = NULL,
    @SortOrder   INT = 0,
    @NewCatID    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Categories WHERE Slug = @Slug)
    BEGIN
        SET @NewCatID = -1; -- Slug đã tồn tại
        RETURN;
    END;
    INSERT INTO Categories (CatName, Slug, ParentID, Description, SortOrder)
    VALUES (@CatName, @Slug, @ParentID, @Description, @SortOrder);
    SET @NewCatID = SCOPE_IDENTITY();
END;
GO

-- ------------------------------------------------------------
-- C3. sp_UpdateCategory
-- ------------------------------------------------------------
CREATE PROCEDURE sp_UpdateCategory
    @CatID       INT,
    @CatName     NVARCHAR(150),
    @Slug        VARCHAR(200),
    @ParentID    INT = NULL,
    @Description NVARCHAR(500) = NULL,
    @SortOrder   INT = 0,
    @IsActive    BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    -- Không cho đặt ParentID là chính mình hoặc con của mình
    IF @ParentID = @CatID
    BEGIN RAISERROR('Không thể chọn chính nó làm chuyên mục cha.', 16, 1); RETURN; END;

    UPDATE Categories
    SET CatName     = @CatName,
        Slug        = @Slug,
        ParentID    = @ParentID,
        Description = @Description,
        SortOrder   = @SortOrder,
        IsActive    = @IsActive
    WHERE CatID = @CatID;
END;
GO

--  NHÓM D: STORED PROCEDURES — COMMENTS

-- ------------------------------------------------------------
-- D1. sp_GetCommentsByNews — Lấy bình luận theo bài viết
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetCommentsByNews
    @NewsID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM vw_CommentDetails
    WHERE NewsID    = @NewsID
      AND IsApproved = 1
      AND ParentID  IS NULL    -- Chỉ lấy bình luận gốc
    ORDER BY CreatedAt;
END;
GO

-- ------------------------------------------------------------
-- D2. sp_AddComment — Thêm bình luận
-- ------------------------------------------------------------
CREATE PROCEDURE sp_AddComment
    @NewsID     INT,
    @UserID     INT          = NULL,
    @GuestName  NVARCHAR(100) = NULL,
    @GuestEmail VARCHAR(150) = NULL,
    @Content    NVARCHAR(2000),
    @ParentID   INT          = NULL,
    @NewCmtID   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    -- Kiểm tra AllowComment
    IF NOT EXISTS (SELECT 1 FROM News WHERE NewsID = @NewsID AND AllowComment = 1 AND IsApproved = 1)
    BEGIN
        SET @NewCmtID = -1;
        RETURN;
    END;

    INSERT INTO Comments (NewsID, UserID, GuestName, GuestEmail, Content, IsApproved, ParentID)
    VALUES (@NewsID, @UserID, @GuestName, @GuestEmail, @Content, 0, @ParentID);
    SET @NewCmtID = SCOPE_IDENTITY();
END;
GO

-- ------------------------------------------------------------
-- D3. sp_ApproveComment — Duyệt bình luận
-- ------------------------------------------------------------
CREATE PROCEDURE sp_ApproveComment
    @CmtID     INT,
    @IsApproved BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Comments SET IsApproved = @IsApproved WHERE CmtID = @CmtID;
END;
GO

--  NHÓM E: STORED PROCEDURES — NEWSLETTER

-- ------------------------------------------------------------
-- E1. sp_SubscribeNewsletter — Đăng ký nhận bản tin
-- ------------------------------------------------------------
CREATE PROCEDURE sp_SubscribeNewsletter
    @Email       VARCHAR(150),
    @FullName    NVARCHAR(150) = NULL,
    @Token       VARCHAR(100),   -- Confirm token (GUID)
    @UnsubToken  VARCHAR(100),   -- Unsubscribe token (GUID)
    @Result      INT OUTPUT      -- 1=OK mới, 2=Đã tồn tại, -1=Lỗi
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Newsletter WHERE Email = @Email AND IsConfirmed = 1)
    BEGIN
        SET @Result = 2; -- Đã đăng ký và xác nhận
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM Newsletter WHERE Email = @Email)
    BEGIN
        -- Gửi lại email xác nhận
        UPDATE Newsletter
        SET ConfirmToken = @Token, FullName = ISNULL(@FullName, FullName)
        WHERE Email = @Email;
        SET @Result = 2;
        RETURN;
    END;

    INSERT INTO Newsletter (Email, FullName, IsActive, IsConfirmed, ConfirmToken, UnsubscribeToken)
    VALUES (@Email, @FullName, 0, 0, @Token, @UnsubToken);
    SET @Result = 1;
END;
GO

-- ------------------------------------------------------------
-- E2. sp_ConfirmNewsletter — Xác nhận đăng ký qua email
-- ------------------------------------------------------------
CREATE PROCEDURE sp_ConfirmNewsletter
    @Token  VARCHAR(100),
    @Result INT OUTPUT   -- 1=OK, -1=Token không hợp lệ
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Newsletter WHERE ConfirmToken = @Token AND IsConfirmed = 0)
    BEGIN
        UPDATE Newsletter
        SET IsActive    = 1,
            IsConfirmed = 1,
            ConfirmedAt = GETDATE()
        WHERE ConfirmToken = @Token;
        SET @Result = 1;
    END
    ELSE
        SET @Result = -1;
END;
GO

-- ------------------------------------------------------------
-- E3. sp_UnsubscribeNewsletter — Hủy đăng ký
-- ------------------------------------------------------------
CREATE PROCEDURE sp_UnsubscribeNewsletter
    @Token  VARCHAR(100),
    @Result INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Newsletter WHERE UnsubscribeToken = @Token)
    BEGIN
        UPDATE Newsletter SET IsActive = 0 WHERE UnsubscribeToken = @Token;
        SET @Result = 1;
    END
    ELSE
        SET @Result = -1;
END;
GO

-- ------------------------------------------------------------
-- E4. sp_GetActiveSubscribers — Lấy danh sách đăng ký để gửi mail
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetActiveSubscribers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SubID, Email, FullName, UnsubscribeToken
    FROM Newsletter
    WHERE IsActive = 1 AND IsConfirmed = 1
    ORDER BY SubscribedAt;
END;
GO

--  NHÓM F: STORED PROCEDURES — SHARE LOG

-- ------------------------------------------------------------
-- F1. sp_AddShareLog — Lưu log gửi tin cho bạn
-- ------------------------------------------------------------
CREATE PROCEDURE sp_AddShareLog
    @NewsID        INT,
    @SenderName    NVARCHAR(100),
    @SenderEmail   VARCHAR(150),
    @ReceiverEmail VARCHAR(150),
    @Message       NVARCHAR(500) = NULL,
    @IsSent        BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ShareLog (NewsID, SenderName, SenderEmail, ReceiverEmail, Message, IsSent)
    VALUES (@NewsID, @SenderName, @SenderEmail, @ReceiverEmail, @Message, @IsSent);
END;
GO

--  NHÓM G: STORED PROCEDURES — TAGS

-- ------------------------------------------------------------
-- G1. sp_AddTagsToNews — Gán tags cho bài viết
-- ------------------------------------------------------------
CREATE PROCEDURE sp_AddTagsToNews
    @NewsID   INT,
    @TagNames NVARCHAR(MAX)  -- Danh sách tên tag, phân cách bằng dấu phẩy
AS
BEGIN
    SET NOCOUNT ON;
    -- Xoá tags cũ
    DELETE FROM News_Tags WHERE NewsID = @NewsID;

    -- Parse và thêm tags mới
    DECLARE @TagName NVARCHAR(100);
    DECLARE @TagID   INT;
    DECLARE @TagSlug VARCHAR(150);

    DECLARE @tagTable TABLE (TagName NVARCHAR(100));
    INSERT INTO @tagTable
    SELECT LTRIM(RTRIM(value))
    FROM STRING_SPLIT(@TagNames, ',')
    WHERE LTRIM(RTRIM(value)) <> '';

    DECLARE tagCursor CURSOR FOR SELECT TagName FROM @tagTable;
    OPEN tagCursor;
    FETCH NEXT FROM tagCursor INTO @TagName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @TagSlug = LOWER(REPLACE(@TagName, ' ', '-'));

        -- Thêm tag nếu chưa tồn tại
        IF NOT EXISTS (SELECT 1 FROM Tags WHERE TagName = @TagName)
        BEGIN
            INSERT INTO Tags (TagName, Slug) VALUES (@TagName, @TagSlug);
            SET @TagID = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            SELECT @TagID = TagID FROM Tags WHERE TagName = @TagName;
        END;

        -- Gán tag cho bài viết
        IF NOT EXISTS (SELECT 1 FROM News_Tags WHERE NewsID = @NewsID AND TagID = @TagID)
            INSERT INTO News_Tags (NewsID, TagID) VALUES (@NewsID, @TagID);

        -- Cập nhật UseCount
        UPDATE Tags SET UseCount = (SELECT COUNT(*) FROM News_Tags WHERE TagID = @TagID)
        WHERE TagID = @TagID;

        FETCH NEXT FROM tagCursor INTO @TagName;
    END;

    CLOSE tagCursor;
    DEALLOCATE tagCursor;
END;
GO

-- ------------------------------------------------------------
-- G2. sp_GetTagsByNews — Lấy tags của bài viết
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetTagsByNews
    @NewsID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.TagID, t.TagName, t.Slug, t.UseCount
    FROM Tags t
    INNER JOIN News_Tags nt ON t.TagID = nt.TagID
    WHERE nt.NewsID = @NewsID
    ORDER BY t.TagName;
END;
GO

-- ------------------------------------------------------------
-- G3. sp_GetPopularTags — Top tags phổ biến
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetPopularTags
    @Top INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top) TagID, TagName, Slug, UseCount
    FROM Tags
    WHERE UseCount > 0
    ORDER BY UseCount DESC;
END;
GO

--  NHÓM H: STORED PROCEDURES — SETTINGS & STATS

-- ------------------------------------------------------------
-- H1. sp_GetSetting — Lấy cấu hình
-- ------------------------------------------------------------
CREATE PROCEDURE sp_GetSetting
    @Key VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SettingValue FROM Settings WHERE SettingKey = @Key;
END;
GO

-- ------------------------------------------------------------
-- H2. sp_SetSetting — Cập nhật cấu hình
-- ------------------------------------------------------------
CREATE PROCEDURE sp_SetSetting
    @Key   VARCHAR(100),
    @Value NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Settings WHERE SettingKey = @Key)
        UPDATE Settings SET SettingValue = @Value, UpdatedAt = GETDATE()
        WHERE SettingKey = @Key;
    ELSE
        INSERT INTO Settings (SettingKey, SettingValue) VALUES (@Key, @Value);
END;
GO

--  TRIGGERS

-- ------------------------------------------------------------
-- T1. trg_News_UpdatedAt — Tự cập nhật UpdatedAt khi sửa News
-- ------------------------------------------------------------
CREATE TRIGGER trg_News_UpdatedAt
ON News
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(UpdatedAt)   -- Tránh trigger tự kích hoạt lại
        UPDATE News
        SET UpdatedAt = GETDATE()
        WHERE NewsID IN (SELECT NewsID FROM inserted);
END;
GO

-- ------------------------------------------------------------
-- T2. trg_Tags_UseCount — Tự cập nhật UseCount của Tag
-- ------------------------------------------------------------
CREATE TRIGGER trg_Tags_UseCount
ON News_Tags
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Tags
    SET UseCount = (SELECT COUNT(*) FROM News_Tags WHERE TagID = Tags.TagID)
    WHERE TagID IN (
        SELECT TagID FROM inserted
        UNION
        SELECT TagID FROM deleted
    );
END;
GO



--   DỮ LIỆU MẪU (SEED DATA)

-- ------------------------------------------------------------
-- 7.1  Roles
-- ------------------------------------------------------------
INSERT INTO Roles (RoleName, Description) VALUES
    (N'Admin',  N'Quản trị viên: toàn quyền hệ thống'),
    (N'Editor', N'Biên tập viên: viết và quản lý bài của mình'),
    (N'Reader', N'Độc giả: đọc tin, bình luận, đăng ký newsletter');
GO

-- ------------------------------------------------------------
-- 7.2  Users (mật khẩu gốc: Admin@123 / Editor@123 / Reader@123)
--      Lưu ý: password trong thực tế phải được hash ở tầng C#
--      Ở đây để placeholder VARCHAR cho nhận biết
-- ------------------------------------------------------------
INSERT INTO Users (Username, Password, Salt, Email, FullName, RoleID, IsActive) VALUES
    ('admin',
     'de869070a1ecaf6335c3c1dfd9f90ebc3f906a4ca3ca462204b6f7bbb5963066',
     'qfsRzRkP2W7+bLjZpTn1ug==',
     'admin@baodientu.vn',
     N'Quản trị viên',
     1, 1),
    ('editor01',
     '4043c3ac7e0c88d2d3850fccba4b5702596a4f140a290ac72e120f85ec930e67',
     'Rs5kU6Qy2xK9+J52bbTx1A==',
     'editor01@baodientu.vn',
     N'Nguyễn Văn Biên Tập',
     2, 1),
    ('editor02',
     'f766c38045c869538ed649803e75baf20495000567745447d3718393c370a7c0',
     'V8t3ixIpl22FkbM42x+7mw==',
     'editor02@baodientu.vn',
     N'Trần Thị Thu Hà',
     2, 1),
    ('reader01',
     '85825ec1f599c5a8178106d60b943a9df3988380f60a28499212252a9f33c662',
     '6+Q5aXaWkhJClOQKy7sJGw==',
     'reader01@gmail.com',
     N'Lê Văn Đọc Báo',
     3, 1);
GO

-- ------------------------------------------------------------
-- 7.3  Settings — Cấu hình hệ thống
-- ------------------------------------------------------------
INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES
    ('SiteName',        N'Báo Điện Tử',         N'Tên website'),
    ('SiteDescription', N'Tin tức cập nhật 24/7', N'Mô tả website'),
    ('SiteUrl',         'http://localhost:8080',  N'URL website'),
    ('ContactEmail',    'contact@baodientu.vn',  N'Email liên hệ'),
    ('FacebookUrl',     'https://facebook.com',  N'Link Facebook'),
    ('NewsPerPage',     '10',                    N'Số tin mỗi trang'),
    ('FeaturedCount',   '6',                     N'Số tin nổi bật trang chủ'),
    ('AllowGuestComment','1',                    N'Cho phép khách bình luận'),
    ('CommentAutoApprove','0',                   N'Tự duyệt bình luận'),
    ('MaxUploadSizeMB', '5',                     N'Dung lượng upload tối đa (MB)'),
    ('AllowedImageExts','jpg,jpeg,png,gif,webp', N'Định dạng ảnh cho phép'),
    ('SMTP_Host',       'smtp.gmail.com',        N'SMTP server'),
    ('SMTP_Port',       '587',                   N'SMTP port'),
    ('SMTP_User',       '',                      N'SMTP username (email)'),
    ('SMTP_Pass',       '',                      N'SMTP password (app password)');
GO

-- ------------------------------------------------------------
-- 7.4  Categories — Chuyên mục mẫu
-- ------------------------------------------------------------
INSERT INTO Categories (CatName, Slug, ParentID, SortOrder, IsActive) VALUES
    (N'Chính trị',    'chinh-tri',    NULL, 1, 1),
    (N'Kinh tế',      'kinh-te',      NULL, 2, 1),
    (N'Xã hội',       'xa-hoi',       NULL, 3, 1),
    (N'Thế giới',     'the-gioi',     NULL, 4, 1),
    (N'Thể thao',     'the-thao',     NULL, 5, 1),
    (N'Giải trí',     'giai-tri',     NULL, 6, 1),
    (N'Công nghệ',    'cong-nghe',    NULL, 7, 1),
    (N'Giáo dục',     'giao-duc',     NULL, 8, 1),
    (N'Sức khỏe',     'suc-khoe',     NULL, 9, 1),
    (N'Du lịch',      'du-lich',      NULL, 10, 1);
GO

-- Danh mục con
DECLARE @KinhTeID  INT = (SELECT CatID FROM Categories WHERE Slug = 'kinh-te');
DECLARE @CongNgheID INT = (SELECT CatID FROM Categories WHERE Slug = 'cong-nghe');
DECLARE @TheThaoID INT = (SELECT CatID FROM Categories WHERE Slug = 'the-thao');

INSERT INTO Categories (CatName, Slug, ParentID, SortOrder, IsActive) VALUES
    (N'Chứng khoán',    'chung-khoan',  @KinhTeID,  1, 1),
    (N'Bất động sản',   'bat-dong-san', @KinhTeID,  2, 1),
    (N'Doanh nghiệp',   'doanh-nghiep', @KinhTeID,  3, 1),
    (N'Công nghệ thông tin', 'cntt',    @CongNgheID, 1, 1),
    (N'Điện tử - Điện máy','dien-tu',   @CongNgheID, 2, 1),
    (N'Bóng đá',        'bong-da',      @TheThaoID, 1, 1),
    (N'Tennis',         'tennis',       @TheThaoID, 2, 1),
    (N'Cầu lông',       'cau-long',     @TheThaoID, 3, 1);
GO

-- ------------------------------------------------------------
-- 7.5  Tags — Thẻ từ khóa mẫu
-- ------------------------------------------------------------
INSERT INTO Tags (TagName, Slug) VALUES
    (N'Chính phủ',          'chinh-phu'),
    (N'Kinh tế vĩ mô',      'kinh-te-vi-mo'),
    (N'Trí tuệ nhân tạo',   'tri-tue-nhan-tao'),
    (N'Bóng đá Việt Nam',   'bong-da-viet-nam'),
    (N'COVID-19',            'covid-19'),
    (N'Bầu cử',              'bau-cu'),
    (N'Startup',             'startup'),
    (N'Blockchain',          'blockchain'),
    (N'Giáo dục đại học',   'giao-duc-dai-hoc'),
    (N'Môi trường',          'moi-truong');
GO

-- ------------------------------------------------------------
-- 7.6  News — Tin tức mẫu (đã duyệt)
-- ------------------------------------------------------------
DECLARE @EditorID1 INT = (SELECT UserID FROM Users WHERE Username = 'editor01');
DECLARE @EditorID2 INT = (SELECT UserID FROM Users WHERE Username = 'editor02');
DECLARE @AdminID   INT = (SELECT UserID FROM Users WHERE Username = 'admin');
DECLARE @CatCNTT   INT = (SELECT CatID FROM Categories WHERE Slug = 'cntt');
DECLARE @CatKinhTe INT = (SELECT CatID FROM Categories WHERE Slug = 'kinh-te');
DECLARE @CatBongDa INT = (SELECT CatID FROM Categories WHERE Slug = 'bong-da');
DECLARE @CatXaHoi  INT = (SELECT CatID FROM Categories WHERE Slug = 'xa-hoi');
DECLARE @CatCongNghe INT = (SELECT CatID FROM Categories WHERE Slug = 'cong-nghe');

INSERT INTO News
    (Title, Slug, Summary, Content, Thumbnail, AuthorID, CatID,
     Status, IsApproved, ApprovedBy, ApprovedAt, IsFeatured, IsHot,
     ViewCount, AllowComment, PublishedAt)
VALUES
(
    N'Trí tuệ nhân tạo và tương lai của ngành báo chí Việt Nam',
    'tri-tue-nhan-tao-tuong-lai-bao-chi-viet-nam-202401',
    N'Sự phát triển mạnh mẽ của AI đang đặt ra nhiều câu hỏi về tương lai của ngành báo chí truyền thống. Liệu AI có thay thế được phóng viên?',
    N'<p>Trong bối cảnh công nghệ phát triển như vũ bão, <strong>Trí tuệ Nhân tạo (AI)</strong> đang dần xâm nhập vào mọi lĩnh vực, trong đó có báo chí.</p>
     <h2>1. AI trong phòng tin tức</h2>
     <p>Các tòa soạn lớn trên thế giới như Reuters, Associated Press đã bắt đầu sử dụng AI để tự động tạo ra các bài báo tài chính, thể thao từ dữ liệu thô. Điều này giúp tiết kiệm đáng kể thời gian và chi phí.</p>
     <h2>2. Cơ hội và thách thức</h2>
     <p>Với ngành báo chí Việt Nam, AI mở ra cơ hội lớn trong việc xử lý lượng thông tin khổng lồ, cá nhân hóa nội dung cho độc giả và phát hiện tin giả. Tuy nhiên, việc kiểm soát chất lượng và đạo đức báo chí vẫn là thách thức không nhỏ.</p>
     <p>Các chuyên gia cho rằng, AI sẽ là <em>công cụ hỗ trợ</em> chứ không thể thay thế hoàn toàn phóng viên, đặc biệt trong các lĩnh vực đòi hỏi tư duy phân tích sâu và cảm xúc con người.</p>',
    'https://images.unsplash.com/photo-1677442136019-21780ecad995?auto=format&fit=crop&w=1200&q=80',
    @EditorID1, @CatCNTT,
    2, 1, @AdminID, GETDATE(), 1, 1,
    1520, 1, DATEADD(DAY, -2, GETDATE())
),
(
    N'Kinh tế Việt Nam tăng trưởng vượt mục tiêu trong quý I năm 2024',
    'kinh-te-viet-nam-tang-truong-quy-1-2024',
    N'GDP quý I/2024 tăng 5,87%, vượt mục tiêu đề ra. Đây là tín hiệu tích cực cho cả năm 2024 với nhiều điểm sáng từ sản xuất công nghiệp và xuất khẩu.',
    N'<p>Theo số liệu vừa được Tổng cục Thống kê công bố, <strong>GDP quý I/2024 của Việt Nam tăng 5,87%</strong> so với cùng kỳ năm trước, vượt mức mục tiêu 5,5% đề ra.</p>
     <h2>Các điểm sáng của nền kinh tế</h2>
     <p>Khu vực công nghiệp và xây dựng tiếp tục là động lực tăng trưởng chính với mức tăng 6,28%. Đặc biệt, ngành chế biến chế tạo ghi nhận mức tăng 6,98%.</p>
     <p>Xuất khẩu hàng hóa đạt 93,06 tỷ USD, tăng 17% so với cùng kỳ. Trong đó, điện thoại và linh kiện tiếp tục dẫn đầu với kim ngạch 14,8 tỷ USD.</p>
     <h2>Dự báo cả năm 2024</h2>
     <p>Các chuyên gia kinh tế nhận định, với đà tăng trưởng tích cực này, mục tiêu GDP cả năm 2024 đạt 6-6,5% là hoàn toàn khả thi nếu Chính phủ tiếp tục duy trì các chính sách hỗ trợ doanh nghiệp.</p>',
    'https://images.unsplash.com/photo-1520607162513-77705c0f0d4a?auto=format&fit=crop&w=1200&q=80',
    @EditorID2, @CatKinhTe,
    2, 1, @AdminID, GETDATE(), 1, 1,
    2340, 1, DATEADD(DAY, -1, GETDATE())
),
(
    N'Đội tuyển Việt Nam chuẩn bị cho vòng loại World Cup 2026',
    'doi-tuyen-viet-nam-vong-loai-world-cup-2026',
    N'HLV trưởng Kim Sang-sik đã công bố danh sách 30 cầu thủ tham dự vòng loại thứ 3 World Cup 2026 khu vực châu Á. Nhiều gương mặt trẻ được gọi lên.',
    N'<p>Huấn luyện viên trưởng <strong>Kim Sang-sik</strong> vừa chính thức công bố danh sách 30 cầu thủ được triệu tập lên đội tuyển quốc gia Việt Nam cho giai đoạn vòng loại cuối World Cup 2026.</p>
     <h2>Điểm nhấn trong danh sách</h2>
     <p>Đáng chú ý, nhiều cầu thủ trẻ từ các giải U23 và V.League được gọi lên lần đầu. Tiêu biểu là tiền đạo Nguyễn Tuấn Tài (21 tuổi, HAGL) với phong độ ấn tượng trong mùa giải V.League 2024.</p>
     <p>Các trụ cột như thủ thành Đặng Văn Lâm, hậu vệ Bùi Hoàng Việt Anh và tiền vệ Nguyễn Quang Hải vẫn có mặt trong danh sách.</p>
     <h2>Lịch thi đấu sắp tới</h2>
     <p>Việt Nam sẽ có 2 trận đấu quan trọng trong tháng tới, tiếp đón Iraq và làm khách trước Iraq. Đây là 2 trận mang tính chất quyết định cho cơ hội đi tiếp của đội tuyển.</p>',
    'https://images.unsplash.com/photo-1431324155629-1a6deb1dec8d?auto=format&fit=crop&w=1200&q=80',
    @EditorID1, @CatBongDa,
    2, 1, @AdminID, GETDATE(), 0, 1,
    3100, 1, DATEADD(HOUR, -5, GETDATE())
),
(
    N'Hà Nội triển khai hệ thống giao thông thông minh tại 50 nút giao',
    'ha-noi-giao-thong-thong-minh-50-nut-giao',
    N'Thành phố Hà Nội vừa hoàn thành lắp đặt camera AI và hệ thống đèn tín hiệu thông minh tại 50 nút giao thông trọng điểm, giảm ùn tắc đáng kể.',
    N'<p>UBND thành phố Hà Nội vừa chính thức khai trương hệ thống <strong>Giao thông Thông minh (ITS)</strong> giai đoạn 1 tại 50 nút giao thông trọng điểm trên địa bàn.</p>
     <h2>Công nghệ được áp dụng</h2>
     <p>Hệ thống sử dụng camera AI có khả năng nhận diện biển số xe, phát hiện vi phạm giao thông và phân tích mật độ phương tiện theo thời gian thực. Dữ liệu được truyền về Trung tâm Điều hành Giao thông đặt tại Sở Giao thông Vận tải.</p>
     <p>Đèn tín hiệu giao thông được trang bị bộ điều khiển thông minh, tự động điều chỉnh thời gian xanh-đỏ dựa trên lưu lượng phương tiện thực tế.</p>
     <h2>Kết quả bước đầu</h2>
     <p>Sau 1 tháng vận hành thử nghiệm, thời gian di chuyển trung bình tại các tuyến đường thí điểm giảm 15-20%. Hệ thống đã phát hiện và xử lý hơn 5.000 trường hợp vi phạm giao thông.</p>',
    'https://images.unsplash.com/photo-1449824913935-59a10b8d2000?auto=format&fit=crop&w=1200&q=80',
    @EditorID2, @CatXaHoi,
    2, 1, @AdminID, GETDATE(), 1, 0,
    987, 1, DATEADD(HOUR, -12, GETDATE())
),
(
    N'Top 10 xu hướng công nghệ nổi bật năm 2024',
    'top-10-xu-huong-cong-nghe-2024',
    N'Từ Generative AI, điện toán lượng tử đến xe điện và metaverse, năm 2024 chứng kiến sự bứt phá của hàng loạt công nghệ tiên tiến định hình lại thế giới.',
    N'<p>Năm 2024 được giới chuyên gia công nghệ đánh giá là năm của sự bùng nổ với nhiều đột phá quan trọng. Dưới đây là 10 xu hướng nổi bật nhất.</p>
     <h2>1. Generative AI trở thành công cụ phổ biến</h2>
     <p>Các mô hình AI tạo sinh như GPT-4, Claude, Gemini không còn chỉ là sản phẩm thử nghiệm mà đã được tích hợp vào hàng triệu ứng dụng thực tế.</p>
     <h2>2. Điện toán lượng tử (Quantum Computing)</h2>
     <p>Google và IBM đã đạt được những cột mốc quan trọng trong việc tăng số lượng qubit và giảm tỷ lệ lỗi, tiến gần hơn đến máy tính lượng tử thực dụng.</p>
     <h2>3. Xe điện và hạ tầng sạc</h2>
     <p>Thị phần xe điện toàn cầu đạt 18% trong Q1/2024. Hạ tầng trạm sạc nhanh đang được đầu tư mạnh tại Đông Nam Á, trong đó có Việt Nam.</p>
     <h2>4-10. Các xu hướng khác</h2>
     <p>Blockchain trong chuỗi cung ứng, IoT y tế, robot tự hành, thực tế tăng cường (AR), bảo mật Zero Trust, edge computing và điện toán sinh học đều có những bước tiến đáng kể trong năm qua.</p>',
    'https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=1200&q=80',
    @EditorID1, @CatCongNghe,
    2, 1, @AdminID, GETDATE(), 1, 0,
    2750, 1, DATEADD(DAY, -3, GETDATE())
);
GO

-- ------------------------------------------------------------
-- 7.7  Comments — Bình luận mẫu
-- ------------------------------------------------------------
DECLARE @NewsID1 INT = (SELECT NewsID FROM News WHERE Slug = 'tri-tue-nhan-tao-tuong-lai-bao-chi-viet-nam-202401');
DECLARE @NewsID2 INT = (SELECT NewsID FROM News WHERE Slug = 'kinh-te-viet-nam-tang-truong-quy-1-2024');
DECLARE @ReaderID INT = (SELECT UserID FROM Users WHERE Username = 'reader01');

INSERT INTO Comments (NewsID, UserID, GuestName, GuestEmail, Content, IsApproved) VALUES
    (@NewsID1, @ReaderID, NULL, NULL,
     N'Bài viết rất hay và có chiều sâu! AI đang thay đổi mọi ngành nghề, báo chí cũng không ngoại lệ.', 1),
    (@NewsID1, NULL, N'Nguyễn Minh Tuấn', 'minhtuan@gmail.com',
     N'Tôi đồng ý với quan điểm AI chỉ là công cụ hỗ trợ. Cảm xúc và tư duy phê phán vẫn là điều con người làm tốt hơn máy móc.', 1),
    (@NewsID1, NULL, N'Phạm Lan Hương', 'lhuong@yahoo.com',
     N'Bài viết cần cập nhật thêm ví dụ về các tòa soạn Việt Nam đang ứng dụng AI.', 1),
    (@NewsID2, @ReaderID, NULL, NULL,
     N'Tin vui! Hy vọng tăng trưởng kinh tế sẽ tạo thêm nhiều việc làm cho người lao động.', 1),
    (@NewsID2, NULL, N'Trần Đức Anh', 'ducanhkt@gmail.com',
     N'Số liệu xuất khẩu ấn tượng. Cần đa dạng hóa thêm thị trường để giảm rủi ro phụ thuộc.', 1);
GO

-- ------------------------------------------------------------
-- 7.8  Newsletter subscribers mẫu
-- ------------------------------------------------------------
INSERT INTO Newsletter (Email, FullName, IsActive, IsConfirmed,
                        ConfirmToken, UnsubscribeToken, ConfirmedAt) VALUES
    ('subscriber1@gmail.com', N'Lê Thị Mai',     1, 1,
     NEWID(), NEWID(), DATEADD(DAY, -10, GETDATE())),
    ('subscriber2@gmail.com', N'Hoàng Văn Nam',  1, 1,
     NEWID(), NEWID(), DATEADD(DAY, -5, GETDATE())),
    ('subscriber3@yahoo.com', N'Phan Thị Lan',   1, 1,
     NEWID(), NEWID(), DATEADD(DAY, -2, GETDATE())),
    ('subscriber4@outlook.com', N'Đỗ Mạnh Hùng', 0, 0,
     NEWID(), NEWID(), NULL);  -- Chưa xác nhận email
GO

--  KIỂM TRA DỮ LIỆU (VERIFICATION QUERIES)

PRINT '============================================';
PRINT '  KIỂM TRA DATABASE BÁO ĐIỆN TỬ';
PRINT '============================================';

PRINT '--- Bảng và số dòng ---';
SELECT 'Roles'      AS [Table], COUNT(*) AS [Rows] FROM Roles
UNION ALL SELECT 'Users',      COUNT(*) FROM Users
UNION ALL SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL SELECT 'News',       COUNT(*) FROM News
UNION ALL SELECT 'Tags',       COUNT(*) FROM Tags
UNION ALL SELECT 'Comments',   COUNT(*) FROM Comments
UNION ALL SELECT 'Newsletter', COUNT(*) FROM Newsletter
UNION ALL SELECT 'Settings',   COUNT(*) FROM Settings;

PRINT '--- Stored Procedures ---';
SELECT name AS [Stored Procedure]
FROM sys.objects
WHERE type = 'P' AND name LIKE 'sp_%'
ORDER BY name;

PRINT '--- Views ---';
SELECT name AS [View]
FROM sys.objects
WHERE type = 'V'
ORDER BY name;

PRINT '--- Tin tức mẫu ---';
SELECT n.NewsID, n.Title, c.CatName,
       u.FullName AS Author, n.ViewCount,
       CASE n.Status
           WHEN 0 THEN 'Draft'
           WHEN 1 THEN 'Pending'
           WHEN 2 THEN 'Approved'
           WHEN 3 THEN 'Rejected'
       END AS StatusLabel
FROM News n
INNER JOIN Users u      ON n.AuthorID = u.UserID
INNER JOIN Categories c ON n.CatID    = c.CatID;

PRINT '  Lưu ý: Cập nhật SMTP_User và SMTP_Pass trong bảng Settings';
PRINT '          trước khi chạy chức năng gửi email!';
GO
