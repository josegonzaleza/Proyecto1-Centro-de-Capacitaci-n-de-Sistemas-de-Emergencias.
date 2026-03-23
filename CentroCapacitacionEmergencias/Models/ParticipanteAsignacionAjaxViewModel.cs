using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentroCapacitacionEmergencias.Models
{
    public class ParticipanteAsignacionAjaxViewModel
    {
        [Required]
        public int ParticipanteId { get; set; }

        [Required]
        public int CohorteId { get; set; }

        [Required]
        public List<int> CursosIds { get; set; }
    }
}