using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
//using System;
//using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using A1RConsole.Models.Orders;
using A1RConsole.Core;

namespace A1RConsole.PdfGeneration
{
    public class PrintInvoicePDF
    {
        Document document;
        TextFrame addressFrame;
        Table table;
        public SalesOrder salesOrder;
        public DateTime currentDate;

        public PrintInvoicePDF(SalesOrder so)
        {
            salesOrder = so;
            currentDate = DateTime.Now;
        }

        public Tuple<Exception, string> CreateInvoice()
        {

            Exception res = null;
            string filePath = string.Empty;
            Document document = CreateDocument();
            document.UseCmykColor = true;
            document.DefaultPageSetup.FooterDistance = "-4cm";
            document.DefaultPageSetup.LeftMargin = "1.2cm";
            document.DefaultPageSetup.RightMargin = "1.2cm";
            document.DefaultPageSetup.TopMargin = "3.2cm";

            const bool unicode = false;
            const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();
            InvoicingManager im = new InvoicingManager();

            string filename = "INV" + salesOrder.InvoiceNo + "_" + im.Get8Digits() + ".pdf";
            try
            {
                filePath = "S:/PRODUCTION/DONOTDELETE/Invoices/" + filename;
                pdfRenderer.PdfDocument.Save(filePath);
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
            this.document.Info.Title = "Tax Invoice";
            this.document.Info.Subject = "Tax Invoice";
            this.document.Info.Author = "Chamara Walaliyadde";

            DefineStyles();
            CreatePage();


            // FillContent();

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
            MigraDoc.DocumentObjectModel.Shapes.Image image = section.Headers.Primary.AddImage("S:/PRODUCTION/QImg/a1rubber_logo.png");
            image.Height = "3cm";
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Left;
            image.WrapFormat.Style = WrapStyle.Through;

            MigraDoc.DocumentObjectModel.Paragraph paragraph = section.AddParagraph();

            this.addressFrame = section.AddTextFrame();
            this.addressFrame.MarginRight = "0";
            this.addressFrame.Height = "3.5cm";
            this.addressFrame.Width = "7cm";
            this.addressFrame.Left = ShapePosition.Right;
            this.addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            this.addressFrame.RelativeVertical = RelativeVertical.Page;
            this.addressFrame.MarginTop = "-1.5cm";

            paragraph = this.addressFrame.AddParagraph("Tax Invoice");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 30;
            paragraph.Format.SpaceBefore = "2cm";
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = this.addressFrame.AddParagraph("www.a1rubber.com");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Bold = false;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = this.addressFrame.AddParagraph("ABN 92 095 559 130");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Bold = false;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            paragraph = this.addressFrame.AddParagraph(salesOrder.InvoiceNo > 0 ? "INVOICE NO : " + salesOrder.InvoiceNo : "");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 15;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = this.addressFrame.AddParagraph(salesOrder.DispatchOrder.DeliveryDocketNo > 0 ? "Delivery Docket No: " + salesOrder.DispatchOrder.DeliveryDocketNo : "");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 12;
            //paragraph.Format.SpaceBefore = "2cm";
            paragraph.Format.Font.Bold = false;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = this.addressFrame.AddParagraph("Sales Order No: " + salesOrder.SalesOrderNo);
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Bold = false;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            paragraph = this.addressFrame.AddParagraph("Payment Due Date: " + salesOrder.PaymentDueDate.ToString("dd/MM/yyyy"));
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 15;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-2cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            //Footer page numbers
            TextFrame textFrame2 = section.Footers.Primary.AddTextFrame();
            textFrame2.Width = "18cm";
            textFrame2.Top = "10cm";
            Paragraph footerPara2 = textFrame2.AddParagraph();
            footerPara2.AddText("Printed Date - " + DateTime.Now.ToString("dd/MM/yyy"));
            footerPara2.Format.Font.Size = 7;
            footerPara2.Format.Alignment = ParagraphAlignment.Left;

            footerPara2 = textFrame2.AddParagraph();
            footerPara2.AddText("Printed Time - " + DateTime.Now.ToString("hh:mm tt"));
            footerPara2.Format.Font.Size = 7;
            footerPara2.Format.Alignment = ParagraphAlignment.Left;

            TextFrame textFrame1 = section.Footers.Primary.AddTextFrame();
            textFrame1.Width = "18.2cm";
            Paragraph footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("www.a1rubber.com");
            footerPara1.Format.Font.Size = 7;
            footerPara1.Format.Alignment = ParagraphAlignment.Right;

            footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("©Copyright A1Rubber " + DateTime.Now.Year);
            footerPara1.Format.Font.Size = 7;
            footerPara1.Format.Alignment = ParagraphAlignment.Right;

            footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("Page");
            footerPara1.AddPageField();
            footerPara1.AddText(" of ");
            footerPara1.AddNumPagesField();
            footerPara1.Format.Font.Size = 7;
            footerPara1.Format.Alignment = ParagraphAlignment.Center;

            // Create the Top table
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Format.Font.Size = 9;
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 4.0;
            this.table.BottomPadding = 1.0;
            this.table.Borders.Width = 1;
            this.table.Format.Alignment = ParagraphAlignment.Center;


            Column column1 = this.table.AddColumn("2cm");
            column1.Format.Alignment = ParagraphAlignment.Center;
            column1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column1 = this.table.AddColumn("7cm");
            column1.Format.Alignment = ParagraphAlignment.Center;
            column1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column1 = this.table.AddColumn("2cm");
            column1.Format.Alignment = ParagraphAlignment.Center;
            column1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column1 = this.table.AddColumn("7cm");
            column1.Format.Alignment = ParagraphAlignment.Center;
            column1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row row1 = table.AddRow();
            row1.HeadingFormat = true;
            row1.Format.Alignment = ParagraphAlignment.Center;
            row1.Format.Font.Bold = true;
            row1.BottomPadding = 0;

            row1.Cells[0].AddParagraph("");
            row1.Cells[0].Format.Font.Bold = true;
            row1.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row1.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row1.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row1.Cells[0].Borders.Left.Width = 0;
            row1.Cells[0].Borders.Right.Width = 0;
            row1.Cells[0].Borders.Top.Width = 0;
            row1.Cells[0].Borders.Bottom.Width = 0;

            row1.Cells[1].AddParagraph("Bill To");
            row1.Cells[1].Format.Font.Size = 12;
            row1.Cells[1].Format.Font.Bold = true;
            row1.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row1.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row1.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row1.Cells[1].Borders.Left.Width = 0;
            row1.Cells[1].Borders.Top.Width = 0;
            row1.Cells[1].Borders.Right.Width = 0;

            row1.Cells[2].AddParagraph("");
            row1.Cells[2].Format.Font.Bold = true;
            row1.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            row1.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row1.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row1.Cells[2].Borders.Left.Width = 0;
            row1.Cells[2].Borders.Right.Width = 0;
            row1.Cells[2].Borders.Top.Width = 0;
            row1.Cells[2].Borders.Bottom.Width = 0;

            row1.Cells[3].AddParagraph("Shipped To");
            row1.Cells[3].Format.Font.Size = 12;
            row1.Cells[3].Format.Font.Bold = true;
            row1.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            row1.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row1.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row1.Cells[3].Borders.Left.Width = 0;
            row1.Cells[3].Borders.Top.Width = 0;
            row1.Cells[3].Borders.Right.Width = 0;

            Row row2 = table.AddRow();
            row2.HeadingFormat = true;
            row2.Format.Alignment = ParagraphAlignment.Center;
            row2.Format.Font.Bold = true;

            row2.Cells[0].AddParagraph("");
            row2.Cells[0].Format.Font.Bold = true;
            row2.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row2.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row2.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row2.Cells[0].Borders.Left.Width = 0;
            row2.Cells[0].Borders.Top.Width = 0;
            row2.Cells[0].Borders.Bottom.Width = 0;

            string name = string.Empty;
            if (salesOrder.Customer.CustomerType == "Account")
            {
                name = salesOrder.Customer.CompanyName;
            }
            else if (salesOrder.Customer.CustomerType == "Prepaid" && !string.IsNullOrWhiteSpace(salesOrder.Customer.CompanyName))
            {
                name = salesOrder.Customer.CompanyName;
            }
            else
            {
                name = salesOrder.PrepaidCustomerName;
            }
            row2.Cells[1].AddParagraph(name + System.Environment.NewLine + salesOrder.BillTo);
            row2.Cells[1].Format.Font.Size = 12;
            row2.Cells[1].Format.Font.Bold = true;
            row2.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row2.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row2.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            row2.Cells[2].AddParagraph("");
            row2.Cells[2].Format.Font.Bold = true;
            row2.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            row2.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row2.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row2.Cells[2].Borders.Top.Width = 0;
            row2.Cells[2].Borders.Bottom.Width = 0;

            row2.Cells[3].AddParagraph(name + System.Environment.NewLine + salesOrder.ShipTo);
            row2.Cells[3].Format.Font.Size = 12;
            row2.Cells[3].Format.Font.Bold = true;
            row2.Cells[3].Format.Alignment = ParagraphAlignment.Left;
            row2.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row2.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            // Create the Table2
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Format.Font.Size = 9;
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 4.0;
            this.table.BottomPadding = 1.0;
            this.table.Borders.Width = 1;
            this.table.Format.Alignment = ParagraphAlignment.Center;


            Column column2 = this.table.AddColumn("4.8cm");
            column2.Format.Alignment = ParagraphAlignment.Center;
            column2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column2 = this.table.AddColumn("3cm");
            column2.Format.Alignment = ParagraphAlignment.Center;
            column2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column2 = this.table.AddColumn("5cm");
            column2.Format.Alignment = ParagraphAlignment.Center;
            column2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column2 = this.table.AddColumn("3cm");
            column2.Format.Alignment = ParagraphAlignment.Center;
            column2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column2 = this.table.AddColumn("3cm");
            column2.Format.Alignment = ParagraphAlignment.Center;
            column2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row row3 = table.AddRow();
            row3.HeadingFormat = true;
            row3.Format.Alignment = ParagraphAlignment.Center;
            row3.Format.Font.Bold = true;
            row3.BottomPadding = 0;

            row3.Cells[0].AddParagraph("Sales Person");
            row3.Cells[0].Format.Font.Bold = true;
            row3.Cells[0].Format.Font.Size = 12;
            row3.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row3.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row3.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row3.Cells[0].Borders.Left.Width = 0;
            row3.Cells[0].Borders.Right.Width = 0;
            row3.Cells[0].Borders.Top.Width = 0;
            row3.Cells[0].Borders.Bottom.Width = 1;

            row3.Cells[1].AddParagraph("Order No");
            row3.Cells[1].Format.Font.Size = 12;
            row3.Cells[1].Format.Font.Bold = true;
            row3.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row3.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row3.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row3.Cells[1].Borders.Left.Width = 0;
            row3.Cells[1].Borders.Top.Width = 0;
            row3.Cells[1].Borders.Right.Width = 0;
            row3.Cells[1].Borders.Bottom.Width = 1;

            row3.Cells[2].AddParagraph("Freight Company");
            row3.Cells[2].Format.Font.Size = 12;
            row3.Cells[2].Format.Font.Bold = true;
            row3.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            row3.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row3.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row3.Cells[2].Borders.Left.Width = 0;
            row3.Cells[2].Borders.Right.Width = 0;
            row3.Cells[2].Borders.Top.Width = 0;
            row3.Cells[2].Borders.Bottom.Width = 1;

            row3.Cells[3].AddParagraph("Terms");
            row3.Cells[3].Format.Font.Size = 12;
            row3.Cells[3].Format.Font.Bold = true;
            row3.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            row3.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row3.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row3.Cells[3].Borders.Left.Width = 0;
            row3.Cells[3].Borders.Top.Width = 0;
            row3.Cells[3].Borders.Right.Width = 0;
            row3.Cells[3].Borders.Bottom.Width = 1;

            row3.Cells[4].AddParagraph("Inv Date");
            row3.Cells[4].Format.Font.Size = 12;
            row3.Cells[4].Format.Font.Bold = true;
            row3.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            row3.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row3.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row3.Cells[4].Borders.Left.Width = 0;
            row3.Cells[4].Borders.Top.Width = 0;
            row3.Cells[4].Borders.Right.Width = 0;
            row3.Cells[4].Borders.Bottom.Width = 1;

            Row row4 = table.AddRow();
            row4.HeadingFormat = true;
            row4.Format.Alignment = ParagraphAlignment.Center;
            row4.Format.Font.Bold = true;

            row4.Cells[0].AddParagraph(salesOrder.SalesMadeBy);
            row4.Cells[0].Format.Font.Bold = true;
            row4.Cells[0].Format.Font.Size = 12;
            row4.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row4.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row4.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            row4.Cells[1].AddParagraph(salesOrder.CustomerOrderNo);
            row4.Cells[1].Format.Font.Size = 12;
            row4.Cells[1].Format.Font.Bold = true;
            row4.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row4.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row4.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            row4.Cells[2].AddParagraph(salesOrder.FreightCarrier.FreightName);
            row4.Cells[2].Format.Font.Bold = true;
            row4.Cells[2].Format.Font.Size = 12;
            row4.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            row4.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row4.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            row4.Cells[3].AddParagraph(salesOrder.TermsID);
            row4.Cells[3].Format.Font.Size = 12;
            row4.Cells[3].Format.Font.Bold = true;
            row4.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            row4.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row4.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            row4.Cells[4].AddParagraph(salesOrder.Invoice.InvoicedDate.ToString("dd/MM/yyyy"));
            row4.Cells[4].Format.Font.Size = 12;
            row4.Cells[4].Format.Font.Bold = true;
            row4.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            row4.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row4.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            // Item Table
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Format.Font.Size = 9;
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 4.0;
            this.table.BottomPadding = 1.0;
            this.table.Borders.Width = 1;
            this.table.Format.Alignment = ParagraphAlignment.Center;


            Column column3 = this.table.AddColumn("2.7cm");
            column3.Format.Alignment = ParagraphAlignment.Center;
            column3.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column3 = this.table.AddColumn("7.1cm");
            column3.Format.Alignment = ParagraphAlignment.Center;
            column3.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column3 = this.table.AddColumn("1.8cm");
            column3.Format.Alignment = ParagraphAlignment.Center;
            column3.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column3 = this.table.AddColumn("2.5cm");
            column3.Format.Alignment = ParagraphAlignment.Center;
            column3.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column3 = this.table.AddColumn("2.2cm");
            column3.Format.Alignment = ParagraphAlignment.Center;
            column3.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column3 = this.table.AddColumn("2.5cm");
            column3.Format.Alignment = ParagraphAlignment.Center;
            column3.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row row5 = table.AddRow();
            row5.HeadingFormat = true;
            row5.Format.Alignment = ParagraphAlignment.Center;
            row5.Format.Font.Bold = true;
            row5.BottomPadding = 0;

            row5.Cells[0].AddParagraph("Qty Shipped");
            row5.Cells[0].Format.Font.Bold = true;
            row5.Cells[0].Format.Font.Size = 12;
            row5.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row5.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row5.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row5.Cells[0].Borders.Left.Width = 0;
            row5.Cells[0].Borders.Right.Width = 0;
            row5.Cells[0].Borders.Top.Width = 0;
            row5.Cells[0].Borders.Bottom.Width = 1;

            row5.Cells[1].AddParagraph("Product Description");
            row5.Cells[1].Format.Font.Size = 12;
            row5.Cells[1].Format.Font.Bold = true;
            row5.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row5.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row5.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row5.Cells[1].Borders.Left.Width = 0;
            row5.Cells[1].Borders.Top.Width = 0;
            row5.Cells[1].Borders.Right.Width = 0;
            row5.Cells[1].Borders.Bottom.Width = 1;

            row5.Cells[2].AddParagraph("UM");
            row5.Cells[2].Format.Font.Size = 12;
            row5.Cells[2].Format.Font.Bold = true;
            row5.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            row5.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row5.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row5.Cells[2].Borders.Left.Width = 0;
            row5.Cells[2].Borders.Right.Width = 0;
            row5.Cells[2].Borders.Top.Width = 0;
            row5.Cells[2].Borders.Bottom.Width = 1;

            row5.Cells[3].AddParagraph("Unit Price");
            row5.Cells[3].Format.Font.Size = 12;
            row5.Cells[3].Format.Font.Bold = true;
            row5.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            row5.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row5.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row5.Cells[3].Borders.Left.Width = 0;
            row5.Cells[3].Borders.Top.Width = 0;
            row5.Cells[3].Borders.Right.Width = 0;
            row5.Cells[3].Borders.Bottom.Width = 1;

            row5.Cells[4].AddParagraph("Discount");
            row5.Cells[4].Format.Font.Size = 12;
            row5.Cells[4].Format.Font.Bold = true;
            row5.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            row5.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row5.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row5.Cells[4].Borders.Left.Width = 0;
            row5.Cells[4].Borders.Top.Width = 0;
            row5.Cells[4].Borders.Right.Width = 0;
            row5.Cells[4].Borders.Bottom.Width = 1;

            row5.Cells[5].AddParagraph("Extension");
            row5.Cells[5].Format.Font.Size = 12;
            row5.Cells[5].Format.Font.Bold = true;
            row5.Cells[5].Format.Alignment = ParagraphAlignment.Center;
            row5.Cells[5].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row5.Cells[5].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row5.Cells[5].Borders.Left.Width = 0;
            row5.Cells[5].Borders.Top.Width = 0;
            row5.Cells[5].Borders.Right.Width = 0;
            row5.Cells[5].Borders.Bottom.Width = 1;


            foreach (var item in salesOrder.SalesOrderDetails)
            {
                Row row6 = table.AddRow();
                row6.HeadingFormat = true;
                row6.Format.Alignment = ParagraphAlignment.Center;
                row6.Format.Font.Bold = true;

                row6.Cells[0].AddParagraph(string.Format("{0:0.0}", Math.Truncate(item.Quantity * 10) / 10));
                row6.Cells[0].Format.Font.Bold = true;
                row6.Cells[0].Format.Font.Size = 10;
                row6.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row6.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row6.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                row6.Cells[1].AddParagraph(item.QuoteProductDescription);
                row6.Cells[1].Format.Font.Size = 10;
                row6.Cells[1].Format.Font.Bold = true;
                row6.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                row6.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row6.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                row6.Cells[2].AddParagraph(item.Product.ProductUnit);
                row6.Cells[2].Format.Font.Bold = true;
                row6.Cells[2].Format.Font.Size = 10;
                row6.Cells[2].Format.Alignment = ParagraphAlignment.Center;
                row6.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row6.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                row6.Cells[3].AddParagraph(item.QuoteUnitPrice.ToString("C", CultureInfo.CurrentCulture));
                row6.Cells[3].Format.Font.Size = 10;
                row6.Cells[3].Format.Font.Bold = true;
                row6.Cells[3].Format.Alignment = ParagraphAlignment.Right;
                row6.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row6.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                row6.Cells[4].AddParagraph(string.Format("{0}%", item.Discount));
                row6.Cells[4].Format.Font.Size = 10;
                row6.Cells[4].Format.Font.Bold = true;
                row6.Cells[4].Format.Alignment = ParagraphAlignment.Center;
                row6.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row6.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                row6.Cells[5].AddParagraph(item.Total.ToString("C", CultureInfo.CurrentCulture));
                row6.Cells[5].Format.Font.Size = 10;
                row6.Cells[5].Format.Font.Bold = true;
                row6.Cells[5].Format.Alignment = ParagraphAlignment.Right;
                row6.Cells[5].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row6.Cells[5].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            }

            foreach (var item in salesOrder.FreightDetails)
            {
                Row rowFre = table.AddRow();
                rowFre.HeadingFormat = true;
                rowFre.Format.Alignment = ParagraphAlignment.Center;
                rowFre.Format.Font.Bold = true;

                rowFre.Cells[0].AddParagraph(string.Format("{0:0.0}", Math.Truncate(item.Pallets * 10) / 10));
                rowFre.Cells[0].Format.Font.Bold = true;
                rowFre.Cells[0].Format.Font.Size = 10;
                rowFre.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                rowFre.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                rowFre.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                rowFre.Cells[1].AddParagraph(item.DummyDescription);
                rowFre.Cells[1].Format.Font.Size = 10;
                rowFre.Cells[1].Format.Font.Bold = true;
                rowFre.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                rowFre.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                rowFre.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                rowFre.Cells[2].AddParagraph("EA");
                rowFre.Cells[2].Format.Font.Bold = true;
                rowFre.Cells[2].Format.Font.Size = 10;
                rowFre.Cells[2].Format.Alignment = ParagraphAlignment.Center;
                rowFre.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                rowFre.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                rowFre.Cells[3].AddParagraph(item.FreightCodeDetails.Price.ToString("C", CultureInfo.CurrentCulture));
                rowFre.Cells[3].Format.Font.Size = 10;
                rowFre.Cells[3].Format.Font.Bold = true;
                rowFre.Cells[3].Format.Alignment = ParagraphAlignment.Right;
                rowFre.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                rowFre.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                rowFre.Cells[4].AddParagraph(string.Format("{0}%", item.Discount));
                rowFre.Cells[4].Format.Font.Size = 10;
                rowFre.Cells[4].Format.Font.Bold = true;
                rowFre.Cells[4].Format.Alignment = ParagraphAlignment.Center;
                rowFre.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                rowFre.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

                rowFre.Cells[5].AddParagraph(item.Total.ToString("C", CultureInfo.CurrentCulture));
                rowFre.Cells[5].Format.Font.Size = 10;
                rowFre.Cells[5].Format.Font.Bold = true;
                rowFre.Cells[5].Format.Alignment = ParagraphAlignment.Right;
                rowFre.Cells[5].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                rowFre.Cells[5].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            }
            //string space = "-0.5cm";

            //if (salesOrder.SalesOrderDetails.Count == 1)
            //{
            //    space = "6.6cm";
            //}
            //else if (salesOrder.SalesOrderDetails.Count == 2)
            //{
            //    space = "5.6cm";
            //}
            //else if (salesOrder.SalesOrderDetails.Count == 3)
            //{
            //    space = "4.6cm";
            //}
            //else if (salesOrder.SalesOrderDetails.Count == 4)
            //{
            //    space = "3.6cm";
            //}
            //if (salesOrder.SalesOrderDetails.Count == 5)
            //{
            //    space = "2.6cm";
            //}
            //else if (salesOrder.SalesOrderDetails.Count == 6)
            //{
            //    space = "1.6cm";
            //}
            //else if (salesOrder.SalesOrderDetails.Count == 7)
            //{
            //    space = "0.6cm";
            //}
            //else if (salesOrder.SalesOrderDetails.Count >= 8)
            //{
            //    space = "1cm";
            //    document.LastSection.AddPageBreak();
            //}

            paragraph = section.AddParagraph();
            //paragraph.Format.SpaceBefore = space;
            paragraph.Style = "Reference";
            paragraph.AddTab();

            // Total Table
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Format.Font.Size = 9;
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 1.0;
            this.table.BottomPadding = 1.0;
            this.table.Borders.Width = 1;
            this.table.Format.Alignment = ParagraphAlignment.Center;

            Column column6 = this.table.AddColumn("11.5cm");
            column6.Format.Alignment = ParagraphAlignment.Center;
            column6.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column6 = this.table.AddColumn("0.5cm");
            column6.Format.Alignment = ParagraphAlignment.Center;
            column6.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column6 = this.table.AddColumn("3.8cm");
            column6.Format.Alignment = ParagraphAlignment.Center;
            column6.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column6 = this.table.AddColumn("0.5cm");
            column6.Format.Alignment = ParagraphAlignment.Center;
            column6.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column6 = this.table.AddColumn("2.5cm");
            column6.Format.Alignment = ParagraphAlignment.Center;
            column6.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row row7 = table.AddRow();
            row7.HeadingFormat = true;
            row7.Format.Alignment = ParagraphAlignment.Center;
            row7.Format.Font.Bold = true;
            row7.BottomPadding = 0;
            row7.KeepWith = 11;

            row7.Cells[0].AddParagraph("Preferred Payment Methods:");
            row7.Cells[0].Format.Font.Bold = true;
            row7.Cells[0].Format.Font.Size = 9;
            row7.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row7.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row7.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row7.Cells[0].MergeRight = 4;
            row7.Cells[0].Borders.Left.Width = 1;
            row7.Cells[0].Borders.Right.Width = 1;
            row7.Cells[0].Borders.Top.Width = 1;
            row7.Cells[0].Borders.Bottom.Width = 0;

            Row row8 = table.AddRow();
            row8.HeadingFormat = true;
            row8.Format.Alignment = ParagraphAlignment.Center;
            row8.Format.Font.Bold = true;
            row8.BottomPadding = 0;

            row8.Cells[0].AddParagraph("Electronic Funds Transfer (EFT):");
            row8.Cells[0].Format.Font.Bold = true;
            row8.Cells[0].Format.Font.Underline = Underline.Single;
            row8.Cells[0].Format.Font.Size = 9;
            row8.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row8.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row8.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row8.Cells[0].Borders.Left.Width = 1;
            row8.Cells[0].Borders.Right.Width = 1;
            row8.Cells[0].Borders.Top.Width = 0;
            row8.Cells[0].Borders.Bottom.Width = 0;
            row8.Cells[0].MergeRight = 4;

            Row row9 = table.AddRow();
            row9.HeadingFormat = true;
            row9.Format.Alignment = ParagraphAlignment.Center;
            row9.Format.Font.Bold = true;
            row9.BottomPadding = 0;

            row9.Cells[0].AddParagraph("Deposits can be made directly to our account using our bank details as follows:");
            row9.Cells[0].Format.Font.Bold = false;
            row9.Cells[0].Format.Font.Size = 8;
            row9.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row9.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row9.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row9.Cells[0].Borders.Left.Width = 1;
            row9.Cells[0].Borders.Right.Width = 0;
            row9.Cells[0].Borders.Top.Width = 0;
            row9.Cells[0].Borders.Bottom.Width = 0;

            row9.Cells[1].AddParagraph("");
            row9.Cells[1].Format.Font.Bold = true;
            row9.Cells[1].Format.Font.Size = 10;
            row9.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row9.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row9.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row9.Cells[1].Borders.Left.Width = 0;
            row9.Cells[1].Borders.Right.Width = 0;
            row9.Cells[1].Borders.Top.Width = 0;
            row9.Cells[1].Borders.Bottom.Width = 0;

            row9.Cells[2].AddParagraph("Total Before Tax:");
            row9.Cells[2].Format.Font.Bold = true;
            row9.Cells[2].Format.Font.Size = 12;
            row9.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row9.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row9.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row9.Cells[2].Borders.Left.Width = 0;
            row9.Cells[2].Borders.Right.Width = 0;
            row9.Cells[2].Borders.Top.Width = 0;
            row9.Cells[2].Borders.Bottom.Width = 0;

            row9.Cells[3].AddParagraph("");
            row9.Cells[3].Format.Font.Bold = true;
            row9.Cells[3].Format.Font.Size = 12;
            row9.Cells[3].Format.Alignment = ParagraphAlignment.Right;
            row9.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row9.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row9.Cells[3].Borders.Left.Width = 0;
            row9.Cells[3].Borders.Right.Width = 0;
            row9.Cells[3].Borders.Top.Width = 0;
            row9.Cells[3].Borders.Bottom.Width = 0;

            row9.Cells[4].AddParagraph((salesOrder.ListPriceTotal + salesOrder.FreightTotal).ToString("C", CultureInfo.CurrentCulture));
            row9.Cells[4].Format.Font.Bold = true;
            row9.Cells[4].Format.Font.Size = 12;
            row9.Cells[4].Format.Alignment = ParagraphAlignment.Right;
            row9.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row9.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row9.Cells[4].Borders.Left.Width = 0;
            row9.Cells[4].Borders.Right.Width = 1;
            row9.Cells[4].Borders.Top.Width = 0;
            row9.Cells[4].Borders.Bottom.Width = 0;

            Row row10 = table.AddRow();
            row10.HeadingFormat = true;
            row10.Format.Alignment = ParagraphAlignment.Center;
            row10.Format.Font.Bold = true;
            row10.BottomPadding = 0;

            row10.Cells[0].AddParagraph("Account Name: A1 Rubber");
            row10.Cells[0].Format.Font.Bold = false;
            row10.Cells[0].Format.Font.Size = 8;
            row10.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row10.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row10.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row10.Cells[0].Borders.Left.Width = 1;
            row10.Cells[0].Borders.Right.Width = 0;
            row10.Cells[0].Borders.Top.Width = 0;
            row10.Cells[0].Borders.Bottom.Width = 0;

            row10.Cells[1].AddParagraph("");
            row10.Cells[1].Format.Font.Bold = true;
            row10.Cells[1].Format.Font.Size = 10;
            row10.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row10.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row10.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row10.Cells[1].Borders.Left.Width = 0;
            row10.Cells[1].Borders.Right.Width = 0;
            row10.Cells[1].Borders.Top.Width = 0;
            row10.Cells[1].Borders.Bottom.Width = 0;

            row10.Cells[2].AddParagraph("Tax:");
            row10.Cells[2].Format.Font.Bold = true;
            row10.Cells[2].Format.Font.Size = 12;
            row10.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row10.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row10.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row10.Cells[2].Borders.Left.Width = 0;
            row10.Cells[2].Borders.Right.Width = 0;
            row10.Cells[2].Borders.Top.Width = 0;
            row10.Cells[2].Borders.Bottom.Width = 0;

            row10.Cells[3].AddParagraph("");
            row10.Cells[3].Format.Font.Bold = true;
            row10.Cells[3].Format.Font.Size = 12;
            row10.Cells[3].Format.Alignment = ParagraphAlignment.Right;
            row10.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row10.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row10.Cells[3].Borders.Left.Width = 0;
            row10.Cells[3].Borders.Right.Width = 0;
            row10.Cells[3].Borders.Top.Width = 0;
            row10.Cells[3].Borders.Bottom.Width = 0;

            row10.Cells[4].AddParagraph(salesOrder.GST.ToString("C", CultureInfo.CurrentCulture));
            row10.Cells[4].Format.Font.Bold = true;
            row10.Cells[4].Format.Font.Size = 12;
            row10.Cells[4].Format.Alignment = ParagraphAlignment.Right;
            row10.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row10.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row10.Cells[4].Borders.Left.Width = 0;
            row10.Cells[4].Borders.Right.Width = 1;
            row10.Cells[4].Borders.Top.Width = 0;
            row10.Cells[4].Borders.Bottom.Width = 0;

            Row row11 = table.AddRow();
            row11.HeadingFormat = true;
            row11.Format.Alignment = ParagraphAlignment.Center;
            row11.Format.Font.Bold = true;
            row11.BottomPadding = 0;

            row11.Cells[0].AddParagraph("BSB: 638 060 Account No: 008341648");
            row11.Cells[0].Format.Font.Bold = false;
            row11.Cells[0].Format.Font.Size = 8;
            row11.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row11.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row11.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row11.Cells[0].Borders.Left.Width = 1;
            row11.Cells[0].Borders.Right.Width = 0;
            row11.Cells[0].Borders.Top.Width = 0;
            row11.Cells[0].Borders.Bottom.Width = 0;

            row11.Cells[1].AddParagraph("");
            row11.Cells[1].Format.Font.Bold = true;
            row11.Cells[1].Format.Font.Size = 10;
            row11.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row11.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row11.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row11.Cells[1].Borders.Left.Width = 0;
            row11.Cells[1].Borders.Right.Width = 0;
            row11.Cells[1].Borders.Top.Width = 0;
            row11.Cells[1].Borders.Bottom.Width = 0;

            row11.Cells[2].AddParagraph("Grand Total:");
            row11.Cells[2].Format.Font.Bold = true;
            row11.Cells[2].Format.Font.Size = 12;
            row11.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row11.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row11.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row11.Cells[2].Borders.Left.Width = 0;
            row11.Cells[2].Borders.Right.Width = 0;
            row11.Cells[2].Borders.Top.Width = 0;
            row11.Cells[2].Borders.Bottom.Width = 0;

            row11.Cells[3].AddParagraph("");
            row11.Cells[3].Format.Font.Bold = true;
            row11.Cells[3].Format.Font.Size = 12;
            row11.Cells[3].Format.Alignment = ParagraphAlignment.Right;
            row11.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row11.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row11.Cells[3].Borders.Left.Width = 0;
            row11.Cells[3].Borders.Right.Width = 0;
            row11.Cells[3].Borders.Top.Width = 0;
            row11.Cells[3].Borders.Bottom.Width = 0;

            row11.Cells[4].AddParagraph(salesOrder.TotalAmount.ToString("C", CultureInfo.CurrentCulture));
            row11.Cells[4].Format.Font.Bold = true;
            row11.Cells[4].Format.Font.Size = 12;
            row11.Cells[4].Format.Alignment = ParagraphAlignment.Right;
            row11.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row11.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row11.Cells[4].Borders.Left.Width = 0;
            row11.Cells[4].Borders.Right.Width = 1;
            row11.Cells[4].Borders.Top.Width = 0;
            row11.Cells[4].Borders.Bottom.Width = 0;

            Row row12 = table.AddRow();
            row12.HeadingFormat = true;
            row12.Format.Alignment = ParagraphAlignment.Center;
            row12.Format.Font.Bold = true;
            row12.BottomPadding = 0;

            row12.Cells[0].AddParagraph("Please e-mail your payment remittance to accounts@a1rubber.com or fax it to\n (07) 38072344");
            row12.Cells[0].Format.Font.Bold = false;
            row12.Cells[0].Format.Font.Size = 8;
            row12.Cells[0].VerticalAlignment = VerticalAlignment.Bottom;
            row12.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row12.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row12.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row12.Cells[0].Borders.Left.Width = 1;
            row12.Cells[0].Borders.Right.Width = 0;
            row12.Cells[0].Borders.Top.Width = 0;
            row12.Cells[0].Borders.Bottom.Width = 0;

            row12.Cells[1].AddParagraph("");
            row12.Cells[1].Format.Font.Bold = true;
            row12.Cells[1].Format.Font.Size = 10;
            row12.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row12.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row12.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row12.Cells[1].Borders.Left.Width = 0;
            row12.Cells[1].Borders.Right.Width = 0;
            row12.Cells[1].Borders.Top.Width = 0;
            row12.Cells[1].Borders.Bottom.Width = 0;

            row12.Cells[2].AddParagraph("Remit this amount");
            row12.Cells[2].Format.Font.Bold = true;
            row12.Cells[2].Format.Font.Size = 12;
            row12.Cells[2].Format.Font.Italic = true;
            row12.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row12.Cells[2].VerticalAlignment = VerticalAlignment.Bottom;
            row12.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row12.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row12.Cells[2].Borders.Left.Width = 0;
            row12.Cells[2].Borders.Right.Width = 0;
            row12.Cells[2].Borders.Top.Width = 0;
            row12.Cells[2].Borders.Bottom.Width = 0;

            row12.Cells[3].AddParagraph("");
            row12.Cells[3].Format.Font.Bold = true;
            row12.Cells[3].Format.Font.Size = 12;
            row12.Cells[3].Format.Alignment = ParagraphAlignment.Right;
            row12.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row12.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row12.Cells[3].Borders.Left.Width = 0;
            row12.Cells[3].Borders.Right.Width = 0;
            row12.Cells[3].Borders.Top.Width = 0;
            row12.Cells[3].Borders.Bottom.Width = 0;

            row12.Cells[4].AddParagraph().AddImage("S:/PRODUCTION/QImg/arrow.png");
            row12.Cells[4].Format.Font.Bold = true;
            row12.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            row12.Cells[4].VerticalAlignment = VerticalAlignment.Top;
            row12.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row12.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row12.Cells[4].Borders.Left.Width = 0;
            row12.Cells[4].Borders.Right.Width = 1;
            row12.Cells[4].Borders.Top.Width = 0;
            row12.Cells[4].Borders.Bottom.Width = 0;

            Row row13 = table.AddRow();
            row13.HeadingFormat = true;
            row13.Format.Alignment = ParagraphAlignment.Center;
            row13.Format.Font.Bold = true;
            row13.BottomPadding = 0;

            row13.Cells[0].AddParagraph("Phone Pay");
            row13.Cells[0].Format.Font.Bold = true;
            row13.Cells[0].Format.Font.Size = 9;
            row13.Cells[0].Format.Font.Underline = Underline.Single;
            row13.Cells[0].VerticalAlignment = VerticalAlignment.Bottom;
            row13.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row13.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row13.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row13.Cells[0].Borders.Left.Width = 1;
            row13.Cells[0].Borders.Right.Width = 0;
            row13.Cells[0].Borders.Top.Width = 0;
            row13.Cells[0].Borders.Bottom.Width = 0;

            row13.Cells[1].AddParagraph("");
            row13.Cells[1].Format.Font.Bold = true;
            row13.Cells[1].Format.Font.Size = 10;
            row13.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            row13.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row13.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row13.Cells[1].Borders.Left.Width = 0;
            row13.Cells[1].Borders.Right.Width = 0;
            row13.Cells[1].Borders.Top.Width = 0;
            row13.Cells[1].Borders.Bottom.Width = 0;

            row13.Cells[2].AddParagraph("");
            row13.Cells[2].Format.Font.Bold = true;
            row13.Cells[2].Format.Font.Size = 12;
            row13.Cells[2].Format.Font.Italic = true;
            row13.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row13.Cells[2].VerticalAlignment = VerticalAlignment.Bottom;
            row13.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row13.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row13.Cells[2].Borders.Left.Width = 0;
            row13.Cells[2].Borders.Right.Width = 0;
            row13.Cells[2].Borders.Top.Width = 0;
            row13.Cells[2].Borders.Bottom.Width = 0;

            row13.Cells[3].AddParagraph("");
            row13.Cells[3].Format.Font.Bold = true;
            row13.Cells[3].Format.Font.Size = 12;
            row13.Cells[3].Format.Alignment = ParagraphAlignment.Right;
            row13.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row13.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row13.Cells[3].Borders.Left.Width = 0;
            row13.Cells[3].Borders.Right.Width = 0;
            row13.Cells[3].Borders.Top.Width = 0;
            row13.Cells[3].Borders.Bottom.Width = 0;

            row13.Cells[4].AddParagraph("");
            row13.Cells[4].Format.Font.Bold = true;
            row13.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            row13.Cells[4].VerticalAlignment = VerticalAlignment.Top;
            row13.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row13.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row13.Cells[4].Borders.Left.Width = 0;
            row13.Cells[4].Borders.Right.Width = 1;
            row13.Cells[4].Borders.Top.Width = 0;
            row13.Cells[4].Borders.Bottom.Width = 0;

            Row row14 = table.AddRow();
            row14.HeadingFormat = true;
            row14.Format.Alignment = ParagraphAlignment.Center;
            row14.Format.Font.Bold = true;
            row14.BottomPadding = 0;

            row14.Cells[0].AddParagraph("For Credit Card Payments - please phone (07) 3807 3666\n (payments made with a Visa or Mastercard will incur a payment processing fee of 1.2% (incl GST) of the total payment.)");
            row14.Cells[0].Format.Font.Bold = false;
            row14.Cells[0].Format.Font.Size = 8;
            row14.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row14.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row14.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row14.Cells[0].Borders.Left.Width = 1;
            row14.Cells[0].Borders.Right.Width = 0;
            row14.Cells[0].Borders.Top.Width = 0;
            row14.Cells[0].Borders.Bottom.Width = 0;
            row14.Cells[0].MergeRight = 4;

            Row row15 = table.AddRow();
            row15.HeadingFormat = true;
            row15.Format.Alignment = ParagraphAlignment.Center;
            row15.Format.Font.Bold = true;
            row15.BottomPadding = 0;

            row15.Cells[0].AddParagraph("Mail");
            row15.Cells[0].Format.Font.Bold = true;
            row15.Cells[0].Format.Font.Size = 9;
            row15.Cells[0].Format.Font.Underline = Underline.Single;
            row15.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row15.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row15.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row15.Cells[0].Borders.Left.Width = 1;
            row15.Cells[0].Borders.Right.Width = 0;
            row15.Cells[0].Borders.Top.Width = 0;
            row15.Cells[0].Borders.Bottom.Width = 0;
            row15.Cells[0].MergeRight = 4;

            Row row16 = table.AddRow();
            row16.HeadingFormat = true;
            row16.Format.Alignment = ParagraphAlignment.Center;
            row16.Format.Font.Bold = true;
            row16.BottomPadding = 0;

            row16.Cells[0].AddParagraph("Post Cheques to PO Box 6278, Yatala QLD 4207");
            row16.Cells[0].Format.Font.Bold = false;
            row16.Cells[0].Format.Font.Size = 8;
            row16.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row16.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row16.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row16.Cells[0].Borders.Left.Width = 1;
            row16.Cells[0].Borders.Right.Width = 0;
            row16.Cells[0].Borders.Top.Width = 0;
            row16.Cells[0].Borders.Bottom.Width = 0;
            row16.Cells[0].MergeRight = 4;

            Row row17 = table.AddRow();
            row17.HeadingFormat = true;
            row17.Format.Alignment = ParagraphAlignment.Center;
            row17.Format.Font.Bold = true;
            row17.BottomPadding = 0;

            row17.Cells[0].AddParagraph("This invoice is subject to our standard Terms and Conditions of Sale which are available\n upon request,on our website - www.a1rubber.com, or for prepaid customers printed overleaf");
            row17.Cells[0].Format.Font.Bold = true;
            row17.Cells[0].Format.Font.Size = 9;
            row17.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row17.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row17.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row17.Cells[0].Borders.Left.Width = 1;
            row17.Cells[0].Borders.Right.Width = 0;
            row17.Cells[0].Borders.Top.Width = 1;
            row17.Cells[0].Borders.Bottom.Width = 1;
            row17.Cells[0].MergeRight = 4;

            Row row18 = table.AddRow();
            row18.HeadingFormat = true;
            row18.Format.Alignment = ParagraphAlignment.Center;
            row18.Format.Font.Bold = true;
            row18.TopPadding = 1.5;

            row18.Cells[0].AddParagraph("Postal Address: PO Box 6278, Yatala QLD 4207\n Factory/Pickup Address: Cnr Byte & Binary Sts, Yatala QLD 4207\n Ph: 07 3807 3666 Fax: 3807 2344");
            row18.Cells[0].Format.Font.Bold = true;
            row18.Cells[0].Format.Font.Size = 10;
            row18.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row18.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row18.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row18.Cells[0].Borders.Left.Width = 0;
            row18.Cells[0].Borders.Right.Width = 0;
            row18.Cells[0].Borders.Top.Width = 1;
            row18.Cells[0].Borders.Bottom.Width = 0;
            row18.Cells[0].MergeRight = 4;

            row18.Cells[4].AddParagraph("");
            row18.Cells[4].Format.Font.Bold = true;
            row18.Cells[4].Format.Font.Size = 10;
            row18.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            row18.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row18.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row18.Cells[4].Borders.Left.Width = 0;
            row18.Cells[4].Borders.Right.Width = 0;
            row18.Cells[4].Borders.Top.Width = 1;
            row18.Cells[4].Borders.Bottom.Width = 0;
            // row18.Cells[4].MergeRight = 3;    

        }
    }
}
