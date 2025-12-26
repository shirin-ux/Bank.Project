-- =============================================
-- Script: Import فایل یلدا از CSV (نسخه پیشرفته)
-- Database: KhanoumiCore
-- Description: Import با BULK INSERT + UPDATE/INSERT منطقی
-- =============================================
-- ⚠️ توجه: فایل CSV باید در مسیری باشد که SQL Server به آن دسترسی دارد
-- پیشنهاد: فایل را به C:\Import\ یا مسیر مشترک منتقل کنید
-- =============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

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
    
    BULK INSERT #GiftCardRaw
    FROM 'C:\Import\یلدا.csv'  -- ⚠️ اینجا مسیر فایل را تغییر دهید
    WITH
    (
        FIRSTROW = 2,              -- ردیف اول header است
        FIELDTERMINATOR = ',',
        ROWTERMINATOR = '0x0A',    -- اگر خطا خورد: '0x0D0A' امتحان کنید
        CODEPAGE = '65001',        -- UTF-8
        TABLOCK
    );

    DECLARE @RawCount INT = (SELECT COUNT(*) FROM #GiftCardRaw);
    PRINT '✓ تعداد رکوردهای خوانده شده: ' + CAST(@RawCount AS NVARCHAR(10));
    PRINT '';

    -- =============================================
    -- 2) تبدیل + پاکسازی + Validation داخل #Final
    -- =============================================
    IF OBJECT_ID('tempdb..#Final') IS NOT NULL DROP TABLE #Final;

    ;WITH Parsed AS
    (
        SELECT
            NationalCode = NULLIF(LTRIM(RTRIM([کد_ملی])), ''),
            MobileNumber = NULLIF(LTRIM(RTRIM([تلفن_همراه])), ''),
            FullName = NULLIF(LTRIM(RTRIM([نام_و_نام_خانوادگی])), ''),
            PersonnelCode = NULLIF(LTRIM(RTRIM([کد_پرسنلی])), '')
        FROM #GiftCardRaw
    ),
    Clean AS
    (
        SELECT
            NationalCode,
            MobileNumber,
            FullName,
            PersonnelCode,
            -- Validation
            IsValid = CASE
                WHEN NationalCode IS NOT NULL 
                     AND LEN(NationalCode) = 10
                     AND MobileNumber IS NOT NULL
                     AND LEN(MobileNumber) = 11
                     AND MobileNumber LIKE '09%'
                THEN 1
                ELSE 0
            END,
            rn = ROW_NUMBER() OVER
                 (
                    PARTITION BY NationalCode
                    ORDER BY 
                        CASE WHEN FullName IS NOT NULL THEN 1 ELSE 2 END,
                        CASE WHEN PersonnelCode IS NOT NULL THEN 1 ELSE 2 END
                 )
        FROM Parsed
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

    DECLARE @ValidCount INT = (SELECT COUNT(*) FROM #Final);
    PRINT '✓ تعداد رکوردهای معتبر: ' + CAST(@ValidCount AS NVARCHAR(10));
    PRINT '';

    -- =============================================
    -- 3) UPDATE موجودها (اگر اطلاعات جدیدتر یا کامل‌تر است)
    -- =============================================
    UPDATE T
    SET
        T.MobileNumber = F.MobileNumber,
        T.FullName = ISNULL(F.FullName, T.FullName),  -- فقط اگر NULL بود update شود
        T.PersonnelCode = ISNULL(F.PersonnelCode, T.PersonnelCode),
        T.UpdatedAtUtc = SYSUTCDATETIME()
    FROM dbo.GiftCardEligibleUsers AS T
    JOIN #Final AS F
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

    PRINT '';
    PRINT '=========================================';
    PRINT '✗ خطا در Import';
    PRINT '=========================================';
    PRINT 'Error Number: ' + CAST(@ErrorNumber AS NVARCHAR(10));
    PRINT 'Error Line: ' + CAST(@ErrorLine AS NVARCHAR(10));
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
    PRINT '';

    SELECT
        @ErrorNumber  AS ErrorNumber,
        @ErrorLine    AS ErrorLine,
        @ErrorMessage AS ErrorMessage;

    THROW;
END CATCH;

GO

