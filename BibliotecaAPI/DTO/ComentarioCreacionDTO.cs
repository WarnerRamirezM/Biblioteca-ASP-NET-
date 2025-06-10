using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTO
{
    public class ComentarioCreacionDTO
    {
        [Required]
        public required string Cuerpo { get; set; }


       
    }
}
