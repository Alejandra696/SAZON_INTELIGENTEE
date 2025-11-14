using System.Collections.Generic;
using System.Web;

namespace INTELIGENTE_SAZÓN.Dtos
{
    public class RecipeIngredientDto
    {
        public int Amount { get; set; }       // cantidad
        public int UnitId { get; set; }       // ID_Unit_Measure
        public int FoodId { get; set; }       // ID_Food
    }

    public class RecipeChefDto
    {
        public string name_Recipe { get; set; }
        public string instrucc_Recipe { get; set; }
        public HttpPostedFileBase imag_File { get; set; } // Imagen de la receta
        public List<RecipeIngredientDto> Ingredients { get; set; } = new List<RecipeIngredientDto>();
    }
}