using INTELIGENTE_SAZÓN.Dtos;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace INTELIGENTE_SAZÓN.Utilities
{
    public static class ReportPdfGenerator
    {
        //============================================================
        // REPORTE: USUARIOS NUEVOS (ÚLTIMO MES)
        // ============================================================
        public static byte[] GenerateNewUsersPdf(List<UserCombinedDtos> usuarios, string usuario)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    float width = 756 * 0.75f;
                    float height = 1066 * 0.75f;
                    var pageSize = new Rectangle(width, height);
                    Document doc = new Document(pageSize, 0, 0, 0, 0);
                    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // ===== FONDO =====
                    string fondoPath = HttpContext.Current.Server.MapPath("~/Content/Images/BackgroundReports.png");
                    iTextSharp.text.Image fondoImg = null;
                    if (File.Exists(fondoPath))
                    {
                        fondoImg = iTextSharp.text.Image.GetInstance(fondoPath);
                        fondoImg.SetAbsolutePosition(0, 0);
                        fondoImg.ScaleAbsolute(width, height);
                    }

                    // ===== FUENTES =====
                    BaseFont fontTitulo = BaseFont.CreateFont(BaseFont.TIMES_BOLDITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontSub = BaseFont.CreateFont(BaseFont.TIMES_ITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontTabla = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                    // ===== MÁRGENES Y MEDIDAS =====
                    float marginLeft = 45;
                    float marginRight = 45;
                    float tablaX = marginLeft;
                    float tablaAncho = width - marginLeft - marginRight;
                    float filaAltura = 36f;

                    float topSpace = 150f;
                    float bottomSpace = 90f;

                    float tablaAvailableHeight = height - topSpace - bottomSpace;

                    // ===== COLUMNAS (MISMA DISTRIBUCIÓN QUE RETIRADOS) =====
                    float col1 = tablaX;         // ID
                    float col2 = tablaX + 55;    // Nombre
                    float col3 = tablaX + 190;   // Correo
                    float col4 = tablaX + 370;   // Fecha Registro
                    float col5 = tablaX + tablaAncho;

                    float[] columnas = { col1, col2, col3, col4, col5 };

                    int filasPorPagina = Math.Max(1, (int)Math.Floor(tablaAvailableHeight / filaAltura) - 1);

                    // ============================================================
                    // ENCABEZADO
                    // ============================================================
                    void DrawHeader(PdfContentByte cb, int total)
                    {
                        if (fondoImg != null)
                            cb.AddImage(fondoImg);

                        // TÍTULO
                        cb.BeginText();
                        cb.SetFontAndSize(fontTitulo, 50);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "USUARIOS", 50, height - 75, 0);

                        cb.SetFontAndSize(fontSub, 33);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "NUEVOS", 230, height - 110, 0);
                        cb.EndText();

                        // CUADRO TOTAL
                        float cuadroX = width - 170;
                        float cuadroY = height - 75 - 28;
                        float cuadroAncho = 120;
                        float cuadroAlto = 40;

                        cb.SetLineWidth(1.5f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(cuadroX, cuadroY - cuadroAlto, cuadroAncho, cuadroAlto);
                        cb.Stroke();

                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 24);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, total.ToString(), cuadroX + cuadroAncho / 2, cuadroY - 25, 0);
                        cb.EndText();

                        // TABLA
                        float totalFilasVisual = filasPorPagina + 1;
                        float tablaAltura = totalFilasVisual * filaAltura;
                        float tablaY = height - topSpace;

                        cb.SetLineWidth(1.3f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(tablaX, tablaY - tablaAltura, tablaAncho, tablaAltura);
                        cb.Stroke();

                        foreach (float col in columnas)
                        {
                            cb.MoveTo(col, tablaY);
                            cb.LineTo(col, tablaY - tablaAltura);
                            cb.Stroke();
                        }

                        for (int i = 0; i <= filasPorPagina + 1; i++)
                        {
                            float y = tablaY - (i * filaAltura);
                            cb.MoveTo(tablaX, y);
                            cb.LineTo(tablaX + tablaAncho, y);
                            cb.Stroke();
                        }

                        // ENCABEZADOS DE COLUMNA
                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 10);
                        float center = tablaY - (filaAltura / 2) + 5;

                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ID", (col1 + col2) / 2, center + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "NOMBRE", (col2 + col3) / 2, center + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "CORREO", (col3 + col4) / 2, center + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "FECHA DE REGISTRO", (col4 + col5) / 2, center + 3, 0);

                        cb.EndText();
                    }

                    // ============================================================
                    // FOOTER
                    // ============================================================
                    void DrawFooter(PdfContentByte cb)
                    {
                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 13);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy}", 55, 65, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Hora del Reporte: {DateTime.Now:HH:mm:ss}", 55, 50, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Usuario: Administrador del Sistema.", 55, 35, 0);
                        cb.EndText();
                    }

                    // ============================================================
                    // GENERACIÓN DE PÁGINAS
                    // ============================================================
                    int totalUsuarios = usuarios?.Count ?? 0;
                    int pagina = 0;
                    int index = 0;

                    while (index < totalUsuarios || (totalUsuarios == 0 && pagina == 0))
                    {
                        if (pagina > 0) doc.NewPage();
                        pagina++;

                        var cb = writer.DirectContent;
                        DrawHeader(cb, totalUsuarios);

                        float tablaTopY = height - topSpace;
                        float yStartForRows = tablaTopY - filaAltura;

                        cb.BeginText();
                        cb.SetFontAndSize(fontTabla, 10);

                        for (int r = 0; r < filasPorPagina && index < totalUsuarios; r++, index++)
                        {
                            var u = usuarios[index];
                            float y = yStartForRows - (r * filaAltura);
                            float centerY = y - (filaAltura / 2) + 5;

                            cb.ShowTextAligned(Element.ALIGN_CENTER, (index + 1).ToString(), (col1 + col2) / 2, centerY, 0);
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.Name ?? "-"), col2 + 5, centerY, 0);
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.Email ?? "-"), col3 + 5, centerY, 0);
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.DateRegisUser?.ToString("yyyy-MM-dd") ?? "-"), (col4 + col5) / 2, centerY, 0);
                        }

                        cb.EndText();
                        DrawFooter(cb);
                    }

                    doc.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GenerateNewUsersPdf: " + ex.ToString());
                return new byte[0];
            }
        }


        //============================================================
        // REPORTE: USUARIOS REGISTRADOS (TODOS)
        // ============================================================
        public static byte[] GenerateRegisteredUsersPdf(List<UserCombinedDtos> usuarios, string usuario)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    float width = 756 * 0.75f;
                    float height = 1066 * 0.75f;
                    var pageSize = new Rectangle(width, height);
                    Document doc = new Document(pageSize, 0, 0, 0, 0);
                    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // Fondo
                    string fondoPath = HttpContext.Current.Server.MapPath("~/Content/Images/BackgroundReports.png");
                    iTextSharp.text.Image fondoImg = null;
                    if (File.Exists(fondoPath))
                    {
                        fondoImg = iTextSharp.text.Image.GetInstance(fondoPath);
                        fondoImg.SetAbsolutePosition(0, 0);
                        fondoImg.ScaleAbsolute(width, height);
                    }

                    // Fuentes
                    BaseFont fontTitulo = BaseFont.CreateFont(BaseFont.TIMES_BOLDITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontSub = BaseFont.CreateFont(BaseFont.TIMES_ITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontTabla = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                    // Márgenes y medidas
                    float marginLeft = 45;
                    float marginRight = 45;
                    float tablaX = marginLeft;
                    float tablaAncho = width - marginLeft - marginRight;
                    float filaAltura = 36f;
                    float topSpace = 150f;
                    float bottomSpace = 90f;
                    float tablaAvailableHeight = height - topSpace - bottomSpace;

                    // Columnas: ID | ROL | CORREO | FECHA
                    float col1 = tablaX;
                    float col2 = tablaX + 40;
                    float col3 = tablaX + 190;
                    float col4 = tablaX + 370;
                    float col5 = tablaX + tablaAncho;
                    float[] columnas = { col1, col2, col3, col4, col5 };

                    int filasPorPagina = Math.Max(1, (int)Math.Floor(tablaAvailableHeight / filaAltura) - 1);

                    void DrawPageHeader(PdfContentByte cb, int total)
                    {
                        if (fondoImg != null)
                            cb.AddImage(fondoImg);

                        cb.BeginText();
                        cb.SetFontAndSize(fontTitulo, 50);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "USUARIOS", 50, height - 75, 0);
                        cb.SetFontAndSize(fontSub, 33);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "REGISTRADOS", 151, height - 110, 0);
                        cb.EndText();

                        float cuadroX = width - 170;
                        float cuadroY = height - 75 - 28;
                        float cuadroAncho = 120;
                        float cuadroAlto = 40;
                        cb.SetLineWidth(1.5f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(cuadroX, cuadroY - cuadroAlto, cuadroAncho, cuadroAlto);
                        cb.Stroke();

                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 24);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, total.ToString(), cuadroX + cuadroAncho / 2, cuadroY - 25, 0);
                        cb.EndText();

                        float totalFilasVisual = filasPorPagina + 1;
                        float tablaAltura = totalFilasVisual * filaAltura;
                        float tablaY = height - topSpace;

                        cb.SetLineWidth(1.3f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(tablaX, tablaY - tablaAltura, tablaAncho, tablaAltura);
                        cb.Stroke();

                        foreach (float col in columnas)
                        {
                            cb.MoveTo(col, tablaY);
                            cb.LineTo(col, tablaY - tablaAltura);
                            cb.Stroke();
                        }

                        for (int i = 0; i <= filasPorPagina + 1; i++)
                        {
                            float y = tablaY - (i * filaAltura);
                            cb.MoveTo(tablaX, y);
                            cb.LineTo(tablaX + tablaAncho, y);
                            cb.Stroke();
                        }

                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 10);
                        float encabezadoCentro = tablaY - (filaAltura / 2) + 5;
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ID", (col1 + col2) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ROL", (col2 + col3) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "CORREO", (col3 + col4) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "FECHA DE REGISTRO", (col4 + col5) / 2, encabezadoCentro + 3, 0);
                        cb.EndText();
                    }

                    void DrawFooter(PdfContentByte cb)
                    {
                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 13);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy}", 55, 65, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Hora del Reporte: {DateTime.Now:HH:mm:ss}", 55, 50, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Usuario: Administrador del Sistema.", 55, 35, 0);
                        cb.EndText();
                    }

                    var cbGlobal = writer.DirectContent;
                    int totalUsuarios = usuarios?.Count ?? 0;
                    int pagina = 0;
                    int index = 0;

                    while (index < totalUsuarios || (totalUsuarios == 0 && pagina == 0))
                    {
                        if (pagina > 0) doc.NewPage();
                        pagina++;

                        var cb = writer.DirectContent;
                        DrawPageHeader(cb, totalUsuarios);

                        float tablaTopY = height - topSpace;
                        float yStartForRows = tablaTopY - filaAltura;

                        cb.BeginText();
                        cb.SetFontAndSize(fontTabla, 10);

                        int filasEnEstaPagina = 0;
                        for (int r = 0; r < filasPorPagina && index < totalUsuarios; r++, index++)
                        {
                            var u = usuarios[index];
                            float y = yStartForRows - (r * filaAltura);
                            float centerY = y - (filaAltura / 2) + 5;

                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.Id).ToString(), (col1 + col2) / 2, centerY, 0);
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.RoleName ?? "-"), (col2 + col3) / 2, centerY, 0);
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.Email ?? "-"), (col3 + 10), centerY, 0);
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.DateRegisUser?.ToString("yyyy-MM-dd") ?? "-"), (col4 + col5) / 2, centerY, 0);
                            filasEnEstaPagina++;
                        }

                        cb.EndText();
                        DrawFooter(cb);
                    }

                    doc.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GenerateRegisteredUsersPdf: " + ex.ToString());
                return new byte[0];
            }
        }

        // ============================================================
        // REPORTE: USUARIOS RETIRADOS
        // ============================================================
        public static byte[] GenerateRetiredUsersPdf(List<UserCombinedDtos> usuarios, string usuario)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    float width = 756 * 0.75f;
                    float height = 1066 * 0.75f;
                    var pageSize = new Rectangle(width, height);
                    Document doc = new Document(pageSize, 0, 0, 0, 0);
                    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // Fondo
                    string fondoPath = HttpContext.Current.Server.MapPath("~/Content/Images/BackgroundReports.png");
                    iTextSharp.text.Image fondoImg = null;
                    if (File.Exists(fondoPath))
                    {
                        fondoImg = iTextSharp.text.Image.GetInstance(fondoPath);
                        fondoImg.SetAbsolutePosition(0, 0);
                        fondoImg.ScaleAbsolute(width, height);
                    }

                    // Fuentes (idénticas al PDF de registrados)
                    BaseFont fontTitulo = BaseFont.CreateFont(BaseFont.TIMES_BOLDITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontSub = BaseFont.CreateFont(BaseFont.TIMES_ITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontTabla = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                    // Márgenes y medidas
                    float marginLeft = 45;
                    float marginRight = 45;
                    float tablaX = marginLeft;
                    float tablaAncho = width - marginLeft - marginRight;
                    float filaAltura = 36f;
                    float topSpace = 150f;
                    float bottomSpace = 90f;
                    float tablaAvailableHeight = height - topSpace - bottomSpace;

                    // Columnas: ID | NOMBRE | ROL | LOGIN
                    float col1 = tablaX;
                    float col2 = tablaX + 40;     // ID
                    float col3 = tablaX + 230;    // NOMBRE
                    float col4 = tablaX + 370;    // ROL
                    float col5 = tablaX + tablaAncho; // LOGIN
                    float[] columnas = { col1, col2, col3, col4, col5 };

                    int filasPorPagina = Math.Max(1, (int)Math.Floor(tablaAvailableHeight / filaAltura) - 1);

                    // --------------------------------------------------------------
                    // ENCABEZADO → IGUAL A USUARIOS REGISTRADOS (solo texto cambia)
                    // --------------------------------------------------------------
                    void DrawPageHeader(PdfContentByte cb, int total)
                    {
                        if (fondoImg != null)
                            cb.AddImage(fondoImg);

                        cb.BeginText();
                        cb.SetFontAndSize(fontTitulo, 50); // Tamaño reducido como pediste
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "USUARIOS", 50, height - 75, 0);
                        cb.SetFontAndSize(fontSub, 33);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "RETIRADOS", 170, height - 110, 0);
                        cb.EndText();

                        // Cuadro total (idéntico al PDF guía)
                        float cuadroX = width - 170;
                        float cuadroY = height - 75 - 28;
                        float cuadroAncho = 120;
                        float cuadroAlto = 40;

                        cb.SetLineWidth(1.5f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB)); // mismo color
                        cb.Rectangle(cuadroX, cuadroY - cuadroAlto, cuadroAncho, cuadroAlto);
                        cb.Stroke();

                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 24);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, total.ToString(), cuadroX + cuadroAncho / 2, cuadroY - 25, 0);
                        cb.EndText();

                        // Tabla contenedora
                        float totalFilasVisual = filasPorPagina + 1;
                        float tablaAltura = totalFilasVisual * filaAltura;
                        float tablaY = height - topSpace;

                        cb.SetLineWidth(1.3f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(tablaX, tablaY - tablaAltura, tablaAncho, tablaAltura);
                        cb.Stroke();

                        // Líneas verticales
                        foreach (float col in columnas)
                        {
                            cb.MoveTo(col, tablaY);
                            cb.LineTo(col, tablaY - tablaAltura);
                            cb.Stroke();
                        }

                        // Líneas horizontales
                        for (int i = 0; i <= filasPorPagina + 1; i++)
                        {
                            float y = tablaY - (i * filaAltura);
                            cb.MoveTo(tablaX, y);
                            cb.LineTo(tablaX + tablaAncho, y);
                            cb.Stroke();
                        }

                        // Encabezados de columnas
                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 10);
                        float center = tablaY - (filaAltura / 2) + 5;

                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ID", (col1 + col2) / 2, center + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "NOMBRE", (col2 + col3) / 2, center + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ROL", (col3 + col4) / 2, center + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ÚLTIMO LOGIN", (col4 + col5) / 2, center + 3, 0);

                        cb.EndText();
                    }

                    // --------------------------------------------------------------
                    // FOOTER IDENTICO AL PDF GUIA
                    // --------------------------------------------------------------
                    void DrawFooter(PdfContentByte cb)
                    {
                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 13);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy}", 55, 65, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Hora del Reporte: {DateTime.Now:HH:mm:ss}", 55, 50, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Usuario: Administrador del Sistema.", 55, 35, 0);
                        cb.EndText();
                    }

                    int totalUsuarios = usuarios?.Count ?? 0;
                    int pagina = 0;
                    int index = 0;

                    while (index < totalUsuarios || (totalUsuarios == 0 && pagina == 0))
                    {
                        if (pagina > 0) doc.NewPage();
                        pagina++;

                        var cb = writer.DirectContent;
                        DrawPageHeader(cb, totalUsuarios);

                        float tablaTopY = height - topSpace;
                        float yStartForRows = tablaTopY - filaAltura;

                        cb.BeginText();
                        cb.SetFontAndSize(fontTabla, 10);

                        for (int r = 0; r < filasPorPagina && index < totalUsuarios; r++, index++)
                        {
                            var u = usuarios[index];
                            float y = yStartForRows - (r * filaAltura);
                            float centerY = y - (filaAltura / 2) + 5;

                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.Id).ToString(), (col1 + col2) / 2, centerY, 0);
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.Name ?? "-"), col2 + 8, centerY, 0);
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.RoleName ?? "-"), (col3 + col4) / 2, centerY, 0);
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.LastLogin?.ToString("yyyy-MM-dd") ?? "-"), (col4 + col5) / 2, centerY, 0);
                        }

                        cb.EndText();
                        DrawFooter(cb);
                    }

                    doc.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GenerateRetiredUsersPdf: " + ex.ToString());
                return new byte[0];
            }
        }


        // ============================================================
        // PDF: USUARIOS ACTIVOS (4 columnas: ID, ROL, CORREO, ÚLTIMO LOGIN)
        // ============================================================
        public static byte[] GenerateActiveUsersPdf(List<UserCombinedDtos> usuarios, string usuario)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    float width = 756 * 0.75f;
                    float height = 1066 * 0.75f;
                    var pageSize = new Rectangle(width, height);
                    Document doc = new Document(pageSize, 0, 0, 0, 0);
                    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // Fondo
                    string fondoPath = HttpContext.Current.Server.MapPath("~/Content/Images/BackgroundReports.png");
                    iTextSharp.text.Image fondoImg = null;
                    if (File.Exists(fondoPath))
                    {
                        fondoImg = iTextSharp.text.Image.GetInstance(fondoPath);
                        fondoImg.SetAbsolutePosition(0, 0);
                        fondoImg.ScaleAbsolute(width, height);
                    }

                    // Fuentes unificadas
                    BaseFont fontTitulo = BaseFont.CreateFont(BaseFont.TIMES_BOLDITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontSub = BaseFont.CreateFont(BaseFont.TIMES_ITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontTabla = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                    // Medidas
                    float marginLeft = 45;
                    float marginRight = 45;
                    float tablaX = marginLeft;
                    float tablaAncho = width - marginLeft - marginRight;
                    float filaAltura = 36f;
                    float topSpace = 150f;
                    float bottomSpace = 90f;
                    float tablaAvailableHeight = height - topSpace - bottomSpace;

                    // === POSICIONES PROBADAS Y CORREGIDAS ===
                    float colID = tablaX;
                    float colROL = tablaX + 75;
                    float colCORREO = tablaX + 220;
                    float colLAST = tablaX + 400;
                    float colRight = tablaX + tablaAncho;

                    float[] columnas = { colID, colROL, colCORREO, colLAST, colRight };

                    int filasPorPagina = Math.Max(1, (int)Math.Floor(tablaAvailableHeight / filaAltura) - 1);

                    // ==============================
                    // HEADER
                    // ==============================
                    void DrawHeader(PdfContentByte cb, int TotalUsuarios)
                    {
                        if (fondoImg != null) cb.AddImage(fondoImg);

                        cb.BeginText();
                        cb.SetFontAndSize(fontTitulo, 50);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "USUARIOS", 50, height - 75, 0);

                        cb.SetFontAndSize(fontSub, 33);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "ACTIVOS", 230, height - 110, 0);
                        cb.EndText();

                        // Caja TOTAL
                        float cuadroX = width - 170;
                        float cuadroY = height - 75 - 28;
                        float cuadroAncho = 120;
                        float cuadroAlto = 40;

                        cb.SetLineWidth(1.5f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(cuadroX, cuadroY - cuadroAlto, cuadroAncho, cuadroAlto);
                        cb.Stroke();

                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 24);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, TotalUsuarios.ToString(),
                                           cuadroX + cuadroAncho / 2, cuadroY - 25, 0);
                        cb.EndText();

                        // Tabla
                        float totalFilasVisual = filasPorPagina + 1;
                        float tablaAltura = totalFilasVisual * filaAltura;
                        float tablaY = height - topSpace;

                        cb.SetLineWidth(1.3f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(tablaX, tablaY - tablaAltura, tablaAncho, tablaAltura);
                        cb.Stroke();

                        // Columnas
                        foreach (float col in columnas)
                        {
                            cb.MoveTo(col, tablaY);
                            cb.LineTo(col, tablaY - tablaAltura);
                            cb.Stroke();
                        }

                        // Líneas
                        for (int i = 0; i <= filasPorPagina + 1; i++)
                        {
                            float y = tablaY - (i * filaAltura);
                            cb.MoveTo(tablaX, y);
                            cb.LineTo(tablaX + tablaAncho, y);
                            cb.Stroke();
                        }

                        // Encabezados
                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 10);
                        float c = tablaY - (filaAltura / 2) + 5;

                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ID", (colID + colROL) / 2, c + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ROL", (colROL + colCORREO) / 2, c + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "CORREO", (colCORREO + colLAST) / 2, c + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ÚLTIMO LOGIN", (colLAST + colRight) / 2, c + 3, 0);

                        cb.EndText();
                    }

                    // FOOTER
                    void DrawFooter(PdfContentByte cb)
                    {
                        cb.BeginText();
                        cb.SetFontAndSize(fontSub, 13);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy}", 55, 65, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Hora del Reporte: {DateTime.Now:HH:mm:ss}", 55, 50, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Usuario: Administrador del Sistema.", 55, 35, 0);
                        cb.EndText();
                    }

                    // ==============================
                    // GENERACIÓN DE PÁGINAS
                    // ==============================
                    int totalUsuarios = usuarios?.Count ?? 0;
                    int pagina = 0;
                    int index = 0;

                    while (index < totalUsuarios || (totalUsuarios == 0 && pagina == 0))
                    {
                        if (pagina > 0) doc.NewPage();
                        pagina++;

                        var cb = writer.DirectContent;
                        DrawHeader(cb, totalUsuarios);

                        float tablaTopY = height - topSpace;
                        float yStartForRows = tablaTopY - filaAltura;

                        cb.BeginText();
                        cb.SetFontAndSize(fontTabla, 10);

                        for (int r = 0; r < filasPorPagina && index < totalUsuarios; r++, index++)
                        {
                            var u = usuarios[index];
                            float y = yStartForRows - (r * filaAltura);
                            float cy = y - (filaAltura / 2) + 5;

                            cb.ShowTextAligned(Element.ALIGN_CENTER, (index + 1).ToString(), (colID + colROL) / 2, cy, 0);
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.RoleName ?? "-"), (colROL + colCORREO) / 2, cy, 0);
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.Email ?? "-"), colCORREO + 5, cy, 0);
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.LastLogin?.ToString("dd/MM/yyyy") ?? "-"),
                                               (colLAST + colRight) / 2, cy, 0);
                        }

                        cb.EndText();
                        DrawFooter(cb);
                    }

                    doc.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GenerateActiveUsersPdf: " + ex.ToString());
                return new byte[0];
            }
        }
    }
}
