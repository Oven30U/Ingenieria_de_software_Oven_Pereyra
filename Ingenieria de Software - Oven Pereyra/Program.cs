using BLL;
using DAL;
using System;
using System.Windows.Forms;

namespace UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Inicializar la bitacora al arrancar (crea carpeta logs y archivo nuevo)
            Bitacora log = Bitacora.Instancia;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var dao = new UsuarioDAL();
                dao.InicializarBaseDatos();
                log.Info("INICIO", "Base de datos inicializada correctamente.");
            }
            catch (Exception ex)
            {
                log.ErrorBaseDatos("InicializarBaseDatos", ex.Message);
                MessageBox.Show("Error al conectar con la base de datos:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                log.CierreApp();
                return;
            }

            // Registrar cierre de la app al salir
            Application.ApplicationExit += (s, e) => log.CierreApp();

            Application.Run(new Form1());
        }
    }
}
