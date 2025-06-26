using Microsoft.AspNetCore.Mvc;
using QLPhongKham.Data;
using QLPhongKham.TestTools;

namespace QLPhongKham.Controllers
{
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public DebugController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet("check-mei-chan")]
        public async Task<IActionResult> CheckMeiChan()
        {
            try
            {
                await DatabaseChecker.CheckMeiChanAppointments(_context);
                await DatabaseChecker.CheckCompletedAppointmentsWithoutInvoices(_context);
                
                return Ok("Check completed - see console output");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
