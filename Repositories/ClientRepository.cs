using System;
using System.Collections.Generic;
using System.Linq;
using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories.Models;

namespace INTELIGENTE_SAZÓN.Repositories
{
    public class ClientRepository : IDisposable
    {
        private readonly Sazon_inteligenteDBEntities1 _ctx;

        public ClientRepository()
        {
            _ctx = new Sazon_inteligenteDBEntities1();
        }

        public List<FoodItemDto> GetAllFoodsSimple()
        {
            return _ctx.FOODs
                .Select(f => new FoodItemDto
                {
                    ID_Food = f.ID_Food,
                    name_Food = f.name_Food,
                    imag_Food = f.imag_Food
                }).ToList();
        }

        public List<KeyValuePair<int, string>> GetUnits()
        {
            return _ctx.UNIT_MEASURE
                       .ToList()  // <-- importante, carga primero desde SQL
                       .Select(u => new KeyValuePair<int, string>(u.ID_Unit_Measure, u.type_Unit_Measure))
                       .ToList();
        }

        public List<KeyValuePair<int, string>> GetCategories()
        {
            return _ctx.CATEGORY_FOOD
                       .ToList() // <-- igual
                       .Select(c => new KeyValuePair<int, string>(c.ID_Categ_Food, c.name_Categ_Food))
                       .ToList();
        }
        public REGISTER_FOOD GetRegisterFoodByFoodAndInventory(int idFood, int idInvent)
        {
            return _ctx.REGISTER_FOOD
                .FirstOrDefault(r => r.ID_Food == idFood && r.ID_Invent == idInvent && (r.delete_Food == null || r.delete_Food == false));
        }

        public void AddRegisterFood(REGISTER_FOOD entity)
        {
            _ctx.REGISTER_FOOD.Add(entity);
        }

        public void SaveChanges()
        {
            _ctx.SaveChanges();
        }

        public INVENTORY GetOrCreateInventoryForUser(int idUser)
        {
            var invent = _ctx.INVENTORies.FirstOrDefault(i => i.ID_User == idUser);

            if (invent != null) return invent;

            invent = new INVENTORY
            {
                date_Reg_Invent = DateTime.Now,
                ID_User = idUser
            };

            _ctx.INVENTORies.Add(invent);
            _ctx.SaveChanges();

            return invent;
        }

        /* =========================================================================
           GUARDAR CAMBIOS (si en algún método no se guardó)
           ========================================================================= */
        public USER GetUserByEmail(string email)
        {
            return _ctx.USERs.FirstOrDefault(u => u.email_User == email);
        }

        public void Dispose()
        {
            _ctx.Dispose();
        }

        public List<InventoryItemDto> GetInventoryItems(int idInvent)
        {
            var query = from r in _ctx.REGISTER_FOOD
                        join f in _ctx.FOODs on r.ID_Food equals f.ID_Food
                        join c in _ctx.CATEGORY_FOOD on r.ID_Categ_Food equals c.ID_Categ_Food
                        join s in _ctx.SPECIFICATION_FOOD on r.ID_Food equals s.ID_Food into specs
                        from s in specs.DefaultIfEmpty()
                        where r.ID_Invent == idInvent && (r.delete_Food == null || r.delete_Food == false)
                        select new InventoryItemDto
                        {
                            CategoryName = c.name_Categ_Food,
                            FoodName = f.name_Food,
                            ImageUrl = f.imag_Food,
                            Amount = r.amount_Product ?? 0,
                            OptimalTime = s.time_Opet_Days,
                            Suggestion = s.sugges_Conser_Food
                        };

            return query.ToList();
        }
        /// <summary>
        /// Trae todas las recetas (no eliminadas) con sus ingredientes (JOIN a RECIPE_INGREDIENTS y FOOD y UNIT_MEASURE)
        /// </summary>
        public IEnumerable<dynamic> GetAllRecipesWithIngredientsRaw()
        {
            // devolvemos datos anónimos que el Service mapea
            var q = from r in _ctx.RECIPEs
                    where r.delete_Recipe == null || r.delete_Recipe == false
                    select new
                    {
                        Recipe = r,
                        Ingredients = r.RECIPE_INGREDIENTS.ToList()
                    };

            return q.ToList();
        }

        /// <summary>
        /// Trae la lista de ingredientes por receta (con nombres de food y unidad)
        /// </summary>
        public List<RecipeIngredienUsertDto> GetIngredientsForRecipe(int idRecipe)
        {
            var list = (from ri in _ctx.RECIPE_INGREDIENTS
                        join f in _ctx.FOODs on ri.ID_Food equals f.ID_Food
                        join u in _ctx.UNIT_MEASURE on ri.ID_Unit_Measure equals u.ID_Unit_Measure
                        where ri.ID_Recipe == idRecipe
                        select new RecipeIngredienUsertDto
                        {
                            ID_Food = ri.ID_Food,
                            name_Food = f.name_Food,
                            amount_Ingred = ri.amount_Ingred,
                            ID_Unit_Measure = ri.ID_Unit_Measure,
                            unitName = u.type_Unit_Measure
                        }).ToList();
            return list;
        }

        /// <summary>
        /// Obtiene lista de REGISTER_FOOD (inventario) del inventario del usuario
        /// </summary>
        public List<REGISTER_FOOD> GetRegisterFoodByInventId(int idInvent)
        {
            return _ctx.REGISTER_FOOD
                       .Where(r => r.ID_Invent == idInvent && (r.delete_Food == null || r.delete_Food == false))
                       .ToList();
        }

        /// <summary>
        /// Obtiene el inventario del usuario (primero que encuentre)
        /// </summary>
        public INVENTORY GetInventoryByUserId(int idUser)
        {
            return _ctx.INVENTORies.FirstOrDefault(i => i.ID_User == idUser);
        }


        /// <summary>
        /// Comprueba si una receta está en favoritos de un usuario
        /// </summary>
        public bool IsFavorite(int idUser, int idRecipe)
        {
            return _ctx.FAVORITE_RECIPE.Any(f => f.ID_User == idUser && f.ID_Recipe == idRecipe);
        }

        /// <summary>
        /// Añade favorito
        /// </summary>
        public void AddFavorite(FAVORITE_RECIPE fav)
        {
            _ctx.FAVORITE_RECIPE.Add(fav);
            _ctx.SaveChanges();
        }

        /// <summary>
        /// Remueve favorito por user + recipe
        /// </summary>
        public void RemoveFavorite(int idUser, int idRecipe)
        {
            var f = _ctx.FAVORITE_RECIPE.FirstOrDefault(x => x.ID_User == idUser && x.ID_Recipe == idRecipe);
            if (f != null)
            {
                _ctx.FAVORITE_RECIPE.Remove(f);
                _ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Trae todas las recetas (modelo EF) no marcadas como delete
        /// </summary>
        public List<RECIPE> GetAllRecipes()
        {
            return _ctx.RECIPEs.Where(r => r.delete_Recipe == null || r.delete_Recipe == false).ToList();
        }
        // ======================================================
        // MÉTODO: Obtener las recetas favoritas de un usuario
        // ======================================================
        public List<RecipeRecommendationDto> GetFavoriteRecipesByUser(int userId)
        {
            var query = from fav in _ctx.FAVORITE_RECIPE
                        join recipe in _ctx.RECIPEs on fav.ID_Recipe equals recipe.ID_Recipe
                        where fav.ID_User == userId
                        select new RecipeRecommendationDto
                        {
                            ID_Recipe = recipe.ID_Recipe,
                            name_Recipe = recipe.name_Recipe,
                            imag_Recipe = recipe.imag_Recipe,
                            instruc_Recipe = recipe.instrucc_Recipe,
                            Ingredients = (from ing in _ctx.RECIPE_INGREDIENTS
                                           join f in _ctx.FOODs on ing.ID_Food equals f.ID_Food
                                           where ing.ID_Recipe == recipe.ID_Recipe
                                           select new RecipeIngredienUsertDto
                                           {
                                               ID_Food = ing.ID_Food,
                                               name_Food = f.name_Food,
                                               amount_Ingred = ing.amount_Ingred
                                           }).ToList(),
                            IsFavorite = true
                        };

            return query.ToList();
        }

        
    }

}
