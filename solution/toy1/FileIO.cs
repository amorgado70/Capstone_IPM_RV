using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Win32;

namespace FileReadNWrite
{
    public class lineWriter
    {

    }

    public class lineReader
    {

    }

    public enum FILETYPE { UNKNOWN, BINARY, PLAINTEXT, CSV, XML, EXCEL, KML };
    
    abstract public class FileIO : IDisposable
    {
        public static readonly string[] FileExt = { "*.*", "*.*", "*.txt", "*.csv", "*.xml", "*.xls", "*.kml"};
        public static readonly string[] FileDesc = {
                                              "All Files|*.*", "All Files|*.*", 
                                              "TXT documents (.txt)|*.txt", 
                                              "CSV documents (.csv)|*.csv", 
                                              "XML documents (.xml)|*.xml", 
                                              "XLS documents (.xls)|*.xls",
                                              "KML documents (.kml)|*.kml" }; 

        public static FILETYPE FileType { get; set; }
        public string FileName{ get; set; }

        public FileIO(string FileName, FILETYPE type = FILETYPE.UNKNOWN)
        {
            this.FileName = FileName;
            FileType = type;
        }

        public static string ShowFileDialog(bool bWrite = false)
        {
            return fileDialog(FILETYPE.UNKNOWN, bWrite);
        }

        protected static string fileDialog( FILETYPE type, bool bWrite )
        {
            FileDialog dlg = bWrite ? ((FileDialog)(new SaveFileDialog())) : ((FileDialog)(new OpenFileDialog()));

            int idx = (int)(type);

            dlg.DefaultExt = FileExt[idx];
            dlg.Filter = FileDesc[idx];
            dlg.CheckFileExists = false;

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name
            if (result == true)
                return dlg.FileName;

            return "";
        }

        abstract public void Dispose();
    }

    public class BinaryIO : FileIO
    {
        private FileStream fileStream;
        private int numBytesRead = 0, writePosition = 0;

        public BinaryIO(string FileName = "", FILETYPE type = FILETYPE.BINARY)
            : base(FileName, type)
        {
            if (FileName != "")
            {
                fileStream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
        }

        public byte[] Read( int size = -1 )
        {
            byte[] bytes = size == -1 ? new byte[fileStream.Length] : new byte[size];
            int numBytesToRead = size == -1 ? (int)fileStream.Length - numBytesRead : size;

            // Break when the end of the file is reached. 
            while (fileStream.Read(bytes, numBytesRead, numBytesToRead) != 0)
            {
                numBytesRead++;
                numBytesToRead--;

                if (numBytesToRead == 0)
                    break;
            }
            return bytes;
        }

        virtual public string ReadLine()
        {
            byte[] bytes = new byte[fileStream.Length];
            int numBytesToRead = (int)fileStream.Length - numBytesRead;

            // Break when the end of the file is reached. 
            while (fileStream.Read(bytes, numBytesRead, 1) != 0)
            {
                string str = System.Text.Encoding.UTF8.GetString(bytes, numBytesRead, 1);
                if (str == "\n")
                    break;

                numBytesRead++;
                numBytesToRead--;

                if (numBytesToRead == 0)
                    break;
            }

            return System.Text.Encoding.UTF8.GetString(bytes, 0, numBytesRead - 1);
        }

        public bool Write( byte[] bytes )
        {
            fileStream.Write(bytes, writePosition, bytes.Length);
            fileStream.Flush();
            writePosition += bytes.Length;
            return true;
        }

        virtual public bool WriteLine(string line)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(line);

            fileStream.Write(bytes, writePosition, bytes.Length);
            fileStream.Flush();
            writePosition += bytes.Length;
            return false;
        }

        new public static string ShowFileDialog(bool bWrite = false)
        {
            return FileIO.fileDialog(FILETYPE.BINARY, bWrite);
        }

        override public void Dispose()
        {
        }
    }

    public class TextIO : FileIO
    {
        private StreamReader reader;
        private StreamWriter writer;

        public TextIO( string FileName = "", FILETYPE type = FILETYPE.PLAINTEXT ) : base(FileName, type)
        {
            if (FileName != "")
            {
                writer = new StreamWriter(new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite));
                reader = new StreamReader(new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite));
            }
        }

        virtual public string ReadLine()
        {
            return reader.ReadLine();
        }

        virtual public bool WriteLine(string line)
        {
            writer.WriteLine(line);
            writer.Flush();
            return true;
        }

        new public static string ShowFileDialog(bool bWrite = false)
        {
            return FileIO.fileDialog(FILETYPE.PLAINTEXT, bWrite);
        }

        override public void Dispose()
        {
        }
    }

    public class CSVIO : TextIO
    {
        public CSVIO( string FileName = "", FILETYPE type = FILETYPE.CSV ) : base(FileName, type)
        {
        }

        override public string ReadLine()
        {
            return base.ReadLine();
        }

        public List<string> ParseLine( string str )
        {
            List<string> sa = new List<string>();
            string temp = "";
            foreach( char s in str )
            {
                if (s != ',')
                    temp += s;
                else
                {
                    sa.Add(temp);
                    temp = "";
                }
            }
            if (temp != "")
                sa.Add(temp);
            return sa;
        }

        override public bool WriteLine(string line)
        {
            return base.WriteLine(line);
        }

        public  bool WriteCSV( List<string> sa )
        {
            string temp = "";
            foreach (string s in sa)
                temp += temp==""? s : "," + s;
            return WriteLine(temp);
        }

        new public static string ShowFileDialog(bool bWrite = false)
        {
            return FileIO.fileDialog(FILETYPE.CSV, bWrite);
        }
    }

    public class XMLIO : FileIO
    {
        public XmlWriter Writer { get; set; }
        public XmlReader Reader { get; set; }
        public XMLIO(string FileName = "", FILETYPE type = FILETYPE.XML)
            : base(FileName, type)
        {
            FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Encoding = new UTF8Encoding(false);
            xws.ConformanceLevel = ConformanceLevel.Document;
            xws.Indent = true;
            Writer = XmlWriter.Create(fs, xws);
        }

        public XmlDocument Read()
        {
            FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            XmlReaderSettings xrs = new XmlReaderSettings();
            xrs.ValidationType = ValidationType.Schema;
            Reader = XmlReader.Create(fs);

            XmlDocument doc = new XmlDocument();
            doc.Load(Reader);
            return doc;
        }

        public void Write()
        {
            Writer.Flush();
            Writer.Close();
        }

        new public static string ShowFileDialog(bool bWrite = false)
        {
            return FileIO.fileDialog(FILETYPE.XML, bWrite);
        }

        override public void Dispose()
        {
        }
    }

    public class KMLIO : XMLIO
    {
        public KMLIO(string FileName = "", FILETYPE type = FILETYPE.KML)
            : base(FileName, type)
        {
        }

        new public static string ShowFileDialog(bool bWrite = false)
        {
            return FileIO.fileDialog(FILETYPE.KML, bWrite);
        }

        override public void Dispose()
        {
        }
    }

    public class EXCELIO : FileIO
    {
        private Excel.Application excelApp;
        Excel._Worksheet workSheet;
        public EXCELIO(string FileName = "", FILETYPE type = FILETYPE.XML)
            : base(FileName, type)
        {
            excelApp = new Excel.Application();
        }

        public void SetSheet( bool bWrite=false )
        {
            if( bWrite )
            {
                excelApp.Workbooks.Add();
                workSheet = excelApp.ActiveSheet;
                excelApp.Visible = true;
            }
            else 
            {
                Excel.Workbook xlWorkBook = excelApp.Workbooks.Open( FileName, 0, true, 5, "", "", true, 
                                    Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                workSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            }
        }

        public List<string> Read(int row, int columnCnt)
        {
            List<string> contents = new List<string>();

            if (workSheet.UsedRange.Rows.Count < row)
                return null;

            int icol = 'A';
            for (int i = 0; i < columnCnt; i++)
            {
                string val = string.Format("{0}{1}", Convert.ToChar(icol + i), row);
                contents.Add(workSheet.get_Range(val, val).Value.ToString());
            }

            return contents;
        }

        public void Write( int row, List<string> contents )
        {
            int icol = 'A';
            foreach (string s in contents)
            {
                char cCol = Convert.ToChar(icol++);
                workSheet.Cells[row, cCol.ToString()] = s;
            }
        }

        new public static string ShowFileDialog(bool bWrite = false)
        {
            return FileIO.fileDialog(FILETYPE.EXCEL, bWrite);
        }

        override public void Dispose()
        {
            workSheet.SaveAs(FileName);
        }
    }
}



