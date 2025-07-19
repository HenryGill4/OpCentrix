using Microsoft.AspNetCore.Mvc;
using OpCentrix.Data;

namespace OpCentrix.Controllers
{
    public class SchedulerController : Controller
    {
        private readonly SchedulerContext _context;

        public SchedulerController(SchedulerContext context)
        {
            _context = context;
        }

        // GET: /Scheduler/
        public IActionResult Index()
        {
            return View();
        }
    }
}
