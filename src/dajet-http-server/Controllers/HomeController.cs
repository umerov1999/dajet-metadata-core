using Microsoft.AspNetCore.Mvc;

namespace DaJet.Http.Controllers
{
    [ApiController][Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet()] public ActionResult Home()
        {
            return Redirect("swagger");
        }
    }
}