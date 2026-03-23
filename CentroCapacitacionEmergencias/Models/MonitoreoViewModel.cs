using System.Collections.Generic;
using System.Web.Mvc;

namespace CentroCapacitacionEmergencias.Models
{
    public class MonitoreoViewModel
    {
        public int CohorteId { get; set; }
        public int CursoId { get; set; }

        public IEnumerable<SelectListItem> Cohortes { get; set; }
        public IEnumerable<SelectListItem> Cursos { get; set; }

        public decimal TasaGlobalAprobacion { get; set; }
        public int EstadoCertificacion { get; set; }
        public int IntervencionPendiente { get; set; }

        public List<RiesgoItem> Riesgos { get; set; }
    }

    public class RiesgoItem
    {
        public int DestrezaId { get; set; }
        public string Destreza { get; set; }
        public int Cantidad { get; set; }
    }
}