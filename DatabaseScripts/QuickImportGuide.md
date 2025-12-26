# راهنمای سریع Import لیست Excel به GiftCardEligibleUsers

## ⚡ روش سریع (پیشنهادی): استفاده از SQL Server Import Wizard

### مرحله 1: آماده‌سازی فایل Excel

1. فایل Excel را باز کنید
2. مطمئن شوید ستون‌ها به این ترتیب هستند:
   - **ستون A**: کد ملی (10 رقم)
   - **ستون B**: شماره موبایل (11 رقم، شروع با 09)
   - **ستون C**: نام و نام خانوادگی
   - **ستون D**: کد پرسنلی (اختیاری)
3. فایل را ذخیره کنید

### مرحله 2: Import در SQL Server Management Studio

1. **SSMS** را باز کنید
2. به دیتابیس **KhanoumiCore** متصل شوید
3. روی دیتابیس **KhanoumiCore** راست کلیک کنید
4. **Tasks** → **Import Data...** را انتخاب کنید
5. در **Data source**: `Microsoft Excel` را انتخاب کنید
6. مسیر فایل Excel را انتخاب کنید
7. در **Destination**: `SQL Server Native Client` را انتخاب کنید
8. دیتابیس: `KhanoumiCore`
9. در **Select Source Tables and Views**:
   - Sheet مربوطه را انتخاب کنید (مثلاً `Sheet1$`)
   - روی **Edit Mappings...** کلیک کنید
   - **Destination table**: `[dbo].[GiftCardEligibleUsers]` را انتخاب کنید
   - Mapping را تنظیم کنید:
     - کد ملی → `NationalCode`
     - تلفن همراه → `MobileNumber`
     - نام و نام خانوادگی → `FullName`
     - کد پرسنلی → `PersonnelCode`
   - ستون‌های `Id`, `CreatedAtUtc`, `IsProcessed`, `ProcessedAtUtc` را **Ignore** کنید
10. **Finish** را کلیک کنید

### مرحله 3: بررسی

بعد از Import، این کوئری را اجرا کنید:

```sql
USE [KhanoumiCore]
GO

-- بررسی تعداد رکوردها
SELECT COUNT(*) AS TotalRecords
FROM [dbo].[GiftCardEligibleUsers];

-- بررسی رکوردهای تکراری (باید خالی باشد)
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

## 🔧 روش جایگزین: تبدیل Excel به CSV و Import

### مرحله 1: تبدیل Excel به CSV

1. فایل Excel را باز کنید
2. **File** → **Save As**
3. نوع فایل را **CSV (Comma delimited) (*.csv)** انتخاب کنید
4. فایل را ذخیره کنید

### مرحله 2: Import از CSV

از اسکریپت `ImportExcelToGiftCardEligibleUsers.sql` استفاده کنید و بخش **روش 2** را اجرا کنید.

---

## 📝 روش دستی: INSERT مستقیم

اگر لیست کوچک است (کمتر از 100 رکورد):

1. فایل Excel را باز کنید
2. داده‌ها را کپی کنید
3. از یک Converter آنلاین استفاده کنید (مثل https://convertcsv.com/csv-to-sql.htm)
4. فایل SQL ایجاد شده را در SSMS اجرا کنید

---

## ⚠️ نکات مهم

1. ✅ قبل از Import، از دیتابیس **Backup** بگیرید
2. ✅ کد ملی باید دقیقاً **10 رقم** باشد
3. ✅ شماره موبایل باید دقیقاً **11 رقم** باشد و با **09** شروع شود
4. ✅ کدهای ملی تکراری را قبل از Import حذف کنید
5. ✅ بعد از Import، تعداد رکوردها را بررسی کنید

---

## 🐛 رفع مشکلات

### خطا: "Cannot open file"
- فایل Excel را ببندید
- مسیر فایل را بررسی کنید
- مطمئن شوید فایل قفل نشده است

### خطا: "Duplicate key error"
- کدهای ملی تکراری را از Excel حذف کنید
- یا از `INSERT IGNORE` استفاده کنید (در اسکریپت موجود است)

### خطا: "Data type conversion error"
- فرمت کد ملی و شماره موبایل را بررسی کنید
- مطمئن شوید اعداد به صورت Text ذخیره شده‌اند (نه Number)

---

## 📞 پشتیبانی

اگر مشکلی پیش آمد:
1. لاگ خطا را بررسی کنید
2. تعداد رکوردهای Excel را با تعداد Import شده مقایسه کنید
3. چند رکورد را دستی بررسی کنید







