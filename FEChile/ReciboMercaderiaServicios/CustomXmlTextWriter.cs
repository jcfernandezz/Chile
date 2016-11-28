using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReciboMercaderiaServicios
{
    public class CustomXmlTextWriter : XmlTextWriter
    {
        private string _strEncoding;
        public CustomXmlTextWriter(string filename, string strEncoding)
            : base(filename, Encoding.GetEncoding(strEncoding))
        {
            _strEncoding = strEncoding;
        }

        public override void WriteStartDocument()
        {
            WriteRaw("<?xml version=\"1.0\" encoding=\"" + _strEncoding + "\"?>");
        }

        public override void WriteEndDocument()
        {
        }
    }
}
