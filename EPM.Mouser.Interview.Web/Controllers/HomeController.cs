using EPM.Mouser.Interview.Data;
using Microsoft.AspNetCore.Mvc;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IWarehouseRepository _warehouseRepository;
        public HomeController(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            return View(await _warehouseRepository.List());
        }
    }
}
