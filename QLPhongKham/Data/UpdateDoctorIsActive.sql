-- Cập nhật tất cả bác sĩ hiện tại để có IsActive = true
UPDATE Doctors SET IsActive = 1 WHERE IsActive = 0;

-- Kiểm tra kết quả
SELECT DoctorId, FirstName, LastName, IsActive FROM Doctors;
