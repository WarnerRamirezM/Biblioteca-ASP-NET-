using System.Security.Principal;

namespace BibliotecaAPI.DTO
{
    public class ComentarioDTO
    {
        public Guid Id { get; set; } //string aleatorio
        public required string Cuerpo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        //datos del usuario dentro del comentario
        public required string UsuarioId { get; set; }
        public required string UsuarioEmail { get; set; }
    }
}
