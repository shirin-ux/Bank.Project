-- =============================================
-- Script: Import فایل یلدا.xlsx به جدول GiftCardEligibleUsers
-- Database: KhanoumiCore
-- =============================================
-- این query داده‌های فایل "یلدا.xlsx" را از Desktop به جدول GiftCardEligibleUsers import می‌کند
-- =============================================

USE [KhanoumiCore]
GO

-- فعال کردن Ad Hoc Distributed Queries (فقط یک بار نیاز است)
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'Ad Hoc Distributed Queries', 1;
RECONFIGURE;
GO

-- =============================================
-- تنظیمات فایل
-- =============================================
DECLARE @FileName NVARCHAR(255) = N'یلدا.xlsx'
DECLARE @SheetName NVARCHAR(100) = N'Sheet1$'  -- اگر نام Sheet متفاوت است، تغییر دهید
DECLARE @ExcelFilePath NVARCHAR(500)

-- ساخت مسیر کامل Desktop (خودکار)
SET @ExcelFilePath = N'C:\Users\' + SUSER_SNAME() + N'\Desktop\' + @FileName

-- اگر SUSER_SNAME() کار نکرد، مسیر را دستی وارد کنید:
-- SET @ExcelFilePath = N'C:\Users\YourUsername\Desktop\یلدا.xlsx'

PRINT '========================================='
PRINT 'شروع Import از فایل: ' + @FileName
PRINT 'مسیر: ' + @ExcelFilePath
PRINT '========================================='
GO

-- =============================================
-- Import داده‌ها
-- =============================================
DECLARE @FileName NVARCHAR(255) = N'یلدا.xlsx'
DECLARE @SheetName NVARCHAR(100) = N'Sheet1$'
DECLARE @ExcelFilePath NVARCHAR(500) = N'C:\Users\' + SUSER_SNAME() + N'\Desktop\' + @FileName
DECLARE @SQL NVARCHAR(MAX)

BEGIN TRY
    -- ساخت Query برای Import
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
    PRINT '1. مطمئن شوید فایل "یلدا.xlsx" در Desktop قرار دارد'
    PRINT '2. مطمئن شوید فایل Excel بسته است'
    PRINT '3. اگر خطای "provider is not registered" گرفتید:'
    PRINT '   - Microsoft Access Database Engine را نصب کنید'
    PRINT '   - دانلود: https://www.microsoft.com/en-us/download/details.aspx?id=54920'
    PRINT '4. اگر مسیر Desktop درست نیست، خط 25 را تغییر دهید'
    PRINT '5. اگر نام Sheet متفاوت است، خط 22 را تغییر دهید'
    PRINT ''
    
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH
GO

-- =============================================
-- بررسی نتایج
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







