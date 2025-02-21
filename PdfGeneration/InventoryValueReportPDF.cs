using A1RConsole.Models.Products;
using A1RConsole.Models.Stock;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.PdfGeneration
{
    public class InventoryValueReportPDF
    {
        private string headerString;
        Document document;
        TextFrame addressFrame;
        MigraDoc.DocumentObjectModel.Tables.Table table;
        private DateTime currentDate;
        private List<InventoryValue> inventoryValue;

        public InventoryValueReportPDF(string sl, List<InventoryValue> iv, Product product, ProductType productType, string commodityCode)
        {

            inventoryValue = iv;
            if (sl == "QLD")
            {
                headerString += "Stock Location : QLD Stock A1Rubber";
            }
            else
            {
                headerString += "Stock Location : NSW Stock A1Rubber";
            }

            if (product.ProductCode != null)
            {
                headerString += " | Product Code : " + product.ProductCode;
            }

            if (productType.Type != null)
            {
                headerString += " | Product Type : " + productType.Type;
            }

            if (!string.IsNullOrWhiteSpace(commodityCode))
            {
                headerString += " | Commodity Code : " + commodityCode;
            }

            currentDate = DateTime.Now;
        }

        public Tuple<Exception, string> InventoryValueReport()
        {
            Exception res = null;
            string filePath = string.Empty;
            Document document = CreateDocument();
            document.UseCmykColor = true;
            document.DefaultPageSetup.FooterDistance = "-4cm";
            document.DefaultPageSetup.LeftMargin = "1.2cm";
            document.DefaultPageSetup.RightMargin = "1.2cm";
            document.DefaultPageSetup.TopMargin = "3.4cm";

            const bool unicode = false;
            const PdfFontEmbedding embedding = PdfFontEmbedding.Always;
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();

            string filename = "IV" + "_" + currentDate.ToString("_ddMMyyyy_HHmmss") + ".pdf";
            try
            {
                filePath = "S:/PRODUCTION/DONOTDELETE/InventoryValue/" + filename;
                pdfRenderer.PdfDocument.Save(filePath);
                //ProcessStartInfo info = new ProcessStartInfo("S:/PRODUCTION/DONOTDELETE/InventoryValue/" + filename);
                //info.Verb = "Print";
                //info.CreateNoWindow = true;
                //info.WindowStyle = ProcessWindowStyle.Hidden;
                //Process.Start(info);
            }
            catch (Exception ex)
            {
                res = ex;
            }

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.Delete(Path.Combine(desktopPath, filename));

            return Tuple.Create(res, filePath);
        }

        public Document CreateDocument()
        {
            // Create a new MigraDoc document
            this.document = new Document();
            this.document.Info.Title = "Vehicle Parts";
            this.document.Info.Subject = "Vehicle Parts";
            this.document.Info.Author = "Chamara Walaliyadde";

            DefineStyles();
            CreatePage();

            return this.document;
        }

        void DefineStyles()
        {
            // Get the predefined style Normal.
            MigraDoc.DocumentObjectModel.Style style = this.document.Styles["Normal"];

            style.Font.Name = "Helvatica";

            style = this.document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = this.document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called Table based on style Normal
            style = this.document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Helvatica";
            style.Font.Size = 9;

            // Create a new style called Reference based on style Normal
            style = this.document.Styles.AddStyle("Reference", "Normal");
            style.ParagraphFormat.SpaceBefore = "5mm";
            style.ParagraphFormat.SpaceAfter = "5mm";
            style.ParagraphFormat.TabStops.AddTabStop("16cm", TabAlignment.Right);
        }

        void CreatePage()
        {
            MigraDoc.DocumentObjectModel.Section section = this.document.AddSection();
            MigraDoc.DocumentObjectModel.Paragraph paragraph = section.AddParagraph();

            MigraDoc.DocumentObjectModel.Shapes.Image image = section.Headers.Primary.AddImage("S:/PRODUCTION/QImg/a1rubber_logo_small.png");
            image.Height = "2cm";
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Left;
            image.WrapFormat.Style = WrapStyle.Through;


            this.addressFrame = section.AddTextFrame();
            this.addressFrame.MarginRight = "0";
            this.addressFrame.Height = "3.5cm";
            this.addressFrame.Width = "18.6cm";
            this.addressFrame.Left = ShapePosition.Right;
            this.addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            this.addressFrame.RelativeVertical = RelativeVertical.Page;
            this.addressFrame.MarginTop = "-0.1cm";

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            paragraph = this.addressFrame.AddParagraph("Inventory Value");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 17;
            paragraph.Format.SpaceBefore = "2cm";
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            //paragraph = section.AddParagraph();
            //paragraph.Format.SpaceBefore = "-2cm";
            //paragraph.Style = "Reference";
            //paragraph.AddTab();

            //paragraph = this.addressFrame.AddParagraph("Date - " + currentDate.ToString("dd/MM/yyyy") + " at " + DateTime.Now.ToString("hh:mm tt"));
            //paragraph.Format.Font.Name = "Calibri";
            //paragraph.Format.Font.Size = 11;
            //paragraph.Format.Font.Bold = true;
            //paragraph.Format.SpaceBefore = "2cm";
            //paragraph.Format.Alignment = ParagraphAlignment.Left;

            paragraph = section.AddParagraph(headerString);
            paragraph.Format.SpaceBefore = "-1cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            //Footer page numbers
            TextFrame textFrame2 = section.Footers.Primary.AddTextFrame();
            textFrame2.Width = "18cm";
            textFrame2.Top = "10cm";
            Paragraph footerPara2 = textFrame2.AddParagraph();
            footerPara2.AddText("Printed Date/Time - " + currentDate.ToString("dd/MM/yyyy") + " at " + DateTime.Now.ToString("hh:mm tt"));
            footerPara2.Format.Font.Size = 7;
            footerPara2.Format.Alignment = ParagraphAlignment.Left;

            footerPara2 = textFrame2.AddParagraph();
            footerPara2.AddText("");
            footerPara2.Format.Font.Size = 7;
            footerPara2.Format.Alignment = ParagraphAlignment.Left;

            TextFrame textFrame1 = section.Footers.Primary.AddTextFrame();
            textFrame1.Width = "18.2cm";
            Paragraph footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("www.a1rubber.com");
            footerPara1.Format.Font.Size = 7;
            footerPara1.Format.Alignment = ParagraphAlignment.Right;

            footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("©Copyright A1Rubber 2019");
            footerPara1.Format.Font.Size = 7;
            footerPara1.Format.Alignment = ParagraphAlignment.Right;

            footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("Page");
            footerPara1.AddPageField();
            footerPara1.AddText(" of ");
            footerPara1.AddNumPagesField();
            footerPara1.Format.Font.Size = 7;
            footerPara1.Format.Alignment = ParagraphAlignment.Center;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            //Table for items
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            this.table.Borders.Width = 0.25;
            this.table.Borders.Left.Width = 0.5;
            this.table.Borders.Right.Width = 0.5;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 1.0;
            this.table.BottomPadding = 1.0;

            Column itemsCol = this.table.AddColumn("3.50cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("7.5cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("2cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("2cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("2.5cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row itemRow = table.AddRow();
            itemRow.HeadingFormat = true;
            itemRow.Format.Alignment = ParagraphAlignment.Center;
            itemRow.Format.Font.Bold = true;

            itemRow.Cells[0].AddParagraph("Product Code");
            itemRow.Cells[0].Format.Font.Size = 9;
            itemRow.Cells[0].Format.Font.Bold = true;
            itemRow.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[1].AddParagraph("Product Description");
            itemRow.Cells[1].Format.Font.Size = 9;
            itemRow.Cells[1].Format.Font.Bold = true;
            itemRow.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[2].AddParagraph("Location");
            itemRow.Cells[2].Format.Font.Size = 9;
            itemRow.Cells[2].Format.Font.Bold = true;
            itemRow.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[3].AddParagraph("QTY On Hand");
            itemRow.Cells[3].Format.Font.Size = 9;
            itemRow.Cells[3].Format.Font.Bold = true;
            itemRow.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[4].AddParagraph("Total Value");
            itemRow.Cells[4].Format.Font.Size = 9;
            itemRow.Cells[4].Format.Font.Bold = true;
            itemRow.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;


            foreach (var item in inventoryValue)
            {
                Row row2 = table.AddRow();
                row2.HeadingFormat = true;
                row2.Format.Alignment = ParagraphAlignment.Center;
                row2.Format.Font.Bold = false;
                row2.BottomPadding = 1;

                row2.Cells[0].AddParagraph(item.ProductCode);
                row2.Cells[0].Format.Font.Size = 9;
                row2.Cells[0].Format.Font.Bold = true;
                row2.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                row2.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[0].Borders.Width = 0;

                row2.Cells[1].AddParagraph(item.ProductDescription);
                row2.Cells[1].Format.Font.Size = 9;
                row2.Cells[1].Format.Font.Bold = true;
                row2.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                row2.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[1].Borders.Width = 0;

                row2.Cells[2].AddParagraph(item.StockLocation.StockName);
                row2.Cells[2].Format.Font.Size = 9;
                row2.Cells[2].Format.Font.Bold = true;
                row2.Cells[2].Format.Alignment = ParagraphAlignment.Center;
                row2.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[2].Borders.Width = 0;

                row2.Cells[3].AddParagraph(string.Format("{0:0.0}", Math.Truncate(item.QtyOnHand * 10) / 10));
                row2.Cells[3].Format.Font.Size = 9;
                row2.Cells[3].Format.Font.Bold = true;
                row2.Cells[3].Format.Alignment = ParagraphAlignment.Right;
                row2.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[3].Borders.Width = 0;

                row2.Cells[4].AddParagraph(String.Format("{0:C}", item.TotalValue));
                row2.Cells[4].Format.Font.Size = 9;
                row2.Cells[4].Format.Font.Bold = true;
                row2.Cells[4].Format.Alignment = ParagraphAlignment.Right;
                row2.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[4].Borders.Width = 0;

            }

            decimal totVal = inventoryValue.Sum(x => x.TotalValue);

            Row row3 = table.AddRow();
            row3.HeadingFormat = true;
            row3.Format.Alignment = ParagraphAlignment.Center;
            row3.Format.Font.Bold = false;
            row3.BottomPadding = 1;

            row3.Cells[0].AddParagraph("Inventory Value : " + String.Format("{0:C}", totVal));
            row3.Cells[0].Format.Font.Size = 12;
            row3.Cells[0].Format.Font.Bold = true;
            row3.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row3.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row3.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row3.Cells[0].MergeRight = 4;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

        }

    }
}

