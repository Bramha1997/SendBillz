using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SendBillz.Models;
using System.Collections.ObjectModel;

namespace SendBillz.Services
{
    public static class PdfGeneratorService
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
        /// <param name="storeAddress">Store/Company address</param>
        /// <param name="logoBytes">Logo image bytes (optional)</param>
        /// <param name="signImageBytes">Signature/hologram image bytes (optional)</param>
        /// <returns>True if PDF generated successfully; false otherwise</returns>
        public static async Task<bool> GenerateIndianInvoicePdfAsync(string filePath,
                                                                     SellerInfo seller,
                                                                     BuyerInfo buyer,
                                                                     string invoiceNumber,
                                                                     string invoiceDate,
                                                                     ObservableCollection<InvoiceItem> items,
                                                                     double totalAmount,
                                                                     string? storeName = null,
                                                                     string? storeAddress = null,
                                                                     byte[]? logoBytes = null,
                                                                     byte[]? signImageBytes = null)
        {
            try
            {
                await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);

                // Offload CPU-bound PDF creation to a background thread
                return await Task.Run(() =>
                {
                    var document = new PdfDocument();

                    var fontHeader = new XFont("NotoSansRegularFont", 20, XFontStyle.Bold);
                    var fontSubHeader = new XFont("NotoSansRegularFont", 12, XFontStyle.Bold);
                    var fontRegular = new XFont("NotoSansRegularFont", 10, XFontStyle.Regular);

                    PdfPage page = null!;
                    XGraphics gfx = null!;
                    double yPos = 0;
                    const double margin = 20;

                    void DrawSignOrHologram()
                    {
                        if (signImageBytes != null && signImageBytes.Length > 0)
                        {
                            using var ms = new MemoryStream(signImageBytes);
                            var signImage = XImage.FromStream(() => ms);

                            double signWidth = 80;
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

                        yPos = margin + 25; // Add 15 units of extra space above store name

                        if (!string.IsNullOrEmpty(storeName))
                        {
                            gfx.DrawString(storeName, fontHeader, XBrushes.Black, margin, yPos);
                            yPos += 25;

                            if (!string.IsNullOrEmpty(storeAddress))
                            {
                                var addressLines = storeAddress.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                                foreach (var line in addressLines)
                                {
                                    gfx.DrawString(line, fontRegular, XBrushes.Black, margin, yPos);
                                    yPos += 15;
                                }
                            }

                            yPos += 10; // Extra spacing after address
                        }

                        if (logoBytes != null && logoBytes.Length > 0)
                        {
                            using var ms = new MemoryStream(logoBytes);
                            var logoImage = XImage.FromStream(() => ms);
                            double logoWidth = 60;
                            double logoHeight = (logoImage.PixelHeight * logoWidth) / logoImage.PixelWidth;
                            gfx.DrawImage(logoImage, page.Width - margin - logoWidth, margin, logoWidth, logoHeight);
                        }

                        yPos += 20;
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

                    NewPage();

                    gfx.DrawString($"Seller: {seller.Name}", fontRegular, XBrushes.Black, margin, yPos);
                    gfx.DrawString($"GSTIN: {seller.Gstin}", fontRegular, XBrushes.Black, 300, yPos);
                    yPos += 15;

                    gfx.DrawString($"Buyer: {buyer.Name}", fontRegular, XBrushes.Black, margin, yPos);
                    gfx.DrawString($"GSTIN: {buyer.Gstin}", fontRegular, XBrushes.Black, 300, yPos);
                    yPos += 25;

                    gfx.DrawString($"Invoice No: {invoiceNumber}", fontRegular, XBrushes.Black, margin, yPos);
                    gfx.DrawString($"Date: {invoiceDate}", fontRegular, XBrushes.Black, 300, yPos);
                    yPos += 25;

                    DrawTableHeader();

                    foreach (var item in items)
                    {
                        if (yPos > page.Height - margin - 80)
                        {
                            NewPage();
                            DrawTableHeader();
                        }

                        var desc = item.Description;
                        var qty = item.Quantity;
                        var price = item.UnitPrice;
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

                    if (yPos > page.Height - margin - 80)
                    {
                        NewPage();
                    }

                    gfx.DrawString($"Grand Total: ₹{totalAmount:F2}", fontSubHeader, XBrushes.Black, margin, yPos + 20);
                    DrawSignOrHologram();

                    document.Save(stream);
                    return true;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF generation failed: {ex.Message}");
                return false;
            }
        }
    }
}
