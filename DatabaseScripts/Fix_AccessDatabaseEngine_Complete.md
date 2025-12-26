# 🔧 راهنمای کامل: رفع خطای "Microsoft.ACE.OLEDB.12.0 has not been registered"

## 📋 مشکل چیست؟

این خطا به این معنی است که **Microsoft Access Database Engine** روی سرور SQL Server نصب نشده است. این موتور برای خواندن فایل‌های Excel با `OPENROWSET` در SQL Server لازم است.

---

## ✅ راه‌حل 1: نصب Microsoft Access Database Engine (پیشنهادی)

### مرحله 1: تشخیص نسخه SQL Server

ابتدا باید بدانید SQL Server شما 32-bit است یا 64-bit:

**روش 1: از Query Editor**
```sql
SELECT @@VERSION;
```

**روش 2: از SSMS**
- Help → About → نسخه را بررسی کنید

### مرحله 2: دانلود Access Database Engine

1. به این آدرس بروید:
   **https://www.microsoft.com/en-us/download/details.aspx?id=54920**

2. نسخه مناسب را دانلود کنید:
   - **AccessDatabaseEngine_X64.exe** → اگر SQL Server شما 64-bit است
   - **AccessDatabaseEngine.exe** → اگر SQL Server شما 32-bit است

### مرحله 3: نصب

1. فایل دانلود شده را اجرا کنید
2. **Next** → **Next** → **Install** را کلیک کنید
3. منتظر بمانید تا نصب کامل شود
4. **⚠️ مهم: SQL Server را Restart کنید**

### مرحله 4: بررسی

بعد از Restart، اسکریپت Import را دوباره اجرا کنید.

---

## ✅ راه‌حل 2: استفاده از SQL Server Import Wizard (ساده‌ترین روش)

این روش نیاز به نصب Access Database Engine ندارد:

### مراحل:

1. **SQL Server Management Studio** را باز کنید
2. به دیتابیس **KhanoumiCore** متصل شوید
3. روی دیتابیس **KhanoumiCore** راست کلیک کنید
4. **Tasks** → **Import Data...** را انتخاب کنید
5. در پنجره Import Wizard:
   - **Data source**: `Microsoft Excel` را انتخاب کنید
   - **Excel file path**: مسیر فایل Excel را انتخاب کنید (مثلاً `C:\Users\YourUsername\Desktop\یلدا.xlsx`)
   - **Excel version**: نسخه Excel خود را انتخاب کنید
   - ✅ **First row has column names** را تیک بزنید
   - **Next** را کلیک کنید
6. در **Destination**:
   - **Destination**: `SQL Server Native Client` را انتخاب کنید
   - **Server name**: نام سرور را وارد کنید
   - **Database**: `KhanoumiCore` را انتخاب کنید
   - **Next** را کلیک کنید
7. در **Select Source Tables and Views**:
   - Sheet مربوطه را انتخاب کنید (مثلاً `Sheet1$`)
   - روی **Edit Mappings...** کلیک کنید
   - **Destination table**: `[dbo].[GiftCardEligibleUsers]` را انتخاب کنید
   - Mapping را تنظیم کنید:
     - کد ملی → `NationalCode`
     - تلفن همراه → `MobileNumber`
     - نام و نام خانوادگی → `FullName`
     - کد پرسنلی → `PersonnelCode`
   - ستون‌های `Id`, `CreatedAtUtc`, `IsProcessed`, `ProcessedAtUtc` را **Ignore** کنید
   - **OK** → **Next**
8. **Finish** را کلیک کنید و منتظر بمانید

**مزایا:**
- ✅ نیاز به نصب Access Database Engine ندارد
- ✅ رابط کاربری ساده
- ✅ مدیریت خطاها خودکار

---

## ✅ راه‌حل 3: استفاده از اسکریپت PowerShell (بدون نیاز به Access Database Engine)

### مراحل:

1. **PowerShell** را باز کنید
2. به پوشه اسکریپت بروید:
   ```powershell
   cd D:\ProjectBank\DatabaseScripts
   ```
3. اسکریپت را اجرا کنید:
   ```powershell
   .\ImportExcel_Simple.ps1
   ```

**مزایا:**
- ✅ نیاز به Access Database Engine ندارد
- ✅ Validation خودکار
- ✅ گزارش کامل
- ✅ مدیریت خطاها

---

## ✅ راه‌حل 4: تبدیل Excel به CSV و Import دستی

### مرحله 1: تبدیل Excel به CSV

1. فایل Excel را باز کنید
2. **File** → **Save As**
3. نوع فایل را **CSV (Comma delimited) (*.csv)** انتخاب کنید
4. فایل را ذخیره کنید

### مرحله 2: Import دستی

از یک Converter آنلاین استفاده کنید:
- https://convertcsv.com/csv-to-sql.htm

یا دستی INSERT کنید:

```sql
INSERT INTO [dbo].[GiftCardEligibleUsers]
    (NationalCode, MobileNumber, FullName, PersonnelCode)
VALUES
    ('0014219921', '09377984643', N'امید نعیمی', '440003'),
    ('1091267634', '09126211588', N'مسعود شاه مرادی', '440004')
    -- ... ادامه لیست
```

---

## 🔍 تشخیص نسخه SQL Server

برای تشخیص اینکه SQL Server شما 32-bit است یا 64-bit:

```sql
-- روش 1: از Query Editor
SELECT @@VERSION;

-- روش 2: بررسی Architecture
SELECT 
    SERVERPROPERTY('MachineName') AS MachineName,
    SERVERPROPERTY('ProductVersion') AS ProductVersion,
    SERVERPROPERTY('ProductLevel') AS ProductLevel,
    CASE 
        WHEN SERVERPROPERTY('Edition') LIKE '%64%' THEN '64-bit'
        ELSE '32-bit'
    END AS Architecture
```

---

## ⚠️ مشکلات رایج در نصب Access Database Engine

### مشکل: "Another version of Microsoft Office is installed"

**راه‌حل:**
از نسخه **/quiet** استفاده کنید:

```cmd
AccessDatabaseEngine_X64.exe /quiet
```

یا از **AccessDatabaseEngineRedist.exe** استفاده کنید که با Office سازگار است.

### مشکل: "Setup is running"

**راه‌حل:**
1. Task Manager را باز کنید
2. تمام پروسه‌های `setup.exe` را ببندید
3. دوباره نصب کنید

### مشکل: بعد از نصب هنوز خطا می‌دهد

**راه‌حل:**
1. مطمئن شوید SQL Server را **Restart** کرده‌اید
2. نسخه درست را نصب کرده‌اید (64-bit یا 32-bit)
3. از دستور زیر برای بررسی استفاده کنید:

```sql
EXEC sp_configure 'Ad Hoc Distributed Queries', 1;
RECONFIGURE;
```

---

## 📊 مقایسه روش‌ها

| روش | نیاز به Access Database Engine | سادگی | سرعت |
|-----|--------------------------------|-------|------|
| SQL Import Wizard | ❌ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| PowerShell Script | ❌ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| نصب Access Engine | ✅ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| CSV Import | ❌ | ⭐⭐ | ⭐⭐⭐ |

---

## 🚀 توصیه

**برای سرعت و سادگی:** از **SQL Server Import Wizard** استفاده کنید (راه‌حل 2)

**برای خودکارسازی:** از **PowerShell Script** استفاده کنید (راه‌حل 3)

**اگر دسترسی به سرور دارید:** **Access Database Engine** را نصب کنید (راه‌حل 1)

---

## 📞 اگر مشکل حل نشد

1. بررسی کنید که SQL Server Service Account دسترسی به فایل Excel دارد
2. بررسی کنید که فایل Excel بسته است
3. بررسی کنید که مسیر فایل درست است
4. از روش PowerShell استفاده کنید (ساده‌ترین روش)







