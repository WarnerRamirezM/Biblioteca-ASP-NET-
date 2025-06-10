using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/configuraciones")]
    public class ConfiguracionesController: ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection seccion01;
        private readonly IConfigurationSection seccion02;

        public ConfiguracionesController(IConfiguration configuration)
        {
            this.configuration = configuration;
            seccion01 = configuration.GetSection("seccion_1");
            seccion02 = configuration.GetSection("seccion_2");
        }

        [HttpGet("seccion01")]
        public IActionResult GetSeccion01()
        {
            string nombre = seccion01.GetValue<string>("nombre")!;
            int edad = seccion01.GetValue<int>("edad");


            return Ok(new { nombre, edad }); //retornamos objeto anonimo
        }
        [HttpGet("seccion02")]
        public IActionResult GetSeccion02()
        {
            string nombre = seccion01.GetValue<string>("nombre")!;
            int edad = seccion01.GetValue<int>("edad");


            return Ok(new { nombre, edad }); //retornamos objeto anonimo
        }

        //para debug 
        [HttpGet("ObtenerTodos")]
        public ActionResult ObtenerTodos()
        {
            var hijos = seccion02.GetChildren().Select(x => $"{x.Key}:{x.Value}");
            return Ok(new { hijos });
        }

        [HttpGet]
        public ActionResult GetProveedor()
        {
            var valor = configuration.GetValue<string>("quien_soy"); //devuelto de appsetting development
            return Ok(new { valor });
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            var opcion1 = configuration["apellido"]; //esta variable esta en el appsetting.development.json
            var opcion2 = configuration.GetValue<string>("apellido")!; //devuelta por el tiempo de dato especifico
            return opcion2;


        }

        [HttpGet("secciones")]
        public ActionResult<string> GetSeccion()
        {
            var opcion1 = configuration["ConnectionString:DefaultConnection"];
           var opcion2 = configuration.GetValue<string>("ConnectionString:DefaultConnection");
            var seccion = configuration.GetSection("ConnectionString"); //obtiene todas las llaves de la seccion 
            var opcion3 = seccion["DefaultConnection"];
            return opcion3!;
        }

    }
}
