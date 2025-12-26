-- =============================================
-- Stored Procedure: CleanGiftCardEligibleUsers
-- Database: KhanoumiCore
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







