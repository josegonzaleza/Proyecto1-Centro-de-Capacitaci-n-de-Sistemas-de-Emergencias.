namespace CentroCapacitacionEmergencias.Models
{
    public class IntervencionPendienteItem
    {
        public int ParticipanteId { get; set; }
        public string CodigoInterno { get; set; }
        public string NombreCompleto { get; set; }
        public string CursoTitulo { get; set; }
        public string CohorteNombre { get; set; }
        public decimal HorasCompletadas { get; set; }
        public int HorasRequeridas { get; set; }
        public decimal PorcentajeCumplimiento { get; set; }
    }
}