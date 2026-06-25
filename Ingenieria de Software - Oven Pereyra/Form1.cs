using BLL;
using Mapper;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI
{
    public partial class Form1 : Form, IObservadorIdioma
    {
        private UsuarioService service = new UsuarioService();
        private GestorIdioma   gestor  = GestorIdioma.Instancia;
        private Bitacora       log     = Bitacora.Instancia;

        private ComboBox cmbIdioma;
        private Button   btnGestionIdiomas;

        public Form1()
        {
            InitializeComponent();
            AgregarControlesIdioma();
            ConfigurarPantalla();
            gestor.Suscribir(this);
        }

        private void AgregarControlesIdioma()
        {
            cmbIdioma = new ComboBox();
            cmbIdioma.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbIdioma.Size          = new Size(130, 26);
            cmbIdioma.Font          = new Font("Segoe UI", 9, FontStyle.Bold);
            cmbIdioma.FlatStyle     = FlatStyle.Flat;
            cmbIdioma.BackColor     = Color.White;
            cmbIdioma.ForeColor     = Color.FromArgb(40, 70, 130);
            cmbIdioma.Anchor        = AnchorStyles.Top | AnchorStyles.Right;
            cmbIdioma.SelectedIndexChanged += CmbIdioma_SelectedIndexChanged;

            btnGestionIdiomas = new Button();
            btnGestionIdiomas.Text      = "Idiomas";
            btnGestionIdiomas.Size      = new Size(80, 26);
            btnGestionIdiomas.FlatStyle = FlatStyle.Flat;
            btnGestionIdiomas.BackColor = Color.FromArgb(60, 100, 160);
            btnGestionIdiomas.ForeColor = Color.White;
            btnGestionIdiomas.Font      = new Font("Segoe UI", 8, FontStyle.Bold);
            btnGestionIdiomas.Anchor    = AnchorStyles.Top | AnchorStyles.Right;
            btnGestionIdiomas.FlatAppearance.BorderSize = 0;
            btnGestionIdiomas.Click += BtnGestionIdiomas_Click;

            this.Load   += (s, e) => PosicionarControlesIdioma();
            this.Resize += (s, e) => PosicionarControlesIdioma();

            this.Controls.Add(cmbIdioma);
            this.Controls.Add(btnGestionIdiomas);
            cmbIdioma.BringToFront();
            btnGestionIdiomas.BringToFront();

            RefrescarComboIdiomas();
        }

        private void PosicionarControlesIdioma()
        {
            btnGestionIdiomas.Location = new Point(this.ClientSize.Width - btnGestionIdiomas.Width - 10, 10);
            cmbIdioma.Location         = new Point(btnGestionIdiomas.Left - cmbIdioma.Width - 8, 10);
        }

        private void RefrescarComboIdiomas()
        {
            cmbIdioma.SelectedIndexChanged -= CmbIdioma_SelectedIndexChanged;
            cmbIdioma.DataSource    = null;
            cmbIdioma.DataSource    = gestor.ObtenerIdiomas();
            cmbIdioma.DisplayMember = "Nombre";
            cmbIdioma.ValueMember   = "Id";
            if (gestor.IdiomaActual != null)
                cmbIdioma.SelectedValue = gestor.IdiomaActual.Id;
            cmbIdioma.SelectedIndexChanged += CmbIdioma_SelectedIndexChanged;
        }

        private void CmbIdioma_SelectedIndexChanged(object sender, EventArgs e)
        {
            Idioma seleccionado = cmbIdioma.SelectedItem as Idioma;
            if (seleccionado != null)
            {
                string antes = gestor.IdiomaActual != null ? gestor.IdiomaActual.Nombre : "?";
                gestor.CambiarIdioma(seleccionado.Id);
                string despues = gestor.IdiomaActual != null ? gestor.IdiomaActual.Nombre : "?";
                if (antes != despues)
                    log.CambioIdioma(antes, despues);
            }
        }

        private void BtnGestionIdiomas_Click(object sender, EventArgs e)
        {
            var form = new FormIdiomas();
            form.ShowDialog(this);
            form.Dispose();
            gestor.CargarIdiomas();
            RefrescarComboIdiomas();
        }

        private void ConfigurarPantalla()
        {
            textBox2.PasswordChar = '*';
            button1.Text  = gestor.T("btn_login");
            button2.Text  = gestor.T("btn_usuarios");
            button3.Text  = gestor.T("btn_logout");
            button4.Text  = gestor.T("btn_composite");
            button5.Text  = "Bitacora";
            button6.Text  = "Control de Cambios";
            button3.Visible = false;
            button2.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
        }

        public void ActualizarIdioma()
        {
            cmbIdioma.SelectedIndexChanged -= CmbIdioma_SelectedIndexChanged;
            if (gestor.IdiomaActual != null)
                cmbIdioma.SelectedValue = gestor.IdiomaActual.Id;
            cmbIdioma.SelectedIndexChanged += CmbIdioma_SelectedIndexChanged;

            button1.Text = gestor.T("btn_login");
            button2.Text = gestor.T("btn_usuarios");
            button3.Text = gestor.T("btn_logout");
            button4.Text = gestor.T("btn_composite");
            this.Text    = gestor.T("titulo_principal");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string usuario = textBox1.Text.Trim();
            string rol = service.Login(usuario, textBox2.Text.Trim());
            if (rol != null)
            {
                log.LoginExitoso(usuario);
                MessageBox.Show(
                    gestor.T("msg_bienvenido", usuario),
                    gestor.T("msg_acceso_ok"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                textBox1.Visible = false;
                textBox2.Visible = false;
                button1.Visible  = false;
                button3.Visible  = true;

                bool esAdmin = SesionManager.Instancia.EsAdmin();
                button2.Visible = esAdmin;
                button4.Visible = esAdmin;
                button5.Visible = esAdmin;
                button6.Visible = esAdmin;
            }
            else
            {
                log.LoginFallido(usuario);
                MessageBox.Show(
                    gestor.T("msg_cred_error"),
                    gestor.T("titulo_principal"),
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
            string usuario = SesionManager.Instancia.ObtenerUsuarioActual() != null
                ? SesionManager.Instancia.ObtenerUsuarioActual().NombreUsuario
                : "desconocido";
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
            button5.Visible  = false;
            button6.Visible  = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var formComposite = new FormComposite();
            formComposite.ShowDialog(this);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var form = new FormBitacora();
            form.ShowDialog(this);
            form.Dispose();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var form = new FormControlCambios();
            form.ShowDialog(this);
            form.Dispose();
        }

        private void panel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var panel = sender as System.Windows.Forms.Panel;
            e.Graphics.Clear(panel.BackColor);
            using (var pen = new System.Drawing.Pen(Color.FromArgb(220, 220, 220), 1))
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            gestor.Desuscribir(this);
            base.OnFormClosed(e);
        }
    }
}
