using System.Collections.Generic;
using System.Web.Mvc;
using INTELIGENTE_SAZÓN.Services;
using INTELIGENTE_SAZÓN.Dtos;

namespace INTELIGENTE_SAZÓN.Controllers
{
    public class ClientController : Controller
    {
        private readonly ClientService _service;

        public ClientController()
        {
            _service = new ClientService();
        }

        [HttpGet]
        public ActionResult RegisterProduct()
        {
            var model = _service.LoadRegisterProductData();

            ViewBag.Message = TempData["SuccessMessage"];

            return View(model);
        }

        [HttpPost]
        public ActionResult SaveProducts(List<RegisterProductItemDto> items)
        {
            string email = Session["UserEmail"]?.ToString();

            if (email == null)
            {
                TempData["SuccessMessage"] = "Debes iniciar sesión.";
                return RedirectToAction("LoginUser", "User");
            }

            bool ok = _service.SaveProducts(email, items);

            if (ok)
                TempData["SuccessMessage"] = "✅ Registro exitoso";

            return RedirectToAction("RegisterProduct"); // CORREGIDO
        }

        [HttpGet]
        public ActionResult ViewInventory()
        {
            string email = Session["UserEmail"]?.ToString();

            if (email == null)
                return RedirectToAction("LoginUser", "User");

            var grouped = _service.GetInventoryGrouped(email);

            return View(grouped); // <--- ESTE ES EL DICCIONARIO
        }
    }
}