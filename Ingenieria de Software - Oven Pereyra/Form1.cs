using BLL;
using Mapper;
using System;
using System.Windows.Forms;

namespace UI
{
    public partial class Form1 : Form, IObservadorIdioma
    {
        private UsuarioService service = new UsuarioService();
        private GestorIdioma gestor = GestorIdioma.Instancia;
        private Bitacora log = Bitacora.Instancia;
        private Button btnIdioma;

        public Form1()
        {
            InitializeComponent();
            AgregarBotonIdioma();
            ConfigurarPantalla();
            gestor.Suscribir(this);
        }

        private void AgregarBotonIdioma()
        {
            btnIdioma = new Button
            {
                Text      = "English",
                Size      = new System.Drawing.Size(90, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(60, 100, 160),
                ForeColor = System.Drawing.Color.White,
                Font      = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            btnIdioma.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(40, 70, 120);
            btnIdioma.Click += (s, e) =>
            {
                string antes = gestor.EsEspanol ? "Español" : "Ingles";
                gestor.CambiarIdioma();
                string despues = gestor.EsEspanol ? "Español" : "Ingles";
                log.CambioIdioma(antes, despues);
            };

            this.Load += (s, e) =>
                btnIdioma.Location = new System.Drawing.Point(this.ClientSize.Width - btnIdioma.Width - 10, 10);
            this.Resize += (s, e) =>
                btnIdioma.Location = new System.Drawing.Point(this.ClientSize.Width - btnIdioma.Width - 10, 10);

            this.Controls.Add(btnIdioma);
            btnIdioma.BringToFront();
        }

        private void ConfigurarPantalla()
        {
            textBox2.PasswordChar = '*';
            button1.Text = gestor.T("Iniciar sesión", "Log in");
            button2.Text = gestor.T("Administrar Usuarios", "Manage Users");
            button3.Text = gestor.T("Cerrar sesión", "Log out");
            button4.Text = gestor.T("Administrar Composite", "Manage Composite");
            button3.Visible = false;
            button2.Visible = false;
            button4.Visible = false;
        }

        public void ActualizarIdioma()
        {
            btnIdioma.Text   = gestor.EsEspanol ? "English" : "Español";
            button1.Text     = gestor.T("Iniciar sesión", "Log in");
            button2.Text     = gestor.T("Administrar Usuarios", "Manage Users");
            button3.Text     = gestor.T("Cerrar sesión", "Log out");
            button4.Text     = gestor.T("Administrar Composite", "Manage Composite");
            this.Text        = gestor.T("Sistema de Usuarios", "User System");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string usuario = textBox1.Text.Trim();
            string rol = service.Login(usuario, textBox2.Text.Trim());
            if (rol != null)
            {
                log.LoginExitoso(usuario);
                MessageBox.Show(
                    gestor.T($"Bienvenido, {usuario}!", $"Welcome, {usuario}!"),
                    gestor.T("Acceso correcto", "Access granted"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                textBox1.Visible = false;
                textBox2.Visible = false;
                button1.Visible  = false;
                button3.Visible  = true;
                button2.Visible  = SesionManager.Instancia.EsAdmin();
                button4.Visible  = SesionManager.Instancia.EsAdmin();
            }
            else
            {
                log.LoginFallido(usuario);
                MessageBox.Show(
                    gestor.T("Usuario o clave incorrectos.", "Incorrect username or password."),
                    gestor.T("Error", "Error"),
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
            string usuario = SesionManager.Instancia.ObtenerUsuarioActual()?.NombreUsuario ?? "desconocido";
            service.Logout();
            log.Logout(usuario);

            textBox1.Clear();
            textBox2.Clear();
            textBox1.Visible = true;
            textBox2.Visible = true;
            button1.Visible  = true;
            button2.Visible  = false;
            button3.Visible  = false;
            button4.Visible  = false;
        }

        private void panel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var panel = sender as System.Windows.Forms.Panel;
            e.Graphics.Clear(panel.BackColor);
            using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(220, 220, 220), 1))
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var formComposite = new FormComposite();
            formComposite.ShowDialog(this);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            gestor.Desuscribir(this);
            base.OnFormClosed(e);
        }
    }
}
