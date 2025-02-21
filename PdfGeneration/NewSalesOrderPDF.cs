using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Orders;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace A1RConsole.PdfGeneration
{
    public class NewSalesOrderPDF
    {
        private int orderNo;
        Document document;
        TextFrame addressFrame;
        MigraDoc.DocumentObjectModel.Tables.Table table;
        private NewOrderPDFM newOrderPDFM;
        //private string prePaidCustomer;
        
        public NewSalesOrderPDF(int id, string fileName)
        {
            orderNo = id;
            newOrderPDFM = new NewOrderPDFM();
            newOrderPDFM = DBAccess.GetNewOrderPDF(orderNo);
            newOrderPDFM.FileName = fileName;
            //prePaidCustomer = ppc;
        }       

        public Exception CreateQuote()
        {
            Exception res = null;
            //string filePath = string.Empty;
            Document document = CreateDocument();
            document.UseCmykColor = true;
            document.DefaultPageSetup.FooterDistance = "-2.0cm";
            document.DefaultPageSetup.LeftMargin = "1.2cm";
            document.DefaultPageSetup.RightMargin = "1.2cm";
            const bool unicode = false;
            const PdfFontEmbedding embedding = PdfFontEmbedding.Always;
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();
            DateTime currentDate = DateTime.Now;

            try
            {
                pdfRenderer.PdfDocument.Save(FilePathManager.GetNewOrderSavingPath() + "/" + newOrderPDFM.FileName);
            }
            catch (Exception ex)
            {
                res = ex;
            }

            return res;
        }

        public Document CreateDocument()
        {
            // Create a new MigraDoc document
            this.document = new Document();
            this.document.Info.Title = "A1 Rubber Sales Order";
            this.document.Info.Subject = "A1 Rubber Sales Order";
            this.document.Info.Author = "A1 Rubber";

            DefineStyles();
            CreatePage();
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

        void CreatePage()
        {

            //Build shipping address as a string
            DateTime reqDate = Convert.ToDateTime(newOrderPDFM.RequiredDate);
            string rDate = reqDate.ToString("dd/MM/yyyy");

            string shipAddress = !String.IsNullOrWhiteSpace(newOrderPDFM.Customer.ShipAddress) ? newOrderPDFM.Customer.ShipAddress + "," + System.Environment.NewLine + newOrderPDFM.Customer.ShipCity : newOrderPDFM.Customer.ShipAddress;

            shipAddress = !String.IsNullOrWhiteSpace(newOrderPDFM.Customer.ShipState) ? shipAddress + "," + System.Environment.NewLine + newOrderPDFM.Customer.ShipState : shipAddress;

            shipAddress = !String.IsNullOrWhiteSpace(newOrderPDFM.Customer.ShipPostCode) ? shipAddress + "," + newOrderPDFM.Customer.ShipPostCode : shipAddress;

            if (newOrderPDFM.Customer.ShipCountry != "Australia")
            {
                shipAddress = !String.IsNullOrWhiteSpace(newOrderPDFM.Customer.ShipCountry) ? shipAddress + "," + System.Environment.NewLine + newOrderPDFM.Customer.ShipCountry : shipAddress;
            }

            shipAddress += System.Environment.NewLine + "Date Req : " + rDate + " " + newOrderPDFM.DateTypeRequired;

            // Each MigraDoc document needs at least one section.
            MigraDoc.DocumentObjectModel.Section section = this.document.AddSection();

            // Put a logo in the header
            MigraDoc.DocumentObjectModel.Shapes.Image image = section.Headers.Primary.AddImage("S:/PRODUCTION/DONOTDELETE/Images/A1Rubber-CMYK-INV_jpg.jpg");
            image.Height = "2.2cm";
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Left;
            image.WrapFormat.Style = WrapStyle.Through;

            // Create footer
            MigraDoc.DocumentObjectModel.Paragraph paragraph = section.Footers.Primary.AddParagraph();

            MigraDoc.DocumentObjectModel.Paragraph paragraph1 = section.Footers.Primary.AddParagraph();
            paragraph1.AddText("QLD Delivery/Pickup Address\n34 Binary Street\nYatala QLD 4207\nPh: 07 3807 3666\nFax: 07 3807 2344");
            paragraph1.Format.Font.Size = 8;
            paragraph1.Format.Font.Bold = false;
            paragraph1.Format.Alignment = ParagraphAlignment.Left;

            MigraDoc.DocumentObjectModel.Paragraph paragraph2 = section.Footers.Primary.AddParagraph();
            paragraph2.AddText("NSW Delivery/Pickup Address\n40 Bentley Street\nWetherill Park NSW 2164\nPh: 02 9756 2146\nFax: 02 9756 2149");
            paragraph2.Format.Font.Size = 8;
            paragraph2.Format.Font.Bold = false;
            paragraph2.Format.Alignment = ParagraphAlignment.Right;

            MigraDoc.DocumentObjectModel.Paragraph paragraph3 = section.Footers.Primary.AddParagraph();
            paragraph3.AddText("VIC Sales Representative\nC/- Leeanne Pagel\nPh: 0408 607 888");
            paragraph3.Format.Font.Size = 8;
            paragraph3.Format.Font.Bold = false;
            paragraph3.Format.Alignment = ParagraphAlignment.Center;

            this.addressFrame = section.AddTextFrame();
            this.addressFrame.MarginLeft = "3.8cm";
            this.addressFrame.Height = "3.0cm";
            this.addressFrame.Width = "7.0cm";
            this.addressFrame.Left = ShapePosition.Right;
            this.addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            this.addressFrame.Top = "-0.1cm";
            this.addressFrame.RelativeVertical = RelativeVertical.Page;

            paragraph = this.addressFrame.AddParagraph("NEW ORDER");
            paragraph.Format.Font.Name = "Calibri";
            paragraph.Format.Font.Size = 17;
            paragraph.Format.SpaceBefore = "2cm";
            paragraph.Format.Font.Bold = true;

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.1cm";
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


            Column column2 = this.table.AddColumn("12.4cm");
            column2.Format.Alignment = ParagraphAlignment.Center;

            column2 = this.table.AddColumn("2.05cm");
            column2.Format.Alignment = ParagraphAlignment.Center;
            column2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column2 = this.table.AddColumn("2.05cm");
            column2.Format.Alignment = ParagraphAlignment.Center;
            column2.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column2 = this.table.AddColumn("2.05cm");
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

            row3.Cells[1].AddParagraph("Order Date");
            row3.Cells[1].Format.Font.Size = 9;
            row3.Cells[1].Format.Font.Bold = true;
            row3.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row3.Cells[1].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Green;
            row3.Cells[1].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

            row3.Cells[2].AddParagraph("Order No");
            row3.Cells[2].Format.Font.Size = 9;
            row3.Cells[2].Format.Font.Bold = true;
            row3.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row3.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Green;
            row3.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

            row3.Cells[3].AddParagraph("Quote No");
            row3.Cells[3].Format.Font.Size = 9;
            row3.Cells[3].Format.Font.Bold = true;
            row3.Cells[3].Format.Alignment = ParagraphAlignment.Left;
            row3.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Green;
            row3.Cells[3].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

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

            row4.Cells[2].AddParagraph(newOrderPDFM.ID == 0 ? "" : newOrderPDFM.ID.ToString());
            row4.Cells[2].Format.Font.Size = 8;
            row4.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row4.Cells[2].Format.Font.Bold = false;
            row4.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;

            row4.Cells[3].AddParagraph(newOrderPDFM.QuoteNoStr == "0" ? "" : newOrderPDFM.QuoteNoStr);
            row4.Cells[3].Format.Font.Size = 8;
            row4.Cells[3].Format.Alignment = ParagraphAlignment.Left;
            row4.Cells[3].Format.Font.Bold = false;
            row4.Cells[3].Shading.Color = MigraDoc.DocumentObjectModel.Colors.AliceBlue;


            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.5cm";
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
            Column column1 = this.table.AddColumn("6.12cm");
            column1.Format.Alignment = ParagraphAlignment.Center;

            column1 = this.table.AddColumn("0.1cm");
            column1.Format.Alignment = ParagraphAlignment.Center;
            column1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column1 = this.table.AddColumn("6.12cm");
            column1.Format.Alignment = ParagraphAlignment.Center;

            column1 = this.table.AddColumn("0.1cm");
            column1.Format.Alignment = ParagraphAlignment.Center;
            column1.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            column1 = this.table.AddColumn("6.12cm");
            column1.Format.Alignment = ParagraphAlignment.Center;

            Row row1 = table.AddRow();
            row1.HeadingFormat = true;
            row1.Format.Alignment = ParagraphAlignment.Center;
            row1.Format.Font.Bold = true;

            row1.Cells[0].AddParagraph("Sold To");
            row1.Cells[0].Format.Font.Size = 9;
            row1.Cells[0].Format.Font.Bold = true;
            row1.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row1.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Green;
            row1.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

            row1.Cells[1].AddParagraph("");
            row1.Cells[1].Format.Font.Size = 9;
            row1.Cells[1].Format.Alignment = ParagraphAlignment.Left;

            row1.Cells[2].AddParagraph("Project Name/PO");
            row1.Cells[2].Format.Font.Size = 9;
            row1.Cells[2].Format.Font.Bold = true;
            row1.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row1.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Green;
            row1.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

            row1.Cells[3].AddParagraph("");
            row1.Cells[3].Format.Font.Size = 9;
            row1.Cells[3].Format.Alignment = ParagraphAlignment.Left;

            row1.Cells[4].AddParagraph("Ship To");
            row1.Cells[4].Format.Font.Size = 9;
            row1.Cells[4].Format.Font.Bold = true;
            row1.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;
            row1.Cells[4].Format.Alignment = ParagraphAlignment.Left;
            row1.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Green;

            Row row2 = table.AddRow();
            row2.HeadingFormat = true;
            row2.Format.Alignment = ParagraphAlignment.Center;
            row2.Format.Font.Bold = true;            

            //row2.Cells[0].AddParagraph(newOrderPDFM.Customer == null || newOrderPDFM.Customer.CustomerId == 0 ? prePaidCustomer : newOrderPDFM.Customer.CompanyName);
            row2.Cells[0].AddParagraph(newOrderPDFM.Customer.CompanyName + " (" + newOrderPDFM.Customer.CustomerType + ")");
            row2.Cells[0].Format.Font.Size = 8;
            row2.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row2.Cells[0].Format.Font.Bold = false;

            row2.Cells[1].AddParagraph("");
            row2.Cells[1].Format.Font.Size = 8;
            row2.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row2.Cells[1].Format.Font.Bold = false;

            row2.Cells[2].AddParagraph(newOrderPDFM.ProjectName);
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

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.8cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            /*******************************************************************/
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
            Column column3 = this.table.AddColumn("6.12cm");
            column3.Format.Alignment = ParagraphAlignment.Center;

            column3 = this.table.AddColumn("0.1cm");
            column3.Format.Alignment = ParagraphAlignment.Center;
            column3.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

            if (newOrderPDFM.Customer.ShipAddress != "Collect from A1 Rubber NSW" && newOrderPDFM.Customer.ShipAddress != "Collect from A1 Rubber QLD")
            {

                column3 = this.table.AddColumn("6.12cm");
                column3.Format.Alignment = ParagraphAlignment.Center;

                column3 = this.table.AddColumn("0.1cm");
                column3.Format.Alignment = ParagraphAlignment.Center;

                column3.Shading.Color = MigraDoc.DocumentObjectModel.Colors.White;

                column3 = this.table.AddColumn("6.12cm");
                column3.Format.Alignment = ParagraphAlignment.Center;
            }

            Row row5 = table.AddRow();
            row5.HeadingFormat = true;
            row5.Format.Alignment = ParagraphAlignment.Center;
            row5.Format.Font.Bold = true;

            row5.Cells[0].AddParagraph("Contact Details");
            row5.Cells[0].Format.Font.Size = 9;
            row5.Cells[0].Format.Font.Bold = true;
            row5.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row5.Cells[0].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Green;
            row5.Cells[0].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

            row5.Cells[1].AddParagraph("");
            row5.Cells[1].Format.Font.Size = 9;
            row5.Cells[1].Format.Alignment = ParagraphAlignment.Left;

            if (newOrderPDFM.Customer.ShipAddress != "Collect from A1 Rubber NSW" && newOrderPDFM.Customer.ShipAddress != "Collect from A1 Rubber QLD")
            {
                row5.Cells[2].AddParagraph("Site Details");
                row5.Cells[2].Format.Font.Size = 9;
                row5.Cells[2].Format.Font.Bold = true;
                row5.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                row5.Cells[2].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Green;
                row5.Cells[2].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

                row5.Cells[3].AddParagraph("");
                row5.Cells[3].Format.Font.Size = 9;
                row5.Cells[3].Format.Alignment = ParagraphAlignment.Left;

                row5.Cells[4].AddParagraph("Freight Details");
                row5.Cells[4].Format.Font.Size = 9;
                row5.Cells[4].Format.Font.Bold = true;
                row5.Cells[4].Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;
                row5.Cells[4].Format.Alignment = ParagraphAlignment.Left;
                row5.Cells[4].Shading.Color = MigraDoc.DocumentObjectModel.Colors.Green;
            }
            /***/
            
            string phoneNostr = string.Empty;
            
            if (!string.IsNullOrWhiteSpace(newOrderPDFM.ContactPerson.PhoneNumber1) && (!string.IsNullOrWhiteSpace(newOrderPDFM.ContactPerson.PhoneNumber2) && newOrderPDFM.ContactPerson.PhoneNumber2 != "Not Available"))
            {
                phoneNostr = "Name : " + newOrderPDFM.ContactPerson.ContactPersonName + System.Environment.NewLine + "Phone : " + newOrderPDFM.ContactPerson.PhoneNumber1 + " | " + newOrderPDFM.ContactPerson.PhoneNumber2 + System.Environment.NewLine + "E-mail : " + newOrderPDFM.ContactPerson.Email;
            }
            else if (!string.IsNullOrWhiteSpace(newOrderPDFM.ContactPerson.PhoneNumber1) && (string.IsNullOrWhiteSpace(newOrderPDFM.ContactPerson.PhoneNumber2) || newOrderPDFM.ContactPerson.PhoneNumber2 == "Not Available"))
            {
                phoneNostr = "Name : " + newOrderPDFM.ContactPerson.ContactPersonName + System.Environment.NewLine + "Phone : " + newOrderPDFM.ContactPerson.PhoneNumber1 + System.Environment.NewLine + "E-mail : " + newOrderPDFM.ContactPerson.Email;
            }
            else if (string.IsNullOrWhiteSpace(newOrderPDFM.ContactPerson.PhoneNumber1) && (!string.IsNullOrWhiteSpace(newOrderPDFM.ContactPerson.PhoneNumber2) && newOrderPDFM.ContactPerson.PhoneNumber2 != "Not Available"))
            {
                phoneNostr = "Name : " + newOrderPDFM.ContactPerson.ContactPersonName + System.Environment.NewLine + "Phone : " + newOrderPDFM.ContactPerson.PhoneNumber2 + System.Environment.NewLine + "E-mail : " + newOrderPDFM.ContactPerson.Email;
            }


            string cusCharge = string.Empty;
            if(newOrderPDFM.CustomerToChargedFreight == "True")
            {
                cusCharge = "Yes";
            }
            else if(newOrderPDFM.CustomerToChargedFreight == "False")
            {
                cusCharge = "No";
            }

            Row row6 = table.AddRow();
            row6.HeadingFormat = true;
            row6.Format.Alignment = ParagraphAlignment.Center;
            row6.Format.Font.Bold = true;

            row6.Cells[0].AddParagraph(phoneNostr);
            row6.Cells[0].Format.Font.Size = 8;
            row6.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row6.Cells[0].Format.Font.Bold = false;

            row6.Cells[1].AddParagraph("");
            row6.Cells[1].Format.Font.Size = 8;
            row6.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row6.Cells[1].Format.Font.Bold = false;

            if (newOrderPDFM.Customer.ShipAddress != "Collect from A1 Rubber NSW" && newOrderPDFM.Customer.ShipAddress != "Collect from A1 Rubber QLD")
            {
                row6.Cells[2].AddParagraph("Site Contact :" + newOrderPDFM.SiteContactName + System.Environment.NewLine + "Phone : " + newOrderPDFM.SiteContactPhone);
                row6.Cells[2].Format.Font.Size = 8;
                row6.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                row6.Cells[2].Format.Font.Bold = false;

                row6.Cells[3].AddParagraph("");
                row6.Cells[3].Format.Font.Size = 8;
                row6.Cells[3].Format.Alignment = ParagraphAlignment.Left;
                row6.Cells[3].Format.Font.Bold = false;

                row6.Cells[4].AddParagraph("Transport : " + newOrderPDFM.CourierName + System.Environment.NewLine + "Unload Type : " + newOrderPDFM.UnloadType +System.Environment.NewLine + "Freight Type : " + newOrderPDFM.FreightType + System.Environment.NewLine + "Order Truck : " + newOrderPDFM.OrderTruck + System.Environment.NewLine + "Is Customer To Charge Freight : " + cusCharge);
                row6.Cells[4].Format.Font.Size = 8;
                row6.Cells[4].Format.Alignment = ParagraphAlignment.Left;
                row6.Cells[4].Format.Font.Bold = false;
            }
            /*******************************************************************/


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
            paragraph.Format.SpaceAfter = 3;
            paragraph.Format.Font.Bold = false;
            double height = (1.0);
            MigraDoc.DocumentObjectModel.Color hrFillColor = new MigraDoc.DocumentObjectModel.Color(255, 0, 0);
            MigraDoc.DocumentObjectModel.Color hrBorderColor = new MigraDoc.DocumentObjectModel.Color(255, 0, 0);

            MigraDoc.DocumentObjectModel.Border newBorder = new MigraDoc.DocumentObjectModel.Border { Style = BorderStyle.Single, Color = hrBorderColor };

            paragraph.Format = new ParagraphFormat
            {
                Font = new Font("Courier New", new Unit(height)),
                Shading = new Shading { Visible = true, Color = hrFillColor },
                Borders = new Borders
                {
                    Bottom = newBorder,
                    Left = newBorder.Clone(),
                    Right = newBorder.Clone(),
                    Top = newBorder.Clone()
                }
            };

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = "-0.5cm";
            paragraph.Style = "Reference";
            paragraph.AddTab();

            // Create the item table
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Format.Font.Name = "Helvatica";
            // this.table.Borders.Color =  TableBorder;
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
            row.HeadingFormat = true;
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

            for (int i = 0; i < newOrderPDFM.QuoteDetails.Count; i++)
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

                row.Cells[0].AddParagraph(newOrderPDFM.QuoteDetails[i].Quantity.ToString());
                row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[0].Format.SpaceBefore = 2;
                row.Cells[0].Format.SpaceAfter = 2;
                row.Cells[0].Format.Font.Size = 8;
                row.Cells[0].Borders.Bottom.Visible = false;

                row.Cells[1].AddParagraph(newOrderPDFM.QuoteDetails[i].Product.ProductCode);
                row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[1].Format.SpaceBefore = 2;
                row.Cells[1].Format.SpaceAfter = 2;
                row.Cells[1].Format.Font.Size = 8;
                row.Cells[1].Borders.Bottom.Visible = false;

                row.Cells[2].AddParagraph(newOrderPDFM.QuoteDetails[i].QuoteProductDescription);
                row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[2].Format.SpaceBefore = 2;
                row.Cells[2].Format.SpaceAfter = 2;
                row.Cells[2].Format.Font.Size = 8;
                row.Cells[2].Borders.Bottom.Visible = false;

                row.Cells[3].AddParagraph(newOrderPDFM.QuoteDetails[i].Product.LocationType == null ? "" : newOrderPDFM.QuoteDetails[i].Product.LocationType);
                row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[3].Format.SpaceBefore = 2;
                row.Cells[3].Format.SpaceAfter = 2;
                row.Cells[3].Format.Font.Size = 8;
                row.Cells[3].Borders.Bottom.Visible = false;

                row.Cells[4].AddParagraph("$" + Math.Round(newOrderPDFM.QuoteDetails[i].QuoteUnitPrice, 2));
                row.Cells[4].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[4].Format.SpaceBefore = 2;
                row.Cells[4].Format.SpaceAfter = 2;
                row.Cells[4].Format.Font.Size = 8;
                row.Cells[4].Borders.Bottom.Visible = false;

                row.Cells[5].AddParagraph(newOrderPDFM.QuoteDetails[i].Product.ProductUnit);
                row.Cells[5].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[5].Format.SpaceBefore = 2;
                row.Cells[5].Format.SpaceAfter = 2;
                row.Cells[5].Format.Font.Size = 8;
                row.Cells[5].Borders.Bottom.Visible = false;

                row.Cells[6].AddParagraph(newOrderPDFM.QuoteDetails[i].Discount.ToString() + "%");
                row.Cells[6].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[6].Format.SpaceBefore = 2;
                row.Cells[6].Format.SpaceAfter = 2;
                row.Cells[6].Format.Font.Size = 8;
                row.Cells[6].Borders.Bottom.Visible = false;

                row.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(newOrderPDFM.QuoteDetails[i].Total, 2)));
                row.Cells[7].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[7].Format.SpaceBefore = 2;
                row.Cells[7].Format.SpaceAfter = 2;
                row.Cells[7].Format.Font.Size = 8;
                row.Cells[7].Borders.Bottom.Visible = false;

                total += Math.Round(newOrderPDFM.QuoteDetails[i].Total, 2);
                x++;
            }

            //tax = quote.GSTActive ? Math.Round((total + quote.FreightTotal) * 10 / 100, 2) : 0;

            /***************************ADDING FREIGHT***************************/

            foreach (var item in newOrderPDFM.FreightDetails)
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
                rowF.Cells[3].Format.Alignment = ParagraphAlignment.Left;
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
            int y = 23 - x;

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
                //row.Cells[2].MergeDown = y;

                row.Cells[3].AddParagraph("");
                row.Cells[3].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[3].Format.SpaceBefore = 2;
                row.Cells[3].Format.SpaceAfter = 2;
                row.Cells[3].Format.Font.Size = 8;
                row.Cells[3].Borders.Bottom.Visible = false;
                //row.Cells[3].MergeDown = y;

                row.Cells[4].AddParagraph("");
                row.Cells[4].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[4].Format.SpaceBefore = 2;
                row.Cells[4].Format.SpaceAfter = 2;
                row.Cells[4].Format.Font.Size = 8;
                row.Cells[4].Borders.Bottom.Visible = false;
                //row.Cells[4].MergeDown = y;

                row.Cells[5].AddParagraph("");
                row.Cells[5].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[5].Format.SpaceBefore = 2;
                row.Cells[5].Format.SpaceAfter = 2;
                row.Cells[5].Format.Font.Size = 8;
                row.Cells[5].Borders.Bottom.Visible = false;
                //row.Cells[5].MergeDown = y;

                row.Cells[6].AddParagraph("");
                row.Cells[6].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[6].Format.SpaceBefore = 2;
                row.Cells[6].Format.SpaceAfter = 2;
                row.Cells[6].Format.Font.Size = 8;
                row.Cells[6].Borders.Bottom.Visible = false;

                row.Cells[7].AddParagraph("");
                row.Cells[7].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[7].Format.SpaceBefore = 2;
                row.Cells[7].Format.SpaceAfter = 2;
                row.Cells[7].Format.Font.Size = 8;
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
            rowListPrice.Cells[0].AddParagraph("Created By: " + newOrderPDFM.User.FullName);
            rowListPrice.Cells[0].Format.Font.Bold = false;
            rowListPrice.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            rowListPrice.Cells[0].MergeRight = 7;

            rowListPrice = this.table.AddRow();
            rowListPrice.Cells[0].AddParagraph("Comments :" + newOrderPDFM.Instructions);
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

            rowListPrice.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(newOrderPDFM.ListPriceTotal, 2)));
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

            decimal disTotal = (newOrderPDFM.ListPriceTotal - newOrderPDFM.DiscountedTotal) < 0 ? 0 : newOrderPDFM.ListPriceTotal - newOrderPDFM.DiscountedTotal;

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
            rowListPrice.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(newOrderPDFM.FreightTotal, 2)));
            rowListPrice.Cells[7].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[7].Format.SpaceBefore = 2;
            rowListPrice.Cells[7].Format.SpaceAfter = 2;

            //GST
            rowListPrice = this.table.AddRow();
            rowListPrice.Cells[0].Borders.Visible = true;
            rowListPrice.Cells[0].Format.Font.Size = 6;
            rowListPrice.Cells[0].AddParagraph("");
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
            rowListPrice.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(newOrderPDFM.Gst, 2)));
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

            rowListPrice.Cells[7].AddParagraph("$" + String.Format("{0:n}", Math.Round(newOrderPDFM.TotalAmount, 2)));
            rowListPrice.Cells[7].Format.Alignment = ParagraphAlignment.Right;
            rowListPrice.Cells[7].Format.Font.Size = 10;
            rowListPrice.Cells[7].Format.Font.Bold = true;
            rowListPrice.Cells[7].Format.SpaceBefore = 2;
            rowListPrice.Cells[7].Format.SpaceAfter = 2;
                        
        }

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }


    }
}

