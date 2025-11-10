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
}