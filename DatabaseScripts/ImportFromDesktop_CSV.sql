-- =============================================
-- Script: Import از CSV (بدون نیاز به Access Database Engine)
-- Database: KhanoumiCore
-- Description: این روش از CSV استفاده می‌کند و نیاز به Access Database Engine ندارد
-- =============================================
-- ⚠️ ابتدا فایل Excel را به CSV تبدیل کنید
-- =============================================

USE [KhanoumiCore]
GO

-- =============================================
-- مرحله 1: تبدیل Excel به CSV
-- =============================================
-- 1. فایل Excel را باز کنید
-- 2. File → Save As
-- 3. نوع فایل را "CSV (Comma delimited) (*.csv)" انتخاب کنید
-- 4. فایل را در Desktop ذخیره کنید

-- =============================================
-- مرحله 2: Import از CSV
-- =============================================

-- ⚠️ اینجا نام فایل CSV خود را وارد کنید
DECLARE @FileName NVARCHAR(255) = N'YourFile.csv'  -- ⚠️ نام فایل CSV را تغییر دهید
DECLARE @CSVFilePath NVARCHAR(500) = N'C:\Users\' + SUSER_SNAME() + N'\Desktop\' + @FileName

-- اگر SUSER_SNAME() کار نکرد، مسیر را دستی وارد کنید:
-- SET @CSVFilePath = N'C:\Users\YourUsername\Desktop\YourFile.csv'

PRINT '========================================='
PRINT 'شروع Import از CSV...'
PRINT 'مسیر فایل: ' + @CSVFilePath
PRINT '========================================='
GO

-- =============================================
-- ایجاد جدول موقت
-- =============================================
IF OBJECT_ID('tempdb..#TempCSVImport') IS NOT NULL
    DROP TABLE #TempCSVImport;
GO

CREATE TABLE #TempCSVImport (
    [RowNum] INT IDENTITY(1,1),
    [Col1] NVARCHAR(MAX) NULL,  -- کد ملی
    [Col2] NVARCHAR(MAX) NULL,  -- تلفن همراه
    [Col3] NVARCHAR(MAX) NULL,  -- نام و نام خانوادگی
    [Col4] NVARCHAR(MAX) NULL   -- کد پرسنلی
);
GO

-- =============================================
-- Import از CSV با BULK INSERT
-- =============================================
DECLARE @FileName NVARCHAR(255) = N'YourFile.csv'  -- ⚠️ تغییر دهید
DECLARE @CSVFilePath NVARCHAR(500) = N'C:\Users\' + SUSER_SNAME() + N'\Desktop\' + @FileName

BEGIN TRY
    -- Import از CSV
    -- ⚠️ توجه: BULK INSERT نیاز به دسترسی به فایل در سرور SQL دارد
    -- اگر فایل در Desktop شماست، باید آن را به یک مسیر مشترک منتقل کنید
    -- یا از روش PowerShell استفاده کنید
    
    -- روش 1: اگر فایل در سرور SQL است
    /*
    BULK INSERT #TempCSVImport
    FROM 'C:\Path\On\SQL\Server\YourFile.csv'
    WITH (
        FIELDTERMINATOR = ',',
        ROWTERMINATOR = '\n',
        FIRSTROW = 2,  -- اگر ردیف اول header است
        CODEPAGE = '65001',  -- UTF-8
        ERRORFILE = 'C:\Path\On\SQL\Server\errors.txt'
    );
    */
    
    -- روش 2: استفاده از OPENROWSET با CSV (نیاز به Access Database Engine ندارد)
    -- اما نیاز به تنظیمات خاص دارد
    
    PRINT '⚠ توجه: BULK INSERT نیاز به دسترسی سرور SQL به فایل دارد.'
    PRINT 'لطفاً از اسکریپت PowerShell استفاده کنید یا فایل را به مسیر مشترک منتقل کنید.'
    
END TRY
BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
    PRINT '✗ خطا: ' + @ErrorMessage
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH
GO

-- =============================================
-- پاک کردن جدول موقت
-- =============================================
IF OBJECT_ID('tempdb..#TempCSVImport') IS NOT NULL
    DROP TABLE #TempCSVImport;
GO

PRINT ''
PRINT 'برای Import از CSV، لطفاً از اسکریپت PowerShell استفاده کنید:'
PRINT 'ImportExcelToSQL.ps1'
GO







