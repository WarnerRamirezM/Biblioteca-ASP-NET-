using BibliotecaAPI.DTO;

namespace BibliotecaAPI.Servicios
{
    public interface IServicioHash
    {
        ResultadoHashDTO Hash(string input);
        ResultadoHashDTO Hash(string input, byte[] sal);
    }
}