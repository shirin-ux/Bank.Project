-- =============================================
-- Script ساده: Insert یک رکورد در GiftCardEligibleUsers
-- Database: KhanoumiCore
-- =============================================

USE [KhanoumiCore]
GO

-- Insert یک رکورد جدید
-- فقط NationalCode اجباری است (باید یکتا باشد)
INSERT INTO [dbo].[GiftCardEligibleUsers]
(
    [NationalCode],
    [MobileNumber],
    [FullName],
    [PersonnelCode]
)
VALUES
(
    N'1234567890',              -- کد ملی (اجباری - باید یکتا باشد)
    N'09123456789',             -- شماره موبایل (اختیاری)
    N'نام و نام خانوادگی',      -- نام کامل (اختیاری)
    N'PERSONNEL001'            -- کد پرسنلی (اختیاری)
);
GO

-- نمایش رکورد اضافه شده
SELECT * FROM [dbo].[GiftCardEligibleUsers] WHERE NationalCode = N'1234567890';
GO







