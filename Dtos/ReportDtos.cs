using System;

namespace INTELIGENTE_SAZÓN.Dtos
{
    /// <summary>
    /// Representa la unión de datos entre las tablas USER y PROFESSIONAL_PROFILE
    /// para efectos de reportes consolidados.
    /// </summary>
    public class UserCombinedDtos
    {
        /// <summary>
        /// Identificador del usuario o perfil profesional.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del usuario o profesional.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Correo electrónico.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Fecha de registro en la plataforma.
        /// </summary>
        public DateTime? DateRegisUser { get; set; }

        /// <summary>
        /// Última fecha de inicio de sesión.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Identificador del rol asociado.
        /// </summary>
        public int? RoleId { get; set; }

        /// <summary>
        /// Nombre del rol (Usuario, Chef, Ingeniero de Alimentos, Administrador, etc.).
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Indica si el usuario o perfil se encuentra activo.
        /// </summary>
        public bool IsActive { get; set; }

        /*saber si pertenece a un usuario normal o a un profesional*/
        public string SourceTable { get; set; }

        public int RowNumber { get; set; }
    }

    // ============================================================
    // Dto del Dashboard
    // ============================================================
    public class DashboardReportDtos
    {
        public int TotalUsers { get; set; }
        public int NewUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }

        // Este campo opcional indica cuándo se generó el reporte
        public DateTime ReportDate { get; set; } = DateTime.Now;
    }

}