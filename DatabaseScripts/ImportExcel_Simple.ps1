# =============================================
# اسکریپت ساده: Import Excel به جدول GiftCardEligibleUsers
# =============================================
# این اسکریپت فایل Excel را می‌خواند و در جدول GiftCardEligibleUsers قرار می‌دهد
# =============================================

# ⚠️ تنظیمات - اینجا را تغییر دهید
$ExcelFile = "$env:USERPROFILE\Desktop\یلدا.xlsx"  # مسیر فایل Excel
$SheetName = "Sheet1"                               # نام Sheet
$ServerName = "192.168.87.11"                       # نام سرور SQL
$DatabaseName = "KhanoumiCore"                      # نام دیتابیس
$UserName = "trans"                                  # نام کاربری
$Password = "t@Rrn!r46T5"                           # رمز عبور

Write-Host "=========================================" -ForegroundColor Green
Write-Host "Import Excel به جدول GiftCardEligibleUsers" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# بررسی وجود فایل
if (-not (Test-Path $ExcelFile)) {
    Write-Host "✗ خطا: فایل Excel یافت نشد!" -ForegroundColor Red
    Write-Host "مسیر: $ExcelFile" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "لطفاً:" -ForegroundColor Yellow
    Write-Host "1. فایل Excel را در Desktop قرار دهید" -ForegroundColor Yellow
    Write-Host "2. یا مسیر فایل را در خط 8 تغییر دهید" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ فایل Excel یافت شد: $ExcelFile" -ForegroundColor Green
Write-Host ""

# نصب ماژول ImportExcel
Write-Host "بررسی ماژول ImportExcel..." -ForegroundColor Cyan
if (-not (Get-Module -ListAvailable -Name ImportExcel)) {
    Write-Host "در حال نصب ماژول ImportExcel..." -ForegroundColor Yellow
    try {
        Install-Module -Name ImportExcel -Force -Scope CurrentUser -AllowClobber -ErrorAction Stop
        Write-Host "✓ ماژول نصب شد" -ForegroundColor Green
    } catch {
        Write-Host "✗ خطا در نصب ماژول" -ForegroundColor Red
        Write-Host "لطفاً PowerShell را به صورت Administrator اجرا کنید و دستور زیر را اجرا کنید:" -ForegroundColor Yellow
        Write-Host "Install-Module -Name ImportExcel -Force -Scope AllUsers" -ForegroundColor Cyan
        exit 1
    }
} else {
    Write-Host "✓ ماژول ImportExcel موجود است" -ForegroundColor Green
}

Import-Module ImportExcel
Write-Host ""

# خواندن داده‌ها از Excel
Write-Host "در حال خواندن داده‌ها از Excel..." -ForegroundColor Cyan
try {
    $data = Import-Excel -Path $ExcelFile -WorksheetName $SheetName -ErrorAction Stop
    Write-Host "✓ تعداد رکوردهای خوانده شده: $($data.Count)" -ForegroundColor Green
} catch {
    Write-Host "✗ خطا در خواندن فایل Excel: $_" -ForegroundColor Red
    Write-Host "ممکن است نام Sheet متفاوت باشد. لطفاً در خط 9 تغییر دهید." -ForegroundColor Yellow
    exit 1
}

if ($data.Count -eq 0) {
    Write-Host "✗ هیچ داده‌ای در فایل Excel یافت نشد!" -ForegroundColor Red
    exit 1
}

Write-Host ""

# بررسی ستون‌های مورد نیاز
Write-Host "بررسی ساختار داده‌ها..." -ForegroundColor Cyan
$requiredColumns = @("کد ملی", "تلفن همراه")
$missingColumns = @()

foreach ($col in $requiredColumns) {
    if ($data[0].PSObject.Properties.Name -notcontains $col) {
        $missingColumns += $col
    }
}

if ($missingColumns.Count -gt 0) {
    Write-Host "✗ خطا: ستون‌های زیر در فایل Excel یافت نشد:" -ForegroundColor Red
    $missingColumns | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host ""
    Write-Host "ستون‌های موجود در فایل:" -ForegroundColor Yellow
    $data[0].PSObject.Properties.Name | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    exit 1
}

Write-Host "✓ ساختار داده‌ها صحیح است" -ForegroundColor Green
Write-Host ""

# اتصال به SQL Server
Write-Host "اتصال به SQL Server..." -ForegroundColor Cyan
$connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$UserName;Password=$Password;TrustServerCertificate=True;"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "✓ اتصال برقرار شد" -ForegroundColor Green
} catch {
    Write-Host "✗ خطا در اتصال به SQL Server: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# شروع Import
Write-Host "شروع Import..." -ForegroundColor Cyan
$transaction = $connection.BeginTransaction()
$command = $connection.CreateCommand()
$command.Transaction = $transaction

$insertQuery = @"
INSERT INTO [dbo].[GiftCardEligibleUsers]
    (NationalCode, MobileNumber, FullName, PersonnelCode)
VALUES
    (@NationalCode, @MobileNumber, @FullName, @PersonnelCode)
"@

$command.CommandText = $insertQuery
$command.Parameters.Add("@NationalCode", [System.Data.SqlDbType]::NVarChar, 10) | Out-Null
$command.Parameters.Add("@MobileNumber", [System.Data.SqlDbType]::NVarChar, 11) | Out-Null
$command.Parameters.Add("@FullName", [System.Data.SqlDbType]::NVarChar, 200) | Out-Null
$command.Parameters.Add("@PersonnelCode", [System.Data.SqlDbType]::NVarChar, 50) | Out-Null

$successCount = 0
$skippedCount = 0
$errorCount = 0

foreach ($row in $data) {
    try {
        # استخراج داده‌ها
        $nationalCode = ($row."کد ملی" -replace '\s', '').ToString().Trim()
        $mobileNumber = ($row."تلفن همراه" -replace '\s', '').ToString().Trim()
        $fullName = if ($row."نام و نام خانوادگی") { ($row."نام و نام خانوادگی" -replace '\s+', ' ').ToString().Trim() } else { $null }
        $personnelCode = if ($row."کد پرسنلی") { ($row."کد پرسنلی" -replace '\s', '').ToString().Trim() } else { $null }
        
        # Validation
        if ([string]::IsNullOrWhiteSpace($nationalCode) -or $nationalCode.Length -ne 10) {
            Write-Warning "  ⚠ رد: کد ملی نامعتبر ($nationalCode)"
            $skippedCount++
            continue
        }
        
        if ([string]::IsNullOrWhiteSpace($mobileNumber) -or $mobileNumber.Length -ne 11 -or -not $mobileNumber.StartsWith("09")) {
            Write-Warning "  ⚠ رد: شماره موبایل نامعتبر ($mobileNumber)"
            $skippedCount++
            continue
        }
        
        # بررسی تکراری
        $checkCmd = $connection.CreateCommand()
        $checkCmd.Transaction = $transaction
        $checkCmd.CommandText = "SELECT COUNT(*) FROM [dbo].[GiftCardEligibleUsers] WHERE NationalCode = @Code"
        $checkParam = $checkCmd.Parameters.Add("@Code", [System.Data.SqlDbType]::NVarChar, 10)
        $checkParam.Value = $nationalCode
        $exists = $checkCmd.ExecuteScalar()
        
        if ($exists -gt 0) {
            Write-Warning "  ⚠ رد: کد ملی تکراری ($nationalCode)"
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
        
        # نمایش پیشرفت
        if ($successCount % 50 -eq 0) {
            Write-Host "  ✓ $successCount رکورد import شد..." -ForegroundColor Gray
        }
        
    } catch {
        Write-Warning "  ✗ خطا در رکورد: $_"
        $errorCount++
    }
}

# Commit
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
    Write-Host "✗ خطا در Commit: $_" -ForegroundColor Red
    exit 1
}

# بررسی نهایی
$checkCmd = $connection.CreateCommand()
$checkCmd.CommandText = "SELECT COUNT(*) FROM [dbo].[GiftCardEligibleUsers]"
$totalCount = $checkCmd.ExecuteScalar()
Write-Host "✓ تعداد کل رکوردها در دیتابیس: $totalCount" -ForegroundColor Green

$connection.Close()

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "تکمیل شد!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green







