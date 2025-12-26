# 📥 راهنمای Import Excel با Query SQL

## 🚀 استفاده سریع

### مرحله 1: باز کردن Query Editor

1. **SQL Server Management Studio** را باز کنید
2. به دیتابیس **KhanoumiCore** متصل شوید
3. **New Query** را کلیک کنید

### مرحله 2: باز کردن اسکریپت

1. فایل `ImportExcel_Query_Simple.sql` را باز کنید
2. یا محتوای آن را در Query Editor کپی کنید

### مرحله 3: تنظیم مسیر فایل

در خط 15، مسیر فایل Excel خود را وارد کنید:

```sql
DECLARE @ExcelFilePath NVARCHAR(500) = N'C:\Users\YourUsername\Desktop\یلدا.xlsx'
```

**مثال:**
```sql
DECLARE @ExcelFilePath NVARCHAR(500) = N'C:\Users\Admin\Desktop\یلدا.xlsx'
```

### مرحله 4: اجرا

**F5** را بزنید یا روی **Execute** کلیک کنید

---

## ⚠️ نکات مهم

### 1. مسیر فایل

- مسیر باید **کامل** باشد
- از **N'** قبل از مسیر استفاده کنید (برای پشتیبانی از Unicode)
- اگر نام کاربری Desktop شما متفاوت است، آن را تغییر دهید

**نحوه پیدا کردن مسیر Desktop:**
- در File Explorer به Desktop بروید
- آدرس را از نوار آدرس کپی کنید
- نام فایل Excel را اضافه کنید

### 2. نام Sheet

اگر نام Sheet شما `Sheet1` نیست، در خط 16 تغییر دهید:

```sql
DECLARE @SheetName NVARCHAR(100) = N'Sheet2$'  -- اگر Sheet شما Sheet2 است
```

### 3. ساختار فایل Excel

فایل Excel باید این ساختار را داشته باشد:

| ستون | نام فارسی | الزامی |
|------|-----------|--------|
| A | کد ملی | ✅ |
| B | تلفن همراه | ✅ |
| C | نام و نام خانوادگی | ❌ |
| D | کد پرسنلی | ❌ |

---

## ❌ خطاهای رایج

### خطا: "Microsoft.ACE.OLEDB.12.0 has not been registered"

**معنی:** Microsoft Access Database Engine نصب نیست

**راه‌حل:**
1. دانلود Access Database Engine:
   https://www.microsoft.com/en-us/download/details.aspx?id=54920
2. نسخه مناسب را نصب کنید (64-bit یا 32-bit)
3. SQL Server را Restart کنید
4. اسکریپت را دوباره اجرا کنید

**یا از روش‌های جایگزین استفاده کنید:**
- SQL Server Import Wizard
- اسکریپت PowerShell (`ImportExcel_Simple.ps1`)

### خطا: "Cannot open file"

**راه‌حل:**
1. مطمئن شوید فایل Excel **بسته** است
2. مسیر فایل را دوباره بررسی کنید
3. مطمئن شوید فایل در Desktop قرار دارد

### خطا: "Invalid column name"

**راه‌حل:**
1. نام ستون‌های Excel را بررسی کنید
2. باید دقیقاً این نام‌ها باشند: `کد ملی`, `تلفن همراه`, `نام و نام خانوادگی`, `کد پرسنلی`
3. نام ستون‌ها باید در **ردیف اول** باشد

---

## 📋 فایل‌های موجود

1. **`ImportExcel_Query_Simple.sql`** - نسخه ساده (فقط 30 خط)
2. **`ImportExcel_Query.sql`** - نسخه کامل با بررسی و گزارش

---

## ✅ بعد از Import

بعد از اجرا، این کوئری را برای بررسی اجرا کنید:

```sql
-- تعداد کل رکوردها
SELECT COUNT(*) AS TotalRecords 
FROM [dbo].[GiftCardEligibleUsers];

-- بررسی تکراری‌ها
SELECT NationalCode, COUNT(*) AS Count
FROM [dbo].[GiftCardEligibleUsers]
GROUP BY NationalCode
HAVING COUNT(*) > 1;

-- نمایش نمونه
SELECT TOP 10 *
FROM [dbo].[GiftCardEligibleUsers]
ORDER BY CreatedAtUtc DESC;
```

---

## 💡 نکات

1. ✅ فایل Excel باید **بسته** باشد
2. ✅ مسیر فایل باید **کامل** باشد
3. ✅ نام ستون‌ها باید **دقیقاً** مطابق باشد
4. ✅ اگر Access Database Engine نصب نیست، از PowerShell استفاده کنید







