-- =============================================
-- Script: Import فایل یلدا از CSV (نسخه بهبود یافته با Debugging)
-- Database: KhanoumiCore
-- Description: Import با BULK INSERT + UPDATE/INSERT منطقی + بررسی مشکلات
-- =============================================
-- ⚠️ توجه: فایل CSV باید در مسیری باشد که SQL Server به آن دسترسی دارد
-- پیشنهاد: فایل را به C:\Import\ یا مسیر مشترک منتقل کنید
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    -- =============================================
    -- 0) بررسی وجود جدول GiftCardEligibleUsers
    -- =============================================
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GiftCardEligibleUsers]') AND type in (N'U'))
    BEGIN
        PRINT '⚠️ جدول GiftCardEligibleUsers وجود ندارد. در حال ایجاد...';
        
        CREATE TABLE [dbo].[GiftCardEligibleUsers](
            [Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT (NEWID()),
            [NationalCode] [nvarchar](10) NOT NULL,
            [MobileNumber] [nvarchar](11) NULL,
            [FullName] [nvarchar](200) NULL,
            [PersonnelCode] [nvarchar](100) NULL,
            [IsProcessed] [bit] NOT NULL DEFAULT (0),
            [ProcessedAtUtc] [datetime2](7) NULL,
            [CreatedAtUtc] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
            [UpdatedAtUtc] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT [PK_GiftCardEligibleUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
        ) ON [PRIMARY];
        
        CREATE UNIQUE NONCLUSTERED INDEX [IX_GiftCardEligibleUsers_NationalCode] 
        ON [dbo].[GiftCardEligibleUsers] ([NationalCode] ASC);
        
        PRINT '✓ جدول GiftCardEligibleUsers با موفقیت ایجاد شد.';
    END
    ELSE
    BEGIN
        PRINT '✓ جدول GiftCardEligibleUsers وجود دارد.';
    END
    PRINT '';

    -- =============================================
    -- تنظیمات
    -- =============================================
    DECLARE @FilePath NVARCHAR(4000) = N'C:\Import\یلدا.csv'; -- ⚠️ مسیر فایل را تغییر دهید
    -- اگر فایل در Desktop است و SQL Server به آن دسترسی ندارد، به مسیر مشترک منتقل کنید
    -- مثال: DECLARE @FilePath NVARCHAR(4000) = N'C:\Temp\یلدا.csv';

    PRINT '=========================================';
    PRINT 'شروع Import از فایل CSV';
    PRINT 'مسیر فایل: ' + @FilePath;
    PRINT '=========================================';
    PRINT '';

    -- =============================================
    -- بررسی وجود فایل (با استفاده از xp_fileexist)
    -- =============================================
    DECLARE @FileExists INT;
    DECLARE @FileIsDirectory INT;
    DECLARE @ParentDirectoryExists INT;
    
    EXEC master.dbo.xp_fileexist @FilePath, 
        @FileExists OUTPUT, 
        @FileIsDirectory OUTPUT, 
        @ParentDirectoryExists OUTPUT;
    
    IF @FileExists = 0
    BEGIN
        PRINT '⚠️ هشدار: فایل در مسیر مشخص شده یافت نشد!';
        PRINT 'مسیر: ' + @FilePath;
        PRINT 'ممکن است SQL Server به این مسیر دسترسی نداشته باشد.';
        PRINT 'لطفاً فایل را به مسیری منتقل کنید که SQL Server به آن دسترسی دارد.';
        PRINT '';
    END
    ELSE
    BEGIN
        PRINT '✓ فایل یافت شد.';
        PRINT '';
    END

    -- =============================================
    -- 1) Temp خام برای BULK INSERT (همه NVARCHAR تا خطای تبدیل نخوریم)
    -- استفاده از DATABASE_DEFAULT برای collation
    -- =============================================
    IF OBJECT_ID('tempdb..#GiftCardRaw') IS NOT NULL DROP TABLE #GiftCardRaw;

    CREATE TABLE #GiftCardRaw
    (
        [کد_ملی]           NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL,
        [تلفن_همراه]       NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL,
        [نام_و_نام_خانوادگی] NVARCHAR(200) COLLATE DATABASE_DEFAULT NULL,
        [کد_پرسنلی]        NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL
    );

    PRINT 'در حال خواندن فایل CSV...';

    -- ⚠️ توجه: BULK INSERT نمی‌تواند از متغیر استفاده کند
    -- باید مسیر را مستقیماً در دستور BULK INSERT بنویسید
    -- خط زیر را با مسیر واقعی فایل CSV خود تغییر دهید:
    
    BEGIN TRY
        BULK INSERT #GiftCardRaw
        FROM 'C:\Import\یلدا.csv'  -- ⚠️ اینجا مسیر فایل را تغییر دهید
        WITH
        (
            FIRSTROW = 2,              -- ردیف اول header است
            FIELDTERMINATOR = ',',
            ROWTERMINATOR = '0x0A',    -- اگر خطا خورد: '0x0D0A' امتحان کنید
            CODEPAGE = '65001',        -- UTF-8
            TABLOCK,
            ERRORFILE = 'C:\Import\یلدا_خطا.txt'  -- فایل خطاها
        );
        
        PRINT '✓ BULK INSERT با موفقیت انجام شد.';
    END TRY
    BEGIN CATCH
        DECLARE @BulkError NVARCHAR(4000) = ERROR_MESSAGE();
        PRINT '✗ خطا در BULK INSERT:';
        PRINT @BulkError;
        PRINT '';
        PRINT 'راهنمای رفع مشکل:';
        PRINT '1. مطمئن شوید فایل CSV در مسیر C:\Import\یلدا.csv قرار دارد';
        PRINT '2. مطمئن شوید SQL Server Service Account به این مسیر دسترسی دارد';
        PRINT '3. اگر خطای "Cannot bulk load" گرفتید:';
        PRINT '   - فایل را به مسیر مشترک منتقل کنید';
        PRINT '   - یا از اسکریپت PowerShell استفاده کنید';
        PRINT '4. اگر خطای Collation گرفتید:';
        PRINT '   - مطمئن شوید فایل CSV با UTF-8 ذخیره شده است';
        PRINT '';
        THROW;
    END CATCH

    DECLARE @RawCount INT = (SELECT COUNT(*) FROM #GiftCardRaw);
    PRINT '✓ تعداد رکوردهای خوانده شده: ' + CAST(@RawCount AS NVARCHAR(10));
    
    IF @RawCount = 0
    BEGIN
        PRINT '⚠️ هشدار: هیچ رکوردی از فایل CSV خوانده نشد!';
        PRINT 'ممکن است:';
        PRINT '1. فایل خالی باشد';
        PRINT '2. FIRSTROW = 2 باشد و فایل فقط header داشته باشد';
        PRINT '3. ROWTERMINATOR اشتباه باشد';
        PRINT '';
        
        -- نمایش نمونه داده‌های خوانده شده
        SELECT TOP 5 * FROM #GiftCardRaw;
    END
    PRINT '';

    -- نمایش نمونه داده‌های خام
    PRINT 'نمونه داده‌های خوانده شده (5 رکورد اول):';
    SELECT TOP 5 * FROM #GiftCardRaw;
    PRINT '';

    -- =============================================
    -- 2) تبدیل + پاکسازی + Validation داخل #Final
    -- =============================================
    IF OBJECT_ID('tempdb..#Final') IS NOT NULL DROP TABLE #Final;
    IF OBJECT_ID('tempdb..#Invalid') IS NOT NULL DROP TABLE #Invalid;

    ;WITH Parsed AS
    (
        SELECT
            -- پاکسازی کد ملی: حذف فاصله، خط تیره و کاراکترهای غیرعددی
            NationalCode = NULLIF(
                REPLACE(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM([کد_ملی])), ' ', ''), '-', ''), '_', ''), CHAR(9), ''), 
                ''),
            -- پاکسازی شماره موبایل: حذف فاصله، خط تیره، پرانتز و کاراکترهای غیرعددی
            MobileNumber = NULLIF(
                REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                    LTRIM(RTRIM([تلفن_همراه])), 
                    ' ', ''), '-', ''), '(', ''), ')', ''), '_', ''), CHAR(9), ''),
                ''),
            -- پاکسازی نام: حذف فاصله‌های اضافی و کاراکترهای کنترل
            FullName = NULLIF(
                REPLACE(REPLACE(LTRIM(RTRIM(REPLACE(REPLACE([نام_و_نام_خانوادگی], CHAR(13), ''), CHAR(10), ''))), 
                    '  ', ' '), CHAR(9), ' '),
                ''),
            -- پاکسازی کد پرسنلی
            PersonnelCode = NULLIF(
                REPLACE(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM([کد_پرسنلی])), ' ', ''), '-', ''), '_', ''), CHAR(9), ''),
                ''),
            OriginalData = CAST([کد_ملی] AS NVARCHAR(100)) + ' | ' + 
                          CAST([تلفن_همراه] AS NVARCHAR(100)) + ' | ' + 
                          CAST([نام_و_نام_خانوادگی] AS NVARCHAR(200))
        FROM #GiftCardRaw
    ),
    Normalized AS
    (
        SELECT
            NationalCode,
            -- نرمال‌سازی شماره موبایل: تبدیل +98 به 0
            MobileNumber = CASE 
                WHEN MobileNumber LIKE '+98%' THEN '0' + SUBSTRING(MobileNumber, 4, LEN(MobileNumber))
                WHEN MobileNumber LIKE '98%' AND LEN(MobileNumber) >= 10 THEN '0' + SUBSTRING(MobileNumber, 3, LEN(MobileNumber))
                WHEN MobileNumber LIKE '9%' AND LEN(MobileNumber) = 9 THEN '0' + MobileNumber
                ELSE MobileNumber
            END,
            FullName,
            PersonnelCode,
            OriginalData
        FROM Parsed
    ),
    Clean AS
    (
        SELECT
            NationalCode,
            MobileNumber,
            FullName,
            PersonnelCode,
            OriginalData,
            -- Validation (بهبود یافته)
            IsValid = CASE
                WHEN NationalCode IS NOT NULL 
                     AND LEN(NationalCode) >= 8
                     AND LEN(NationalCode) <= 10
                     AND NationalCode NOT LIKE '%[^0-9]%'  -- فقط عدد باشد
                THEN 1
                ELSE 0
            END,
            ValidationError = CASE
                WHEN NationalCode IS NULL THEN 'کد ملی خالی است'
                WHEN NationalCode LIKE '%[^0-9]%' THEN 'کد ملی شامل کاراکتر غیرعددی است: ' + NationalCode
                WHEN LEN(NationalCode) < 8 THEN 'کد ملی کمتر از 8 رقم است: ' + NationalCode
                WHEN LEN(NationalCode) > 10 THEN 'کد ملی بیشتر از 10 رقم است: ' + NationalCode
                WHEN MobileNumber IS NOT NULL 
                     AND MobileNumber NOT LIKE '%[^0-9]%'  -- فقط عدد باشد
                     AND LEN(MobileNumber) < 10 THEN 'شماره موبایل کمتر از 10 رقم است: ' + MobileNumber
                WHEN MobileNumber IS NOT NULL 
                     AND MobileNumber NOT LIKE '%[^0-9]%'
                     AND MobileNumber NOT LIKE '09%' 
                     AND LEN(MobileNumber) = 11 THEN 'شماره موبایل باید با 09 شروع شود: ' + MobileNumber
                WHEN MobileNumber IS NOT NULL 
                     AND MobileNumber LIKE '%[^0-9]%' THEN 'شماره موبایل شامل کاراکتر غیرعددی است: ' + MobileNumber
                ELSE NULL
            END,
            rn = ROW_NUMBER() OVER
                 (
                    PARTITION BY NationalCode
                    ORDER BY 
                        CASE WHEN FullName IS NOT NULL THEN 1 ELSE 2 END,
                        CASE WHEN PersonnelCode IS NOT NULL THEN 1 ELSE 2 END,
                        CASE WHEN MobileNumber IS NOT NULL THEN 1 ELSE 2 END
                 )
        FROM Normalized
        WHERE NationalCode IS NOT NULL  -- حداقل کد ملی باید وجود داشته باشد
    )
    SELECT 
        NationalCode,
        MobileNumber,
        FullName,
        PersonnelCode
    INTO #Final
    FROM Clean
    WHERE IsValid = 1 
      AND rn = 1;  -- فقط اولین رکورد برای هر کد ملی

    -- ذخیره رکوردهای نامعتبر برای بررسی
    SELECT 
        OriginalData,
        ValidationError
    INTO #Invalid
    FROM Clean
    WHERE IsValid = 0 OR rn > 1;

    DECLARE @ValidCount INT = (SELECT COUNT(*) FROM #Final);
    DECLARE @InvalidCount INT = (SELECT COUNT(*) FROM #Invalid);
    
    PRINT '✓ تعداد رکوردهای معتبر: ' + CAST(@ValidCount AS NVARCHAR(10));
    PRINT '⚠️ تعداد رکوردهای نامعتبر/تکراری: ' + CAST(@InvalidCount AS NVARCHAR(10));
    
    IF @InvalidCount > 0
    BEGIN
        PRINT '';
        PRINT '=========================================';
        PRINT 'رکوردهای نامعتبر/تکراری:';
        PRINT '=========================================';
        
        -- نمایش آمار خطاها
        SELECT 
            ValidationError,
            COUNT(*) AS Count
        FROM #Invalid
        GROUP BY ValidationError
        ORDER BY COUNT(*) DESC;
        
        PRINT '';
        PRINT 'نمونه رکوردهای نامعتبر (10 رکورد اول):';
        SELECT TOP 10 * FROM #Invalid;
        PRINT '';
        
        -- ذخیره رکوردهای نامعتبر در فایل (اختیاری - نیاز به دسترسی SQL Server به فایل سیستم)
        -- اگر می‌خواهید رکوردهای نامعتبر را در فایل ذخیره کنید، این بخش را فعال کنید:
        /*
        DECLARE @InvalidFilePath NVARCHAR(4000) = 'C:\Import\یلدا_نامعتبر_' + 
            FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') + '.txt';
        
        DECLARE @BCPCommand NVARCHAR(4000) = 
            'bcp "SELECT OriginalData + '' | خطا: '' + ISNULL(ValidationError, '''') FROM tempdb..#Invalid" ' +
            'queryout "' + @InvalidFilePath + '" -c -T -S ' + @@SERVERNAME;
        
        EXEC xp_cmdshell @BCPCommand;
        
        PRINT '✓ رکوردهای نامعتبر در فایل ذخیره شد: ' + @InvalidFilePath;
        */
    END
    
    IF @ValidCount = 0
    BEGIN
        PRINT '';
        PRINT '⚠️ هشدار: هیچ رکورد معتبری یافت نشد!';
        PRINT 'لطفاً فایل CSV را بررسی کنید.';
        PRINT '';
        
        -- نمایش آمار
        SELECT 
            COUNT(*) AS TotalRows,
            COUNT(DISTINCT [کد_ملی]) AS UniqueNationalCodes,
            SUM(CASE WHEN [کد_ملی] IS NULL OR LTRIM(RTRIM([کد_ملی])) = '' THEN 1 ELSE 0 END) AS EmptyNationalCode,
            SUM(CASE WHEN [تلفن_همراه] IS NULL OR LTRIM(RTRIM([تلفن_همراه])) = '' THEN 1 ELSE 0 END) AS EmptyMobile
        FROM #GiftCardRaw;
        
        ROLLBACK;
        RETURN;
    END
    PRINT '';

    -- =============================================
    -- 3) UPDATE موجودها (اگر اطلاعات جدیدتر یا کامل‌تر است)
    -- =============================================
    UPDATE T
    SET
        T.MobileNumber = ISNULL(F.MobileNumber, T.MobileNumber),
        T.FullName = ISNULL(F.FullName, T.FullName),  -- فقط اگر NULL بود update شود
        T.PersonnelCode = ISNULL(F.PersonnelCode, T.PersonnelCode),
        T.UpdatedAtUtc = SYSUTCDATETIME()
    FROM dbo.GiftCardEligibleUsers AS T
    INNER JOIN #Final AS F
      ON T.NationalCode COLLATE DATABASE_DEFAULT = F.NationalCode COLLATE DATABASE_DEFAULT;

    DECLARE @UpdatedCount INT = @@ROWCOUNT;
    PRINT '✓ تعداد رکوردهای به‌روز شده: ' + CAST(@UpdatedCount AS NVARCHAR(10));
    PRINT '';

    -- =============================================
    -- 4) INSERT جدیدها (بدون Id - Id خودکار ایجاد می‌شود)
    -- =============================================
    INSERT INTO dbo.GiftCardEligibleUsers
        (NationalCode, MobileNumber, FullName, PersonnelCode, CreatedAtUtc, IsProcessed)
    SELECT
        F.NationalCode,
        F.MobileNumber,
        F.FullName,
        F.PersonnelCode,
        SYSUTCDATETIME(),
        0  -- IsProcessed = 0 (پردازش نشده)
    FROM #Final AS F
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.GiftCardEligibleUsers AS T
        WHERE T.NationalCode COLLATE DATABASE_DEFAULT = F.NationalCode COLLATE DATABASE_DEFAULT
    );

    DECLARE @InsertedCount INT = @@ROWCOUNT;
    PRINT '✓ تعداد رکوردهای اضافه شده: ' + CAST(@InsertedCount AS NVARCHAR(10));
    PRINT '';

    COMMIT;

    -- =============================================
    -- نمایش نتایج
    -- =============================================
    PRINT '=========================================';
    PRINT 'Import با موفقیت انجام شد!';
    PRINT '=========================================';
    PRINT 'تعداد رکوردهای خوانده شده: ' + CAST(@RawCount AS NVARCHAR(10));
    PRINT 'تعداد رکوردهای معتبر: ' + CAST(@ValidCount AS NVARCHAR(10));
    PRINT 'تعداد رکوردهای نامعتبر/تکراری: ' + CAST(@InvalidCount AS NVARCHAR(10));
    PRINT 'تعداد رکوردهای به‌روز شده: ' + CAST(@UpdatedCount AS NVARCHAR(10));
    PRINT 'تعداد رکوردهای اضافه شده: ' + CAST(@InsertedCount AS NVARCHAR(10));
    PRINT '=========================================';
    PRINT '';

    -- نمایش خلاصه
    SELECT 
        COUNT(*) AS TotalRecords,
        COUNT(DISTINCT NationalCode) AS UniqueNationalCodes,
        COUNT(DISTINCT MobileNumber) AS UniqueMobileNumbers,
        SUM(CASE WHEN IsProcessed = 0 THEN 1 ELSE 0 END) AS PendingCount,
        SUM(CASE WHEN IsProcessed = 1 THEN 1 ELSE 0 END) AS ProcessedCount
    FROM dbo.GiftCardEligibleUsers;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorNumber INT = ERROR_NUMBER();
    DECLARE @ErrorLine INT = ERROR_LINE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    PRINT '';
    PRINT '=========================================';
    PRINT '✗ خطا در Import';
    PRINT '=========================================';
    PRINT 'Error Number: ' + CAST(@ErrorNumber AS NVARCHAR(10));
    PRINT 'Error Line: ' + CAST(@ErrorLine AS NVARCHAR(10));
    PRINT 'Error Severity: ' + CAST(@ErrorSeverity AS NVARCHAR(10));
    PRINT 'Error State: ' + CAST(@ErrorState AS NVARCHAR(10));
    PRINT 'Error Message: ' + @ErrorMessage;
    PRINT '';
    PRINT 'راهنمای رفع مشکل:';
    PRINT '1. مطمئن شوید فایل CSV در مسیر مشخص شده قرار دارد';
    PRINT '2. مطمئن شوید SQL Server به فایل دسترسی دارد';
    PRINT '3. اگر خطای "Cannot bulk load" گرفتید:';
    PRINT '   - فایل را به مسیر مشترک منتقل کنید (مثلاً C:\Import\)';
    PRINT '   - یا از اسکریپت PowerShell استفاده کنید';
    PRINT '4. اگر خطای Collation گرفتید:';
    PRINT '   - مطمئن شوید فایل CSV با UTF-8 ذخیره شده است';
    PRINT '5. اگر خطای "Invalid column name" گرفتید:';
    PRINT '   - نام ستون‌ها را در CSV بررسی کنید';
    PRINT '6. اگر خطای "String or binary data would be truncated" گرفتید:';
    PRINT '   - طول داده‌ها را بررسی کنید';
    PRINT '';

    SELECT
        @ErrorNumber  AS ErrorNumber,
        @ErrorLine    AS ErrorLine,
        @ErrorSeverity AS ErrorSeverity,
        @ErrorState   AS ErrorState,
        @ErrorMessage AS ErrorMessage;

    THROW;
END CATCH;

GO

-- =============================================
-- Stored Procedure: CleanGiftCardEligibleUsers
-- Description: پاکسازی و نرمال‌سازی داده‌های موجود در جدول GiftCardEligibleUsers
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CleanGiftCardEligibleUsers]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CleanGiftCardEligibleUsers];
GO

CREATE PROCEDURE [dbo].[CleanGiftCardEligibleUsers]
    @DeleteInvalid BIT = 0,           -- اگر 1 باشد، رکوردهای نامعتبر را حذف می‌کند
    @DeleteDuplicates BIT = 1,        -- اگر 1 باشد، رکوردهای تکراری را حذف می‌کند
    @NormalizeData BIT = 1            -- اگر 1 باشد، داده‌ها را نرمال‌سازی می‌کند
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @TotalRecords INT = (SELECT COUNT(*) FROM dbo.GiftCardEligibleUsers);
        DECLARE @CleanedCount INT = 0;
        DECLARE @DeletedInvalidCount INT = 0;
        DECLARE @DeletedDuplicatesCount INT = 0;
        DECLARE @NormalizedCount INT = 0;

        PRINT '=========================================';
        PRINT 'شروع پاکسازی داده‌های GiftCardEligibleUsers';
        PRINT '=========================================';
        PRINT 'تعداد کل رکوردها: ' + CAST(@TotalRecords AS NVARCHAR(10));
        PRINT '';

        -- =============================================
        -- 1) نرمال‌سازی داده‌ها
        -- =============================================
        IF @NormalizeData = 1
        BEGIN
            PRINT 'در حال نرمال‌سازی داده‌ها...';

            -- نرمال‌سازی کد ملی
            UPDATE dbo.GiftCardEligibleUsers
            SET 
                NationalCode = REPLACE(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(NationalCode)), ' ', ''), '-', ''), '_', ''), CHAR(9), ''),
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE NationalCode LIKE '%[^0-9]%' 
               OR NationalCode LIKE '% %'
               OR NationalCode LIKE '%-%'
               OR NationalCode LIKE '%_%';

            SET @NormalizedCount = @@ROWCOUNT;

            -- نرمال‌سازی شماره موبایل
            UPDATE dbo.GiftCardEligibleUsers
            SET 
                MobileNumber = CASE 
                    -- حذف کاراکترهای غیرعددی
                    WHEN MobileNumber LIKE '%[^0-9]%' OR MobileNumber LIKE '% %' OR MobileNumber LIKE '%-%' 
                    THEN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                        LTRIM(RTRIM(MobileNumber)), ' ', ''), '-', ''), '(', ''), ')', ''), '_', ''), CHAR(9), '')
                    ELSE MobileNumber
                END,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE MobileNumber IS NOT NULL 
              AND (MobileNumber LIKE '%[^0-9]%' OR MobileNumber LIKE '% %' OR MobileNumber LIKE '%-%');

            SET @NormalizedCount = @NormalizedCount + @@ROWCOUNT;

            -- تبدیل فرمت شماره موبایل (+98 به 0)
            UPDATE dbo.GiftCardEligibleUsers
            SET 
                MobileNumber = CASE 
                    WHEN MobileNumber LIKE '+98%' THEN '0' + SUBSTRING(MobileNumber, 4, LEN(MobileNumber))
                    WHEN MobileNumber LIKE '98%' AND LEN(MobileNumber) >= 10 THEN '0' + SUBSTRING(MobileNumber, 3, LEN(MobileNumber))
                    WHEN MobileNumber LIKE '9%' AND LEN(MobileNumber) = 9 THEN '0' + MobileNumber
                    ELSE MobileNumber
                END,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE MobileNumber IS NOT NULL
              AND (MobileNumber LIKE '+98%' OR (MobileNumber LIKE '98%' AND LEN(MobileNumber) >= 10) OR (MobileNumber LIKE '9%' AND LEN(MobileNumber) = 9));

            SET @NormalizedCount = @NormalizedCount + @@ROWCOUNT;

            -- نرمال‌سازی نام
            UPDATE dbo.GiftCardEligibleUsers
            SET 
                FullName = REPLACE(REPLACE(LTRIM(RTRIM(REPLACE(REPLACE(FullName, CHAR(13), ''), CHAR(10), ''))), '  ', ' '), CHAR(9), ' '),
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE FullName IS NOT NULL
              AND (FullName LIKE '%  %' OR FullName LIKE '%' + CHAR(13) + '%' OR FullName LIKE '%' + CHAR(10) + '%' OR FullName LIKE '%' + CHAR(9) + '%');

            SET @NormalizedCount = @NormalizedCount + @@ROWCOUNT;

            -- نرمال‌سازی کد پرسنلی
            UPDATE dbo.GiftCardEligibleUsers
            SET 
                PersonnelCode = REPLACE(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(PersonnelCode)), ' ', ''), '-', ''), '_', ''), CHAR(9), ''),
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE PersonnelCode IS NOT NULL
              AND (PersonnelCode LIKE '% %' OR PersonnelCode LIKE '%-%' OR PersonnelCode LIKE '%_%');

            SET @NormalizedCount = @NormalizedCount + @@ROWCOUNT;

            PRINT '✓ تعداد رکوردهای نرمال‌سازی شده: ' + CAST(@NormalizedCount AS NVARCHAR(10));
            PRINT '';
        END

        -- =============================================
        -- 2) حذف رکوردهای تکراری (بر اساس کد ملی)
        -- =============================================
        IF @DeleteDuplicates = 1
        BEGIN
            PRINT 'در حال حذف رکوردهای تکراری...';

            -- پیدا کردن و حذف تکراری‌ها (نگه داشتن اولین رکورد با بیشترین اطلاعات)
            WITH Duplicates AS
            (
                SELECT 
                    Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY NationalCode
                        ORDER BY 
                            CASE WHEN FullName IS NOT NULL THEN 1 ELSE 2 END,
                            CASE WHEN MobileNumber IS NOT NULL THEN 1 ELSE 2 END,
                            CASE WHEN PersonnelCode IS NOT NULL THEN 1 ELSE 2 END,
                            CreatedAtUtc ASC
                    ) AS rn
                FROM dbo.GiftCardEligibleUsers
            )
            DELETE FROM dbo.GiftCardEligibleUsers
            WHERE Id IN (SELECT Id FROM Duplicates WHERE rn > 1);

            SET @DeletedDuplicatesCount = @@ROWCOUNT;
            PRINT '✓ تعداد رکوردهای تکراری حذف شده: ' + CAST(@DeletedDuplicatesCount AS NVARCHAR(10));
            PRINT '';
        END

        -- =============================================
        -- 3) حذف رکوردهای نامعتبر
        -- =============================================
        IF @DeleteInvalid = 1
        BEGIN
            PRINT 'در حال حذف رکوردهای نامعتبر...';

            -- حذف رکوردهایی که کد ملی نامعتبر دارند
            DELETE FROM dbo.GiftCardEligibleUsers
            WHERE NationalCode IS NULL
               OR LEN(NationalCode) < 8
               OR LEN(NationalCode) > 10
               OR NationalCode LIKE '%[^0-9]%';

            SET @DeletedInvalidCount = @@ROWCOUNT;
            PRINT '✓ تعداد رکوردهای نامعتبر حذف شده: ' + CAST(@DeletedInvalidCount AS NVARCHAR(10));
            PRINT '';
        END

        COMMIT;

        -- =============================================
        -- نمایش نتایج
        -- =============================================
        DECLARE @FinalCount INT = (SELECT COUNT(*) FROM dbo.GiftCardEligibleUsers);
        
        PRINT '=========================================';
        PRINT 'پاکسازی با موفقیت انجام شد!';
        PRINT '=========================================';
        PRINT 'تعداد رکوردهای اولیه: ' + CAST(@TotalRecords AS NVARCHAR(10));
        PRINT 'تعداد رکوردهای نرمال‌سازی شده: ' + CAST(@NormalizedCount AS NVARCHAR(10));
        PRINT 'تعداد رکوردهای تکراری حذف شده: ' + CAST(@DeletedDuplicatesCount AS NVARCHAR(10));
        PRINT 'تعداد رکوردهای نامعتبر حذف شده: ' + CAST(@DeletedInvalidCount AS NVARCHAR(10));
        PRINT 'تعداد رکوردهای نهایی: ' + CAST(@FinalCount AS NVARCHAR(10));
        PRINT '=========================================';
        PRINT '';

        -- نمایش آمار نهایی
        SELECT 
            COUNT(*) AS TotalRecords,
            COUNT(DISTINCT NationalCode) AS UniqueNationalCodes,
            COUNT(DISTINCT MobileNumber) AS UniqueMobileNumbers,
            SUM(CASE WHEN IsProcessed = 0 THEN 1 ELSE 0 END) AS PendingCount,
            SUM(CASE WHEN IsProcessed = 1 THEN 1 ELSE 0 END) AS ProcessedCount,
            SUM(CASE WHEN NationalCode IS NULL OR LEN(NationalCode) < 8 OR LEN(NationalCode) > 10 OR NationalCode LIKE '%[^0-9]%' THEN 1 ELSE 0 END) AS InvalidNationalCodeCount,
            SUM(CASE WHEN MobileNumber IS NOT NULL AND (LEN(MobileNumber) < 10 OR MobileNumber NOT LIKE '09%') THEN 1 ELSE 0 END) AS InvalidMobileCount
        FROM dbo.GiftCardEligibleUsers;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        DECLARE @ErrorLine INT = ERROR_LINE();

        PRINT '';
        PRINT '=========================================';
        PRINT '✗ خطا در پاکسازی';
        PRINT '=========================================';
        PRINT 'Error Number: ' + CAST(@ErrorNumber AS NVARCHAR(10));
        PRINT 'Error Line: ' + CAST(@ErrorLine AS NVARCHAR(10));
        PRINT 'Error Message: ' + @ErrorMessage;
        PRINT '';

        SELECT
            @ErrorNumber AS ErrorNumber,
            @ErrorLine AS ErrorLine,
            @ErrorMessage AS ErrorMessage;

        THROW;
    END CATCH;
END;
GO

-- =============================================
-- مثال استفاده:
-- =============================================
-- -- فقط نرمال‌سازی (بدون حذف)
-- EXEC dbo.CleanGiftCardEligibleUsers 
--     @DeleteInvalid = 0, 
--     @DeleteDuplicates = 0, 
--     @NormalizeData = 1;
--
-- -- نرمال‌سازی + حذف تکراری‌ها
-- EXEC dbo.CleanGiftCardEligibleUsers 
--     @DeleteInvalid = 0, 
--     @DeleteDuplicates = 1, 
--     @NormalizeData = 1;
--
-- -- پاکسازی کامل (همه چیز)
-- EXEC dbo.CleanGiftCardEligibleUsers 
--     @DeleteInvalid = 1, 
--     @DeleteDuplicates = 1, 
--     @NormalizeData = 1;
-- =============================================

GO
