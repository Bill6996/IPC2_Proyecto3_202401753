using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Backend.Services
{
    public class PdfService
    {
        private readonly XmlDataService _data;
        private readonly EstadoCuentaService _estadoSvc;

        public PdfService(XmlDataService data, EstadoCuentaService estadoSvc)
        {
            _data = data;
            _estadoSvc = estadoSvc;
        }

        // ─────────────────────────────────────────────────────────────────
        // PDF de Estado de Cuenta
        // ─────────────────────────────────────────────────────────────────
        public byte[] GenerarEstadoCuentaPdf(string? nit)
        {
            var resultado = (List<object>)_estadoSvc.GetEstadoCuenta(nit);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text("Estado de Cuenta de Clientes")
                            .Bold().FontSize(16).FontColor("#1a3c5e");

                        col.Item().Text($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Medium);

                        col.Item().PaddingTop(10);

                        foreach (dynamic cliente in resultado)
                        {
                            col.Item().PaddingTop(15).Border(1).BorderColor("#1a3c5e")
                                .Padding(10).Column(clienteCol =>
                                {
                                    // Cabecera del cliente
                                    clienteCol.Item().Background("#1a3c5e").Padding(8)
                                        .Text($"Cliente: {cliente.NIT} — {cliente.Nombre}")
                                        .Bold().FontColor(Colors.White);

                                    clienteCol.Item().PaddingTop(6).Row(row =>
                                    {
                                        row.RelativeItem().Text(
                                            $"Saldo pendiente: Q. {((double)cliente.SaldoActual):N2}")
                                            .Bold().FontColor(
                                                (double)cliente.SaldoActual > 0
                                                ? Colors.Red.Medium : Colors.Green.Medium);

                                        if ((double)cliente.SaldoAFavor > 0)
                                            row.RelativeItem().AlignRight().Text(
                                                $"Saldo a favor: Q. {((double)cliente.SaldoAFavor):N2}")
                                                .Bold().FontColor(Colors.Blue.Medium);
                                    });

                                    // Tabla de transacciones
                                    clienteCol.Item().PaddingTop(8).Table(table =>
                                    {
                                        table.ColumnsDefinition(cols =>
                                        {
                                            cols.RelativeColumn(2);
                                            cols.RelativeColumn(4);
                                            cols.RelativeColumn(3);
                                        });

                                        // Encabezado
                                        table.Header(header =>
                                        {
                                            foreach (var h in new[] { "Fecha", "Descripción", "Monto" })
                                                header.Cell().Background("#2e6da4").Padding(5)
                                                    .Text(h).Bold().FontColor(Colors.White).FontSize(9);
                                        });

                                        // Filas
                                        bool alt = false;
                                        foreach (dynamic t in cliente.Transacciones)
                                        {
                                            string bg = alt ? "#f0f4f8" : Colors.White;
                                            string tipo = (string)t.Tipo == "cargo" ? "Cargo" : "Abono";
                                            string desc = $"{tipo}: {t.Descripcion}";
                                            string monto = $"Q. {((double)t.Monto):N2}";
                                            string color = (string)t.Tipo == "cargo"
                                                ? Colors.Red.Medium : Colors.Green.Medium;

                                            table.Cell().Background(bg).Padding(4).Text((string)t.Fecha).FontSize(9);
                                            table.Cell().Background(bg).Padding(4).Text(desc).FontSize(9);
                                            table.Cell().Background(bg).Padding(4).AlignRight()
                                                .Text(monto).FontSize(9).FontColor(color);

                                            alt = !alt;
                                        }

                                        if (((System.Collections.IList)cliente.Transacciones).Count == 0)
                                        {
                                            table.Cell().ColumnSpan(3).Padding(6)
                                                .Text("Sin transacciones registradas.")
                                                .Italic().FontColor(Colors.Grey.Medium).FontSize(9);
                                        }
                                    });
                                });
                        }

                        if (resultado.Count == 0)
                            col.Item().PaddingTop(20).Text("No se encontraron clientes.")
                                .Italic().FontColor(Colors.Grey.Medium);
                    });

                    page.Footer().AlignCenter()
                        .Text(x =>
                        {
                            x.Span("ITGSA — Industria Típica Guatemalteca S.A. | Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        // ─────────────────────────────────────────────────────────────────
        // PDF de Resumen de Ingresos por Banco
        // ─────────────────────────────────────────────────────────────────
        public byte[] GenerarResumenPagosPdf(int mes, int anio)
        {
            var resultado = (List<object>)_estadoSvc.GetResumenPagos(mes, anio);
            var pagos = _data.GetPagos();
            var bancos = _data.GetBancos();

            // Últimos 3 meses
            var meses = new List<(int m, int a)>();
            for (int i = 0; i < 3; i++)
            {
                int m = mes - i, a = anio;
                while (m <= 0) { m += 12; a--; }
                meses.Add((m, a));
            }

            string NombreMes(int m) =>
                CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.GetMonthName(m);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(4).Text("Resumen de Ingresos por Banco")
                            .Bold().FontSize(16).FontColor("#1a3c5e");

                        col.Item().Text(
                            $"Período: {NombreMes(meses.Last().m)}/{meses.Last().a} — " +
                            $"{NombreMes(meses.First().m)}/{meses.First().a}")
                            .FontSize(10).FontColor(Colors.Black);

                        col.Item().Text($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Medium);

                        col.Item().PaddingTop(16).Table(table =>
                        {
                            // Columnas: Banco + 3 meses + Total
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                foreach (var _ in meses) cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background("#1a3c5e").Padding(6)
                                    .Text("Banco").Bold().FontColor(Colors.White);

                                foreach (var (m, a) in meses)
                                    header.Cell().Background("#1a3c5e").Padding(6).AlignCenter()
                                        .Text($"{NombreMes(m)}\n{a}").Bold()
                                        .FontColor(Colors.White).FontSize(9);

                                header.Cell().Background("#2e6da4").Padding(6).AlignRight()
                                    .Text("Total").Bold().FontColor(Colors.White);
                            });

                            bool alt = false;
                            double[] totalesMes = new double[meses.Count];
                            double granTotal = 0;

                            foreach (dynamic banco in resultado)
                            {
                                string bg = alt ? "#f0f4f8" : Colors.White;
                                double totalBanco = 0;

                                table.Cell().Background(bg).Padding(5)
                                    .Text((string)banco.Nombre).Bold().FontSize(9);

                                int mi = 0;
                                foreach (dynamic mesData in banco.Meses)
                                {
                                    double total = (double)mesData.Total;
                                    totalBanco += total;
                                    totalesMes[mi++] += total;
                                    table.Cell().Background(bg).Padding(5).AlignRight()
                                        .Text($"Q. {total:N2}").FontSize(9);
                                }

                                granTotal += totalBanco;
                                table.Cell().Background(bg).Padding(5).AlignRight()
                                    .Text($"Q. {totalBanco:N2}").Bold().FontSize(9)
                                    .FontColor(Colors.Blue.Darken2);

                                alt = !alt;
                            }

                            // Fila de totales
                            table.Cell().Background("#e8f0fe").Padding(5)
                                .Text("TOTAL").Bold().FontSize(9);
                            foreach (var t in totalesMes)
                                table.Cell().Background("#e8f0fe").Padding(5).AlignRight()
                                    .Text($"Q. {t:N2}").Bold().FontSize(9);
                            table.Cell().Background("#c8dbfa").Padding(5).AlignRight()
                                .Text($"Q. {granTotal:N2}").Bold().FontSize(9)
                                .FontColor("#1a3c5e");
                        });

                        // Resumen textual por mes
                        col.Item().PaddingTop(20).Text("Resumen por mes")
                            .Bold().FontSize(12).FontColor("#1a3c5e");

                        foreach (var (m, a) in meses)
                        {
                            double totalMes = pagos
                                .Where(p => DateTime.TryParseExact(
                                    p.Fecha, "dd/MM/yyyy",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out var d)
                                    && d.Month == m && d.Year == a)
                                .Sum(p => p.Valor);

                            int cantPagos = pagos.Count(p =>
                                DateTime.TryParseExact(p.Fecha, "dd/MM/yyyy",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out var d)
                                && d.Month == m && d.Year == a);

                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Text(
                                    $"• {NombreMes(m)} {a}: {cantPagos} pago(s) " +
                                    $"— Total recaudado: Q. {totalMes:N2}")
                                    .FontSize(10);
                            });
                        }
                    });

                    page.Footer().AlignCenter()
                        .Text(x =>
                        {
                            x.Span("ITGSA — Industria Típica Guatemalteca S.A. | Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        // ─────────────────────────────────────────────────────────────────
        // Encabezado compartido entre PDFs
        // ─────────────────────────────────────────────────────────────────
        private void ComposeHeader(IContainer container)
        {
            container.BorderBottom(2).BorderColor("#1a3c5e").PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Industria Típica Guatemalteca, S.A.")
                        .Bold().FontSize(14).FontColor("#1a3c5e");
                    col.Item().Text("Sistema de Facturación y Pagos — ITGSA")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(120).AlignRight().Column(col =>
                {
                    col.Item().Text("Universidad San Carlos")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                    col.Item().Text("Facultad de Ingeniería")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                    col.Item().Text("IPC2 — Proyecto 3")
                        .FontSize(8).Bold().FontColor("#1a3c5e");
                });
            });
        }
    }
}