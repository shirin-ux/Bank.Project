-- =============================================
-- Script: Import فایل یلدا از CSV (بدون نیاز به Access Database Engine)
-- Database: KhanoumiCore
-- =============================================
-- ⚠️ توجه: BULK INSERT نیاز به دسترسی SQL Server به فایل دارد
-- اگر فایل در Desktop شماست، SQL Server نمی‌تواند به آن دسترسی داشته باشد
-- 
-- ⭐ پیشنهاد: از اسکریپت PowerShell استفاده کنید:
--    .\DatabaseScripts\Import_یلدا_از_CSV_کامل.ps1
--
-- یا فایل CSV را به یک مسیر مشترک منتقل کنید که SQL Server به آن دسترسی دارد
-- =============================================

USE [KhanoumiCore]
GO

-- =============================================
-- تنظیمات
-- =============================================
DECLARE @CsvFilePath NVARCHAR(500) = N'C:\Users\' + SUSER_SNAME() + N'\Desktop\یلدا.csv'
-- اگر مسیر درست نیست، دستی وارد کنید:
-- DECLARE @CsvFilePath NVARCHAR(500) = N'C:\Users\YourUsername\Desktop\یلدا.csv'

PRINT '========================================='
PRINT 'Import از فایل CSV'
PRINT 'مسیر: ' + @CsvFilePath
PRINT '========================================='
GO

-- =============================================
-- ایجاد جدول موقت
-- =============================================
IF OBJECT_ID('tempdb..#TempGiftCardUsers') IS NOT NULL
    DROP TABLE #TempGiftCardUsers;
GO

CREATE TABLE #TempGiftCardUsers (
    [RowNum] INT IDENTITY(1,1),
    [کد_ملی] NVARCHAR(10) COLLATE Persian_100_CI_AI NULL,
    [تلفن_همراه] NVARCHAR(11) COLLATE Persian_100_CI_AI NULL,
    [نام_و_نام_خانوادگی] NVARCHAR(200) COLLATE Persian_100_CI_AI NULL,
    [کد_پرسنلی] NVARCHAR(50) COLLATE Persian_100_CI_AI NULL
);
GO

-- =============================================
-- Import از CSV
-- =============================================
DECLARE @CsvFilePath NVARCHAR(500) = N'C:\Users\' + SUSER_SNAME() + N'\Desktop\یلدا.csv'
DECLARE @SQL NVARCHAR(MAX)

BEGIN TRY
    -- Import از CSV
    SET @SQL = N'
    BULK INSERT #TempGiftCardUsers
    FROM ''' + @CsvFilePath + '''
    WITH (
        FIELDTERMINATOR = '','',
        ROWTERMINATOR = ''\n'',
        FIRSTROW = 2,
        CODEPAGE = ''65001''
    );'
    
    EXEC sp_executesql @SQL
    
    PRINT '✓ فایل CSV خوانده شد'
    
END TRY
BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
    
    PRINT ''
    PRINT '✗ خطا در خواندن CSV:'
    PRINT @ErrorMessage
    PRINT ''
    PRINT 'راهنمای رفع مشکل:'
    PRINT ''
    PRINT '⚠️ مشکل: SQL Server نمی‌تواند به فایل در Desktop دسترسی داشته باشد'
    PRINT ''
    PRINT 'راه‌حل 1 (پیشنهادی): استفاده از PowerShell'
    PRINT '   .\DatabaseScripts\Import_یلدا_از_CSV_کامل.ps1'
    PRINT ''
    PRINT 'راه‌حل 2: انتقال فایل به مسیر مشترک'
    PRINT '   1. فایل CSV را به یک مسیر مشترک منتقل کنید (مثلاً C:\Temp\یلدا.csv)'
    PRINT '   2. خط 18 را تغییر دهید: DECLARE @CsvFilePath = N''C:\Temp\یلدا.csv'''
    PRINT '   3. مطمئن شوید SQL Server Service Account به این مسیر دسترسی دارد'
    PRINT ''
    PRINT 'راه‌حل 3: استفاده از Shared Folder'
    PRINT '   فایل را در یک Shared Folder قرار دهید و مسیر UNC استفاده کنید'
    PRINT ''
    
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH
GO

-- =============================================
-- Insert به جدول اصلی
-- =============================================
BEGIN TRY
    INSERT INTO [dbo].[GiftCardEligibleUsers]
        (NationalCode, MobileNumber, FullName, PersonnelCode)
    SELECT 
        LTRIM(RTRIM([کد_ملی])) AS NationalCode,
        LTRIM(RTRIM([تلفن_همراه])) AS MobileNumber,
        LTRIM(RTRIM([نام_و_نام_خانوادگی])) AS FullName,
        LTRIM(RTRIM([کد_پرسنلی])) AS PersonnelCode
    FROM #TempGiftCardUsers
    WHERE [کد_ملی] IS NOT NULL 
        AND LEN(LTRIM(RTRIM([کد_ملی]))) = 10
        AND [تلفن_همراه] IS NOT NULL
        AND LEN(LTRIM(RTRIM([تلفن_همراه]))) = 11
        AND LTRIM(RTRIM([تلفن_همراه])) LIKE '09%'
        AND NOT EXISTS (
            SELECT 1 
            FROM [dbo].[GiftCardEligibleUsers] 
            WHERE NationalCode COLLATE Persian_100_CI_AI = LTRIM(RTRIM([کد_ملی]))
        );
    
    DECLARE @InsertedCount INT = @@ROWCOUNT
    
    PRINT ''
    PRINT '========================================='
    PRINT N'✓ Import موفق!'
    PRINT N'تعداد رکوردهای اضافه شده: ' + CAST(@InsertedCount AS NVARCHAR(10))
    PRINT '========================================='
    
END TRY
BEGIN CATCH
    DECLARE @ErrorMessage2 NVARCHAR(4000) = ERROR_MESSAGE()
    
    PRINT ''
    PRINT '✗ خطا در Insert:'
    PRINT @ErrorMessage2
    PRINT ''
    
    RAISERROR(@ErrorMessage2, 16, 1);
END CATCH
GO

-- پاک کردن جدول موقت
DROP TABLE #TempGiftCardUsers;
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

