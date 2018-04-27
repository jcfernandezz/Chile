using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using Comun;
using Reporteador;
using cfd.FacturaElectronica;
using MyGeneration.dOOdads;
using MaquinaDeEstados;
using FEChile;
using FEMonitoreaMensajes;
using CLConfiguracion;
using respuestaEnvioDTE;
using Microsoft.Reporting.WinForms;
using System.Xml.Linq;

namespace EjecutableEncriptador
{
    public partial class winformGeneraFE : Form
    {
        static winVisorDeReportes FrmVisorDeReporte;
        static Parametros _param;
        static private ConexionDB DatosConexionDB = new ConexionDB();  //Lee la configuración del archivo xml y obtiene los datos de conexión.
        static vwCfdTransaccionesDeVenta trxVenta;     //documentos de venta

        vwCfdInformeMensualVentas infMes;       //documentos de venta para informe mensual
        DateTime fechaIni = DateTime.Now;
        DateTime fechaFin = DateTime.Now;
        string ultimoMensaje = "";
        int estadoCompletadoCia = 0;
        short idxChkBox = 0;                    //columna check box del grid
        short idxIdDoc = 1;                     //columna id de documento del grid
        short idxSoptype = 2;                   //columna soptype del grid
        short idxSopnumbe = 3;                  //columna sopnumbe del grid
        short idxEstado = 8;                    //columna estado (en letras) del grid
        short idxMensaje = 9;                   //columna mensaje del grid
        //short idxEstadoContab = 10;             //columna estado contabilizado del grid
        short idxAnulado = 11;                  //columna anulado del grid
        short idxEstadoDoc = 13;                //columna estado del documento (en números) del grid
        List<SopDocument> LDocsNoSeleccionados = new List<SopDocument>();   //Docs no marcados del grid

        int dePeriodo = DateTime.Now.Year * 100 + 01;
        int aPeriodo = DateTime.Now.Year * 100 + DateTime.Now.Month;
        static vwCfdLibroCVLog libroCV;         //libros de compra / venta
        short idxCBoxMarcaLib = 0;
        short idxMesLib = 2;
        short idxPeriodoLib = 3;
        short idxTipoLib = 4;
        short idxEstadoLib = 5;
        short idxEstadoBinLib = 7;
        List<LibroCV> lLibrosNoSeleccionados = new List<LibroCV>();         //Libros no marcados del grid

        static vwCfdLogFacturaCompra vwLogFacturaCompra;         //libros de compra / venta
        short idxCBoxMarcaCompra = 0;
        short idxTipoCompra = 2;
        short idxFolioCompra = 3;
        short idxIdProveedor = 4;
        short idxArchivoPdf = 13;
        short idxEstadoCompra = 7;
        List<LogFacturaCompra> lComprasNoSeleccionadas = new List<LogFacturaCompra>();         //Libros no marcados del grid

        private const string DisableCachingName = @"TestSwitch.LocalAppContext.DisableCaching";
        private const string DontEnableSchSendAuxRecordName = @"Switch.System.Net.DontEnableSchSendAuxRecord";

        delegate void reportaProgresoCallback(int i, string s);

        public winformGeneraFE()
        {
            ///Workaround de microsoft para evitar error al usar los servicios web del SII: https://support.microsoft.com/en-us/help/3155464/ms16-065-description-of-the-tls-ssl-protocol-information-disclosure-vu
            AppContext.SetSwitch(DisableCachingName, true);
            AppContext.SetSwitch(DontEnableSchSendAuxRecordName, true);

            InitializeComponent();
            dgridTrxFacturas.AutoGenerateColumns = false;
        }

        private void winformGeneraFE_Load(object sender, EventArgs e)
        {
            if (!cargaCompannias(!DatosConexionDB.Elemento.IntegratedSecurity, DatosConexionDB.Elemento.Intercompany))
            {
                txtbxMensajes.Text = ultimoMensaje;
                HabilitarVentana(false, false, false, false, false, true);
            }
            dtPickerDesde.Value = DateTime.Now;
            dtPickerHasta.Value = DateTime.Now;
            lblFecha.Text = DateTime.Now.ToString();

            cargaColumnasDataGrid();
            cargaColumnasDataGridLibros();
            cargaColumnasDataGridCompras();
            this.reportVFacturaCompra.RefreshReport();
            this.reportVFacturaCompra.RefreshReport();
        }

        void cargaColumnasDataGrid()
        {
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "docid", Name = "Id Doc.", Width=50});
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "soptype", Name = "Tipo Doc.", Width = 100, Visible =false });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "sopnumbe", Name = "Número Doc.", Width = 100 });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "fechahora", Name = "Fecha", Width = 75 });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "nombreCliente", Name = "Nombre Cliente", Width = 150 });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "idImpuestoCliente", Name = "Id. de Impuesto", Width = 100 });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "total", Name = "Total", Width = 100 });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "estado", Name = "Estado", Width = 100, Visible=false });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "mensaje", Name = "Mensaje", Width = 100 });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "estadoContabilizado", Name = "Estado Contabilizado", Width = 100, Visible=false });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "Voidstts", Name = "Anulado", Width = 100, Visible = false });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "mensajeEA", Name = "Observaciones", Width = 370 });
            dgridTrxFacturas.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "estadoActual", Name = "estadoActual", Width = 180, Visible=false });
        }

        void cargaColumnasDataGridLibros()
        {
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "YEAR1", Name = "Año", Width = 50 });
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "PERIODID", Name = "Mes", Width = 35 });
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "periodo", Name = "Periodo", Width = 75, Visible=false });
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "tipo", Name = "Tipo", Width = 40 });
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "estado", Name = "Estado", Width = 100 });
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "mensajeGral", Name = "Mensaje", Width = 250 });
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "estadoActualBin", Name = "estadoActualBin", Width = 180, Visible=false });
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "idxSingleStatus", Name = "idxSingleStatus", Width = 50, Visible=false });
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "mensajeEActual", Name = "Observaciones", Width = 250 });
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "comprobanteXml", Name = "comprobanteXml", Width = 250, Visible = false });
            dgViewCfdLibroCVLog.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "rutaXml", Name = "rutaXml", Width = 250, Visible = false });

        }
        
        void cargaColumnasDataGridCompras()
        {
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "secuencia", Name = "secuencia", Width = 50, Visible = false });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "tipo", Name = "Tipo", Width = 35 });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "folio", Name = "Folio", Width = 50 });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "idImpuestoTercero", Name = "RUT", Width = 100 });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "nombreTercero", Name = "Proveedor", Width = 150 });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "fechaRecepcion", Name = "Fecha", Width = 75 });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "estado", Name = "Estado", Width = 50, Visible = false });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "estadoActual", Name = "EstadoActual", Width = 50, Visible = false });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "idxSingleStatus", Name = "idxSingleStatus", Width = 50, Visible = false });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "mensajeEA", Name = "Observaciones", Width = 270 });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "mensaje", Name = "Mensaje", Width = 200 });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "archivoXML", Name = "archivoXML", Width = 50, Visible = false });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "archivoPDF", Name = "archivo", Width = 180, Visible = true });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "idExterno", Name = "IdExterno", Width = 50, Visible = false });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "fechaAlta", Name = "fechaAlta", Width = 50, Visible = false });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "idUsuario", Name = "idUsuario", Width = 50, Visible = false });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "fechaModificacion", Name = "fechaModificacion", Width = 50, Visible = false });
            dgvCompras.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "idUsuarioModificacion", Name = "idUsuarioModificacion", Width = 50, Visible = false });

        }

        private void ReActualizaDatosDeVentana()
        {
            DatosConexionDB.Elemento.Compannia = cmbBxCompannia.Text.ToString().Trim();
            DatosConexionDB.Elemento.Intercompany = cmbBxCompannia.SelectedValue.ToString().Trim();
            lblUsuario.Text = DatosConexionDB.Elemento.Usuario;
            ToolTip tTipCompannia = new ToolTip();
            tTipCompannia.AutoPopDelay = 5000;
            tTipCompannia.InitialDelay = 1000;
            tTipCompannia.UseFading = true;
            tTipCompannia.Show("Está conectado a: " + DatosConexionDB.Elemento.Compannia, this.cmbBxCompannia, 5000);

            txtbxMensajes.Text = "";
            if (!cargaIdDocumento())
            {
                txtbxMensajes.AppendText(ultimoMensaje);
                HabilitarVentana(false, false, false, false, false, true);
            }

            Parametros configCfd = new Parametros(DatosConexionDB.Elemento.Intercompany);   //Carga configuración desde xml
            estadoCompletadoCia = configCfd.intEstadoCompletado;
            if (!configCfd.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.AppendText(configCfd.ultimoMensaje);
                HabilitarVentana(false, false, false, false, false, true);
                return;
            }

            HabilitarVentana(configCfd.emite, configCfd.anula, configCfd.imprime, configCfd.publica, configCfd.envia, true);
            AplicaFiltroYActualizaPantalla();
        }

        private void ReActualizaDatosDeVentanaLibros(int dePeriodo, int aPeriodo, int dePeriodoDefault, int aPeriodoDefault)
        {
            txtbxMensajes.Text = "";
            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);   //Carga configuración desde xml
            estadoCompletadoCia = _param.intEstadoCompletado;
            if (!_param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.AppendText(_param.ultimoMensaje);
                return;
            }

            AplicaFiltroYActualizaVentanaLibros(_param, dePeriodo, aPeriodo, dePeriodoDefault, aPeriodoDefault);
        }

        private void ReActualizaDatosDeVentanaCompras()
        {
            txtbxMensajes.Text = "";
            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);   //Carga configuración desde xml
            estadoCompletadoCia = _param.intEstadoCompletado;
            if (!_param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.AppendText(_param.ultimoMensaje);
                return;
            }
            AplicaFiltroYActualizaVentanaCompras(_param);
        }

        /// <summary>
        /// Aplica los criterios de filtro, actualiza la pantalla de facturas e inicializa los checkboxes del grid.
        /// </summary>
        /// <param name=""></param>
        /// <returns>bool</returns>
        private bool AplicaFiltroYActualizaPantalla()
        {
            txtbxMensajes.AppendText("Explorando...\r\n");
            txtbxMensajes.Refresh();

            Parametros Compannia = new Parametros(DatosConexionDB.Elemento.Intercompany);
            txtbxMensajes.AppendText(Compannia.ultimoMensaje);
            if (!Compannia.ultimoMensaje.Equals(string.Empty))
                return false;

            cfdReglasFacturaXml regla = new cfdReglasFacturaXml(DatosConexionDB.Elemento, Compannia);
            regla.AplicaFiltroAFacturas(checkBoxFecha.Checked, dtPickerDesde.Value, dtPickerHasta.Value, fechaIni, fechaFin,
                         checkBoxNDoc.Checked, txtBNumDocDesde.Text, txtBNumDocHasta.Text,
                         checkBoxIdDoc.Checked, cmbBIdDoc.Text,
                         checkBoxEstado.Checked, cmbBEstado.Text,
                         checkBoxCliente.Checked, textBCliente.Text,
                         out trxVenta);

            if (regla.numMensajeError == 0)
            {
                vwCfdTransaccionesDeVentaBindingSource.DataSource = trxVenta.DefaultView;
                dgridTrxFacturas.DataSource = vwCfdTransaccionesDeVentaBindingSource;
                txtbxMensajes.AppendText("... " + trxVenta.RowCount.ToString() + " documento(s) consultado(s).\r\n\n");
            }
            else
            {
                vwCfdTransaccionesDeVentaBindingSource.DataSource = null;
                txtbxMensajes.AppendText(regla.ultimoMensaje);
            }
            txtbxMensajes.Refresh();
            dgridTrxFacturas.Refresh();

            //Restituir las filas marcadas usando la lista de docs no seleccionados
            InicializaCheckBoxDelGrid(idxChkBox, LDocsNoSeleccionados);

            return regla.numMensajeError == 0;
        }

        /// <summary>
        /// Aplica los criterios de filtro, actualiza la pantalla de Libros e inicializa los checkboxes del grid.
        /// </summary>
        /// <param name=""></param>
        /// <returns>bool</returns>
        private bool AplicaFiltroYActualizaVentanaLibros(Parametros Compannia, int dePeriodo, int aPeriodo, int dePeriodoDefault, int aPeriodoDefault)
        {
            txtbxMensajes.AppendText("Explorando...\r\n");
            txtbxMensajes.Refresh();

            cfdReglasFacturaXml regla = new cfdReglasFacturaXml(DatosConexionDB.Elemento, Compannia);
            regla.AplicaFiltroALibros(cBoxYearLibro.Checked, dePeriodo, aPeriodo, dePeriodoDefault, aPeriodoDefault,
                                    cBoxEstadoLibro.Checked, comBoxEstadoLibro.Text,
                                    out libroCV);

            if (regla.numMensajeError == 0)
            {
                binSourceVwCfdLibroCVLog.DataSource = libroCV.DefaultView;
                dgViewCfdLibroCVLog.DataSource = binSourceVwCfdLibroCVLog;
                txtbxMensajes.AppendText("... " + libroCV.RowCount.ToString() + " documento(s) consultado(s).\r\n\n");
            }
            else
            {
                binSourceVwCfdLibroCVLog.DataSource = null;
                txtbxMensajes.AppendText(regla.ultimoMensaje);
            }
            txtbxMensajes.Refresh();
            dgViewCfdLibroCVLog.Refresh();

            //Restituir las filas marcadas usando la lista de docs no seleccionados
            InicializaCheckBoxDelGridLibros(idxCBoxMarcaLib, lLibrosNoSeleccionados, idxPeriodoLib, idxTipoLib, idxEstadoLib);

            return regla.numMensajeError == 0;
        }

        /// <summary>
        /// Aplica los criterios de filtro, actualiza la pantalla de facturas de compra e inicializa los checkboxes del grid.
        /// </summary>
        /// <param name=""></param>
        /// <returns>bool</returns>
        private bool AplicaFiltroYActualizaVentanaCompras(Parametros Compannia)
        {
            txtbxMensajes.AppendText("Explorando...\r\n");
            txtbxMensajes.Refresh();

            cfdReglasFacturaXml regla = new cfdReglasFacturaXml(DatosConexionDB.Elemento, Compannia);
            regla.AplicaFiltroACompras(cBoxFechaRCompra.Checked, dtDeFechaRCompra.Value, dtAFechaRCompra.Value, fechaIni, fechaFin,
                         cBoxFolioCompra.Checked, tBoxDeFolioCompra.Text, tBoxAFolioCompra.Text,
                         cBoxTipoCompra.Checked, cmBoxTipoCompra.Text,
                         cBoxEstadoCompra.Checked, cmBoxEstadoCompra.Text,
                         cBoxProveedor.Checked, tBoxProveedor.Text,
                         out vwLogFacturaCompra);

            if (regla.numMensajeError == 0)
            {
                
                binSourceVwCfdLogFacCompra.DataSource = vwLogFacturaCompra.DefaultView;
                dgvCompras.DataSource = binSourceVwCfdLogFacCompra;
                txtbxMensajes.AppendText("... " + vwLogFacturaCompra.RowCount.ToString() + " documento(s) consultado(s).\r\n\n");
            }
            else
            {
                binSourceVwCfdLogFacCompra.DataSource = null;
                txtbxMensajes.AppendText(regla.ultimoMensaje);
            }
            txtbxMensajes.Refresh();
            dgvCompras.Refresh();

            //Restituir las filas marcadas usando la lista de docs no seleccionados
            InicializaCheckBoxDelGridCompras(idxCBoxMarcaCompra, lComprasNoSeleccionadas, idxFolioCompra, idxTipoCompra, idxIdProveedor, idxEstadoCompra);
            
            return regla.numMensajeError == 0;
        }

        void InicializaCheckBoxDelGrid(short idxChkBox, bool marca)
        {
            for (int r = 0; r < dgridTrxFacturas.RowCount; r++)
            {
                dgridTrxFacturas[idxChkBox, r].Value = marca; 
            }
            dgridTrxFacturas.EndEdit();
        }

        void InicializaCheckBoxDelGrid(short idxChkBox, List<SopDocument> LNoSeleccionados)
        {
            for (int r = 0; r < dgridTrxFacturas.RowCount; r++)
            {
                dgridTrxFacturas[idxChkBox, r].Value = !LNoSeleccionados.Exists(delegate(SopDocument match)
                                            {
                                                return (match.idDoc == dgridTrxFacturas[idxIdDoc, r].Value.ToString()
                                                    && match.sopnumbe == dgridTrxFacturas[idxSopnumbe, r].Value.ToString());
                                            });
            }
            dgridTrxFacturas.EndEdit();
            dgridTrxFacturas.Refresh();
        }

        void InicializaCheckBoxDelGridLibros(short idxChkBox, bool marca)
        {
            for (int r = 0; r < dgViewCfdLibroCVLog.RowCount; r++)
            {
                dgViewCfdLibroCVLog[idxChkBox, r].Value = marca;
            }
            dgViewCfdLibroCVLog.EndEdit();
        }

        void InicializaCheckBoxDelGridLibros(short idxChkBoxMarca, List<LibroCV> LNoSeleccionados, short idxPeriodo, short idxTipo, short idxEstado)
        {
            for (int r = 0; r < dgViewCfdLibroCVLog.RowCount; r++)
            {
                dgViewCfdLibroCVLog[idxChkBoxMarca, r].Value = !LNoSeleccionados.Exists(delegate(LibroCV match)
                {
                    return (match.periodo == Convert.ToInt32(dgViewCfdLibroCVLog[idxPeriodo, r].Value.ToString())
                        && match.tipo == dgViewCfdLibroCVLog[idxTipo, r].Value.ToString()
                        && match.estado == dgViewCfdLibroCVLog[idxEstado, r].Value.ToString());
                });
            }
            dgViewCfdLibroCVLog.EndEdit();
            dgViewCfdLibroCVLog.Refresh();
        }

        void InicializaCheckBoxDelGridCompras(short idxChkBox, bool marca)
        {
            for (int r = 0; r < dgvCompras.RowCount; r++)
            {
                dgvCompras[idxChkBox, r].Value = marca;
            }
            dgvCompras.EndEdit();
        }

        void InicializaCheckBoxDelGridCompras(short idxChkBoxMarca, List<LogFacturaCompra> LNoSeleccionados, short idxFolioCompra, short idxTipoCompra, short idxIdProveedor, short idxEstadoCompra)
        {
            
            for (int r = 0; r < dgvCompras.RowCount; r++)
            {
                dgvCompras[idxChkBoxMarca, r].Value = !LNoSeleccionados.Exists(delegate(LogFacturaCompra match)
                {
                    return (match.Folio == dgvCompras[idxFolioCompra, r].Value.ToString()
                        && match.Tipo == Convert.ToInt16( dgvCompras[idxTipoCompra, r].Value.ToString())
                        && match.IdImpuestoTercero == dgvCompras[idxIdProveedor, r].Value.ToString()
                        && match.Estado == dgvCompras[idxEstadoCompra, r].Value.ToString());
                });
            }
            dgvCompras.EndEdit();
            dgvCompras.Refresh();
        }

        private bool cargaIdDocumento()
        {
            vwCfdIdDocumentos iddoc = new vwCfdIdDocumentos(DatosConexionDB.Elemento.ConnStr);
            try
            {
                if (iddoc.LoadAll())
                {
                    cmbBIdDoc.DisplayMember = vwCfdIdDocumentos.ColumnNames.Docid;
                    cmbBIdDoc.ValueMember = vwCfdIdDocumentos.ColumnNames.Docid;
                    cmbBIdDoc.DataSource = iddoc.DefaultView;
                    return true;
                }
                else
                    ultimoMensaje = "Los Id. de documentos de venta no están configurados. Revise la configuración de Procesamiento de Ventas de GP.";
            }
            catch (Exception eIddoc)
            {
                ultimoMensaje = "Contacte al administrador. No se puede acceder a la base de datos." + eIddoc.Message;
            }
            return false;
        }

        private bool cargaCompannias(bool Filtro, string Unica)
        {
            vwCfdCompannias Compannias = new vwCfdCompannias(DatosConexionDB.Elemento.ConnStrDyn);
            //if (Filtro)
            //{
            //    Compannias.Where.INTERID.Value = Unica;
            //    Compannias.Where.INTERID.Operator = WhereParameter.Operand.Equal;
            //}
            try
            {
                if (Compannias.Query.Load())
                {
                    //Ocasiona que se dispare el trigger textChanged del combo box
                    cmbBxCompannia.DisplayMember = vwCfdCompannias.ColumnNames.CMPNYNAM;
                    cmbBxCompannia.ValueMember = vwCfdCompannias.ColumnNames.INTERID;
                    cmbBxCompannia.DataSource = Compannias.DefaultView;
                    cmbBxCompannia.SelectedValue = DatosConexionDB.IngresoPredeterminado;
                    return true;
                }
                else
                    ultimoMensaje = "No tiene acceso a ninguna compañía. Revise los privilegios otorgados a su usuario. [cargaCompannias]";
            }
            catch (Exception eCia)
            {
                ultimoMensaje = "Contacte al administrador. No se puede acceder a la base de datos. [CargaCompannias] " + DatosConexionDB.ultimoMensaje + " - " + eCia.Message;
            }
            return false;
        }

        /// <summary>
        /// Filtra las facturas marcadas en el grid y memoriza las filas no marcadas.
        /// </summary>
        /// <param name=""></param>
        /// <returns>bool: True indica que la lista ha sido filtrada exitosamente</returns>
        public bool filtraListaSeleccionada()
        {
            int i = 1;
            object[] llaveDocumento = new object[2];
            LDocsNoSeleccionados = new List<SopDocument>();
            try
            {
                dgridTrxFacturas.EndEdit();
                progressBar1.Value = 0;
                //cargar lista de no seleccionados
                foreach (DataGridViewRow dgvr in dgridTrxFacturas.Rows)
                {
                    if (!(dgvr.Cells[idxChkBox].Value != null && (dgvr.Cells[idxChkBox].Value.Equals(true) || dgvr.Cells[idxChkBox].Value.ToString().Equals("1"))))  
                        LDocsNoSeleccionados.Add(new SopDocument(dgvr.Cells[idxIdDoc].Value.ToString(), 
                                                                Convert.ToInt16(dgvr.Cells[idxSoptype].Value.ToString()),
                                                                dgvr.Cells[idxSopnumbe].Value.ToString()));
                    progressBar1.Value = Convert.ToInt32( i * 100 / dgridTrxFacturas.RowCount);
                    i++;
                }
                progressBar1.Value = 0;
                bool vacio = dgridTrxFacturas.RowCount == LDocsNoSeleccionados.Count;
                if (vacio)
                    ultimoMensaje = "No ha marcado ningún documento. Marque al menos una casilla en la primera columna para continuar con el proceso [filtraListaSeleccionada].\r\n";
                else
                {
                    //eliminar del datasource los registros no seleccionados
                    trxVenta.DefaultView.Sort = vwCfdTransaccionesDeVenta.ColumnNames.Docid + ", " + vwCfdTransaccionesDeVenta.ColumnNames.Sopnumbe;
                    foreach (SopDocument registro in LDocsNoSeleccionados)
                    {
                        llaveDocumento[0] = registro.idDoc;     //idDoc
                        llaveDocumento[1] = registro.sopnumbe;  //sopnumbe
                        trxVenta.DefaultView.Delete(trxVenta.DefaultView.Find(llaveDocumento));
                    }
                }
                return (!vacio);
            }
            catch (Exception eFiltro)
            {
                ultimoMensaje = "No se pudo filtrar los documentos seleccionados. [filtraListaSeleccionada] " + eFiltro.Message;
                return (false);
            }
        }

        /// <summary>
        /// Filtra los libros marcados en el grid y memoriza las filas no marcadas.
        /// </summary>
        /// <param name=""></param>
        /// <returns>bool: True indica que la lista ha sido filtrada exitosamente</returns>
        public bool filtraListaLibrosSeleccionados(short idxChkBox, short idxPeriodo, short idxTipo, short idxEstado)
        {
            int i = 1;
            object[] llaveDocumento = new object[3];
            lLibrosNoSeleccionados = new List<LibroCV>();
            try
            {
                dgViewCfdLibroCVLog.EndEdit();
                progressBar1.Value = 0;
                //cargar lista de no seleccionados
                foreach (DataGridViewRow dgvr in dgViewCfdLibroCVLog.Rows)
                {
                    if (!(dgvr.Cells[idxChkBox].Value != null && (dgvr.Cells[idxChkBox].Value.Equals(true) || dgvr.Cells[idxChkBox].Value.ToString().Equals("1"))))
                        lLibrosNoSeleccionados.Add(new LibroCV( Convert.ToInt32(dgvr.Cells[idxPeriodo].Value.ToString()),
                                                                dgvr.Cells[idxTipo].Value.ToString(),
                                                                dgvr.Cells[idxEstado].Value.ToString()));
                    progressBar1.Value = Convert.ToInt32(i * 100 / dgViewCfdLibroCVLog.RowCount);
                    i++;
                }
                progressBar1.Value = 0;
                bool vacio = dgViewCfdLibroCVLog.RowCount == lLibrosNoSeleccionados.Count;
                if (vacio)
                    ultimoMensaje = "No ha marcado ningún documento. Marque al menos una casilla en la primera columna para continuar con el proceso [filtraListaLibrosSeleccionados].\r\n";
                else
                {
                    //eliminar del datasource los registros no seleccionados
                    libroCV.DefaultView.Sort = vwCfdLibroCVLog.ColumnNames.Periodo + ", " + vwCfdLibroCVLog.ColumnNames.Tipo + ", " + vwCfdLibroCVLog.ColumnNames.Estado;
                    foreach (LibroCV registro in lLibrosNoSeleccionados)
                    {
                        llaveDocumento[0] = registro.periodo;     
                        llaveDocumento[1] = registro.tipo;
                        llaveDocumento[2] = registro.estado;
                        libroCV.DefaultView.Delete(libroCV.DefaultView.Find(llaveDocumento));
                    }
                }
                return (!vacio);
            }
            catch (Exception eFiltro)
            {
                ultimoMensaje = "Excepción al filtrar los documentos seleccionados. [filtraListaLibrosSeleccionados] " + eFiltro.Message;
                return (false);
            }
        }

        /// <summary>
        /// Filtra las facturas de compra marcadas en el grid y memoriza las filas no marcadas.
        /// </summary>
        /// <param name=""></param>
        /// <returns>bool: True indica que la lista ha sido filtrada exitosamente</returns>
        public bool filtraListaComprasSeleccionadas()
        {
            int i = 1;
            object[] llaveDocumento = new object[4];
            lComprasNoSeleccionadas = new List<LogFacturaCompra>();
            try
            {
                dgvCompras.EndEdit();
                progressBar1.Value = 0;
                //cargar lista de no seleccionados
                foreach (DataGridViewRow dgvr in dgvCompras.Rows)
                {
                    if (!(dgvr.Cells[idxCBoxMarcaCompra].Value != null && (dgvr.Cells[idxCBoxMarcaCompra].Value.Equals(true) || dgvr.Cells[idxCBoxMarcaCompra].Value.ToString().Equals("1"))))
                        lComprasNoSeleccionadas.Add(new LogFacturaCompra(
                                                                Convert.ToInt16(dgvr.Cells[idxTipoCompra].Value.ToString()),
                                                                dgvr.Cells[idxFolioCompra].Value.ToString(),
                                                                dgvr.Cells[idxIdProveedor].Value.ToString(),
                                                                dgvr.Cells[idxEstadoCompra].Value.ToString())
                                                    );
                    progressBar1.Value = Convert.ToInt32(i * 100 / dgvCompras.RowCount);
                    i++;
                }

                progressBar1.Value = 0;
                bool vacio = dgvCompras.RowCount == lComprasNoSeleccionadas.Count;

                if (vacio)
                    ultimoMensaje = "No ha marcado ningún documento. Marque al menos una casilla en la primera columna para continuar con el proceso [filtraListaComprasSeleccionadas].\r\n";
                else
                {
                    //eliminar del datasource los registros no seleccionados
                    vwLogFacturaCompra.DefaultView.Sort = vwCfdLogFacturaCompra.ColumnNames.Tipo + ", " + vwCfdLogFacturaCompra.ColumnNames.Folio + ", " + vwCfdLogFacturaCompra.ColumnNames.IdImpuestoTercero + ", " + vwCfdLogFacturaCompra.ColumnNames.Estado;
                    foreach (LogFacturaCompra registro in lComprasNoSeleccionadas)
                    {
                        llaveDocumento[0] = registro.Tipo;
                        llaveDocumento[1] = registro.Folio;
                        llaveDocumento[2] = registro.IdImpuestoTercero;
                        llaveDocumento[3] = registro.Estado;
                        vwLogFacturaCompra.DefaultView.Delete(vwLogFacturaCompra.DefaultView.Find(llaveDocumento));
                    }
                }
                return (!vacio);
            }
            catch (Exception eFiltro)
            {
                ultimoMensaje = "Excepción al filtrar los documentos seleccionados. [filtraListaComprasSeleccionadas] " + eFiltro.Message;
                return (false);
            }
        }

        private void HabilitarVentana(bool emite, bool anula, bool imprime, bool publica, bool envia, bool cambiaCia)
        {
            cmbBxCompannia.Enabled = cambiaCia;
            tsButtonGenerar.Enabled = emite;      //Emite xml
            tsBtnAbrirXML.Enabled = false;                          //Emite xml
            tsBtnSETPruebas.Enabled = anula;    //Elimina xml
            toolStripPDF.Enabled = imprime;       //Imprime
            toolStripUtiles.Enabled = imprime; //Imprime
            toolStripEmail.Enabled = envia;       //Envía emails
            toolStripEmailMas.Enabled = envia;
            
            tsConsulta.Enabled = emite || anula || imprime || publica || envia;
            btnBuscar.Enabled = emite || anula || imprime || publica || envia;
        }

        private void HabilitarVentanaLibros(bool emite, bool anula, bool imprime, bool publica, bool envia, bool cambiaCia)
        {
            tsButtonEmitirLibros.Enabled = emite;

            btnAlicarFiltroLibros.Enabled = emite || anula || imprime || publica || envia;
            tsdDownFiltroLibros.Enabled = emite || anula || imprime || publica || envia;
        }

        private void HabilitarVentanaCompras(bool acepta, bool anula, bool imprime, bool publica, bool envia, bool cambiaCia)
        {

            tsAceptar.Enabled = acepta;

            bAplicaFiltroCompras.Enabled = acepta || anula || imprime || publica || envia;

            tsDropDFiltroFechaCompras.Enabled = acepta || anula || imprime || publica || envia;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";
            LDocsNoSeleccionados = new List<SopDocument>();
            AplicaFiltroYActualizaPantalla();
        }

        void reportaProgreso(int i, string s)
        {
            //iProgreso = i;
            progressBar1.Increment(i);
            progressBar1.Refresh();

            if (progressBar1.Value == progressBar1.Maximum)
                progressBar1.Value = 0;

            txtbxMensajes.AppendText(s + "\r\n");
            txtbxMensajes.Refresh();
        }

        static void bwReportaProgreso(object sender, DoWorkEventArgs e)
        {
            //bwProcesa.ReportProgress();
            //thread safe call to a windows form control
            //if (this.progressBar1.InvokeRequired)
            //{
            //    reportaProgresoCallback d = new reportaProgresoCallback(reportaProgreso);
            //    this.Invoke(d, new object[] { i, s });
            //}
            //else
            //{
            //    progressBar1.Increment(i);
            //    progressBar1.Refresh();
            //    if (progressBar1.Value == progressBar1.Maximum)
            //        progressBar1.Value = 0;

            //    txtbxMensajes.AppendText(s + "\r\n");
            //    txtbxMensajes.Refresh();
            //}
        }

        void emiteYEnviaAlSII(Parametros Param)
        {
            CFDComprobanteFiscalDigitalFabrica cfdFabrica = new CFDComprobanteFiscalDigitalFabrica(DatosConexionDB.Elemento, Param);
            if (cfdFabrica.iErr == 0)
            {
                cfdFabrica.Progreso += new CFDComprobanteFiscalDigitalFabrica.LogHandler(reportaProgreso);  //suscribe a reporte de progreso
                cfdFabrica.ensamblaFacturas(trxVenta);                                                      //crea lista de envíos dte firmados
            }
            else
            {
                txtbxMensajes.Text = "Excepción al inicializar el proceso de emisión de facturas. " + cfdFabrica.sMsj + " [winformGeneraFE.emiteYEnviaAlSII]";
                return;
            }

            CFDServicioDespachoSolicitudes cfdDespachador = new CFDServicioDespachoSolicitudes(DatosConexionDB.Elemento, Param);
            if (cfdDespachador.iErr == 0)
            {
                cfdDespachador.Progreso += new CFDServicioDespachoSolicitudes.reportaProgreso(reportaProgreso);
                cfdDespachador.EnviaAlSII(cfdFabrica);
            }
            else
            {
                txtbxMensajes.Text = "Excepción en el botón de emisión y envió al SII. " + cfdDespachador.sMsj + " [winformGeneraFE.emiteYEnviaAlSII]";
            }
        }

        void emiteLibroYEnviaAlSII(Parametros Param)
        {
            CFDLibroCVFabrica cfdFabrica = new CFDLibroCVFabrica(DatosConexionDB.Elemento, Param);
            if (cfdFabrica.iErr == 0)
            {
                cfdFabrica.Progreso += new CFDLibroCVFabrica.LogHandler(reportaProgreso);  //suscribe a reporte de progreso
                cfdFabrica.ensamblaLote(libroCV);                                          //obtiene lista de dte's firmados
            }
            else
            {
                txtbxMensajes.Text = "Excepción al iniciar el procesamiento de libros. " + cfdFabrica.sMsj + " [winformGeneraFE.emiteLibroYEnviaAlSII]";
                return;
            }

            //CFDServicioDespachoSolicitudes cfdDespachador = new CFDServicioDespachoSolicitudes(DatosConexionDB.Elemento, Param);
            //if (cfdDespachador.iErr == 0)
            //{
            //    cfdDespachador.Progreso += new CFDServicioDespachoSolicitudes.reportaProgreso(reportaProgreso);
            //    cfdDespachador.EnviaAlSII(cfdFabrica);

            //    //Borra de la bitácora los emitidos no enviados
            //    cfdFabrica.Limpia();
            //}
            //else
            //{
            //    txtbxMensajes.Text = "Excepción al preparar contenedores o iniciar el envío de facturas al SII. " + cfdDespachador.sMsj + " [winformGeneraFE.emiteYEnviaAlSII]";
            //}

        }

        void enviaAlSII(Parametros param)
        {
            CFDComprobanteFiscalDigitalFabrica cfdFabrica = new CFDComprobanteFiscalDigitalFabrica(DatosConexionDB.Elemento, param);
            if (cfdFabrica.iErr == 0)
            {
                cfdFabrica.Progreso += new CFDComprobanteFiscalDigitalFabrica.LogHandler(reportaProgreso);    //suscribe a reporte de progreso
                cfdFabrica.cargaLote(trxVenta, 2);

                cfdFabrica.preparaContenedoresPorFactura();
            }
            else
            {
                txtbxMensajes.Text = "Excepción al iniciar procesamiento de facturas. " + cfdFabrica.sMsj + " [winformGeneraFE.enviaAlSII]";
                return;
            }

            if (cfdFabrica.iErr > 0)
            {
                txtbxMensajes.Text = "Excepción al preparar contenedores. " + cfdFabrica.sMsj + " [winformGeneraFE.enviaAlSII]";
                return;
            }

            CFDServicioDespachoSolicitudes cfdDespachador = new CFDServicioDespachoSolicitudes(DatosConexionDB.Elemento, param);
            if (cfdDespachador.iErr == 0)
            {
                cfdDespachador.Progreso += new CFDServicioDespachoSolicitudes.reportaProgreso(reportaProgreso);
                cfdDespachador.EnviaAlSII(cfdFabrica);
            }
            else
            {
                txtbxMensajes.Text = "Excepción al iniciar el envío de facturas al SII. " + cfdDespachador.sMsj + " [winformGeneraFE.enviaAlSII]";
            }
        }

        void enviaAlCliente(Parametros param)
        {
            CFDComprobanteFiscalDigitalFabrica cfdFabrica = new CFDComprobanteFiscalDigitalFabrica(DatosConexionDB.Elemento, param);
            if (cfdFabrica.iErr == 0)
            {
                cfdFabrica.Progreso += new CFDComprobanteFiscalDigitalFabrica.LogHandler(reportaProgreso);    //suscribe a reporte de progreso
                cfdFabrica.cargaLote(trxVenta, Maquina.eventoEnviaMailACliente);
            }
            else
            {
                txtbxMensajes.Text = "Excepción al iniciar procesamiento de facturas. " + cfdFabrica.sMsj + " [winformGeneraFE.enviaAlCliente]";
                return;
            }

            if (cfdFabrica.iErr > 0)
            {
                txtbxMensajes.Text = "Excepción al cargar el lote de envío a clientes. " + cfdFabrica.sMsj + " [winformGeneraFE.enviaAlCliente]";
                return;
            }

            CFDServicioDespachoSolicitudes cfdDespachador = new CFDServicioDespachoSolicitudes(DatosConexionDB.Elemento, param);
            if (cfdDespachador.iErr == 0)
            {
                cfdDespachador.Progreso += new CFDServicioDespachoSolicitudes.reportaProgreso(reportaProgreso);
                cfdDespachador.EnviaAlCliente(cfdFabrica);
            }
            else
            {
                txtbxMensajes.Text = "Excepción al iniciar el envío de facturas a los clientes. " + cfdDespachador.sMsj + " [winformGeneraFE.enviaAlCliente]";
            }
        }

        void RecibeProductoDelProveedor(Parametros param)
        {
            CFDFacturasCompraFabrica facturasProveedor = new CFDFacturasCompraFabrica(DatosConexionDB.Elemento, param);
            if (facturasProveedor.iErr == 0)
            {
                facturasProveedor.Progreso += new CFDFacturasCompraFabrica.LogHandler(reportaProgreso);  //suscribe a reporte de progreso
                facturasProveedor.EnsamblaLote(vwLogFacturaCompra, Maquina.eventoAcuseProducto, String.Empty);
            }
            else
            {
                txtbxMensajes.Text = "Excepción al inicializar la formación de la respuesta. " + facturasProveedor.SMsj + " [winformGeneraFE.RecibeProductoDelProveedor]";
                return;
            }
            CFDServicioDespachoRespuestas cfdDespachador = new CFDServicioDespachoRespuestas(DatosConexionDB.Elemento, param);
            if (cfdDespachador.IErr == 0)
            {
                cfdDespachador.Progreso += new CFDServicioDespachoRespuestas.reportaProgreso(reportaProgreso);
                cfdDespachador.LProdRecibido = facturasProveedor.LAcuses;
                cfdDespachador.EnviaAcuseAlProveedor();
            }
            else
            {
                txtbxMensajes.Text = "Excepción al iniciar el envío de facturas a los clientes. " + cfdDespachador.SMsj + " [winformGeneraFE.enviaAlCliente]";
            }

        }

        void ResultadoDocProveedor(Parametros param, Boolean aceptado, String motivoRechazo)
        {
            CFDFacturasCompraFabrica facturasProveedor = new CFDFacturasCompraFabrica(DatosConexionDB.Elemento, param);
            if (facturasProveedor.iErr == 0)
            {
                facturasProveedor.Progreso += new CFDFacturasCompraFabrica.LogHandler(reportaProgreso);  //suscribe a reporte de progreso
                if (aceptado)
                    facturasProveedor.EnsamblaLote(vwLogFacturaCompra, Maquina.eventoResultadoAceptado, String.Empty);
                else
                    facturasProveedor.EnsamblaLote(vwLogFacturaCompra, Maquina.eventoResultadoRechazado, " "+ motivoRechazo);
            }
            else
            {
                txtbxMensajes.Text = "Excepción al inicializar la formación de la respuesta. " + facturasProveedor.SMsj + " [winformGeneraFE.ResultadoDocProveedor]";
                return;
            }
            CFDServicioDespachoRespuestas cfdDespachador = new CFDServicioDespachoRespuestas(DatosConexionDB.Elemento, param);
            if (cfdDespachador.IErr == 0)
            {
                cfdDespachador.Progreso += new CFDServicioDespachoRespuestas.reportaProgreso(reportaProgreso);
                cfdDespachador.LDocsRecibidos = facturasProveedor.LRespuestas;
                cfdDespachador.ProcesaRespuestasAlProveedor();
            }
            else
            {
                txtbxMensajes.Text = "Excepción al iniciar el envío de facturas a los clientes. " + cfdDespachador.SMsj + " [winformGeneraFE.enviaAlCliente]";
            }
        }

        /// <summary>
        /// Genera XMLs masivamente
        /// </summary>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int errores = 0;
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();

            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!_param.ultimoMensaje.Equals(string.Empty)) 
            {
                txtbxMensajes.Text = _param.ultimoMensaje;
                errores++;
            }
            if (trxVenta.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para generar. Verifique los criterios de búsqueda.";
                errores++;
            }
            if (!filtraListaSeleccionada()) //Filtra trxVenta sólo con docs marcados
            {
                txtbxMensajes.Text = ultimoMensaje;
                errores++;
            }
            if (errores == 0)
            {
                toolStripPBarActividad.Visible = true;
                HabilitarVentana(false, false, false, false, false, false);

                //bwProcesa = new BackgroundWorker
                //{
                //    WorkerReportsProgress = true,
                //    WorkerSupportsCancellation = true
                //};
                //bwProcesa.DoWork += bwProcesaFacturas;
                //bwProcesa.ProgressChanged += bw_Progress;
                //bwProcesa.RunWorkerCompleted += bw_Completed;
                //bwProcesa.RunWorkerAsync();

                emiteYEnviaAlSII(_param);

                HabilitarVentana(true, true, true, true, true, true);
                AplicaFiltroYActualizaPantalla();
                progressBar1.Value = 0;
               
            }
        }

        //void bw_Progress(object sender, ProgressChangedEventArgs e)
        //{
        //    try
        //    {
        //        progressBar1.Value = iProgreso; // e.ProgressPercentage;
        //        txtbxMensajes.AppendText(e.UserState.ToString());
        //    }
        //    catch (Exception ePr)
        //    {
        //        txtbxMensajes.AppendText("bw Progress: " + ePr.Message);
        //    }
        //}

        void bw_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
            if (e.Cancelled)
                progressBar1.Value = 0;
            else if (e.Error != null)
                txtbxMensajes.AppendText("[cfdFacturaXmlWorker]: " + e.Error.ToString());
            else
                txtbxMensajes.AppendText(e.Result.ToString());

            //Actualiza la pantalla
            Parametros Cia = new Parametros(DatosConexionDB.Elemento.Intercompany);   //Carga configuración desde xml
            HabilitarVentana(Cia.emite, Cia.anula, Cia.imprime, Cia.publica, Cia.envia, true);
            AplicaFiltroYActualizaPantalla();
            progressBar1.Value = 0;
            toolStripPBarActividad.Visible = false;
            }
            catch (Exception eCm)
            {
                txtbxMensajes.AppendText("bw Completed: " + eCm.Message);
            }

        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public bool ExistenFacturasNoEmitidas()
        {
            int i = 0; 
            progressBar1.Value = 0;
            foreach (DataGridViewRow dgvr in dgridTrxFacturas.Rows)
            {
                if (!dgvr.Cells[idxEstado].Value.Equals("emitido"))
                {
                    dgvr.DefaultCellStyle.ForeColor = Color.Red;
                    dgridTrxFacturas.CurrentCell = dgvr.Cells[idxEstado];
                    progressBar1.Value = 0;
                    return true;
                }
                progressBar1.Value = i * 100 / dgridTrxFacturas.RowCount;
                i++;
            }
            progressBar1.Value = 0;
            return false;
        }

        public void GuardaArchivoMensual()
        {
            try
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                // Default file extension
                saveFileDialog1.DefaultExt = "txt";
                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.Title = "Dónde desea guardar el Informe mensual?";
                saveFileDialog1.InitialDirectory = @"C:/";
                saveFileDialog1.FileName = "1" + trxVenta.IdImpuesto.Trim() + dtPickerDesde.Value.Month.ToString().PadLeft(2, '0') + dtPickerDesde.Value.Year.ToString() + ".TXT";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Stream stm = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                    TextWriter tw = new StreamWriter(stm);
                    int i = 0;
                    progressBar1.Value = 0;
                    infMes.Rewind();          //move to first record
                    do
                    {
                        tw.WriteLine(infMes.ComprobanteEmitido);
                        tw.Flush(); // Ensure the TextWriter buffer is empty

                        txtbxMensajes.AppendText("Doc:" + infMes.Sopnumbe + "\r\n");
                        txtbxMensajes.Refresh();
                        progressBar1.Value = i * 100 / infMes.RowCount;
                        i++;
                    } while (infMes.MoveNext());
                    progressBar1.Value = 0;

                    stm.Close();
                    ultimoMensaje = "El informe mensual fue almacenado satisfactoriamente en: " + saveFileDialog1.FileName;
                }
                else
                {
                    ultimoMensaje = "Operación cancelada a pedido del usuario.";
                }
            }
            catch (Exception eFile)
            {
                ultimoMensaje = "Error al almacenar el archivo. " + eFile.Message;
            }

        }

        private void tsConfirmaAnulaXml_MouseLeave(object sender, EventArgs e)
        {
            tsConfirmaAnulaXml.Visible = false;
            //txtbxMensajes.Text = "";

        }

        //private void tsButtonConfirmaAnulaXml_Click(object sender, EventArgs e)
        //{
        //    tsConfirmaAnulaXml.Visible = false;
        //    progressBar1.Value = 0;
        //    txtbxMensajes.Text = "Procesando...";
        //    if (trxVenta.RowCount > 0)
        //    {
        //        trxVenta.Rewind();          //move to first record
        //        cfdReglasFacturaXml regla = null;
        //        Parametros Param = new Parametros(DatosConexionDB.Elemento.Intercompany);
        //        ReglasME maquina = new ReglasME(Param);
        //        int i = 1;
        //        do
        //        {
        //            regla = new cfdReglasFacturaXml(DatosConexionDB.Elemento, Param);
        //            if (trxVenta.Estado.Equals("emitido") 
        //                && maquina.ValidaTransicion(Param.tipoDoc, "ELIMINA XML", trxVenta.EstadoActual, "anulado"))
        //            {
        //                //Anular el archivo xml en la bitácora de la factura emitida
        //                regla.ActualizaFacturaEmitida(trxVenta.Soptype, trxVenta.Sopnumbe, DatosConexionDB.Elemento.Usuario, "emitido", "anulado", maquina.eBinarioNuevo, "Xml eliminado.");
        //                txtbxMensajes.AppendText("Doc:" + trxVenta.Sopnumbe + " " + regla.ultimoMensaje + "\r\n");
        //                txtbxMensajes.Refresh();
        //            }
        //            progressBar1.Value = i * 100 / trxVenta.RowCount;
        //            i++;
        //        } while (trxVenta.MoveNext());

        //        //Actualizar la pantalla
        //        AplicaFiltroYActualizaPantalla();

        //        progressBar1.Value = 0;
        //    }
        //}

        private void tsButtonImprimir_Click(object sender, EventArgs e)
        {
        }

        private void cmbBxCompannia_TextChanged(object sender, EventArgs e)
        {
            ReActualizaDatosDeVentana();
        }

        private void tsBtnAnulaElimina_Click(object sender, EventArgs e)
        {
            int i = 0;
            int errores = 0;
            if (!filtraListaSeleccionada()) //Filtra trxVenta sólo con docs marcados
            {
                txtbxMensajes.Text = ultimoMensaje;
                errores++;
            }

            txtbxMensajes.Text = "No hay documentos para anular. Verifique los criterios de búsqueda.";
            if (errores == 0 && trxVenta.RowCount > 0)
            {
                Parametros Param = new Parametros(DatosConexionDB.Elemento.Intercompany);
                ReglasME maquina = new ReglasME(Param);
                trxVenta.Rewind();          //move to first record
                do
                {
                    if (trxVenta.Estado.Equals("emitido") 
                        && maquina.ValidaTransicion(Param.tipoDoc, "ELIMINA XML", trxVenta.EstadoActual, "anulado"))
                        i++;
                } while (trxVenta.MoveNext());

                if (i > 0)
                {
                    tsLabelConfirma.Text = "Confirmar la anulación de \r\n" + i.ToString() + " documento(s):";
                    txtbxMensajes.Text = "Se anulará(n) " + i.ToString() + " documento(s)";
                    tsConfirmaAnulaXml.Visible = true;
                }
            }
        }

        private void tsBtnAbrirXML_Click(object sender, EventArgs e)
        {
            try
            {
                txtbxMensajes.Text = "";
                string ruta = dgridTrxFacturas.CurrentRow.Cells[idxMensaje].Value.ToString().Replace("Almacenado en ", "").Trim();
                string archivo = ".XML";

                if (dgridTrxFacturas.CurrentRow.Cells[idxEstado].Value.ToString().Equals("emitido"))
                    Help.ShowHelp(this, ruta + archivo);
                else
                    txtbxMensajes.Text = "No se ha emitido el archivo XML para este documento.";
            }
            catch (Exception eAbrexml)
            {
                txtbxMensajes.Text = "Error al abrir el archivo XML. Es probable que el archivo no exista o haya sido trasladado a otra carpeta. " + eAbrexml.Message;
            }

        }

        private void tsBtnArchivoMensual_Click(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";
            ultimoMensaje = "OK";
            Parametros Compannia = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!Compannia.ultimoMensaje.Equals(string.Empty))
                ultimoMensaje = Compannia.ultimoMensaje;
            if (!(checkBoxFecha.Checked && !checkBoxNDoc.Checked && !checkBoxIdDoc.Checked && !checkBoxEstado.Checked))
                ultimoMensaje = "Debe indicar el periodo a generar. Marque únicamente la opción Fecha.";
            if (dtPickerDesde.Value.Year * 100 + dtPickerDesde.Value.Month != dtPickerHasta.Value.Year * 100 + dtPickerHasta.Value.Month)
                ultimoMensaje = "El rango de fechas De/A debe estar en el mismo mes. Verifique el rango de fechas.";

            if (ultimoMensaje.Equals("OK"))
            {
                //Define inicio y fin de mes
                DateTime _fechaIni = new DateTime(dtPickerDesde.Value.Year, dtPickerDesde.Value.Month, 1);
                dtPickerDesde.Value = _fechaIni;
                DateTime _fechaFin = _fechaIni.AddMonths(1);
                int ultimoDia = _fechaFin.Day;
                dtPickerHasta.Value = _fechaFin.AddDays(-ultimoDia);

                cfdReglasFacturaXml regla = new cfdReglasFacturaXml(DatosConexionDB.Elemento, Compannia);

                if (!AplicaFiltroYActualizaPantalla())
                    return;

                if (!regla.AplicaFiltroParaInformeMes(dtPickerDesde.Value, dtPickerHasta.Value, out infMes))
                {
                    txtbxMensajes.AppendText(regla.ultimoMensaje);
                    return;
                }
                txtbxMensajes.AppendText("Consulta de documentos del mes completado.");
                txtbxMensajes.Refresh();

                if (ExistenFacturasNoEmitidas())
                {
                    if (MessageBox.Show("Existen facturas que todavía no fueron emitidas. Desea continuar?",
                        "Advertencia",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                        return;
                    txtbxMensajes.Text = "Advertencia: Existen facturas que todavía no fueron emitidas. Revise el archivo luego de generarlo.\r\n";
                }

                //Guarda el archivo mensual en disco
                if (infMes.RowCount > 0)
                    GuardaArchivoMensual();
            }
            txtbxMensajes.AppendText(ultimoMensaje);
            txtbxMensajes.Refresh();

        }

        private void tsBtnGeneraPDF_Click(object sender, EventArgs e)
        {

        }

        private void cmbBxCompannia_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReActualizaDatosDeVentana();
        }

        private void tsBtnEnviaEmail_Click(object sender, EventArgs e)
        {
            int errores = 0;
            txtbxMensajes.Text = "";

            Parametros Param = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!Param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.Text = Param.ultimoMensaje;
                errores++;
            }
            if (trxVenta.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para generar. Verifique los criterios de búsqueda.";
                errores++;
            }
            if (!filtraListaSeleccionada()) //Filtra trxVenta sólo con docs marcados
            {
                txtbxMensajes.Text = ultimoMensaje;
                errores++;
            }
            if (errores == 0)
            {
                toolStripPBarActividad.Visible = true;
                HabilitarVentana(false, false, false, false, false, false);

                enviaAlCliente(Param);

                HabilitarVentana(true, true, true, true, true, true);
                AplicaFiltroYActualizaPantalla();
                progressBar1.Value = 0;

            }
        }

        /// <summary>
        /// Barre el grid para indicar los colores que corresponden a cada fila.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgridTrxFacturas_RowPostPaint_1(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                bool completado = false;
                if (e.RowIndex >= 0)
                {
                    //Está completo
                    string estado = dgridTrxFacturas.Rows[e.RowIndex].Cells[idxEstadoDoc].Value.ToString();
                    completado = estado.Substring(0, 1).Equals('1') || estado.Substring(1, 1).Equals('1') || estado.Substring(2, 1).Equals('1') || estado.Substring(5, 1).Equals('1');

                    int estadoDoc = Convert.ToInt32(estado, 2);
                    //if (estadoDoc == estadoCompletadoCia)
                    if(completado)
                    {
                        dgridTrxFacturas.Rows[e.RowIndex].Cells[idxIdDoc].Style.BackColor = Color.YellowGreen;
                    }

                    //Está en proceso
                    //if (estadoDoc > 1 && estadoDoc != estadoCompletadoCia)
                    if (!completado && estadoDoc > 1)
                    {
                        dgridTrxFacturas.Rows[e.RowIndex].Cells[idxIdDoc].Style.BackColor = Color.Orange;
                    }
                }
            }
            catch (Exception)
            { 

            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {//pruebas
            //if (filtraListaSeleccionada())
            //{
            //    txtbxMensajes.AppendText("Documentos seleccionados listos para procesar...proceso...\r\n");
            //    AplicaFiltroYActualizaPantalla();
            //}
            //else
            //    txtbxMensajes.Text = ultimoMensaje;
        }

        private void checkBoxMark_CheckedChanged(object sender, EventArgs e)
        {
            InicializaCheckBoxDelGrid(idxChkBox, checkBoxMark.Checked);
        }

        private void tsButtonConsultaTimbre_Click(object sender, EventArgs e)
        {

        }

        private void hoytsMenuItem4_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Now;
            fechaFin = DateTime.Now;
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = hoytsMenuItem4.Text;
        }

        private void ayertsMenuItem5_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Now.AddDays(-1);
            fechaFin = DateTime.Now.AddDays(-1);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ayertsMenuItem5.Text;

        }

        private void ultimos7tsMenuItem6_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Now.AddDays(-6);
            fechaFin = DateTime.Now;
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ultimos7tsMenuItem6.Text;

        }

        private void ultimos30tsMenuItem7_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Now.AddDays(-29);
            fechaFin = DateTime.Now;
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ultimos30tsMenuItem7.Text;
        }

        private void ultimos60tsMenuItem8_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Now.AddDays(-59);
            fechaFin = DateTime.Now;
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ultimos60tsMenuItem8.Text;

        }

        private void mesActualtsMenuItem9_Click(object sender, EventArgs e)
        {
            fechaIni = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            fechaFin = fechaIni.AddMonths(1);
            int ultimoDia = fechaFin.Day;
            fechaFin = fechaFin.AddDays(-ultimoDia);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = mesActualtsMenuItem9.Text;
        }

        private void tsDropDownFiltro_TextChanged(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";
            AplicaFiltroYActualizaPantalla();
        }

        private void tsBtnImprimir_Click(object sender, EventArgs e)
        {
            string prmFolioDesde = "";
            string prmFolioHasta = "";
            string modoImpresion = "B";             //A. original, B. copia
            int prmSopType = 0;
            Parametros configCfd = new Parametros(DatosConexionDB.Elemento.Intercompany);   //Carga configuración desde xml
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();

            if (dgridTrxFacturas.CurrentRow != null)
            {
                if (dgridTrxFacturas.CurrentCell.Selected)
                {
                    prmFolioDesde = dgridTrxFacturas.CurrentRow.Cells[idxSopnumbe].Value.ToString();
                    prmFolioHasta = dgridTrxFacturas.CurrentRow.Cells[idxSopnumbe].Value.ToString();
                    prmSopType = Convert.ToInt16(dgridTrxFacturas.CurrentRow.Cells[idxSoptype].Value.ToString());

                    //En el caso de una compañía que debe emitir xml, controlar que la factura ha sido emitida antes de imprimir.
                    if (configCfd.emite)
                    {
                        if (!dgridTrxFacturas.CurrentRow.Cells[idxEstado].Value.Equals("emitido"))      //estado FE
                        {
                            txtbxMensajes.Text = "La factura " + prmFolioDesde + " no fue emitida. Emita la factura y vuelva a intentar.\r\n";
                            return;
                        }
                    }
                    else
                    {
                        if (dgridTrxFacturas.CurrentRow.Cells[idxAnulado].Value.ToString().Equals("1")) //factura anulada en GP
                        {
                            txtbxMensajes.Text = "La factura " + prmFolioDesde + " no se puede imprimir porque está anulada. \r\n";
                            return;
                        }
                    }

                    if (FrmVisorDeReporte == null)
                    {
                        try
                        {
                            FrmVisorDeReporte = new winVisorDeReportes(DatosConexionDB.Elemento, configCfd, prmFolioDesde, prmFolioHasta, modoImpresion, prmSopType);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else
                    {
                        if (FrmVisorDeReporte.Created == false)
                        {
                            FrmVisorDeReporte = new winVisorDeReportes(DatosConexionDB.Elemento, configCfd, prmFolioDesde, prmFolioHasta, modoImpresion, prmSopType);
                        }
                    }

                    // Always show and activate the WinForm
                    FrmVisorDeReporte.Show();
                    FrmVisorDeReporte.Activate();

                    //Mostrar copia cedible en caso de factura electrónica
                    //if (prmFolioDesde.StartsWith("33"))
                    //{
                    //    if (FrmVisorDeReporteCopia == null)
                    //    {
                    //        try
                    //        {
                    //            FrmVisorDeReporteCopia = new winVisorDeReportes(DatosConexionDB.Elemento, configCfd, prmFolioDesde, prmFolioHasta, "B", prmSopType);
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            MessageBox.Show(ex.Message);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (FrmVisorDeReporteCopia.Created == false)
                    //        {
                    //            FrmVisorDeReporteCopia = new winVisorDeReportes(DatosConexionDB.Elemento, configCfd, prmFolioDesde, prmFolioHasta, "B", prmSopType);
                    //        }
                    //    }

                    //    // Always show and activate the WinForm
                    //    FrmVisorDeReporteCopia.Show();
                    //    FrmVisorDeReporteCopia.Activate();
                    //}

                    txtbxMensajes.Text = FrmVisorDeReporte.mensajeErr;
                }
                else
                    txtbxMensajes.Text = "No seleccionó ninguna factura. Marque la factura a imprimir y luego presione el botón de impresión.";
            }

        }

        private void tsBtnRecibeDelSII_Click(object sender, EventArgs e)
        {
            int _ierr = 0;
            String _sMsj = String.Empty;
            Parametros configCfd = new Parametros(DatosConexionDB.Elemento.Intercompany);   //Carga configuración desde xml
            MonitorDeMensajes monitor = new MonitorDeMensajes((IParametros)configCfd, DatosConexionDB.Elemento, configCfd.emailPop3, configCfd.emailPortIn);
            _ierr = monitor.iErr;
            _sMsj = monitor.sMsj;
            if (_ierr == 0)
            {
                txtbxMensajes.Text = String.Empty;
                monitor.Progreso += new MonitorDeMensajes.reportaProgreso(reportaProgreso);
                monitor.MonitoreaComunicacionesInbound(false, Maquina.estadoBaseReceptor);
                txtbxMensajes.AppendText("\n");
                _ierr = monitor.iErr;
                _sMsj = monitor.sMsj;
            }

            if (_ierr != 0)
                txtbxMensajes.AppendText ("Excepción al revisar los mensajes recibidos. " + _sMsj + " [winformGeneraFE.tsBtnRecibeDelSII_Click]");

            AplicaFiltroYActualizaPantalla();
            progressBar1.Value = 0;
        }

        private void tsBtnEnviaAlSII_Click(object sender, EventArgs e)
        {

        }

        private void tsMenuItemMesActual_Click(object sender, EventArgs e)
        {
            dePeriodo = DateTime.Now.Year * 100 + DateTime.Now.Month;
            cBoxYearLibro.Checked = false;
            tsdDownFiltroLibros.Text = tsMenuItemMesActual.Text;

            txtbxMensajes.Text = "";
            AplicaFiltroYActualizaVentanaLibros(_param, dePeriodo, dePeriodo, dePeriodo, dePeriodo);

        }

        private void tsMenuItemMesAnt_Click(object sender, EventArgs e)
        {
            DateTime periodo = DateTime.Now.AddMonths(-1);
            dePeriodo = periodo.Year * 100 + periodo.Month;

            cBoxYearLibro.Checked = false;
            tsdDownFiltroLibros.Text = tsMenuItemMesAnt.Text;

            txtbxMensajes.Text = "";
            AplicaFiltroYActualizaVentanaLibros(_param, dePeriodo, dePeriodo, dePeriodo, dePeriodo);

        }

        private void tsMenuItemUltimos6m_Click(object sender, EventArgs e)
        {
            DateTime periodo = DateTime.Now.AddMonths(-5);
            dePeriodo = periodo.Year * 100 + periodo.Month ;
            aPeriodo = DateTime.Now.Year * 100 + DateTime.Now.Month;

            cBoxYearLibro.Checked = false;
            tsdDownFiltroLibros.Text = tsMenuItemUltimos6m.Text;

            txtbxMensajes.Text = "";
            AplicaFiltroYActualizaVentanaLibros(_param, dePeriodo, aPeriodo, dePeriodo, aPeriodo);

        }

        private void tsMenuItemUltimo12m_Click(object sender, EventArgs e)
        {
            DateTime periodo = DateTime.Now.AddMonths(-11);
            dePeriodo = periodo.Year * 100 + periodo.Month ;
            aPeriodo = DateTime.Now.Year * 100 + DateTime.Now.Month;

            cBoxYearLibro.Checked = false;
            tsdDownFiltroLibros.Text = tsMenuItemUltimo12m.Text;

            txtbxMensajes.Text = "";
            AplicaFiltroYActualizaVentanaLibros(_param, dePeriodo, aPeriodo, dePeriodo, aPeriodo);

        }

        private void tsMenuItemCurrentYear_Click(object sender, EventArgs e)
        {
            dePeriodo = DateTime.Now.Year * 100 + 01;
            aPeriodo = DateTime.Now.Year * 100 + DateTime.Now.Month;

            cBoxYearLibro.Checked = false;
            tsdDownFiltroLibros.Text = tsMenuItemCurrentYear.Text;

            txtbxMensajes.Text = "";
            AplicaFiltroYActualizaVentanaLibros(_param, dePeriodo, aPeriodo, dePeriodo, aPeriodo);

        }

        private void btnAlicarFiltroLibros_Click(object sender, EventArgs e)
        {
            try
            {
                txtbxMensajes.Text = "";
                int desdePeriodo = 0;
                int hastaPeriodo = 0;

                if (cBoxYearLibro.Checked && !tBoxYear.Text.Equals(String.Empty))
                {
                    desdePeriodo = Convert.ToInt32(tBoxYear.Text) * 100 + 01;
                    hastaPeriodo = Convert.ToInt32(tBoxYear.Text) * 100 + 12;
                }

                lLibrosNoSeleccionados = new List<LibroCV>();
                AplicaFiltroYActualizaVentanaLibros(_param, desdePeriodo, hastaPeriodo, dePeriodo, aPeriodo);
            }
            catch (Exception)
            {
                txtbxMensajes.Text = "-";
            }
        }

        private void tabcFElectronica_SelectedIndexChanged(object sender, EventArgs e)
        {
            //dePeriodo = DateTime.Now.Year * 100 + 01;
            //aPeriodo = DateTime.Now.Year * 100 + DateTime.Now.Month;
            reportVFacturaCompra.Visible = false;
            tsVerFacturaCompra.Text = "Ver factura";

            if (tabcFElectronica.SelectedTab == tabcFElectronica.TabPages["tpLibros"])
            {
                reportVFacturaCompra.Visible = false;
                ReActualizaDatosDeVentanaLibros(dePeriodo, aPeriodo, dePeriodo, aPeriodo);
            }

            if (tabcFElectronica.SelectedTab == tabcFElectronica.TabPages["tpCompras"])
            {
                ReActualizaDatosDeVentanaCompras();
            }

        }

        private void tsButtonEmitirLibros_Click(object sender, EventArgs e)
        {
            int errores = 0;
            txtbxMensajes.Text = "";

            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!_param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.Text = _param.ultimoMensaje;
                errores++;
            }
            if (libroCV.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay libros para generar. Verifique los criterios de búsqueda.";
                errores++;
            }
            if (!filtraListaLibrosSeleccionados(idxCBoxMarcaLib, idxPeriodoLib, idxTipoLib, idxEstadoLib)) //Filtra libros marcados
            {
                txtbxMensajes.Text = ultimoMensaje;
                errores++;
            }
            if (errores == 0)
            {
                toolStripPBarActividad.Visible = true;
                HabilitarVentana(false, false, false, false, false, false);
                HabilitarVentanaLibros(false, false, false, false, false, false);

                emiteLibroYEnviaAlSII(_param);

                HabilitarVentana(true, true, true, true, true, true);
                HabilitarVentanaLibros(true, true, true, true, true, true);

                AplicaFiltroYActualizaVentanaLibros(_param, dePeriodo, aPeriodo, dePeriodo, aPeriodo);
                progressBar1.Value = 0;
            }

        }

        private void cBoxMarcaLibros_CheckedChanged(object sender, EventArgs e)
        {
            InicializaCheckBoxDelGridLibros(idxCBoxMarcaLib, cBoxMarcaLibros.Checked);

        }

        private void dgViewCfdLibroCVLog_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                bool completado = false;
                if (e.RowIndex >= 0)
                {
                    //Está completo
                    string estado = dgViewCfdLibroCVLog.Rows[e.RowIndex].Cells[idxEstadoBinLib].Value.ToString();
                    completado = estado.Substring(8, 1).Equals('1');

                    int estadoDoc = Convert.ToInt32(estado, 2);
                    //if (estadoDoc == estadoCompletadoCia)
                    if (completado)
                    {
                        dgViewCfdLibroCVLog.Rows[e.RowIndex].Cells[idxMesLib].Style.BackColor = Color.YellowGreen;
                    }

                    //Está en proceso
                    //if (estadoDoc > 1 && estadoDoc != estadoCompletadoCia)
                    if (!completado && estadoDoc > 1)
                    {
                        dgViewCfdLibroCVLog.Rows[e.RowIndex].Cells[idxMesLib].Style.BackColor = Color.Orange;
                    }
                }
            }
            catch (Exception)
            {


            }
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {

        }

        private void tsHoyCompras_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Now;
            fechaFin = DateTime.Now;

            cBoxFechaRCompra.Checked = false;
            tsDropDFiltroFechaCompras.Text = tsHoyCompras.Text;
        }

        private void tsAyerCompras_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Now.AddDays(-1);
            fechaFin = DateTime.Now.AddDays(-1);

            cBoxFechaRCompra.Checked = false;
            tsDropDFiltroFechaCompras.Text = tsAyerCompras.Text;
        }

        private void tsUltimos7dCompras_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Now.AddDays(-6);
            fechaFin = DateTime.Now;

            cBoxFechaRCompra.Checked = false;
            tsDropDFiltroFechaCompras.Text = tsUltimos7dCompras.Text;

        }

        private void tsUltimos30d_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Now.AddDays(-29);
            fechaFin = DateTime.Now;

            cBoxFechaRCompra.Checked = false;
            tsDropDFiltroFechaCompras.Text = tsUltimos30d.Text;

        }

        private void tsUltimos60dCompras_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Now.AddDays(-59);
            fechaFin = DateTime.Now;

            cBoxFechaRCompra.Checked = false;
            tsDropDFiltroFechaCompras.Text = tsUltimos60dCompras.Text;
        }

        private void tsMesActualCompras_Click(object sender, EventArgs e)
        {
            fechaIni = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            fechaFin = fechaIni.AddMonths(1);
            int ultimoDia = fechaFin.Day;
            fechaFin = fechaFin.AddDays(-ultimoDia);

            cBoxFechaRCompra.Checked = false;
            tsDropDFiltroFechaCompras.Text = tsMesActualCompras.Text;
        }

        private void bAplicaFiltroCompras_Click(object sender, EventArgs e)
        {
            lComprasNoSeleccionadas = new List<LogFacturaCompra>();
            ReActualizaDatosDeVentanaCompras();
        }

        private void cBoxMarcaCompras_CheckedChanged(object sender, EventArgs e)
        {
            InicializaCheckBoxDelGridCompras(idxCBoxMarcaCompra, cBoxMarcaCompras.Checked);
        }

        private void tsAceptar_Click(object sender, EventArgs e)
        {
            toolStripAuxRechazar.Visible = false;
            int errores = 0;
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();

            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!_param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.Text = _param.ultimoMensaje;
                errores++;
            }
            if (vwLogFacturaCompra.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para procesar. Verifique los criterios de búsqueda.";
                errores++;
            }
            if (!filtraListaComprasSeleccionadas()) //Filtra trxVenta sólo con docs marcados
            {
                txtbxMensajes.Text = ultimoMensaje;
                errores++;
            }
            if (errores == 0)
            {
                toolStripPBarActividad.Visible = true;
                HabilitarVentanaCompras(false, false, false, false, false, false);

                ResultadoDocProveedor(_param, true, String.Empty);

                HabilitarVentanaCompras(true, true, true, true, true, true);
                AplicaFiltroYActualizaVentanaCompras(_param);
                progressBar1.Value = 0;
            }
        }

        private void tsBtnRecibirYAceptar_Click(object sender, EventArgs e)
        {
            int errores = 0;
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();
            toolStripAuxRechazar.Visible = false;

            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!_param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.Text = _param.ultimoMensaje;
                errores++;
            }
            if (vwLogFacturaCompra.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para procesar. Verifique los criterios de búsqueda.";
                errores++;
            }
            if (!filtraListaComprasSeleccionadas()) //Filtra sólo con docs marcados
            {
                txtbxMensajes.Text = ultimoMensaje;
                errores++;
            }
            if (errores == 0)
            {
                toolStripPBarActividad.Visible = true;
                HabilitarVentanaCompras(false, false, false, false, false, false);

                ResultadoDocProveedor(_param, true, String.Empty);

                AplicaFiltroYActualizaVentanaCompras(_param);

                filtraListaComprasSeleccionadas(); //Filtra sólo con docs marcados
                RecibeProductoDelProveedor(_param);

                HabilitarVentanaCompras(true, true, true, true, true, true);
                AplicaFiltroYActualizaVentanaCompras(_param);
                progressBar1.Value = 0;
            }
        }

        private void tsBtnGeneraPDF_Click_1(object sender, EventArgs e)
        {

        }

        private void toolStripAccionesXMLOtros_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void tsRechazar_Click(object sender, EventArgs e)
        {
            toolStripAuxRechazar.Visible = true;
            //ver tsBtnMotivoRechazo_Click()
        }

        private void tsBtnRecibir_Click(object sender, EventArgs e)
        {
            int errores = 0;
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();
            toolStripAuxRechazar.Visible = false;

            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!_param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.Text = _param.ultimoMensaje;
                errores++;
            }
            if (vwLogFacturaCompra.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para procesar. Verifique los criterios de búsqueda.";
                errores++;
            }
            if (!filtraListaComprasSeleccionadas()) //Filtra trxVenta sólo con docs marcados
            {
                txtbxMensajes.Text = ultimoMensaje;
                errores++;
            }
            if (errores == 0)
            {
                toolStripPBarActividad.Visible = true;
                HabilitarVentanaCompras(false, false, false, false, false, false);

                RecibeProductoDelProveedor(_param);

                HabilitarVentanaCompras(true, true, true, true, true, true);
                AplicaFiltroYActualizaVentanaCompras(_param);
                progressBar1.Value = 0;

            }
        }

        void cambiaEstado(Parametros param, int evento)
        {
            CFDComprobanteFiscalDigitalFabrica cfdFabrica = new CFDComprobanteFiscalDigitalFabrica(DatosConexionDB.Elemento, param);
            if (cfdFabrica.iErr == 0)
            {
                cfdFabrica.Progreso += new CFDComprobanteFiscalDigitalFabrica.LogHandler(reportaProgreso);    //suscribe a reporte de progreso
                cfdFabrica.cargaLote(trxVenta, evento);
                cfdFabrica.ActualizaCambioEstado(trxVenta, evento);
            }
            else
            {
                txtbxMensajes.Text = "Excepción al iniciar procesamiento de cambio de estado. " + cfdFabrica.sMsj + " [winformGeneraFE.cambiaEstado]";
                return;
            }

            if (cfdFabrica.iErr > 0)
            {
                txtbxMensajes.Text = "Excepción al cargar el lote. " + cfdFabrica.sMsj + " [winformGeneraFE.cambiaEstado]";
                return;
            }

        }

        private void tsMenuItemReenviar_Click(object sender, EventArgs e)
        {
            int errores = 0;
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();

            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!_param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.Text = _param.ultimoMensaje;
                errores++;
            }
            if (trxVenta.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para generar. Verifique los criterios de búsqueda.";
                errores++;
            }
            if (!filtraListaSeleccionada()) //Filtra trxVenta sólo con docs marcados
            {
                txtbxMensajes.Text = ultimoMensaje;
                errores++;
            }
            if (errores == 0)
            {
                toolStripPBarActividad.Visible = true;
                HabilitarVentana(false, false, false, false, false, false);

                cambiaEstado(_param, Maquina.eventoCambiaAPublicado);

                HabilitarVentana(true, true, true, true, true, true);
                AplicaFiltroYActualizaPantalla();
                progressBar1.Value = 0;

            }
        }

        private void tsBtnMotivoRechazo_Click(object sender, EventArgs e)
        {
            int errores = 0;
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();

            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!_param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.Text = _param.ultimoMensaje;
                errores++;
            }
            if (vwLogFacturaCompra.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para procesar. Verifique los criterios de búsqueda.";
                errores++;
            }
            if (!filtraListaComprasSeleccionadas()) //Filtra trxVenta sólo con docs marcados
            {
                txtbxMensajes.Text = ultimoMensaje;
                errores++;
            }
            if (tsTextBoxMotivoRechazo.Text.Equals(String.Empty))
            {
                txtbxMensajes.Text = "Ingrese el motivo del rechazo.";
                errores++;
            }
            if (tsTextBoxMotivoRechazo.Text.Length > 200 )
            {
                txtbxMensajes.Text = "El texto del motivo de rechazo es demasiado largo.";
                errores++;
            }

            if (errores == 0)
            {
                toolStripPBarActividad.Visible = true;
                HabilitarVentanaCompras(false, false, false, false, false, false);

                ResultadoDocProveedor(_param, false, tsTextBoxMotivoRechazo.Text);

                tsTextBoxMotivoRechazo.Text = String.Empty;
                toolStripAuxRechazar.Visible = false;

                HabilitarVentanaCompras(true, true, true, true, true, true);
                AplicaFiltroYActualizaVentanaCompras(_param);
                progressBar1.Value = 0;
            }

        }

        private void tsDropDFiltroFechaCompras_TextChanged(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";
            AplicaFiltroYActualizaVentanaCompras(_param);
        }

        void muestraReporteDocProveedor()
        {
            String nombreArchivo = String.Empty;
            try
            {
                if (dgvCompras.CurrentRow != null)
                {
                    if (dgvCompras.CurrentCell.Selected)
                    {
                        nombreArchivo = dgvCompras.CurrentRow.Cells[idxArchivoPdf].Value.ToString();
                        String folio = dgvCompras.CurrentRow.Cells[idxFolioCompra].Value.ToString();
                        String tipo = dgvCompras.CurrentRow.Cells[idxTipoCompra].Value.ToString();

                        XDocument xDoc = new XDocument();
                        Encoding _encoding = Encoding.GetEncoding(_param.encoding);
                        string dteFileName = _param.RutaArchivosTemp + nombreArchivo;
                        using (StreamReader sr = new StreamReader(dteFileName, _encoding))
                        {
                            xDoc = XDocument.Load(sr);
                        }

                        CFDFacturaCompraCab factura = new CFDFacturaCompraCab();
                        factura.FormaDTEDeProveedor(xDoc, folio, tipo);

                        cFDFacturaCompraCabBindingSource.DataSource = factura;
                        cFDFacturaCompraCabBindingSource1.DataSource = factura.LDetalleFactura;
                        this.reportVFacturaCompra.RefreshReport();

                    }
                }

            }
            catch (Exception rd)
            {
                reportaProgreso(100, "Excepción encontrada al leer el documento enviado por el proveedor. Verifique que el archivo "+nombreArchivo+ " exista en "+_param.RutaArchivosTemp+" [muestraReporteDocProveedor]" + rd.Message);
            }
        }

        private void tsVerFacturaCompra_Click(object sender, EventArgs e)
        {
            if (tsVerFacturaCompra.Text.Equals("Ocultar factura"))
            {
                tsVerFacturaCompra.Text = "Ver factura";
                reportVFacturaCompra.Visible = false;
            }
            else
            {
                tsVerFacturaCompra.Text = "Ocultar factura";
                reportVFacturaCompra.Visible = true;
                muestraReporteDocProveedor();
            }

        }

        private void dgvCompras_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (reportVFacturaCompra.Visible)
                muestraReporteDocProveedor();
        }

        private void tsMenuItemExcepcion_Click(object sender, EventArgs e)
        {
            int errores = 0;
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();

            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!_param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.Text = _param.ultimoMensaje;
                errores++;
            }
            if (trxVenta.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para generar. Verifique los criterios de búsqueda.";
                errores++;
            }
            if (!filtraListaSeleccionada()) //Filtra trxVenta sólo con docs marcados
            {
                txtbxMensajes.Text = ultimoMensaje;
                errores++;
            }
            if (errores == 0)
            {
                toolStripPBarActividad.Visible = true;
                HabilitarVentana(false, false, false, false, false, false);

                cambiaEstado(_param, Maquina.eventoReceptorExcepcional);

                HabilitarVentana(true, true, true, true, true, true);
                AplicaFiltroYActualizaPantalla();
                progressBar1.Value = 0;

            }

        }

        private void tsBtnSETPruebas_Click(object sender, EventArgs e)
        {
            Stream outputFile = File.Create(@"C:\gpusuario\getToken.txt");
            TextWriterTraceListener textListener = new TextWriterTraceListener(outputFile);
            TraceSource trace = new TraceSource("trSource", SourceLevels.All);
            trace.Listeners.Clear();
            trace.Listeners.Add(textListener);
            trace.TraceInformation("GetToken");

            int errores = 0;
            _param = new Parametros(DatosConexionDB.Elemento.Intercompany);
            if (!_param.ultimoMensaje.Equals(string.Empty))
            {
                txtbxMensajes.Text = _param.ultimoMensaje;
                errores++;
            }

            if (errores == 0)
            {
                //try
                //{
                //    string valorToken = CFDServicioDespachoSII.GetToken();
                //    trace.TraceInformation("valorSemilla: " + valorToken);
                //    trace.TraceInformation(CFDServicioDespachoSII.SMsj);
                //}
                //catch (Exception err)
                //{
                //    trace.TraceInformation("err valorSemilla: " + err.Message);
                //}
                //finally
                //{
                //    trace.Flush();
                //    trace.Close();
                //}

                CFDComprobanteFiscalDigitalFabrica cfdFabrica = new CFDComprobanteFiscalDigitalFabrica(DatosConexionDB.Elemento, _param);
                if (cfdFabrica.iErr == 0)
                {
                    cfdFabrica.Progreso += new CFDComprobanteFiscalDigitalFabrica.LogHandler(reportaProgreso);  //suscribe a reporte de progreso

                    //inicio SET PRUEBAS o SIMULACION
                    cfdFabrica.ensamblaLote(trxVenta);
                    cfdFabrica.preparaUnContenedor();
                    //fin SET PRUEBAS o SIMULACION
                }
                else
                {
                    txtbxMensajes.Text = "Excepción al inicializar el proceso de emisión de facturas. " + cfdFabrica.sMsj + " [winformGeneraFE.tsBtnSETPruebas_Click]";
                    return;
                }

                CFDServicioDespachoSolicitudes cfdDespachador = new CFDServicioDespachoSolicitudes(DatosConexionDB.Elemento, _param);
                if (cfdDespachador.iErr == 0)
                {
                    cfdDespachador.Progreso += new CFDServicioDespachoSolicitudes.reportaProgreso(reportaProgreso);
                    cfdDespachador.generaSETPruebaParaElSII(cfdFabrica);
                }
                else
                {
                    txtbxMensajes.Text = "Excepción en el botón de emisión y envió al SII. " + cfdDespachador.sMsj + " [winformGeneraFE.tsBtnSETPruebas_Click]";
                }
            }
        }

     }
}
