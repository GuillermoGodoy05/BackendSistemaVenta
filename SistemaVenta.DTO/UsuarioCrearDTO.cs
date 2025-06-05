using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.DTO
{
    public class UsuarioCrearDTO
    {
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public int IdRol { get; set; }
        public string Clave { get; set; } // Acá sí
        public int EsActivo { get; set; }
    }
}
