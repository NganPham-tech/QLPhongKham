@echo off
echo ============================================
echo    KHOI DONG UNG DUNG QUAN LY PHONG KHAM
echo ============================================
echo.

echo [1/3] Building project...
cd /d "d:\LTWEB\QLPhongKham\QLPhongKham"
dotnet build --configuration Release

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Build failed! Please check for errors.
    pause
    exit /b 1
)

echo ✅ Build successful!
echo.

echo [2/3] Applying database migrations...
dotnet ef database update

if %ERRORLEVEL% NEQ 0 (
    echo ⚠️  Database update failed, but continuing...
)

echo.
echo [3/3] Starting application...
echo.
echo ============================================
echo  TAO HOA DON CHO MEI CHAN
echo ============================================
echo.
echo 📋 Thông tin bệnh nhân:
echo    - Họ tên: Mei Chan
echo    - Email: mei@gmail.com  
echo    - Ngày khám: 23/6/2025
echo.
echo 🔗 Các bước tiếp theo:
echo    1. Đăng nhập với quyền Admin/NhanVien
echo    2. Vào Admin → Tài chính → Hóa đơn
echo    3. Click "Tạo hóa đơn mới"
echo    4. Chọn bệnh nhân "Mei Chan (mei@gmail.com)"
echo    5. Click "Tạo hóa đơn"
echo.
echo 🌐 Application will start at: https://localhost:5001
echo ============================================
echo.

start https://localhost:5001
dotnet run --configuration Release

pause
