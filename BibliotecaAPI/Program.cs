using BibliotecaAPI;
using BibliotecaAPI.Datos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var diccionario = new Dictionary<string, string>
{
    {"quien_soy","diccionario en memoria" }
};
builder.Configuration.AddInMemoryCollection(diccionario!); //tiene presedencia a -> la linea de comandos,variables de entorno,usersSecret,appSettings

// área de servicios
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddOptions<PersonaOpciones>()
    .Bind(builder.Configuration.GetSection(PersonaOpciones.Seccion))
    .ValidateDataAnnotations() //validaciones required
    .ValidateOnStart();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// área de middlewares

app.MapControllers();

app.Run();
