-- =============================================
-- Script SQL ساده: Import Excel
-- =============================================
-- فقط مسیر فایل Excel را در خط 15 تغییر دهید و اجرا کنید
-- =============================================

USE [KhanoumiCore]
GO

-- فعال کردن Ad Hoc Distributed Queries
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'Ad Hoc Distributed Queries', 1;
RECONFIGURE;
GO

-- ⚠️ اینجا مسیر فایل Excel خود را وارد کنید
DECLARE @ExcelFilePath NVARCHAR(500) = N'C:\Users\YourUsername\Desktop\یلدا.xlsx'  -- ⚠️ تغییر دهید
DECLARE @SheetName NVARCHAR(100) = N'Sheet1$'
DECLARE @SQL NVARCHAR(MAX)

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

BEGIN TRY
    EXEC sp_executesql @SQL
    
    DECLARE @InsertedCount INT = @@ROWCOUNT
    PRINT N'✓ Import انجام شد. تعداد رکوردهای اضافه شده: ' + CAST(@InsertedCount AS NVARCHAR(10))
END TRY
BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
    
    PRINT ''
    PRINT '✗ خطا در Import:'
    PRINT @ErrorMessage
    PRINT ''
    
    IF @ErrorMessage LIKE '%provider is not registered%' OR @ErrorMessage LIKE '%Microsoft.ACE.OLEDB%'
    BEGIN
        PRINT '========================================='
        PRINT 'راه‌حل: Microsoft Access Database Engine نصب نیست'
        PRINT '========================================='
        PRINT ''
        PRINT 'روش 1: نصب Access Database Engine'
        PRINT '  1. دانلود: https://www.microsoft.com/en-us/download/details.aspx?id=54920'
        PRINT '  2. نسخه 64-bit یا 32-bit را بسته به SQL Server نصب کنید'
        PRINT '  3. SQL Server را Restart کنید'
        PRINT ''
        PRINT 'روش 2: استفاده از SQL Server Import Wizard (ساده‌ترین)'
        PRINT '  1. SSMS → دیتابیس KhanoumiCore → راست کلیک'
        PRINT '  2. Tasks → Import Data...'
        PRINT '  3. فایل Excel را انتخاب کنید'
        PRINT ''
        PRINT 'روش 3: استفاده از اسکریپت PowerShell'
        PRINT '  .\ImportExcel_Simple.ps1'
        PRINT ''
    END
    ELSE
    BEGIN
        PRINT 'لطفاً:'
        PRINT '  1. مسیر فایل Excel را بررسی کنید'
        PRINT '  2. مطمئن شوید فایل Excel بسته است'
        PRINT '  3. نام Sheet را بررسی کنید'
    END
    
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH
GO

-- بررسی نتایج
SELECT COUNT(*) AS TotalRecords FROM [dbo].[GiftCardEligibleUsers]
GO

