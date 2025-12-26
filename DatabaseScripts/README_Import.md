# 📥 راهنمای Import لیست Excel به جدول GiftCardEligibleUsers

## 📋 فهرست فایل‌ها

1. **`ImportExcelToGiftCardEligibleUsers.sql`** - اسکریپت SQL کامل با 4 روش مختلف
2. **`QuickImportGuide.md`** - راهنمای سریع و ساده
3. **`ImportExcelToSQL.ps1`** - اسکریپت PowerShell برای Import خودکار

---

## 🚀 روش سریع (پیشنهادی)

### استفاده از SQL Server Import Wizard

1. فایل Excel را آماده کنید (ستون‌ها: کد ملی، تلفن همراه، نام و نام خانوادگی، کد پرسنلی)
2. SQL Server Management Studio را باز کنید
3. روی دیتابیس **KhanoumiCore** راست کلیک کنید
4. **Tasks** → **Import Data...**
5. مراحل را دنبال کنید (راهنمای کامل در `QuickImportGuide.md`)

---

## 💻 استفاده از اسکریپت PowerShell

### پیش‌نیازها

```powershell
# نصب ماژول ImportExcel
Install-Module -Name ImportExcel -Force -Scope CurrentUser
```

### اجرای اسکریپت

```powershell
# روش ساده (با پارامترهای پیش‌فرض)
.\ImportExcelToSQL.ps1 -ExcelFilePath "C:\Path\To\Your\File.xlsx"

# روش کامل (با مشخص کردن تمام پارامترها)
.\ImportExcelToSQL.ps1 `
    -ExcelFilePath "C:\Path\To\Your\File.xlsx" `
    -ServerName "192.168.87.11" `
    -DatabaseName "KhanoumiCore" `
    -UserName "trans" `
    -Password "t@Rrn!r46T5" `
    -SheetName "Sheet1"
```

### مزایای استفاده از PowerShell

- ✅ Validation خودکار داده‌ها
- ✅ بررسی تکراری بودن کدهای ملی
- ✅ گزارش کامل از Import
- ✅ مدیریت خطاها و Transaction

---

## 📝 استفاده از اسکریپت SQL

### روش 1: OPENROWSET (مستقیم از Excel)

```sql
-- نیاز به نصب Microsoft Access Database Engine
-- فعال کردن Ad Hoc Distributed Queries
EXEC sp_configure 'Ad Hoc Distributed Queries', 1;
RECONFIGURE;

-- Import
INSERT INTO [dbo].[GiftCardEligibleUsers]
    (NationalCode, MobileNumber, FullName, PersonnelCode)
SELECT 
    LTRIM(RTRIM([کد ملی])) AS NationalCode,
    LTRIM(RTRIM([تلفن همراه])) AS MobileNumber,
    LTRIM(RTRIM([نام و نام خانوادگی])) AS FullName,
    LTRIM(RTRIM([کد پرسنلی])) AS PersonnelCode
FROM OPENROWSET('Microsoft.ACE.OLEDB.12.0',
    'Excel 12.0 Xml;HDR=YES;Database=C:\Path\To\File.xlsx;',
    'SELECT * FROM [Sheet1$]');
```

### روش 2: استفاده از Stored Procedure

```sql
-- اجرای Stored Procedure
EXEC dbo.usp_ImportGiftCardEligibleUsers 
    @ExcelFilePath = N'C:\Path\To\Your\File.xlsx',
    @SheetName = N'Sheet1$';
```

---

## 📊 ساختار فایل Excel

فایل Excel باید دارای این ستون‌ها باشد:

| ستون | نام فارسی | نام انگلیسی | الزامی | فرمت |
|------|-----------|-------------|--------|------|
| A | کد ملی | NationalCode | ✅ | 10 رقم |
| B | تلفن همراه | MobileNumber | ✅ | 11 رقم (شروع با 09) |
| C | نام و نام خانوادگی | FullName | ❌ | متن |
| D | کد پرسنلی | PersonnelCode | ❌ | متن |

---

## ✅ بررسی بعد از Import

```sql
USE [KhanoumiCore]
GO

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

## ⚠️ نکات مهم

1. ✅ **قبل از Import**: از دیتابیس Backup بگیرید
2. ✅ **Validation**: کد ملی باید دقیقاً 10 رقم باشد
3. ✅ **Validation**: شماره موبایل باید دقیقاً 11 رقم باشد و با 09 شروع شود
4. ✅ **تکراری‌ها**: کدهای ملی تکراری را قبل از Import حذف کنید
5. ✅ **بعد از Import**: تعداد رکوردها را بررسی کنید

---

## 🐛 رفع مشکلات

### خطا: "Cannot open file"
- فایل Excel را ببندید
- مسیر فایل را بررسی کنید
- دسترسی به فایل را چک کنید

### خطا: "Duplicate key error"
- کدهای ملی تکراری را از Excel حذف کنید
- یا از `INSERT IGNORE` استفاده کنید

### خطا: "Ad Hoc Distributed Queries is not enabled"
```sql
EXEC sp_configure 'Ad Hoc Distributed Queries', 1;
RECONFIGURE;
```

### خطا: "Microsoft.ACE.OLEDB.12.0 provider is not registered"
- دانلود و نصب **Microsoft Access Database Engine**:
  https://www.microsoft.com/en-us/download/details.aspx?id=54920

---

## 📞 پشتیبانی

اگر مشکلی پیش آمد:
1. لاگ خطا را بررسی کنید
2. تعداد رکوردهای Excel را با تعداد Import شده مقایسه کنید
3. چند رکورد را دستی بررسی کنید

---

## 📚 منابع بیشتر

- راهنمای کامل: `QuickImportGuide.md`
- اسکریپت SQL کامل: `ImportExcelToGiftCardEligibleUsers.sql`
- اسکریپت PowerShell: `ImportExcelToSQL.ps1`







