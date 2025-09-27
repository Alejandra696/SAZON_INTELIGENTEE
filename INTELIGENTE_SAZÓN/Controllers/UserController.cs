using INTELIGENTE_SAZÓN.Dtos;
using Sazon_Inteligente.Services;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}