-- =============================================
-- Script: Import لیست Excel از Desktop به جدول GiftCardEligibleUsers
-- Database: KhanoumiCore
-- Description: این اسکریپت فایل Excel را از Desktop می‌خواند و import می‌کند
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
-- مرحله 2: تعیین مسیر فایل Excel
-- =============================================
-- ⚠️ توجه: مسیر Desktop را تغییر دهید
-- برای Windows: C:\Users\[YourUsername]\Desktop\YourFile.xlsx
-- یا از متغیر محیطی استفاده کنید

DECLARE @ExcelFilePath NVARCHAR(500)
DECLARE @SheetName NVARCHAR(100) = 'Sheet1$'  -- نام Sheet را تغییر دهید اگر متفاوت است

-- =============================================
-- ⚠️ این خط را تغییر دهید: نام فایل Excel خود را وارد کنید
-- =============================================
SET @ExcelFilePath = N'C:\Users\YourUsername\Desktop\YourFile.xlsx'
-- یا اگر می‌خواهید از متغیر محیطی استفاده کنید:
-- SET @ExcelFilePath = N'C:\Users\' + SUSER_SNAME() + N'\Desktop\YourFile.xlsx'

-- =============================================
-- مرحله 3: Import داده‌ها
-- =============================================
PRINT '========================================='
PRINT 'شروع Import از فایل Excel...'
PRINT 'مسیر فایل: ' + @ExcelFilePath
PRINT '========================================='
GO

BEGIN TRY
    -- ایجاد جدول موقت برای بررسی داده‌ها
    IF OBJECT_ID('tempdb..#TempImport') IS NOT NULL
        DROP TABLE #TempImport;
    
    CREATE TABLE #TempImport (
        [NationalCode] NVARCHAR(10) NULL,
        [MobileNumber] NVARCHAR(11) NULL,
        [FullName] NVARCHAR(200) NULL,
        [PersonnelCode] NVARCHAR(50) NULL,
        [RowNum] INT IDENTITY(1,1)
    );
    
    -- خواندن داده‌ها از Excel
    DECLARE @SQL NVARCHAR(MAX)
    DECLARE @ExcelFilePath NVARCHAR(500) = N'C:\Users\YourUsername\Desktop\YourFile.xlsx'  -- ⚠️ تغییر دهید
    DECLARE @SheetName NVARCHAR(100) = N'Sheet1$'  -- ⚠️ تغییر دهید اگر نام Sheet متفاوت است
    
    SET @SQL = N'
    INSERT INTO #TempImport (NationalCode, MobileNumber, FullName, PersonnelCode)
    SELECT 
        LTRIM(RTRIM(CAST([کد ملی] AS NVARCHAR(10)))) AS NationalCode,
        LTRIM(RTRIM(CAST([تلفن همراه] AS NVARCHAR(11)))) AS MobileNumber,
        LTRIM(RTRIM(CAST([نام و نام خانوادگی] AS NVARCHAR(200)))) AS FullName,
        LTRIM(RTRIM(CAST([کد پرسنلی] AS NVARCHAR(50)))) AS PersonnelCode
    FROM OPENROWSET(''Microsoft.ACE.OLEDB.12.0'',
        ''Excel 12.0 Xml;HDR=YES;Database=' + @ExcelFilePath + ';'',
        ''SELECT * FROM [' + @SheetName + ']'')
    WHERE [کد ملی] IS NOT NULL;'
    
    EXEC sp_executesql @SQL;
    
    PRINT '✓ داده‌ها از Excel خوانده شد.'
    
    -- نمایش تعداد رکوردهای خوانده شده
    DECLARE @TotalRead INT
    SELECT @TotalRead = COUNT(*) FROM #TempImport
    PRINT N'تعداد رکوردهای خوانده شده: ' + CAST(@TotalRead AS NVARCHAR(10))
    
    -- Validation و Insert به جدول اصلی
    DECLARE @InsertedCount INT = 0
    DECLARE @SkippedCount INT = 0
    DECLARE @ErrorCount INT = 0
    
    -- Insert رکوردهای معتبر
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
        AND LTRIM(RTRIM(MobileNumber)) LIKE '09%'
        AND NOT EXISTS (
            SELECT 1 
            FROM [dbo].[GiftCardEligibleUsers] 
            WHERE NationalCode = LTRIM(RTRIM(#TempImport.NationalCode))
        );
    
    SET @InsertedCount = @@ROWCOUNT
    
    -- شمارش رکوردهای رد شده
    SELECT @SkippedCount = COUNT(*)
    FROM #TempImport
    WHERE NationalCode IS NULL 
        OR LEN(LTRIM(RTRIM(NationalCode))) <> 10
        OR MobileNumber IS NULL
        OR LEN(LTRIM(RTRIM(MobileNumber))) <> 11
        OR LTRIM(RTRIM(MobileNumber)) NOT LIKE '09%'
        OR EXISTS (
            SELECT 1 
            FROM [dbo].[GiftCardEligibleUsers] 
            WHERE NationalCode = LTRIM(RTRIM(#TempImport.NationalCode))
        );
    
    -- نمایش نتایج
    PRINT ''
    PRINT '========================================='
    PRINT 'نتایج Import:'
    PRINT '========================================='
    PRINT N'✓ رکوردهای موفق: ' + CAST(@InsertedCount AS NVARCHAR(10))
    PRINT N'⚠ رکوردهای رد شده: ' + CAST(@SkippedCount AS NVARCHAR(10))
    PRINT '========================================='
    
    -- نمایش رکوردهای رد شده (برای بررسی)
    IF @SkippedCount > 0
    BEGIN
        PRINT ''
        PRINT 'رکوردهای رد شده:'
        SELECT TOP 20
            NationalCode,
            MobileNumber,
            FullName,
            CASE 
                WHEN NationalCode IS NULL OR LEN(LTRIM(RTRIM(NationalCode))) <> 10 THEN 'کد ملی نامعتبر'
                WHEN MobileNumber IS NULL OR LEN(LTRIM(RTRIM(MobileNumber))) <> 11 THEN 'شماره موبایل نامعتبر'
                WHEN LTRIM(RTRIM(MobileNumber)) NOT LIKE '09%' THEN 'شماره موبایل باید با 09 شروع شود'
                ELSE 'کد ملی تکراری'
            END AS Reason
        FROM #TempImport
        WHERE NationalCode IS NULL 
            OR LEN(LTRIM(RTRIM(NationalCode))) <> 10
            OR MobileNumber IS NULL
            OR LEN(LTRIM(RTRIM(MobileNumber))) <> 11
            OR LTRIM(RTRIM(MobileNumber)) NOT LIKE '09%'
            OR EXISTS (
                SELECT 1 
                FROM [dbo].[GiftCardEligibleUsers] 
                WHERE NationalCode = LTRIM(RTRIM(#TempImport.NationalCode))
            );
    END
    
    -- پاک کردن جدول موقت
    DROP TABLE #TempImport;
    
    PRINT ''
    PRINT '✓ Import با موفقیت انجام شد!'
    
END TRY
BEGIN CATCH
    IF OBJECT_ID('tempdb..#TempImport') IS NOT NULL
        DROP TABLE #TempImport;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
    DECLARE @ErrorState INT = ERROR_STATE()
    
    PRINT ''
    PRINT '✗ خطا در Import:'
    PRINT @ErrorMessage
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
GO

-- =============================================
-- مرحله 4: بررسی نهایی
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'بررسی نهایی:'
PRINT '========================================='

SELECT 
    COUNT(*) AS TotalRecords,
    COUNT(DISTINCT NationalCode) AS UniqueNationalCodes,
    COUNT(DISTINCT MobileNumber) AS UniqueMobileNumbers,
    SUM(CASE WHEN IsProcessed = 1 THEN 1 ELSE 0 END) AS ProcessedCount,
    SUM(CASE WHEN IsProcessed = 0 THEN 1 ELSE 0 END) AS PendingCount
FROM [dbo].[GiftCardEligibleUsers];

-- بررسی رکوردهای تکراری (باید خالی باشد)
IF EXISTS (
    SELECT 1 
    FROM [dbo].[GiftCardEligibleUsers]
    GROUP BY NationalCode
    HAVING COUNT(*) > 1
)
BEGIN
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
    PRINT '✓ هیچ رکورد تکراری یافت نشد.'
END

-- نمایش چند رکورد نمونه
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







