using A1RConsole.Models.Orders;
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
    public class PrintSalesReportPDF
    {
        private IOrderedEnumerable<SalesOrder> soWithDuplicates;
        private List<SalesOrder> soNoDuplicates;
        private DateTime currentDate;
        private string fromDate;
        private string toDate;
        private string customerName;
        Document document;
        MigraDoc.DocumentObjectModel.Shapes.TextFrame addressFrame;
        MigraDoc.DocumentObjectModel.Tables.Table table;

        public PrintSalesReportPDF(IOrderedEnumerable<SalesOrder> soWD, List<SalesOrder> soND, string fd, string td, string cn)
        {
            currentDate = DateTime.Now;
            soWithDuplicates = soWD;
            soNoDuplicates = soND;
            fromDate = fd;
            toDate = td;
            customerName = cn;
        }

        public Tuple<Exception, string> CreatePDF()
        {
            string filePath = string.Empty;
            Exception res = null;
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

            string filename = "SR" + currentDate.ToString("_ddMMyyyy_HHmmss") + ".pdf";
            try
            {
                filePath = "S:/PRODUCTION/DONOTDELETE/SalesReport/" + filename;
                pdfRenderer.PdfDocument.Save(filePath);

                //ProcessStartInfo info = new ProcessStartInfo("S:/PRODUCTION/DONOTDELETE/SalesReport/" + filename);
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
            this.document.Info.Title = "Sales Report";
            this.document.Info.Subject = "Sales Report";
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
            this.addressFrame.MarginTop = "-0.5cm";

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            paragraph = this.addressFrame.AddParagraph("Sales Report");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 16;
            paragraph.Format.SpaceBefore = "4cm";
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            if (!String.IsNullOrWhiteSpace(fromDate) || !String.IsNullOrWhiteSpace(toDate) || !String.IsNullOrWhiteSpace(customerName))
            {
                string ft = string.Empty;

                if (!String.IsNullOrWhiteSpace(fromDate) && !String.IsNullOrWhiteSpace(toDate))
                {
                    ft = "From - " + fromDate + "   " + "To - " + toDate;
                }
                else if (!String.IsNullOrWhiteSpace(fromDate) && String.IsNullOrWhiteSpace(toDate))
                {
                    ft = "From - " + fromDate + "   " + "To - " + fromDate;
                }
                else if (String.IsNullOrWhiteSpace(fromDate) && !String.IsNullOrWhiteSpace(toDate))
                {
                    ft = "From - " + toDate + "   " + "To - " + toDate;
                }

                if (!String.IsNullOrWhiteSpace(customerName))
                {
                    ft += "  |  " + "Customer Name - " + customerName;
                }

                paragraph = section.AddParagraph(ft);
                paragraph.Format.SpaceBefore = "-1cm";
                paragraph.Style = "Reference";
                paragraph.AddTab();
            }

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

            MigraDoc.DocumentObjectModel.Tables.Column itemsCol = this.table.AddColumn("8.50cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("2cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            itemsCol = this.table.AddColumn("8cm");
            itemsCol.Format.Alignment = ParagraphAlignment.Center;
            itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            //itemsCol = this.table.AddColumn("2cm");
            //itemsCol.Format.Alignment = ParagraphAlignment.Center;
            //itemsCol.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            //Row itemRow = table.AddRow();
            //itemRow.HeadingFormat = true;
            //itemRow.Format.Alignment = ParagraphAlignment.Center;
            //itemRow.Format.Font.Bold = true;

            //itemRow.Cells[0].AddParagraph("Order Date");
            //itemRow.Cells[0].Format.Font.Size = 9;
            //itemRow.Cells[0].Format.Font.Bold = true;
            //itemRow.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            //itemRow.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            //itemRow.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            //itemRow.Cells[0].Borders.Width = 0;

            //itemRow.Cells[1].AddParagraph("Customer");
            //itemRow.Cells[1].Format.Font.Size = 9;
            //itemRow.Cells[1].Format.Font.Bold = true;
            //itemRow.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            //itemRow.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            //itemRow.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            //itemRow.Cells[1].Borders.Width = 0;

            //itemRow.Cells[2].AddParagraph("Sales Order");
            //itemRow.Cells[2].Format.Font.Size = 9;
            //itemRow.Cells[2].Format.Font.Bold = true;
            //itemRow.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            //itemRow.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            //itemRow.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            //itemRow.Cells[2].Borders.Width = 0;

            //itemRow.Cells[3].AddParagraph("Total");
            //itemRow.Cells[3].Format.Font.Size = 9;
            //itemRow.Cells[3].Format.Font.Bold = true;
            //itemRow.Cells[3].Format.Alignment = ParagraphAlignment.Center;
            //itemRow.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            //itemRow.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            //itemRow.Cells[3].Borders.Width = 0;

            decimal totalAmount = 0;

            foreach (var item in soNoDuplicates)
            {
                decimal total = 0;
                Row row1 = table.AddRow();
                row1.HeadingFormat = true;
                row1.Format.Alignment = ParagraphAlignment.Center;
                row1.Format.Font.Bold = false;
                row1.BottomPadding = 1;

                row1.Cells[0].AddParagraph(item.OrderDate.ToString("dd/MM/yyyy"));
                row1.Cells[0].Format.Font.Size = 9;
                row1.Cells[0].Format.Font.Bold = true;
                row1.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                row1.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row1.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row1.Cells[0].Borders.Left.Width = 0;
                row1.Cells[0].Borders.Right.Width = 0;
                row1.Cells[0].Borders.Top.Width = 0;
                row1.Cells[0].Borders.Bottom.Width = 1;

                row1.Cells[1].AddParagraph("Order No");
                row1.Cells[1].Format.Font.Size = 9;
                row1.Cells[1].Format.Font.Bold = true;
                row1.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                row1.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row1.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row1.Cells[1].MergeRight = 1;
                row1.Cells[1].Borders.Left.Width = 0;
                row1.Cells[1].Borders.Right.Width = 0;
                row1.Cells[1].Borders.Top.Width = 0;
                row1.Cells[1].Borders.Bottom.Width = 1;
                row1.Cells[2].Borders.Right.Width = 0;

                foreach (var items in soWithDuplicates)
                {
                    if (item.OrderDate == items.OrderDate)
                    {
                        Row row2 = table.AddRow();
                        row2.HeadingFormat = true;
                        row2.Format.Alignment = ParagraphAlignment.Center;
                        row2.Format.Font.Bold = false;
                        row2.BottomPadding = 1;

                        row2.Cells[0].AddParagraph(items.Customer.CompanyName);
                        row2.Cells[0].Format.Font.Size = 9;
                        row2.Cells[0].Format.Font.Bold = true;
                        row2.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                        row2.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                        row2.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                        row2.Cells[0].Borders.Left.Width = 0;
                        row2.Cells[0].Borders.Right.Width = 0;
                        row2.Cells[0].Borders.Top.Width = 0;
                        row2.Cells[0].Borders.Bottom.Width = 0;

                        row2.Cells[1].AddParagraph(items.SalesOrderNo.ToString());
                        row2.Cells[1].Format.Font.Size = 9;
                        row2.Cells[1].Format.Font.Bold = true;
                        row2.Cells[1].Format.Alignment = ParagraphAlignment.Center;
                        row2.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                        row2.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                        row2.Cells[1].Borders.Left.Width = 0;
                        row2.Cells[1].Borders.Right.Width = 0;
                        row2.Cells[1].Borders.Top.Width = 0;
                        row2.Cells[1].Borders.Bottom.Width = 0;

                        row2.Cells[2].AddParagraph(string.Format("{0:C}", items.ListPriceTotal));
                        row2.Cells[2].Format.Font.Size = 9;
                        row2.Cells[2].Format.Font.Bold = true;
                        row2.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                        row2.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                        row2.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                        row2.Cells[2].Borders.Left.Width = 0;
                        row2.Cells[2].Borders.Right.Width = 0;
                        row2.Cells[2].Borders.Top.Width = 0;
                        row2.Cells[2].Borders.Bottom.Width = 0;
                        total += items.ListPriceTotal;
                        totalAmount += items.ListPriceTotal;
                    }
                }
                Row row3 = table.AddRow();
                row3.HeadingFormat = true;
                row3.Format.Alignment = ParagraphAlignment.Center;
                row3.Format.Font.Bold = false;
                row3.BottomPadding = 1;

                row3.Cells[0].AddParagraph("");
                row3.Cells[0].Format.Font.Size = 9;
                row3.Cells[0].Format.Font.Bold = true;
                row3.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row3.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row3.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row3.Cells[0].Borders.Left.Width = 0;
                row3.Cells[0].Borders.Right.Width = 0;
                row3.Cells[0].Borders.Top.Width = 0;
                row3.Cells[0].Borders.Bottom.Width = 0;

                row3.Cells[1].AddParagraph("");
                row3.Cells[1].Format.Font.Size = 9;
                row3.Cells[1].Format.Font.Bold = true;
                row3.Cells[1].Format.Alignment = ParagraphAlignment.Right;
                row3.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row3.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row3.Cells[1].Borders.Left.Width = 0;
                row3.Cells[1].Borders.Right.Width = 0;
                row3.Cells[1].Borders.Top.Width = 0;
                row3.Cells[1].Borders.Bottom.Width = 0;

                row3.Cells[2].AddParagraph("Total : " + string.Format("{0:C}", total));
                row3.Cells[2].Format.Font.Size = 9;
                row3.Cells[2].Format.Font.Bold = true;
                row3.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                row3.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row3.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row3.Cells[2].Borders.Left.Width = 0;
                row3.Cells[2].Borders.Right.Width = 0;


                Row row4 = table.AddRow();
                row4.HeadingFormat = true;
                row4.Format.Alignment = ParagraphAlignment.Center;
                row4.Format.Font.Bold = false;
                row4.BottomPadding = 1;

                row4.Cells[0].AddParagraph("");
                row4.Cells[0].Format.Font.Size = 9;
                row4.Cells[0].Format.Font.Bold = true;
                row4.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row4.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row4.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row4.Cells[0].Borders.Left.Width = 0;
                row4.Cells[0].Borders.Right.Width = 0;
                row4.Cells[0].Borders.Top.Width = 0;
                row4.Cells[0].Borders.Bottom.Width = 0;

                row4.Cells[1].AddParagraph("");
                row4.Cells[1].Format.Font.Size = 9;
                row4.Cells[1].Format.Font.Bold = true;
                row4.Cells[1].Format.Alignment = ParagraphAlignment.Right;
                row4.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row4.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row4.Cells[1].Borders.Left.Width = 0;
                row4.Cells[1].Borders.Right.Width = 0;
                row4.Cells[1].Borders.Top.Width = 0;
                row4.Cells[1].Borders.Bottom.Width = 0;

                row4.Cells[2].AddParagraph("");
                row4.Cells[2].Format.Font.Size = 9;
                row4.Cells[2].Format.Font.Bold = true;
                row4.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                row4.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row4.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
                row4.Cells[2].Borders.Left.Width = 0;
                row4.Cells[2].Borders.Right.Width = 0;
                row4.Cells[2].Borders.Top.Width = 0;
                row4.Cells[2].Borders.Bottom.Width = 0;

            }

            Row row5 = table.AddRow();
            row5.HeadingFormat = true;
            row5.Format.Alignment = ParagraphAlignment.Center;
            row5.Format.Font.Bold = false;
            row5.BottomPadding = 1;

            row5.Cells[0].AddParagraph("");
            row5.Cells[0].Format.Font.Size = 9;
            row5.Cells[0].Format.Font.Bold = true;
            row5.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row5.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row5.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row5.Cells[0].Borders.Left.Width = 0;
            row5.Cells[0].Borders.Right.Width = 0;
            row5.Cells[0].Borders.Top.Width = 0;
            row5.Cells[0].Borders.Bottom.Width = 0;

            row5.Cells[1].AddParagraph("");
            row5.Cells[1].Format.Font.Size = 9;
            row5.Cells[1].Format.Font.Bold = true;
            row5.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            row5.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row5.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row5.Cells[1].Borders.Left.Width = 0;
            row5.Cells[1].Borders.Right.Width = 0;
            row5.Cells[1].Borders.Top.Width = 0;
            row5.Cells[1].Borders.Bottom.Width = 0;

            row5.Cells[2].AddParagraph("");
            row5.Cells[2].Format.Font.Size = 9;
            row5.Cells[2].Format.Font.Bold = true;
            row5.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row5.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row5.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row5.Cells[2].Borders.Left.Width = 0;
            row5.Cells[2].Borders.Right.Width = 0;
            row5.Cells[2].Borders.Top.Width = 0;
            row5.Cells[2].Borders.Bottom.Width = 0;

            Row row6 = table.AddRow();
            row6.HeadingFormat = true;
            row6.Format.Alignment = ParagraphAlignment.Center;
            row6.Format.Font.Bold = false;
            row6.BottomPadding = 1;

            row6.Cells[0].AddParagraph("");
            row6.Cells[0].Format.Font.Size = 9;
            row6.Cells[0].Format.Font.Bold = true;
            row6.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row6.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row6.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row6.Cells[0].Borders.Left.Width = 0;
            row6.Cells[0].Borders.Right.Width = 0;
            row6.Cells[0].Borders.Top.Width = 0;
            row6.Cells[0].Borders.Bottom.Width = 0;

            row6.Cells[1].AddParagraph("");
            row6.Cells[1].Format.Font.Size = 9;
            row6.Cells[1].Format.Font.Bold = true;
            row6.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            row6.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row6.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row6.Cells[1].Borders.Left.Width = 0;
            row6.Cells[1].Borders.Right.Width = 0;
            row6.Cells[1].Borders.Top.Width = 0;
            row6.Cells[1].Borders.Bottom.Width = 0;

            row6.Cells[2].AddParagraph("Total Amount : " + string.Format("{0:C}", totalAmount));
            row6.Cells[2].Format.Font.Size = 12;
            row6.Cells[2].Format.Font.Bold = true;
            row6.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            row6.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row6.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
            row6.Cells[2].Borders.Left.Width = 0;
            row6.Cells[2].Borders.Right.Width = 0;
            row6.Cells[2].Borders.Top.Width = 0;
            row6.Cells[2].Borders.Bottom.Width = 0;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();
        }
    }
}
