using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Quoting;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.PdfGeneration
{
    public class UpdateQuotePDF
    {
        Document document;
        TextFrame addressFrame;
        MigraDoc.DocumentObjectModel.Tables.Table table;
        private Quote quote;

        public UpdateQuotePDF(Quote q)
        {
            quote = new Quote();

            quote = DBAccess.GetQuote(q.QuoteNo);
            quote.FileName = q.FileName;
            //contactPerson = quote.ContactPerson;
        }

        public Exception CreateQuote()
        {
            Exception res = null;
            Document document = CreateDocument(quote.QuoteNo);
            document.UseCmykColor = true;
            document.DefaultPageSetup.FooterDistance = "-2.0cm";
            document.DefaultPageSetup.LeftMargin = "1.2cm";
            document.DefaultPageSetup.RightMargin = "1.2cm";
            document.DefaultPageSetup.Orientation= Orientation.Portrait;
            const bool unicode = false;
            const PdfFontEmbedding embedding = PdfFontEmbedding.Always;
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();
            DateTime currentDate = DateTime.Now;

            try
            {
                pdfRenderer.PdfDocument.Save(FilePathManager.GetQuoteSavingPath() + "/" + quote.FileName);
            }
            catch (Exception ex)
            {
                res = ex;
            }

            return res;
        }

        public Document CreateDocument(int QuoteNo)
        {
            // Create a new MigraDoc document
            this.document = new Document();
            this.document.Info.Title = "A1 Rubber Quotation";
            this.document.Info.Subject = "A1 Rubber Quotation";
            this.document.Info.Author = "A1 Rubber";

            DefineStyles();
            CreatePage(QuoteNo);
            FillContent();

            return this.document;
        }


        void DefineStyles()
        {
            // Get the predefined style Normal.
            MigraDoc.DocumentObjectModel.Style style = this.document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
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

        void CreatePage(int QuoteNo)
        {
            //Build shipping address as a string

            string shipAddress = !String.IsNullOrWhiteSpace(quote.Customer.ShipCity) ? quote.Customer.ShipAddress + "," + System.Environment.NewLine + quote.Customer.ShipCity : quote.Customer.ShipAddress;

            shipAddress = !String.IsNullOrWhiteSpace(quote.Customer.ShipState) ? shipAddress + "," + System.Environment.NewLine + quote.Customer.ShipState : shipAddress;

            shipAddress = !String.IsNullOrWhiteSpace(quote.Customer.ShipPostCode) ? shipAddress + "," + quote.Customer.ShipPostCode : shipAddress;

            if (quote.Customer.ShipCountry != "Australia")
            {
                shipAddress = !String.IsNullOrWhiteSpace(quote.Customer.ShipCountry) ? shipAddress + "," + System.Environment.NewLine + quote.Customer.ShipCountry : shipAddress;
            }

            if (!string.IsNullOrWhiteSpace(quote.QuoteCourierName) && (!quote.Customer.ShipAddress.Equals("Collect from A1 Rubber NSW") && !quote.Customer.ShipAddress.Equals("Collect from A1 Rubber QLD")))
            {
                shipAddress += System.Environment.NewLine + "Transport : " + quote.QuoteCourierName;
            }


            // Each MigraDoc document needs at least one section.
            MigraDoc.DocumentObjectModel.Section section = this.document.AddSection();
            section.PageSetup.TopMargin = Unit.FromCentimeter(3.3);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(3.6);

            // Put a logo in the header
            MigraDoc.DocumentObjectModel.Shapes.Image image = section.Headers.Primary.AddImage("S:/PRODUCTION/DONOTDELETE/Images/A1Rubber-CMYK-INV_jpg.jpg");
            image.Height = "2.4cm";
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Left;
            image.WrapFormat.Style = WrapStyle.Through;

            // Create footer
            MigraDoc.DocumentObjectModel.Paragraph paragraph = section.Footers.Primary.AddParagraph();

            Paragraph paragraph1;
            TextFrame frame = section.Headers.Primary.AddTextFrame();
            image = frame.AddImage("S:/PRODUCTION/DONOTDELETE/Images/A1Rubber-CMYK-INV_jpg.jpg");
            image.Height = "2cm";
            image.Width = "3cm";
            image.Left = "-1cm";
            image.Top = "26cm";
            paragraph1 = frame.AddParagraph("\n\n\n");


            Paragraph paragraph6;
            TextFrame frame6 = section.Headers.Primary.AddTextFrame();
            frame6.Left = "-0.7cm";
            frame6.Width = "3cm";
            frame6.MarginTop = "25.2cm";
            paragraph6 = frame6.AddParagraph("www.a1rubber.com");
            paragraph6.Format.Font.Size = 8;
            paragraph6.Format.Font.Color = Colors.Red;

            Paragraph paragraph2 = new Paragraph();
            TextFrame frame2 = section.Footers.Primary.AddTextFrame();
            frame2.Left = "2.8cm";
            frame2.Width = "3cm";
            frame2.MarginTop = "9cm";
            paragraph2 = frame2.AddParagraph("QLD \n(07) 3807 3666\n34 Binary St,\nYatala, 4207");
            paragraph2.Format.Font.Size = 8;
            paragraph2.Format.Alignment = ParagraphAlignment.Left;

            Paragraph paragraph3;
            TextFrame frame3 = section.Footers.Primary.AddTextFrame();
            frame3.Left = "5.5cm";
            frame3.Width = "4cm";
            frame3.MarginTop = "9cm";
            paragraph3 = frame3.AddParagraph("NSW \n(02) 9756 2146\n40 Bentley St,\nWetherill Park, 2164");
            paragraph3.Format.Font.Size = 8;
            paragraph3.Format.Font.Bold = true;
            paragraph3.Format.Alignment = ParagraphAlignment.Left;

            Paragraph paragraph4;
            TextFrame frame4 = section.Footers.Primary.AddTextFrame();
            frame4.Left = "8.8cm";
            frame4.Width = "3cm";
            frame4.MarginTop = "9cm";
            paragraph4 = frame4.AddParagraph("VIC \n0408 607 888");
            paragraph4.Format.Font.Size = 8;
            paragraph4.Format.Font.Bold = true;
            paragraph4.Format.Alignment = ParagraphAlignment.Left;

            Paragraph paragraph5;
            TextFrame frame5 = section.Footers.Primary.AddTextFrame();
            frame5.MarginTop = "8.8cm";
            image = frame5.AddImage("S:/PRODUCTION/DONOTDELETE/Images/cec_banner_v2.png");
            image.Height = "2cm";
            image.Width = "8cm";
            image.Left = "11cm";
            paragraph5 = frame5.AddParagraph();

            Paragraph paragraph7;
            TextFrame frame7 = section.Footers.Primary.AddTextFrame();
            frame7.MarginTop = "7.3cm";
            image = frame7.AddImage("S:/PRODUCTION/DONOTDELETE/Images/ATFA_member_ISO_logo.jpg");
            image.Height = "1.3cm";
            image.Width = "6cm";
            image.Left = "13cm";
            paragraph7 = frame7.AddParagraph();


            this.addressFrame = section.AddTextFrame();
            this.addressFrame.MarginLeft = "3.8cm";
            this.addressFrame.Height = "3.0cm";
            this.addressFrame.Width = "7.0cm";
            this.addressFrame.Left = ShapePosition.Right;
            this.addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            this.addressFrame.Top = "-0.1cm";
            this.addressFrame.RelativeVertical = RelativeVertical.Page;

            paragraph = this.addressFrame.AddParagraph("QUOTATION");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 17;
            paragraph.Format.SpaceBefore = "2cm";
            paragraph.Format.Font.Bold = true;

            //paragraph = section.AddParagraph();
            //paragraph.Format.SpaceBefore = "-0.1cm";
            //paragraph.Style = "Reference";
            //paragraph.AddTab();

            // Create the item table
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.White;
            this.table.Borders.Width = 0.25;
            this.table.Borders.Left.Width = 0.5;
            this.table.Borders.Right.Width = 0.5;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 1.0;
            this.table.BottomPadding = 1.0;


            Column column2 = this.table.AddColumn("12.7cm");
            column2.Format.Alignment = ParagraphAlignment.Center;

            column2 = this.table.AddColumn("2.92cm");
            column2.Format.Alignment = ParagraphAlignment.Center;
            column2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column2 = this.table.AddColumn("2.92cm");
            column2.Format.Alignment = ParagraphAlignment.Center;
            column2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row row3 = table.AddRow();
            row3.HeadingFormat = true;
            row3.Format.Alignment = ParagraphAlignment.Center;
            row3.Format.Font.Bold = true;

            row3.Cells[0].AddParagraph("    www.a1rubber.com");
            row3.Cells[0].Format.Font.Size = 9;
            row3.Cells[0].Format.Font.Bold = true;
            row3.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row3.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row3.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            row3.Cells[1].AddParagraph("Date");
            row3.Cells[1].Format.Font.Size = 9;
            row3.Cells[1].Format.Font.Bold = true;
            row3.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row3.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Red;
            row3.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

            row3.Cells[2].AddParagraph("Quote No");
            row3.Cells[2].Format.Font.Size = 9;
            row3.Cells[2].Format.Font.Bold = true;
            row3.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row3.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Red;
            row3.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row row4 = table.AddRow();
            row4.HeadingFormat = true;
            row4.Format.Alignment = ParagraphAlignment.Center;
            row4.Format.Font.Bold = true;

            row4.Cells[0].AddParagraph("    ABN: 85 663 589 062");
            row4.Cells[0].Format.Font.Size = 8;
            row4.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row4.Cells[0].Format.Font.Bold = false;
            row4.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            row4.Cells[1].AddParagraph(DateTime.Now.ToString("dd/MM/yyyy"));
            row4.Cells[1].Format.Font.Size = 8;
            row4.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row4.Cells[1].Format.Font.Bold = false;
            row4.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;

            row4.Cells[2].AddParagraph(quote.QuoteNo.ToString());
            row4.Cells[2].Format.Font.Size = 8;
            row4.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row4.Cells[2].Format.Font.Bold = false;
            row4.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;


            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "0.1cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            // Create the item table
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
            this.table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.White;
            this.table.Borders.Width = 0.25;
            this.table.Borders.Left.Width = 0.5;
            this.table.Borders.Right.Width = 0.5;
            this.table.Rows.LeftIndent = 0;
            this.table.TopPadding = 1.0;
            this.table.BottomPadding = 1.0;

            // Before you can add a row, you must define the columns
            Column column1 = this.table.AddColumn("5.85cm");
            column1.Format.Alignment = ParagraphAlignment.Center;

            column1 = this.table.AddColumn("0.5cm");
            column1.Format.Alignment = ParagraphAlignment.Center;
            column1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column1 = this.table.AddColumn("5.85cm");
            column1.Format.Alignment = ParagraphAlignment.Center;

            column1 = this.table.AddColumn("0.5cm");
            column1.Format.Alignment = ParagraphAlignment.Center;
            column1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column1 = this.table.AddColumn("5.85cm");
            column1.Format.Alignment = ParagraphAlignment.Center;

            Row row1 = table.AddRow();
            row1.HeadingFormat = true;
            row1.Format.Alignment = ParagraphAlignment.Center;
            row1.Format.Font.Bold = true;

            row1.Cells[0].AddParagraph("Sold To");
            row1.Cells[0].Format.Font.Size = 9;
            row1.Cells[0].Format.Font.Bold = true;
            row1.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row1.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Red;
            row1.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

            row1.Cells[1].AddParagraph("");
            row1.Cells[1].Format.Font.Size = 9;
            row1.Cells[1].Format.Alignment = ParagraphAlignment.Left;

            row1.Cells[2].AddParagraph("Project Name");
            row1.Cells[2].Format.Font.Size = 9;
            row1.Cells[2].Format.Font.Bold = true;
            row1.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row1.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Red;
            row1.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

            row1.Cells[3].AddParagraph("");
            row1.Cells[3].Format.Font.Size = 9;
            row1.Cells[3].Format.Alignment = ParagraphAlignment.Left;

            row1.Cells[4].AddParagraph("Ship To");
            row1.Cells[4].Format.Font.Size = 9;
            row1.Cells[4].Format.Font.Bold = true;
            row1.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row1.Cells[4].Format.Alignment = ParagraphAlignment.Left;
            row1.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Red;

            Row row2 = table.AddRow();
            row2.HeadingFormat = true;
            row2.Format.Alignment = ParagraphAlignment.Center;
            row2.Format.Font.Bold = true;


            string conName = string.Empty;

            conName = quote.ContactPerson.ContactPersonName;

            string soldToStr = quote.Customer.CompanyName;
            string phoneNostr = quote.ContactPerson.PhoneNumber1;

            if (!string.IsNullOrWhiteSpace(quote.ContactPerson.PhoneNumber1) && (!string.IsNullOrWhiteSpace(quote.ContactPerson.PhoneNumber2) && quote.ContactPerson.PhoneNumber2 != "Not Available"))
            {
                phoneNostr = soldToStr + System.Environment.NewLine + "Contact : " + conName + System.Environment.NewLine + "Phone : " + quote.ContactPerson.PhoneNumber1 + " | " + quote.ContactPerson.PhoneNumber2 + System.Environment.NewLine + "E-mail : " + quote.ContactPerson.Email;
            }
            else if (!string.IsNullOrWhiteSpace(quote.ContactPerson.PhoneNumber1) && (string.IsNullOrWhiteSpace(quote.ContactPerson.PhoneNumber2) || quote.ContactPerson.PhoneNumber2 == "Not Available"))
            {
                phoneNostr = soldToStr + System.Environment.NewLine + "Contact : " + conName + System.Environment.NewLine + "Phone : " + quote.ContactPerson.PhoneNumber1 + System.Environment.NewLine + "E-mail : " + quote.ContactPerson.Email;
            }
            else if (string.IsNullOrWhiteSpace(quote.ContactPerson.PhoneNumber1) && (!string.IsNullOrWhiteSpace(quote.ContactPerson.PhoneNumber2) && quote.ContactPerson.PhoneNumber2 != "Not Available"))
            {
                phoneNostr = soldToStr + System.Environment.NewLine + "Contact : " + conName + System.Environment.NewLine + "Phone : " + quote.ContactPerson.PhoneNumber2 + System.Environment.NewLine + "E-mail : " + quote.ContactPerson.Email;
            }

            row2.Cells[0].AddParagraph(phoneNostr);
            row2.Cells[0].Format.Font.Size = 8;
            row2.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row2.Cells[0].Format.Font.Bold = false;

            row2.Cells[1].AddParagraph("");
            row2.Cells[1].Format.Font.Size = 8;
            row2.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row2.Cells[1].Format.Font.Bold = false;

            row2.Cells[2].AddParagraph(quote.ProjectName);
            row2.Cells[2].Format.Font.Size = 8;
            row2.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row2.Cells[2].Format.Font.Bold = false;

            row2.Cells[3].AddParagraph("");
            row2.Cells[3].Format.Font.Size = 8;
            row2.Cells[3].Format.Alignment = ParagraphAlignment.Left;
            row2.Cells[3].Format.Font.Bold = false;

            row2.Cells[4].AddParagraph(shipAddress);
            row2.Cells[4].Format.Font.Size = 8;
            row2.Cells[4].Format.Alignment = ParagraphAlignment.Left;
            row2.Cells[4].Format.Font.Bold = false;

            // Create the text frame for the address 
            this.addressFrame = section.AddTextFrame();
            // this.addressFrame.MarginLeft = "3.0cm";
            this.addressFrame.Height = "3.0cm";
            this.addressFrame.Width = "18.5cm";
            this.addressFrame.Left = ShapePosition.Left;
            this.addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;

            this.addressFrame.MarginTop = "27cm";
            this.addressFrame.RelativeVertical = RelativeVertical.Page;

            paragraph = this.addressFrame.AddParagraph("");
            paragraph.Format.Font.Name = "Helvatica";
            paragraph.Format.Font.Size = 8;
            paragraph.Format.SpaceAfter = 15;
            paragraph.Format.Font.Bold = false;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.3cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            // Create the item table
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            this.table.Borders.Width = 0.25;
            this.table.Borders.Left.Width = 0.5;
            this.table.Borders.Right.Width = 0.5;
            this.table.Rows.LeftIndent = 0;


            // Before you can add a row, you must define the columns

            Column column = this.table.AddColumn("0.8cm");//Qty
            column.Format.Alignment = ParagraphAlignment.Center;

            column = this.table.AddColumn("3.5cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = this.table.AddColumn("6.5cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = this.table.AddColumn("1.6cm");//Stocked
            column.Format.Alignment = ParagraphAlignment.Center;

            column = this.table.AddColumn("1.7cm");//Price
            column.Format.Alignment = ParagraphAlignment.Center;

            column = this.table.AddColumn("0.8cm");//Unit
            column.Format.Alignment = ParagraphAlignment.Center;

            column = this.table.AddColumn("1.1cm");//Disc
            column.Format.Alignment = ParagraphAlignment.Center;

            column = this.table.AddColumn("2.5cm");//Tot
            column.Format.Alignment = ParagraphAlignment.Center;


            Row row = table.AddRow();
            row.HeadingFormat = false;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Format.Font.Name = "Calibri";

            row.Cells[0].AddParagraph("Qty");
            row.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[0].Format.Font.Size = 9;

            row.Cells[1].AddParagraph("Product Code");
            row.Cells[1].Format.Font.Size = 9;
            row.Cells[1].Format.Alignment = ParagraphAlignment.Center;

            row.Cells[2].AddParagraph("Product Description");
            row.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[2].Format.Font.Size = 9;

            row.Cells[3].AddParagraph("Stocked");
            row.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[3].Format.Font.Size = 9;

            row.Cells[4].AddParagraph("List Price");
            row.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[4].Format.Font.Size = 9;

            row.Cells[5].AddParagraph("UM");
            row.Cells[5].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[5].Format.Font.Size = 9;

            row.Cells[6].AddParagraph("Disc %");
            row.Cells[6].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[6].Format.Font.Size = 9;

            row.Cells[7].AddParagraph("Discounted Total");
            row.Cells[7].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[7].Format.Font.Size = 9;
        }

        void FillContent()
        {

            MigraDoc.DocumentObjectModel.Paragraph paragraph = this.addressFrame.AddParagraph();
            int x = 1;
            decimal total = 0;
            //decimal tax = 0;

            MigraDoc.DocumentObjectModel.Color col = MigraDoc.DocumentObjectModel.Colors.AliceBlue;

            for (int i = 0; i < quote.QuoteDetails.Count; i++)
            {
                if (IsOdd(i))
                {
                    col = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
                }
                else
                {
                    col = MigraDoc.DocumentObjectModel.Colors.White;
                }

                Row row = this.table.AddRow();
                row.TopPadding = 1.5;
                row.Shading.Color = col;

                row.Cells[0].AddParagraph(quote.QuoteDetails[i].Quantity.ToString());
                row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[0].Format.SpaceBefore = 2;
                row.Cells[0].Format.SpaceAfter = 2;
                row.Cells[0].Format.Font.Size = 8;
                row.Cells[0].Borders.Bottom.Visible = false;

                row.Cells[1].AddParagraph(quote.QuoteDetails[i].Product.ProductCode);
                row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[1].Format.SpaceBefore = 2;
                row.Cells[1].Format.SpaceAfter = 2;
                row.Cells[1].Format.Font.Size = 8;
                row.Cells[1].Borders.Bottom.Visible = false;

                row.Cells[2].AddParagraph(quote.QuoteDetails[i].QuoteProductDescription);
                row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[2].Format.SpaceBefore = 2;
                row.Cells[2].Format.SpaceAfter = 2;
                row.Cells[2].Format.Font.Size = 8;
                row.Cells[2].Borders.Bottom.Visible = false;

                row.Cells[3].AddParagraph(quote.QuoteDetails[i].Product.LocationType == null ? "" : quote.QuoteDetails[i].Product.LocationType);
                row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[3].Format.SpaceBefore = 2;
                row.Cells[3].Format.SpaceAfter = 2;
                row.Cells[3].Format.Font.Size = 8;
                row.Cells[3].Borders.Bottom.Visible = false;

                row.Cells[4].AddParagraph("$" + Math.Round(quote.QuoteDetails[i].QuoteUnitPrice, 2));
                row.Cells[4].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[4].Format.SpaceBefore = 2;
                row.Cells[4].Format.SpaceAfter = 2;
                row.Cells[4].Format.Font.Size = 8;
                row.Cells[4].Borders.Bottom.Visible = false;

                row.Cells[5].AddParagraph(quote.QuoteDetails[i].Product.ProductUnit);
                row.Cells[5].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[5].Format.SpaceBefore = 2;
                row.Cells[5].Format.SpaceAfter = 2;
                row.Cells[5].Format.Font.Size = 8;
                row.Cells[5].Borders.Bottom.Visible = false;

                row.Cells[6].AddParagraph(quote.QuoteDetails[i].Discount.ToString() + "%");
                row.Cells[6].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[6].Format.SpaceBefore = 2;
                row.Cells[6].Format.SpaceAfter = 2;
                row.Cells[6].Format.Font.Size = 8;
                row.Cells[6].Borders.Bottom.Visible = false;

                row.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(quote.QuoteDetails[i].Total, 2)));
                row.Cells[7].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[7].Format.SpaceBefore = 2;
                row.Cells[7].Format.SpaceAfter = 2;
                row.Cells[7].Format.Font.Size = 8;
                row.Cells[7].Borders.Bottom.Visible = false;

                total += Math.Round(quote.QuoteDetails[i].Total, 2);
                x++;
            }

            //tax = quote.GSTActive ? Math.Round((total + quote.FreightTotal) * 10 / 100, 2) : 0;

            /***************************ADDING FREIGHT***************************/

            foreach (var item in quote.FreightDetails)
            {
                if (col == MigraDoc.DocumentObjectModel.Colors.AliceBlue)
                {
                    col = MigraDoc.DocumentObjectModel.Colors.White;
                }
                else
                {
                    col = MigraDoc.DocumentObjectModel.Colors.AliceBlue;
                }

                Row rowF = this.table.AddRow();
                rowF.TopPadding = 1.5;
                rowF.Shading.Color = col;

                rowF.Cells[0].AddParagraph(item.Pallets.ToString("G29"));
                rowF.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                rowF.Cells[0].Format.SpaceBefore = 2;
                rowF.Cells[0].Format.SpaceAfter = 2;
                rowF.Cells[0].Format.Font.Size = 8;
                rowF.Cells[0].Borders.Bottom.Visible = false;

                rowF.Cells[1].AddParagraph(item.FreightCodeDetails.Code);
                rowF.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                rowF.Cells[1].Format.SpaceBefore = 2;
                rowF.Cells[1].Format.SpaceAfter = 2;
                rowF.Cells[1].Format.Font.Size = 8;
                rowF.Cells[1].Borders.Bottom.Visible = false;

                rowF.Cells[2].AddParagraph(item.FreightCodeDetails.Description);
                rowF.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                rowF.Cells[2].Format.SpaceBefore = 2;
                rowF.Cells[2].Format.SpaceAfter = 2;
                rowF.Cells[2].Format.Font.Size = 8;
                rowF.Cells[2].Borders.Bottom.Visible = false;

                rowF.Cells[3].AddParagraph("");
                rowF.Cells[3].Format.Alignment = ParagraphAlignment.Right;
                rowF.Cells[3].Format.SpaceBefore = 2;
                rowF.Cells[3].Format.SpaceAfter = 2;
                rowF.Cells[3].Format.Font.Size = 8;
                rowF.Cells[3].Borders.Bottom.Visible = false;

                rowF.Cells[4].AddParagraph("$" + Math.Round(item.FreightCodeDetails.Price, 2));
                rowF.Cells[4].Format.Alignment = ParagraphAlignment.Right;
                rowF.Cells[4].Format.SpaceBefore = 2;
                rowF.Cells[4].Format.SpaceAfter = 2;
                rowF.Cells[4].Format.Font.Size = 8;
                rowF.Cells[4].Borders.Bottom.Visible = false;

                rowF.Cells[5].AddParagraph(item.FreightCodeDetails.Unit);
                rowF.Cells[5].Format.Alignment = ParagraphAlignment.Left;
                rowF.Cells[5].Format.SpaceBefore = 2;
                rowF.Cells[5].Format.SpaceAfter = 2;
                rowF.Cells[5].Format.Font.Size = 8;
                rowF.Cells[5].Borders.Bottom.Visible = false;

                rowF.Cells[6].AddParagraph(item.Discount + "%");
                rowF.Cells[6].Format.Alignment = ParagraphAlignment.Right;
                rowF.Cells[6].Format.SpaceBefore = 2;
                rowF.Cells[6].Format.SpaceAfter = 2;
                rowF.Cells[6].Format.Font.Size = 8;
                rowF.Cells[6].Borders.Bottom.Visible = false;

                rowF.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(item.Total, 2)));
                rowF.Cells[7].Format.Alignment = ParagraphAlignment.Right;
                rowF.Cells[7].Format.SpaceBefore = 2;
                rowF.Cells[7].Format.SpaceAfter = 2;
                rowF.Cells[7].Format.Font.Size = 8;
                rowF.Cells[7].Borders.Bottom.Visible = false;

            }


            /**********************END OF ADDING FREIGHT************************/
            int y = 11 - x;

            for (int i = 0; i < y; i++)
            {
                Row row = this.table.AddRow();
                row.TopPadding = 1.5;

                row.Cells[0].AddParagraph("");
                row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[0].Format.SpaceBefore = 2;
                row.Cells[0].Format.SpaceAfter = 2;
                row.Cells[0].Format.Font.Size = 8;
                row.Cells[0].Borders.Bottom.Visible = false;
                //row.Cells[0].MergeDown = y;

                row.Cells[1].AddParagraph("");
                row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[1].Format.SpaceBefore = 2;
                row.Cells[1].Format.SpaceAfter = 2;
                row.Cells[1].Format.Font.Size = 8;
                row.Cells[1].Borders.Bottom.Visible = false;
                //row.Cells[1].MergeDown = y;

                row.Cells[2].AddParagraph("");
                row.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[2].Format.SpaceBefore = 2;
                row.Cells[2].Format.SpaceAfter = 2;
                row.Cells[2].Format.Font.Size = 8;
                row.Cells[2].Borders.Bottom.Visible = false;

                row.Cells[3].AddParagraph("");
                row.Cells[3].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[3].Format.SpaceBefore = 2;
                row.Cells[3].Format.SpaceAfter = 2;
                row.Cells[3].Format.Font.Size = 8;
                row.Cells[3].Borders.Bottom.Visible = false;

                row.Cells[4].AddParagraph("");
                row.Cells[4].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[4].Format.SpaceBefore = 2;
                row.Cells[4].Format.SpaceAfter = 2;
                row.Cells[4].Format.Font.Size = 8;
                row.Cells[4].Borders.Bottom.Visible = false;
                //row.Cells[3].MergeDown = y;

                row.Cells[5].AddParagraph("");
                row.Cells[5].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[5].Format.SpaceBefore = 2;
                row.Cells[5].Format.SpaceAfter = 2;
                row.Cells[5].Format.Font.Size = 8;
                row.Cells[5].Borders.Bottom.Visible = false;
                //row.Cells[4].MergeDown = y;

                row.Cells[6].AddParagraph("");
                row.Cells[6].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[6].Format.SpaceBefore = 2;
                row.Cells[6].Format.SpaceAfter = 2;
                row.Cells[6].Format.Font.Size = 8;
                row.Cells[6].Borders.Bottom.Visible = false;
                //row.Cells[5].MergeDown = y;

                row.Cells[7].AddParagraph("");
                row.Cells[7].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[7].Format.SpaceBefore = 2;
                row.Cells[7].Format.SpaceAfter = 2;
                row.Cells[7].Format.Font.Size = 8;
                //row.Cells[6].MergeDown = y;
                row.Cells[7].Borders.Bottom.Visible = false;

                x++;
            }


            //// Add an invisible row as a space line to the table
            //Row rowListPrice = this.table.AddRow();
            //rowListPrice.Borders.Visible = false;
            //rowListPrice.TopPadding = "0.6cm";

            Row rowListPrice = this.table.AddRow();
            // Add Quote text
            rowListPrice = this.table.AddRow();
            rowListPrice.Cells[0].Borders.Visible = true;
            rowListPrice.Cells[0].Format.Font.Size = 10;
            rowListPrice.Cells[0].AddParagraph("Quoted By: " + quote.User.FullName);
            rowListPrice.Cells[0].Format.Font.Bold = false;
            rowListPrice.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowListPrice.Cells[0].MergeRight = 7;

            rowListPrice = this.table.AddRow();
            rowListPrice.Cells[0].AddParagraph("Comments :" + quote.Instructions);
            rowListPrice.Cells[0].Format.Font.Name = "Calibri";
            rowListPrice.Cells[0].Format.Font.Size = 10;
            rowListPrice.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowListPrice.Cells[0].MergeRight = 3;
            rowListPrice.Cells[0].Format.SpaceBefore = 2;
            rowListPrice.Cells[0].Format.SpaceAfter = 2;
            rowListPrice.Cells[0].Borders.Visible = true;

            rowListPrice.Cells[0].Format.Font.Bold = false;
            rowListPrice.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowListPrice.Cells[0].MergeRight = 3;
            rowListPrice.Cells[0].MergeDown = 2;
            rowListPrice.Cells[4].Borders.Visible = false;

            // Add the list price row
            rowListPrice.Cells[4].Borders.Visible = true;
            rowListPrice.Cells[4].AddParagraph("List Price Total");
            rowListPrice.Cells[4].Format.Font.Name = "Calibri";
            rowListPrice.Cells[4].Format.Font.Size = 12;
            rowListPrice.Cells[4].Format.Font.Bold = true;
            rowListPrice.Cells[4].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[4].MergeRight = 2;

            rowListPrice.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(quote.ListPriceTotal, 2)));
            rowListPrice.Cells[7].Format.Font.Size = 9;
            rowListPrice.Cells[7].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[7].Format.SpaceBefore = 2;
            rowListPrice.Cells[7].Format.SpaceAfter = 2;

            //Discounted Total
            rowListPrice = this.table.AddRow();
            rowListPrice.Cells[0].Borders.Visible = true;
            rowListPrice.Cells[0].Format.Font.Size = 8;
            rowListPrice.Cells[0].AddParagraph();
            rowListPrice.Cells[0].Format.Font.Bold = false;
            rowListPrice.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowListPrice.Cells[0].MergeRight = 2;
            rowListPrice.Cells[4].Borders.Visible = false;

            rowListPrice.Cells[4].Borders.Visible = true;
            rowListPrice.Cells[4].AddParagraph("Discounted Total");
            rowListPrice.Cells[4].Format.Font.Name = "Calibri";
            rowListPrice.Cells[4].Format.Font.Size = 12;
            rowListPrice.Cells[4].Format.Font.Bold = true;
            rowListPrice.Cells[4].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[4].MergeRight = 2;

            decimal disTotal = (quote.ListPriceTotal - quote.DiscountedTotal) < 0 ? 0 : quote.ListPriceTotal - quote.DiscountedTotal;

            rowListPrice.Cells[7].Format.Font.Size = 9;
            rowListPrice.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(disTotal, 2)));
            rowListPrice.Cells[7].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[7].Format.SpaceBefore = 2;
            rowListPrice.Cells[7].Format.SpaceAfter = 2;

            //Freight
            rowListPrice = this.table.AddRow();
            rowListPrice.Cells[0].Borders.Visible = true;
            rowListPrice.Cells[0].Format.Font.Size = 8;
            rowListPrice.Cells[0].AddParagraph();
            rowListPrice.Cells[0].Format.Font.Bold = false;
            rowListPrice.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowListPrice.Cells[0].MergeRight = 2;

            rowListPrice.Cells[4].Borders.Visible = true;
            rowListPrice.Cells[4].AddParagraph("Freight");
            rowListPrice.Cells[4].Format.Font.Name = "Calibri";
            rowListPrice.Cells[4].Format.Font.Size = 12;
            rowListPrice.Cells[4].Format.Font.Bold = true;
            rowListPrice.Cells[4].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[4].MergeRight = 2;

            rowListPrice.Cells[7].Format.Font.Size = 9;
            rowListPrice.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(quote.FreightTotal, 2)));
            rowListPrice.Cells[7].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[7].Format.SpaceBefore = 2;
            rowListPrice.Cells[7].Format.SpaceAfter = 2;

            //GST
            rowListPrice = this.table.AddRow();
            rowListPrice.Cells[0].Borders.Visible = true;
            rowListPrice.Cells[0].Format.Font.Size = 6;
            rowListPrice.Cells[0].AddParagraph("This Quote is valid for 30 days only and cannot be used as an invoice. Should you wish to proceed, please send an acceptance of this quote and you will be sent an invoice.");
            rowListPrice.Cells[0].Format.Font.Bold = false;
            rowListPrice.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowListPrice.Cells[0].MergeRight = 3;
            rowListPrice.Cells[0].MergeDown = 1;

            rowListPrice.Cells[4].Borders.Visible = true;
            rowListPrice.Cells[4].AddParagraph("GST");
            rowListPrice.Cells[4].Format.Font.Name = "Calibri";
            rowListPrice.Cells[4].Format.Font.Size = 12;
            rowListPrice.Cells[4].Format.Font.Bold = true;
            rowListPrice.Cells[4].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[4].MergeRight = 2;

            rowListPrice.Cells[7].Format.Font.Size = 9;
            rowListPrice.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(quote.Gst, 2)));
            rowListPrice.Cells[7].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[7].Format.SpaceBefore = 2;
            rowListPrice.Cells[7].Format.SpaceAfter = 2;

            // Add the sub total row
            rowListPrice = this.table.AddRow();
            rowListPrice.Cells[0].Borders.Visible = true;
            rowListPrice.Cells[0].Format.Font.Size = 9;
            rowListPrice.Cells[0].AddParagraph();
            rowListPrice.Cells[0].Format.Font.Bold = false;
            rowListPrice.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowListPrice.Cells[0].MergeRight = 2;

            rowListPrice.Cells[4].Borders.Visible = true;
            rowListPrice.Cells[4].AddParagraph("Total Amount");
            rowListPrice.Cells[4].Format.Font.Name = "Calibri";
            rowListPrice.Cells[4].Format.Font.Bold = true;
            rowListPrice.Cells[4].Format.Font.Size = 16;
            rowListPrice.Cells[4].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[4].MergeRight = 2;

            rowListPrice.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(quote.TotalAmount, 2)));
            rowListPrice.Cells[7].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[7].Format.Font.Size = 10;
            rowListPrice.Cells[7].Format.Font.Bold = true;
            rowListPrice.Cells[7].Format.SpaceBefore = 2;
            rowListPrice.Cells[7].Format.SpaceAfter = 2;

            // Add an invisible row as a space line to the table
            Row inv1 = this.table.AddRow();
            inv1.Borders.Visible = false;
            inv1.TopPadding = "0.2cm";

            Row inv2 = this.table.AddRow();
            inv2.Borders.Visible = false;
            inv1.TopPadding = "-0.3cm";

            //Disclaimer
            rowListPrice = this.table.AddRow();
            rowListPrice.Cells[0].Borders.Visible = false;
            rowListPrice.Cells[0].Format.Font.Size = 10;
            rowListPrice.Cells[0].AddParagraph("Disclaimer: Please help to ensure you are getting the correct information you requested by reading this quotation carefully, as we don’t warrant that the products quoted are as requested. Under no circumstances do we make the representation that the products quoted here are as requested, as we may have misunderstood your request, so we accept no liability for any loss or damage which you may suffer if you fail to check the accuracy of the products quoted against those that you’ve requested, thankyou.");
            rowListPrice.Cells[0].Format.Font.Bold = false;
            rowListPrice.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowListPrice.Cells[0].MergeRight = 7;
            rowListPrice.Cells[7].Borders.Visible = false;



        }

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }


    }
}

