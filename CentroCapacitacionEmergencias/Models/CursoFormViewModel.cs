using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CentroCapacitacionEmergencias.Models
{
    public class CursoFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(30)]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150)]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "Las horas prácticas son obligatorias.")]
        [Range(1, 10000)]
        public int TotalHorasPracticas { get; set; }

        public bool Archivado { get; set; }

        [Required(ErrorMessage = "Debe seleccionar al menos un instructor.")]
        public List<int> InstructoresIds { get; set; }

        [Required(ErrorMessage = "Debe seleccionar al menos una cohorte.")]
        public List<int> CohortesIds { get; set; }

        public IEnumerable<SelectListItem> Instructores { get; set; }
        public IEnumerable<SelectListItem> Cohortes { get; set; }
    }
}