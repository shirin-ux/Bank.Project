-- =============================================
-- Script: ایجاد جدول GiftCard
-- Database: KhanoumiCore (TransactionDB1)
-- Description: جدول برای ثبت دریافت کارت هدیه توسط کاربران
-- =============================================

USE [KhanoumiCore]
GO

-- ایجاد جدول GiftCard
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[GiftCard](
        [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] [uniqueidentifier] NOT NULL,
        [OrderId] [uniqueidentifier] NULL,
        [IsReceived] [bit] NOT NULL DEFAULT 0,
        [IsPending] [bit] NOT NULL DEFAULT 0,
        [CreatedAtUtc] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RowVersion] [rowversion] NOT NULL,
        [ReceivedAtUtc] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
        [Description] [nvarchar](1000) NULL,
        CONSTRAINT [PK_GiftCard] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY]
    
    -- ایجاد Index روی UserId برای جستجوی سریع‌تر
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = 'IX_GiftCard_UserId')
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_GiftCard_UserId] 
        ON [dbo].[GiftCard] ([UserId] ASC)
        INCLUDE ([ReceivedAtUtc], [IsReceived])
    END
    
    PRINT 'جدول GiftCard با موفقیت ایجاد شد.'
END
ELSE
BEGIN
    -- اضافه کردن فیلدهای جدید اگر جدول از قبل وجود دارد
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = 'IsReceived')
    BEGIN
        ALTER TABLE [dbo].[GiftCard]
        ADD [IsReceived] [bit] NOT NULL DEFAULT 0;
        PRINT 'فیلد IsReceived به جدول GiftCard اضافه شد.'
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = 'OrderId')
    BEGIN
        ALTER TABLE [dbo].[GiftCard]
        ADD [OrderId] [uniqueidentifier] NULL;
        PRINT 'فیلد OrderId به جدول GiftCard اضافه شد.'
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = 'IsPending')
    BEGIN
        ALTER TABLE [dbo].[GiftCard]
        ADD [IsPending] [bit] NOT NULL DEFAULT 0;
        PRINT 'فیلد IsPending به جدول GiftCard اضافه شد.'
    END
    
    -- ایجاد Index اگر وجود نداشته باشد
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = 'IX_GiftCard_UserId')
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_GiftCard_UserId] 
        ON [dbo].[GiftCard] ([UserId] ASC)
        INCLUDE ([ReceivedAtUtc], [IsReceived])
        PRINT 'Index IX_GiftCard_UserId ایجاد شد.'
    END
    
    PRINT 'جدول GiftCard از قبل وجود دارد.'
END
GO

