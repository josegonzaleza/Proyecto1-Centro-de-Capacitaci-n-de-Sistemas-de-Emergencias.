using System;

namespace CentroCapacitacionEmergencias.Models
{
    public class ParticipanteAsignacion
    {
        public int Id { get; set; }
        public int ParticipanteId { get; set; }
        public int CohorteId { get; set; }
        public int CursoId { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public DateTime? FechaFinAsignacion { get; set; }
        public string MotivoCambio { get; set; }
    }
}