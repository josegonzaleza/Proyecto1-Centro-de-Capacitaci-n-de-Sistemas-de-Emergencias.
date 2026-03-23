namespace CentroCapacitacionEmergencias.Models
{
    public class Curso
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Titulo { get; set; }
        public int TotalHorasPracticas { get; set; }
        public bool Archivado { get; set; }
        public bool Activo { get; set; }
    }
}