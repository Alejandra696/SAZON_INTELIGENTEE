using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories;
using INTELIGENTE_SAZÓN.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace INTELIGENTE_SAZÓN.Services
{
    public class ClientService
    {
        private readonly ClientRepository _repo;

        public ClientService()
        {
            _repo = new ClientRepository();
        }

        public RegisterProductViewModel LoadRegisterProductData()
        {
            return new RegisterProductViewModel
            {
                Foods = _repo.GetAllFoodsSimple(),
                Units = _repo.GetUnits(),
                Categories = _repo.GetCategories()
            };
        }

        public bool SaveProducts(string email, List<RegisterProductItemDto> items)
        {
            var user = _repo.GetUserByEmail(email);
            if (user == null) return false;

            var invent = _repo.GetOrCreateInventoryForUser(user.ID_User);

            foreach (var item in items)
            {
                if (item.amount_Product <= 0)
                    continue;

                var existing = _repo.GetRegisterFoodByFoodAndInventory(item.ID_Food, invent.ID_Invent);

                if (existing != null)
                {
                    // actualizar
                    existing.amount_Product = item.amount_Product;
                    existing.ID_Unit_Measure = item.ID_Unit_Measure;
                    existing.ID_Categ_Food = item.ID_Categ_Food;
                }
                else
                {
                    // crear nuevo
                    var newReg = new REGISTER_FOOD
                    {
                        amount_Product = item.amount_Product,
                        date_Regis_Food = System.DateTime.Now,
                        ID_Food = item.ID_Food,
                        ID_Unit_Measure = item.ID_Unit_Measure,
                        ID_Categ_Food = item.ID_Categ_Food,
                        ID_Invent = invent.ID_Invent,
                        ID_User = user.ID_User
                    };

                    _repo.AddRegisterFood(newReg);
                }
            }

            _repo.SaveChanges();
            return true;
        }

        public ViewInventoryViewModel LoadInventory(string email)
        {
            using (var repo = new ClientRepository())
            {
                var user = repo.GetUserByEmail(email);
                var inventory = repo.GetOrCreateInventoryForUser(user.ID_User);

                var items = repo.GetInventoryItems(inventory.ID_Invent);

                var grouped = items
                    .GroupBy(i => i.CategoryName)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return new ViewInventoryViewModel
                {
                    ItemsByCategory = grouped
                };
            }
        }

        public Dictionary<string, List<InventoryItemDto>> GetInventoryGrouped(string email)
        {
            var user = _repo.GetUserByEmail(email);
            var inventory = _repo.GetOrCreateInventoryForUser(user.ID_User);

            // 🟣 Obtener los alimentos registrados
            var items = _repo.GetInventoryItems(inventory.ID_Invent);

            // 🟣 Agrupar los alimentos por categoría
            var grouped = items
                .GroupBy(i => i.CategoryName)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 🟣 Obtener todas las categorías del sistema
            var allCategories = _repo.GetCategories()
                .Select(c => c.Value)
                .ToList();

            // 🟣 Asegurar que todas las categorías existan en el diccionario,
            // incluso las que están vacías
            foreach (var category in allCategories)
            {
                if (!grouped.ContainsKey(category))
                {
                    grouped[category] = new List<InventoryItemDto>();
                }
            }

            return grouped;
        }



        /// <summary>
        /// Obtiene la lista de recetas recomendadas para el usuario identificado por email.
        /// Ordena de mayor a menor por número de ingredientes coincididos (primero las que tienen todos).
        /// Devuelve solo recetas donde matchedIngredients > 0 (tiene al menos 1 ingrediente).
        /// </summary>
        public List<RecipeRecommendationDto> GetRecipeRecommendationsByEmail(string email)
        {
            var user = _repo.GetUserByEmail(email);
            if (user == null) return new List<RecipeRecommendationDto>();

            // Obtener inventario del usuario (puede ser null si no tiene inventario)
            var inv = _repo.GetInventoryByUserId(user.ID_User);
            List<REGISTER_FOOD> regFoods = new List<REGISTER_FOOD>();
            if (inv != null)
                regFoods = _repo.GetRegisterFoodByInventId(inv.ID_Invent);

            // Diccionario por ID_Food -> cantidad disponible
            var invDict = regFoods
                .GroupBy(r => r.ID_Food)
                .ToDictionary(g => g.Key, g => g.Sum(x => (decimal?)x.amount_Product) ?? 0m);

            var recipes = _repo.GetAllRecipes();

            var result = new List<RecipeRecommendationDto>();

            foreach (var r in recipes)
            {
                var ingList = _repo.GetIngredientsForRecipe(r.ID_Recipe) ?? new List<RecipeIngredienUsertDto>();
                int total = ingList.Count;
                int matched = 0;

                foreach (var ing in ingList)
                {
                    if (invDict.TryGetValue(ing.ID_Food, out decimal available))
                    {
                        // Consideramos coincidencia si tiene al menos la cantidad requerida (o >0 si no se especifica cantidad exacta).
                        // Si tu sistema usa cantidades enteras, ajusta el comparador.
                        if (available >= ing.amount_Ingred)
                            matched++;
                    }
                }

                if (matched > 0) // solo mostrar recetas donde tenga al menos un ingrediente
                {
                    result.Add(new RecipeRecommendationDto
                    {
                        ID_Recipe = r.ID_Recipe,
                        name_Recipe = r.name_Recipe,
                        imag_Recipe = r.imag_Recipe,
                        instruc_Recipe = r.instrucc_Recipe,
                        Ingredients = ingList,
                        totalIngredients = total,
                        matchedIngredients = matched,
                        IsFavorite = _repo.IsFavorite(user.ID_User, r.ID_Recipe)
                    });
                }
            }

            // Ordenamiento: primero las recetas donde matched == total (tiene todos), luego por ratio descendente matched/total
            var ordered = result
                .OrderByDescending(x => x.matchedIngredients == x.totalIngredients) // primero true
                .ThenByDescending(x => (double)x.matchedIngredients / (x.totalIngredients == 0 ? 1 : x.totalIngredients))
                .ThenByDescending(x => x.matchedIngredients)
                .ToList();

            return ordered;
        }

        /// <summary>
        /// Toggle favorito: si existe lo quita, si no existe lo crea.
        /// Devuelve el nuevo estado (true=ahora es favorito)
        /// </summary>
        public bool ToggleFavoriteByEmail(string email, int idRecipe)
        {
            var user = _repo.GetUserByEmail(email);
            if (user == null) throw new Exception("Usuario no encontrado");

            bool exists = _repo.IsFavorite(user.ID_User, idRecipe);
            if (exists)
            {
                _repo.RemoveFavorite(user.ID_User, idRecipe);
                return false;
            }
            else
            {
                var fav = new FAVORITE_RECIPE
                {
                    ID_Recipe = idRecipe,
                    ID_User = user.ID_User
                };
                _repo.AddFavorite(fav);
                return true;
            }
        }

        // ======================================================
        // MÉTODO: Obtener las recetas favoritas del usuario activo
        // ======================================================
        public List<RecipeRecommendationDto> GetFavoriteRecipesByEmail(string email)
        {
            var user = _repo.GetUserByEmail(email);
            if (user == null) throw new Exception("Usuario no encontrado o sesión expirada.");

            var favorites = _repo.GetFavoriteRecipesByUser(user.ID_User);
            return favorites ?? new List<RecipeRecommendationDto>();
        }
    }

}
