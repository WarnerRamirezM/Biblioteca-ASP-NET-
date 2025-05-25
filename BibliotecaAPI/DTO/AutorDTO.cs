namespace BibliotecaAPI.DTO
{
    /*
     ¿Por qué usar DTOs?

    Seguridad: Ocultas datos sensibles (como contraseñas, tokens, etc.).

    Flexibilidad: Controlas qué datos se envían y reciben.

    Desacoplamiento: Mantienes independencia entre la lógica de negocio y la presentación o transporte de datos.

    Optimización: Puedes enviar solo los datos necesarios, reduciendo el tráfico en red o carga de datos.
    */
    public class AutorDTO
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }


    }
}
