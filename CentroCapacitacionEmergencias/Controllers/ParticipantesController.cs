using System;
using System.Linq;
using System.Web.Mvc;
using CentroCapacitacionEmergencias.Filters;
using CentroCapacitacionEmergencias.Models;
using CentroCapacitacionEmergencias.Repositories;

namespace CentroCapacitacionEmergencias.Controllers
{
    [SessionAuthorize]
    public class ParticipantesController : Controller
    {
        private readonly ParticipanteRepository _repo = new ParticipanteRepository();
        private readonly CatalogoRepository _catalogoRepo = new CatalogoRepository();

        public ActionResult Index()
        {
            ViewBag.Cohortes = new SelectList(_catalogoRepo.GetCohortes(), "Id", "Nombre");
            ViewBag.Cursos = new SelectList(_catalogoRepo.GetCursos(), "Id", "Titulo");

            var model = _repo.Buscar(null, null, null);
            return View(model);
        }

        [HttpGet]
        public PartialViewResult Buscar(string texto, int? cohorteId, int? cursoId)
        {
            var data = _repo.Buscar(texto, cohorteId, cursoId);
            return PartialView("_TablaParticipantes", data);
        }

        public ActionResult Details(int id)
        {
            var participante = _repo.GetById(id);
            if (participante == null) return HttpNotFound();

            var vm = new ParticipanteDetalleViewModel
            {
                Participante = participante,
                CohorteActual = _repo.GetCohorteActual(id),
                CursosAsignados = _repo.GetCursosAsignadosDetalle(id),
                HistorialDestrezas = _repo.GetHistorialDestrezas(id)
            };

            ViewBag.Cohortes = new SelectList(_catalogoRepo.GetCohortes(), "Id", "Nombre");
            ViewBag.Cursos = new MultiSelectList(_catalogoRepo.GetCursos(), "Id", "Titulo");

            return View(vm);
        }

        [SessionAuthorize("Administrador")]
        [HttpGet]
        public ActionResult Create()
        {
            return View(BuildFormModel(new ParticipanteFormViewModel
            {
                CursosIds = new System.Collections.Generic.List<int>(),
                HabilitarAsignacion = false
            }));
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ParticipanteFormViewModel model)
        {
            if (model.FechaNacimiento > DateTime.Today)
                ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser futura.");

            if (!ModelState.IsValid)
                return View(BuildFormModel(model));

            try
            {
                int id = _repo.Crear(model);
                TempData["Ok"] = "Participante registrado correctamente. Ahora puede asignar cohorte y cursos.";
                return RedirectToAction("Edit", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(BuildFormModel(model));
            }
        }

        [SessionAuthorize("Administrador")]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var item = _repo.GetById(id);
            if (item == null) return HttpNotFound();

            var model = BuildFormModel(new ParticipanteFormViewModel
            {
                Id = item.Id,
                TipoIdentificacion = item.TipoIdentificacion,
                Identificacion = item.Identificacion,
                NombreCompleto = item.NombreCompleto,
                FechaNacimiento = item.FechaNacimiento,
                Provincia = item.Provincia,
                Canton = item.Canton,
                Distrito = item.Distrito,
                DetalleDireccion = item.DetalleDireccion,
                EstadoCivil = item.EstadoCivil,
                Correo = item.Correo,
                Telefono = item.Telefono,
                DireccionResidenciaSecundaria = item.DireccionResidenciaSecundaria,
                ContactoEmergencia = item.ContactoEmergencia,
                CohorteId = _repo.GetCohorteAsignadaId(id),
                CursosIds = _repo.GetCursosAsignadosIds(id),
                HabilitarAsignacion = true
            });

            return View(model);
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ParticipanteFormViewModel model)
        {
            if (model.FechaNacimiento > DateTime.Today)
                ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser futura.");

            if (!ModelState.IsValid)
                return View(BuildFormModel(model));

            try
            {
                _repo.Editar(model);
                TempData["Ok"] = "Participante actualizado correctamente.";
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(BuildFormModel(model));
            }
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        public JsonResult DesactivarAjax(int id)
        {
            try
            {
                _repo.Desactivar(id);
                return Json(new { ok = true, mensaje = "Participante desactivado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        public JsonResult AsignarAjax(ParticipanteAsignacionAjaxViewModel model)
        {
            try
            {
                if (model.CursosIds == null || model.CursosIds.Count == 0)
                    return Json(new { ok = false, mensaje = "Debe seleccionar al menos un curso." });

                _repo.AsignarCohorteYCursos(model.ParticipanteId, model.CohorteId, model.CursosIds);
                return Json(new { ok = true, mensaje = "Asignación realizada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        public JsonResult RemoverCursoAjax(int participanteId, int cursoId, string motivoCambio)
        {
            try
            {
                _repo.RemoverCurso(participanteId, cursoId, motivoCambio);
                return Json(new { ok = true, mensaje = "Curso removido correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        public JsonResult CambiarCohorteAjax(int participanteId, int cohorteId, string motivoCambio)
        {
            try
            {
                _repo.CambiarCohorte(participanteId, cohorteId, motivoCambio);
                return Json(new { ok = true, mensaje = "Cohorte actualizada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        private ParticipanteFormViewModel BuildFormModel(ParticipanteFormViewModel model)
        {
            model.Cohortes = _catalogoRepo.GetCohortes()
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Nombre,
                    Selected = model.CohorteId.HasValue && model.CohorteId.Value == x.Id
                });

            model.Cursos = _catalogoRepo.GetCursos()
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Titulo,
                    Selected = model.CursosIds != null && model.CursosIds.Contains(x.Id)
                });

            if (model.CursosIds == null)
                model.CursosIds = new System.Collections.Generic.List<int>();

            return model;
        }
    }
}