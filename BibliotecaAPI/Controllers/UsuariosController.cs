using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTO;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
   
    public class UsuariosController: ControllerBase
    {
        private readonly UserManager<Usuario> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IServiciosUsuarios serviciosUsuarios;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        //clase para la autenticacion 
        public UsuariosController(UserManager<Usuario> userManager, IConfiguration configuration, 
            SignInManager<Usuario> signInManager, IServiciosUsuarios serviciosUsuarios,
            ApplicationDbContext context, IMapper mapper) //para crear un usuario, y configuration para acceder a valores de un proveedor de configuracion 
        {

            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.serviciosUsuarios = serviciosUsuarios;
            this.context = context;
            this.mapper = mapper;
        }
        [HttpGet]
        [Authorize(Policy = "esadmin")]
        public async Task<IEnumerable<UsuarioDTO>> Get()
        {
            var usuarios = await context.Users.ToListAsync(); //obtenemos la lista de usuarios
            var usuarioDTO = mapper.Map<IEnumerable<UsuarioDTO>>(usuarios); //mapeo 
            return usuarioDTO;


        }

        [HttpPost("registro")]
       // [AllowAnonymous]
        //Devolvemos la respuesta y mandamos como parametros los valores necesarios para la autenticacion 
        //https://localhost:7194/api/usuarios/registro
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = new Usuario
            {
                UserName = credencialesUsuarioDTO.Email, //nombre de usuario 
                Email = credencialesUsuarioDTO.Email //y el correo 
            };
            var resultado = await userManager.CreateAsync(usuario, credencialesUsuarioDTO.Password!); //le mandamos el usuario y le mandamos la contraseña
            if (resultado.Succeeded) //si es exitoso se contruye el token
            {
                var respuestaAutenticacion = await ConstruirToken(credencialesUsuarioDTO);
                return respuestaAutenticacion;
            }
            else //si hubo un error
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return ValidationProblem(); //devuelve una respuesta HTTP 400 con los detalles de validación.
            }


        }
        [HttpGet("renovar-token")]
        [Authorize]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> RenovarToken()
        {
            var usuario = await serviciosUsuarios.ObtenerUsuario();
            if(usuario is null)
            {
                return NotFound();
            }
            var credencialesUsuarioDTO = new CredencialesUsuarioDTO { Email = usuario.Email! }; //se le manda el email del usuario por es el que se necesita de claim
            var respuestaAutenticacion = await ConstruirToken(credencialesUsuarioDTO); //mandamos las credenciales a la creacion del token
            return respuestaAutenticacion;

        }
        [HttpPost("hacer-admin")]
        [Authorize(Policy = "esadmin")]
        public async Task<ActionResult> HacerAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);
            if(usuario is null)
            {
                return NotFound();
            }
            await userManager.AddClaimAsync(usuario, new Claim("esadmin", "true")); //creamos un nuevo claim para el usuario 
            return NoContent();

        }
        [HttpPost("remover-admin")]
        [Authorize(Policy = "esadmin")]
        public async Task<ActionResult> RemoverAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);
            if (usuario is null)
            {
                return NotFound();
            }
            await userManager.RemoveClaimAsync(usuario, new Claim("esadmin", "true")); //creamos un nuevo claim para el usuario 
            return NoContent();

        }
        [HttpPut]
        [Authorize]
        public async Task<ActionResult> Put(ActualizarUsuarioDTO actualizarUsuarioDTO)
        {
            var usuario = await serviciosUsuarios.ObtenerUsuario();
            if(usuario is null)
            {
                return NotFound();
            }
            usuario.FechaNacimiento = actualizarUsuarioDTO.FechaNacimiento;
            await userManager.UpdateAsync(usuario);
            return NoContent();
        }

        //El siguiente metodo loguea el usuario
        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(
            CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email); //busqueda por email 
            if(usuario is null)
            {
                return RetornarLoginIncorrecto();
            }
            var resultado = await signInManager.CheckPasswordSignInAsync(usuario,
                credencialesUsuarioDTO.Password!, lockoutOnFailure: false);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuarioDTO);
            }
            else
            {
                return RetornarLoginIncorrecto();
            }

        }
        //metodo para retornar un msj de error cuando el login es incorrecto por el email 
        private ActionResult RetornarLoginIncorrecto()
        {
            ModelState.AddModelError(string.Empty, "Login incorrecto");
            return ValidationProblem();
        }
        private async Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            //se crea los claims= informacion acerca de los usuarios
            var claims = new List<Claim>
            {
                new Claim("email", credencialesUsuarioDTO.Email),
                new Claim("lo que quiera", "cualquier valor")
            };
            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);
            var ClaimsDB = await userManager.GetClaimsAsync(usuario!);
            claims.AddRange(ClaimsDB);
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]!));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256); //algoritmo que permite firmar el jwt para que nadie pueda editar los claims
            var expiracion = DateTime.UtcNow.AddYears(1); //token con un año de vigencia
            var tokenDeSeguridad = new JwtSecurityToken(issuer: null, audience: null, claims:claims, expires:expiracion,signingCredentials:credenciales);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);
            
            return new RespuestaAutenticacionDTO 
            { 
                Token = token,
                Expiracion = expiracion
            };
        }

    }
}
