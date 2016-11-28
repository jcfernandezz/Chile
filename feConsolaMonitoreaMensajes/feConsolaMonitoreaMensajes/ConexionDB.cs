using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;

using FEMonitoreaMensajes;
using Comun;
using CLConfiguracion;

namespace feConsolaMonitoreaMensajes
{
    class ConexionDB
    {
        //public static string sUsuario = Microsoft.Dexterity.Applications.Dynamics.Globals.UserId.Value;
        //public static string sPassword = Microsoft.Dexterity.Applications.Dynamics.Globals.SqlPassword.Value;
        //public static string sIntercompany = Microsoft.Dexterity.Applications.Dynamics.Globals.IntercompanyId.Value;
        //public static string sSqlDSN = Microsoft.Dexterity.Applications.Dynamics.Globals.SqlDataSourceName;

        private string _Compannia = "";
        private string _Usuario = "";
        private string _Password = "";
        private string _Intercompany = "";
        private string _ServerAddress = "";
        private bool _IntegratedSecurity = false;

        public IConexionAFuenteDatos Elemento;
        public string sMsj;
        private String _ingresoPredeterminado = String.Empty;
        public String IngresoPredeterminado
        {
            get { return _ingresoPredeterminado; }
            set { _ingresoPredeterminado = value; }
        }

        public ConexionDB()
        {
            sMsj = string.Empty;
            Parametros config = new Parametros();
            _ingresoPredeterminado = config.IngresoPredeterminado;
            _ServerAddress = config.servidor;
            sMsj = config.sMsj;

            //Si la app no viene de GP usar seguridad integrada o un usuario sql (configurado en el archivo de inicio)
            if (_Usuario.Equals(string.Empty))
            {
                _IntegratedSecurity = config.seguridadIntegrada;
                _Intercompany = "Dynamics";

                if (_IntegratedSecurity)            //Usar seguridad integrada
                    _Usuario = WindowsIdentity.GetCurrent().Name.Trim();
                else
                {                                   //Usar un usuario sql con privilegios
                    _Usuario = config.usuarioSql;
                    _Password = config.passwordSql;
                }
            }

            Elemento = (IConexionAFuenteDatos)new ConexionAFuenteDatos(_Compannia, _Usuario, _Password, _Intercompany, _ServerAddress, _IntegratedSecurity);
        }
    }
}
