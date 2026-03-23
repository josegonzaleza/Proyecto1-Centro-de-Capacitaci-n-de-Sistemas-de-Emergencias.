using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CentroCapacitacionEmergencias.Filters;
using CentroCapacitacionEmergencias.Models;
using CentroCapacitacionEmergencias.Repositories;

namespace CentroCapacitacionEmergencias.Controllers
{
    [SessionAuthorize]
    public class MonitoreoController : Controller
    {
        private readonly MonitoreoRepository _repo = new MonitoreoRepository();
        private readonly CatalogoRepository _catalogoRepo = new CatalogoRepository();

        public ActionResult Index()
        {
            var user = (SessionUser)Session["User"];

            var vm = new MonitoreoViewModel
            {
                Riesgos = new List<RiesgoItem>()
            };

            if (user.Rol == "Instructor")
            {
                vm.Cohortes = _repo.GetCohortesDelInstructor(user.Id).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Nombre
                });

                vm.Cursos = _repo.GetCursosDelInstructor(user.Id).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Titulo
                });
            }
            else
            {
                vm.Cohortes = _catalogoRepo.GetCohortes().Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Nombre
                });

                vm.Cursos = _catalogoRepo.GetCursos().Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Titulo
                });
            }

            return View(vm);
        }

        [HttpGet]
        public JsonResult Resumen(int cohorteId, int cursoId)
        {
            var user = (SessionUser)Session["User"];

            if (user.Rol == "Instructor" && !_repo.InstructorTieneAcceso(user.Id, cohorteId, cursoId))
            {
                return Json(new { ok = false, mensaje = "No autorizado." }, JsonRequestBehavior.AllowGet);
            }

            var data = _repo.GetResumen(cohorteId, cursoId);

            return Json(new
            {
                ok = true,
                data.TasaGlobalAprobacion,
                data.EstadoCertificacion,
                data.IntervencionPendiente,
                data.Riesgos
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public PartialViewResult IntervencionPendiente(int cohorteId, int cursoId)
        {
            var user = (SessionUser)Session["User"];

            if (user.Rol == "Instructor" && !_repo.InstructorTieneAcceso(user.Id, cohorteId, cursoId))
            {
                return PartialView("_IntervencionPendiente", new List<IntervencionPendienteItem>());
            }

            var data = _repo.GetIntervencionPendiente(cohorteId, cursoId);
            return PartialView("_IntervencionPendiente", data);
        }

        public ActionResult Riesgo(int destrezaId)
        {
            var riesgoRepo = new RiesgoDetalleRepository();
            var model = riesgoRepo.GetRiesgoDetalle(destrezaId);
            return View(model);
        }
    }
}