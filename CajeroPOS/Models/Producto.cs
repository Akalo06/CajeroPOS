using System.IO;

namespace CajeroPOS.Models
{
    public class Producto
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public decimal Precio { get; set; }

        public int Cantidad { get; set; }


        public string Imagen { get; set; }


        public int? CategoriaId { get; set; }

        public Categoria Categoria { get; set; }

        //convierte el nombre de la imagen guardada en un archivo en la carpeta iamgenes
        public string ImagenPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Imagen))
                    return null;

                return Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "imagenes",
                    Imagen
                );
            }


        }

        public decimal PrecioTotal
        {
            get
            {
                return Precio * Cantidad;
            }
        }
    }
}