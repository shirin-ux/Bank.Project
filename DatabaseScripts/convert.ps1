$excel = "$env:USERPROFILE\Desktop\یلدا.xlsx"
$csv = "$env:USERPROFILE\Desktop\یلدا.csv"

if (-not (Test-Path $excel)) {
    Write-Host "File not found: $excel" -ForegroundColor Red
    exit 1
}

if (-not (Get-Module -ListAvailable -Name ImportExcel)) {
    Install-Module -Name ImportExcel -Force -Scope CurrentUser
}

Import-Module ImportExcel
Import-Excel -Path $excel | Export-Csv -Path $csv -Encoding UTF8 -NoTypeInformation
Write-Host "Done! CSV saved to: $csv" -ForegroundColor Green







