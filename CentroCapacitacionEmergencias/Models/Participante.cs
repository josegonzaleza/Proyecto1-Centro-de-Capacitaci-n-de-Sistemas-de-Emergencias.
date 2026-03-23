using System;

namespace CentroCapacitacionEmergencias.Models
{
    public class Participante
    {
        public int Id { get; set; }
        public string CodigoInterno { get; set; }
        public string TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string NombreCompleto { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Provincia { get; set; }
        public string Canton { get; set; }
        public string Distrito { get; set; }
        public string DetalleDireccion { get; set; }
        public string EstadoCivil { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string DireccionResidenciaSecundaria { get; set; }
        public string ContactoEmergencia { get; set; }
        public string EstadoCertificacion { get; set; }
        public bool Activo { get; set; }
    }
}