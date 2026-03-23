using System.Web.Mvc;
using CentroCapacitacionEmergencias.Filters;

namespace CentroCapacitacionEmergencias.Controllers
{
    [SessionAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}