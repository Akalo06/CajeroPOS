namespace CajeroPOS.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        // relación 1-N
        public List<Producto> Productos { get; set; }
    }
}