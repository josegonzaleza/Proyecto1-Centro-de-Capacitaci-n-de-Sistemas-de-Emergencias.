using System;

namespace CentroCapacitacionEmergencias.Models
{
    public class Evaluacion
    {
        public int Id { get; set; }
        public int ParticipanteId { get; set; }
        public int CursoId { get; set; }
        public int DestrezaId { get; set; }
        public int InstructorId { get; set; }
        public int TiempoRespuestaSegundos { get; set; }
        public int TiempoMinutos { get; set; }
        public int TiempoSegundos { get; set; }
        public decimal PuntajeOriginal { get; set; }
        public decimal PuntajeFinal { get; set; }
        public string AceptacionInstructor { get; set; }
        public string Comentarios { get; set; }
        public DateTime FechaEvaluacion { get; set; }
        public string EstadoDominio { get; set; }
    }
}