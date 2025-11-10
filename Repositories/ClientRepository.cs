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
    }
}