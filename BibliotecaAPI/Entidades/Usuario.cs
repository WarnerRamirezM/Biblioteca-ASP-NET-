using Microsoft.AspNetCore.Identity;

namespace BibliotecaAPI.Entidades
{
    public class Usuario: IdentityUser //clase que representa un usuario
    {
        public DateTime FechaNacimiento { get; set; }

    }
}
