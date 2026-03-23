using System.Collections.Generic;

namespace CentroCapacitacionEmergencias.Models
{
    public class ParticipanteDetalleViewModel
    {
        public Participante Participante { get; set; }
        public string CohorteActual { get; set; }
        public List<CursoAsignadoViewModel> CursosAsignados { get; set; }
        public List<string> HistorialDestrezas { get; set; }
    }

    public class CursoAsignadoViewModel
    {
        public int CursoId { get; set; }
        public string CursoTitulo { get; set; }
        public bool Activo { get; set; }
    }
}