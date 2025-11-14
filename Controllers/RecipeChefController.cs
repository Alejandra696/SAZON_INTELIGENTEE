using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Services;
using System;
using System.Web;
using System.Web.Mvc;

namespace INTELIGENTE_SAZÓN.Controllers
{
    public class RecipeChefController : Controller
    {
        private readonly RecipeChefService _service;

        public RecipeChefController()
        {
            _service = new RecipeChefService();
        }

        // ============================================================
        // GET: Vista para crear recetas
        // ============================================================
        [HttpGet]
        public ActionResult ChefCreateRecipe()
        {
            ViewBag.Units = new SelectList(_service.GetUnits(), "ID_Unit_Measure", "type_Unit_Measure");
            ViewBag.Foods = new SelectList(_service.GetFoods(), "ID_Food", "name_Food");
            return View();
        }

        // ============================================================
        // POST: Guardar receta
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChefCreateRecipe(FormCollection form)
        {
            try
            {
                var dto = new RecipeChefDto
                {
                    name_Recipe = form["name_Recipe"],
                    instrucc_Recipe = form["instrucc_Recipe"],
                    imag_File = Request.Files["imag_File"]
                };

                // Obtener arrays de ingredientes
                var amounts = form.GetValues("ingredientAmount");
                var units = form.GetValues("ingredientUnit");
                var foods = form.GetValues("ingredientFood");

                if (amounts != null)
                {
                    for (int i = 0; i < amounts.Length; i++)
                    {
                        if (int.TryParse(amounts[i], out int amount) &&
                            int.TryParse(units[i], out int unitId) &&
                            int.TryParse(foods[i], out int foodId))
                        {
                            dto.Ingredients.Add(new RecipeIngredientDto
                            {
                                Amount = amount,
                                UnitId = unitId,
                                FoodId = foodId
                            });
                        }
                    }
                }

                bool ok = _service.SaveRecipe(dto, out string msg);

                if (ok)
                {
                    TempData["Success"] = msg;
                    return RedirectToAction("ChefPrincipalUser", "User");
                }
                else
                {
                    TempData["Error"] = msg;
                    return RedirectToAction("ChefCreateRecipe");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error inesperado: " + ex.Message;
                return RedirectToAction("ChefCreateRecipe");
            }
        }
        // =======================================================================
        // MÉTODO: Redirigir a vista principal del usuario (ClientPrincipalUser)
        // ========================================================================
        public ActionResult ClientPrincipal()
        {
            try
            {
                return RedirectToAction("ClientPrincipalUser", "Client");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al redirigir a la vista del usuario: " + ex.Message;
                return View("Error");
            }
        }
    }
}