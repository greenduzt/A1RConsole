using A1RConsole.Models.Customers;
using A1RConsole.Models.Quoting;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.PdfGeneration
{
    public class OpenQuotesPDF
    {
        Document document;
        MigraDoc.DocumentObjectModel.Shapes.TextFrame addressFrame;
        MigraDoc.DocumentObjectModel.Tables.Table table;
        private DateTime currentDate;
        private ObservableCollection<OpenQuotes> openQuotes;
        public OpenQuotesPDF(ObservableCollection<OpenQuotes> oq)
        {
            currentDate = DateTime.Now;
            openQuotes = oq;
        }

        public Tuple<Exception, string> CreatePDF()
        {
            string filePath = string.Empty;
            Exception res = null;
            Document document = CreateDocument();
            document.UseCmykColor = true;
            document.DefaultPageSetup.FooterDistance = "-4cm";
            document.DefaultPageSetup.LeftMargin = "0.2cm";
            document.DefaultPageSetup.RightMargin = "0.2cm";
            document.DefaultPageSetup.TopMargin = "3.4cm";
            document.DefaultPageSetup.Orientation = Orientation.Landscape;

            const bool unicode = false;
            const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();

            string filename = "OQ" + currentDate.ToString("_ddMMyyyy_HHmmss") + ".pdf";
            try
            {
                filePath = "S:/sales support/customers/customer quotes/Open_quotes/" + filename;
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
            this.document.Info.Title = "Open Quotes";
            this.document.Info.Subject = "Open Quotes";
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
            this.addressFrame.Width = "28.6cm";
            this.addressFrame.Left = ShapePosition.Right;
            this.addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            this.addressFrame.RelativeVertical = RelativeVertical.Page;
            this.addressFrame.MarginTop = "-2cm";

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            paragraph = this.addressFrame.AddParagraph("Open Quotes");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 16;
            paragraph.Format.SpaceBefore = "4cm";
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Center;
                       

            //Footer page numbers
            TextFrame textFrame2 = section.Footers.Primary.AddTextFrame();
            textFrame2.Width = "18cm";
            textFrame2.Top = "10cm";
            Paragraph footerPara2 = textFrame2.AddParagraph();
            footerPara2.AddText("");
            footerPara2.Format.Font.Size = 7;
            footerPara2.Format.Alignment = ParagraphAlignment.Left;

            footerPara2 = textFrame2.AddParagraph();
            footerPara2.AddText("Printed Date - " + currentDate.ToString("dd/MM/yyyy") + " at " + DateTime.Now.ToString("hh:mm tt"));
            footerPara2.Format.Font.Size = 7;
            footerPara2.Format.Alignment = ParagraphAlignment.Left;

            TextFrame textFrame1 = section.Footers.Primary.AddTextFrame();
            textFrame1.Width = "28.5cm";
            Paragraph footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("www.a1rubber.com");
            footerPara1.Format.Font.Size = 7;
            footerPara1.Format.Alignment = ParagraphAlignment.Right;

            footerPara1 = textFrame1.AddParagraph();
            footerPara1.AddText("©Copyright A1Rubber "+ DateTime.Now.Year);
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
            paragraph.Format.SpaceBefore = "-2.5cm";
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
            this.table.Borders.DistanceFromLeft = -1.0;

            Column itemsCol = this.table.AddColumn("1.3cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("1.8cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("4.3cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("4.3cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("8cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("2.2cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("4.7cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("2.5cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            Row itemRow = table.AddRow();
            itemRow.HeadingFormat = true;
            itemRow.Format.Alignment = ParagraphAlignment.Center;
            itemRow.Format.Font.Bold = true;

            itemRow.Cells[0].AddParagraph("Quote No");
            itemRow.Cells[0].Format.Font.Size = 9;
            itemRow.Cells[0].Format.Font.Bold = true;
            itemRow.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[1].AddParagraph("Quote Date");
            itemRow.Cells[1].Format.Font.Size = 9;
            itemRow.Cells[1].Format.Font.Bold = true;
            itemRow.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[2].AddParagraph("Customer");
            itemRow.Cells[2].Format.Font.Size = 9;
            itemRow.Cells[2].Format.Font.Bold = true;
            itemRow.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[3].AddParagraph("Project Name");
            itemRow.Cells[3].Format.Font.Size = 9;
            itemRow.Cells[3].Format.Font.Bold = true;
            itemRow.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[4].AddParagraph("Product Details");
            itemRow.Cells[4].Format.Font.Size = 9;
            itemRow.Cells[4].Format.Font.Bold = true;
            itemRow.Cells[4].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[5].AddParagraph("Total");
            itemRow.Cells[5].Format.Font.Size = 9;
            itemRow.Cells[5].Format.Font.Bold = true;
            itemRow.Cells[5].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[5].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[5].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[6].AddParagraph("Contact");
            itemRow.Cells[6].Format.Font.Size = 9;
            itemRow.Cells[6].Format.Font.Bold = true;
            itemRow.Cells[6].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[6].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[6].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemRow.Cells[7].AddParagraph("Quoted By");
            itemRow.Cells[7].Format.Font.Size = 9;
            itemRow.Cells[7].Format.Font.Bold = true;
            itemRow.Cells[7].Format.Alignment = ParagraphAlignment.Center;
            itemRow.Cells[7].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            itemRow.Cells[7].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            foreach (var item in openQuotes)
            {
                Row row2 = table.AddRow();
                row2.HeadingFormat = true;
                row2.Format.Alignment = ParagraphAlignment.Center;
                row2.Format.Font.Bold = false;
                row2.BottomPadding = 1;

                row2.Cells[0].AddParagraph(item.QuoteNo.ToString());
                row2.Cells[0].Format.Font.Size = 9;
                row2.Cells[0].Format.Font.Bold = true;
                row2.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row2.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[0].Borders.Width = 0;

                row2.Cells[1].AddParagraph(item.QuoteDate.ToString("dd/MM/yyyy"));
                row2.Cells[1].Format.Font.Size = 9;
                row2.Cells[1].Format.Font.Bold = true;
                row2.Cells[1].Format.Alignment = ParagraphAlignment.Center;
                row2.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[1].Borders.Width = 0;

                row2.Cells[2].AddParagraph(item.Customer.CompanyName);
                row2.Cells[2].Format.Font.Size = 9;
                row2.Cells[2].Format.Font.Bold = true;
                row2.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                row2.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[2].Borders.Width = 0;

                row2.Cells[3].AddParagraph(item.ProjectName);
                row2.Cells[3].Format.Font.Size = 9;
                row2.Cells[3].Format.Font.Bold = true;
                row2.Cells[3].Format.Alignment = ParagraphAlignment.Left;
                row2.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[3].Borders.Width = 0;

                row2.Cells[4].AddParagraph(item.ProductDetails);
                row2.Cells[4].Format.Font.Size = 9;
                row2.Cells[4].Format.Font.Bold = true;
                row2.Cells[4].Format.Alignment = ParagraphAlignment.Left;
                row2.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[4].Borders.Width = 0;

                row2.Cells[5].AddParagraph(Convert.ToDecimal(item.TotalAmount).ToString("C", CultureInfo.CurrentCulture));
                row2.Cells[5].Format.Font.Size = 9;
                row2.Cells[5].Format.Font.Bold = true;
                row2.Cells[5].Format.Alignment = ParagraphAlignment.Right;
                row2.Cells[5].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[5].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[5].Borders.Width = 0;

                row2.Cells[6].AddParagraph(item.ContactPerson.ContactPersonName);
                row2.Cells[6].Format.Font.Size = 9;
                row2.Cells[6].Format.Font.Bold = true;
                row2.Cells[6].Format.Alignment = ParagraphAlignment.Left;
                row2.Cells[6].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[6].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[6].Borders.Width = 0;

                row2.Cells[7].AddParagraph(item.User.FullName);
                row2.Cells[7].Format.Font.Size = 9;
                row2.Cells[7].Format.Font.Bold = true;
                row2.Cells[7].Format.Alignment = ParagraphAlignment.Left;
                row2.Cells[7].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row2.Cells[7].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row2.Cells[7].Borders.Width = 0;
            }

        }
    }
}
