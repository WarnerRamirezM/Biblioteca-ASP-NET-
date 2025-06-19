using BibliotecaAPI.DTO;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace BibliotecaAPI.Servicios
{
    public class ServicioHash : IServicioHash
    {
        public ResultadoHashDTO Hash(string input)
        {
            var sal = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(sal);

            }
            return Hash(input, sal);
        }
        //implementarlo 
        public ResultadoHashDTO Hash(string input, byte[] sal)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: input,
                salt: sal,
                prf: KeyDerivationPrf.HMACSHA1, //algoritmo utilizado
                iterationCount: 10_000, //iteraciones se ejecuta 10mil veces
                numBytesRequested: 256 / 8 //tamaño del hash 256 bits


                ));
            return new ResultadoHashDTO
            {
                Hash = hashed,
                Sal = sal

            };
        }
    }
}
