# WebBaoDienTu

Du an Bao Dien Tu duoc xay dung bang ASP.NET Web Forms (.NET Framework 4.8), ADO.NET va SQL Server. He thong di theo mo hinh 3 lop: Presentation (`Pages`, `Admin`, `Editor`, `User`), Business Logic (`Code/BLL`) va Data Access (`Code/DAL`).

## Cong nghe

- ASP.NET Web Forms, C#, .NET Framework 4.8
- SQL Server Express/SQL Server, stored procedures va views
- ADO.NET voi `SqlParameter`
- Bootstrap 5, CSS rieng tai `Static/css/site.css`
- CKEditor 5 cho form soan bai va newsletter
- Forms Authentication + Session

## Chuc nang chinh

### Trang cong khai

- Trang chu hien thi tin noi bat, tin doc nhieu, tin moi nhat va chuyen muc.
- Danh sach tin theo chuyen muc, phan trang.
- Tim kiem theo tu khoa.
- Xem tin theo tag.
- Chi tiet bai viet voi noi dung HTML, tag, tin lien quan, in bai viet.
- Luot xem tang khi mo trang chi tiet va trang chu/danh sach lay lai so moi nhat khi tai lai.
- Binh luan bai viet: khach nhap ho ten/email, thanh vien da dang nhap dung thong tin tai khoan; binh luan cho admin duyet truoc khi hien thi.
- Gui tin cho ban qua email va ghi log chia se.
- Dang ky newsletter, xac nhan email, huy dang ky bang token.

### Tai khoan nguoi dung

- Dang ky tai khoan doc gia.
- Dang nhap/dang xuat.
- Phan quyen Admin, Editor, Reader.
- Cap nhat ho so ca nhan.
- Doi mat khau.
- Khoa tai khoan tam thoi khi dang nhap sai nhieu lan theo logic trong database.

### Khu Editor

- Xem danh sach bai viet cua minh.
- Viet bai moi, sua bai hien co.
- Nhap tieu de, tom tat, noi dung HTML, anh dai dien, chuyen muc, tag.
- Upload anh trong CKEditor qua `Handlers/UploadImage.ashx`.
- Bai moi/cap nhat duoc gui vao trang thai cho duyet.

### Khu Admin

- Dashboard thong ke: bai da xuat ban, bai cho duyet, user, subscriber, binh luan cho duyet, tong luot xem.
- Quan ly bai viet: xem, sua, duyet, tu choi, xoa.
- Duyet bai cho xuat ban.
- Quan ly chuyen muc va trang thai hien/an.
- Quan ly nguoi dung va khoa/mo tai khoan.
- Quan ly binh luan: duyet, an, xoa.
- Quan ly newsletter va gui ban tin cho subscriber da xac nhan.
- Cau hinh he thong: `SiteName`, `SiteUrl`, `ContactEmail`, `SMTP_*`.

## Cau truc thu muc

```text
baoDienTu/
  Admin/                 Trang quan tri
  Editor/                Trang bien tap vien
  User/                  Ho so va doi mat khau
  Pages/                 Trang public thuc te
  MasterPages/           Layout dung chung va layout trang in
  Controls/              User controls dung chung
  Code/BLL/              Xu ly nghiep vu
  Code/DAL/              Truy cap database
  Code/Helpers/          Helper UI, auth, upload, paging, security
  Code/Models/           DTO/model
  Database/              Script tao va seed database
  Docs/                  Tai lieu phan tich/thiet ke
  Handlers/              AJAX/upload/share/view handlers
  Static/css/            CSS giao dien
  Static/images/         Favicon va anh tinh
```

Luu y: URL public nhu `Default.aspx`, `NewsDetail.aspx`, `Login.aspx` duoc map trong `App_Start/RouteConfig.cs` sang file tuong ung trong `Pages/`. Root project chi giu cac file bat buoc cua Web Forms/IIS nhu `Web.config`, `Global.asax`, `.csproj`, `.sln`.

## Cai dat va chay du an

1. Mo solution:

```text
baoDienTu/baoDienTu.sln
```

2. Tao database bang SQL Server Management Studio hoac `sqlcmd`:

```text
baoDienTu/Database/BaoDienTu_Database.sql
```

3. Kiem tra connection string trong `baoDienTu/Web.config`:

```xml
<add name="BaoDienTuDB"
     connectionString="Data Source=localhost\SQLEXPRESS;Initial Catalog=BaoDienTuDB;Integrated Security=True;MultipleActiveResultSets=True"
     providerName="System.Data.SqlClient" />
```

4. Build solution trong Visual Studio 2022 hoac MSBuild.

5. Chay bang IIS Express/Visual Studio. URL mac dinh trong seed setting la:

```text
http://localhost:8080
```

Neu chay port khac, cap nhat `SiteUrl` trong Admin > Cau hinh hoac bang `Settings` de link email dung domain/port.

## Tai khoan mau

| Vai tro | Username | Password |
| --- | --- | --- |
| Admin | `admin` | `Admin@123` |
| Editor | `editor01` | `Editor@123` |
| Editor | `editor02` | `Editor@123` |
| Reader | `reader01` | `Reader@123` |

## Cau hinh email SMTP

Chuc nang gui tin cho ban va newsletter can SMTP. Seed database hien de trong:

- `SMTP_User`
- `SMTP_Pass`

Khi chua cau hinh, form van hoat dong va se tra thong bao loi cau hinh SMTP. De gui email that, cap nhat trong Admin > Cau hinh:

- `SMTP_Host`: vi du `smtp.gmail.com`
- `SMTP_Port`: vi du `587`
- `SMTP_User`: email gui
- `SMTP_Pass`: app password
- `SiteUrl`: URL dang chay cua website

## Checklist kiem thu nhanh

- Trang chu hien thi tin noi bat, doc nhieu, tin moi nhat va chuyen muc.
- Mo chi tiet bai viet, quay lai/tai lai trang chu, so luot xem duoc cap nhat theo database.
- Gui binh luan khach, thay thong bao cho duyet; vao Admin > Binh luan de duyet; quay lai chi tiet de thay binh luan.
- Form gui tin cho ban bao loi email rong/sai; khi SMTP trong thi bao loi cau hinh; khi SMTP dung thi gui mail va ghi log `ShareLog`.
- Dang nhap Admin vao dashboard va cac trang quan ly.
- Dang nhap Editor viet/sua bai va cho Admin duyet.
- Dang ky newsletter, xac nhan token va huy dang ky bang link email.
- Kiem tra responsive o mobile/tablet/desktop, dac biet header, thanh chuyen muc, search va footer.

## Ghi chu bao tri

- Khong doi schema database khi chi sua UI/behavior nho.
- Cac URL public hien tai duoc giu nguyen: `Default.aspx`, `NewsDetail.aspx?slug=...`, `NewsList.aspx?cat=...`, `Search.aspx?q=...`.
- `Handlers/IncreaseView.ashx` van duoc giu tuong thich, nhung trang chu khong goi handler nay de tranh tang view trung.
- `Handlers/ShareNews.ashx` nhan `application/x-www-form-urlencoded` va tra JSON `{ success, message }`.
