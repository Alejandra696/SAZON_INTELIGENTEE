using System.Collections.Generic;
using System;
using System.Web;

namespace INTELIGENTE_SAZÓN.Dtos
{
    public class FoodItemDto
    {
        public int ID_Food { get; set; }
        public string name_Food { get; set; }
        public string imag_Food { get; set; }
    }

    public class RegisterProductItemDto
    {
        public int ID_Food { get; set; }
        public int amount_Product { get; set; }
        public int ID_Unit_Measure { get; set; }
        public int ID_Categ_Food { get; set; }
    }

    public class RegisterProductViewModel
    {
        public List<FoodItemDto> Foods { get; set; }
        public List<KeyValuePair<int, string>> Units { get; set; }
        public List<KeyValuePair<int, string>> Categories { get; set; }
    }

    public class InventoryItemDto
    {
        public string CategoryName { get; set; }
        public string FoodName { get; set; }
        public string ImageUrl { get; set; }
        public int Amount { get; set; }
        public string OptimalTime { get; set; }
        public string Suggestion { get; set; }
    }

    public class ViewInventoryViewModel
    {
        public Dictionary<string, List<InventoryItemDto>> ItemsByCategory { get; set; }
    }




    // Ingrediente de la receta para mostrar
    public class RecipeIngredienUsertDto
    {
        public int? ID_Food { get; set; }
        public string name_Food { get; set; }
        public decimal? amount_Ingred { get; set; } // según tu BD puede ser int o decimal
        public int? ID_Unit_Measure { get; set; }
        public string unitName { get; set; } // para mostrar la unidad
    }

    // Datos básicos de receta para mostrar
    public class RecipeRecommendationDto
    {
        public int ID_Recipe { get; set; }
        public string name_Recipe { get; set; }
        public string imag_Recipe { get; set; }
        public string instruc_Recipe { get; set; }

        // Lista de ingredientes de la receta (para mostrar)
        public List<RecipeIngredienUsertDto> Ingredients { get; set; } = new List<RecipeIngredienUsertDto>();

        // Para el ordenamiento/filtrado
        public int totalIngredients { get; set; }
        public int matchedIngredients { get; set; }

        // Si el usuario ya marcó como favorito
        public bool IsFavorite { get; set; }
    }
}