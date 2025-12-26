-- =============================================
-- Script: افزودن فیلد GiftStatus به جدول User
-- Database: KhanoumiCore (TransactionDB1)
-- Description: فیلد برای مشخص کردن وضعیت دریافت کارت هدیه کاربر
-- =============================================

USE [KhanoumiCore]
GO

-- بررسی وجود فیلد GiftStatus
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = 'GiftStatus')
BEGIN
    -- افزودن فیلد GiftStatus به جدول User
    ALTER TABLE [dbo].[User]
    ADD [GiftStatus] [tinyint] NOT NULL DEFAULT (0)
    
    -- اضافه کردن توضیحات برای فیلد
    EXEC sys.sp_addextendedproperty 
        @name = N'MS_Description', 
        @value = N'وضعیت دریافت کارت هدیه: 0 = غیرفعال، 1 = فعال', 
        @level0type = N'SCHEMA', @level0name = N'dbo', 
        @level1type = N'TABLE', @level1name = N'User', 
        @level2type = N'COLUMN', @level2name = N'GiftStatus'
    
    PRINT 'فیلد GiftStatus با موفقیت به جدول User اضافه شد.'
END
ELSE
BEGIN
    PRINT 'فیلد GiftStatus از قبل در جدول User وجود دارد.'
END
GO











