# =============================================
# اسکریپت: تبدیل خودکار Excel به CSV
# =============================================
# این اسکریپت فایل Excel را به CSV تبدیل می‌کند
# =============================================

# ⚠️ تنظیمات
$ExcelFile = "$env:USERPROFILE\Desktop\یلدا.xlsx"  # مسیر فایل Excel
$CsvFile = "$env:USERPROFILE\Desktop\یلدا.csv"     # مسیر فایل CSV خروجی
$SheetName = "Sheet1"                               # نام Sheet

Write-Host "=========================================" -ForegroundColor Green
Write-Host "تبدیل Excel به CSV" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# بررسی وجود فایل Excel
if (-not (Test-Path $ExcelFile)) {
    Write-Host "✗ خطا: فایل Excel یافت نشد!" -ForegroundColor Red
    Write-Host "مسیر: $ExcelFile" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "لطفاً:" -ForegroundColor Yellow
    Write-Host "1. فایل 'یلدا.xlsx' را در Desktop قرار دهید" -ForegroundColor Yellow
    Write-Host "2. یا مسیر فایل را در خط 8 تغییر دهید" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ فایل Excel یافت شد: $ExcelFile" -ForegroundColor Green
Write-Host ""

# نصب ماژول ImportExcel (اگر نصب نشده باشد)
Write-Host "بررسی ماژول ImportExcel..." -ForegroundColor Cyan
if (-not (Get-Module -ListAvailable -Name ImportExcel)) {
    Write-Host "در حال نصب ماژول ImportExcel..." -ForegroundColor Yellow
    try {
        Install-Module -Name ImportExcel -Force -Scope CurrentUser -AllowClobber -ErrorAction Stop
        Write-Host "✓ ماژول ImportExcel نصب شد" -ForegroundColor Green
    } catch {
        Write-Host "✗ خطا در نصب ماژول" -ForegroundColor Red
        Write-Host ""
        Write-Host "راه‌حل:" -ForegroundColor Yellow
        Write-Host "PowerShell را به صورت Administrator اجرا کنید و دستور زیر را اجرا کنید:" -ForegroundColor Yellow
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
    Write-Host ""
    Write-Host "ممکن است:" -ForegroundColor Yellow
    Write-Host "1. نام Sheet متفاوت باشد (خط 10 را تغییر دهید)" -ForegroundColor Yellow
    Write-Host "2. فایل Excel باز باشد (لطفاً ببندید)" -ForegroundColor Yellow
    exit 1
}

if ($data.Count -eq 0) {
    Write-Host "✗ هیچ داده‌ای در فایل Excel یافت نشد!" -ForegroundColor Red
    exit 1
}

Write-Host ""

# تبدیل به CSV
Write-Host "در حال تبدیل به CSV..." -ForegroundColor Cyan
try {
    $data | Export-Csv -Path $CsvFile -Encoding UTF8 -NoTypeInformation -ErrorAction Stop
    Write-Host "✓ فایل CSV ایجاد شد: $CsvFile" -ForegroundColor Green
} catch {
    Write-Host "✗ خطا در ایجاد فایل CSV: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# بررسی فایل CSV
if (Test-Path $CsvFile) {
    $fileInfo = Get-Item $CsvFile
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "✓ تبدیل با موفقیت انجام شد!" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "فایل CSV: $CsvFile" -ForegroundColor Yellow
    Write-Host "حجم فایل: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "حالا می‌توانید اسکریپت SQL را اجرا کنید:" -ForegroundColor Cyan
    Write-Host "Import_یلدا_از_CSV.sql" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host "✗ خطا: فایل CSV ایجاد نشد!" -ForegroundColor Red
    exit 1
}







