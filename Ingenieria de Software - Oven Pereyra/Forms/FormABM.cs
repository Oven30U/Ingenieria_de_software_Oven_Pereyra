using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Ingenieria_de_Software___Oven_Pereyra.Logica;
using Ingenieria_de_Software___Oven_Pereyra.Modelos;

namespace Ingenieria_de_Software___Oven_Pereyra.Forms
{
    public partial class FormABM : Form
    {
        private UsuarioService service = new UsuarioService();
        private int idSeleccionado = -1;
        private string usuarioSeleccionado = "";

        private DataGridView grilla;
        private TextBox txtUsuario, txtClave;
        private ComboBox cmbRol;
        private Button btnAgregar, btnModificar, btnEliminar, btnLimpiar;
        private Label lblUsuario, lblClave, lblRol, lblTitulo;

        public FormABM()
        {
            InitializeComponent();
            ConstruirUI();
            CargarGrilla();
        }

        private void ConstruirUI()
        {
            this.Text = "Administrar Usuarios";
            this.Size = new System.Drawing.Size(620, 520);
            this.StartPosition = FormStartPosition.CenterParent;

            lblTitulo = new Label { Text = "ABM de Usuarios", Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(20, 15), Size = new System.Drawing.Size(250, 30) };

            lblUsuario = new Label { Text = "Usuario:", Location = new System.Drawing.Point(20, 65), Size = new System.Drawing.Size(60, 20) };
            txtUsuario = new TextBox { Location = new System.Drawing.Point(90, 62), Size = new System.Drawing.Size(150, 20) };

            lblClave = new Label { Text = "Clave:", Location = new System.Drawing.Point(260, 65), Size = new System.Drawing.Size(50, 20) };
            txtClave = new TextBox { Location = new System.Drawing.Point(320, 62), Size = new System.Drawing.Size(150, 20), PasswordChar = '*' };

            lblRol = new Label { Text = "Rol:", Location = new System.Drawing.Point(20, 100), Size = new System.Drawing.Size(60, 20) };
            cmbRol = new ComboBox { Location = new System.Drawing.Point(90, 97), Size = new System.Drawing.Size(150, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRol.Items.AddRange(new string[] { "usuario", "admin" });
            cmbRol.SelectedIndex = 0;

            btnAgregar = new Button { Text = "Agregar", Location = new System.Drawing.Point(20, 135), Size = new System.Drawing.Size(100, 30) };
            btnModificar = new Button { Text = "Modificar", Location = new System.Drawing.Point(130, 135), Size = new System.Drawing.Size(100, 30) };
            btnEliminar = new Button { Text = "Eliminar", Location = new System.Drawing.Point(240, 135), Size = new System.Drawing.Size(100, 30) };
            btnLimpiar = new Button { Text = "Limpiar", Location = new System.Drawing.Point(350, 135), Size = new System.Drawing.Size(100, 30) };

            grilla = new DataGridView
            {
                Location = new System.Drawing.Point(20, 185),
                Size = new System.Drawing.Size(565, 280),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            btnAgregar.Click += BtnAgregar_Click;
            btnModificar.Click += BtnModificar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
            grilla.CellClick += Grilla_CellClick;

            this.Controls.AddRange(new Control[] {
                lblTitulo,
                lblUsuario, txtUsuario,
                lblClave, txtClave,
                lblRol, cmbRol,
                btnAgregar, btnModificar, btnEliminar, btnLimpiar,
                grilla
            });
        }

        private void CargarGrilla()
        {
            List<Usuario> lista = service.ObtenerTodos();
            grilla.DataSource = lista;
            if (grilla.Columns.Contains("Id"))
                grilla.Columns["Id"].HeaderText = "ID";
            if (grilla.Columns.Contains("NombreUsuario"))
                grilla.Columns["NombreUsuario"].HeaderText = "Usuario";
            if (grilla.Columns.Contains("Clave"))
                grilla.Columns["Clave"].Visible = false;
            if (grilla.Columns.Contains("Rol"))
                grilla.Columns["Rol"].HeaderText = "Rol";
        }

        private void Grilla_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var fila = grilla.Rows[e.RowIndex];
            idSeleccionado = (int)fila.Cells["Id"].Value;
            usuarioSeleccionado = fila.Cells["NombreUsuario"].Value.ToString();
            txtUsuario.Text = usuarioSeleccionado;
            txtClave.Text = "";
            cmbRol.SelectedItem = fila.Cells["Rol"].Value.ToString();
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            var (ok, mensaje) = service.Agregar(txtUsuario.Text, txtClave.Text, cmbRol.SelectedItem.ToString());
            MessageBox.Show(mensaje, ok ? "Éxito" : "Error",
                MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (ok) { Limpiar(); CargarGrilla(); }
        }

        private void BtnModificar_Click(object sender, EventArgs e)
        {
            if (idSeleccionado == -1) { MessageBox.Show("Seleccioná un usuario de la grilla.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var (ok, mensaje) = service.Modificar(idSeleccionado, txtUsuario.Text, txtClave.Text, cmbRol.SelectedItem.ToString());
            MessageBox.Show(mensaje, ok ? "Éxito" : "Error",
                MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (ok) { Limpiar(); CargarGrilla(); }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (idSeleccionado == -1) { MessageBox.Show("Seleccioná un usuario de la grilla.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var confirm = MessageBox.Show($"¿Seguro que querés eliminar a '{usuarioSeleccionado}'?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                var (ok, mensaje) = service.Eliminar(idSeleccionado, usuarioSeleccionado);
                MessageBox.Show(mensaje, ok ? "Éxito" : "Atención",
                    MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (ok) { Limpiar(); CargarGrilla(); }
            }
        }

        private void BtnLimpiar_Click(object sender, EventArgs e) => Limpiar();

        private void Limpiar()
        {
            txtUsuario.Clear();
            txtClave.Clear();
            cmbRol.SelectedIndex = 0;
            idSeleccionado = -1;
            usuarioSeleccionado = "";
            grilla.ClearSelection();
        }
    }
}