-- =============================================
-- Script: ایجاد جدول GiftCardEligibleUsers
-- Database: KhanoumiCore
-- Description: جدول برای ذخیره کاربران واجد شرایط دریافت کارت هدیه
-- =============================================

USE [KhanoumiCore]
GO

-- بررسی وجود جدول
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GiftCardEligibleUsers]') AND type in (N'U'))
BEGIN
    PRINT 'در حال ایجاد جدول GiftCardEligibleUsers...';
    
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
    
    -- ایجاد Index روی NationalCode برای جستجوی سریع‌تر
    CREATE UNIQUE NONCLUSTERED INDEX [IX_GiftCardEligibleUsers_NationalCode] 
    ON [dbo].[GiftCardEligibleUsers] ([NationalCode] ASC);
    
    -- ایجاد Index روی IsProcessed برای فیلتر کردن رکوردهای پردازش نشده
    CREATE NONCLUSTERED INDEX [IX_GiftCardEligibleUsers_IsProcessed] 
    ON [dbo].[GiftCardEligibleUsers] ([IsProcessed] ASC)
    INCLUDE ([NationalCode], [MobileNumber]);
    
    PRINT '✓ جدول GiftCardEligibleUsers با موفقیت ایجاد شد.';
    PRINT '';
    PRINT 'ساختار جدول:';
    PRINT '  - Id: uniqueidentifier (Primary Key, Auto-generated)';
    PRINT '  - NationalCode: nvarchar(10) (Unique, Required)';
    PRINT '  - MobileNumber: nvarchar(11) (Optional)';
    PRINT '  - FullName: nvarchar(200) (Optional)';
    PRINT '  - PersonnelCode: nvarchar(100) (Optional)';
    PRINT '  - IsProcessed: bit (Default: 0)';
    PRINT '  - ProcessedAtUtc: datetime2 (Optional)';
    PRINT '  - CreatedAtUtc: datetime2 (Auto)';
    PRINT '  - UpdatedAtUtc: datetime2 (Auto)';
END
ELSE
BEGIN
    PRINT 'جدول GiftCardEligibleUsers از قبل وجود دارد.';
    PRINT '';
    PRINT 'ساختار فعلی جدول:';
    SELECT 
        c.COLUMN_NAME AS [Column Name],
        c.DATA_TYPE AS [Data Type],
        CASE 
            WHEN c.CHARACTER_MAXIMUM_LENGTH IS NOT NULL 
            THEN c.DATA_TYPE + '(' + CAST(c.CHARACTER_MAXIMUM_LENGTH AS VARCHAR) + ')'
            ELSE c.DATA_TYPE
        END AS [Full Type],
        c.IS_NULLABLE AS [Nullable],
        c.COLUMN_DEFAULT AS [Default Value]
    FROM INFORMATION_SCHEMA.COLUMNS c
    WHERE c.TABLE_SCHEMA = 'dbo'
      AND c.TABLE_NAME = 'GiftCardEligibleUsers'
    ORDER BY c.ORDINAL_POSITION;
END
GO
