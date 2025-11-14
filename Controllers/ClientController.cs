using System.Collections.Generic;
using System.Web.Mvc;
using INTELIGENTE_SAZÓN.Services;
using INTELIGENTE_SAZÓN.Dtos;
using System;

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


        // <summary>
        /// GET: Mostrar recomendaciones de recetas para el usuario actual (usa Session["UserEmail"])
        /// </summary>
        [HttpGet]
        public ActionResult RecipesRecommendations()
        {
            string email = Session["UserEmail"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para ver las recomendaciones.";
                return RedirectToAction("LoginUser", "User");
            }

            var model = _service.GetRecipeRecommendationsByEmail(email);
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(model);
        }

        /// <summary>
        /// POST: Toggle favorito (AJAX)
        /// </summary>
        [HttpPost]
        public JsonResult ToggleFavorite(int idRecipe)
        {
            try
            {
                string email = Session["UserEmail"]?.ToString();
                if (string.IsNullOrEmpty(email))
                    return Json(new { ok = false, message = "Sesión no iniciada" });

                bool nowFav = _service.ToggleFavoriteByEmail(email, idRecipe);
                return Json(new { ok = true, isFavorite = nowFav });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }

        }
        // ======================================================
        // VISTA: Mostrar recetas favoritas del usuario logueado
        // ======================================================
        public ActionResult FavoriteRecipes()
        {
            // Validar sesión
            if (Session["UserEmail"] == null)
                return RedirectToAction("LoginUser", "User");

            try
            {
                string email = Session["UserEmail"].ToString();
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                var favorites = _service.GetFavoriteRecipesByEmail(email);

                return View(favorites);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<INTELIGENTE_SAZÓN.Dtos.RecipeRecommendationDto>());
            }
        }

        // ==================
        // MÉTODO: VOLVER
        // ==================
        public ActionResult BackToMain()
        {
            return RedirectToAction("ClientPrincipalUser", "User"); // Cambia "MainInterface" por el nombre de tu vista principal
        }
    }
    
}