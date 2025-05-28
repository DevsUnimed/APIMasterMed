using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIMasterMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaDigitalController : ControllerBase
    {
        // POST: PaDigitalController
        [HttpPost]
        public ActionResult Index()
        {
            return Ok("API funcionando!");
        }
    }
}
