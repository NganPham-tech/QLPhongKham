using System.ComponentModel.DataAnnotations;

namespace QLPhongKham.Models
{
    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string Type { get; set; } = "Info"; 
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string? RelatedUrl { get; set; }
        
        public int? AppointmentId { get; set; }
        
        // Navigation properties
        public Appointment? Appointment { get; set; }
    }
}
