using System;

namespace CentroCapacitacionEmergencias.Models
{
    public class Cohorte
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Archivado { get; set; }
        public bool Activo { get; set; }
    }
}