using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;

namespace DesignPaletteGenerator
{
    public partial class PaletteGen : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ButtonUpload(object sender, EventArgs e)
        {
            List<Byte> filebytes = SwatchBytes();

            if (filebytes.Count > 0)
            {
                Palette swch = new Palette(filebytes);
                if (swch.colors != null)
                {
                    colorlist.DataSource = swch.colors;
                    colorlist.DataBind();
                    Pname.Text = swch.name;
                    //WritePalette(swch);
                    // does not return
                }
                else
                {
                    // parser problem
                    usermsg.Text = "That file doesn't appear to be a valid ASE palette file.";
                }
            }
            else
            {
                // null or no file
                usermsg.Text = "Please upload a valid ASE palette file.";
            }
        }

        private List<Byte> SwatchBytes ()
        {
            BinaryReader br = new BinaryReader(asefile.PostedFile.InputStream);
            int filesize = asefile.PostedFile.ContentLength;
            List<Byte> filebytes = new List<byte>(filesize);

            try
            {
                while (br.BaseStream.Position < filesize)
                {
                    byte b = br.ReadByte();
                    filebytes.Add(b);
                }
            }
            catch (EndOfStreamException)
            {
            }
            catch (IOException)
            {
                throw;
            }
            finally
            {
                br.Close();
            }

            return (filebytes);
        }

        private static void WritePalette(Palette swch)
        {
            //<SwatchLibrary xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
            // xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
            // Name="palette_name" 
            // xmlns="http://schemas.microsoft.com/expression/design/2007">
            XmlDocument xd = new XmlDocument();
            XmlElement root = xd.CreateElement("SwatchLibrary");
            XmlAttribute roota = xd.CreateAttribute("xmlns");
            roota.Value = @"http://schemas.microsoft.com/expression/design/2007";
            root.Attributes.Append(roota);
            roota = xd.CreateAttribute("xmlns:xsi");
            roota.Value = @"http://www.w3.org/2001/XMLSchema-instance";
            root.Attributes.Append(roota);
            roota = xd.CreateAttribute("xmlns:xsd");
            roota.Value = @"http://www.w3.org/2001/XMLSchema";
            root.Attributes.Append(roota);
            roota = xd.CreateAttribute("Name");
            roota.Value = swch.name;
            root.Attributes.Append(roota);
            xd.AppendChild(root);

            // add declaration
            XmlDeclaration xmldecl = xd.CreateXmlDeclaration("1.0", "utf-8", null);
            xd.InsertBefore(xmldecl, root);

            foreach (NamedSwatch n in swch.colors)
            {
                XmlElement xe = xd.CreateElement("SolidColorSwatch");
                XmlAttribute xa = xd.CreateAttribute("Name");
                if (n.name.Length > 0)
                {
                    xa.Value = n.name;
                    xe.Attributes.Append(xa);
                }
                xa = xd.CreateAttribute("Color");
                xa.Value = "#" + n.argbcolor().ToUpper();
                xe.Attributes.Append(xa);
                root.AppendChild(xe);
            }

            // if palette has no name, title it "palette"
            String pname = swch.name.Length < 1 ? "palette" : swch.name;

            WriteXml(xd, pname);
        }

        public static void WriteXml(XmlDocument doc, String filename)
        {

            HttpContext.Current.Response.Charset = "utf-8";
            HttpContext.Current.Response.ContentType = "unk/unk";
            HttpContext.Current.Response.AddHeader("Content-Disposition", @"attachment; filename=""" + filename + @".xml""");

            XmlTextWriter writer = new XmlTextWriter(HttpContext.Current.Response.Output);
            writer.Formatting = Formatting.Indented;
            doc.WriteTo(writer);
            writer.Flush();

            HttpContext.Current.Response.End();
        }
    }

    public class Palette
    {
        private string palettename = String.Empty;

        public List<NamedSwatch> colors = new List<NamedSwatch>();

        public string name
        {
            get
            {
                return (palettename);
            }
        }

        public Palette(List<Byte> byts)
        {
            try
            {
                AsePaletteFile myfile = new AsePaletteFile(byts);
                palettename = myfile.SwatchName;
                List<List<Byte>> colorlist = myfile.SwatchBytes;

                foreach (List<Byte> clr in colorlist)
                {
                    colors.Add(new NamedSwatch(clr));
                }
            }
            catch
            {
                palettename = "INVALID";
                colors = null;
            }
        }

    }

    public class AsePaletteFile
    {
        #region Private Properties
        private const UInt32 palette_name_chunk = 0xc0010000;
        private const UInt32 swatch_name_chunk  = 0x00010000;

        private readonly List<Byte> filebytes;
        private int readlocation = 0;
        private List<Byte> namechunk;
        private List<List<Byte>> swatchchunks;
        #endregion

        #region Public Accessors
        public int SwatchCount { get { return (swatchchunks.Count); } }
        public List<List<Byte>> SwatchBytes { get { return (swatchchunks); } }
        public String SwatchName { get { return (PaletteUtil.ExtractDblByteString(namechunk)); } }
        #endregion

        public AsePaletteFile(List<Byte> b)
        {
            filebytes = b;
            swatchchunks = new List<List<Byte>>();
            int numchunks;

            ConsumeNextExpectedChar('A');
            ConsumeNextExpectedChar('S');
            ConsumeNextExpectedChar('E');
            ConsumeNextExpectedChar('F');

            ChunkSpacer();
            NextByte();
            NextByte();
            numchunks = NextDoubleByte();

            for (int i = 0; i < numchunks; i++ )
            {
                UInt32 chunktype = ChunkSpacer();

                if (chunktype == palette_name_chunk)
                    namechunk = NextChunk();

                else if (chunktype == swatch_name_chunk)
                    swatchchunks.Add(NextChunk());
            }
        }

        public List<Byte> NextChunk()
        {
            List<Byte> chunk = new List<Byte>();

            int lgth = NextDoubleByte();

            if (lgth == 0) return (null);

            for (Byte i = 0; i < lgth; i++)
            {
                chunk.Add(NextByte());
            }

            return (chunk);
        }

        private UInt32 ChunkSpacer()
        {
            UInt32 spacerval = NextByte();
            spacerval = spacerval << 8;
            spacerval += NextByte();
            spacerval = spacerval << 8;
            spacerval += NextByte();
            spacerval = spacerval << 8;
            spacerval += NextByte();

            return (spacerval);
        }

        private void ConsumeNextExpectedChar(Char ch)
        {
            if (ch != NextShortChar())
                throw (new Exception());
        }

        private Char NextShortChar()
        {
            return (Convert.ToChar(NextByte()));
        }

        private Byte NextByte()
        {
            return (filebytes[readlocation++]);
        }

        private int NextDoubleByte()
        {
            return (256 * NextByte() + NextByte());
        }
    }

    public class PaletteUtil
    {
        public static String ExtractDblByteString(List<Byte> chk)
        {
            StringBuilder sb = new StringBuilder();

            if (chk != null)
            {
                int numchars = DbyteCharAt(0, chk) - 1;

                for (int i = 0; i < numchars; i++)
                {
                    int byteidx = i*2 + 2;
                    sb.Append(DbyteCharAt(byteidx, chk));
                }
            }

            return (sb.ToString());
        }

        public static List<Byte> FourBytesAt(int byteindex, List<Byte> inlist)
        {
            List<Byte> outbytes = new List<byte>();
            for (int i = 0; i < 4; i++)
            {
                outbytes.Add(inlist[byteindex + i]);
            }
            return (outbytes);
        }

        public static char DbyteCharAt(int byteidx, List<byte> chk)
        {
            return Convert.ToChar(chk[byteidx] * 256 + chk[byteidx + 1]);
        }

        public static char SbyteCharAt(int byteidx, List<byte> chk)
        {
            return Convert.ToChar(chk[byteidx]);
        }

        public static UInt32 toUi32(List<byte> bfloat)
        {
            UInt32 loadval = 0;
            foreach (byte b in bfloat)
            {
                loadval = loadval << 8;
                loadval = loadval | b;
            }
            return (loadval);
        }

        public static Double toDbl(List<Byte> bte)
        {
            return (toDbl(toUi32(bte)));
        }

        public static Double toDbl(UInt32 uin)
        {
            // example for -118.625   0xC2ED4000
            // 1100 0010 1110 1101 0100 0000 0000 0000
            // 1   10000101    11011010100000000000000
            //   1 | 1000 0101 | 0110 1101 0100 0000 0000 0000
            // 0x1 |      0x85 |                      0x6d4000

            UInt32 bits_sign = uin >> 31;
            UInt32 bits_exp = (uin << 1) >> 24;
            UInt32 bits_fract = (uin << 9) >> 9;

            Double sign = bits_sign * -2 + 1;
            Double exp = bits_exp - 127.0;

            Double fraction = 1.0;
            Double multiplier = 1.0;

            for (int clearleft = 9; clearleft < 32; clearleft++)
            {
                UInt32 bit = (bits_fract << clearleft) >> 31;
                multiplier *= 0.5;
                fraction += bit * multiplier;
            }

            return (sign * fraction * Math.Pow(2.0, exp));
        }
    }

    public class NamedSwatch
    {
        #region Private Properties
        private string cname;
        private string cspace;
        private List<Double> colorvalues = new List<Double>();
        #endregion

        public string colorspace
        {
            get
            {
                return (cspace.Trim().ToLower());
            }
        }

        public int colorcount
        {
            get
            {
                return (colorvalues.Count);
            }
        }

        public String name
        {
            get
            {
                return (cname);
            }
        }

        public string hexcolor
        {
            get
            {
                return argbcolor().Substring(2);
            }
        }

        public string argbcolor()
        {
            StringBuilder hexstr = new StringBuilder("ff"); // Force 100% Alpha value

            if (colorspace.Equals("rgb"))
            {
                // rgb color values are entered/stored as percentage
                // we need to multiply by 255 to convert to rgb scale
                // I know I know. 255 provides better rounding results than 256

                foreach (Double cval in colorvalues)
                {
                    hexstr.Append(Convert.ToInt32((cval*255)).ToString("x2"));
                }
            }
            else if (colorspace.Equals("cmyk"))
            {
                // colorspace cmyk
                Double cmykc = colorvalues[0];
                Double cmykm = colorvalues[1];
                Double cmyky = colorvalues[2];
                Double cmykk = colorvalues[3];

                //C = ( C * ( 1 - K ) + K )
                //M = ( M * ( 1 - K ) + K )
                //Y = ( Y * ( 1 - K ) + K )
                Double cmyc = cmykc*(1.0 - cmykk) + cmykk;
                Double cmym = cmykm*(1.0 - cmykk) + cmykk;
                Double cmyy = cmyky*(1.0 - cmykk) + cmykk;

                //R = ( 1 - C ) * 255
                //G = ( 1 - M ) * 255
                //B = ( 1 - Y ) * 255
                Double rgbr = (1.0 - cmyc)*255.0;
                Double rgbg = (1.0 - cmym)*255.0;
                Double rgbb = (1.0 - cmyy)*255.0;

                hexstr.Append(Convert.ToInt32((rgbr)).ToString("x2"));
                hexstr.Append(Convert.ToInt32((rgbg)).ToString("x2"));
                hexstr.Append(Convert.ToInt32((rgbb)).ToString("x2"));
            }
            else if (colorspace.Equals("gray"))
            {
                // colorspace gray: three identical rgb values

                for (int i = 0; i < 3; i++ )
                {
                    hexstr.Append(Convert.ToInt32((colorvalues[0] * 255)).ToString("x2"));
                }
                
            }
            else
            {
                // unknown -- return black
                hexstr.Append("000000");
            }
            
            return (hexstr.ToString());


}

        public NamedSwatch(List<Byte> clrbytes)
        {
            cname = PaletteUtil.ExtractDblByteString(clrbytes);

            StringBuilder sb = new StringBuilder();
            int namelength = cname.Length;
            int spaceindex = (namelength + 2) * 2;
            for (int i = 0; i < 4; i++)
            {
                sb.Append(PaletteUtil.SbyteCharAt(spaceindex + i, clrbytes));
            }
            cspace = sb.ToString();

            int validx = spaceindex + 4;
            int numvals = ((clrbytes.Count - 2) - validx) / 4;

            for (int i = 0; i < numvals; i++)
            {
                colorvalues.Add(PaletteUtil.toDbl(PaletteUtil.FourBytesAt(validx + i * 4, clrbytes)));
            }
        }
    }
}
