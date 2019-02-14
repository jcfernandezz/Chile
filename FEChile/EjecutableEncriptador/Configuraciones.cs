using Comun;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using cfdFolios;

namespace EjecutableEncriptador
{
    public partial class Configuraciones : Form
    {
        string _metadataGP = "metadata=res://*/ModelGPChile.csdl|res://*/ModelGPChile.ssdl|res://*/ModelGPChile.msl;provider=System.Data.SqlClient;";
        string _connectionStringTargetEF;
        IntegraBD _bdgp;

        public Configuraciones(ConexionAFuenteDatos connAFuenteDatos)
        {
            InitializeComponent();
            _connectionStringTargetEF = _metadataGP + connAFuenteDatos.ArmaConnStrEF();

            try
            {
                _bdgp = new IntegraBD(_connectionStringTargetEF);
                //var folios = _bdgp.getFoliosAsync();
                //folios.Wait();

                var folios = _bdgp.getFolios();

                lboxFolios.DataSource = folios;
                lboxFolios.Refresh();
                tsStatusLabel.Text = "OK";

            }
            catch (Exception ex)
            {
                tsStatusLabel.Text = "Error";
                tboxMensajesConfig.Text = ex.Message + " - " + ex?.InnerException;
            }
            finally
            {
                statusStrip1.Refresh();
            }
        }

        private void tsButtonCargaFolios_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = "Archivos XML|*.xml";
                openFileDialog1.Multiselect = false;
                DialogResult dr = openFileDialog1.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    string[] filenames = openFileDialog1.FileNames;
                    var nombreArchivos = filenames
                            .Select(y => new
                            {
                                archivo = System.IO.Path.GetFileName(y),
                                carpeta = System.IO.Path.GetDirectoryName(y)
                            });
                    string lNombreArchivo = nombreArchivos.Select(a => a.archivo).First();
                    string lCarpetaFolios = nombreArchivos.Select(a => a.carpeta).First();
                    string rutaYNombreFolioOrigen = System.IO.Path.Combine(lCarpetaFolios, lNombreArchivo);

                    //Validación
                    if (System.IO.File.Exists(rutaYNombreFolioOrigen))
                    {
                        string xml = System.IO.File.ReadAllText(rutaYNombreFolioOrigen);
                        XDocument xdoc = XDocument.Parse(xml);
                        var tipoDocSii = xdoc.Descendants("TD").Select(d => d.Value).First();
                        var desde = int.Parse(xdoc.Descendants("D").Select(d => d.Value).First());
                        var hasta = int.Parse(xdoc.Descendants("H").Select(d => d.Value).First());
                        var caduca = xdoc.Descendants("FA").Select(d => d.Value).First();

                        var tdsii = _bdgp.getIdDocumentos(tipoDocSii).First();
                        string rutaYNombreFolioDestino = System.IO.Path.Combine(tdsii.carpetaFolio, lNombreArchivo);
                        _bdgp.IngresaRangoFolios(tdsii.soptype, tdsii.docid, desde, hasta, rutaYNombreFolioDestino);

                        System.IO.File.Delete(rutaYNombreFolioDestino);
                        System.IO.File.Copy(rutaYNombreFolioOrigen, rutaYNombreFolioDestino);
                        lboxFolios.DataSource = _bdgp.getFolios();
                        lboxFolios.Refresh();
                        tsStatusLabel.Text = "OK";
                    }
                    else
                        tboxMensajesConfig.Text = "El archivo no existe.";
                }
            }
            catch (Exception ex)
            {
                tsStatusLabel.Text = "Error";
                tboxMensajesConfig.Text = ex.Message + " - " + ex?.InnerException;
            }
            finally
            {
                statusStrip1.Refresh();
            }

        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();

        }
    }
}
