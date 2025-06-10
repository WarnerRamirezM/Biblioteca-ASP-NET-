using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTO
{
    public class LibroCreacionDTO
    {
        [Required]
        public required string Titulo { get; set; }
        public List<int> AutoresIds { get; set; } = []; 
    }
}
