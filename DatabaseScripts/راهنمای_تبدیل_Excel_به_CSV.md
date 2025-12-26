# راهنمای تبدیل Excel به CSV و Import به SQL

## مرحله 1: تبدیل فایل Excel به CSV

### روش 1: استفاده از Microsoft Excel (پیشنهادی)

1. **فایل Excel را باز کنید**
   - فایل `یلدا.xlsx` را در Excel باز کنید

2. **ذخیره به صورت CSV**
   - روی منوی **File** کلیک کنید
   - گزینه **Save As** را انتخاب کنید
   - در قسمت **Save as type**، یکی از گزینه‌های زیر را انتخاب کنید:
     - **CSV UTF-8 (Comma delimited) (*.csv)** ← **این گزینه را انتخاب کنید** ⭐
     - یا **CSV (Comma delimited) (*.csv)**

3. **ذخیره در Desktop**
   - نام فایل را `یلدا.csv` بگذارید
   - مسیر را Desktop انتخاب کنید
   - روی **Save** کلیک کنید

4. **تأیید**
   - اگر پیغام هشدار داد، روی **Yes** کلیک کنید

---

### روش 2: استفاده از Google Sheets (اگر Excel ندارید)

1. فایل Excel را در Google Sheets باز کنید
2. File → Download → Comma Separated Values (.csv)
3. فایل را در Desktop ذخیره کنید

---

### روش 3: استفاده از PowerShell (خودکار)

اگر می‌خواهید به صورت خودکار تبدیل کنید، این اسکریپت را اجرا کنید:

```powershell
# تبدیل Excel به CSV با PowerShell
$excelFile = "$env:USERPROFILE\Desktop\یلدا.xlsx"
$csvFile = "$env:USERPROFILE\Desktop\یلدا.csv"

# نصب ماژول ImportExcel (اگر نصب نشده)
if (-not (Get-Module -ListAvailable -Name ImportExcel)) {
    Install-Module -Name ImportExcel -Force -Scope CurrentUser
}

Import-Module ImportExcel

# تبدیل
Import-Excel -Path $excelFile | Export-Csv -Path $csvFile -Encoding UTF8 -NoTypeInformation

Write-Host "✓ فایل CSV ایجاد شد: $csvFile" -ForegroundColor Green
```

---

## مرحله 2: بررسی ساختار فایل CSV

قبل از اجرای اسکریپت SQL، مطمئن شوید:

1. **فایل CSV در Desktop قرار دارد** با نام `یلدا.csv`
2. **ستون‌های فایل CSV** باید این نام‌ها را داشته باشند:
   - `کد ملی`
   - `تلفن همراه`
   - `نام و نام خانوادگی` (اختیاری)
   - `کد پرسنلی` (اختیاری)

3. **فایل CSV با encoding UTF-8** ذخیره شده باشد

---

## مرحله 3: اجرای اسکریپت SQL

### روش 1: در SQL Server Management Studio (SSMS)

1. **SSMS را باز کنید**
2. **اتصال به سرور** برقرار کنید:
   - Server: `192.168.87.11`
   - Username: `trans`
   - Password: `t@Rrn!r46T5`

3. **فایل SQL را باز کنید**
   - فایل `Import_یلدا_از_CSV.sql` را در SSMS باز کنید

4. **اجرای اسکریپت**
   - کلید `F5` را بزنید یا روی دکمه **Execute** کلیک کنید

---

### روش 2: از Command Line

```bash
sqlcmd -S 192.168.87.11 -U trans -P "t@Rrn!r46T5" -d KhanoumiCore -i "DatabaseScripts\Import_یلدا_از_CSV.sql"
```

---

## مرحله 4: بررسی نتایج

بعد از اجرای اسکریپت، باید پیغام‌های زیر را ببینید:

```
✓ فایل CSV خوانده شد
✓ Import موفق!
تعداد رکوردهای اضافه شده: [عدد]
```

---

## رفع مشکلات رایج

### مشکل 1: خطای "Cannot bulk load because the file could not be opened"

**راه‌حل:**
- مطمئن شوید فایل CSV در Desktop قرار دارد
- مطمئن شوید SQL Server به فایل دسترسی دارد
- اگر مشکل ادامه داشت، فایل را به مسیر دیگری منتقل کنید و مسیر را در اسکریپت تغییر دهید

---

### مشکل 2: خطای "Invalid column name"

**راه‌حل:**
- نام ستون‌ها در CSV باید دقیقاً این باشد:
  - `کد ملی`
  - `تلفن همراه`
  - `نام و نام خانوادگی`
  - `کد پرسنلی`
- اگر نام ستون‌ها متفاوت است، در Excel تغییر دهید و دوباره به CSV تبدیل کنید

---

### مشکل 3: کاراکترهای فارسی درست نمایش داده نمی‌شوند

**راه‌حل:**
- مطمئن شوید فایل CSV با **UTF-8** ذخیره شده است
- در Excel: File → Save As → **CSV UTF-8 (Comma delimited)**

---

### مشکل 4: خطای "String or binary data would be truncated"

**راه‌حل:**
- این خطا یعنی یکی از فیلدها خیلی بزرگ است
- بررسی کنید که:
  - کد ملی حداکثر 10 کاراکتر باشد
  - شماره موبایل حداکثر 11 کاراکتر باشد
  - نام حداکثر 200 کاراکتر باشد

---

## نکات مهم

1. ✅ **فایل CSV را ببندید** قبل از اجرای اسکریپت
2. ✅ **نام فایل** باید دقیقاً `یلدا.csv` باشد
3. ✅ **مسیر فایل** باید Desktop باشد
4. ✅ **Encoding** باید UTF-8 باشد
5. ✅ **ستون اول** باید Header باشد (نام ستون‌ها)

---

## بررسی نهایی

بعد از Import، می‌توانید با این Query بررسی کنید:

```sql
SELECT COUNT(*) AS TotalRecords 
FROM [dbo].[GiftCardEligibleUsers];

SELECT TOP 10 * 
FROM [dbo].[GiftCardEligibleUsers]
ORDER BY CreatedAtUtc DESC;
```







