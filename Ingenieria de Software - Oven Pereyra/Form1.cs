using BLL;
using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UI
{
    public partial class Form1 : Form
    {
        private UsuarioService service = new UsuarioService();
        public Form1()
        {
            InitializeComponent();
            ConfigurarPantalla();
        }
        private void ConfigurarPantalla()
        {
            textBox2.PasswordChar = '*';
            button1.Text = "Login";
            button2.Text = "Administrar Usuarios";
            button3.Text = "Logout";
            button3.Visible = false;
            button2.Visible = false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string rol = service.Login(textBox1.Text.Trim(), textBox2.Text.Trim());
            if (rol != null)
            {
                MessageBox.Show($"Bienvenido, {textBox1.Text}!", "Acceso correcto",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox1.Visible = false;
                textBox2.Visible = false;
                button1.Visible = false;
                button3.Visible = true;
                button2.Visible = SesionManager.Instancia.EsAdmin();
            }
            else
            {
                MessageBox.Show("Usuario o clave incorrectos.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            var formABM = new FormABM();
            formABM.ShowDialog(this);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            service.Logout();
            textBox1.Clear();
            textBox2.Clear();
            textBox1.Visible = true;
            textBox2.Visible = true;
            button1.Visible = true;
            button2.Visible = false;
            button3.Visible = false;
        }
        private void panel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var panel = sender as System.Windows.Forms.Panel;
            e.Graphics.Clear(panel.BackColor);
            using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(220, 220, 220), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            }
        }
    }
}