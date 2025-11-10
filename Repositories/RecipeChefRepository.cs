using INTELIGENTE_SAZÓN.Repositories.Models; // EF context namespace
using System;
using System.Collections.Generic;
using System.Linq;

namespace INTELIGENTE_SAZÓN.Repositories
{
    public class RecipeChefRepository
    {
        // ============================================================
        // Inserta una receta y sus ingredientes
        // ============================================================
        public bool InsertRecipeWithIngredients(
            string nameRecipe,
            string imagePath,
            string instructions,
            List<INTELIGENTE_SAZÓN.Dtos.RecipeIngredientDto> ingredients)
        {
            try
            {
                using (var db = new Sazon_inteligenteDBEntities1())
                {
                    // 1️⃣ Crear la receta
                    var recipe = new RECIPE
                    {
                        name_Recipe = nameRecipe,
                        imag_Recipe = imagePath,
                        instrucc_Recipe = instructions,
                        delete_Recipe = false
                    };

                    db.RECIPEs.Add(recipe);
                    db.SaveChanges(); // Genera ID_Recipe

                    // 2️⃣ Crear los ingredientes asociados
                    foreach (var ingDto in ingredients)
                    {
                        var ing = new RECIPE_INGREDIENTS
                        {
                            amount_Ingred = ingDto.Amount,
                            ID_Unit_Measure = ingDto.UnitId,
                            ID_Food = ingDto.FoodId,
                            ID_Recipe = recipe.ID_Recipe
                        };
                        db.RECIPE_INGREDIENTS.Add(ing);
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ============================================================
        // Obtiene todas las unidades de medida
        // ============================================================
        public List<UNIT_MEASURE> GetAllUnitMeasures()
        {
            using (var db = new Sazon_inteligenteDBEntities1())
            {
                return db.UNIT_MEASURE.ToList();
            }
        }

        // ============================================================
        // Obtiene todos los alimentos disponibles
        // ============================================================
        public List<FOOD> GetAllFoods()
        {
            using (var db = new Sazon_inteligenteDBEntities1())
            {
                return db.FOODs.ToList();
            }
        }
    }
}