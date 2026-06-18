# RoomBuildingService - Huong dan bat service

Tai lieu nay chi danh cho phan `RoomBuildingService` cua nhom 1.

## 1. Muc tieu

Service nay phu trach:

- toa nha
- loai phong
- phong
- giuong
- thiet bi
- phong kha dung

Frontend `Qly_ktx` se goi API cua service nay.

## 2. Chuan bi

Can co:

- .NET SDK phu hop voi project
- SQL Server hoac SQL Somee
- neu demo qua Render: can them `ngrok`

## 3. Cau hinh database

File cau hinh:

```text
src/RoomBuildingService.Api/appsettings.json
```

Connection string dang dung Somee:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=RoommDB.mssql.somee.com;Database=RoommDB;User Id=quang19911_SQLLogin_2;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

Neu doi database thi sua lai `DefaultConnection`.

## 4. Cach bat backend local

Mo terminal tai thu muc:

```powershell
cd RoomBuildingService/src/RoomBuildingService.Api
```

Chay service:

```powershell
dotnet run
```

Mac dinh service se chay o port:

```text
http://localhost:5285
```

Swagger:

```text
http://localhost:5285/swagger
```

## 5. Cach demo cung frontend local

Neu frontend `Qly_ktx` cung chay tren may trong cung mang LAN, chi can:

1. bat backend:

```powershell
dotnet run
```

2. dam bao frontend dang dung:

```env
VITE_ROOM_BUILDING_API_URL=http://192.168.24.194:5285
```

3. bat frontend local

Luc nay khong can `ngrok`.

## 6. Cach demo voi frontend tren Render

Neu frontend dang deploy tren Render thi chi bat `dotnet run` la khong du.

Can lam them `ngrok` de Render goi duoc backend tren may local.

### Buoc 1: bat backend

```powershell
cd RoomBuildingService/src/RoomBuildingService.Api
dotnet run
```

### Buoc 2: bat ngrok

Neu da dung tai khoan ngrok va add authtoken xong:

```powershell
ngrok http 5285
```

Neu dang o ngay trong thu muc chua `ngrok.exe`:

```powershell
.\ngrok.exe http 5285
```

Ngrok se tra ve URL public dang:

```text
https://xxxxx.ngrok-free.dev
```

### Buoc 3: gan vao Render

Tai Render, them environment variable:

```env
VITE_ROOM_BUILDING_PUBLIC_API_URL=https://xxxxx.ngrok-free.dev
```

Sau do redeploy frontend.

## 7. Khi demo can bat gi

### Truong hop 1 - demo local

Can bat:

- backend `dotnet run`
- frontend local

Khong can `ngrok`.

### Truong hop 2 - demo frontend Render

Can bat:

- backend `dotnet run`
- `ngrok http 5285`

Neu tat 1 trong 2 cai nay thi frontend tren Render se mat ket noi API.

## 8. Kiem tra nhanh

Sau khi bat backend:

```text
http://localhost:5285/swagger
```

Sau khi bat ngrok:

```text
https://your-ngrok-url/swagger
```

Neu mo duoc Swagger thi frontend se goi API duoc.

## 9. Ghi chu

- ban free cua ngrok thuong doi URL moi khi restart
- moi lan doi URL ngrok can cap nhat lai bien tren Render
- khong nen push password that cua database len GitHub public
