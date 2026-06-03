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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                var dao = new UsuarioDAL();
                dao.InicializarBaseDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la base de datos:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.Run(new Form1());
        }
    }
}