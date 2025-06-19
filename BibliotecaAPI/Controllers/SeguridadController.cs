using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers
{
    [Route("api/seguridad")]
    [ApiController]
    public class SeguridadController: ControllerBase
    {
        private readonly IDataProtector protector;
        private readonly ITimeLimitedDataProtector protectorLimitadoPorTiempo;
        private readonly IServicioHash servicioHash;

        public SeguridadController(IDataProtectionProvider protectionProvider, IServicioHash servicioHash)
        {
            //nadie puede desencriptar lo que esta en esta clase 
            protector = protectionProvider.CreateProtector("SeguridadController"); //(parte de una llave)se le pasa un string de proposito
            protectorLimitadoPorTiempo = protector.ToTimeLimitedDataProtector();
            this.servicioHash = servicioHash;
        }
        //hash
        [HttpGet("hash")]
        public ActionResult Hash(string textoPlano)
        {
            var hash1 = servicioHash.Hash(textoPlano);
            var hash2 = servicioHash.Hash(textoPlano);
            var hash3 = servicioHash.Hash(textoPlano, hash2.Sal); //le pasamos la sal del hash2
            var resultado = new {textoPlano, hash1, hash2, hash3}; //devolvemos en una variable
            return Ok(resultado);
        
        }
        //fin hash
        [HttpGet("encriptar")]
        public ActionResult Encriptar(string textoPlano)
        {
            string textoCifrado = protector.Protect(textoPlano); //cifra el texto
            return Ok(new { textoCifrado }); //devuelve el texto cifrado
        }
        [HttpGet("desencriptar")]
        public ActionResult Desencriptar(string textoCifrado)
        {
            string textoPlano = protector.Unprotect(textoCifrado); //descifra el texto
            return Ok(new { textoPlano }); //devuelve el texto cifrado
        }
        //inicio protector limitados por tiempos
        [HttpGet("encriptar-limitado-tiempo")]
        public ActionResult EncriptarLimitadoTiempo(string textoPlano)
        {
            string textoCifrado = protectorLimitadoPorTiempo.Protect(textoPlano,lifetime: TimeSpan.FromSeconds(30)); //cifra el texto, expiracion de tiempo cifrado
            return Ok(new { textoCifrado }); //devuelve el texto cifrado
        }
        [HttpGet("desencriptar-limitado-tiempo")]
        public ActionResult DesencriptarLimitadoTiempo(string textoCifrado)
        {
            string textoPlano = protectorLimitadoPorTiempo.Unprotect(textoCifrado); //descifra el texto
            return Ok(new { textoPlano }); //devuelve el texto cifrado
        }
        //fin protector limitados por tiempos

    }
}
