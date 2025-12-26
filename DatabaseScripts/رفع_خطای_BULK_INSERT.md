# راهنمای رفع خطای BULK INSERT و Collation

## خطاهای شما:

1. **خطای BULK INSERT**: `Cannot fetch a row from OLE DB provider "BULK"`
2. **خطای Collation**: `Cannot resolve the collation conflict between "SQL_Latin1_General_CP1_CI_AS" and "Persian_100_CI_AI"`

---

## ⭐ راه‌حل پیشنهادی: استفاده از PowerShell

**این روش ساده‌ترین و بهترین راه است** - بدون نیاز به دسترسی SQL Server به فایل

### مراحل:

1. **فایل Excel را به CSV تبدیل کنید:**
   ```powershell
   .\DatabaseScripts\تبدیل_Excel_به_CSV.ps1
   ```

2. **Import از CSV:**
   ```powershell
   .\DatabaseScripts\Import_یلدا_از_CSV_کامل.ps1
   ```

**تمام!** این روش:
- ✅ بدون نیاز به دسترسی SQL Server به فایل
- ✅ مشکل Collation را خودکار حل می‌کند
- ✅ ساده و سریع است

---

## راه‌حل 2: استفاده از SQL (اگر می‌خواهید از SQL استفاده کنید)

### مشکل 1: BULK INSERT نمی‌تواند فایل Desktop را بخواند

**راه‌حل:** فایل CSV را به یک مسیر مشترک منتقل کنید

1. **فایل CSV را کپی کنید:**
   - از Desktop فایل `یلدا.csv` را کپی کنید
   - به مسیر `C:\Temp\` منتقل کنید (یا هر مسیر دیگری که SQL Server به آن دسترسی دارد)

2. **اسکریپت SQL را تغییر دهید:**
   ```sql
   -- خط 18 را تغییر دهید:
   DECLARE @CsvFilePath NVARCHAR(500) = N'C:\Temp\یلدا.csv'
   ```

3. **دسترسی SQL Server را بررسی کنید:**
   - مطمئن شوید SQL Server Service Account به مسیر `C:\Temp\` دسترسی دارد
   - یا فایل را در یک Shared Folder قرار دهید

### مشکل 2: Collation Conflict

**این مشکل در اسکریپت SQL اصلاح شده است** - از COLLATE استفاده شده است.

---

## راه‌حل 3: استفاده از Shared Folder

اگر می‌خواهید از SQL استفاده کنید و فایل در Desktop است:

1. **یک Shared Folder ایجاد کنید:**
   - یک پوشه در شبکه ایجاد کنید (مثلاً `\\Server\Shared\`)
   - فایل CSV را در آن قرار دهید

2. **در SQL از مسیر UNC استفاده کنید:**
   ```sql
   DECLARE @CsvFilePath NVARCHAR(500) = N'\\Server\Shared\یلدا.csv'
   ```

---

## مقایسه روش‌ها:

| روش | سادگی | نیاز به دسترسی | مشکل Collation |
|-----|-------|----------------|----------------|
| **PowerShell** ⭐ | ⭐⭐⭐ | ❌ ندارد | ✅ حل شده |
| SQL + مسیر مشترک | ⭐⭐ | ✅ دارد | ✅ حل شده |
| SQL + Shared Folder | ⭐ | ✅ دارد | ✅ حل شده |

---

## توصیه نهایی:

**از PowerShell استفاده کنید!** (`Import_یلدا_از_CSV_کامل.ps1`)

این روش:
- ساده‌تر است
- نیاز به تنظیمات اضافی ندارد
- مشکل Collation را خودکار حل می‌کند
- فایل می‌تواند در Desktop باشد

---

## اگر هنوز مشکل دارید:

1. مطمئن شوید فایل CSV با UTF-8 ذخیره شده است
2. نام ستون‌ها را بررسی کنید: `کد ملی`, `تلفن همراه`, `نام و نام خانوادگی`, `کد پرسنلی`
3. فایل CSV را ببندید قبل از import
4. از PowerShell استفاده کنید (بهترین راه)







