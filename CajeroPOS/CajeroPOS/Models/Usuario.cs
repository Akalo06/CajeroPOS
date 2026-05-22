namespace CajeroPOS.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string PasswordHash { get; set; }

        public string Salt { get; set; }

        public string Rol { get; set; }
    }
}