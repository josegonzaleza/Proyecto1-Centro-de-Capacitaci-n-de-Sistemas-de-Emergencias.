using System.Collections.Generic;

namespace CentroCapacitacionEmergencias.Models
{
    public class RiesgoDetalleViewModel
    {
        public int DestrezaId { get; set; }
        public string DestrezaNombre { get; set; }
        public List<RiesgoParticipanteItem> Participantes { get; set; }
    }

    public class RiesgoParticipanteItem
    {
        public int ParticipanteId { get; set; }
        public string CodigoInterno { get; set; }
        public string NombreCompleto { get; set; }
        public string Identificacion { get; set; }
        public decimal UltimoPuntajeFinal { get; set; }
        public string EstadoDominio { get; set; }
        public string CursoTitulo { get; set; }
        public string CohorteNombre { get; set; }
        public string FechaEvaluacionTexto { get; set; }
        public string Comentarios { get; set; }
    }
}