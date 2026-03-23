using System;
using System.Linq;
using System.Web.Mvc;
using CentroCapacitacionEmergencias.Filters;
using CentroCapacitacionEmergencias.Models;
using CentroCapacitacionEmergencias.Repositories;

namespace CentroCapacitacionEmergencias.Controllers
{
    [SessionAuthorize("Instructor")]
    public class EvaluacionesController : Controller
    {
        private readonly EvaluacionRepository _repo = new EvaluacionRepository();

        [HttpGet]
        public ActionResult Create(int participanteId, int? cursoId = null, int? destrezaId = null)
        {
            var user = Session["User"] as SessionUser;
            var participante = _repo.GetParticipante(participanteId);
            if (participante == null) return HttpNotFound();

            var cursos = _repo.GetCursosDelParticipante(participanteId, user.Id);

            var vm = new EvaluacionViewModel
            {
                ParticipanteId = participanteId,
                ParticipanteNombre = participante.NombreCompleto,
                TiempoMinutos = 0,
                TiempoSegundos = 0,
                PuntosControl = new System.Collections.Generic.List<PuntoControlItem>(),
                CursosDisponibles = cursos.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Titulo,
                    Selected = cursoId.HasValue && cursoId.Value == x.Id
                }),
                DestrezasDisponibles = new System.Collections.Generic.List<SelectListItem>()
            };

            if (cursoId.HasValue)
            {
                vm.CursoId = cursoId.Value;
                vm.CursoNombre = _repo.GetCursoNombre(cursoId.Value);

                var destrezas = _repo.GetDestrezasPorCurso(cursoId.Value);
                vm.DestrezasDisponibles = destrezas.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Nombre,
                    Selected = destrezaId.HasValue && destrezaId.Value == x.Id
                });

                if (destrezaId.HasValue)
                {
                    vm.DestrezaId = destrezaId.Value;
                    vm.DestrezaNombre = _repo.GetDestrezaNombre(destrezaId.Value);

                    vm.PuntosControl = _repo.GetPuntosControlAjax(destrezaId.Value)
                        .Select(x => new PuntoControlItem
                        {
                            PuntoControlId = x.Id,
                            Descripcion = x.Descripcion,
                            Cumplido = null
                        }).ToList();
                }
            }

            return View(vm);
        }

        [HttpGet]
        public JsonResult DestrezasPorCurso(int cursoId)
        {
            var user = Session["User"] as SessionUser;

            if (!_repo.CursoPerteneceAInstructor(cursoId, user.Id))
                return Json(new { ok = false, mensaje = "No autorizado." }, JsonRequestBehavior.AllowGet);

            var data = _repo.GetDestrezasPorCurso(cursoId);
            return Json(new { ok = true, data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult PuntosControlPorDestreza(int destrezaId)
        {
            var data = _repo.GetPuntosControlAjax(destrezaId);
            return Json(new { ok = true, data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EvaluacionViewModel model)
        {
            var user = Session["User"] as SessionUser;
            var participante = _repo.GetParticipante(model.ParticipanteId);

            if (participante == null)
                return HttpNotFound();

            model.ParticipanteNombre = participante.NombreCompleto;
            model.CursoNombre = _repo.GetCursoNombre(model.CursoId);
            model.DestrezaNombre = _repo.GetDestrezaNombre(model.DestrezaId);

            var cursos = _repo.GetCursosDelParticipante(model.ParticipanteId, user.Id);
            model.CursosDisponibles = cursos.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Titulo,
                Selected = x.Id == model.CursoId
            });

            var destrezas = _repo.GetDestrezasPorCurso(model.CursoId);
            model.DestrezasDisponibles = destrezas.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Nombre,
                Selected = x.Id == model.DestrezaId
            });

            if (!_repo.CursoPerteneceAInstructor(model.CursoId, user.Id))
                ModelState.AddModelError("", "No tiene permiso para evaluar este curso.");

            if (!_repo.ParticipanteTieneCursoActivo(model.ParticipanteId, model.CursoId))
                ModelState.AddModelError("", "El participante no tiene asignado este curso de forma activa.");

            if (model.PuntosControl == null || model.PuntosControl.Count == 0)
            {
                ModelState.AddModelError("", "La destreza no tiene puntos de control configurados.");
                model.PuntosControl = _repo.GetPuntosControlAjax(model.DestrezaId)
                    .Select(x => new PuntoControlItem
                    {
                        PuntoControlId = x.Id,
                        Descripcion = x.Descripcion,
                        Cumplido = null
                    }).ToList();
            }
            else
            {
                for (int i = 0; i < model.PuntosControl.Count; i++)
                {
                    if (!model.PuntosControl[i].Cumplido.HasValue)
                    {
                        ModelState.AddModelError("", "Debe indicar Cumplido o No Cumplido para todos los puntos críticos.");
                        break;
                    }
                }
            }

            if (_repo.YaCertificadaConMaxima(model.ParticipanteId, model.DestrezaId))
                ModelState.AddModelError("", "La destreza ya fue certificada con la nota máxima. No se puede registrar una nueva calificación.");

            if (!ModelState.IsValid)
                return View(model);

            model.PuntajeFinalCalculado = _repo.CalcularPuntajeFinal(model.PuntajeOriginal, model.PuntosControl);
            model.EstadoDominioCalculado = _repo.CalcularEstadoDominio(model.PuntajeFinalCalculado);

            _repo.Guardar(model, user.Id);

            TempData["Ok"] = "Evaluación registrada correctamente.";
            return RedirectToAction("Details", "Participantes", new { id = model.ParticipanteId });
        }
    }
}