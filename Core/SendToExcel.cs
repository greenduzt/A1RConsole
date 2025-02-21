
using A1RConsole.Models.Invoices;
using Microsoft.Office.Interop.Excel;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class SendToExcel
    {
        private List<Invoice> invoiceList;
        private string filePath;
        public SendToExcel(List<Invoice> iv, string fp)
        {
            invoiceList = iv;
            filePath = fp;
        }

        public void ConvertToExcel()
        {
            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();

            if (xlApp == null)
            {
                Msg.Show("Excel is not properly installed!!", "Excel Not Installed", MsgBoxButtons.OK, MsgBoxImage.Error);

                return;
            }

            Workbook xlWorkBook;
            Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.get_Item(1);

            xlWorkSheet.Cells[1, 1] = "Co./Last Name";
            xlWorkSheet.Cells[1, 2] = "Addr1";
            xlWorkSheet.Cells[1, 3] = "Addr2";
            xlWorkSheet.Cells[1, 4] = "Addr3";
            xlWorkSheet.Cells[1, 5] = "Addr4";
            xlWorkSheet.Cells[1, 6] = "Invoice#";
            xlWorkSheet.Cells[1, 7] = "Date";
            xlWorkSheet.Cells[1, 8] = "Customer PO";
            xlWorkSheet.Cells[1, 9] = "Ship Via";
            xlWorkSheet.Cells[1, 10] = "Item Number";
            xlWorkSheet.Cells[1, 11] = "Quantity";
            xlWorkSheet.Cells[1, 12] = "Description";
            xlWorkSheet.Cells[1, 13] = "Price";
            xlWorkSheet.Cells[1, 14] = "Discount";
            xlWorkSheet.Cells[1, 15] = "Total";
            xlWorkSheet.Cells[1, 16] = "Shipping Date";
            xlWorkSheet.Cells[1, 17] = "Tax Code";
            xlWorkSheet.Cells[1, 18] = "Invoice Delivery Status";
            xlWorkSheet.Cells[1, 19] = "Terms Type";
            xlWorkSheet.Cells[1, 20] = "Due Days";
            xlWorkSheet.Cells[1, 21] = "Discount%";
            xlWorkSheet.Cells[1, 22] = "%Monthly Charge";

            int row = 2;
            foreach (var item in invoiceList)
            {
                foreach (var items in item.SalesOrderDetails)
                {
                    xlWorkSheet.Cells[row, 1] = item.Customer.CompanyName;
                    xlWorkSheet.Cells[row, 2] = item.Customer.ShipAddress;
                    xlWorkSheet.Cells[row, 3] = item.Customer.ShipCity;
                    xlWorkSheet.Cells[row, 4] = item.Customer.ShipState;
                    xlWorkSheet.Cells[row, 5] = item.Customer.ShipPostCode;
                    xlWorkSheet.Cells[row, 6] = "INV" + item.InvoiceNo;
                    xlWorkSheet.Cells[row, 7] = item.DesiredDispatchDate;
                    xlWorkSheet.Cells[row, 8] = item.CustomerOrderNo;
                    xlWorkSheet.Cells[row, 9] = item.FreightCarrier.FreightName;
                    xlWorkSheet.Cells[row, 10] = items.Product.ProductCode;
                    xlWorkSheet.Cells[row, 11] = items.Quantity;
                    xlWorkSheet.Cells[row, 12] = items.Product.ProductDescription;
                    xlWorkSheet.Cells[row, 13] = items.Product.UnitPrice;
                    xlWorkSheet.Cells[row, 14] = items.Discount;
                    xlWorkSheet.Cells[row, 15] = items.Total;
                    xlWorkSheet.Cells[row, 16] = item.OrderDate;
                    xlWorkSheet.Cells[row, 17] = "GST";
                    xlWorkSheet.Cells[row, 18] = "E";
                    xlWorkSheet.Cells[row, 19] = item.TermsID;
                    xlWorkSheet.Cells[row, 20] = "30";
                    xlWorkSheet.Cells[row, 21] = items.Discount;
                    xlWorkSheet.Cells[row, 22] = 0;
                    row++;
                }
            }

            string dateTime = DateTime.Now.ToString("ddMMyyyyhhmmss");

            xlWorkBook.SaveAs(filePath + "\\InvoicesMyOb_" + dateTime + ".xls", XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);
        }
    }
}
