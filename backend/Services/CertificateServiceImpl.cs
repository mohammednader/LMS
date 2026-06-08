using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Geom;

namespace TrainingService.Services;

public class CertificateServiceImpl : ICertificateService
{
    private readonly ILogger<CertificateServiceImpl> _logger;

    // Royal Commission brand colours
    private static readonly DeviceRgb RcBlue   = new(0, 70, 127);
    private static readonly DeviceRgb RcGold   = new(180, 140, 50);
    private static readonly DeviceRgb LightBg  = new(245, 248, 252);
    private static readonly DeviceRgb DarkText = new(30, 30, 30);
    private static readonly DeviceRgb GreyText = new(100, 100, 100);

    public CertificateServiceImpl(ILogger<CertificateServiceImpl> logger) => _logger = logger;

    public byte[] GenerateCertificate(int orgId, string studentName, string courseName, DateTime date, bool isValid = true)
    {
        try
        {
            using var ms = new MemoryStream();
            var writer   = new PdfWriter(ms);
            var pdf      = new PdfDocument(writer);

            // A4 landscape
            var pageSize = PageSize.A4.Rotate();
            pdf.SetDefaultPageSize(pageSize);

            var document = new Document(pdf, pageSize);
            document.SetMargins(0, 0, 0, 0);

            var page   = pdf.AddNewPage();
            var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);
            float w    = pageSize.GetWidth();
            float h    = pageSize.GetHeight();

            // ── Background fill ──────────────────────────────────────────────
            canvas.SetFillColor(LightBg).Rectangle(0, 0, w, h).Fill();

            // ── Outer thick border ───────────────────────────────────────────
            float m = 18f;
            canvas.SetStrokeColor(RcBlue).SetLineWidth(6)
                  .Rectangle(m, m, w - 2 * m, h - 2 * m).Stroke();

            // ── Inner thin gold border ───────────────────────────────────────
            float m2 = 26f;
            canvas.SetStrokeColor(RcGold).SetLineWidth(1.5f)
                  .Rectangle(m2, m2, w - 2 * m2, h - 2 * m2).Stroke();

            // ── Top banner ───────────────────────────────────────────────────
            canvas.SetFillColor(RcBlue)
                  .Rectangle(0, h - 80, w, 80).Fill();

            // ── Bottom banner ────────────────────────────────────────────────
            canvas.SetFillColor(RcBlue)
                  .Rectangle(0, 0, w, 50).Fill();

            // ── Gold accent line below top banner ────────────────────────────
            canvas.SetStrokeColor(RcGold).SetLineWidth(3)
                  .MoveTo(0, h - 83).LineTo(w, h - 83).Stroke();

            // ── Corner ornaments ─────────────────────────────────────────────
            float cs = 22f;
            canvas.SetFillColor(RcGold);
            // top-left
            canvas.Rectangle(m + 4, h - m - 4 - cs, cs, cs).Fill();
            // top-right
            canvas.Rectangle(w - m - 4 - cs, h - m - 4 - cs, cs, cs).Fill();
            // bottom-left
            canvas.Rectangle(m + 4, m + 4, cs, cs).Fill();
            // bottom-right
            canvas.Rectangle(w - m - 4 - cs, m + 4, cs, cs).Fill();

            // ── Watermark diagonal text ──────────────────────────────────────
            var watermarkFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            canvas.SaveState()
                  .SetFillColor(new DeviceRgb(220, 225, 235))
                  .BeginText()
                  .SetFontAndSize(watermarkFont, 60)
                  .SetTextRenderingMode(3)
                  .SetTextMatrix(0.7f, 0.3f, -0.3f, 0.7f, 80, 120)
                  .ShowText("TRAINING CERTIFICATE")
                  .EndText()
                  .RestoreState();

            // ── Expired stamp ─────────────────────────────────────────────────
            if (!isValid)
            {
                canvas.SaveState()
                      .SetStrokeColor(ColorConstants.RED)
                      .SetFillColor(new DeviceRgb(255, 200, 200))
                      .SetLineWidth(3)
                      .Rectangle(w / 2 - 110, h / 2 - 30, 220, 60).FillStroke()
                      .BeginText()
                      .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD), 28)
                      .SetFillColor(ColorConstants.RED)
                      .MoveText(w / 2 - 90, h / 2 - 8)
                      .ShowText("EXPIRED")
                      .EndText()
                      .RestoreState();
            }

            document.Close();

            // ── Now add text with Document API ────────────────────────────────
            using var ms2   = new MemoryStream();
            var writer2     = new PdfWriter(ms2);
            var pdf2        = new PdfDocument(writer2);
            pdf2.SetDefaultPageSize(pageSize);
            var doc2        = new Document(pdf2, pageSize);
            doc2.SetMargins(0, 0, 0, 0);

            var page2  = pdf2.AddNewPage();
            var can2   = new iText.Kernel.Pdf.Canvas.PdfCanvas(page2);

            // ── Repeat all drawing ────────────────────────────────────────────
            can2.SetFillColor(LightBg).Rectangle(0, 0, w, h).Fill();
            can2.SetStrokeColor(RcBlue).SetLineWidth(6).Rectangle(m, m, w - 2 * m, h - 2 * m).Stroke();
            can2.SetStrokeColor(RcGold).SetLineWidth(1.5f).Rectangle(m2, m2, w - 2 * m2, h - 2 * m2).Stroke();
            can2.SetFillColor(RcBlue).Rectangle(0, h - 80, w, 80).Fill();
            can2.SetFillColor(RcBlue).Rectangle(0, 0, w, 50).Fill();
            can2.SetStrokeColor(RcGold).SetLineWidth(3).MoveTo(0, h - 83).LineTo(w, h - 83).Stroke();
            can2.SetFillColor(RcGold);
            can2.Rectangle(m + 4, h - m - 4 - cs, cs, cs).Fill();
            can2.Rectangle(w - m - 4 - cs, h - m - 4 - cs, cs, cs).Fill();
            can2.Rectangle(m + 4, m + 4, cs, cs).Fill();
            can2.Rectangle(w - m - 4 - cs, m + 4, cs, cs).Fill();

            // Gold divider line in middle area
            can2.SetStrokeColor(RcGold).SetLineWidth(1f)
                .MoveTo(80, h / 2 - 10).LineTo(w - 80, h / 2 - 10).Stroke();

            if (!isValid)
            {
                can2.SaveState()
                    .SetStrokeColor(ColorConstants.RED)
                    .SetFillColor(new DeviceRgb(255, 230, 230))
                    .SetLineWidth(3)
                    .Rectangle(w / 2 - 100, h / 2 + 50, 200, 55).FillStroke()
                    .BeginText()
                    .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD), 26)
                    .SetFillColor(ColorConstants.RED)
                    .MoveText(w / 2 - 78, h / 2 + 70)
                    .ShowText("CERTIFICATE EXPIRED")
                    .EndText()
                    .RestoreState();
            }

            var boldFont   = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var italicFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

            var orgName = orgId == 2 ? "Royal Commission — Yanbu" : "Royal Commission — Jubail";

            // Organisation in top banner
            doc2.Add(new Paragraph(orgName)
                .SetFont(boldFont).SetFontSize(15)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(18).SetMarginBottom(0));

            // Main title
            doc2.Add(new Paragraph("Certificate of Completion")
                .SetFont(boldFont).SetFontSize(34)
                .SetFontColor(RcBlue)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(38).SetMarginBottom(4));

            doc2.Add(new Paragraph("This is to certify that")
                .SetFont(italicFont).SetFontSize(14)
                .SetFontColor(GreyText)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(0).SetMarginBottom(0));

            // Student name — prominent
            doc2.Add(new Paragraph(studentName)
                .SetFont(boldFont).SetFontSize(30)
                .SetFontColor(RcBlue)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(4).SetMarginBottom(4)
                .SetBorderBottom(new SolidBorder(RcGold, 2))
                .SetPaddingBottom(6));

            doc2.Add(new Paragraph("has successfully completed the course")
                .SetFont(italicFont).SetFontSize(14)
                .SetFontColor(GreyText)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(10).SetMarginBottom(2));

            // Course name
            doc2.Add(new Paragraph(courseName)
                .SetFont(boldFont).SetFontSize(20)
                .SetFontColor(DarkText)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(2).SetMarginBottom(0));

            // Date
            doc2.Add(new Paragraph($"Issued on  {date:dd MMMM yyyy}")
                .SetFont(normalFont).SetFontSize(12)
                .SetFontColor(GreyText)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(18));

            // Bottom banner text
            doc2.Add(new Paragraph("Training & Development Department  |  Royal Commission for Jubail & Yanbu")
                .SetFont(normalFont).SetFontSize(9)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(22));

            doc2.Close();
            return ms2.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating certificate");
            return Array.Empty<byte>();
        }
    }
}
