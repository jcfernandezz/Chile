using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FEChile
{
    public class CFDFacturaCompraCab
    {
        private String _xNameSpace = "{http://www.sii.cl/SiiDte}";

        #region Propiedades
        private int _iErr = 0;
        public int IErr
        {
            get { return _iErr; }
            set { _iErr = value; }
        }
        private string _sMsj = string.Empty;
        public string SMsj
        {
            get { return _sMsj; }
            set { _sMsj = value; }
        }
        private String _folio;

        public String Folio
        {
            get { return _folio; }
            set { _folio = value; }
        }
        private String _tipoDTE;

        public String TipoDTE
        {
            get { return _tipoDTE; }
            set { _tipoDTE = value; }
        }
        private String _descTipoDTE;

        public String DescTipoDTE
        {
            get {
                if (_tipoDTE.Equals("33"))
                    return "FACTURA ELECTRONICA";
                if (_tipoDTE.Equals("56"))
                    return "NOTA DE DEBITO ELECTRONICA";
                if (_tipoDTE.Equals("61"))
                    return "NOTA DE CREDITO ELECTRONICA";
                else
                    return "DOCUMENTO DESCONOCIDO";
                }
            set { _descTipoDTE = value; }
        }
        private DateTime _fchEmis;

        public DateTime FchEmis
        {
            get { return _fchEmis; }
            set { _fchEmis = value; }
        }
        private String _rutReceptor;

        public String RutReceptor
        {
            get { return _rutReceptor; }
            set { _rutReceptor = value; }
        }
        private String _rutEmisor;

        public String RutEmisor
        {
            get { return _rutEmisor; }
            set { _rutEmisor = value; }
        }
        private String _nomEmisor;

        public String NomEmisor
        {
            get { return _nomEmisor; }
            set { _nomEmisor = value; }
        }
        private decimal _MntTotal;

        public decimal MntTotal
        {
            get { return _MntTotal; }
            set { _MntTotal = value; }
        }
        private decimal _MntNeto;

        public decimal MntNeto
        {
            get { return _MntNeto; }
            set { _MntNeto = value; }
        }

        private decimal _iva;

        public decimal Iva
        {
            get { return _iva; }
            set { _iva = value; }
        }
        private string _nomReceptor;

        public string NomReceptor
        {
            get { return _nomReceptor; }
            set { _nomReceptor = value; }
        }

        private List<CFDFacturaCompraDet> _lDetalleFactura;
        public List<CFDFacturaCompraDet> LDetalleFactura
        {
            get { return _lDetalleFactura; }
            set { _lDetalleFactura = value; }
        }
        
        #endregion

        public CFDFacturaCompraCab()
        {
 
        }

        public void FormaDTEDeProveedor(XDocument xEnvioDTE, String folio, String tipoDte)
        {
            _iErr = 0;
            _sMsj = String.Empty;

            try
            {
                var xSet = xEnvioDTE.Elements(_xNameSpace + "EnvioDTE").Elements(_xNameSpace + "SetDTE").Elements();

                //Revisar cada DTE
                foreach (var dte in xSet)
                {
                    if (dte.Name.ToString().Equals(_xNameSpace + "DTE"))
                    {
                        _folio = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "IdDoc").Element(_xNameSpace + "Folio").Value;
                        _tipoDTE = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "IdDoc").Element(_xNameSpace + "TipoDTE").Value;
                        if (_folio.Equals(folio) && _tipoDTE.Equals(tipoDte))
                        {
                            _fchEmis = DateTime.ParseExact(
                                            dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "IdDoc").Element(_xNameSpace + "FchEmis").Value,
                                            "yyyy-M-d", null);

                            _rutEmisor = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Emisor").Element(_xNameSpace + "RUTEmisor").Value; //empresa que envía el dte (según DTE)
                            _nomEmisor = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Emisor").Element(_xNameSpace + "RznSoc").Value;
                            _rutReceptor = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Receptor").Element(_xNameSpace + "RUTRecep").Value; //empresa que recibe el dte (según DTE)
                            _nomReceptor = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Receptor").Element(_xNameSpace + "RznSocRecep").Value; //empresa que recibe el dte (según DTE)
                            _MntTotal = Convert.ToDecimal(dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Totales").Element(_xNameSpace + "MntTotal").Value);
                            _MntNeto = Convert.ToDecimal(dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Totales").Element(_xNameSpace + "MntNeto").Value);
                            _iva = Convert.ToDecimal(dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Totales").Element(_xNameSpace + "IVA").Value);

                            var xDet = dte.Elements(_xNameSpace + "Documento").Elements();
                            _lDetalleFactura = new List<CFDFacturaCompraDet>();
                            foreach (var detalleXml in xDet)
                            {
                                if (detalleXml.Name.ToString().Equals(_xNameSpace + "Detalle"))
                                {
                                    CFDFacturaCompraDet camposDet = new CFDFacturaCompraDet();
                                    camposDet.NmbItem = detalleXml.Element(_xNameSpace + "NmbItem").Value;
                                    camposDet.QtyItem = Convert.ToDecimal(detalleXml.Element(_xNameSpace + "QtyItem").Value);
                                    camposDet.PrcItem = Convert.ToDecimal(detalleXml.Element(_xNameSpace + "PrcItem").Value);
                                    camposDet.MontoItem = Convert.ToDecimal(detalleXml.Element(_xNameSpace + "MontoItem").Value);

                                    _lDetalleFactura.Add(camposDet);

                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch (NullReferenceException nr)
            {
                _iErr++;
                _sMsj = "No se encuentra el folio del documento del proveedor. " + nr.Message + " [CFDFacturaCompraCab.FormaDTEDeProveedor]";
            }
            catch (Exception re)
            {
                _sMsj = "Excepción desconocida al formar el DTE del proveedor. " + re.Message + " [CFDFacturaCompraCab.FormaDTEDeProveedor]";
                _iErr++;
            }
        }

    }
}
