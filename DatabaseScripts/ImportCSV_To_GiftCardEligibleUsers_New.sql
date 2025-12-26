SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @FilePath NVARCHAR(4000) = N'C:\Import\data.csv'; -- مسیر فایل

    --------------------------------------------------------------------
    -- 1) Temp خام برای BULK INSERT (همه NVARCHAR تا خطای تبدیل نخوریم)
    --------------------------------------------------------------------
    IF OBJECT_ID('tempdb..#GiftCardRaw') IS NOT NULL DROP TABLE #GiftCardRaw;

    CREATE TABLE #GiftCardRaw
    (
        NationalCode   NVARCHAR(100) NULL,
        MobileNumber   NVARCHAR(100) NULL,
        FullName       NVARCHAR(200) NULL,
        PersonnelCode  NVARCHAR(100) NULL
    );

    BULK INSERT #GiftCardRaw
    FROM 'C:\Import\data.csv'
    WITH
    (
        FIRSTROW = 2,
        FIELDTERMINATOR = ',',
        ROWTERMINATOR = '0x0A',    -- اگر خطا خورد: '0x0D0A'
        CODEPAGE = '65001',
        TABLOCK
    );

    --------------------------------------------------------------------
    -- 2) تبدیل + پاکسازی + Validation داخل #Final
    --------------------------------------------------------------------
    IF OBJECT_ID('tempdb..#Final') IS NOT NULL DROP TABLE #Final;

    ;WITH Parsed AS
    (
        SELECT
            NationalCode_Str = REPLACE(REPLACE(REPLACE(REPLACE(
                NULLIF(LTRIM(RTRIM(NationalCode)), ''), ' ', ''), '-', ''), '_', ''), CHAR(9), ''),
            MobileNumber_Str = CASE 
                WHEN NULLIF(LTRIM(RTRIM(MobileNumber)), '') LIKE '+98%' 
                THEN '0' + SUBSTRING(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                    NULLIF(LTRIM(RTRIM(MobileNumber)), ''), ' ', ''), '-', ''), '(', ''), ')', ''), '_', ''), CHAR(9), ''), 4, 100)
                WHEN NULLIF(LTRIM(RTRIM(MobileNumber)), '') LIKE '98%' 
                THEN '0' + SUBSTRING(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                    NULLIF(LTRIM(RTRIM(MobileNumber)), ''), ' ', ''), '-', ''), '(', ''), ')', ''), '_', ''), CHAR(9), ''), 3, 100)
                ELSE REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                    NULLIF(LTRIM(RTRIM(MobileNumber)), ''), ' ', ''), '-', ''), '(', ''), ')', ''), '_', ''), CHAR(9), '')
            END,
            FullName_Str = NULLIF(
                REPLACE(REPLACE(LTRIM(RTRIM(REPLACE(REPLACE(FullName, CHAR(13), ''), CHAR(10), ''))), 
                    '  ', ' '), CHAR(9), ' '),
                ''),
            PersonnelCode_Str = NULLIF(
                REPLACE(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(PersonnelCode)), ' ', ''), '-', ''), '_', ''), CHAR(9), ''),
                '')
        FROM #GiftCardRaw
    ),
    Clean AS
    (
        SELECT
            NationalCode = NationalCode_Str,
            MobileNumber = MobileNumber_Str,
            FullName = FullName_Str,
            PersonnelCode = PersonnelCode_Str,
            rn = ROW_NUMBER() OVER
                 (
                    PARTITION BY NationalCode_Str
                    ORDER BY 
                        CASE WHEN FullName_Str IS NOT NULL THEN 1 ELSE 2 END,
                        CASE WHEN MobileNumber_Str IS NOT NULL THEN 1 ELSE 2 END,
                        CASE WHEN PersonnelCode_Str IS NOT NULL THEN 1 ELSE 2 END
                 )
        FROM Parsed
        WHERE
            NationalCode_Str IS NOT NULL
            AND LEN(NationalCode_Str) >= 8
            AND LEN(NationalCode_Str) <= 10
            AND NationalCode_Str NOT LIKE '%[^0-9]%'  -- فقط عدد باشد
    )
    SELECT NationalCode, MobileNumber, FullName, PersonnelCode
    INTO #Final
    FROM Clean
    WHERE rn = 1;

    --------------------------------------------------------------------
    -- 3) UPDATE موجودها
    --------------------------------------------------------------------
    UPDATE T
    SET
        T.MobileNumber = ISNULL(F.MobileNumber, T.MobileNumber),
        T.FullName = ISNULL(F.FullName, T.FullName),
        T.PersonnelCode = ISNULL(F.PersonnelCode, T.PersonnelCode),
        T.UpdatedAtUtc = SYSUTCDATETIME()
    FROM dbo.GiftCardEligibleUsers AS T
    JOIN #Final AS F
      ON T.NationalCode COLLATE DATABASE_DEFAULT = F.NationalCode COLLATE DATABASE_DEFAULT;

    --------------------------------------------------------------------
    -- 4) INSERT جدیدها (بدون Id)
    --------------------------------------------------------------------
    INSERT INTO dbo.GiftCardEligibleUsers
        (NationalCode, MobileNumber, FullName, PersonnelCode, CreatedAtUtc, UpdatedAtUtc, IsProcessed)
    SELECT
        F.NationalCode, F.MobileNumber, F.FullName, F.PersonnelCode,
        SYSUTCDATETIME(), SYSUTCDATETIME(), 0
    FROM #Final AS F
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.GiftCardEligibleUsers AS T
        WHERE T.NationalCode COLLATE DATABASE_DEFAULT = F.NationalCode COLLATE DATABASE_DEFAULT
    );

    COMMIT;

    SELECT COUNT(*) AS ImportedRows FROM #Final;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;

    SELECT
        ERROR_NUMBER()  AS ErrorNumber,
        ERROR_LINE()    AS ErrorLine,
        ERROR_MESSAGE() AS ErrorMessage;
END CATCH;

