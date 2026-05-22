using System;
using System.Collections.Generic;

namespace CajeroPOS.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }

        public decimal Importe { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }
}