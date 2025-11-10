using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories;
using INTELIGENTE_SAZÓN.Repositories.Models;
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

            var items = _repo.GetInventoryItems(inventory.ID_Invent);

            var grouped = items
                .GroupBy(i => i.CategoryName)
                .ToDictionary(g => g.Key, g => g.ToList());

            return grouped;
        }

        public USER GetUserByEmail(string email)
        {
            using (var repo = new ClientRepository())
            {
                return repo.GetUserByEmail(email);
            }
        }
    }
}