using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BLL;
using Mapper;

namespace UI
{
    public partial class FormComposite : Form, IObservadorIdioma
    {
        private UsuarioService service = new UsuarioService();
        private GestorIdioma gestor = GestorIdioma.Instancia;
        private Bitacora log = Bitacora.Instancia;
        private Usuario _usuarioEditado;

        private TreeView treeArbol;
        private TreeView treePreview;
        private Label lblUsuarioActual, lblNueva, lblCol1, lblCol2, lblCol4;
        private ListBox lstFamilias, lstParientes;
        private TextBox txtNueva;
        private Button btnAgregar, btnAgregarFamilia, btnAgregarPariente;
        private Button btnEnlazarPariente, btnEliminarSeleccionado, btnGuardarFamilia;

        public FormComposite(Usuario usuario = null)
        {
            _usuarioEditado = usuario ?? SesionManager.Instancia.ObtenerUsuarioActual();
            InitializeComponent();
            ConstruirUI();
            CargarPermisosDelUsuario();
            RefrescarArbol();
            RefrescarListas();
            gestor.Suscribir(this);
        }

        private void CargarPermisosDelUsuario()
        {
            if (_usuarioEditado == null)
            {
                MessageBox.Show(gestor.T("No hay un usuario para editar permisos.", "No user selected to edit permissions."),
                    gestor.T("Atención", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            service.CargarPermisosDeUsuario(_usuarioEditado.Id);
        }

        private void ConstruirUI()
        {
            string nombreUser = _usuarioEditado != null ? _usuarioEditado.NombreUsuario : "(sin usuario)";
            this.Text = gestor.T("Administrar Composite - Usuario: ", "Manage Composite - User: ") + nombreUser;
            this.Size = new System.Drawing.Size(1020, 640);
            this.MinimumSize = new System.Drawing.Size(1020, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);

            lblUsuarioActual = new Label { Text = gestor.T("Editando permisos de: ", "Editing permissions for: ") + nombreUser, Location = new System.Drawing.Point(10, 8), Size = new System.Drawing.Size(990, 22), Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold), ForeColor = System.Drawing.Color.FromArgb(40, 70, 110) };

            var panelIzq = new Panel { Location = new System.Drawing.Point(10, 36), Size = new System.Drawing.Size(230, 555), BackColor = System.Drawing.Color.White, BorderStyle = BorderStyle.FixedSingle };
            treeArbol = new TreeView { Location = new System.Drawing.Point(2, 2), Size = new System.Drawing.Size(224, 549), BorderStyle = BorderStyle.None };
            panelIzq.Controls.Add(treeArbol);

            var panelDer = new Panel { Location = new System.Drawing.Point(248, 36), Size = new System.Drawing.Size(752, 555), BackColor = System.Drawing.Color.White, BorderStyle = BorderStyle.FixedSingle };

            lblNueva = new Label { Text = gestor.T("Nueva familia / pariente:", "New family / member:"), Location = new System.Drawing.Point(8, 10), Size = new System.Drawing.Size(200, 18), Font = new System.Drawing.Font("Segoe UI", 9) };
            txtNueva = new TextBox { Location = new System.Drawing.Point(8, 30), Size = new System.Drawing.Size(430, 22), BorderStyle = BorderStyle.FixedSingle, Font = new System.Drawing.Font("Segoe UI", 9) };
            txtNueva.TextChanged += (s, e) => ActualizarPreview();

            btnAgregar = new Button { Location = new System.Drawing.Point(446, 29), Size = new System.Drawing.Size(298, 24), Text = gestor.T("Agregar familia + pariente", "Add family + member"), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(200, 215, 230), Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Bold) };
            btnAgregar.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            btnAgregar.Click += BtnAgregar_Click;

            int headerTop = 60, listaTop = 78, listaAlto = 370;

            // Col3 (A eliminar) eliminada — Preview ocupa todo ese espacio
            lblCol1 = new Label { Text = gestor.T("Familias", "Families"),  Location = new System.Drawing.Point(8,   headerTop), Size = new System.Drawing.Size(220, 16), Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Bold) };
            lblCol2 = new Label { Text = gestor.T("Parientes", "Members"),  Location = new System.Drawing.Point(236, headerTop), Size = new System.Drawing.Size(220, 16), Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Bold) };
            lblCol4 = new Label { Text = "Preview",                          Location = new System.Drawing.Point(464, headerTop), Size = new System.Drawing.Size(280, 16), Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Bold) };

            lstFamilias  = new ListBox { Location = new System.Drawing.Point(8,   listaTop), Size = new System.Drawing.Size(220, listaAlto), BorderStyle = BorderStyle.FixedSingle, Font = new System.Drawing.Font("Segoe UI", 9), SelectionMode = SelectionMode.One };
            lstParientes = new ListBox { Location = new System.Drawing.Point(236, listaTop), Size = new System.Drawing.Size(220, listaAlto), BorderStyle = BorderStyle.FixedSingle, Font = new System.Drawing.Font("Segoe UI", 9), SelectionMode = SelectionMode.One };
            treePreview  = new TreeView { Location = new System.Drawing.Point(464, listaTop), Size = new System.Drawing.Size(280, listaAlto), BorderStyle = BorderStyle.FixedSingle, Font = new System.Drawing.Font("Segoe UI", 8) };

            lstFamilias.SelectedIndexChanged  += (s, e) => ActualizarPreview();
            lstParientes.SelectedIndexChanged += (s, e) => ActualizarPreview();
            lstFamilias.MouseDown  += ListBox_MouseDown;
            lstParientes.MouseDown += ListBox_MouseDown;

            int fila1 = listaTop + listaAlto + 10;
            int fila2 = fila1 + 36;

            btnAgregarFamilia       = ConstruirBoton(gestor.T("Agregar Familia",            "Add Family"),              8,   fila1, 220, 30, false); btnAgregarFamilia.Click       += BtnAgregarFamilia_Click;
            btnAgregarPariente      = ConstruirBoton(gestor.T("Agregar Pariente",           "Add Member"),            236,   fila1, 220, 30, false); btnAgregarPariente.Click      += BtnAgregarPariente_Click;
            btnEliminarSeleccionado = ConstruirBoton(gestor.T("Eliminar seleccionado",      "Delete selected"),       464,   fila1, 280, 30, true);  btnEliminarSeleccionado.Click += BtnEliminarSeleccionado_Click;
            btnEnlazarPariente      = ConstruirBoton(gestor.T("Enlazar Pariente a Familia", "Link Member to Family"), 236,   fila2, 220, 30, false); btnEnlazarPariente.Click      += BtnEnlazarPariente_Click;

            btnGuardarFamilia = new Button { Location = new System.Drawing.Point(464, fila2), Size = new System.Drawing.Size(280, 30), Text = gestor.T("💾  GUARDAR PERMISOS", "💾  SAVE PERMISSIONS"), FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(100, 180, 100), ForeColor = System.Drawing.Color.White, Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) };
            btnGuardarFamilia.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 140, 60);
            btnGuardarFamilia.Click += BtnGuardarFamilia_Click;

            panelDer.Controls.AddRange(new Control[] { lblNueva, txtNueva, btnAgregar, lblCol1, lstFamilias, lblCol2, lstParientes, lblCol4, treePreview, btnAgregarFamilia, btnAgregarPariente, btnEnlazarPariente, btnEliminarSeleccionado, btnGuardarFamilia });
            this.Controls.AddRange(new Control[] { lblUsuarioActual, panelIzq, panelDer });
        }

        private Button ConstruirBoton(string texto, int x, int y, int w, int h, bool esEliminar)
        {
            var btn = new Button { Location = new System.Drawing.Point(x, y), Size = new System.Drawing.Size(w, h), Text = texto, FlatStyle = FlatStyle.Flat, BackColor = esEliminar ? System.Drawing.Color.FromArgb(230, 200, 200) : System.Drawing.Color.FromArgb(200, 215, 230), Font = new System.Drawing.Font("Segoe UI", 8) };
            btn.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            return btn;
        }

        public void ActualizarIdioma()
        {
            string nombreUser      = _usuarioEditado != null ? _usuarioEditado.NombreUsuario : "";
            this.Text              = gestor.T("Administrar Composite - Usuario: ", "Manage Composite - User: ") + nombreUser;
            lblUsuarioActual.Text  = gestor.T("Editando permisos de: ", "Editing permissions for: ") + nombreUser;
            lblNueva.Text          = gestor.T("Nueva familia / pariente:", "New family / member:");
            lblCol1.Text           = gestor.T("Familias", "Families");
            lblCol2.Text           = gestor.T("Parientes", "Members");
            btnAgregar.Text        = gestor.T("Agregar familia + pariente", "Add family + member");
            btnAgregarFamilia.Text  = gestor.T("Agregar Familia", "Add Family");
            btnAgregarPariente.Text = gestor.T("Agregar Pariente", "Add Member");
            btnEnlazarPariente.Text = gestor.T("Enlazar Pariente a Familia", "Link Member to Family");
            btnEliminarSeleccionado.Text = gestor.T("Eliminar seleccionado", "Delete selected");
            btnGuardarFamilia.Text  = gestor.T("💾  GUARDAR PERMISOS", "💾  SAVE PERMISSIONS");
        }

        private void RefrescarArbol()
        {
            treeArbol.Nodes.Clear();
            treeArbol.Nodes.Add(ArmarNodo(service.ObtenerArbol()));
            treeArbol.ExpandAll();
        }

        private TreeNode ArmarNodo(IComponentePermiso comp)
        {
            var nodo = new TreeNode(comp.Nombre);
            GrupoPermiso g = comp as GrupoPermiso;
            if (g != null) foreach (var h in g.Hijos()) nodo.Nodes.Add(ArmarNodo(h));
            return nodo;
        }

        private void RefrescarListas()
        {
            string fa = lstFamilias.SelectedItem  != null ? lstFamilias.SelectedItem.ToString()  : null;
            string pa = lstParientes.SelectedItem != null ? lstParientes.SelectedItem.ToString() : null;
            lstFamilias.Items.Clear(); lstParientes.Items.Clear();
            GrupoPermiso raiz = service.ObtenerArbol();
            RecolectarGrupos(raiz, lstFamilias);
            RecolectarHojas(raiz, lstParientes);
            foreach (string p in service.ObtenerParientesDisponibles())
                if (!lstParientes.Items.Contains(p)) lstParientes.Items.Add(p);
            if (fa != null && lstFamilias.Items.Contains(fa))  lstFamilias.SelectedItem  = fa;
            if (pa != null && lstParientes.Items.Contains(pa)) lstParientes.SelectedItem = pa;
        }

        private void RecolectarGrupos(GrupoPermiso g, ListBox lb) { lb.Items.Add(g.Nombre); foreach (var h in g.Hijos()) { GrupoPermiso gg = h as GrupoPermiso; if (gg != null) RecolectarGrupos(gg, lb); } }
        private void RecolectarHojas(GrupoPermiso g, ListBox lb)  { foreach (var h in g.Hijos()) { if (h is PermisoLeaf) { if (!lb.Items.Contains(h.Nombre)) lb.Items.Add(h.Nombre); } else { GrupoPermiso gg = h as GrupoPermiso; if (gg != null) RecolectarHojas(gg, lb); } } }

        private void ActualizarPreview()
        {
            treePreview.Nodes.Clear();
            string nn = txtNueva.Text.Trim();
            string fs = lstFamilias.SelectedItem  != null ? lstFamilias.SelectedItem.ToString()  : null;
            string ps = lstParientes.SelectedItem != null ? lstParientes.SelectedItem.ToString() : null;
            bool hn = !string.IsNullOrWhiteSpace(nn), hf = !string.IsNullOrWhiteSpace(fs), hp = !string.IsNullOrWhiteSpace(ps);
            if (hn || hf || hp)
            {
                TreeNode np = new TreeNode(hf ? fs : "(familia padre)");
                if (hn) { var nh = new TreeNode(nn); if (hp) nh.Nodes.Add(new TreeNode(ps)); np.Nodes.Add(nh); }
                else if (hp) np.Nodes.Add(new TreeNode(ps));
                treePreview.Nodes.Add(np); treePreview.ExpandAll();
            }
        }

        private GrupoPermiso BuscarGrupo(GrupoPermiso actual, string nombre)
        {
            if (actual.Nombre == nombre) return actual;
            foreach (var h in actual.Hijos()) { GrupoPermiso g = h as GrupoPermiso; if (g != null) { var r = BuscarGrupo(g, nombre); if (r != null) return r; } }
            return null;
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            string nn = txtNueva.Text.Trim(), pn = lstFamilias.SelectedItem != null ? lstFamilias.SelectedItem.ToString() : null, par = lstParientes.SelectedItem != null ? lstParientes.SelectedItem.ToString() : null;
            if (string.IsNullOrWhiteSpace(nn))  { MessageBox.Show(gestor.T("Escribí el nombre de la nueva familia.", "Enter the name of the new family."), gestor.T("Atención","Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(pn))  { MessageBox.Show(gestor.T("Seleccioná una familia padre.", "Select a parent family."), gestor.T("Atención","Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            Resultado res = service.AgregarFamilia(nn, pn);
            if (!res.Ok) { MessageBox.Show(res.Mensaje, gestor.T("Error","Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!string.IsNullOrWhiteSpace(par)) { Resultado rp = service.AgregarPariente(par, nn); if (!rp.Ok) MessageBox.Show(gestor.T("Familia creada pero no se pudo agregar el pariente: ","Family created but member could not be added: ") + rp.Mensaje, gestor.T("Atención","Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            RefrescarArbol(); RefrescarListas(); txtNueva.Clear(); ActualizarPreview();
            MessageBox.Show(gestor.T("Agregado correctamente.", "Added successfully."), gestor.T("Éxito","Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAgregarFamilia_Click(object sender, EventArgs e)
        {
            string nn = txtNueva.Text.Trim(), pn = lstFamilias.SelectedItem != null ? lstFamilias.SelectedItem.ToString() : null;
            if (string.IsNullOrWhiteSpace(nn)) { MessageBox.Show(gestor.T("Escribí el nombre de la nueva familia.", "Enter the name of the new family."), gestor.T("Atención","Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(pn)) { MessageBox.Show(gestor.T("Seleccioná una familia padre.", "Select a parent family."), gestor.T("Atención","Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            Resultado res = service.AgregarFamilia(nn, pn);
            MessageBox.Show(res.Mensaje, res.Ok ? gestor.T("Éxito","Success") : gestor.T("Error","Error"), MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok) { txtNueva.Clear(); RefrescarArbol(); RefrescarListas(); ActualizarPreview(); }
        }

        private void BtnAgregarPariente_Click(object sender, EventArgs e)
        {
            string nn = txtNueva.Text.Trim();
            if (string.IsNullOrWhiteSpace(nn)) { MessageBox.Show(gestor.T("Escribí el nombre del pariente.", "Enter the member name."), gestor.T("Atención","Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (lstParientes.Items.Contains(nn)) { MessageBox.Show(gestor.T("Ya existe un pariente con ese nombre.", "A member with that name already exists."), gestor.T("Atención","Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            service.AgregarParienteLibre(nn);
            txtNueva.Clear(); RefrescarArbol(); RefrescarListas(); ActualizarPreview();
        }

        private void BtnEnlazarPariente_Click(object sender, EventArgs e)
        {
            string pn = lstParientes.SelectedItem != null ? lstParientes.SelectedItem.ToString() : null, fn = lstFamilias.SelectedItem != null ? lstFamilias.SelectedItem.ToString() : null;
            if (string.IsNullOrWhiteSpace(pn)) { MessageBox.Show(gestor.T("Seleccioná un pariente.", "Select a member."), gestor.T("Atención","Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(fn)) { MessageBox.Show(gestor.T("Seleccioná una familia destino.", "Select a destination family."), gestor.T("Atención","Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            Resultado res = service.AgregarPariente(pn, fn);
            MessageBox.Show(res.Mensaje, res.Ok ? gestor.T("Éxito","Success") : gestor.T("Error","Error"), MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok) { RefrescarArbol(); RefrescarListas(); ActualizarPreview(); }
        }

        private void BtnEliminarSeleccionado_Click(object sender, EventArgs e)
        {
            string fs = lstFamilias.SelectedItem != null ? lstFamilias.SelectedItem.ToString() : null, ps = lstParientes.SelectedItem != null ? lstParientes.SelectedItem.ToString() : null;
            if (string.IsNullOrWhiteSpace(fs) && string.IsNullOrWhiteSpace(ps)) { MessageBox.Show(gestor.T("Seleccioná una familia o pariente para eliminar.", "Select a family or member to delete."), gestor.T("Atención","Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string msg = gestor.T("¿Confirmas eliminar:\n","Confirm delete:\n");
            if (!string.IsNullOrWhiteSpace(fs)) msg += "  - " + gestor.T("Familia: ","Family: ") + fs + "\n";
            if (!string.IsNullOrWhiteSpace(ps)) msg += "  - " + gestor.T("Pariente: ","Member: ") + ps + "\n";
            if (MessageBox.Show(msg, gestor.T("Confirmar","Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (!string.IsNullOrWhiteSpace(fs)) service.EliminarFamilia(fs);
                if (!string.IsNullOrWhiteSpace(ps)) service.EliminarPariente(ps);
                RefrescarArbol(); RefrescarListas(); ActualizarPreview();
            }
        }

        private void BtnGuardarFamilia_Click(object sender, EventArgs e)
        {
            if (_usuarioEditado == null) { MessageBox.Show(gestor.T("No hay un usuario para guardar permisos.", "No user to save permissions for."), gestor.T("Error","Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string tipoPermiso = service.ObtenerTipoPermiso();
            Resultado res = service.GuardarPermisosDeUsuario(tipoPermiso);
            if (res.Ok)
            {
                string operador = SesionManager.Instancia.ObtenerUsuarioActual() != null ? SesionManager.Instancia.ObtenerUsuarioActual().NombreUsuario : "desconocido";
                log.PermisosGuardados(operador, _usuarioEditado.NombreUsuario, tipoPermiso);
            }
            MessageBox.Show(res.Mensaje, res.Ok ? gestor.T("Guardar","Save") : gestor.T("Error","Error"),
                MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void ListBox_MouseDown(object sender, MouseEventArgs e)
        {
            ListBox lb = sender as ListBox;
            int idx = lb.IndexFromPoint(e.Location);
            if (idx >= 0 && lb.SelectedIndex == idx) { lb.SelectedIndex = -1; ActualizarPreview(); }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            gestor.Desuscribir(this);
            base.OnFormClosed(e);
        }
    }
}
