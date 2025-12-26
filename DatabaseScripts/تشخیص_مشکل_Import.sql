-- =============================================
-- Script: تشخیص مشکل Import فایل CSV
-- Database: KhanoumiCore
-- Description: بررسی مشکلات احتمالی در Import
-- =============================================

SET NOCOUNT ON;

PRINT '=========================================';
PRINT 'بررسی مشکلات احتمالی Import';
PRINT '=========================================';
PRINT '';

-- =============================================
-- 1) بررسی وجود جدول
-- =============================================
PRINT '1. بررسی وجود جدول GiftCardEligibleUsers...';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GiftCardEligibleUsers]') AND type in (N'U'))
BEGIN
    PRINT '✓ جدول GiftCardEligibleUsers وجود دارد.';
    
    -- نمایش ساختار جدول
    PRINT '';
    PRINT 'ساختار جدول:';
    SELECT 
        c.COLUMN_NAME,
        c.DATA_TYPE,
        c.CHARACTER_MAXIMUM_LENGTH,
        c.IS_NULLABLE,
        c.COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS c
    WHERE c.TABLE_SCHEMA = 'dbo'
      AND c.TABLE_NAME = 'GiftCardEligibleUsers'
    ORDER BY c.ORDINAL_POSITION;
    
    -- تعداد رکوردهای موجود
    DECLARE @TableCount INT = (SELECT COUNT(*) FROM dbo.GiftCardEligibleUsers);
    PRINT '';
    PRINT 'تعداد رکوردهای موجود در جدول: ' + CAST(@TableCount AS NVARCHAR(10));
END
ELSE
BEGIN
    PRINT '✗ جدول GiftCardEligibleUsers وجود ندارد!';
    PRINT 'لطفاً ابتدا جدول را ایجاد کنید.';
END
PRINT '';

-- =============================================
-- 2) بررسی مسیر فایل
-- =============================================
PRINT '2. بررسی مسیر فایل CSV...';
DECLARE @FilePath NVARCHAR(4000) = N'C:\Import\یلدا.csv';
DECLARE @FileExists INT;
DECLARE @FileIsDirectory INT;
DECLARE @ParentDirectoryExists INT;

EXEC master.dbo.xp_fileexist @FilePath, 
    @FileExists OUTPUT, 
    @FileIsDirectory OUTPUT, 
    @ParentDirectoryExists OUTPUT;

IF @FileExists = 1
BEGIN
    PRINT '✓ فایل در مسیر ' + @FilePath + ' یافت شد.';
END
ELSE
BEGIN
    PRINT '✗ فایل در مسیر ' + @FilePath + ' یافت نشد!';
    PRINT '';
    PRINT 'راهنمای رفع مشکل:';
    PRINT '1. مطمئن شوید فایل CSV در مسیر C:\Import\ قرار دارد';
    PRINT '2. اگر فایل در جای دیگری است، مسیر را در اسکریپت Import تغییر دهید';
    PRINT '3. مطمئن شوید SQL Server Service Account به این مسیر دسترسی دارد';
    PRINT '';
    PRINT 'برای بررسی دسترسی SQL Server:';
    PRINT '   - Service Account را در Services.msc بررسی کنید';
    PRINT '   - معمولاً NT Service\MSSQLSERVER یا NT AUTHORITY\SYSTEM است';
    PRINT '   - به این Account دسترسی Read برای پوشه C:\Import\ بدهید';
END

IF @ParentDirectoryExists = 0
BEGIN
    PRINT '⚠️ پوشه والد (C:\Import\) وجود ندارد!';
    PRINT 'لطفاً پوشه را ایجاد کنید و فایل را در آن قرار دهید.';
END
PRINT '';

-- =============================================
-- 3) بررسی دسترسی SQL Server
-- =============================================
PRINT '3. بررسی دسترسی SQL Server...';
DECLARE @ServiceAccount NVARCHAR(500);
EXEC xp_regread 
    @rootkey = 'HKEY_LOCAL_MACHINE',
    @key = 'SYSTEM\CurrentControlSet\Services\MSSQLSERVER',
    @value_name = 'ObjectName',
    @value = @ServiceAccount OUTPUT;

IF @ServiceAccount IS NOT NULL
BEGIN
    PRINT 'SQL Server Service Account: ' + @ServiceAccount;
    PRINT '⚠️ مطمئن شوید این Account به پوشه C:\Import\ دسترسی دارد.';
END
ELSE
BEGIN
    PRINT '⚠️ نتوانست Service Account را پیدا کند.';
END
PRINT '';

-- =============================================
-- 4) بررسی فرمت فایل CSV (اگر وجود دارد)
-- =============================================
PRINT '4. بررسی فرمت فایل CSV...';
IF @FileExists = 1
BEGIN
    PRINT '✓ فایل وجود دارد.';
    PRINT '';
    PRINT 'لطفاً موارد زیر را بررسی کنید:';
    PRINT '1. فایل باید با UTF-8 ذخیره شده باشد';
    PRINT '2. ستون‌ها باید با کاما (,) جدا شده باشند';
    PRINT '3. ردیف اول باید header باشد: کد_ملی,تلفن_همراه,نام_و_نام_خانوادگی,کد_پرسنلی';
    PRINT '4. هر ردیف باید در یک خط جدید باشد';
    PRINT '';
    PRINT 'برای تبدیل فایل به UTF-8:';
    PRINT '   - در Notepad++: Encoding > Convert to UTF-8';
    PRINT '   - در Excel: Save As > CSV UTF-8 (Comma delimited)';
END
ELSE
BEGIN
    PRINT '⚠️ فایل یافت نشد، نمی‌توان فرمت را بررسی کرد.';
END
PRINT '';

-- =============================================
-- 5) تست خواندن فایل (اگر وجود دارد)
-- =============================================
IF @FileExists = 1
BEGIN
    PRINT '5. تست خواندن فایل CSV...';
    
    -- ایجاد جدول موقت
    IF OBJECT_ID('tempdb..#TestRead') IS NOT NULL DROP TABLE #TestRead;
    
    CREATE TABLE #TestRead
    (
        [کد_ملی]           NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL,
        [تلفن_همراه]       NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL,
        [نام_و_نام_خانوادگی] NVARCHAR(200) COLLATE DATABASE_DEFAULT NULL,
        [کد_پرسنلی]        NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL
    );
    
    BEGIN TRY
        BULK INSERT #TestRead
        FROM 'C:\Import\یلدا.csv'
        WITH
        (
            FIRSTROW = 2,
            FIELDTERMINATOR = ',',
            ROWTERMINATOR = '0x0A',
            CODEPAGE = '65001',
            TABLOCK
        );
        
        DECLARE @TestCount INT = (SELECT COUNT(*) FROM #TestRead);
        PRINT '✓ فایل با موفقیت خوانده شد.';
        PRINT 'تعداد رکوردهای خوانده شده: ' + CAST(@TestCount AS NVARCHAR(10));
        
        IF @TestCount > 0
        BEGIN
            PRINT '';
            PRINT 'نمونه داده‌های خوانده شده (3 رکورد اول):';
            SELECT TOP 3 * FROM #TestRead;
        END
        ELSE
        BEGIN
            PRINT '⚠️ هیچ رکوردی خوانده نشد!';
            PRINT 'ممکن است:';
            PRINT '  - فایل فقط header داشته باشد';
            PRINT '  - ROWTERMINATOR اشتباه باشد';
            PRINT '  - فایل خالی باشد';
        END
    END TRY
    BEGIN CATCH
        DECLARE @TestError NVARCHAR(4000) = ERROR_MESSAGE();
        PRINT '✗ خطا در خواندن فایل:';
        PRINT @TestError;
        PRINT '';
        PRINT 'راهنمای رفع مشکل:';
        PRINT '1. مطمئن شوید فایل با UTF-8 ذخیره شده است';
        PRINT '2. مطمئن شوید SQL Server به فایل دسترسی دارد';
        PRINT '3. ROWTERMINATOR را امتحان کنید: ''0x0D0A'' (برای Windows)';
    END CATCH
    
    IF OBJECT_ID('tempdb..#TestRead') IS NOT NULL DROP TABLE #TestRead;
END
ELSE
BEGIN
    PRINT '5. تست خواندن فایل CSV...';
    PRINT '⚠️ فایل یافت نشد، نمی‌توان تست کرد.';
END
PRINT '';

-- =============================================
-- خلاصه
-- =============================================
PRINT '=========================================';
PRINT 'خلاصه بررسی';
PRINT '=========================================';
PRINT '';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GiftCardEligibleUsers]') AND type in (N'U'))
    PRINT '✓ جدول وجود دارد';
ELSE
    PRINT '✗ جدول وجود ندارد - باید ایجاد شود';

IF @FileExists = 1
    PRINT '✓ فایل CSV یافت شد';
ELSE
    PRINT '✗ فایل CSV یافت نشد - باید مسیر را بررسی کنید';

PRINT '';
PRINT 'اگر همه موارد ✓ هستند، اسکریپت Import_یلدا_از_CSV_پیشرفته_بهبود_یافته.sql را اجرا کنید.';
PRINT '=========================================';

GO







