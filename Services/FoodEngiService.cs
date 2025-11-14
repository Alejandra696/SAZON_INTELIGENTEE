using INTELIGENTE_SAZÓN.Repositories;
using System;
using System.IO;
using System.Web;
using System.Collections.Generic;
using INTELIGENTE_SAZÓN.Dtos;

namespace INTELIGENTE_SAZÓN.Services
{
    public class FoodEngiService
    {
        private readonly FoodEngiRepository _repo;

        public FoodEngiService()
        {
            _repo = new FoodEngiRepository();
        }

        // ============================================================
        // REGISTRA VARIOS ALIMENTOS (Tiempo Óptimo)
        // ============================================================
        public bool SaveFoods(string[] names, string[] timeDays, HttpFileCollectionBase files, out string message)
        {
            message = "";
            if (names == null || names.Length == 0)
            {
                message = "No se recibieron alimentos.";
                return false;
            }

            // Carpeta física y relativa
            var folderRelative = "/Content/food_images/";
            var folderPhysical = HttpContext.Current.Server.MapPath("~" + folderRelative);

            try
            {
                if (!Directory.Exists(folderPhysical))
                    Directory.CreateDirectory(folderPhysical);

                // Recorremos cada alimento
                for (int i = 0; i < names.Length; i++)
                {
                    var name = names[i]?.Trim();
                    var time = (timeDays != null && i < timeDays.Length) ? timeDays[i]?.Trim() : null;

                    if (string.IsNullOrEmpty(name))
                        continue; // omitimos entradas vacías

                    // Procesar archivo si existe en la misma posición
                    string savedRelativePath = null;
                    if (files != null && files.Count > 0)
                    {
                        HttpPostedFileBase file = null;
                        if (i < files.Count)
                            file = files[i];
                        else
                        {
                            for (int f = 0; f < files.Count; f++)
                            {
                                if (!string.IsNullOrEmpty(files[f].FileName))
                                {
                                    file = files[f];
                                    break;
                                }
                            }
                        }

                        if (file != null && file.ContentLength > 0)
                        {
                            var ext = Path.GetExtension(file.FileName)?.ToLower();
                            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                            if (Array.IndexOf(allowed, ext) < 0)
                            {
                                message += $"Archivo '{file.FileName}' no permitido. Solo imágenes.\n";
                                continue;
                            }

                            const int maxBytes = 5 * 1024 * 1024; // 5 MB
                            if (file.ContentLength > maxBytes)
                            {
                                message += $"Archivo '{file.FileName}' excede 5 MB.\n";
                                continue;
                            }

                            var uniqueName = Guid.NewGuid().ToString("N") + ext;
                            var savePath = Path.Combine(folderPhysical, uniqueName);
                            file.SaveAs(savePath);
                            savedRelativePath = folderRelative + uniqueName;
                        }
                    }

                    // Guardar en base de datos
                    var ok = _repo.InsertFoodWithSpecification(name, savedRelativePath, time);
                    if (!ok)
                    {
                        message += $"No se pudo guardar el alimento '{name}'.\n";
                    }
                }

                if (string.IsNullOrEmpty(message))
                {
                    message = "Guardado correctamente.";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = "Error inesperado: " + ex.Message;
                return false;
            }
        }

        // ============================================================
        // 🟣 NUEVO: Obtener alimentos con sus sugerencias
        // ============================================================
        public List<FoodEngiDtos> GetAllFoodsWithSuggestions()
        {
            try
            {
                return _repo.GetAllFoodsWithSuggestions();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los alimentos: " + ex.Message);
            }
        }

        // ============================================================
        // 🟢 NUEVO: Guardar sugerencias de conservación
        // ============================================================
        public bool SaveConservationSuggestions(string[] ids, string[] suggestions, out string message)
        {
            message = "";
            try
            {
                if (ids == null || suggestions == null || ids.Length != suggestions.Length)
                {
                    message = "Datos inválidos.";
                    return false;
                }

                for (int i = 0; i < ids.Length; i++)
                {
                    if (int.TryParse(ids[i], out int idSpec))
                    {
                        string sugg = suggestions[i]?.Trim();
                        if (!string.IsNullOrEmpty(sugg))
                        {
                            bool ok = _repo.UpdateConservationSuggestion(idSpec, sugg);
                            if (!ok)
                            {
                                message += $"No se pudo actualizar el registro con ID {idSpec}.\n";
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(message))
                {
                    message = "Sugerencias guardadas correctamente.";
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                message = "Error al guardar sugerencias: " + ex.Message;
                return false;
            }
        }
    }
}