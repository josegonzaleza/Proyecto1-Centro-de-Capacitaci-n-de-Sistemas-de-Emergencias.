namespace CentroCapacitacionEmergencias.Models
{
    public class Destreza
    {
        public int Id { get; set; }
        public int CursoId { get; set; }
        public string Nombre { get; set; }
        public decimal NotaMinimaAprobacion { get; set; }
        public bool Activa { get; set; }
    }
}