-- =============================================
-- Script: Insert یک رکورد در جدول GiftCardEligibleUsers
-- Database: KhanoumiCore
-- Description: افزودن یک کاربر به لیست کاربران واجد شرایط دریافت کارت هدیه
-- =============================================

USE [KhanoumiCore]
GO

-- بررسی وجود جدول
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GiftCardEligibleUsers]') AND type in (N'U'))
BEGIN
    PRINT '✗ خطا: جدول GiftCardEligibleUsers وجود ندارد!';
    PRINT 'لطفاً ابتدا اسکریپت CreateGiftCardEligibleUsersTable.sql را اجرا کنید.';
    RETURN;
END
GO

-- Insert یک رکورد جدید
-- توجه: NationalCode فیلد اجباری و Unique است
INSERT INTO [dbo].[GiftCardEligibleUsers]
(
    [NationalCode],
    [MobileNumber],
    [FullName],
    [PersonnelCode],
    [IsProcessed],
    [ProcessedAtUtc]
)
VALUES
(
    N'1234567890',              -- NationalCode (اجباری - باید یکتا باشد)
    N'09123456789',             -- MobileNumber (اختیاری)
    N'نام و نام خانوادگی',      -- FullName (اختیاری)
    N'PERSONNEL001',            -- PersonnelCode (اختیاری)
    0,                          -- IsProcessed (0 = پردازش نشده)
    NULL                        -- ProcessedAtUtc (NULL = هنوز پردازش نشده)
);
GO

-- بررسی موفقیت‌آمیز بودن Insert
IF @@ROWCOUNT > 0
BEGIN
    PRINT '✓ رکورد با موفقیت اضافه شد.';
    PRINT '';
    
    -- نمایش رکورد اضافه شده
    SELECT 
        Id,
        NationalCode,
        MobileNumber,
        FullName,
        PersonnelCode,
        IsProcessed,
        ProcessedAtUtc,
        CreatedAtUtc,
        UpdatedAtUtc
    FROM [dbo].[GiftCardEligibleUsers]
    WHERE NationalCode = N'1234567890';
END
ELSE
BEGIN
    PRINT '✗ خطا: رکورد اضافه نشد.';
END
GO

-- =============================================
-- مثال‌های دیگر برای Insert:
-- =============================================

/*
-- مثال 1: Insert فقط با NationalCode (حداقل فیلدهای مورد نیاز)
INSERT INTO [dbo].[GiftCardEligibleUsers]
(
    [NationalCode]
)
VALUES
(
    N'9876543210'
);
GO

-- مثال 2: Insert با تمام فیلدها
INSERT INTO [dbo].[GiftCardEligibleUsers]
(
    [NationalCode],
    [MobileNumber],
    [FullName],
    [PersonnelCode],
    [IsProcessed],
    [ProcessedAtUtc]
)
VALUES
(
    N'1111111111',
    N'09111111111',
    N'علی احمدی',
    N'EMP001',
    0,
    NULL
);
GO

-- مثال 3: Insert با بررسی عدم تکراری بودن NationalCode
IF NOT EXISTS (SELECT 1 FROM [dbo].[GiftCardEligibleUsers] WHERE NationalCode = N'2222222222')
BEGIN
    INSERT INTO [dbo].[GiftCardEligibleUsers]
    (
        [NationalCode],
        [MobileNumber],
        [FullName]
    )
    VALUES
    (
        N'2222222222',
        N'09222222222',
        N'محمد رضایی'
    );
    PRINT '✓ رکورد اضافه شد.';
END
ELSE
BEGIN
    PRINT '✗ خطا: این کد ملی قبلاً ثبت شده است.';
END
GO
*/







