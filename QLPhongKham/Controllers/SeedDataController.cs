using Microsoft.AspNetCore.Mvc;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Controllers
{
    public class SeedDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SeedDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SeedServices()
        {
            // Kiểm tra nếu đã có dữ liệu
            if (_context.Services.Any())
            {
                return Json(new { success = true, message = "Dữ liệu đã tồn tại" });
            }

            var services = new List<Service>
            {
                new Service { Name = "Khám nội khoa tổng quát", Description = "Khám và tư vấn về các bệnh lý nội khoa", Price = 200000, Duration = 30, Specialty = "Nội khoa", IsActive = true },
                new Service { Name = "Khám tim mạch", Description = "Khám và tư vấn về các bệnh lý tim mạch", Price = 300000, Duration = 45, Specialty = "Nội khoa", IsActive = true },
                new Service { Name = "Phẫu thuật nội soi", Description = "Phẫu thuật nội soi ít xâm lấn", Price = 5000000, Duration = 120, Specialty = "Ngoại khoa", IsActive = true },
                new Service { Name = "Khám nhi khoa", Description = "Khám sức khỏe trẻ em", Price = 150000, Duration = 30, Specialty = "Nhi khoa", IsActive = true },
                new Service { Name = "Siêu âm thai", Description = "Siêu âm theo dõi thai nhi", Price = 250000, Duration = 30, Specialty = "Sản khoa", IsActive = true },
                new Service { Name = "Khám tai mũi họng", Description = "Khám và điều trị bệnh lý tai mũi họng", Price = 180000, Duration = 30, Specialty = "Tai Mũi Họng", IsActive = true },
                new Service { Name = "Khám mắt", Description = "Khám và tư vấn về thị lực", Price = 150000, Duration = 30, Specialty = "Mắt", IsActive = true },
                new Service { Name = "Điều trị da", Description = "Điều trị các bệnh lý về da", Price = 200000, Duration = 45, Specialty = "Da liễu", IsActive = true },
                new Service { Name = "Khám nha khoa", Description = "Khám và tư vấn về răng miệng", Price = 100000, Duration = 30, Specialty = "Răng Hàm Mặt", IsActive = true },
                new Service { Name = "Khám tổng quát", Description = "Khám sức khỏe tổng quát", Price = 300000, Duration = 60, Specialty = null, IsActive = true }
            };

            _context.Services.AddRange(services);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã tạo dữ liệu services thành công" });
        }

        public async Task<IActionResult> SeedDoctors()
        {
            // Kiểm tra nếu đã có dữ liệu
            if (_context.Doctors.Any())
            {
                return Json(new { success = true, message = "Dữ liệu bác sĩ đã tồn tại" });
            }

            var doctors = new List<Doctor>
            {
                new Doctor { FirstName = "Nguyễn", LastName = "Văn An", DateOfBirth = new DateTime(1980, 5, 15), Gender = "Nam", Specialty = "Nội khoa", Phone = "0901234567", Email = "nva@hospital.com", DateHired = DateTime.Now.AddYears(-5), IsActive = true },
                new Doctor { FirstName = "Trần", LastName = "Thị Bình", DateOfBirth = new DateTime(1985, 8, 20), Gender = "Nữ", Specialty = "Ngoại khoa", Phone = "0901234568", Email = "ttb@hospital.com", DateHired = DateTime.Now.AddYears(-3), IsActive = true },
                new Doctor { FirstName = "Lê", LastName = "Văn Cường", DateOfBirth = new DateTime(1983, 3, 10), Gender = "Nam", Specialty = "Nhi khoa", Phone = "0901234569", Email = "lvc@hospital.com", DateHired = DateTime.Now.AddYears(-4), IsActive = true },
                new Doctor { FirstName = "Phạm", LastName = "Thị Dung", DateOfBirth = new DateTime(1982, 12, 5), Gender = "Nữ", Specialty = "Sản khoa", Phone = "0901234570", Email = "ptd@hospital.com", DateHired = DateTime.Now.AddYears(-6), IsActive = true },
                new Doctor { FirstName = "Hoàng", LastName = "Văn Hạnh", DateOfBirth = new DateTime(1981, 7, 25), Gender = "Nam", Specialty = "Tai Mũi Họng", Phone = "0901234571", Email = "hvh@hospital.com", DateHired = DateTime.Now.AddYears(-5), IsActive = true },
                new Doctor { FirstName = "Vũ", LastName = "Thị Mai", DateOfBirth = new DateTime(1984, 9, 15), Gender = "Nữ", Specialty = "Mắt", Phone = "0901234572", Email = "vtm@hospital.com", DateHired = DateTime.Now.AddYears(-2), IsActive = true },
                new Doctor { FirstName = "Đặng", LastName = "Văn Nam", DateOfBirth = new DateTime(1979, 4, 30), Gender = "Nam", Specialty = "Da liễu", Phone = "0901234573", Email = "dvn@hospital.com", DateHired = DateTime.Now.AddYears(-7), IsActive = true },
                new Doctor { FirstName = "Bùi", LastName = "Thị Oanh", DateOfBirth = new DateTime(1986, 11, 12), Gender = "Nữ", Specialty = "Răng Hàm Mặt", Phone = "0901234574", Email = "bto@hospital.com", DateHired = DateTime.Now.AddYears(-1), IsActive = true }
            };

            _context.Doctors.AddRange(doctors);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã tạo dữ liệu doctors thành công" });
        }

    [HttpPost]
    public async Task<IActionResult> SeedFAQData()
    {
        try
        {
            // Kiểm tra xem đã có dữ liệu FAQ chưa
            if (_context.FAQs.Any())
            {
                return Json(new { success = false, message = "Dữ liệu FAQ đã tồn tại" });
            }

            var faqData = new List<FAQ>
            {
                // Quy trình đặt lịch khám
                new FAQ
                {
                    Question = "Làm thế nào để đặt lịch khám?",
                    Answer = "Bạn có thể đặt lịch khám bằng cách:\n1. Truy cập website và đăng nhập tài khoản\n2. Chọn menu \"Đặt lịch khám\"\n3. Chọn dịch vụ khám và bác sĩ\n4. Chọn ngày giờ phù hợp\n5. Xác nhận thông tin và hoàn tất đặt lịch",
                    Category = "Quy trình đặt lịch khám",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },                new FAQ
                {
                    Question = "Tôi có thể đặt lịch khám cho người khác không?",
                    Answer = "Có, bạn có thể đặt lịch khám cho người thân. Tuy nhiên, bạn cần cung cấp đầy đủ thông tin của người được đặt lịch và có thể cần xuất trình giấy tờ tùy thân khi đến khám.",
                    Category = "Quy trình đặt lịch khám",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new FAQ
                {
                    Question = "Tôi có thể đặt lịch khám cùng lúc nhiều dịch vụ không?",
                    Answer = "Hiện tại hệ thống chỉ hỗ trợ đặt lịch cho một dịch vụ mỗi lần. Nếu bạn cần khám nhiều dịch vụ, vui lòng đặt lịch riêng biệt cho từng dịch vụ.",
                    Category = "Quy trình đặt lịch khám",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },

                // Cách thanh toán
                new FAQ
                {
                    Question = "Phòng khám hỗ trợ những hình thức thanh toán nào?",
                    Answer = "Phòng khám hỗ trợ các hình thức thanh toán sau:\n- Tiền mặt\n- Thẻ ATM/Credit Card\n- Chuyển khoản ngân hàng\n- Ví điện tử (MoMo, ZaloPay, ViettelPay)",
                    Category = "Cách thanh toán",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new FAQ
                {
                    Question = "Tôi có cần thanh toán trước khi khám không?",
                    Answer = "Không, bạn chỉ cần thanh toán sau khi hoàn tất quá trình khám và nhận kết quả. Tuy nhiên, một số dịch vụ đặc biệt có thể yêu cầu đặt cọc trước.",
                    Category = "Cách thanh toán",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new FAQ
                {
                    Question = "Phòng khám có xuất hóa đơn VAT không?",
                    Answer = "Có, phòng khám sẽ xuất hóa đơn VAT cho tất cả các dịch vụ. Vui lòng thông báo nhu cầu xuất hóa đơn VAT và cung cấp thông tin công ty khi thanh toán.",
                    Category = "Cách thanh toán",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },

                // Chính sách hủy lịch
                new FAQ
                {
                    Question = "Tôi có thể hủy lịch khám không?",
                    Answer = "Có, bạn có thể hủy lịch khám. Vui lòng hủy lịch ít nhất 2 giờ trước giờ khám để tránh ảnh hưởng đến lịch trình của bác sĩ và bệnh nhân khác.",
                    Category = "Chính sách hủy lịch",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new FAQ
                {
                    Question = "Làm thế nào để hủy lịch khám?",
                    Answer = "Bạn có thể hủy lịch khám bằng cách:\n1. Đăng nhập vào tài khoản\n2. Vào mục \"Lịch sử khám\"\n3. Tìm lịch khám cần hủy\n4. Nhấn nút \"Hủy lịch\"\n5. Xác nhận hủy lịch",
                    Category = "Chính sách hủy lịch",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new FAQ
                {
                    Question = "Có mất phí khi hủy lịch khám không?",
                    Answer = "Không, việc hủy lịch khám hoàn toàn miễn phí nếu bạn hủy trước 2 giờ so với giờ khám đã đặt. Nếu hủy muộn hơn, có thể áp dụng phí hủy lịch.",
                    Category = "Chính sách hủy lịch",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },

                // Thời gian chờ kết quả
                new FAQ
                {
                    Question = "Tôi phải chờ bao lâu để có kết quả xét nghiệm?",
                    Answer = "Thời gian chờ kết quả tùy thuộc vào loại xét nghiệm:\n- Xét nghiệm máu cơ bản: 2-4 giờ\n- Xét nghiệm sinh hóa: 1-2 ngày\n- Xét nghiệm vi sinh: 3-5 ngày\n- Xét nghiệm đặc biệt: 5-7 ngày",
                    Category = "Thời gian chờ kết quả",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new FAQ
                {
                    Question = "Làm thế nào để nhận kết quả xét nghiệm?",
                    Answer = "Bạn có thể nhận kết quả xét nghiệm bằng cách:\n1. Đến trực tiếp phòng khám để nhận\n2. Xem kết quả online trên website\n3. Nhận qua email (nếu đăng ký dịch vụ)\n4. Gọi điện thoại để hỏi kết quả cơ bản",
                    Category = "Thời gian chờ kết quả",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },

                // Bảo hiểm y tế
                new FAQ
                {
                    Question = "Phòng khám có hỗ trợ bảo hiểm y tế không?",
                    Answer = "Có, phòng khám hỗ trợ các loại bảo hiểm y tế sau:\n- Bảo hiểm y tế xã hội (BHYT)\n- Bảo hiểm y tế tư nhân\n- Bảo hiểm của các công ty bảo hiểm lớn",
                    Category = "Bảo hiểm y tế",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new FAQ
                {
                    Question = "Tôi cần mang gì khi sử dụng bảo hiểm y tế?",
                    Answer = "Khi sử dụng bảo hiểm y tế, bạn cần mang:\n- Thẻ bảo hiểm y tế\n- CMND/CCCD\n- Giấy chuyển viện (nếu có)\n- Hồ sơ bệnh án cũ (nếu có)",
                    Category = "Bảo hiểm y tế",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },

                // Chuẩn bị trước khi khám
                new FAQ
                {
                    Question = "Tôi cần chuẩn bị gì trước khi đến khám?",
                    Answer = "Trước khi đến khám, bạn nên:\n1. Mang theo CMND/CCCD\n2. Mang theo thẻ bảo hiểm y tế (nếu có)\n3. Chuẩn bị danh sách thuốc đang sử dụng\n4. Mang theo kết quả xét nghiệm cũ (nếu có)\n5. Đến sớm 15-30 phút trước giờ hẹn",
                    Category = "Chuẩn bị trước khi khám",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new FAQ
                {
                    Question = "Tôi có cần nhịn ăn trước khi xét nghiệm không?",
                    Answer = "Tùy thuộc vào loại xét nghiệm:\n- Xét nghiệm glucose, lipid: Nhịn ăn 8-12 tiếng\n- Xét nghiệm máu thường: Không cần nhịn ăn\n- Siêu âm bụng: Nhịn ăn 6-8 tiếng\nBác sĩ sẽ hướng dẫn cụ thể khi đặt lịch.",
                    Category = "Chuẩn bị trước khi khám",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new FAQ
                {
                    Question = "Tôi có nên tắm trước khi đến khám không?",
                    Answer = "Có, bạn nên tắm và vệ sinh cá nhân sạch sẽ trước khi đến khám. Tuy nhiên, đối với một số xét nghiệm da liễu, có thể cần tránh sử dụng xà phòng hoặc kem dưỡng da.",
                    Category = "Chuẩn bị trước khi khám",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
                };

                _context.FAQs.AddRange(faqData);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã tạo thành công {faqData.Count} câu hỏi FAQ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tạo dữ liệu FAQ: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SeedArticleData()
        {
            try
            {
                // Kiểm tra xem đã có dữ liệu Article chưa
                if (_context.Articles.Any())
                {
                    return Json(new { success = false, message = "Dữ liệu Article đã tồn tại" });
                }            var articleData = new List<Article>
            {
                new Article
                {
                    Title = "Lợi ích của việc khám sức khỏe định kỳ",
                    Content = "Khám sức khỏe định kỳ là một trong những biện pháp quan trọng nhất để duy trì sức khỏe và phát hiện sớm các bệnh lý. Việc khám định kỳ giúp:\n\n1. **Phát hiện sớm bệnh lý**: Nhiều bệnh trong giai đoạn đầu không có triệu chứng rõ ràng. Khám định kỳ giúp phát hiện sớm các bệnh như tiểu đường, tăng huyết áp, ung thư...\n\n2. **Theo dõi các chỉ số sức khỏe**: Kiểm tra định kỳ các chỉ số như huyết áp, đường huyết, cholesterol giúp đánh giá tình trạng sức khỏe tổng thể.\n\n3. **Tư vấn lối sống**: Bác sĩ sẽ tư vấn về chế độ ăn uống, tập luyện và lối sống phù hợp với tình trạng sức khỏe của bạn.\n\n4. **Tiết kiệm chi phí**: Phát hiện và điều trị sớm sẽ giúp tiết kiệm chi phí điều trị trong tương lai.\n\nNên khám sức khỏe định kỳ 6 tháng - 1 năm một lần tùy theo độ tuổi và tình trạng sức khỏe.",
                    Summary = "Khám sức khỏe định kỳ giúp phát hiện sớm bệnh lý, theo dõi chỉ số sức khỏe và nhận tư vấn từ bác sĩ chuyên khoa.",
                    Category = "Sức khỏe tổng quát",
                    FeaturedImage = "/images/articles/general-health-checkup.jpg",
                    Author = "BS. Nguyễn Văn A",
                    IsActive = true,
                    IsFeatured = true,
                    ViewCount = 0,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now.AddDays(-5)
                },                new Article
                {
                    Title = "Cách phòng ngừa bệnh tim mạch hiệu quả",
                    Content = "Bệnh tim mạch là nguyên nhân hàng đầu gây tử vong trên toàn thế giới. Tuy nhiên, nhiều bệnh tim mạch có thể được phòng ngừa bằng cách thay đổi lối sống:\n\n**1. Chế độ ăn uống lành mạnh:**\n- Hạn chế muối, đường và chất béo trans\n- Tăng cường rau xanh, trái cây và ngũ cốc nguyên hạt\n- Ăn cá giàu omega-3 như cá hồi, cá thu\n\n**2. Tập thể dục thường xuyên:**\n- Ít nhất 150 phút hoạt động vừa phải mỗi tuần\n- Bao gồm cả bài tập aerobic và tập tạ\n\n**3. Kiểm soát cân nặng:**\n- Duy trì BMI trong khoảng 18.5-24.9\n- Đo vòng eo: nam <90cm, nữ <80cm\n\n**4. Không hút thuốc và hạn chế rượu bia**\n\n**5. Quản lý stress:**\n- Thực hành thiền, yoga\n- Đảm bảo giấc ngủ đủ 7-8 tiếng/đêm\n\n**6. Kiểm tra sức khỏe định kỳ:**\n- Đo huyết áp, kiểm tra cholesterol định kỳ\n- Khám tim mạch nếu có yếu tố nguy cơ",
                    Summary = "Phòng ngừa bệnh tim mạch thông qua chế độ ăn uống lành mạnh, tập thể dục, kiểm soát cân nặng và khám sức khỏe định kỳ.",
                    Category = "Tim mạch",
                    FeaturedImage = "/images/articles/heart-health.jpg",
                    Author = "BS. Trần Thị B",
                    IsActive = true,
                    IsFeatured = false,
                    ViewCount = 0,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    UpdatedAt = DateTime.Now.AddDays(-3)
                },
                new Article
                {
                    Title = "Tầm quan trọng của việc tiêm vaccine cho trẻ em",
                    Content = "Vaccine là một trong những thành tựu y học vĩ đại nhất, giúp bảo vệ trẻ em khỏi các bệnh nguy hiểm. Đây là những điều phụ huynh cần biết về vaccine:\n\n**Vaccine hoạt động như thế nào?**\nVaccine chứa vi khuẩn hoặc virus đã được làm yếu hoặc tiêu diệt, giúp hệ miễn dịch nhận biết và tạo kháng thể chống lại bệnh tật.\n\n**Lịch tiêm vaccine cơ bản cho trẻ em:**\n- Sơ sinh: BCG, Viêm gan B\n- 2 tháng: 5 trong 1, Rotavirus, Phế cầu\n- 4 tháng: 5 trong 1, Rotavirus, Phế cầu\n- 6 tháng: 5 trong 1, Rotavirus, Phế cầu\n- 9 tháng: Sởi\n- 12 tháng: MMR, Thủy đậu\n- 18 tháng: 5 trong 1, MMR, Thủy đậu\n\n**Lợi ích của việc tiêm vaccine:**\n- Bảo vệ trẻ khỏi các bệnh nguy hiểm\n- Tạo miễn dịch cộng đồng\n- Ngăn ngừa biến chứng nghiêm trọng\n- Tiết kiệm chi phí điều trị\n\n**Tác dụng phụ thường gặp:**\n- Sốt nhẹ, đau tại chỗ tiêm\n- Thường xuất hiện trong 1-2 ngày và tự khỏi\n\nPhụ huynh nên tham khảo ý kiến bác sĩ về lịch tiêm vaccine phù hợp cho con em mình.",
                    Summary = "Vaccine giúp bảo vệ trẻ em khỏi các bệnh nguy hiểm. Tuân thủ lịch tiêm vaccine đầy đủ và đúng thời gian là rất quan trọng.",
                    Category = "Nhi khoa",
                    FeaturedImage = "/images/articles/child-vaccination.jpg",
                    Author = "BS. Lê Văn C",
                    IsActive = true,
                    IsFeatured = false,
                    ViewCount = 0,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now.AddDays(-1)
                },
                new Article
                {
                    Title = "Chăm sóc sức khỏe phụ nữ sau tuổi 40",
                    Content = "Sau tuổi 40, cơ thể phụ nữ bắt đầu có những thay đổi về hormone, đặc biệt là trong giai đoạn tiền mãn kinh. Đây là giai đoạn quan trọng cần chú ý chăm sóc sức khỏe:\n\n**Những thay đổi thường gặp:**\n- Chu kỳ kinh nguyệt không đều\n- Bốc hỏa, đổ mồ hôi đêm\n- Thay đổi tâm trạng\n- Giảm mật độ xương\n- Tăng nguy cơ bệnh tim mạch\n\n**Các xét nghiệm nên thực hiện định kỳ:**\n- Tầm soát ung thư vú: chụp mammography hàng năm\n- Tầm soát ung thư cổ tử cung: Pap smear 3 năm/lần\n- Đo mật độ xương: 2 năm/lần\n- Kiểm tra cholesterol, đường huyết\n- Khám mắt, khám da\n\n**Chế độ dinh dưỡng:**\n- Tăng cường canxi và vitamin D\n- Ăn nhiều thực phẩm giàu phytoestrogen (đậu nành)\n- Hạn chế caffeine và rượu\n- Uống đủ nước (2-2.5 lít/ngày)\n\n**Hoạt động thể chất:**\n- Tập thể dục có tác động (đi bộ, chạy bộ)\n- Yoga, pilates để tăng cường độ dẻo dai\n- Tập tạ để duy trì khối lượng cơ\n\n**Chăm sóc tâm lý:**\n- Duy trì mối quan hệ xã hội tích cực\n- Tìm hiểu về những thay đổi tự nhiên của cơ thể\n- Tham khảo bác sĩ khi cần thiết",
                    Summary = "Phụ nữ sau 40 tuổi cần chú ý chăm sóc sức khỏe toàn diện với các xét nghiệm định kỳ, chế độ dinh dưỡng và vận động phù hợp.",
                    Category = "Phụ khoa",
                    FeaturedImage = "/images/articles/women-health-40.jpg",
                    Author = "BS. Phạm Thị D",
                    IsActive = true,
                    IsFeatured = true,
                    ViewCount = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new Article
                {
                    Title = "Những dấu hiệu cảnh báo bệnh về mắt",
                    Content = "Mắt là cơ quan quan trọng giúp chúng ta tiếp nhận thông tin từ thế giới bên ngoài. Tuy nhiên, nhiều bệnh về mắt phát triển âm thầm. Dưới đây là những dấu hiệu cảnh báo bạn nên đến khám mắt:\n\n**Dấu hiệu cần khám ngay:**\n- Đau mắt dữ dội, đột ngột\n- Mất thị lực đột ngột\n- Thấy những vệt sáng, chớp nháy\n- Thấy \"màn mờ\" che phủ một phần tầm nhìn\n- Nhìn thấy vòng hào quang quanh đèn\n\n**Dấu hiệu cần khám trong vài ngày:**\n- Mờ mắt kéo dài\n- Nhìn đôi\n- Khó thích nghi với ánh sáng\n- Đau đầu thường xuyên kèm mỏi mắt\n- Khó nhìn về đêm\n\n**Các bệnh về mắt thường gặp:**\n\n*1. Cận thị, viễn thị, loạn thị:*\n- Triệu chứng: mờ mắt, đau đầu, mỏi mắt\n- Điều trị: kính, lens hoặc phẫu thuật\n\n*2. Khô mắt:*\n- Triệu chứng: cảm giác cát trong mắt, đỏ mắt\n- Điều trị: thuốc nhỏ mắt, thay đổi môi trường\n\n*3. Glaucoma:*\n- Triệu chứng: đau mắt, nhìn thấy hào quang\n- Nguy hiểm: có thể gây mù lòa\n\n*4. Đục thủy tinh thể:*\n- Triệu chứng: mờ mắt dần dần\n- Điều trị: phẫu thuật thay thủy tinh thể\n\n**Cách bảo vệ mắt:**\n- Nghỉ ngơi mắt khi làm việc máy tính (quy tắc 20-20-20)\n- Đeo kính râm khi ra nắng\n- Ăn thực phẩm giàu vitamin A, C, E\n- Không dụi mắt bằng tay bẩn\n- Khám mắt định kỳ hàng năm",
                    Summary = "Nhận biết các dấu hiệu cảnh báo bệnh về mắt và thực hiện các biện pháp bảo vệ mắt hàng ngày để duy trì thị lực tốt.",
                    Category = "Nhãn khoa",
                    FeaturedImage = "/images/articles/eye-health.jpg",
                    Author = "BS. Hoàng Văn E",
                    IsActive = true,
                    IsFeatured = false,
                    ViewCount = 0,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    UpdatedAt = DateTime.Now.AddDays(-2)
                }
                };

                _context.Articles.AddRange(articleData);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã tạo thành công {articleData.Count} bài viết" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tạo dữ liệu Article: {ex.Message}" });
            }
        }

        public async Task<IActionResult> SeedAll()
        {
            await SeedServices();
            await SeedDoctors();
            return Json(new { success = true, message = "Đã tạo toàn bộ dữ liệu mẫu thành công" });
        }
    }
}
