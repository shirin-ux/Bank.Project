# =============================================
# Script PowerShell: Import Excel به SQL Server
# Description: این اسکریپت فایل Excel را می‌خواند و به جدول GiftCardEligibleUsers import می‌کند
# =============================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ExcelFilePath,
    
    [Parameter(Mandatory=$false)]
    [string]$ServerName = "192.168.87.11",
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "KhanoumiCore",
    
    [Parameter(Mandatory=$false)]
    [string]$UserName = "trans",
    
    [Parameter(Mandatory=$false)]
    [string]$Password = "t@Rrn!r46T5",
    
    [Parameter(Mandatory=$false)]
    [string]$SheetName = "Sheet1"
)

# بررسی وجود فایل Excel
if (-not (Test-Path $ExcelFilePath)) {
    Write-Error "فایل Excel یافت نشد: $ExcelFilePath"
    exit 1
}

Write-Host "=========================================" -ForegroundColor Green
Write-Host "شروع Import از Excel به SQL Server" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host "فایل Excel: $ExcelFilePath" -ForegroundColor Yellow
Write-Host "سرور: $ServerName" -ForegroundColor Yellow
Write-Host "دیتابیس: $DatabaseName" -ForegroundColor Yellow
Write-Host ""

# نصب ماژول ImportExcel (اگر نصب نشده باشد)
if (-not (Get-Module -ListAvailable -Name ImportExcel)) {
    Write-Host "در حال نصب ماژول ImportExcel..." -ForegroundColor Yellow
    Install-Module -Name ImportExcel -Force -Scope CurrentUser
}

Import-Module ImportExcel

# خواندن داده‌ها از Excel
Write-Host "در حال خواندن داده‌ها از Excel..." -ForegroundColor Cyan
try {
    $excelData = Import-Excel -Path $ExcelFilePath -WorksheetName $SheetName
    Write-Host "✓ تعداد رکوردهای خوانده شده: $($excelData.Count)" -ForegroundColor Green
} catch {
    Write-Error "خطا در خواندن فایل Excel: $_"
    exit 1
}

# بررسی ساختار داده‌ها
$requiredColumns = @("کد ملی", "تلفن همراه", "نام و نام خانوادگی")
$missingColumns = @()

foreach ($col in $requiredColumns) {
    if ($excelData[0].PSObject.Properties.Name -notcontains $col) {
        $missingColumns += $col
    }
}

if ($missingColumns.Count -gt 0) {
    Write-Error "ستون‌های زیر در فایل Excel یافت نشد: $($missingColumns -join ', ')"
    Write-Host "ستون‌های موجود: $($excelData[0].PSObject.Properties.Name -join ', ')" -ForegroundColor Yellow
    exit 1
}

# آماده‌سازی Connection String
$connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$UserName;Password=$Password;TrustServerCertificate=True;"

# ایجاد SQL Connection
Write-Host "در حال اتصال به SQL Server..." -ForegroundColor Cyan
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "✓ اتصال برقرار شد" -ForegroundColor Green
} catch {
    Write-Error "خطا در اتصال به SQL Server: $_"
    exit 1
}

# شروع Transaction
$transaction = $connection.BeginTransaction()
$command = $connection.CreateCommand()
$command.Transaction = $transaction

# آماده‌سازی Query
$insertQuery = @"
INSERT INTO [dbo].[GiftCardEligibleUsers]
    (NationalCode, MobileNumber, FullName, PersonnelCode)
VALUES
    (@NationalCode, @MobileNumber, @FullName, @PersonnelCode)
"@

$command.CommandText = $insertQuery

# اضافه کردن Parameters
$command.Parameters.Add("@NationalCode", [System.Data.SqlDbType]::NVarChar, 10) | Out-Null
$command.Parameters.Add("@MobileNumber", [System.Data.SqlDbType]::NVarChar, 11) | Out-Null
$command.Parameters.Add("@FullName", [System.Data.SqlDbType]::NVarChar, 200) | Out-Null
$command.Parameters.Add("@PersonnelCode", [System.Data.SqlDbType]::NVarChar, 50) | Out-Null

# Import داده‌ها
$successCount = 0
$errorCount = 0
$skippedCount = 0

Write-Host "در حال Import داده‌ها..." -ForegroundColor Cyan

foreach ($row in $excelData) {
    try {
        # استخراج و پاکسازی داده‌ها
        $nationalCode = ($row."کد ملی" -replace '\s', '').ToString().Trim()
        $mobileNumber = ($row."تلفن همراه" -replace '\s', '').ToString().Trim()
        $fullName = ($row."نام و نام خانوادگی" -replace '\s+', ' ').ToString().Trim()
        $personnelCode = if ($row."کد پرسنلی") { ($row."کد پرسنلی" -replace '\s', '').ToString().Trim() } else { $null }
        
        # Validation
        if ([string]::IsNullOrWhiteSpace($nationalCode) -or $nationalCode.Length -ne 10) {
            Write-Warning "رد کردن رکورد: کد ملی نامعتبر ($nationalCode)"
            $skippedCount++
            continue
        }
        
        if ([string]::IsNullOrWhiteSpace($mobileNumber) -or $mobileNumber.Length -ne 11 -or -not $mobileNumber.StartsWith("09")) {
            Write-Warning "رد کردن رکورد: شماره موبایل نامعتبر ($mobileNumber)"
            $skippedCount++
            continue
        }
        
        # بررسی تکراری بودن
        $checkQuery = "SELECT COUNT(*) FROM [dbo].[GiftCardEligibleUsers] WHERE NationalCode = @NationalCode"
        $checkCommand = $connection.CreateCommand()
        $checkCommand.Transaction = $transaction
        $checkCommand.CommandText = $checkQuery
        $checkCommand.Parameters.Add("@NationalCode", [System.Data.SqlDbType]::NVarChar, 10).Value = $nationalCode
        $exists = $checkCommand.ExecuteScalar()
        
        if ($exists -gt 0) {
            Write-Warning "رد کردن رکورد: کد ملی تکراری ($nationalCode)"
            $skippedCount++
            continue
        }
        
        # Insert
        $command.Parameters["@NationalCode"].Value = $nationalCode
        $command.Parameters["@MobileNumber"].Value = $mobileNumber
        $command.Parameters["@FullName"].Value = if ([string]::IsNullOrWhiteSpace($fullName)) { [DBNull]::Value } else { $fullName }
        $command.Parameters["@PersonnelCode"].Value = if ([string]::IsNullOrWhiteSpace($personnelCode)) { [DBNull]::Value } else { $personnelCode }
        
        $command.ExecuteNonQuery() | Out-Null
        $successCount++
        
        if ($successCount % 50 -eq 0) {
            Write-Host "  ✓ $successCount رکورد import شد..." -ForegroundColor Gray
        }
        
    } catch {
        Write-Warning "خطا در import رکورد: $_"
        $errorCount++
    }
}

# Commit Transaction
try {
    $transaction.Commit()
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "Import با موفقیت انجام شد!" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "✓ رکوردهای موفق: $successCount" -ForegroundColor Green
    Write-Host "⚠ رکوردهای رد شده: $skippedCount" -ForegroundColor Yellow
    Write-Host "✗ رکوردهای خطا: $errorCount" -ForegroundColor Red
    Write-Host ""
} catch {
    $transaction.Rollback()
    Write-Error "خطا در Commit: $_"
}

# بستن Connection
$connection.Close()

# بررسی نهایی
Write-Host "در حال بررسی نهایی..." -ForegroundColor Cyan
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $checkCommand = $connection.CreateCommand()
    $checkCommand.CommandText = "SELECT COUNT(*) FROM [dbo].[GiftCardEligibleUsers]"
    $totalCount = $checkCommand.ExecuteScalar()
    Write-Host "✓ تعداد کل رکوردها در دیتابیس: $totalCount" -ForegroundColor Green
    $connection.Close()
} catch {
    Write-Warning "خطا در بررسی نهایی: $_"
}

Write-Host ""
Write-Host "Import تکمیل شد!" -ForegroundColor Green







