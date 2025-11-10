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
        public static byte[] GenerateNewUsersPdf(List<UserCombinedDtos> usuarios, string usuario)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    // Tamaño base (igual al que ya tenías)
                    float width = 756 * 0.75f;
                    float height = 1066 * 0.75f;
                    var pageSize = new Rectangle(width, height);
                    Document doc = new Document(pageSize, 0, 0, 0, 0);
                    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // Cargar fondo si existe
                    string fondoPath = HttpContext.Current.Server.MapPath("~/Content/Images/BackgroundReports.png");
                    iTextSharp.text.Image fondoImg = null;
                    if (File.Exists(fondoPath))
                    {
                        fondoImg = iTextSharp.text.Image.GetInstance(fondoPath);
                        fondoImg.SetAbsolutePosition(0, 0);
                        fondoImg.ScaleAbsolute(width, height);
                    }

                    // Fuentes
                    BaseFont fontUsuarios = BaseFont.CreateFont(BaseFont.TIMES_BOLDITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontNuevos = BaseFont.CreateFont(BaseFont.TIMES_ITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontTabla = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                    // Parámetros visuales de la tabla
                    float marginLeft = 45;
                    float marginRight = 45;
                    float tablaX = marginLeft;
                    float tablaAncho = width - marginLeft - marginRight;
                    float filaAltura = 36f;

                    // Área disponible vertical para la tabla en cada página:
                    // Dejamos espacio arriba para título y cuadro total, y abajo para el pie.
                    float topSpace = 150f;   // espacio para encabezado (ajusta si hace falta)
                    float bottomSpace = 90f; // espacio para pie con fecha/hora
                    float tablaAvailableHeight = height - topSpace - bottomSpace;

                    // Columnas (manteniendo las proporciones originales)
                    float col1 = tablaX;
                    float col2 = tablaX + 55;
                    float col3 = tablaX + 230;
                    float col4 = tablaX + 400;
                    float col5 = tablaX + tablaAncho;
                    float[] columnas = { col1, col2, col3, col4, col5 };

                    // Calcular filas por página (restamos 1 para header)
                    int filasPorPagina = Math.Max(1, (int)Math.Floor(tablaAvailableHeight / filaAltura) - 1);

                    // Helper local para dibujar encabezado y cuadro de total en la página actual
                    void DrawPageHeaderAndBox(PdfContentByte cb, int TotalUsuarios)
                    {
                        // fondo
                        if (fondoImg != null)
                        {
                            cb.AddImage(fondoImg);
                        }

                        // Títulos grandes top-left
                        cb.BeginText();
                        cb.SetFontAndSize(fontUsuarios, 55);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "USUARIOS", 60, height - 75, 0);
                        cb.SetFontAndSize(fontNuevos, 37);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "NUEVOS", 185, height - 110, 0);
                        cb.EndText();

                        // Cuadro con total (top-right)
                        float cuadroX = width - 170;
                        float cuadroY = height - 75 - 28;
                        float cuadroAncho = 120;
                        float cuadroAlto = 40;
                        cb.SetLineWidth(1.5f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(cuadroX, cuadroY - cuadroAlto, cuadroAncho, cuadroAlto);
                        cb.Stroke();

                        cb.BeginText();
                        cb.SetFontAndSize(fontNuevos, 24);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, TotalUsuarios.ToString(), cuadroX + cuadroAncho / 2, cuadroY - 25, 0);
                        cb.EndText();

                        // Dibujar rectángulo de tabla (aquí se hace referencia al área de la tabla)
                        float totalFilasVisual = filasPorPagina + 1; // header + filasPorPagina
                        float tablaAltura = totalFilasVisual * filaAltura;
                        float tablaY = height - topSpace; // top de la tabla

                        cb.SetLineWidth(1.3f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(tablaX, tablaY - tablaAltura, tablaAncho, tablaAltura);
                        cb.Stroke();

                        // Columnas verticales
                        foreach (float col in columnas)
                        {
                            cb.MoveTo(col, tablaY);
                            cb.LineTo(col, tablaY - tablaAltura);
                            cb.Stroke();
                        }

                        // Líneas horizontales (header + separadores suficientes)
                        for (int i = 0; i <= filasPorPagina + 1; i++)
                        {
                            float y = tablaY - (i * filaAltura);
                            cb.MoveTo(tablaX, y);
                            cb.LineTo(tablaX + tablaAncho, y);
                            cb.Stroke();
                        }

                        // Encabezados de columna (centrados)
                        cb.BeginText();
                        cb.SetFontAndSize(fontNuevos, 10);
                        float encabezadoCentro = tablaY - (filaAltura / 2) + 5;
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ID", (col1 + col2) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "NOMBRE", (col2 + col3) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "CORREO", (col3 + col4) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "FECHA DE REGISTRO", (col4 + col5) / 2, encabezadoCentro + 3, 0);
                        cb.EndText();
                    }

                    // Helper para dibujar pie (fecha/hora/usuario) en la página actual
                    void DrawFooter(PdfContentByte cb)
                    {
                        cb.BeginText();
                        cb.SetFontAndSize(fontNuevos, 13);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy}", 55, 65, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Hora del Reporte: {DateTime.Now:HH:mm:ss}", 55, 50, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Usuario: {usuario}", 55, 35, 0);
                        cb.EndText();
                    }

                    // Comenzamos a escribir datos por páginas
                    var cbGlobal = writer.DirectContent;
                    int totalUsuarios = usuarios?.Count ?? 0;
                    int pagina = 0;
                    int index = 0;

                    while (index < totalUsuarios || (totalUsuarios == 0 && pagina == 0))
                    {
                        if (pagina > 0) doc.NewPage(); // nueva página si no es la primera
                        pagina++;

                        var cb = writer.DirectContent; // contenido de esta página

                        // Dibujar encabezado (incluye recuadro y encabezado de tabla)
                        DrawPageHeaderAndBox(cb, totalUsuarios);

                        // Coordenada Y del top de la tabla en esta página
                        float tablaTopY = height - topSpace;
                        // El header ocupa la primera fila; los datos comienzan en (fila 1..filasPorPagina)
                        float yStartForRows = tablaTopY - filaAltura; // primer y de fila de datos (centrado se calculará)

                        // Dibujar filas de esta página
                        cb.BeginText();
                        cb.SetFontAndSize(fontTabla, 10);

                        int filasEnEstaPagina = 0;
                        for (int r = 0; r < filasPorPagina && index < totalUsuarios; r++, index++)
                        {
                            var u = usuarios[index];
                            float y = yStartForRows - (r * filaAltura);
                            float centerY = y - (filaAltura / 2) + 5;

                            // ID (numero secuencial global)
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (index + 1).ToString(), (col1 + col2) / 2, centerY, 0);

                            // Nombre - se limita a una cadena razonable; si es larga no saldrá del recuadro (si necesitas wrap, habría que usar ColumnText)
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.Name ?? "-"), (col2 + 5), centerY, 0);

                            // Correo - centrado/izquierda dependiendo de tu diseño
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.Email ?? "-"), (col3 + 5), centerY, 0);

                            // Fecha
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.DateRegisUser?.ToString("yyyy-MM-dd") ?? "-"), (col4 + col5) / 2, centerY, 0);

                            filasEnEstaPagina++;
                        }

                        cb.EndText();

                        // Pie
                        DrawFooter(cb);
                    }

                    // cerrar doc y devolver bytes
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
                    float col2 = tablaX + 80;
                    float col3 = tablaX + 270;
                    float col4 = tablaX + 460;
                    float col5 = tablaX + tablaAncho;
                    float[] columnas = { col1, col2, col3, col4, col5 };

                    int filasPorPagina = Math.Max(1, (int)Math.Floor(tablaAvailableHeight / filaAltura) - 1);

                    void DrawPageHeader(PdfContentByte cb, int total)
                    {
                        if (fondoImg != null)
                            cb.AddImage(fondoImg);

                        cb.BeginText();
                        cb.SetFontAndSize(fontTitulo, 50);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "USUARIOS", 60, height - 75, 0);
                        cb.SetFontAndSize(fontSub, 33);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "REGISTRADOS", 180, height - 110, 0);
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
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Usuario: {usuario}", 55, 35, 0);
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
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.Email ?? "-"), (col3 + 5), centerY, 0);
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
        // PDF: USUARIOS RETIRADOS
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

                    string fondoPath = HttpContext.Current.Server.MapPath("~/Content/Images/BackgroundReports.png");
                    iTextSharp.text.Image fondoImg = null;
                    if (File.Exists(fondoPath))
                    {
                        fondoImg = iTextSharp.text.Image.GetInstance(fondoPath);
                        fondoImg.SetAbsolutePosition(0, 0);
                        fondoImg.ScaleAbsolute(width, height);
                    }

                    BaseFont fontUsuarios = BaseFont.CreateFont(BaseFont.TIMES_BOLDITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontSubtitulo = BaseFont.CreateFont(BaseFont.TIMES_ITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontTabla = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                    float marginLeft = 45;
                    float marginRight = 45;
                    float tablaX = marginLeft;
                    float tablaAncho = width - marginLeft - marginRight;
                    float filaAltura = 36f;
                    float topSpace = 150f;
                    float bottomSpace = 90f;
                    float tablaAvailableHeight = height - topSpace - bottomSpace;

                    float col1 = tablaX;
                    float col2 = tablaX + 55;
                    float col3 = tablaX + 230;
                    float col4 = tablaX + 400;
                    float col5 = tablaX + tablaAncho;
                    float[] columnas = { col1, col2, col3, col4, col5 };
                    int filasPorPagina = Math.Max(1, (int)Math.Floor(tablaAvailableHeight / filaAltura) - 1);

                    void DrawHeader(PdfContentByte cb, int total)
                    {
                        if (fondoImg != null) cb.AddImage(fondoImg);
                        cb.BeginText();
                        cb.SetFontAndSize(fontUsuarios, 55);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "USUARIOS", 60, height - 75, 0);
                        cb.SetFontAndSize(fontSubtitulo, 37);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "RETIRADOS", 185, height - 110, 0);
                        cb.EndText();

                        float cuadroX = width - 170;
                        float cuadroY = height - 75 - 28;
                        float cuadroAncho = 120;
                        float cuadroAlto = 40;
                        cb.SetLineWidth(1.5f);
                        cb.Rectangle(cuadroX, cuadroY - cuadroAlto, cuadroAncho, cuadroAlto);
                        cb.Stroke();
                        cb.BeginText();
                        cb.SetFontAndSize(fontSubtitulo, 24);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, total.ToString(), cuadroX + cuadroAncho / 2, cuadroY - 25, 0);
                        cb.EndText();

                        float totalFilasVisual = filasPorPagina + 1;
                        float tablaAltura = totalFilasVisual * filaAltura;
                        float tablaY = height - topSpace;
                        cb.SetLineWidth(1.3f);
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
                        cb.SetFontAndSize(fontSubtitulo, 10);
                        float encabezadoCentro = tablaY - (filaAltura / 2) + 5;
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ID", (col1 + col2) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "NOMBRE", (col2 + col3) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ROL", (col3 + col4) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ÚLTIMO LOGIN", (col4 + col5) / 2, encabezadoCentro + 3, 0);
                        cb.EndText();
                    }

                    void DrawFooter(PdfContentByte cb)
                    {
                        cb.BeginText();
                        cb.SetFontAndSize(fontSubtitulo, 13);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy}", 55, 65, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Hora del Reporte: {DateTime.Now:HH:mm:ss}", 55, 50, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Usuario: {usuario}", 55, 35, 0);
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
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.Name ?? "-"), (col2 + 5), centerY, 0);
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.RoleName ?? "-"), (col3 + 5), centerY, 0);
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

                    string fondoPath = HttpContext.Current.Server.MapPath("~/Content/Images/BackgroundReports.png");
                    iTextSharp.text.Image fondoImg = null;
                    if (File.Exists(fondoPath))
                    {
                        fondoImg = iTextSharp.text.Image.GetInstance(fondoPath);
                        fondoImg.SetAbsolutePosition(0, 0);
                        fondoImg.ScaleAbsolute(width, height);
                    }

                    BaseFont fontUsuarios = BaseFont.CreateFont(BaseFont.TIMES_BOLDITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontNuevos = BaseFont.CreateFont(BaseFont.TIMES_ITALIC, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    BaseFont fontTabla = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                    float marginLeft = 45;
                    float marginRight = 45;
                    float tablaX = marginLeft;
                    float tablaAncho = width - marginLeft - marginRight;
                    float filaAltura = 36f;
                    float topSpace = 150f;
                    float bottomSpace = 90f;
                    float tablaAvailableHeight = height - topSpace - bottomSpace;

                    // --- DEFINIMOS 4 COLUMNAS (posiciones)
                    // ID | ROL | CORREO | ÚLTIMO LOGIN
                    float colID = tablaX;
                    float colROL = tablaX + 70;      // limite izquierda de la columna ROL
                    float colCORREO = tablaX + 300;  // limite izquierda columna CORREO
                    float colLAST = tablaX + 520;    // limite izquierda columna ULTIMO LOGIN
                    float colRight = tablaX + tablaAncho; // borde derecho
                    float[] columnas = { colID, colROL, colCORREO, colLAST, colRight };

                    int filasPorPagina = Math.Max(1, (int)Math.Floor(tablaAvailableHeight / filaAltura) - 1);

                    void DrawPageHeaderAndBox(PdfContentByte cb, int TotalUsuarios)
                    {
                        if (fondoImg != null) cb.AddImage(fondoImg);

                        // Títulos
                        cb.BeginText();
                        cb.SetFontAndSize(fontUsuarios, 55);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "USUARIOS", 60, height - 75, 0);
                        cb.SetFontAndSize(fontNuevos, 37);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, "ACTIVOS", 185, height - 110, 0);
                        cb.EndText();

                        // Caja total
                        float cuadroX = width - 170;
                        float cuadroY = height - 75 - 28;
                        float cuadroAncho = 120;
                        float cuadroAlto = 40;
                        cb.SetLineWidth(1.5f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(cuadroX, cuadroY - cuadroAlto, cuadroAncho, cuadroAlto);
                        cb.Stroke();
                        cb.BeginText();
                        cb.SetFontAndSize(fontNuevos, 24);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, TotalUsuarios.ToString(), cuadroX + cuadroAncho / 2, cuadroY - 25, 0);
                        cb.EndText();

                        // Rectángulo tabla
                        float totalFilasVisual = filasPorPagina + 1;
                        float tablaAltura = totalFilasVisual * filaAltura;
                        float tablaY = height - topSpace;
                        cb.SetLineWidth(1.3f);
                        cb.SetColorStroke(new BaseColor(0xD9, 0xCF, 0xEB));
                        cb.Rectangle(tablaX, tablaY - tablaAltura, tablaAncho, tablaAltura);
                        cb.Stroke();

                        // Columnas verticales
                        foreach (float col in columnas) { cb.MoveTo(col, tablaY); cb.LineTo(col, tablaY - tablaAltura); cb.Stroke(); }

                        // Líneas horizontales
                        for (int i = 0; i <= filasPorPagina + 1; i++)
                        {
                            float y = tablaY - (i * filaAltura);
                            cb.MoveTo(tablaX, y);
                            cb.LineTo(tablaX + tablaAncho, y);
                            cb.Stroke();
                        }

                        // Encabezados (centrados según cada columna)
                        cb.BeginText();
                        cb.SetFontAndSize(fontNuevos, 10);
                        float encabezadoCentro = tablaY - (filaAltura / 2) + 5;
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ID", (colID + colROL) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ROL", (colROL + colCORREO) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "CORREO", (colCORREO + colLAST) / 2, encabezadoCentro + 3, 0);
                        cb.ShowTextAligned(Element.ALIGN_CENTER, "ÚLTIMO LOGIN", (colLAST + colRight) / 2, encabezadoCentro + 3, 0);
                        cb.EndText();
                    }

                    void DrawFooter(PdfContentByte cb)
                    {
                        cb.BeginText();
                        cb.SetFontAndSize(fontNuevos, 13);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy}", 55, 65, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Hora del Reporte: {DateTime.Now:HH:mm:ss}", 55, 50, 0);
                        cb.ShowTextAligned(Element.ALIGN_LEFT, $"Usuario: {usuario}", 55, 35, 0);
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
                        DrawPageHeaderAndBox(cb, totalUsuarios);

                        float tablaTopY = height - topSpace;
                        float yStartForRows = tablaTopY - filaAltura;

                        cb.BeginText();
                        cb.SetFontAndSize(fontTabla, 10);

                        for (int r = 0; r < filasPorPagina && index < totalUsuarios; r++, index++)
                        {
                            var u = usuarios[index];
                            float y = yStartForRows - (r * filaAltura);
                            float centerY = y - (filaAltura / 2) + 5;

                            // ID
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (index + 1).ToString(), (colID + colROL) / 2, centerY, 0);
                            // ROL (usamos RoleName)
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.RoleName ?? "-"), colROL + 5, centerY, 0);
                            // CORREO
                            cb.ShowTextAligned(Element.ALIGN_LEFT, (u.Email ?? "-"), colCORREO + 5, centerY, 0);
                            // ÚLTIMO LOGIN
                            cb.ShowTextAligned(Element.ALIGN_CENTER, (u.LastLogin?.ToString("dd/MM/yyyy") ?? "-"), (colLAST + colRight) / 2, centerY, 0);
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
