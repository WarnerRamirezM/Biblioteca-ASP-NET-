using BibliotecaAPI;
using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// área de servicios
//proteccion de datos 
builder.Services.AddDataProtection(); 
var origenesPermitidos = builder.Configuration.GetSection("origenesPermitidos").Get<string[]>()!; //obtenido del appsetting.dev.json para obtener los dominios permitidos
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddIdentityCore<Usuario>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<UserManager<Usuario>>();
builder.Services.AddScoped<SignInManager<Usuario>>();
builder.Services.AddTransient<IServiciosUsuarios, ServiciosUsuarios>();
builder.Services.AddTransient<IServicioHash, ServicioHash>();   
//configurar cors
builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(opcionesCORS =>
    {
        opcionesCORS.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader() //permite cualquier origen, metodo y cabecera
                                 .WithExposedHeaders("mi-cabecera"); //esto expone las cabeceras pasadas por el CORS


        });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false;
    opciones.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["llavejwt"]!)),
        ClockSkew = TimeSpan.Zero


    };
});
//politica de autorizacion
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("esadmin", politica => politica.RequireClaim("esadmin")); //se crea una politica que necesita tener un claim 
    //se pueden crear tantas politicas como quiera..
}

);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// área de middlewares
//indicar que queremos utilizar cors
app.Use(async (contexto, next) => {
    contexto.Response.Headers.Append("mi-cabecera", "valor");
    await next();
});
app.UseCors();
app.MapControllers();

app.Run();
