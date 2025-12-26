-- =============================================
-- Script SQL: Import Excel به جدول GiftCardEligibleUsers
-- Database: KhanoumiCore
-- =============================================
-- این اسکریپت را در SQL Server Management Studio اجرا کنید
-- =============================================

USE [KhanoumiCore]
GO

-- =============================================
-- مرحله 1: فعال کردن Ad Hoc Distributed Queries
-- =============================================
PRINT 'فعال کردن Ad Hoc Distributed Queries...'
GO

EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
GO

EXEC sp_configure 'Ad Hoc Distributed Queries', 1;
RECONFIGURE;
GO

PRINT '✓ Ad Hoc Distributed Queries فعال شد.'
GO

-- =============================================
-- مرحله 2: تنظیمات فایل Excel
-- =============================================
-- ⚠️ اینجا مسیر فایل Excel خود را وارد کنید
-- =============================================

DECLARE @ExcelFilePath NVARCHAR(500)
DECLARE @SheetName NVARCHAR(100) = N'Sheet1$'  -- اگر نام Sheet متفاوت است، تغییر دهید

-- ⚠️ مسیر فایل Excel را اینجا وارد کنید
-- مثال: SET @ExcelFilePath = N'C:\Users\YourUsername\Desktop\یلدا.xlsx'
SET @ExcelFilePath = N'C:\Users\YourUsername\Desktop\یلدا.xlsx'  -- ⚠️ تغییر دهید

PRINT '========================================='
PRINT 'شروع Import از Excel...'
PRINT 'مسیر فایل: ' + @ExcelFilePath
PRINT '========================================='
GO

-- =============================================
-- مرحله 3: Import داده‌ها
-- =============================================

DECLARE @ExcelFilePath NVARCHAR(500) = N'C:\Users\YourUsername\Desktop\یلدا.xlsx'  -- ⚠️ تغییر دهید
DECLARE @SheetName NVARCHAR(100) = N'Sheet1$'
DECLARE @SQL NVARCHAR(MAX)

BEGIN TRY
    -- ساخت Query با استفاده از sp_executesql
    SET @SQL = N'
    INSERT INTO [dbo].[GiftCardEligibleUsers]
        (NationalCode, MobileNumber, FullName, PersonnelCode)
    SELECT 
        LTRIM(RTRIM(CAST([کد ملی] AS NVARCHAR(10)))) AS NationalCode,
        LTRIM(RTRIM(CAST([تلفن همراه] AS NVARCHAR(11)))) AS MobileNumber,
        LTRIM(RTRIM(CAST([نام و نام خانوادگی] AS NVARCHAR(200)))) AS FullName,
        LTRIM(RTRIM(CAST([کد پرسنلی] AS NVARCHAR(50)))) AS PersonnelCode
    FROM OPENROWSET(''Microsoft.ACE.OLEDB.12.0'',
        ''Excel 12.0 Xml;HDR=YES;Database=' + @ExcelFilePath + ';'',
        ''SELECT * FROM [' + @SheetName + ']'')
    WHERE [کد ملی] IS NOT NULL 
        AND LEN(LTRIM(RTRIM(CAST([کد ملی] AS NVARCHAR(10))))) = 10
        AND [تلفن همراه] IS NOT NULL
        AND LEN(LTRIM(RTRIM(CAST([تلفن همراه] AS NVARCHAR(11))))) = 11
        AND LTRIM(RTRIM(CAST([تلفن همراه] AS NVARCHAR(11)))) LIKE ''09%''
        AND NOT EXISTS (
            SELECT 1 
            FROM [dbo].[GiftCardEligibleUsers] 
            WHERE NationalCode = LTRIM(RTRIM(CAST([کد ملی] AS NVARCHAR(10))))
        );'
    
    -- اجرای Query
    EXEC sp_executesql @SQL
    
    DECLARE @InsertedCount INT = @@ROWCOUNT
    
    PRINT ''
    PRINT '========================================='
    PRINT N'✓ Import موفق!'
    PRINT N'تعداد رکوردهای اضافه شده: ' + CAST(@InsertedCount AS NVARCHAR(10))
    PRINT '========================================='
    
END TRY
BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
    
    PRINT ''
    PRINT '✗ خطا در Import:'
    PRINT @ErrorMessage
    PRINT ''
    PRINT '========================================='
    PRINT 'راهنمای رفع مشکل:'
    PRINT '========================================='
    
    IF @ErrorMessage LIKE '%provider is not registered%' OR @ErrorMessage LIKE '%Microsoft.ACE.OLEDB%'
    BEGIN
        PRINT 'مشکل: Microsoft Access Database Engine نصب نیست'
        PRINT ''
        PRINT 'راه‌حل 1: نصب Access Database Engine'
        PRINT '  1. دانلود: https://www.microsoft.com/en-us/download/details.aspx?id=54920'
        PRINT '  2. نسخه 64-bit یا 32-bit را بسته به SQL Server نصب کنید'
        PRINT '  3. SQL Server را Restart کنید'
        PRINT '  4. این اسکریپت را دوباره اجرا کنید'
        PRINT ''
        PRINT 'راه‌حل 2: استفاده از SQL Server Import Wizard'
        PRINT '  1. SSMS → دیتابیس KhanoumiCore → راست کلیک'
        PRINT '  2. Tasks → Import Data...'
        PRINT '  3. فایل Excel را انتخاب کنید'
        PRINT '  4. مراحل را دنبال کنید'
        PRINT ''
        PRINT 'راه‌حل 3: استفاده از اسکریپت PowerShell'
        PRINT '  .\ImportExcel_Simple.ps1'
    END
    ELSE IF @ErrorMessage LIKE '%Cannot open%' OR @ErrorMessage LIKE '%file%'
    BEGIN
        PRINT 'مشکل: فایل Excel یافت نشد یا قفل است'
        PRINT '  1. مطمئن شوید فایل Excel بسته است'
        PRINT '  2. مسیر فایل را در خط 25 بررسی کنید'
        PRINT '  3. مطمئن شوید فایل در Desktop قرار دارد'
    END
    ELSE
    BEGIN
        PRINT '  1. مسیر فایل Excel را بررسی کنید'
        PRINT '  2. نام Sheet را بررسی کنید'
        PRINT '  3. ساختار فایل Excel را بررسی کنید'
    END
    
    PRINT ''
    
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH
GO

-- =============================================
-- مرحله 4: بررسی نتایج
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'بررسی نتایج:'
PRINT '========================================='

SELECT 
    COUNT(*) AS TotalRecords,
    COUNT(DISTINCT NationalCode) AS UniqueNationalCodes,
    COUNT(DISTINCT MobileNumber) AS UniqueMobileNumbers,
    SUM(CASE WHEN IsProcessed = 0 THEN 1 ELSE 0 END) AS PendingCount,
    SUM(CASE WHEN IsProcessed = 1 THEN 1 ELSE 0 END) AS ProcessedCount
FROM [dbo].[GiftCardEligibleUsers];

-- بررسی رکوردهای تکراری
IF EXISTS (
    SELECT 1 
    FROM [dbo].[GiftCardEligibleUsers]
    GROUP BY NationalCode
    HAVING COUNT(*) > 1
)
BEGIN
    PRINT ''
    PRINT '⚠ هشدار: رکوردهای تکراری یافت شد:'
    SELECT 
        NationalCode,
        COUNT(*) AS Count
    FROM [dbo].[GiftCardEligibleUsers]
    GROUP BY NationalCode
    HAVING COUNT(*) > 1;
END
ELSE
BEGIN
    PRINT ''
    PRINT '✓ هیچ رکورد تکراری یافت نشد.'
END

-- نمایش نمونه رکوردها
PRINT ''
PRINT 'نمونه رکوردهای Import شده:'
SELECT TOP 10 
    NationalCode,
    MobileNumber,
    FullName,
    PersonnelCode,
    IsProcessed,
    CreatedAtUtc
FROM [dbo].[GiftCardEligibleUsers]
ORDER BY CreatedAtUtc DESC;

GO

PRINT ''
PRINT '========================================='
PRINT 'Import تکمیل شد!'
PRINT '========================================='
GO







