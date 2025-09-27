using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace INTELIGENTE_SAZÓN.Dtos
{
    public class UserDtos
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
    public class LoginDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

