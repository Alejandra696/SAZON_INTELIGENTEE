using System;
using System.Web;

namespace INTELIGENTE_SAZÓN.Dtos
{
    // DTO para un alimento individual (útil para pasar datos internamente)
    public class FoodEngiDtos
    {
        // --- Identificadores ---
        public int? ID_Food { get; set; }            // PK de la tabla Food (nullable para nuevos registros)
        public int? ID_Specif_Food { get; set; }     // PK de la tabla SPECIFICATION_FOOD (nullable si no existe)

        // --- Datos visibles en la interfaz ---
        public string name_Food { get; set; }        // nombre mostrado
        public string time_Opet_Days { get; set; }   // tiempo sugerido en días (string para aceptar formatos libres)

        // --- Imagen ---
        public HttpPostedFileBase imag_File { get; set; } // usado para binding cuando el usuario sube una imagen
        public string imag_Food { get; set; }        // ruta (o base64) de la imagen guardada para mostrar en la vista

        // --- Especificación / Sugerencias ---
        public string sugges_Conser_Food { get; set; } // texto de sugerencia de conservación (tabla SPECIFICATION_FOOD)

        // --- Control lógico ---
        public bool delete_Food { get; set; } = false; // flag lógico (soft-delete) si lo manejas en SPECIFICATION_FOOD

        // --- Auxiliares (opcional) ---
        public DateTime? CreatedAt { get; set; }     // opcional, si quieres pasar fecha de creación
    }
}