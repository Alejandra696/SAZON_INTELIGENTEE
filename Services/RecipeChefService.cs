using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories;
using System;
using System.IO;
using System.Web;

namespace INTELIGENTE_SAZÓN.Services
{
    public class RecipeChefService
    {
        private readonly RecipeChefRepository _repo;

        public RecipeChefService()
        {
            _repo = new RecipeChefRepository();
        }

        // ============================================================
        // Guarda receta con imagen e ingredientes
        // ============================================================
        public bool SaveRecipe(RecipeChefDto dto, out string message)
        {
            message = "";

            if (dto == null)
            {
                message = "❌ No se recibió la información de la receta.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.name_Recipe))
            {
                message = "⚠️ El nombre de la receta es obligatorio.";
                return false;
            }

            if (dto.imag_File == null || dto.imag_File.ContentLength == 0)
            {
                message = "⚠️ Debe subir una imagen para la receta.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.instrucc_Recipe))
            {
                message = "⚠️ Las instrucciones son obligatorias.";
                return false;
            }

            if (dto.Ingredients == null || dto.Ingredients.Count == 0)
            {
                message = "⚠️ Debe agregar al menos un ingrediente.";
                return false;
            }

            try
            {
                // === Guardar la imagen ===
                string relativeFolder = "/Content/recipe_images/";
                string absoluteFolder = HttpContext.Current.Server.MapPath("~" + relativeFolder);

                if (!Directory.Exists(absoluteFolder))
                    Directory.CreateDirectory(absoluteFolder);

                string ext = Path.GetExtension(dto.imag_File.FileName);
                string fileName = Guid.NewGuid().ToString("N") + ext;
                string fullPath = Path.Combine(absoluteFolder, fileName);

                dto.imag_File.SaveAs(fullPath);
                string savedPath = relativeFolder + fileName;

                // === Guardar en BD ===
                bool ok = _repo.InsertRecipeWithIngredients(
                    dto.name_Recipe.Trim(),
                    savedPath,
                    dto.instrucc_Recipe.Trim(),
                    dto.Ingredients
                );

                if (!ok)
                {
                    message = "❌ Error al guardar la receta en la base de datos.";
                    return false;
                }

                message = "✅ Receta guardada correctamente.";
                return true;
            }
            catch (Exception ex)
            {
                message = "Error inesperado: " + ex.Message;
                return false;
            }
        }

        // ============================================================
        // Cargar listas auxiliares
        // ============================================================
        public System.Collections.Generic.List<Repositories.Models.UNIT_MEASURE> GetUnits()
        {
            return _repo.GetAllUnitMeasures();
        }

        public System.Collections.Generic.List<Repositories.Models.FOOD> GetFoods()
        {
            return _repo.GetAllFoods();
        }
    }
}