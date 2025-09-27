using INTELIGENTE_SAZÓN.Dtos;
using Sazon_Inteligente.Services;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}
