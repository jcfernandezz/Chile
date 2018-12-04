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

namespace EjecutableEncriptador
{
    public partial class Configuraciones : Form
    {
        public Configuraciones()
        {
            InitializeComponent();
        }

        private void tsButtonCargaFolios_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = "Archivos XML|*.xml|*.XML";
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
                    var lNombreArchivo = nombreArchivos.Select(a => a.archivo).First();
                    var lCarpeta = nombreArchivos.Select(a => a.carpeta).First();
                    //var f = from ff in filenames
                    //        select new
                    //        {
                    //            archivo = System.IO.Path.GetFileName(ff),
                    //            //directorio = System.IO.Path.GetDirectoryName(ff),
                    //        };
                    //List<string> lNombreArchivos = f.Select(x => x.archivo).ToList();

                    //Validación
                    if (System.IO.File.Exists(lNombreArchivo))
                    {
                        string xml = System.IO.File.ReadAllText(lNombreArchivo);
                        XDocument xdoc = XDocument.Parse(xml);
                        var tipoDoc = xdoc.Descendants("TD").Select(d => d.Value).First();
                        var desde = xdoc.Descendants("D").Select(d => d.Value).First();
                        var hasta = xdoc.Descendants("H").Select(d => d.Value).First();
                        var caduca = xdoc.Descendants("FA").Select(d => d.Value).First();


                    }
                }
            }
            catch (Exception ex)
            {
                tboxMensajesConfig.Text = ex.Message;
            }

        }
    }
}
