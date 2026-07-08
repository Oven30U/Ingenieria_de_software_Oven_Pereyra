using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BLL;
using Mapper;

namespace UI
{
    public partial class FormABM : Form, IObservadorIdioma
    {
        private UsuarioService service = new UsuarioService();
        private GestorIdioma gestor = GestorIdioma.Instancia;
        private Bitacora log = Bitacora.Instancia;
        private int idSeleccionado = -1;
        private string usuarioSeleccionado = "";

        private DataGridView grilla;
        private TextBox txtUsuario, txtClave;
        private ComboBox cmbRol;
        private Button btnAgregar, btnModificar, btnEliminar, btnLimpiar, btnPermisos, btnEliminarPermisos;
        private Label lblUsuario, lblClave, lblRol, lblTitulo;

        public FormABM()
        {
            InitializeComponent();
            ConstruirUI();
            CargarGrilla();
            gestor.Suscribir(this);
        }

        private void ConstruirUI()
        {
            this.Text = gestor.T("Administrar Usuarios", "Manage Users");
            this.Size = new System.Drawing.Size(900, 520);
            this.StartPosition = FormStartPosition.CenterParent;

            lblTitulo   = new Label { Text = gestor.T("ABM de Usuarios", "User Management"), Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(20, 15), Size = new System.Drawing.Size(300, 30) };
            lblUsuario  = new Label { Text = gestor.T("Usuario:", "Username:"), Location = new System.Drawing.Point(20, 65), Size = new System.Drawing.Size(70, 20) };
            txtUsuario  = new TextBox { Location = new System.Drawing.Point(100, 62), Size = new System.Drawing.Size(150, 20) };
            lblClave    = new Label { Text = gestor.T("Clave:", "Password:"), Location = new System.Drawing.Point(270, 65), Size = new System.Drawing.Size(60, 20) };
            txtClave    = new TextBox { Location = new System.Drawing.Point(340, 62), Size = new System.Drawing.Size(150, 20), PasswordChar = '*' };
            lblRol      = new Label { Text = gestor.T("Rol:", "Role:"), Location = new System.Drawing.Point(20, 100), Size = new System.Drawing.Size(70, 20) };
            cmbRol      = new ComboBox { Location = new System.Drawing.Point(100, 97), Size = new System.Drawing.Size(150, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRol.Items.AddRange(new string[] { "usuario", "admin" });
            cmbRol.SelectedIndex = 0;

            btnAgregar   = new Button { Text = gestor.T("Agregar", "Add"),      Location = new System.Drawing.Point(20,  135), Size = new System.Drawing.Size(100, 30) };
            btnModificar = new Button { Text = gestor.T("Modificar", "Modify"), Location = new System.Drawing.Point(130, 135), Size = new System.Drawing.Size(100, 30) };
            btnEliminar  = new Button { Text = gestor.T("Eliminar", "Delete"),  Location = new System.Drawing.Point(240, 135), Size = new System.Drawing.Size(100, 30) };
            btnLimpiar   = new Button { Text = gestor.T("Limpiar", "Clear"),    Location = new System.Drawing.Point(350, 135), Size = new System.Drawing.Size(100, 30) };
            btnPermisos  = new Button { Text = gestor.T("Editar Permisos", "Edit Permissions"), Location = new System.Drawing.Point(460, 135), Size = new System.Drawing.Size(130, 30), BackColor = System.Drawing.Color.FromArgb(180, 220, 180) };
            btnEliminarPermisos = new Button { Text = gestor.T("Eliminar Permisos", "Remove Permissions"), Location = new System.Drawing.Point(600, 135), Size = new System.Drawing.Size(130, 30), BackColor = System.Drawing.Color.FromArgb(220, 90, 90), ForeColor = System.Drawing.Color.White, Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Bold) };

            grilla = new DataGridView
            {
                Location           = new System.Drawing.Point(20, 185),
                Size               = new System.Drawing.Size(840, 280),
                ReadOnly           = true,
                SelectionMode      = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect        = false,
                AllowUserToAddRows = false
            };

            btnAgregar.Click   += BtnAgregar_Click;
            btnModificar.Click += BtnModificar_Click;
            btnEliminar.Click  += BtnEliminar_Click;
            btnLimpiar.Click   += BtnLimpiar_Click;
            btnPermisos.Click  += BtnPermisos_Click;
            btnEliminarPermisos.Click += BtnEliminarPermisos_Click;
            grilla.CellClick   += Grilla_CellClick;

            this.Controls.AddRange(new Control[] {
                lblTitulo, lblUsuario, txtUsuario, lblClave, txtClave,
                lblRol, cmbRol, btnAgregar, btnModificar, btnEliminar,
                btnLimpiar, btnPermisos, btnEliminarPermisos, grilla
            });
        }

        public void ActualizarIdioma()
        {
            this.Text            = gestor.T("Administrar Usuarios", "Manage Users");
            lblTitulo.Text       = gestor.T("ABM de Usuarios", "User Management");
            lblUsuario.Text      = gestor.T("Usuario:", "Username:");
            lblClave.Text        = gestor.T("Clave:", "Password:");
            lblRol.Text          = gestor.T("Rol:", "Role:");
            btnAgregar.Text      = gestor.T("Agregar", "Add");
            btnModificar.Text    = gestor.T("Modificar", "Modify");
            btnEliminar.Text     = gestor.T("Eliminar", "Delete");
            btnLimpiar.Text      = gestor.T("Limpiar", "Clear");
            btnPermisos.Text     = gestor.T("Editar Permisos", "Edit Permissions");
            btnEliminarPermisos.Text = gestor.T("Eliminar Permisos", "Remove Permissions");

            if (grilla.Columns.Contains("NombreUsuario"))    grilla.Columns["NombreUsuario"].HeaderText    = gestor.T("Usuario", "Username");
            if (grilla.Columns.Contains("Rol"))               grilla.Columns["Rol"].HeaderText               = gestor.T("Rol", "Role");
            if (grilla.Columns.Contains("TipoPermiso"))      grilla.Columns["TipoPermiso"].HeaderText       = gestor.T("Tipo Permiso", "Permission Type");
        }

        private void CargarGrilla()
        {
            List<Usuario> lista = service.ObtenerTodos();
            grilla.DataSource = lista;
            if (grilla.Columns.Contains("Id"))            grilla.Columns["Id"].HeaderText            = "ID";
            if (grilla.Columns.Contains("NombreUsuario")) grilla.Columns["NombreUsuario"].HeaderText  = gestor.T("Usuario", "Username");
            if (grilla.Columns.Contains("Clave"))         grilla.Columns["Clave"].Visible             = false;
            if (grilla.Columns.Contains("TienePermisos")) grilla.Columns["TienePermisos"].Visible     = false;
            if (grilla.Columns.Contains("Rol"))           grilla.Columns["Rol"].HeaderText            = gestor.T("Rol", "Role");
            if (grilla.Columns.Contains("TipoPermiso"))   grilla.Columns["TipoPermiso"].HeaderText    = gestor.T("Tipo Permiso", "Permission Type");
        }

        private void Grilla_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var fila = grilla.Rows[e.RowIndex];
            idSeleccionado      = (int)fila.Cells["Id"].Value;
            usuarioSeleccionado = fila.Cells["NombreUsuario"].Value.ToString();
            txtUsuario.Text     = usuarioSeleccionado;
            txtClave.Text       = "";
            cmbRol.SelectedItem = fila.Cells["Rol"].Value.ToString();
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            string operador = SesionManager.Instancia.ObtenerUsuarioActual()?.NombreUsuario ?? "desconocido";
            string nuevoUsuario = txtUsuario.Text;
            string rol = cmbRol.SelectedItem.ToString();

            Resultado res = service.Agregar(nuevoUsuario, txtClave.Text, rol);
            if (res.Ok)
                log.UsuarioAgregado(operador, nuevoUsuario, rol);
            else
                log.Advertencia("ABM", $"'{operador}' intento agregar '{nuevoUsuario}' pero fallo: {res.Mensaje}");

            MessageBox.Show(res.Mensaje, res.Ok ? gestor.T("Éxito", "Success") : gestor.T("Error", "Error"),
                MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok) { Limpiar(); CargarGrilla(); }
        }

        private void BtnModificar_Click(object sender, EventArgs e)
        {
            if (idSeleccionado == -1)
            {
                MessageBox.Show(gestor.T("Seleccioná un usuario de la grilla.", "Select a user from the list."),
                    gestor.T("Atención", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string operador = SesionManager.Instancia.ObtenerUsuarioActual()?.NombreUsuario ?? "desconocido";
            Resultado res = service.Modificar(idSeleccionado, txtUsuario.Text, txtClave.Text, cmbRol.SelectedItem.ToString());
            if (res.Ok)
                log.UsuarioModificado(operador, usuarioSeleccionado);
            else
                log.Advertencia("ABM", $"'{operador}' intento modificar '{usuarioSeleccionado}' pero fallo: {res.Mensaje}");

            MessageBox.Show(res.Mensaje, res.Ok ? gestor.T("Éxito", "Success") : gestor.T("Error", "Error"),
                MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok) { Limpiar(); CargarGrilla(); }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (idSeleccionado == -1)
            {
                MessageBox.Show(gestor.T("Seleccioná un usuario de la grilla.", "Select a user from the list."),
                    gestor.T("Atención", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var confirm = MessageBox.Show(
                gestor.T($"¿Seguro que querés eliminar a '{usuarioSeleccionado}'?", $"Are you sure you want to delete '{usuarioSeleccionado}'?"),
                gestor.T("Confirmar", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string operador = SesionManager.Instancia.ObtenerUsuarioActual()?.NombreUsuario ?? "desconocido";
                Resultado res = service.Eliminar(idSeleccionado, usuarioSeleccionado);
                if (res.Ok)
                    log.UsuarioEliminado(operador, usuarioSeleccionado);
                else
                    log.Advertencia("ABM", $"'{operador}' intento eliminar '{usuarioSeleccionado}' pero fallo: {res.Mensaje}");

                MessageBox.Show(res.Mensaje, res.Ok ? gestor.T("Éxito", "Success") : gestor.T("Atención", "Warning"),
                    MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (res.Ok) { Limpiar(); CargarGrilla(); }
            }
        }

        private void BtnPermisos_Click(object sender, EventArgs e)
        {
            if (idSeleccionado == -1)
            {
                MessageBox.Show(gestor.T("Seleccioná un usuario de la grilla.", "Select a user from the list."),
                    gestor.T("Atención", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var usuarios = service.ObtenerTodos();
            Usuario uSel = usuarios.Find(x => x.Id == idSeleccionado);
            if (uSel == null)
            {
                MessageBox.Show(gestor.T("No se encontró el usuario.", "User not found."),
                    gestor.T("Error", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            using (var form = new FormComposite(uSel))
                form.ShowDialog(this);
            CargarGrilla();
        }

        private void BtnEliminarPermisos_Click(object sender, EventArgs e)
        {
            if (idSeleccionado == -1)
            {
                MessageBox.Show(gestor.T("Seleccioná un usuario de la grilla.", "Select a user from the list."),
                    gestor.T("Atención", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var confirm = MessageBox.Show(
                gestor.T($"¿Seguro que querés quitarle la familia de permisos a '{usuarioSeleccionado}'?", $"Are you sure you want to remove the permission family from '{usuarioSeleccionado}'?"),
                gestor.T("Confirmar", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string operador = SesionManager.Instancia.ObtenerUsuarioActual()?.NombreUsuario ?? "desconocido";
                Resultado res = service.EliminarPermisosDeUsuario(idSeleccionado);
                if (res.Ok)
                    log.PermisosGuardados(operador, usuarioSeleccionado, "(sin permisos)");
                else
                    log.Advertencia("ABM", $"'{operador}' intento quitar permisos a '{usuarioSeleccionado}' pero fallo: {res.Mensaje}");

                MessageBox.Show(res.Mensaje, res.Ok ? gestor.T("Éxito", "Success") : gestor.T("Error", "Error"),
                    MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (res.Ok) { Limpiar(); CargarGrilla(); }
            }
        }

        private void BtnLimpiar_Click(object sender, EventArgs e) => Limpiar();

        private void Limpiar()
        {
            txtUsuario.Clear();
            txtClave.Clear();
            cmbRol.SelectedIndex = 0;
            idSeleccionado       = -1;
            usuarioSeleccionado  = "";
            grilla.ClearSelection();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            gestor.Desuscribir(this);
            base.OnFormClosed(e);
        }
    }
}
