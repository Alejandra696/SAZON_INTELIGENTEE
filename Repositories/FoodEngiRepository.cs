using INTELIGENTE_SAZÓN.Repositories.Models; // namespace generado por EF (ajusta si tu namespace difiere)
using INTELIGENTE_SAZÓN.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace INTELIGENTE_SAZÓN.Repositories
{
    public class FoodEngiRepository
    {
        // ============================================================
        // INSERTAR ALIMENTO Y SU ESPECIFICACIÓN
        // ============================================================
        public bool InsertFoodWithSpecification(string nameFood, string imageRelativePath, string timeOpetDays)
        {
            try
            {
                using (var db = new Sazon_inteligenteDBEntities1()) // ajusta el nombre del contexto si es otro
                {
                    var food = new FOOD
                    {
                        name_Food = nameFood,
                        imag_Food = imageRelativePath
                    };
                    db.FOODs.Add(food);
                    db.SaveChanges();

                    var spec = new SPECIFICATION_FOOD
                    {
                        time_Opet_Days = timeOpetDays,
                        delete_Food = false,
                        sugges_Conser_Food = null,
                        ID_Food = food.ID_Food
                    };

                    db.SPECIFICATION_FOOD.Add(spec);
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
        // 🟣 NUEVO: Obtener todos los alimentos con su especificación
        // ============================================================
        public List<FoodEngiDtos> GetAllFoodsWithSuggestions()
        {
            try
            {
                using (var db = new Sazon_inteligenteDBEntities1())
                {
                    var data = (from f in db.FOODs
                                join s in db.SPECIFICATION_FOOD on f.ID_Food equals s.ID_Food
                                where s.delete_Food == false
                                select new FoodEngiDtos
                                {
                                    ID_Specif_Food = s.ID_Specif_Food,
                                    name_Food = f.name_Food,
                                    time_Opet_Days = s.time_Opet_Days,
                                    sugges_Conser_Food = s.sugges_Conser_Food,
                                    imag_Food = f.imag_Food
                                }).ToList();

                    return data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los alimentos: " + ex.Message);
            }
        }

        // ============================================================
        // 🟢 NUEVO: Actualizar sugerencia de conservación
        // ============================================================
        public bool UpdateConservationSuggestion(int idSpecifFood, string suggestion)
        {
            try
            {
                using (var db = new Sazon_inteligenteDBEntities1())
                {
                    var spec = db.SPECIFICATION_FOOD.FirstOrDefault(s => s.ID_Specif_Food == idSpecifFood);
                    if (spec == null)
                        return false;

                    spec.sugges_Conser_Food = suggestion;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}