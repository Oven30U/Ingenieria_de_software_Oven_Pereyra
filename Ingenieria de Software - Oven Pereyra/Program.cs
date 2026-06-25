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
            Bitacora log = Bitacora.Instancia;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var dao = new UsuarioDAL();
                dao.InicializarBaseDatos();

                var idiomaDAL = new IdiomaDAL();
                idiomaDAL.InicializarTablas();

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

            Application.ApplicationExit += (s, e) => log.CierreApp();
            Application.Run(new Form1());
        }
    }
}
