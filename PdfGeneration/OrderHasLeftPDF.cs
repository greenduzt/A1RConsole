using A1RConsole.Models.Dispatch;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.PdfGeneration
{
    public class OrderHasLeftPDF
    {
        private DispatchOrder dispatchOrder;
        private DateTime currentDate;
        private string fileName;
        private Document document;
        private TextFrame addressFrame;
        private MigraDoc.DocumentObjectModel.Tables.Table table;

        public OrderHasLeftPDF(DispatchOrder diso)
        {
            currentDate = DateTime.Now;
            dispatchOrder = diso;
        }


        public Tuple<Exception, string, string> CreateDeliveryDocket()
        {
            string path = string.Empty;
            Exception res = null;
            fileName = "ohl_" + dispatchOrder.SalesOrderNo + "_" + dispatchOrder.DeliveryDocketString + "_" + currentDate.ToString("_ddMMyyyy_HHmmss") + ".pdf";
            Document document = CreateDocument();
            document.UseCmykColor = true;
            document.DefaultPageSetup.FooterDistance = "-3.3cm";
            document.DefaultPageSetup.LeftMargin = "1.2cm";
            document.DefaultPageSetup.RightMargin = "1.2cm";
            const bool unicode = false;
            const PdfFontEmbedding embedding = PdfFontEmbedding.Always;
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();

            try
            {
                path = "S:/PRODUCTION/DONOTDELETE/OrderHasLeft/" + fileName;
                pdfRenderer.PdfDocument.Save(path);
                //ProcessStartInfo info = new ProcessStartInfo(path);
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
            return Tuple.Create(res, path, fileName);
        }

        public Document CreateDocument()
        {
            // Create a new MigraDoc document
            this.document = new Document();
            this.document.Info.Title = "A1 Rubber - Your Order Has Left";
            this.document.Info.Subject = "A1 Rubber - Your Order Has Left";
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
            //string shipTo = dispatchOrder.Customer.CompanyName + System.Environment.NewLine + dispatchOrder.ShipTo;
            //string billTo = dispatchOrder.Customer.CompanyName + System.Environment.NewLine + dispatchOrder.BillTo;

            string shipTo = dispatchOrder.Customer.CompanyName + System.Environment.NewLine + dispatchOrder.ShipTo;
            string billTo = dispatchOrder.Customer.CompanyName + System.Environment.NewLine + dispatchOrder.BillTo;
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
            this.addressFrame.Width = "15cm";
            this.addressFrame.Left = ShapePosition.Right;
            this.addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            this.addressFrame.Top = "-0.2cm";
            this.addressFrame.RelativeVertical = RelativeVertical.Page;

            paragraph = this.addressFrame.AddParagraph("YOUR ORDER HAS LEFT");
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

            //paragraph = this.addressFrame.AddParagraph("Date - " + DateTime.Now.ToString("dd/MM/yyy"));
            //paragraph.Format.Font.Name = "Calibri";
            //paragraph.Format.Font.Size = 12;
            ////paragraph.Format.SpaceBefore = "2cm";
            //paragraph.Format.Font.Bold = true;
            //paragraph.Format.Alignment = ParagraphAlignment.Right;

            //paragraph = this.addressFrame.AddParagraph("Printed Time - " + DateTime.Now.ToString("hh:mm tt"));
            //paragraph.Format.Font.Name = "Calibri";
            //paragraph.Format.Font.Size = 12;
            ////paragraph.Format.SpaceBefore = "2cm";
            //paragraph.Format.Font.Bold = true;
            //paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = this.addressFrame.AddParagraph("Delivery Docket No - " + dispatchOrder.DeliveryDocketString);
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = this.addressFrame.AddParagraph("Sales No - " + dispatchOrder.SalesOrderNo);
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 12;
            //paragraph.Format.SpaceBefore = "2cm";
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-1.7cm";
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

            rowTop1.Cells[0].AddParagraph("Ship To");
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

            rowTop1.Cells[2].AddParagraph("Bill To");
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

            rowTop2.Cells[0].AddParagraph(shipTo);
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

            rowTop2.Cells[2].AddParagraph(billTo);
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

            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Gray;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 4.0;
            this.table.BottomPadding = 1.0;
            this.table.Borders.Width = 1;
            this.table.Format.Alignment = ParagraphAlignment.Center;

            Column col2 = this.table.AddColumn("3cm");
            col2.Format.Alignment = ParagraphAlignment.Center;
            col2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            col2 = this.table.AddColumn("3cm");
            col2.Format.Alignment = ParagraphAlignment.Center;
            col2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            col2 = this.table.AddColumn("7.5cm");
            col2.Format.Alignment = ParagraphAlignment.Center;
            col2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            col2 = this.table.AddColumn("2.5cm");
            col2.Format.Alignment = ParagraphAlignment.Center;
            col2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            col2 = this.table.AddColumn("2.5cm");
            col2.Format.Alignment = ParagraphAlignment.Center;
            col2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row rowInfo1 = table.AddRow();
            rowInfo1.HeadingFormat = true;
            rowInfo1.Format.Alignment = ParagraphAlignment.Center;
            rowInfo1.Format.Font.Bold = true;
            rowInfo1.BottomPadding = 0;

            rowInfo1.Cells[0].AddParagraph("Con Note No");
            rowInfo1.Cells[0].Format.Font.Size = 10;
            rowInfo1.Cells[0].Format.Font.Bold = true;
            rowInfo1.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            rowInfo1.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowInfo1.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowInfo1.Cells[0].Borders.Left.Width = 0;
            rowInfo1.Cells[0].Borders.Right.Width = 0;
            rowInfo1.Cells[0].Borders.Top.Width = 0;
            rowInfo1.Cells[0].Borders.Bottom.Width = 0;

            rowInfo1.Cells[1].AddParagraph("Order No");
            rowInfo1.Cells[1].Format.Font.Size = 10;
            rowInfo1.Cells[1].Format.Font.Bold = true;
            rowInfo1.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            rowInfo1.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowInfo1.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowInfo1.Cells[1].Borders.Left.Width = 0;
            rowInfo1.Cells[1].Borders.Right.Width = 0;
            rowInfo1.Cells[1].Borders.Top.Width = 0;
            rowInfo1.Cells[1].Borders.Bottom.Width = 0;

            rowInfo1.Cells[2].AddParagraph("Freight Company");
            rowInfo1.Cells[2].Format.Font.Size = 10;
            rowInfo1.Cells[2].Format.Font.Bold = true;
            rowInfo1.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            rowInfo1.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowInfo1.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowInfo1.Cells[2].Borders.Left.Width = 0;
            rowInfo1.Cells[2].Borders.Right.Width = 0;
            rowInfo1.Cells[2].Borders.Top.Width = 0;
            rowInfo1.Cells[2].Borders.Bottom.Width = 0;

            rowInfo1.Cells[3].AddParagraph("Terms");
            rowInfo1.Cells[3].Format.Font.Size = 10;
            rowInfo1.Cells[3].Format.Font.Bold = true;
            rowInfo1.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            rowInfo1.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowInfo1.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowInfo1.Cells[3].Borders.Left.Width = 0;
            rowInfo1.Cells[3].Borders.Right.Width = 0;
            rowInfo1.Cells[3].Borders.Top.Width = 0;
            rowInfo1.Cells[3].Borders.Bottom.Width = 0;

            rowInfo1.Cells[4].AddParagraph("Date");
            rowInfo1.Cells[4].Format.Font.Size = 10;
            rowInfo1.Cells[4].Format.Font.Bold = true;
            rowInfo1.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            rowInfo1.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowInfo1.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowInfo1.Cells[4].Borders.Left.Width = 0;
            rowInfo1.Cells[4].Borders.Right.Width = 0;
            rowInfo1.Cells[4].Borders.Top.Width = 0;
            rowInfo1.Cells[4].Borders.Bottom.Width = 0;

            Row rowInfo2 = table.AddRow();
            rowInfo2.HeadingFormat = true;
            rowInfo2.Format.Alignment = ParagraphAlignment.Center;
            rowInfo2.Format.Font.Bold = true;
            rowInfo2.BottomPadding = 1;

            rowInfo2.Cells[0].AddParagraph(dispatchOrder.ConNoteNumber);
            rowInfo2.Cells[0].Format.Font.Size = 10;
            rowInfo2.Cells[0].Format.Font.Bold = false;
            rowInfo2.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            rowInfo2.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowInfo2.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            rowInfo2.Cells[1].AddParagraph(dispatchOrder.CustomerOrderNo);
            rowInfo2.Cells[1].Format.Font.Size = 10;
            rowInfo2.Cells[1].Format.Font.Bold = false;
            rowInfo2.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            rowInfo2.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowInfo2.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            rowInfo2.Cells[2].AddParagraph(dispatchOrder.FreightCarrier.FreightName);
            rowInfo2.Cells[2].Format.Font.Size = 10;
            rowInfo2.Cells[2].Format.Font.Bold = false;
            rowInfo2.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            rowInfo2.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowInfo2.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            rowInfo2.Cells[3].AddParagraph(DateTime.Now.ToString(dispatchOrder.TermsID));
            rowInfo2.Cells[3].Format.Font.Size = 10;
            rowInfo2.Cells[3].Format.Font.Bold = false;
            rowInfo2.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            rowInfo2.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowInfo2.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            rowInfo2.Cells[4].AddParagraph(DateTime.Now.ToString("dd/MM/yyyy"));
            rowInfo2.Cells[4].Format.Font.Size = 10;
            rowInfo2.Cells[4].Format.Font.Bold = false;
            rowInfo2.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            rowInfo2.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowInfo2.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.5cm";
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

            Column colItem = this.table.AddColumn("2cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem = this.table.AddColumn("2cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem = this.table.AddColumn("3.5cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem = this.table.AddColumn("9.5cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            colItem = this.table.AddColumn("1.5cm");
            colItem.Format.Alignment = ParagraphAlignment.Center;
            colItem.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row rowItem1 = table.AddRow();
            rowItem1.HeadingFormat = true;
            rowItem1.Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Format.Font.Bold = true;
            rowItem1.BottomPadding = 0;

            rowItem1.Cells[0].AddParagraph("Order Qty");
            rowItem1.Cells[0].Format.Font.Bold = true;
            rowItem1.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[0].Borders.Left.Width = 0;
            rowItem1.Cells[0].Borders.Right.Width = 0;
            rowItem1.Cells[0].Borders.Top.Width = 0;
            rowItem1.Cells[0].Borders.Bottom.Width = 0;

            rowItem1.Cells[1].AddParagraph("Shipped");
            rowItem1.Cells[1].Format.Font.Bold = true;
            rowItem1.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[1].Borders.Left.Width = 0;
            rowItem1.Cells[1].Borders.Right.Width = 0;
            rowItem1.Cells[1].Borders.Top.Width = 0;
            rowItem1.Cells[1].Borders.Bottom.Width = 0;

            rowItem1.Cells[2].AddParagraph("Product Code");
            rowItem1.Cells[2].Format.Font.Bold = true;
            rowItem1.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[2].Borders.Left.Width = 0;
            rowItem1.Cells[2].Borders.Right.Width = 0;
            rowItem1.Cells[2].Borders.Top.Width = 0;
            rowItem1.Cells[2].Borders.Bottom.Width = 0;

            rowItem1.Cells[3].AddParagraph("Description");
            rowItem1.Cells[3].Format.Font.Bold = true;
            rowItem1.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[3].Borders.Left.Width = 0;
            rowItem1.Cells[3].Borders.Right.Width = 0;
            rowItem1.Cells[3].Borders.Top.Width = 0;
            rowItem1.Cells[3].Borders.Bottom.Width = 0;

            rowItem1.Cells[4].AddParagraph("Unit");
            rowItem1.Cells[4].Format.Font.Bold = true;
            rowItem1.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            rowItem1.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            rowItem1.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            rowItem1.Cells[4].Borders.Left.Width = 0;
            rowItem1.Cells[4].Borders.Right.Width = 0;
            rowItem1.Cells[4].Borders.Top.Width = 0;
            rowItem1.Cells[4].Borders.Bottom.Width = 0;

            int rowsToIterate = 26;
            int listCount = dispatchOrder.DispatchOrderItem.Count;
            if (listCount > 26)
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
                row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
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


                for (int x = 0; x < dispatchOrder.DispatchOrderItem.Count; x++)
                {
                    if (i == x)
                    {
                        row.Cells[0].AddParagraph(GetQtyByUnit(dispatchOrder.DispatchOrderItem[x].OrderQty, dispatchOrder.DispatchOrderItem[x].Product.ProductUnit).ToString());
                        row.Cells[1].AddParagraph(GetQtyByUnit(dispatchOrder.DispatchOrderItem[x].OrderQty, dispatchOrder.DispatchOrderItem[x].Product.ProductUnit).ToString());
                        row.Cells[2].AddParagraph(dispatchOrder.DispatchOrderItem[x].Product.ProductCode);
                        row.Cells[3].AddParagraph(dispatchOrder.DispatchOrderItem[x].Product.ProductDescription);
                        row.Cells[4].AddParagraph(dispatchOrder.DispatchOrderItem[x].Product.ProductUnit);
                    }
                }
            }


            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

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

            Column col = this.table.AddColumn("9.25cm");
            col.Format.Alignment = ParagraphAlignment.Center;
            col.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            col = this.table.AddColumn("9.25cm");
            col.Format.Alignment = ParagraphAlignment.Center;
            col.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row row100 = table.AddRow();
            row100.HeadingFormat = true;
            row100.Format.Alignment = ParagraphAlignment.Center;
            row100.Format.Font.Bold = true;
            row100.BottomPadding = 1;

            row100.Cells[0].Format.Font.Bold = true;
            row100.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row100.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row100.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row100.Cells[0].Borders.Left.Width = 1;
            row100.Cells[0].Borders.Right.Width = 1;
            row100.Cells[0].Borders.Top.Width = 1;
            row100.Cells[0].Borders.Bottom.Width = 0;
            row100.Cells[0].MergeRight = 1;
            row100.Cells[0].AddParagraph("This document is confirmation that your order has left. Please advise us if you need further");

            Row row101 = table.AddRow();
            row101.HeadingFormat = true;
            row101.Format.Alignment = ParagraphAlignment.Center;
            row101.Format.Font.Bold = true;
            row101.BottomPadding = 1;

            row101.Cells[0].Format.Font.Bold = true;
            row101.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row101.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row101.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row101.Cells[0].Borders.Left.Width = 1;
            row101.Cells[0].Borders.Right.Width = 1;
            row101.Cells[0].Borders.Top.Width = 0;
            row101.Cells[0].Borders.Bottom.Width = 0;
            row101.Cells[0].MergeRight = 1;
            row101.Cells[0].AddParagraph("assistance or information on the delivery of your order, or alternatively you can contact the specified");

            Row row102 = table.AddRow();
            row102.HeadingFormat = true;
            row102.Format.Alignment = ParagraphAlignment.Center;
            row102.Format.Font.Bold = true;
            row102.BottomPadding = 1;

            row102.Cells[0].Format.Font.Bold = true;
            row102.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row102.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row102.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row102.Cells[0].Borders.Left.Width = 1;
            row102.Cells[0].Borders.Right.Width = 1;
            row102.Cells[0].Borders.Top.Width = 0;
            row102.Cells[0].Borders.Bottom.Width = 0;
            row102.Cells[0].MergeRight = 1;
            row102.Cells[0].AddParagraph("carrier and quote the above con note number in reference.");

            Row row103 = table.AddRow();
            row103.HeadingFormat = true;
            row103.Format.Alignment = ParagraphAlignment.Center;
            row103.Format.Font.Bold = true;
            row103.BottomPadding = 1;

            row103.Cells[0].Format.Font.Bold = true;
            row103.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row103.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row103.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row103.Cells[0].Borders.Left.Width = 1;
            row103.Cells[0].Borders.Right.Width = 1;
            row103.Cells[0].Borders.Top.Width = 1;
            row103.Cells[0].Borders.Bottom.Width = 0;
            row103.Cells[0].AddParagraph("A1 Rubber QLD Head Office & Manufacturing");

            row103.Cells[1].Format.Font.Bold = true;
            row103.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row103.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row103.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row103.Cells[1].Borders.Left.Width = 1;
            row103.Cells[1].Borders.Right.Width = 1;
            row103.Cells[1].Borders.Top.Width = 1;
            row103.Cells[1].Borders.Bottom.Width = 0;
            row103.Cells[1].AddParagraph("A1 Rubber NSW Warehouse");

            Row row104 = table.AddRow();
            row104.HeadingFormat = true;
            row104.Format.Alignment = ParagraphAlignment.Center;
            row104.Format.Font.Bold = true;
            row104.BottomPadding = 1;

            row104.Cells[0].Format.Font.Bold = true;
            row104.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row104.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row104.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row104.Cells[0].Borders.Left.Width = 1;
            row104.Cells[0].Borders.Right.Width = 1;
            row104.Cells[0].Borders.Top.Width = 0;
            row104.Cells[0].Borders.Bottom.Width = 0;
            row104.Cells[0].AddParagraph("Postal: PO Box 6278, Yatala QLD 4207");

            row104.Cells[1].Format.Font.Bold = true;
            row104.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row104.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row104.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row104.Cells[1].Borders.Left.Width = 1;
            row104.Cells[1].Borders.Right.Width = 1;
            row104.Cells[1].Borders.Top.Width = 0;
            row104.Cells[1].Borders.Bottom.Width = 0;
            row104.Cells[1].AddParagraph("Pickup: 40 Bentley Street, Wetherill Park NSW 2164");

            Row row105 = table.AddRow();
            row105.HeadingFormat = true;
            row105.Format.Alignment = ParagraphAlignment.Center;
            row105.Format.Font.Bold = true;
            row105.BottomPadding = 1;

            row105.Cells[0].Format.Font.Bold = true;
            row105.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row105.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row105.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row105.Cells[0].Borders.Left.Width = 1;
            row105.Cells[0].Borders.Right.Width = 1;
            row105.Cells[0].Borders.Top.Width = 0;
            row105.Cells[0].Borders.Bottom.Width = 0;
            row105.Cells[0].Borders.Width = 0;
            row105.Cells[0].AddParagraph("Factory/Pickup: Cnr Byte & Binary Sts, Yatala QLD 4207");

            row105.Cells[1].Format.Font.Bold = true;
            row105.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row105.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row105.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row105.Cells[1].Borders.Left.Width = 1;
            row105.Cells[1].Borders.Right.Width = 1;
            row105.Cells[1].Borders.Top.Width = 0;
            row105.Cells[1].Borders.Bottom.Width = 0;
            row105.Cells[1].AddParagraph("Ph: 02 9756 2146 Fax: 02 9756 2149");

            Row row106 = table.AddRow();
            row106.HeadingFormat = true;
            row106.Format.Alignment = ParagraphAlignment.Center;
            row106.Format.Font.Bold = true;
            row106.BottomPadding = 1;

            row106.Cells[0].Format.Font.Bold = true;
            row106.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row106.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row106.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row106.Cells[0].Borders.Left.Width = 1;
            row106.Cells[0].Borders.Right.Width = 1;
            row106.Cells[0].Borders.Top.Width = 0;
            row106.Cells[0].Borders.Bottom.Width = 1;
            row106.Cells[0].Borders.Width = 0;
            row106.Cells[0].AddParagraph("Ph: 07 3807 3666 Fax: 07 3807 2344");

            row106.Cells[1].Format.Font.Bold = true;
            row106.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row106.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row106.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row106.Cells[1].Borders.Left.Width = 1;
            row106.Cells[1].Borders.Right.Width = 1;
            row106.Cells[1].Borders.Top.Width = 0;
            row106.Cells[1].Borders.Bottom.Width = 1;
            row106.Cells[1].AddParagraph("");

            //Footer
            TextFrame textFrame2 = section.Footers.Primary.AddTextFrame();
            textFrame2.Width = "18cm";
            Paragraph footerPara2 = textFrame2.AddParagraph();
            footerPara2.AddText(DateTime.Now.ToString("dd/MM/yyy") + " " + DateTime.Now.ToString("hh:mm tt"));
            footerPara2.Format.Font.Size = 7;
            footerPara2.Format.Alignment = ParagraphAlignment.Left;

            //footerPara2 = textFrame2.AddParagraph();
            //footerPara2.AddText("");
            //footerPara2.Format.Font.Size = 7;
            //footerPara2.Format.Alignment = ParagraphAlignment.Center;

            //footerPara2 = textFrame2.AddParagraph();
            //footerPara2.AddText("");
            //footerPara2.Format.Font.Size = 7;
            //footerPara2.Format.Alignment = ParagraphAlignment.Center;

            TextFrame textFrame1 = section.Footers.Primary.AddTextFrame();
            textFrame1.Width = "18cm";
            Paragraph footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("©Copyright A1Rubber 2019");
            footerPara1.Format.Font.Size = 7;
            footerPara1.Format.Alignment = ParagraphAlignment.Right;

            //footerPara1 = textFrame1.AddParagraph();
            //footerPara1.AddText("©Copyright A1Rubber 2019");
            //footerPara1.Format.Font.Size = 7;
            //footerPara1.Format.Alignment = ParagraphAlignment.Right;

            footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("Page");
            footerPara1.AddPageField();
            footerPara1.AddText(" of ");
            footerPara1.AddNumPagesField();
            footerPara1.Format.Font.Size = 7;
            footerPara1.Format.Alignment = ParagraphAlignment.Center;



        }
        private decimal GetQtyByUnit(decimal q, string t)
        {
            decimal quantity = 0;

            if (t.ToLower() == "ea" || t.ToLower() == "tile" || t.ToLower() == "kg")
            {
                quantity = Math.Ceiling(q);
            }
            else if (t.ToLower() == "m2" || t.ToLower() == "roll")
            {
                quantity = Math.Round(q, 2);
            }
            return quantity;
        }
    }
}
