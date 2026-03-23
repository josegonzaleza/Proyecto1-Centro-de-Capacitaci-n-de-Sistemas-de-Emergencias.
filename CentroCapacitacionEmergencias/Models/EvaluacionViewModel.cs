using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CentroCapacitacionEmergencias.Models
{
    public class EvaluacionViewModel
    {
        [Required]
        public int ParticipanteId { get; set; }

        [Required]
        public int CursoId { get; set; }

        [Required]
        public int DestrezaId { get; set; }

        public string ParticipanteNombre { get; set; }
        public string CursoNombre { get; set; }
        public string DestrezaNombre { get; set; }

        [Required(ErrorMessage = "El tiempo en minutos es obligatorio.")]
        [Range(0, 999)]
        public int TiempoMinutos { get; set; }

        [Required(ErrorMessage = "El tiempo en segundos es obligatorio.")]
        [Range(0, 59)]
        public int TiempoSegundos { get; set; }

        [Required(ErrorMessage = "El puntaje final de ejecución es obligatorio.")]
        [Range(typeof(decimal), "1", "100")]
        public decimal PuntajeOriginal { get; set; }

        [Required(ErrorMessage = "La aceptación del instructor es obligatoria.")]
        [StringLength(150)]
        public string AceptacionInstructor { get; set; }

        [StringLength(500)]
        public string Comentarios { get; set; }

        public decimal PuntajeFinalCalculado { get; set; }
        public string EstadoDominioCalculado { get; set; }

        public List<PuntoControlItem> PuntosControl { get; set; }

        public IEnumerable<SelectListItem> CursosDisponibles { get; set; }
        public IEnumerable<SelectListItem> DestrezasDisponibles { get; set; }
    }

    public class PuntoControlItem
    {
        public int PuntoControlId { get; set; }
        public string Descripcion { get; set; }

        [Required]
        public bool? Cumplido { get; set; }
    }
}