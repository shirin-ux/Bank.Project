-- =============================================
-- Script: اصلاح فیلد IsPending در جدول GiftCard
-- Database: KhanoumiCore (TransactionDB1)
-- Description: اگر فیلد IsPending وجود دارد اما nullable است، آن را به NOT NULL تبدیل می‌کند
--              اگر فیلد وجود ندارد، آن را اضافه می‌کند
-- =============================================

USE [KhanoumiCore]
GO

-- بررسی وجود جدول
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND type in (N'U'))
BEGIN
    -- بررسی وجود فیلد IsPending
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = 'IsPending')
    BEGIN
        -- اگر فیلد nullable است، ابتدا مقادیر NULL را به 0 تبدیل می‌کنیم
        UPDATE [dbo].[GiftCard]
        SET [IsPending] = 0
        WHERE [IsPending] IS NULL;
        
        -- سپس constraint NOT NULL را اضافه می‌کنیم
        -- ابتدا constraint موجود را حذف می‌کنیم (اگر وجود دارد)
        DECLARE @ConstraintName NVARCHAR(200);
        SELECT @ConstraintName = name
        FROM sys.default_constraints
        WHERE parent_object_id = OBJECT_ID(N'[dbo].[GiftCard]')
          AND parent_column_id = (SELECT column_id FROM sys.columns 
                                   WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') 
                                   AND name = 'IsPending');
        
        IF @ConstraintName IS NOT NULL
        BEGIN
            EXEC('ALTER TABLE [dbo].[GiftCard] DROP CONSTRAINT ' + @ConstraintName);
        END
        
        -- تغییر فیلد به NOT NULL با DEFAULT
        ALTER TABLE [dbo].[GiftCard]
        ALTER COLUMN [IsPending] [bit] NOT NULL;
        
        ALTER TABLE [dbo].[GiftCard]
        ADD CONSTRAINT [DF_GiftCard_IsPending] DEFAULT (0) FOR [IsPending];
        
        PRINT 'فیلد IsPending به NOT NULL با DEFAULT 0 تبدیل شد.'
    END
    ELSE
    BEGIN
        -- اگر فیلد وجود ندارد، آن را اضافه می‌کنیم
        ALTER TABLE [dbo].[GiftCard]
        ADD [IsPending] [bit] NOT NULL DEFAULT 0;
        
        PRINT 'فیلد IsPending به جدول GiftCard اضافه شد.'
    END
END
ELSE
BEGIN
    PRINT 'جدول GiftCard وجود ندارد. لطفاً ابتدا اسکریپت CreateGiftCardTable.sql را اجرا کنید.'
END
GO




