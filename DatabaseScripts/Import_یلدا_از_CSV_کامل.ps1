# =============================================
# اسکریپت کامل: Import از CSV به SQL (بدون نیاز به Access Database Engine)
# =============================================
# این اسکریپت فایل CSV را می‌خواند و به جدول GiftCardEligibleUsers import می‌کند
# =============================================

# ⚠️ تنظیمات
$CsvFile = "$env:USERPROFILE\Desktop\یلدا.csv"     # مسیر فایل CSV
$ServerName = "192.168.87.11"                       # نام سرور SQL
$DatabaseName = "KhanoumiCore"                      # نام دیتابیس
$UserName = "trans"                                  # نام کاربری
$Password = "t@Rrn!r46T5"                           # رمز عبور

Write-Host "=========================================" -ForegroundColor Green
Write-Host "Import فایل CSV به جدول GiftCardEligibleUsers" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# بررسی وجود فایل CSV
if (-not (Test-Path $CsvFile)) {
    Write-Host "✗ خطا: فایل CSV یافت نشد!" -ForegroundColor Red
    Write-Host "مسیر: $CsvFile" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "لطفاً:" -ForegroundColor Yellow
    Write-Host "1. فایل 'یلدا.csv' را در Desktop قرار دهید" -ForegroundColor Yellow
    Write-Host "2. یا فایل Excel را به CSV تبدیل کنید:" -ForegroundColor Yellow
    Write-Host "   - فایل Excel را در Excel باز کنید" -ForegroundColor Cyan
    Write-Host "   - File → Save As → CSV UTF-8 (Comma delimited)" -ForegroundColor Cyan
    Write-Host "   - یا از اسکریپت تبدیل استفاده کنید: تبدیل_Excel_به_CSV.ps1" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

Write-Host "✓ فایل CSV یافت شد: $CsvFile" -ForegroundColor Green
Write-Host ""

# خواندن داده‌ها از CSV
Write-Host "در حال خواندن داده‌ها از CSV..." -ForegroundColor Cyan
try {
    # خواندن CSV با encoding UTF-8
    $data = Import-Csv -Path $CsvFile -Encoding UTF8 -ErrorAction Stop
    Write-Host "✓ تعداد رکوردهای خوانده شده: $($data.Count)" -ForegroundColor Green
} catch {
    Write-Host "✗ خطا در خواندن فایل CSV: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "ممکن است:" -ForegroundColor Yellow
    Write-Host "1. فایل CSV با encoding UTF-8 ذخیره نشده باشد" -ForegroundColor Yellow
    Write-Host "2. ساختار فایل CSV درست نباشد" -ForegroundColor Yellow
    Write-Host "3. فایل CSV باز باشد (لطفاً ببندید)" -ForegroundColor Yellow
    exit 1
}

if ($data.Count -eq 0) {
    Write-Host "✗ هیچ داده‌ای در فایل CSV یافت نشد!" -ForegroundColor Red
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
    Write-Host "✗ خطا: ستون‌های زیر در فایل CSV یافت نشد:" -ForegroundColor Red
    $missingColumns | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host ""
    Write-Host "ستون‌های موجود در فایل:" -ForegroundColor Yellow
    $data[0].PSObject.Properties.Name | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    Write-Host ""
    Write-Host "لطفاً نام ستون‌ها را در فایل Excel بررسی کنید و دوباره به CSV تبدیل کنید." -ForegroundColor Yellow
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
    Write-Host ""
    Write-Host "لطفاً اطلاعات اتصال را در خطوط 10-13 بررسی کنید" -ForegroundColor Yellow
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
        # استخراج و پاکسازی داده‌ها
        $nationalCode = ($row."کد ملی" -replace '\s', '').ToString().Trim()
        $mobileNumber = ($row."تلفن همراه" -replace '\s', '').ToString().Trim()
        $fullName = if ($row."نام و نام خانوادگی") { ($row."نام و نام خانوادگی" -replace '\s+', ' ').ToString().Trim() } else { $null }
        $personnelCode = if ($row."کد پرسنلی") { ($row."کد پرسنلی" -replace '\s', '').ToString().Trim() } else { $null }
        
        # Validation
        if ([string]::IsNullOrWhiteSpace($nationalCode) -or $nationalCode.Length -ne 10) {
            $skippedCount++
            continue
        }
        
        if ([string]::IsNullOrWhiteSpace($mobileNumber) -or $mobileNumber.Length -ne 11 -or -not $mobileNumber.StartsWith("09")) {
            $skippedCount++
            continue
        }
        
        # بررسی تکراری بودن
        $checkCmd = $connection.CreateCommand()
        $checkCmd.Transaction = $transaction
        $checkCmd.CommandText = "SELECT COUNT(*) FROM [dbo].[GiftCardEligibleUsers] WHERE NationalCode = @Code"
        $checkParam = $checkCmd.Parameters.Add("@Code", [System.Data.SqlDbType]::NVarChar, 10)
        $checkParam.Value = $nationalCode
        $exists = $checkCmd.ExecuteScalar()
        
        if ($exists -gt 0) {
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
        $errorCount++
    }
}

# Commit
try {
    $transaction.Commit()
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "✓ Import با موفقیت انجام شد!" -ForegroundColor Green
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







