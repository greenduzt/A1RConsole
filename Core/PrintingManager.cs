using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class PrintingManager
    {
        public Tuple<string, Exception> AddDateTime(string formulaName, string fn, string po)
        {
            Exception exc = null;
            DateTime currentDate = DateTime.Now;
            string filename = fn;
            string pathout = po + filename;

            try
            {

                PdfReader reader = new PdfReader(formulaName);
                reader.SelectPages("1-3");
                PdfStamper stamper = new PdfStamper(reader, new FileStream(pathout, FileMode.Create));
                PdfContentByte pbover = stamper.GetOverContent(1);
                ColumnText.ShowTextAligned(pbover, Element.ALIGN_LEFT, new Phrase("Printed on " + currentDate.ToString("dd/MM/yyyy") + " at " + string.Format("{0:hh:mm:ss tt}", currentDate)), 5, 5, 0);
                PdfContentByte pbunder = stamper.GetUnderContent(1);
                stamper.Close();

                Console.Read();
            }
            catch (Exception ex)
            {
                exc = ex;
                //int result = DBAccess.InsertErrorLog(ex.Message);
                //Msg.Show("The file is currently opened. Please close the file and try again.", "File is in use", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
            }
            return Tuple.Create(pathout, exc);
        }

    }
}
