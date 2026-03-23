using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CentroCapacitacionEmergencias.Models
{
    public class ParticipanteFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tipo de identificación obligatorio.")]
        public string TipoIdentificacion { get; set; }

        [Required(ErrorMessage = "Identificación obligatoria.")]
        [StringLength(50)]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "Nombre completo obligatorio.")]
        [StringLength(150)]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "Fecha de nacimiento obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "Provincia obligatoria.")]
        [StringLength(100)]
        public string Provincia { get; set; }

        [Required(ErrorMessage = "Cantón obligatorio.")]
        [StringLength(100)]
        public string Canton { get; set; }

        [Required(ErrorMessage = "Distrito obligatorio.")]
        [StringLength(100)]
        public string Distrito { get; set; }

        [Required(ErrorMessage = "Detalle de dirección obligatorio.")]
        [StringLength(250)]
        public string DetalleDireccion { get; set; }

        [Required(ErrorMessage = "Estado civil obligatorio.")]
        [StringLength(20)]
        public string EstadoCivil { get; set; }

        [Required(ErrorMessage = "Correo obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        [StringLength(150)]
        public string Correo { get; set; }

        [StringLength(30)]
        public string Telefono { get; set; }

        [StringLength(250)]
        public string DireccionResidenciaSecundaria { get; set; }

        [StringLength(150)]
        public string ContactoEmergencia { get; set; }

        public int? CohorteId { get; set; }
        public List<int> CursosIds { get; set; }

        public IEnumerable<SelectListItem> Cohortes { get; set; }
        public IEnumerable<SelectListItem> Cursos { get; set; }

        public bool HabilitarAsignacion { get; set; }
    }
}