-- =============================================
-- Script ساده: Import Excel از Desktop
-- Database: KhanoumiCore
-- =============================================
-- ⚠️ فقط نام فایل Excel را در خط 15 تغییر دهید
-- =============================================

USE [KhanoumiCore]
GO

-- =============================================
-- فعال کردن Ad Hoc Distributed Queries
-- =============================================
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'Ad Hoc Distributed Queries', 1;
RECONFIGURE;
GO

-- =============================================
-- ⚠️ اینجا نام فایل Excel خود را وارد کنید
-- =============================================
DECLARE @FileName NVARCHAR(255) = N'یلدا.xlsx'  -- نام فایل Excel
DECLARE @SheetName NVARCHAR(100) = N'Sheet1$'   -- نام Sheet (اگر متفاوت است، تغییر دهید)
DECLARE @ExcelFilePath NVARCHAR(500)

-- ساخت مسیر کامل Desktop
-- ⚠️ اگر SUSER_SNAME() کار نکرد، مسیر را دستی وارد کنید:
-- SET @ExcelFilePath = N'C:\Users\YourUsername\Desktop\' + @FileName
SET @ExcelFilePath = N'C:\Users\' + SUSER_SNAME() + N'\Desktop\' + @FileName

PRINT '========================================='
PRINT 'شروع Import از Desktop...'
PRINT 'مسیر فایل: ' + @ExcelFilePath
PRINT '========================================='

BEGIN TRY
    -- Import مستقیم به جدول
    INSERT INTO [dbo].[GiftCardEligibleUsers]
        (NationalCode, MobileNumber, FullName, PersonnelCode)
    SELECT 
        LTRIM(RTRIM(CAST([کد ملی] AS NVARCHAR(10)))) AS NationalCode,
        LTRIM(RTRIM(CAST([تلفن همراه] AS NVARCHAR(11)))) AS MobileNumber,
        LTRIM(RTRIM(CAST([نام و نام خانوادگی] AS NVARCHAR(200)))) AS FullName,
        LTRIM(RTRIM(CAST([کد پرسنلی] AS NVARCHAR(50)))) AS PersonnelCode
    FROM OPENROWSET('Microsoft.ACE.OLEDB.12.0',
        'Excel 12.0 Xml;HDR=YES;Database=' + @ExcelFilePath + ';',
        'SELECT * FROM [' + @SheetName + ']')
    WHERE [کد ملی] IS NOT NULL 
        AND LEN(LTRIM(RTRIM(CAST([کد ملی] AS NVARCHAR(10))))) = 10
        AND [تلفن همراه] IS NOT NULL
        AND LEN(LTRIM(RTRIM(CAST([تلفن همراه] AS NVARCHAR(11))))) = 11
        AND LTRIM(RTRIM(CAST([تلفن همراه] AS NVARCHAR(11)))) LIKE '09%'
        AND NOT EXISTS (
            SELECT 1 
            FROM [dbo].[GiftCardEligibleUsers] 
            WHERE NationalCode = LTRIM(RTRIM(CAST([کد ملی] AS NVARCHAR(10))))
        );
    
    DECLARE @InsertedCount INT = @@ROWCOUNT
    
    PRINT ''
    PRINT '========================================='
    PRINT N'✓ Import موفق! تعداد رکوردهای اضافه شده: ' + CAST(@InsertedCount AS NVARCHAR(10))
    PRINT '========================================='
    
END TRY
BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
    
    PRINT ''
    PRINT '✗ خطا در Import:'
    PRINT @ErrorMessage
    PRINT ''
    PRINT 'راهنمای رفع مشکل:'
    PRINT '1. مطمئن شوید فایل Excel در Desktop قرار دارد'
    PRINT '2. نام فایل را در خط 14 بررسی کنید'
    PRINT '3. مطمئن شوید فایل Excel بسته است'
    PRINT '4. اگر خطای "provider is not registered" گرفتید، Microsoft Access Database Engine را نصب کنید'
    PRINT '5. اگر مسیر Desktop درست نیست، خط 18 را تغییر دهید و مسیر کامل را وارد کنید'
    
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH
GO

-- =============================================
-- بررسی نتایج
-- =============================================
PRINT ''
PRINT 'بررسی نتایج:'
SELECT 
    COUNT(*) AS TotalRecords,
    COUNT(DISTINCT NationalCode) AS UniqueNationalCodes,
    SUM(CASE WHEN IsProcessed = 0 THEN 1 ELSE 0 END) AS PendingCount
FROM [dbo].[GiftCardEligibleUsers];

PRINT ''
PRINT 'نمونه رکوردهای Import شده:'
SELECT TOP 5 
    NationalCode,
    MobileNumber,
    FullName,
    CreatedAtUtc
FROM [dbo].[GiftCardEligibleUsers]
ORDER BY CreatedAtUtc DESC;

GO

