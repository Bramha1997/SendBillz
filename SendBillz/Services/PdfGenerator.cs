using SendBillz.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Collections.ObjectModel;
using System.IO;

namespace SendBillz.Services
{
    public static class PdfGenerator
    {
        /// <summary>
        /// Generates an Indian invoice PDF as per the described requirements.
        /// </summary>
        /// <param name="filePath">File path for the PDF output</param>
        /// <param name="seller">Seller details</param>
        /// <param name="buyer">Buyer details</param>
        /// <param name="invoiceNumber">Invoice number string</param>
        /// <param name="invoiceDate">Invoice date string</param>
        /// <param name="items">Invoice items</param>
        /// <param name="totalAmount">Grand total</param>
        /// <param name="storeName">Store/Company name (optional)</param>
        /// <param name="logoBytes">Logo image bytes (optional)</param>
        /// <param name="signImageBytes">Signature/hologram image bytes (optional)</param>
        /// <returns>True if PDF generated successfully; false otherwise</returns>
        public static bool GenerateIndianInvoicePdfAsync(
            string filePath,
            SellerInfo seller,
            BuyerInfo buyer,
            string invoiceNumber,
            string invoiceDate,
            ObservableCollection<InvoiceItem> items,
            double totalAmount,
            string? storeName = null,
            byte[]? logoBytes = null,
            byte[]? signImageBytes = null)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Create);
                var document = new PdfDocument();

                // Define fonts (ensure you have this font or replace with another, e.g., "Arial")
                var fontHeader = new XFont("NotoSansRegularFont", 20, XFontStyle.Bold);
                var fontSubHeader = new XFont("NotoSansRegularFont", 12, XFontStyle.Bold);
                var fontRegular = new XFont("NotoSansRegularFont", 10, XFontStyle.Regular);

                PdfPage page = null!;
                XGraphics gfx = null!;
                double yPos = 0;
                const double margin = 20;

                void DrawSignOrHologram()
                {
                    if (signImageBytes != null)
                    {
                        using var ms = new MemoryStream(signImageBytes);
                        var signImage = XImage.FromStream(() => ms);

                        double signWidth = 80; // Adjust as desired
                        double signHeight = (signImage.PixelHeight * signWidth) / signImage.PixelWidth;

                        double xPos = page.Width - margin - signWidth;
                        double yPosSign = page.Height - margin - signHeight;

                        gfx.DrawImage(signImage, xPos, yPosSign, signWidth, signHeight);
                    }
                }

                void NewPage()
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPos = margin;

                    // Draw Store Name centered at top (if provided)
                    if (!string.IsNullOrEmpty(storeName))
                    {
                        var storeNameSize = gfx.MeasureString(storeName, fontHeader);
                        gfx.DrawString(storeName, fontHeader, XBrushes.Black,
                            new XRect(0, yPos, page.Width, storeNameSize.Height),
                            XStringFormats.TopCenter);
                    }

                    // Draw Logo at top right corner (if provided)
                    if (logoBytes != null)
                    {
                        using var ms = new MemoryStream(logoBytes);
                        var logoImage = XImage.FromStream(() => ms);
                        double logoWidth = 60; // Adjust as desired
                        double logoHeight = (logoImage.PixelHeight * logoWidth) / logoImage.PixelWidth;
                        gfx.DrawImage(logoImage, page.Width - margin - logoWidth, yPos, logoWidth, logoHeight);
                    }

                    yPos += 50; // space after header and logo

                    // Draw signature/hologram image immediately to ensure it's present on every page
                    //DrawSignOrHologram();
                }

                void DrawTableHeader()
                {
                    gfx.DrawString("Description", fontSubHeader, XBrushes.DarkGreen, margin, yPos);
                    gfx.DrawString("Qty", fontSubHeader, XBrushes.DarkGreen, 200, yPos);
                    gfx.DrawString("Unit Price", fontSubHeader, XBrushes.DarkGreen, 250, yPos);
                    gfx.DrawString("Discount", fontSubHeader, XBrushes.DarkGreen, 340, yPos);
                    gfx.DrawString("GST", fontSubHeader, XBrushes.DarkGreen, 410, yPos);
                    gfx.DrawString("Total", fontSubHeader, XBrushes.DarkGreen, 490, yPos);
                    yPos += 25;
                }

                // -- Start first page
                NewPage();

                // Seller and Buyer Details
                gfx.DrawString($"Seller: {seller.Name}", fontRegular, XBrushes.Black, margin, yPos);
                gfx.DrawString($"GSTIN: {seller.Gstin}", fontRegular, XBrushes.Black, 300, yPos);
                yPos += 15;

                gfx.DrawString($"Buyer: {buyer.Name}", fontRegular, XBrushes.Black, margin, yPos);
                gfx.DrawString($"GSTIN: {buyer.Gstin}", fontRegular, XBrushes.Black, 300, yPos);
                yPos += 25;

                gfx.DrawString($"Invoice No: {invoiceNumber}", fontRegular, XBrushes.Black, margin, yPos);
                gfx.DrawString($"Date: {invoiceDate}", fontRegular, XBrushes.Black, 300, yPos);
                yPos += 25;

                // -- Table Header
                DrawTableHeader();

                // -- Items Loop
                foreach (var item in items)
                {
                    // If content goes beyond bottom margin, start a new page
                    if (yPos > page.Height - margin - 80)
                    {
                        NewPage();

                        // Seller/Buyer/Invoice info is not repeated -- only table header
                        DrawTableHeader();
                    }

                    var desc = item.Description;
                    var qty = item.Quantity;
                    var price = item.UnitPrice;
                    // Calculate discount: expects Discount is percent per unit (e.g., 5 for 5%)
                    var discAmmount = qty * price * ((item.Discount) / 100.0);
                    var gstRate = item.GstRate;
                    var amount = qty * price - discAmmount;
                    var gstAmt = amount * (gstRate / 100);
                    var totalItem = amount + gstAmt;

                    gfx.DrawString(desc, fontRegular, XBrushes.Black, margin, yPos);
                    gfx.DrawString(qty.ToString(), fontRegular, XBrushes.Black, 200, yPos);
                    gfx.DrawString($"₹{price:F2}", fontRegular, XBrushes.Black, 250, yPos);
                    gfx.DrawString($"₹{discAmmount:F2}", fontRegular, XBrushes.Black, 340, yPos);
                    gfx.DrawString($"₹{gstAmt:F2}", fontRegular, XBrushes.Black, 410, yPos);
                    gfx.DrawString($"₹{totalItem:F2}", fontRegular, XBrushes.Black, 490, yPos);
                    yPos += 20;
                }

                // -- Grand Total (on "current" or a new page if insufficient space)
                if (yPos > page.Height - margin - 80)
                {
                    NewPage();
                    // Optionally redraw table header (not essential if only total is shown)
                }
                gfx.DrawString($"Grand Total: ₹{totalAmount:F2}", fontSubHeader, XBrushes.Black, margin, yPos + 20);

                // (Optional) To show signature _only_ on the last page and not on all pages,
                // Comment out DrawSignOrHologram() in NewPage()
                // and insert here before saving the document:
                DrawSignOrHologram();

                // Save document
                document.Save(stream);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF generation failed: {ex.Message}");
                return false;
            }
        }
    }
}
