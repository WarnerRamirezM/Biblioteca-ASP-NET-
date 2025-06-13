namespace BibliotecaAPI.DTO
{
    public class RespuestaAutenticacionDTO
    {
        //cuando el usuario se logea representa el valor de salida.
        public required string Token { get; set; }
        public DateTime Expiracion { get; set; }


    }
}
