using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace INTELIGENTE_SAZÓN.Dtos
{
    public class ProfePerDtos
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        [Display(Name = "Correo electrónico.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debes confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Selecciona un rol..")]
        public string Role { get; set; }

        [Display(Name = "Certificado.")]
        public HttpPostedFileBase Certificate { get; set; }

        public string CertificatePath { get; set; }
    }
}