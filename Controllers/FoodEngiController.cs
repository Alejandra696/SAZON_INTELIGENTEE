using INTELIGENTE_SAZÓN.Services;
using System;
using System.Web;
using System.Web.Mvc;
using INTELIGENTE_SAZÓN.Dtos;
using System.Collections.Generic;

namespace INTELIGENTE_SAZÓN.Controllers
{
    public class FoodEngiController : Controller
    {
        private readonly FoodEngiService _service;

        public FoodEngiController()
        {
            _service = new FoodEngiService();
        }

        // ============================================================
        // GET: Vista del formulario - REGISTRO TIEMPO ÓPTIMO
        // ============================================================
        [HttpGet]
        public ActionResult EngiRegisterFood()
        {
            return View();
        }

        // ============================================================
        // POST: Guarda múltiples alimentos (Tiempo óptimo)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EngiRegisterFoodSave()
        {
            try
            {
                var names = Request.Form.GetValues("foodName");
                var days = Request.Form.GetValues("foodDays");
                var files = Request.Files;

                // ==============================
                // 🔍 VALIDACIONES DE CAMPOS
                // ==============================
                if (names == null || days == null || files.Count == 0)
                {
                    TempData["Error"] = "Por favor, complete todos los campos antes de guardar.";
                    return RedirectToAction("EngiRegisterFood");
                }

                for (int i = 0; i < names.Length; i++)
                {
                    string nombre = names[i]?.Trim();
                    string dias = days[i]?.Trim();
                    HttpPostedFileBase imagen = files[i];

                    if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(dias) || imagen == null || imagen.ContentLength == 0)
                    {
                        TempData["Error"] = "Todos los alimentos deben tener nombre, imagen y tiempo sugerido en días.";
                        return RedirectToAction("EngiRegisterFood");
                    }

                    if (!int.TryParse(dias, out int d) || d <= 0)
                    {
                        TempData["Error"] = "El tiempo sugerido debe ser un número válido y mayor que cero.";
                        return RedirectToAction("EngiRegisterFood");
                    }
                }

                // ==============================
                // ✅ GUARDAR DATOS
                // ==============================
                string msg;
                bool ok = _service.SaveFoods(names, days, files, out msg);

                if (ok)
                {
                    TempData["Success"] = "✅ Alimentos guardados correctamente.";
                    // 🔁 Redirigir al panel principal del Ingeniero
                    return RedirectToAction("EngineerPrincipalUser", "User");
                }
                else
                {
                    TempData["Error"] = msg;
                    return RedirectToAction("EngiRegisterFood");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error inesperado: " + ex.Message;
                return RedirectToAction("EngiRegisterFood");
            }
        }

        // ============================================================
        // 🟣 NUEVO: Vista de SUGERENCIAS DE CONSERVACIÓN
        // ============================================================
        [HttpGet]
        public ActionResult EngiSpeciFood()
        {
            try
            {
                // Traemos la lista de alimentos con su información actual
                List<FoodEngiDtos> foods = _service.GetAllFoodsWithSuggestions();

                return View(foods); // pasamos la lista a la vista
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los alimentos: " + ex.Message;
                return RedirectToAction("EngineerPrincipalUser", "User");
            }
        }

        // ============================================================
        // 🟢 POST: Guardar las sugerencias de conservación
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EngiSpeciFoodSave(FormCollection form)
        {
            try
            {
                var ids = form.GetValues("ID_Specif_Food");
                var suggestions = form.GetValues("sugges_Conser_Food");

                if (ids == null || suggestions == null || ids.Length != suggestions.Length)
                {
                    TempData["Error"] = "Error en los datos enviados. Intente nuevamente.";
                    return RedirectToAction("EngiSpeciFood");
                }

                // Validar que no haya campos vacíos
                for (int i = 0; i < suggestions.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(suggestions[i]))
                    {
                        TempData["Error"] = "Por favor, complete todas las sugerencias antes de guardar.";
                        return RedirectToAction("EngiSpeciFood");
                    }
                }

                string msg;
                bool ok = _service.SaveConservationSuggestions(ids, suggestions, out msg);

                if (ok)
                {
                    TempData["Success"] = "✅ Sugerencias guardadas correctamente.";
                    return RedirectToAction("EngineerPrincipalUser", "User");
                }
                else
                {
                    TempData["Error"] = msg;
                    return RedirectToAction("EngiSpeciFood");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error inesperado: " + ex.Message;
                return RedirectToAction("EngiSpeciFood");
            }
        }
        // =======================================================================
        // MÉTODO: Redirigir a vista principal del usuario (ClientPrincipalUser)
        // ========================================================================
        public ActionResult ClientPrincipalEngi()
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