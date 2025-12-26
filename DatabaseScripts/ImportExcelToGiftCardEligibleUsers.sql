-- =============================================
-- Script: Import لیست Excel به جدول GiftCardEligibleUsers
-- Database: KhanoumiCore
-- Description: این اسکریپت برای import کردن لیست Excel به جدول GiftCardEligibleUsers استفاده می‌شود
-- =============================================

USE [KhanoumiCore]
GO

-- =============================================
-- روش 1: استفاده از OPENROWSET (مستقیم از Excel)
-- =============================================
-- پیش‌نیاز: نصب Microsoft Access Database Engine
-- دانلود: https://www.microsoft.com/en-us/download/details.aspx?id=54920

-- فعال کردن Ad Hoc Distributed Queries (فقط یک بار)
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'Ad Hoc Distributed Queries', 1;
RECONFIGURE;
GO

-- مثال استفاده از OPENROWSET:
/*
INSERT INTO [dbo].[GiftCardEligibleUsers]
    (NationalCode, MobileNumber, FullName, PersonnelCode)
SELECT 
    LTRIM(RTRIM([کد ملی])) AS NationalCode,
    LTRIM(RTRIM([تلفن همراه])) AS MobileNumber,
    LTRIM(RTRIM([نام و نام خانوادگی])) AS FullName,
    LTRIM(RTRIM([کد پرسنلی])) AS PersonnelCode
FROM OPENROWSET('Microsoft.ACE.OLEDB.12.0',
    'Excel 12.0 Xml;HDR=YES;Database=C:\Path\To\Your\File.xlsx;',
    'SELECT * FROM [Sheet1$]')
WHERE [کد ملی] IS NOT NULL 
    AND LEN(LTRIM(RTRIM([کد ملی]))) = 10
    AND LEN(LTRIM(RTRIM([تلفن همراه]))) = 11;
GO
*/

-- =============================================
-- روش 2: استفاده از BULK INSERT از CSV
-- =============================================
-- مرحله 1: فایل Excel را به CSV تبدیل کنید
-- مرحله 2: فایل CSV را در مسیر سرور SQL قرار دهید
-- مرحله 3: اسکریپت زیر را اجرا کنید

-- ایجاد جدول موقت برای import
IF OBJECT_ID('tempdb..#TempGiftCardUsers') IS NOT NULL
    DROP TABLE #TempGiftCardUsers;
GO

CREATE TABLE #TempGiftCardUsers (
    [RowNum] INT IDENTITY(1,1),
    [NationalCode] NVARCHAR(10) NULL,
    [MobileNumber] NVARCHAR(11) NULL,
    [FullName] NVARCHAR(200) NULL,
    [PersonnelCode] NVARCHAR(50) NULL
);
GO

-- Import از CSV (مسیر فایل را تغییر دهید)
/*
BULK INSERT #TempGiftCardUsers
FROM 'C:\Path\To\Your\File.csv'
WITH (
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,  -- اگر ردیف اول header است
    CODEPAGE = '65001'  -- UTF-8 encoding
);
GO

-- پاک کردن رکوردهای نامعتبر و insert به جدول اصلی
INSERT INTO [dbo].[GiftCardEligibleUsers]
    (NationalCode, MobileNumber, FullName, PersonnelCode)
SELECT 
    LTRIM(RTRIM(NationalCode)),
    LTRIM(RTRIM(MobileNumber)),
    LTRIM(RTRIM(FullName)),
    LTRIM(RTRIM(PersonnelCode))
FROM #TempGiftCardUsers
WHERE NationalCode IS NOT NULL 
    AND LEN(LTRIM(RTRIM(NationalCode))) = 10
    AND MobileNumber IS NOT NULL
    AND LEN(LTRIM(RTRIM(MobileNumber))) = 11;

DROP TABLE #TempGiftCardUsers;
GO
*/

-- =============================================
-- روش 3: Import دستی با INSERT (برای لیست کوچک)
-- =============================================
-- این روش برای لیست‌های کوچک (تا 1000 رکورد) مناسب است
-- فایل Excel را باز کنید، داده‌ها را کپی کنید و در VALUES قرار دهید

/*
INSERT INTO [dbo].[GiftCardEligibleUsers]
    (NationalCode, MobileNumber, FullName, PersonnelCode)
VALUES
    ('0014219921', '09377984643', N'امید نعیمی', '440003'),
    ('1091267634', '09126211588', N'مسعود شاه مرادی', '440004'),
    ('0020325509', '09190810722', N'مهسا ثابتی', '440016')
    -- ... ادامه لیست
    ;
GO
*/

-- =============================================
-- روش 4: استفاده از Stored Procedure برای Import از Excel
-- =============================================

IF OBJECT_ID('dbo.usp_ImportGiftCardEligibleUsers', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_ImportGiftCardEligibleUsers;
GO

CREATE PROCEDURE dbo.usp_ImportGiftCardEligibleUsers
    @ExcelFilePath NVARCHAR(500),
    @SheetName NVARCHAR(100) = 'Sheet1$'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @ErrorCount INT = 0;
    
    BEGIN TRY
        -- ایجاد جدول موقت
        IF OBJECT_ID('tempdb..#TempImport') IS NOT NULL
            DROP TABLE #TempImport;
            
        CREATE TABLE #TempImport (
            [NationalCode] NVARCHAR(10) NULL,
            [MobileNumber] NVARCHAR(11) NULL,
            [FullName] NVARCHAR(200) NULL,
            [PersonnelCode] NVARCHAR(50) NULL
        );
        
        -- ساخت Query برای خواندن از Excel
        SET @SQL = N'
        INSERT INTO #TempImport
        SELECT 
            LTRIM(RTRIM([کد ملی])) AS NationalCode,
            LTRIM(RTRIM([تلفن همراه])) AS MobileNumber,
            LTRIM(RTRIM([نام و نام خانوادگی])) AS FullName,
            LTRIM(RTRIM([کد پرسنلی])) AS PersonnelCode
        FROM OPENROWSET(''Microsoft.ACE.OLEDB.12.0'',
            ''Excel 12.0 Xml;HDR=YES;Database=' + @ExcelFilePath + ';'',
            ''SELECT * FROM [' + @SheetName + ']'')
        WHERE [کد ملی] IS NOT NULL;';
        
        EXEC sp_executesql @SQL;
        
        -- بررسی و Insert به جدول اصلی
        INSERT INTO [dbo].[GiftCardEligibleUsers]
            (NationalCode, MobileNumber, FullName, PersonnelCode)
        SELECT 
            LTRIM(RTRIM(NationalCode)),
            LTRIM(RTRIM(MobileNumber)),
            LTRIM(RTRIM(FullName)),
            LTRIM(RTRIM(PersonnelCode))
        FROM #TempImport
        WHERE NationalCode IS NOT NULL 
            AND LEN(LTRIM(RTRIM(NationalCode))) = 10
            AND MobileNumber IS NOT NULL
            AND LEN(LTRIM(RTRIM(MobileNumber))) = 11
            AND NOT EXISTS (
                SELECT 1 
                FROM [dbo].[GiftCardEligibleUsers] 
                WHERE NationalCode = LTRIM(RTRIM(#TempImport.NationalCode))
            );
        
        -- شمارش رکوردهای اضافه شده
        SELECT @@ROWCOUNT AS RecordsInserted;
        
        -- شمارش رکوردهای رد شده
        SELECT @ErrorCount = COUNT(*)
        FROM #TempImport
        WHERE NationalCode IS NULL 
            OR LEN(LTRIM(RTRIM(NationalCode))) <> 10
            OR MobileNumber IS NULL
            OR LEN(LTRIM(RTRIM(MobileNumber))) <> 11;
            
        IF @ErrorCount > 0
            PRINT N'توجه: ' + CAST(@ErrorCount AS NVARCHAR(10)) + N' رکورد به دلیل نامعتبر بودن رد شد.';
        
        -- پاک کردن جدول موقت
        DROP TABLE #TempImport;
        
    END TRY
    BEGIN CATCH
        IF OBJECT_ID('tempdb..#TempImport') IS NOT NULL
            DROP TABLE #TempImport;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- مثال استفاده از Stored Procedure:
/*
EXEC dbo.usp_ImportGiftCardEligibleUsers 
    @ExcelFilePath = N'C:\Path\To\Your\File.xlsx',
    @SheetName = N'Sheet1$';
GO
*/

-- =============================================
-- بررسی و Validation بعد از Import
-- =============================================

-- بررسی تعداد کل رکوردها
SELECT 
    COUNT(*) AS TotalRecords,
    COUNT(DISTINCT NationalCode) AS UniqueNationalCodes,
    COUNT(DISTINCT MobileNumber) AS UniqueMobileNumbers,
    SUM(CASE WHEN IsProcessed = 1 THEN 1 ELSE 0 END) AS ProcessedCount,
    SUM(CASE WHEN IsProcessed = 0 THEN 1 ELSE 0 END) AS PendingCount
FROM [dbo].[GiftCardEligibleUsers];

-- بررسی رکوردهای تکراری (باید خالی باشد)
SELECT 
    NationalCode,
    COUNT(*) AS Count
FROM [dbo].[GiftCardEligibleUsers]
GROUP BY NationalCode
HAVING COUNT(*) > 1;

-- بررسی رکوردهای با کد ملی نامعتبر
SELECT 
    Id,
    NationalCode,
    MobileNumber,
    FullName
FROM [dbo].[GiftCardEligibleUsers]
WHERE LEN(LTRIM(RTRIM(NationalCode))) <> 10
    OR NationalCode IS NULL
    OR LEN(LTRIM(RTRIM(MobileNumber))) <> 11
    OR MobileNumber IS NULL;

-- نمایش چند رکورد نمونه
SELECT TOP 10 
    Id,
    NationalCode,
    MobileNumber,
    FullName,
    PersonnelCode,
    IsProcessed,
    CreatedAtUtc
FROM [dbo].[GiftCardEligibleUsers]
ORDER BY CreatedAtUtc DESC;

-- =============================================
-- پاک کردن داده‌های قبلی (در صورت نیاز)
-- =============================================
-- ⚠️ هشدار: این دستور تمام داده‌های جدول را پاک می‌کند!
/*
TRUNCATE TABLE [dbo].[GiftCardEligibleUsers];
GO
*/

-- یا پاک کردن فقط رکوردهای پردازش نشده:
/*
DELETE FROM [dbo].[GiftCardEligibleUsers]
WHERE IsProcessed = 0;
GO
*/

PRINT '========================================='
PRINT 'اسکریپت Import آماده است.'
PRINT 'لطفاً یکی از روش‌های بالا را انتخاب و اجرا کنید.'
PRINT '========================================='
GO







