using A1RConsole.Models.Purchasing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MsgBox;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.PdfGeneration
{
    public class PrintPurchaseOrderPDF
    {
        private PurchaseOrder purchaseOrder;
        private DateTime currentDate;
        private string fileName;
        private Document document;
        private TextFrame addressFrame;
        private MigraDoc.DocumentObjectModel.Tables.Table table;

        public PrintPurchaseOrderPDF(PurchaseOrder po)
        {
            currentDate = DateTime.Now;
            purchaseOrder = po;
        }

        public Tuple<Exception, string> CreatePurcheOrderDoc()
        {
            Exception res = null;
            string filePath = string.Empty;
            fileName = "po_" + purchaseOrder.PurchasingOrderNo + "_" + currentDate.ToString("_ddMMyyyy_HHmmss") + ".pdf";
            Document document = CreateDocument();
            document.UseCmykColor = true;
            document.DefaultPageSetup.FooterDistance = "-5.6cm";
            document.DefaultPageSetup.LeftMargin = "1.2cm";
            document.DefaultPageSetup.RightMargin = "1.2cm";

            const bool unicode = false;
            const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();

            try
            {
                filePath = "S:/PRODUCTION/DONOTDELETE/PurchaseOrders/" + fileName;
                pdfRenderer.PdfDocument.Save(filePath);
                //ProcessStartInfo info = new ProcessStartInfo(filePath);
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
            File.Delete(Path.Combine(desktopPath, fileName));

            return Tuple.Create(res, filePath);
        }

        public Document CreateDocument()
        {
            // Create a new MigraDoc document
            this.document = new Document();
            this.document.Info.Title = "A1 Rubber Purchase Order";
            this.document.Info.Subject = "A1 Rubber Purchase Order";
            this.document.Info.Author = "A1 Rubber - Chamara Walaliyadde";

            DefineStyles();
            CreatePage();
            //FillContent();

            return this.document;
        }

        void DefineStyles()
        {

            MigraDoc.DocumentObjectModel.Style style = this.document.Styles["Normal"];
            style.Font.Name = "Helvatica";

            style = this.document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = this.document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("4cm", TabAlignment.Center);

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
            string to = purchaseOrder.PurchaseFrom;
            string shipTo = purchaseOrder.ShipTo;
            MigraDoc.DocumentObjectModel.Section section = this.document.AddSection();

            // Put a logo in the header
            MigraDoc.DocumentObjectModel.Shapes.Image image = section.Headers.Primary.AddImage("S:/PRODUCTION/QImg/a1rubber_logo.png");
            image.Height = "2.2cm";
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Left;
            image.WrapFormat.Style = WrapStyle.Through;

            Paragraph paragraph = section.AddParagraph();

            this.addressFrame = section.AddTextFrame();
            this.addressFrame.MarginRight = "0";
            this.addressFrame.Height = "3.0cm";
            this.addressFrame.Width = "7cm";
            this.addressFrame.Left = ShapePosition.Right;
            this.addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            this.addressFrame.Top = "-0.2cm";
            this.addressFrame.RelativeVertical = RelativeVertical.Page;

            paragraph = this.addressFrame.AddParagraph("PURCHASE ORDER");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 22;
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            paragraph = this.addressFrame.AddParagraph("ABN - 92 095 559 130");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = this.addressFrame.AddParagraph("Date - " + DateTime.Now.ToString("dd/MM/yyy"));
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 12;
            //paragraph.Format.SpaceBefore = "2cm";
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            //paragraph = this.addressFrame.AddParagraph("Printed Time - " + DateTime.Now.ToString("hh:mm tt"));
            //paragraph.Format.Font.Name = "Calibri";
            //paragraph.Format.Font.Size = 12;
            //paragraph.Format.Font.Bold = true;
            //paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = this.addressFrame.AddParagraph("Purchase Order No - " + purchaseOrder.PurchasingOrderNo);
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.3cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            //SHIP TO AND BILL TO
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Gray;
            this.table.Borders.Width = 0;
            this.table.Borders.Left.Width = 0;
            this.table.Borders.Right.Width = 0;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 4.0;
            this.table.BottomPadding = 1.0;

            Column columnTop1 = this.table.AddColumn("9cm");
            columnTop1.Format.Alignment = ParagraphAlignment.Center;
            columnTop1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            columnTop1 = this.table.AddColumn("0.5cm");
            columnTop1.Format.Alignment = ParagraphAlignment.Center;
            columnTop1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            columnTop1 = this.table.AddColumn("9cm");
            columnTop1.Format.Alignment = ParagraphAlignment.Center;
            columnTop1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row rowTop1 = table.AddRow();
            rowTop1.HeadingFormat = true;
            rowTop1.Format.Alignment = ParagraphAlignment.Center;
            rowTop1.Format.Font.Bold = true;
            rowTop1.BottomPadding = 1;

            rowTop1.Cells[0].AddParagraph("From");
            rowTop1.Cells[0].Format.Font.Size = 10;
            rowTop1.Cells[0].Format.Font.Bold = true;
            rowTop1.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowTop1.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowTop1.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            rowTop1.Cells[1].AddParagraph();
            rowTop1.Cells[1].Format.Font.Size = 10;
            rowTop1.Cells[1].Format.Font.Bold = true;
            rowTop1.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            rowTop1.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowTop1.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            rowTop1.Cells[2].AddParagraph("Ship To");
            rowTop1.Cells[2].Format.Font.Size = 10;
            rowTop1.Cells[2].Format.Font.Bold = true;
            rowTop1.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            rowTop1.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowTop1.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            Row rowTop2 = table.AddRow();
            rowTop2.HeadingFormat = true;
            rowTop2.Format.Alignment = ParagraphAlignment.Center;
            rowTop2.Format.Font.Bold = true;
            rowTop2.BottomPadding = 1;
            //rowTop2.Height = 60;

            rowTop2.Cells[0].AddParagraph(purchaseOrder.PurchaseFrom);
            rowTop2.Cells[0].Format.Font.Size = 10;
            rowTop2.Cells[0].Format.Font.Bold = true;
            rowTop2.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowTop2.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowTop2.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowTop2.Cells[0].Borders.Left.Width = 1;
            rowTop2.Cells[0].Borders.Right.Width = 1;
            rowTop2.Cells[0].Borders.Top.Width = 1;
            rowTop2.Cells[0].Borders.Bottom.Width = 1;

            rowTop2.Cells[1].AddParagraph();
            rowTop2.Cells[1].Format.Font.Size = 10;
            rowTop2.Cells[1].Format.Font.Bold = true;
            rowTop2.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            rowTop2.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowTop2.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            rowTop2.Cells[2].AddParagraph(purchaseOrder.ShipTo);
            rowTop2.Cells[2].Format.Font.Size = 10;
            rowTop2.Cells[2].Format.Font.Bold = true;
            rowTop2.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            rowTop2.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowTop2.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowTop2.Cells[2].Borders.Left.Width = 1;
            rowTop2.Cells[2].Borders.Right.Width = 1;
            rowTop2.Cells[2].Borders.Top.Width = 1;
            rowTop2.Cells[2].Borders.Bottom.Width = 1;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.3cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();


            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.3cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            //ITEM TABLE
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Format.Font.Size = 9;
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Gray;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 4.0;
            this.table.BottomPadding = 1.0;
            this.table.Borders.Width = 1;
            this.table.Format.Alignment = ParagraphAlignment.Center;

            Column colItem = this.table.AddColumn("1.5cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem = this.table.AddColumn("2cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem = this.table.AddColumn("7.5cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem = this.table.AddColumn("2cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem = this.table.AddColumn("2.5cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem = this.table.AddColumn("3cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row rowItem1 = table.AddRow();
            rowItem1.HeadingFormat = true;
            rowItem1.Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Format.Font.Bold = true;
            rowItem1.BottomPadding = 0;

            rowItem1.Cells[0].AddParagraph("Line No");
            rowItem1.Cells[0].Format.Font.Bold = true;
            rowItem1.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[0].Borders.Left.Width = 1;
            rowItem1.Cells[0].Borders.Right.Width = 1;
            rowItem1.Cells[0].Borders.Top.Width = 1;
            rowItem1.Cells[0].Borders.Bottom.Width = 1;

            rowItem1.Cells[1].AddParagraph("Qty");
            rowItem1.Cells[1].Format.Font.Bold = true;
            rowItem1.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[1].Borders.Left.Width = 1;
            rowItem1.Cells[1].Borders.Right.Width = 1;
            rowItem1.Cells[1].Borders.Top.Width = 1;
            rowItem1.Cells[1].Borders.Bottom.Width = 1;

            rowItem1.Cells[2].AddParagraph("Product Description");
            rowItem1.Cells[2].Format.Font.Bold = true;
            rowItem1.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[2].Borders.Left.Width = 1;
            rowItem1.Cells[2].Borders.Right.Width = 1;
            rowItem1.Cells[2].Borders.Top.Width = 1;
            rowItem1.Cells[2].Borders.Bottom.Width = 1;

            rowItem1.Cells[3].AddParagraph("Unit");
            rowItem1.Cells[3].Format.Font.Bold = true;
            rowItem1.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[3].Borders.Left.Width = 1;
            rowItem1.Cells[3].Borders.Right.Width = 1;
            rowItem1.Cells[3].Borders.Top.Width = 1;
            rowItem1.Cells[3].Borders.Bottom.Width = 1;

            rowItem1.Cells[4].AddParagraph("Unit Price");
            rowItem1.Cells[4].Format.Font.Bold = true;
            rowItem1.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[4].Borders.Left.Width = 1;
            rowItem1.Cells[4].Borders.Right.Width = 1;
            rowItem1.Cells[4].Borders.Top.Width = 1;
            rowItem1.Cells[4].Borders.Bottom.Width = 1;

            rowItem1.Cells[5].AddParagraph("Total");
            rowItem1.Cells[5].Format.Font.Bold = true;
            rowItem1.Cells[5].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[5].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[5].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[5].Borders.Left.Width = 1;
            rowItem1.Cells[5].Borders.Right.Width = 1;
            rowItem1.Cells[5].Borders.Top.Width = 1;
            rowItem1.Cells[5].Borders.Bottom.Width = 1;

            int rowsToIterate = 25;
            int listCount = purchaseOrder.PurchaseOrderDetails.Count;
            if (listCount > 25)
            {
                rowsToIterate = listCount;
            }


            for (int i = 0; i < rowsToIterate; i++)
            {
                Row row = table.AddRow();
                row.HeadingFormat = true;
                row.Format.Alignment = ParagraphAlignment.Center;
                row.Format.Font.Bold = true;
                row.BottomPadding = 1;

                row.Cells[0].Format.Font.Bold = true;
                row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row.Cells[0].Borders.Left.Width = 1;
                row.Cells[0].Borders.Right.Width = 1;
                row.Cells[0].Borders.Top.Width = 1;
                row.Cells[0].Borders.Bottom.Width = 1;

                row.Cells[1].Format.Font.Bold = true;
                row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row.Cells[1].Borders.Left.Width = 1;
                row.Cells[1].Borders.Right.Width = 1;
                row.Cells[1].Borders.Top.Width = 1;
                row.Cells[1].Borders.Bottom.Width = 1;

                row.Cells[2].Format.Font.Bold = true;
                row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row.Cells[2].Borders.Left.Width = 1;
                row.Cells[2].Borders.Right.Width = 1;
                row.Cells[2].Borders.Top.Width = 1;
                row.Cells[2].Borders.Bottom.Width = 1;

                row.Cells[3].Format.Font.Bold = true;
                row.Cells[3].Format.Alignment = ParagraphAlignment.Center;
                row.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row.Cells[3].Borders.Left.Width = 1;
                row.Cells[3].Borders.Right.Width = 1;
                row.Cells[3].Borders.Top.Width = 1;
                row.Cells[3].Borders.Bottom.Width = 1;

                row.Cells[4].Format.Font.Bold = true;
                row.Cells[4].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row.Cells[4].Borders.Left.Width = 1;
                row.Cells[4].Borders.Right.Width = 1;
                row.Cells[4].Borders.Top.Width = 1;
                row.Cells[4].Borders.Bottom.Width = 1;

                row.Cells[5].Format.Font.Bold = true;
                row.Cells[5].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[5].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row.Cells[5].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row.Cells[5].Borders.Left.Width = 1;
                row.Cells[5].Borders.Right.Width = 1;
                row.Cells[5].Borders.Top.Width = 1;
                row.Cells[5].Borders.Bottom.Width = 1;


                for (int x = 0; x < purchaseOrder.PurchaseOrderDetails.Count; x++)
                {
                    if (i == x)
                    {
                        row.Cells[0].AddParagraph(purchaseOrder.PurchaseOrderDetails[x].LineNo.ToString());
                        row.Cells[1].AddParagraph(GetQtyByUnit(purchaseOrder.PurchaseOrderDetails[x].OrderQty, purchaseOrder.PurchaseOrderDetails[x].Product.ProductUnit).ToString());
                        row.Cells[2].AddParagraph(purchaseOrder.PurchaseOrderDetails[x].Product.ProductDescription);
                        row.Cells[3].AddParagraph(purchaseOrder.PurchaseOrderDetails[x].Product.ProductUnit);
                        row.Cells[4].Format.Alignment = ParagraphAlignment.Right;
                        row.Cells[4].AddParagraph(purchaseOrder.PurchaseOrderDetails[x].Product.MaterialCost.ToString("C", CultureInfo.CurrentCulture));
                        row.Cells[5].Format.Alignment = ParagraphAlignment.Right;
                        row.Cells[5].AddParagraph(purchaseOrder.PurchaseOrderDetails[x].Total.ToString("C", CultureInfo.CurrentCulture));
                    }
                }

            }

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.3cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            //ITEM TABLE
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Format.Font.Size = 9;
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Gray;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 4.0;
            this.table.BottomPadding = 1.0;
            this.table.Borders.Width = 1;
            this.table.Format.Alignment = ParagraphAlignment.Center;

            Column colItem1 = this.table.AddColumn("1.5cm");
            colItem1.Format.Alignment = ParagraphAlignment.Center;
            colItem1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem1 = this.table.AddColumn("2cm");
            colItem1.Format.Alignment = ParagraphAlignment.Center;
            colItem1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem1 = this.table.AddColumn("7.5cm");
            colItem1.Format.Alignment = ParagraphAlignment.Center;
            colItem1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem1 = this.table.AddColumn("2cm");
            colItem1.Format.Alignment = ParagraphAlignment.Center;
            colItem1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem1 = this.table.AddColumn("2.5cm");
            colItem1.Format.Alignment = ParagraphAlignment.Center;
            colItem1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem1 = this.table.AddColumn("3cm");
            colItem1.Format.Alignment = ParagraphAlignment.Center;
            colItem1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row rowItem2 = table.AddRow();
            rowItem2.HeadingFormat = true;
            rowItem2.Format.Alignment = ParagraphAlignment.Center;
            rowItem2.Format.Font.Bold = true;
            rowItem2.BottomPadding = 0;

            rowItem2.Cells[0].AddParagraph("");
            rowItem2.Cells[0].Format.Font.Bold = true;
            rowItem2.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            rowItem2.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem2.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem2.Cells[0].Borders.Left.Width = 0;
            rowItem2.Cells[0].Borders.Right.Width = 0;
            rowItem2.Cells[0].Borders.Top.Width = 0;
            rowItem2.Cells[0].Borders.Bottom.Width = 0;
            rowItem2.Cells[0].MergeRight = 3;

            rowItem2.Cells[4].AddParagraph("Sub Total");
            rowItem2.Cells[4].Format.Font.Bold = true;
            rowItem2.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            rowItem2.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem2.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem2.Cells[4].Borders.Left.Width = 1;
            rowItem2.Cells[4].Borders.Right.Width = 1;
            rowItem2.Cells[4].Borders.Top.Width = 1;
            rowItem2.Cells[4].Borders.Bottom.Width = 1;

            rowItem2.Cells[5].AddParagraph(purchaseOrder.SubTotal.ToString("C", CultureInfo.CurrentCulture));
            rowItem2.Cells[5].Format.Font.Bold = true;
            rowItem2.Cells[5].Format.Alignment = ParagraphAlignment.Right;
            rowItem2.Cells[5].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem2.Cells[5].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem2.Cells[5].Borders.Left.Width = 1;
            rowItem2.Cells[5].Borders.Right.Width = 1;
            rowItem2.Cells[5].Borders.Top.Width = 1;
            rowItem2.Cells[5].Borders.Bottom.Width = 1;

            Row rowItem3 = table.AddRow();
            rowItem3.HeadingFormat = true;
            rowItem3.Format.Alignment = ParagraphAlignment.Center;
            rowItem3.Format.Font.Bold = true;
            rowItem3.BottomPadding = 0;

            rowItem3.Cells[0].AddParagraph(purchaseOrder.Notes);
            rowItem3.Cells[0].Format.Font.Bold = true;
            rowItem3.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowItem3.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem3.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem3.Cells[0].Borders.Left.Width = 0;
            rowItem3.Cells[0].Borders.Right.Width = 0;
            rowItem3.Cells[0].Borders.Top.Width = 0;
            rowItem3.Cells[0].Borders.Bottom.Width = 0;
            rowItem3.Cells[0].Format.Font.Size = 14;
            rowItem3.Cells[0].MergeRight = 3;
            rowItem3.Cells[0].MergeDown = 1;

            rowItem3.Cells[4].AddParagraph("GST");
            rowItem3.Cells[4].Format.Font.Bold = true;
            rowItem3.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            rowItem3.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem3.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem3.Cells[4].Borders.Left.Width = 1;
            rowItem3.Cells[4].Borders.Right.Width = 1;
            rowItem3.Cells[4].Borders.Top.Width = 1;
            rowItem3.Cells[4].Borders.Bottom.Width = 1;

            rowItem3.Cells[5].AddParagraph(purchaseOrder.Tax.ToString("C", CultureInfo.CurrentCulture));
            rowItem3.Cells[5].Format.Font.Bold = true;
            rowItem3.Cells[5].Format.Alignment = ParagraphAlignment.Right;
            rowItem3.Cells[5].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem3.Cells[5].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem3.Cells[5].Borders.Left.Width = 1;
            rowItem3.Cells[5].Borders.Right.Width = 1;
            rowItem3.Cells[5].Borders.Top.Width = 1;
            rowItem3.Cells[5].Borders.Bottom.Width = 1;

            Row rowItem4 = table.AddRow();
            rowItem4.HeadingFormat = true;
            rowItem4.Format.Alignment = ParagraphAlignment.Center;
            rowItem4.Format.Font.Bold = true;
            rowItem4.BottomPadding = 0;

            rowItem4.Cells[0].AddParagraph("");
            rowItem4.Cells[0].Format.Font.Bold = true;
            rowItem4.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            rowItem4.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem4.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem4.Cells[0].Borders.Left.Width = 0;
            rowItem4.Cells[0].Borders.Right.Width = 0;
            rowItem4.Cells[0].Borders.Top.Width = 0;
            rowItem4.Cells[0].Borders.Bottom.Width = 0;
            rowItem4.Cells[0].MergeRight = 3;

            rowItem4.Cells[4].AddParagraph("Grand Total");
            rowItem4.Cells[4].Format.Font.Bold = true;
            rowItem4.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            rowItem4.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem4.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem4.Cells[4].Borders.Left.Width = 1;
            rowItem4.Cells[4].Borders.Right.Width = 1;
            rowItem4.Cells[4].Borders.Top.Width = 1;
            rowItem4.Cells[4].Borders.Bottom.Width = 1;

            rowItem4.Cells[5].AddParagraph(purchaseOrder.TotalAmount.ToString("C", CultureInfo.CurrentCulture));
            rowItem4.Cells[5].Format.Font.Bold = true;
            rowItem4.Cells[5].Format.Alignment = ParagraphAlignment.Right;
            rowItem4.Cells[5].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem4.Cells[5].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem4.Cells[5].Borders.Left.Width = 1;
            rowItem4.Cells[5].Borders.Right.Width = 1;
            rowItem4.Cells[5].Borders.Top.Width = 1;
            rowItem4.Cells[5].Borders.Bottom.Width = 1;



            //Footer

            TextFrame textFrame2 = section.Footers.Primary.AddTextFrame();
            textFrame2.Width = "18cm";
            Paragraph footerPara2 = textFrame2.AddParagraph();
            footerPara2.AddText("Factory Address: 34 Binary Street, Yatala QLD 4207");
            footerPara2.Format.Font.Size = 8;
            footerPara2.Format.Alignment = ParagraphAlignment.Center;

            footerPara2 = textFrame2.AddParagraph();
            footerPara2.AddText("Postal Address: PO Box 6278, Yatala QLD 4207");
            footerPara2.Format.Font.Size = 8;
            footerPara2.Format.Alignment = ParagraphAlignment.Center;

            footerPara2 = textFrame2.AddParagraph();
            footerPara2.AddText("Ph: 07 3807 3666 Fax: 3807 2344");
            footerPara2.Format.Font.Size = 8;
            footerPara2.Format.Alignment = ParagraphAlignment.Center;

            TextFrame textFrame1 = section.Footers.Primary.AddTextFrame();
            textFrame1.Width = "18.2cm";
            Paragraph footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("www.a1rubber.com");
            footerPara1.Format.Font.Size = 8;
            footerPara1.Format.Alignment = ParagraphAlignment.Right;

            footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("©Copyright A1Rubber " + DateTime.Now.Year);
            footerPara1.Format.Font.Size = 8;
            footerPara1.Format.Alignment = ParagraphAlignment.Right;

            footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("Page");
            footerPara1.AddPageField();
            footerPara1.AddText(" of ");
            footerPara1.AddNumPagesField();
            footerPara1.Format.Font.Size = 8;
            footerPara1.Format.Alignment = ParagraphAlignment.Right;


            TextFrame textFrame3 = section.Footers.Primary.AddTextFrame();
            textFrame3.Width = "7.2cm";
            Paragraph footerPara3 = textFrame1.AddParagraph();
            footerPara3.AddText(String.Format("{0:g}", DateTime.Now));
            footerPara3.Format.Font.Size = 8;
            footerPara3.Format.Alignment = ParagraphAlignment.Left;

        }
        private string GetQtyByUnit(decimal q, string t)
        {
            decimal quantity = 0;

            if (string.Equals(t, "EA", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "TILE", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "kg", StringComparison.OrdinalIgnoreCase))
            {
                quantity = Math.Ceiling(q);
            }
            else if (string.Equals(t, "M2", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "ROLL", StringComparison.OrdinalIgnoreCase))
            {
                quantity = Math.Round(q, 2);
            }
            return string.Format("{0:0.0}", Math.Truncate(quantity * 10) / 10);
        }
    }
}
