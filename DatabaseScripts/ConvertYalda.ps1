# Convert Excel to CSV
$excelFile = "$env:USERPROFILE\Desktop\یلدا.xlsx"
$csvFile = "$env:USERPROFILE\Desktop\یلدا.csv"

Write-Host "Converting Excel to CSV..." -ForegroundColor Cyan

if (-not (Test-Path $excelFile)) {
    Write-Host "Error: Excel file not found: $excelFile" -ForegroundColor Red
    exit 1
}

if (-not (Get-Module -ListAvailable -Name ImportExcel)) {
    Write-Host "Installing ImportExcel module..." -ForegroundColor Yellow
    Install-Module -Name ImportExcel -Force -Scope CurrentUser -AllowClobber
}

Import-Module ImportExcel

try {
    Import-Excel -Path $excelFile | Export-Csv -Path $csvFile -Encoding UTF8 -NoTypeInformation
    Write-Host "Success! CSV file created: $csvFile" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}







