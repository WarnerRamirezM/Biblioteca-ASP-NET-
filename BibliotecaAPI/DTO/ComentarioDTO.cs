namespace BibliotecaAPI.DTO
{
    public class ComentarioDTO
    {
        public Guid Id { get; set; } //string aleatorio
        public required string Cuerpo { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
