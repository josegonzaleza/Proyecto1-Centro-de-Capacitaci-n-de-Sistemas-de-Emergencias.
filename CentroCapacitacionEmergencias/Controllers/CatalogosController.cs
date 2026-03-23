using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CentroCapacitacionEmergencias.Filters;
using CentroCapacitacionEmergencias.Models;
using CentroCapacitacionEmergencias.Repositories;

namespace CentroCapacitacionEmergencias.Controllers
{
    [SessionAuthorize]
    public class CatalogosController : Controller
    {
        private readonly CatalogoRepository _repo = new CatalogoRepository();

        public ActionResult Cohortes(bool verArchivados = false)
        {
            ViewBag.VerArchivados = verArchivados;
            return View(_repo.GetCohortes(verArchivados));
        }

        [SessionAuthorize("Administrador")]
        [HttpGet]
        public ActionResult CreateCohorte()
        {
            return View(new CohorteFormViewModel
            {
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddMonths(1)
            });
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCohorte(CohorteFormViewModel model)
        {
            if (model.FechaFin < model.FechaInicio)
                ModelState.AddModelError("", "La fecha fin no puede ser menor que la fecha inicio.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _repo.CrearCohorte(new Cohorte
                {
                    Nombre = model.Nombre.Trim(),
                    FechaInicio = model.FechaInicio,
                    FechaFin = model.FechaFin
                });

                TempData["Ok"] = "Cohorte creada correctamente.";
                return RedirectToAction("Cohortes");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [SessionAuthorize("Administrador")]
        [HttpGet]
        public ActionResult EditCohorte(int id)
        {
            var item = _repo.GetCohorteById(id);
            if (item == null) return HttpNotFound();

            return View(new CohorteFormViewModel
            {
                Id = item.Id,
                Nombre = item.Nombre,
                FechaInicio = item.FechaInicio,
                FechaFin = item.FechaFin,
                Archivado = item.Archivado
            });
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCohorte(CohorteFormViewModel model)
        {
            if (model.FechaFin < model.FechaInicio)
                ModelState.AddModelError("", "La fecha fin no puede ser menor que la fecha inicio.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _repo.EditarCohorte(new Cohorte
                {
                    Id = model.Id,
                    Nombre = model.Nombre.Trim(),
                    FechaInicio = model.FechaInicio,
                    FechaFin = model.FechaFin
                });

                TempData["Ok"] = "Cohorte actualizada correctamente.";
                return RedirectToAction("Cohortes", new { verArchivados = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        public JsonResult ToggleArchivadoCohorteAjax(int id)
        {
            try
            {
                _repo.ToggleArchivadoCohorte(id);
                return Json(new { ok = true, mensaje = "Estado de cohorte actualizado." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        public ActionResult Cursos(bool verArchivados = false)
        {
            ViewBag.VerArchivados = verArchivados;
            return View(_repo.GetCursos(verArchivados));
        }

        [SessionAuthorize("Administrador")]
        [HttpGet]
        public ActionResult CreateCurso()
        {
            return View(BuildCursoForm(new CursoFormViewModel
            {
                InstructoresIds = new List<int>(),
                CohortesIds = new List<int>()
            }));
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCurso(CursoFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(BuildCursoForm(model));

            try
            {
                _repo.CrearCurso(new Curso
                {
                    Codigo = model.Codigo.Trim(),
                    Titulo = model.Titulo.Trim(),
                    TotalHorasPracticas = model.TotalHorasPracticas
                }, model.InstructoresIds, model.CohortesIds);

                TempData["Ok"] = "Curso creado correctamente.";
                return RedirectToAction("Cursos");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(BuildCursoForm(model));
            }
        }

        [SessionAuthorize("Administrador")]
        [HttpGet]
        public ActionResult EditCurso(int id)
        {
            var item = _repo.GetCursoById(id);
            if (item == null) return HttpNotFound();

            return View(BuildCursoForm(new CursoFormViewModel
            {
                Id = item.Id,
                Codigo = item.Codigo,
                Titulo = item.Titulo,
                TotalHorasPracticas = item.TotalHorasPracticas,
                Archivado = item.Archivado,
                InstructoresIds = _repo.GetInstructoresPorCurso(id),
                CohortesIds = _repo.GetCohortesPorCurso(id)
            }));
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCurso(CursoFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(BuildCursoForm(model));

            try
            {
                _repo.EditarCurso(new Curso
                {
                    Id = model.Id,
                    Codigo = model.Codigo.Trim(),
                    Titulo = model.Titulo.Trim(),
                    TotalHorasPracticas = model.TotalHorasPracticas
                }, model.InstructoresIds, model.CohortesIds);

                TempData["Ok"] = "Curso actualizado correctamente.";
                return RedirectToAction("Cursos", new { verArchivados = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(BuildCursoForm(model));
            }
        }

        [SessionAuthorize("Administrador")]
        [HttpPost]
        public JsonResult ToggleArchivadoCursoAjax(int id)
        {
            try
            {
                _repo.ToggleArchivadoCurso(id);
                return Json(new { ok = true, mensaje = "Estado del curso actualizado." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        private CursoFormViewModel BuildCursoForm(CursoFormViewModel model)
        {
            model.Instructores = _repo.GetInstructores().Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.NombreCompleto,
                Selected = model.InstructoresIds != null && model.InstructoresIds.Contains(x.Id)
            });

            model.Cohortes = _repo.GetCohortes(true).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Nombre + (x.Archivado ? " (Archivada)" : ""),
                Selected = model.CohortesIds != null && model.CohortesIds.Contains(x.Id)
            });

            if (model.InstructoresIds == null)
                model.InstructoresIds = new List<int>();

            if (model.CohortesIds == null)
                model.CohortesIds = new List<int>();

            return model;
        }
    }
}