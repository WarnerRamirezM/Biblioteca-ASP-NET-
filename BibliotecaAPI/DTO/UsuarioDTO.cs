using System.Reflection.Metadata.Ecma335;

namespace BibliotecaAPI.DTO
{
    public class UsuarioDTO
    {
        public required string Email { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}
