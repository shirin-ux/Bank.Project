# 🔧 رفع خطا: "Microsoft.ACE.OLEDB.12.0 has not been registered"

## 📋 مشکل

این خطا به این معنی است که **Microsoft Access Database Engine** روی سرور SQL Server نصب نشده است.

---

## ✅ راه‌حل 1: نصب Microsoft Access Database Engine (پیشنهادی)

### مرحله 1: دانلود

1. به این آدرس بروید:
   https://www.microsoft.com/en-us/download/details.aspx?id=54920

2. نسخه مناسب را دانلود کنید:
   - **64-bit**: اگر SQL Server شما 64-bit است
   - **32-bit**: اگر SQL Server شما 32-bit است

### مرحله 2: نصب

1. فایل دانلود شده را اجرا کنید
2. **Next** → **Next** → **Install** را کلیک کنید
3. منتظر بمانید تا نصب کامل شود
4. **SQL Server را Restart کنید**

### مرحله 3: بررسی

بعد از نصب و Restart، اسکریپت Import را دوباره اجرا کنید.

---

## ✅ راه‌حل 2: استفاده از اسکریپت PowerShell (بدون نیاز به Access Database Engine)

اگر نمی‌توانید Access Database Engine را نصب کنید، از اسکریپت PowerShell استفاده کنید:

### اجرا:

```powershell
# نصب ماژول ImportExcel (فقط یک بار)
Install-Module -Name ImportExcel -Force -Scope CurrentUser

# اجرای اسکریپت
.\ImportExcelToSQL.ps1 -ExcelFilePath "C:\Users\YourUsername\Desktop\YourFile.xlsx"
```

**مزایا:**
- ✅ نیاز به Access Database Engine ندارد
- ✅ Validation خودکار
- ✅ گزارش کامل
- ✅ مدیریت خطاها

---

## ✅ راه‌حل 3: تبدیل Excel به CSV و Import دستی

### مرحله 1: تبدیل Excel به CSV

1. فایل Excel را باز کنید
2. **File** → **Save As**
3. نوع فایل را **CSV (Comma delimited) (*.csv)** انتخاب کنید
4. فایل را در Desktop ذخیره کنید

### مرحله 2: Import دستی با INSERT

1. فایل CSV را باز کنید
2. داده‌ها را کپی کنید
3. از یک Converter آنلاین استفاده کنید:
   - https://convertcsv.com/csv-to-sql.htm
4. فایل SQL ایجاد شده را در SSMS اجرا کنید

---

## ✅ راه‌حل 4: استفاده از SQL Server Import Wizard

این روش نیاز به Access Database Engine ندارد:

1. SQL Server Management Studio را باز کنید
2. روی دیتابیس **KhanoumiCore** راست کلیک کنید
3. **Tasks** → **Import Data...**
4. **Data source**: `Microsoft Excel` را انتخاب کنید
5. مسیر فایل Excel را انتخاب کنید
6. مراحل را دنبال کنید

**نکته:** Import Wizard خودش Access Database Engine را مدیریت می‌کند.

---

## 🔍 تشخیص نسخه SQL Server

برای تشخیص اینکه SQL Server شما 32-bit است یا 64-bit:

```sql
SELECT @@VERSION;
```

یا در SQL Server Management Studio:
- **Help** → **About** → نسخه را بررسی کنید

---

## ⚠️ نکات مهم

1. ✅ بعد از نصب Access Database Engine، **حتماً SQL Server را Restart کنید**
2. ✅ اگر SQL Server 64-bit دارید، Access Database Engine 64-bit نصب کنید
3. ✅ اگر SQL Server 32-bit دارید، Access Database Engine 32-bit نصب کنید
4. ✅ اگر نمی‌توانید نصب کنید، از روش PowerShell استفاده کنید

---

## 📞 اگر مشکل حل نشد

1. بررسی کنید که SQL Server Service Account دسترسی به فایل Excel دارد
2. بررسی کنید که فایل Excel بسته است
3. بررسی کنید که مسیر فایل درست است
4. از روش PowerShell استفاده کنید (ساده‌ترین روش)

---

## 🚀 روش پیشنهادی

**برای سرعت بیشتر:** از اسکریپت PowerShell استفاده کنید (`ImportExcelToSQL.ps1`)

**برای نصب Access Database Engine:** اگر دسترسی به سرور دارید و می‌توانید نصب کنید







