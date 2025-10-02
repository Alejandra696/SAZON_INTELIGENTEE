using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

//CONTROLADOR PARA EL REGISTRO DE USUARIOS
namespace INTELIGENTE_SAZÓN.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController()
        {
            _userService = new UserService();
        }

        // MUESTRA EL FORMULARIO
        [HttpGet]
        public ActionResult RegisterUser()
        {
            return View(new UserDtos());
        }

        // PROCESA EL FORMULARIO
        [HttpPost]
        public ActionResult Register(UserDtos model)
        {
            if (ModelState.IsValid)
            {
                bool registrado = _userService.Register(model);

                if (registrado)
                {
                    return View("SucessUser");
                }
            }
            return View("RegisterUser", model);
        }

        public ActionResult SucessUser()
        {
            return View();
        }


        //CONTROLADOR PARA EL INICIO DE SESIÒN DE LOS USUARIOS

        //MUESTRA EL FORMULARIO DE LOGIN
        [HttpGet]
        public ActionResult LoginUser()
        {
            return View(new UserDtos());
        }

        // PROCESA EL FORMULARIO DE LOGIN
        [HttpPost]
        [ValidateAntiForgeryToken] // SEGURIDAD CONTRA ATAQUES CSRF, PARA QUE LOS PERFILES ACTIVOS NO ESTEN EN PELIGROOU
        public ActionResult Login(UserDtos model)
        {
            if (ModelState.IsValid)
            {
                bool loginOk = _userService.Login(model.FullName, model.Password);
                if (loginOk)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            }

            return View(model);
        }

        // VISTA DEL ADMINISTRADOR
        [HttpGet]
        public ActionResult AdmiPrincipalUser()
        {
            return View();
        }


        // VISTA PARA QUE EL ADMI CREE LOS PERFILES PROFESIONALES  
        [HttpGet]
        public ActionResult AdmiProfePerUser()
        {
            // Muestra la vista con un modelo vacío  
            return View(new ProfePerDtos());
        }

        // PROCESA EL FORMULARIO DE LOS PERFILES PROFESIONALES  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProfessionalProfile(ProfePerDtos model)
        {
            // Solo admin puede acceder
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                return RedirectToAction("Login");

            // Valida el modelo
            if (!ModelState.IsValid)
                return View(model);

            string savedFilePath = null;

            // Validar archivo PDF  
            if (model.Certificate != null && model.Certificate.ContentLength > 0)
            {
                var allowedExt = new[] { ".pdf" };
                var ext = Path.GetExtension(model.Certificate.FileName)?.ToLower();
                if (Array.IndexOf(allowedExt, ext) < 0)
                {
                    ModelState.AddModelError("Certificate", "Tipo de archivo no permitido (solo PDF).");
                    return View(model);
                }

                const int maxBytes = 5 * 1024 * 1024; // 5 MB  
                if (model.Certificate.ContentLength > maxBytes)
                {
                    ModelState.AddModelError("Certificate", "El archivo excede 5 MB.");
                    return View(model);
                }

                // Carpeta de guardado
                var folder = Server.MapPath("~/Content/certificates");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                // Nombre único
                var uniqueName = Guid.NewGuid().ToString() + ext;
                var path = Path.Combine(folder, uniqueName);
                model.Certificate.SaveAs(path);
                savedFilePath = "/Content/certificates/" + uniqueName;
            }

            // Llamar al servicio para guardar en BD
            var created = _userService.CreateProfessionalProfile(new ProfePerDtos
            {
                Nombre = model.Nombre,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role,
                CertificatePath = savedFilePath
            });

            // ✅ Aquí devolvemos siempre algo en todos los caminos
            if (created)
            {
                TempData["Success"] = "Perfil creado correctamente.";
                return RedirectToAction("AdmiPrincipalUser");
            }

            // Si no se pudo crear
            ModelState.AddModelError("", "Error al crear el perfil. Intenta nuevamente.");
            return View(model);
        }
    }
}