# Script dọn dẹp project QLPhongKham
# Chạy script này khi muốn dọn dẹp project trước khi commit hoặc khi Visual Studio chạy chậm

Write-Host "🧹 Bắt đầu dọn dẹp project QLPhongKham..." -ForegroundColor Green

# Chuyển đến thư mục project
Set-Location "d:\LTWEB\QLPhongKham"

# Xóa thư mục Visual Studio cache
Write-Host "🗑️  Xóa thư mục .vs..." -ForegroundColor Yellow
Remove-Item -Recurse -Force ".vs" -ErrorAction SilentlyContinue

# Chuyển đến thư mục QLPhongKham
Set-Location "QLPhongKham"

# Xóa các thư mục build
Write-Host "🗑️  Xóa thư mục bin và obj..." -ForegroundColor Yellow
Remove-Item -Recurse -Force "bin" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "obj" -ErrorAction SilentlyContinue

# Xóa các file user settings
Write-Host "🗑️  Xóa file user settings..." -ForegroundColor Yellow
Remove-Item "*.csproj.user" -ErrorAction SilentlyContinue

# Xóa các file temp và log
Write-Host "🗑️  Xóa file temp và log..." -ForegroundColor Yellow
Get-ChildItem -Recurse -Filter "*.log" | Remove-Item -Force -ErrorAction SilentlyContinue
Get-ChildItem -Recurse -Filter "*.tmp" | Remove-Item -Force -ErrorAction SilentlyContinue

# Xóa thư mục packages nếu có (NuGet local packages)
Write-Host "🗑️  Xóa thư mục packages..." -ForegroundColor Yellow
Remove-Item -Recurse -Force "packages" -ErrorAction SilentlyContinue

Write-Host "✅ Dọn dẹp hoàn tất!" -ForegroundColor Green
Write-Host "💡 Bạn có thể rebuild project bằng lệnh: dotnet build" -ForegroundColor Cyan

# Quay lại thư mục gốc
Set-Location ".."

# Hiển thị kích thước thư mục sau khi dọn dẹp
Write-Host "📊 Kích thước thư mục sau khi dọn dẹp:" -ForegroundColor Cyan
$size = (Get-ChildItem -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "   $([math]::Round($size, 2)) MB" -ForegroundColor White
