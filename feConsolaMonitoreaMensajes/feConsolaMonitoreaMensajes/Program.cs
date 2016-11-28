using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Comun;
using FEMonitoreaMensajes;
using CLConfiguracion;
using MaquinaDeEstados;

namespace feConsolaMonitoreaMensajes
{
    class Program
    {
        static private ConexionDB DatosConexionDB;

        static void Main(string[] args)
        {
            int _ierr = 0;
            String _sMsj = String.Empty;
            DatosConexionDB = new ConexionDB();         //Lee la configuración del archivo xml y obtiene los datos de conexión.
            DatosConexionDB.Elemento.Compannia = DatosConexionDB.IngresoPredeterminado;
            DatosConexionDB.Elemento.Intercompany = DatosConexionDB.IngresoPredeterminado;

            if (!DatosConexionDB.sMsj.Equals(string.Empty))
            {
                reportaProgreso(100, "Excepción al leer los datos de conexión. " + DatosConexionDB.sMsj + " [Main]");
                return;
            }

            Parametros param = new Parametros(DatosConexionDB.Elemento.Compannia);
            if (param.iErr != 0)
            {
                reportaProgreso(100, "Excepción al leer los parámetros de la aplicación. " + param.sMsj + " [Main]");
                return;
            }

            MonitorDeMensajes monitor = new MonitorDeMensajes((IParametros)param, (IConexionAFuenteDatos)DatosConexionDB.Elemento, param.emailPop3, param.emailPortIn);
            _ierr = monitor.iErr;
            _sMsj = monitor.sMsj;
            if (monitor.iErr == 0)
            {
                reportaProgreso(100, "Chile - Factura electrónica recepción de documentos v. 1.0.2");

                monitor.Progreso += new MonitorDeMensajes.reportaProgreso(reportaProgreso);
                monitor.MonitoreaComunicacionesInbound(false, Maquina.estadoBaseReceptor);
                _ierr = monitor.iErr;
                _sMsj = monitor.sMsj;
            }

            if (_ierr == 0)
                reportaProgreso(100, "Fin de la recepción de mensajes.");
            else
                reportaProgreso(100, "Excepción al iniciar el monitoreo de mensajes recibidos. " + _sMsj + " [Main]");

            reportaProgreso(100, "Presione cualquier tecla para cerrar esta ventana.");
            Console.ReadKey();
        }
        static void reportaProgreso(int i, string s)
        {
            //iProgreso = i;
            Console.WriteLine(s + "\r\n");
        }

    }
}
