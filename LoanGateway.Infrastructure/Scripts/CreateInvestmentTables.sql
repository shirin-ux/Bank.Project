-- =============================================
-- Script ایجاد جداول سرمایه‌گذاری
-- این اسکریپت جداول InvestmentAccount و InvestmentOperation را ایجاد می‌کند
-- Database: BNPLDB (TransactionDB)
-- =============================================

USE [BNPLDB]
GO

-- حذف جداول در صورت وجود (برای development/testing)
-- IF OBJECT_ID('dbo.InvestmentOperation', 'U') IS NOT NULL DROP TABLE dbo.InvestmentOperation;
-- IF OBJECT_ID('dbo.InvestmentAccount', 'U') IS NOT NULL DROP TABLE dbo.InvestmentAccount;

-- =============================================
-- جدول InvestmentAccount
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvestmentAccount]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InvestmentAccount]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [ProviderPolicyId] UNIQUEIDENTIFIER NULL, -- PolicyId از سرویس کاریزما
        [NationalCode] NVARCHAR(10) NULL, -- کد ملی کاربر
        [BirthDate] NVARCHAR(10) NULL, -- تاریخ تولد (فرمت: yyyy/MM/dd)
        [PlanCode] INT NULL, -- نوع طرح سرمایه‌گذاری (1=Gold, 2=Silver, 3=FixedIncome)
        
        [PostalCode] NVARCHAR(10) NULL,
        [Address] NVARCHAR(500) NULL,
        
        [State] INT NOT NULL DEFAULT 0, -- وضعیت حساب (0=CreatedWithoutDeposit, 1=Active, 2=Closed, 3=Suspended)
        
        -- خلاصه وضعیت مالی
        [TotalInvested] DECIMAL(18, 2) NOT NULL DEFAULT 0, -- جمع کل سرمایه‌گذاری‌های انجام شده
        [TotalWithdrawn] DECIMAL(18, 2) NOT NULL DEFAULT 0, -- جمع کل برداشت‌های انجام شده
        [CurrentValue] DECIMAL(18, 2) NOT NULL DEFAULT 0, -- ارزش روز دارایی
        [RevokableAmount] DECIMAL(18, 2) NOT NULL DEFAULT 0, -- مبلغ قابل برداشت
        [CollateralAmount] DECIMAL(18, 2) NOT NULL DEFAULT 0, -- مبلغ وثیقه‌شده
        
        [LastTraceId] NVARCHAR(100) NULL, -- آخرین TraceId استفاده‌شده
        
        -- BaseEntity fields
        [CreatedAtUtc] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAtUtc] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [RowVersion] ROWVERSION NULL -- برای Optimistic Concurrency
    )
    
    -- Index برای جستجوی سریع‌تر
    CREATE NONCLUSTERED INDEX [IX_InvestmentAccount_NationalCode_PlanCode] 
    ON [dbo].[InvestmentAccount] ([NationalCode], [PlanCode])
    INCLUDE ([ProviderPolicyId], [State], [TotalInvested], [RevokableAmount]);
    
    CREATE NONCLUSTERED INDEX [IX_InvestmentAccount_ProviderPolicyId] 
    ON [dbo].[InvestmentAccount] ([ProviderPolicyId])
    WHERE [ProviderPolicyId] IS NOT NULL;
    
    PRINT 'جدول InvestmentAccount با موفقیت ایجاد شد.'
END
ELSE
BEGIN
    PRINT 'جدول InvestmentAccount قبلاً وجود دارد.'
END
GO

-- =============================================
-- جدول InvestmentOperation
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvestmentOperation]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InvestmentOperation]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [PolicyId] UNIQUEIDENTIFIER NULL, -- PolicyId از سرویس کاریزما (FK به InvestmentAccount.ProviderPolicyId)
        [TraceId] NVARCHAR(100) NULL, -- شناسه رهگیری عملیات
        
        [Type] INT NOT NULL, -- نوع عملیات (0=CreateAccount, 1=IncreaseDirect, 2=IncreaseOnline, 3=DecreaseDirect)
        [Status] TINYINT NOT NULL DEFAULT 1, -- وضعیت (1=Created, 2=PendingPayment, 3=Paid, 4=Failed, 5=Cancelled)
        
        [Amount] DECIMAL(18, 2) NOT NULL DEFAULT 0, -- مبلغ عملیات
        [OperationDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), -- تاریخ عملیات
        [ReceiptDate] DATETIME2 NULL, -- تاریخ رسید/فیش
        [ReceiptNumber] NVARCHAR(100) NULL, -- شماره مرجع پرداخت/رسید
        
        [Description] NVARCHAR(500) NULL, -- توضیحات
        
        -- BaseEntity fields
        [CreatedAtUtc] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAtUtc] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [RowVersion] ROWVERSION NULL -- برای Optimistic Concurrency
    )
    
    -- Index برای جستجوی سریع‌تر
    CREATE NONCLUSTERED INDEX [IX_InvestmentOperation_PolicyId] 
    ON [dbo].[InvestmentOperation] ([PolicyId])
    WHERE [PolicyId] IS NOT NULL;
    
    CREATE NONCLUSTERED INDEX [IX_InvestmentOperation_PolicyId_ReceiptNumber] 
    ON [dbo].[InvestmentOperation] ([PolicyId], [ReceiptNumber])
    WHERE [PolicyId] IS NOT NULL AND [ReceiptNumber] IS NOT NULL;
    
    CREATE NONCLUSTERED INDEX [IX_InvestmentOperation_TraceId] 
    ON [dbo].[InvestmentOperation] ([TraceId])
    WHERE [TraceId] IS NOT NULL;
    
    CREATE NONCLUSTERED INDEX [IX_InvestmentOperation_Type_Status] 
    ON [dbo].[InvestmentOperation] ([Type], [Status]);
    
    PRINT 'جدول InvestmentOperation با موفقیت ایجاد شد.'
END
ELSE
BEGIN
    PRINT 'جدول InvestmentOperation قبلاً وجود دارد.'
END
GO

-- =============================================
-- توضیحات Enum ها
-- =============================================
/*
-- InvestmentState (برای فیلد State در InvestmentAccount):
0 = CreatedWithoutDeposit  -- تازه ایجاد شده، هنوز شاید سرمایه‌گذاری نشده
1 = Active                 -- دارای سرمایه‌گذاری فعال
2 = Closed                 -- حساب بسته شده
3 = Suspended              -- حساب تعلیق شده

-- InvestmentPlanType (برای فیلد PlanCode در InvestmentAccount):
1 = Gold                   -- طلا
2 = Silver                 -- نقره
3 = FixedIncome            -- درآمد ثابت

-- InvestmentOperationType (برای فیلد Type در InvestmentOperation):
0 = CreateAccount          -- ایجاد حساب
1 = IncreaseDirect         -- افزایش سرمایه با ثبت فیش/پرداخت انجام‌شده
2 = IncreaseOnline         -- افزایش سرمایه با درگاه آنلاین
3 = DecreaseDirect         -- برداشت مستقیم بعد از پرداخت به کاربر

-- InvestmentOrderState (برای فیلد Status در InvestmentOperation):
1 = Created                -- ساخته شده، هنوز نرفته درگاه
2 = PendingPayment         -- لینک درگاه گرفته شده، منتظر پرداخت
3 = Paid                   -- پرداخت موفق
4 = Failed                 -- پرداخت ناموفق / خطا
5 = Cancelled              -- لغو شده
*/
GO

PRINT 'اسکریپت ایجاد جداول سرمایه‌گذاری با موفقیت اجرا شد.'
GO

