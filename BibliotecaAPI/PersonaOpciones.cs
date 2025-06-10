using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI
{
    public class PersonaOpciones
    {
        //esta data se actualiza mediante ioption snpshot
        public const string Seccion = "seccion_1";
        [Required]
        public required string Nombre {  get; set; }
        public int Edad {  get; set; }

    }
}
