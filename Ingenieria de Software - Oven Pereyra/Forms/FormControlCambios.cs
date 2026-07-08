using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using DAL;
using Mapper;

namespace UI
{
    public class FormControlCambios : Form
    {
        private readonly UsuarioDAL    _dal     = new UsuarioDAL();
        private readonly SesionManager _sesion  = SesionManager.Instancia;
        private readonly Bitacora      _log     = Bitacora.Instancia;

        private Label            lblTitulo;
        private Label            lblFiltroUsuario;
        private ComboBox         cmbUsuario;
        private Label            lblFiltroEvento;
        private ComboBox         cmbEvento;
        private Label            lblDesde;
        private DateTimePicker   dtpDesde;
        private Label            lblHasta;
        private DateTimePicker   dtpHasta;
        private Button           btnFiltrar;
        private Button           btnLimpiar;
        private Button           btnRestaurar;
        private Button           btnCerrar;
        private DataGridView     grid;
        private Label            lblTotal;
        private Label            lblAviso;

        private List<RegistroClave> _todos = new List<RegistroClave>();

        public FormControlCambios()
        {
            ConstruirUI();
            CargarDatos();
        }

        private void ConstruirUI()
        {
            this.Text            = "Control de Cambios - Claves";
            this.ClientSize      = new Size(980, 580);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize     = new Size(860, 480);
            this.Font            = new Font("Segoe UI", 9);
            this.BackColor       = Color.FromArgb(245, 246, 250);

            lblTitulo           = new Label();
            lblTitulo.Text      = "Control de Cambios de Claves";
            lblTitulo.Font      = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(40, 70, 130);
            lblTitulo.Location  = new Point(16, 12);
            lblTitulo.AutoSize  = true;

            lblAviso           = new Label();
            lblAviso.Text      = "Solo se pueden restaurar claves que tuvieron al menos una modificacion posterior al alta.";
            lblAviso.ForeColor = Color.FromArgb(120, 80, 0);
            lblAviso.Location  = new Point(16, 44);
            lblAviso.AutoSize  = true;
            lblAviso.Font      = new Font("Segoe UI", 8, FontStyle.Italic);

            lblFiltroUsuario           = new Label();
            lblFiltroUsuario.Text      = "Usuario:";
            lblFiltroUsuario.Location  = new Point(16, 72);
            lblFiltroUsuario.Size      = new Size(58, 24);
            lblFiltroUsuario.TextAlign = ContentAlignment.MiddleLeft;

            cmbUsuario = new ComboBox();
            cmbUsuario.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUsuario.Location      = new Point(76, 70);
            cmbUsuario.Size          = new Size(160, 24);

            lblFiltroEvento           = new Label();
            lblFiltroEvento.Text      = "Evento:";
            lblFiltroEvento.Location  = new Point(246, 72);
            lblFiltroEvento.Size      = new Size(52, 24);
            lblFiltroEvento.TextAlign = ContentAlignment.MiddleLeft;

            cmbEvento = new ComboBox();
            cmbEvento.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEvento.Location      = new Point(300, 70);
            cmbEvento.Size          = new Size(130, 24);
            cmbEvento.Items.AddRange(new object[] { "Todos", "ALTA", "MODIFICACION", "RESTAURACION" });
            cmbEvento.SelectedIndex = 0;

            lblDesde           = new Label();
            lblDesde.Text      = "Desde:";
            lblDesde.Location  = new Point(442, 72);
            lblDesde.Size      = new Size(50, 24);
            lblDesde.TextAlign = ContentAlignment.MiddleLeft;

            dtpDesde        = new DateTimePicker();
            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpDesde.Location = new Point(494, 70);
            dtpDesde.Size   = new Size(110, 24);
            dtpDesde.Value  = DateTime.Today.AddMonths(-3);

            lblHasta           = new Label();
            lblHasta.Text      = "Hasta:";
            lblHasta.Location  = new Point(614, 72);
            lblHasta.Size      = new Size(50, 24);
            lblHasta.TextAlign = ContentAlignment.MiddleLeft;

            dtpHasta        = new DateTimePicker();
            dtpHasta.Format = DateTimePickerFormat.Short;
            dtpHasta.Location = new Point(666, 70);
            dtpHasta.Size   = new Size(110, 24);
            dtpHasta.Value  = DateTime.Today;

            btnFiltrar  = CrearBoton("Filtrar",  new Point(786, 68), 80,  Color.FromArgb(25, 118, 210));
            btnLimpiar  = CrearBoton("Limpiar",  new Point(874, 68), 80,  Color.FromArgb(100, 100, 100));

            btnFiltrar.Click += (s, e) => AplicarFiltro();
            btnLimpiar.Click += BtnLimpiar_Click;

            grid                       = new DataGridView();
            grid.Location              = new Point(16, 106);
            grid.Size                  = new Size(948, 408);
            grid.Anchor                = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.ReadOnly              = true;
            grid.AllowUserToAddRows    = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect           = false;
            grid.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor       = Color.White;
            grid.BorderStyle           = BorderStyle.FixedSingle;
            grid.RowHeadersVisible     = false;
            grid.Font                  = new Font("Segoe UI", 9);
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.CellFormatting       += Grid_CellFormatting;

            var cHid = new DataGridViewTextBoxColumn(); cHid.Name = "ColId";       cHid.HeaderText = "ID";       cHid.Visible = false; grid.Columns.Add(cHid);
            var cHid2= new DataGridViewTextBoxColumn(); cHid2.Name= "ColIdUsr";    cHid2.HeaderText= "IdUsr";    cHid2.Visible= false; grid.Columns.Add(cHid2);
            var cHid3= new DataGridViewTextBoxColumn(); cHid3.Name= "ColHash";     cHid3.HeaderText= "Hash";     cHid3.Visible= false; grid.Columns.Add(cHid3);

            var c1 = new DataGridViewTextBoxColumn(); c1.Name = "ColFecha";   c1.HeaderText = "Fecha y hora";  c1.FillWeight = 18; grid.Columns.Add(c1);
            var c2 = new DataGridViewTextBoxColumn(); c2.Name = "ColUsuario"; c2.HeaderText = "Usuario";       c2.FillWeight = 18; grid.Columns.Add(c2);
            var c3 = new DataGridViewTextBoxColumn(); c3.Name = "ColOp";      c3.HeaderText = "Operador";      c3.FillWeight = 18; grid.Columns.Add(c3);
            var c4 = new DataGridViewTextBoxColumn(); c4.Name = "ColEvento";  c4.HeaderText = "Evento";        c4.FillWeight = 16; grid.Columns.Add(c4);
            var c5 = new DataGridViewTextBoxColumn(); c5.Name = "ColClave";   c5.HeaderText = "Clave (hash)";  c5.FillWeight = 30; grid.Columns.Add(c5);

            lblTotal           = new Label();
            lblTotal.Location  = new Point(16, 524);
            lblTotal.Size      = new Size(550, 20);
            lblTotal.Anchor    = AnchorStyles.Bottom | AnchorStyles.Left;
            lblTotal.ForeColor = Color.FromArgb(80, 80, 80);

            btnRestaurar        = CrearBoton("Restaurar version", new Point(670, 520), 160, Color.FromArgb(46, 125, 50));
            btnRestaurar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRestaurar.Click += BtnRestaurar_Click;

            btnCerrar        = CrearBoton("Cerrar", new Point(840, 520), 100, Color.FromArgb(180, 40, 40));
            btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblAviso);
            this.Controls.Add(lblFiltroUsuario); this.Controls.Add(cmbUsuario);
            this.Controls.Add(lblFiltroEvento);  this.Controls.Add(cmbEvento);
            this.Controls.Add(lblDesde);         this.Controls.Add(dtpDesde);
            this.Controls.Add(lblHasta);         this.Controls.Add(dtpHasta);
            this.Controls.Add(btnFiltrar);       this.Controls.Add(btnLimpiar);
            this.Controls.Add(grid);
            this.Controls.Add(lblTotal);
            this.Controls.Add(btnRestaurar);
            this.Controls.Add(btnCerrar);
        }

        private static Button CrearBoton(string texto, Point loc, int ancho, Color color)
        {
            var btn = new Button();
            btn.Text      = texto;
            btn.Location  = loc;
            btn.Size      = new Size(ancho, 28);
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void CargarDatos()
        {
            _todos = _dal.ObtenerHistorialClaves();

            var usuarios = new System.Collections.Generic.HashSet<string>();
            usuarios.Add("Todos");
            foreach (var r in _todos)
                usuarios.Add(r.NombreUsuario);

            cmbUsuario.Items.Clear();
            foreach (var u in usuarios)
                cmbUsuario.Items.Add(u);
            cmbUsuario.SelectedIndex = 0;

            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            string usuario = cmbUsuario.SelectedIndex > 0 ? cmbUsuario.SelectedItem.ToString() : "";
            string evento  = cmbEvento.SelectedIndex  > 0 ? cmbEvento.SelectedItem.ToString()  : "";
            DateTime desde = dtpDesde.Value.Date;
            DateTime hasta = dtpHasta.Value.Date.AddDays(1).AddSeconds(-1);

            grid.Rows.Clear();
            int count = 0;

            foreach (var r in _todos)
            {
                if (r.Fecha < desde || r.Fecha > hasta) continue;
                if (!string.IsNullOrEmpty(usuario) &&
                    !r.NombreUsuario.Equals(usuario, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(evento) && r.Evento != evento) continue;

                grid.Rows.Add(
                    r.Id,
                    r.IdUsuario,
                    r.ClaveHash,
                    r.Fecha.ToString("dd/MM/yyyy HH:mm:ss"),
                    r.NombreUsuario,
                    r.Operador,
                    r.Evento,
                    r.ClaveHash.Substring(0, 16) + "..."
                );
                count++;
            }

            lblTotal.Text = "Mostrando " + count + " de " + _todos.Count + " registros.";
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            cmbUsuario.SelectedIndex = 0;
            cmbEvento.SelectedIndex  = 0;
            dtpDesde.Value           = DateTime.Today.AddMonths(-3);
            dtpHasta.Value           = DateTime.Today;
            AplicarFiltro();
        }

        private void BtnRestaurar_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione una fila del historial para restaurar.",
                    "Atencion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var fila = grid.SelectedRows[0];
            int idRegistro = (int)fila.Cells["ColId"].Value;

            RegistroClave registro = _todos.Find(r => r.Id == idRegistro);
            if (registro == null)
            {
                MessageBox.Show("No se encontro el registro seleccionado.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int idUsuario      = registro.IdUsuario;
            string nomUsuario  = registro.NombreUsuario;
            string claveHash   = registro.ClaveHash;
            DateTime fecha     = registro.Fecha;
            string fechaStr    = fecha.ToString("dd/MM/yyyy HH:mm:ss");

            bool hayVersionPosterior = _todos.Exists(r => r.IdUsuario == idUsuario && r.Fecha > fecha);

            if (!hayVersionPosterior)
            {
                MessageBox.Show(
                    "No se puede restaurar esta version porque es la mas reciente o la unica registrada para este usuario.",
                    "Restauracion no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conf = MessageBox.Show(
                "Restaurar la clave de '" + nomUsuario + "' al estado del " + fechaStr + "?\n\n" +
                "Esta accion reemplazara la clave actual y quedara registrada en el historial.",
                "Confirmar restauracion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (conf != DialogResult.Yes) return;

            string operador = _sesion.ObtenerUsuarioActual() != null
                ? _sesion.ObtenerUsuarioActual().NombreUsuario
                : "admin";

            bool ok = _dal.RestaurarClave(idUsuario, nomUsuario, claveHash, operador);

            if (ok)
            {
                _log.Info("CLAVE", "'" + operador + "' restauro la clave de '" + nomUsuario + "' al estado del " + fechaStr + ".");
                MessageBox.Show("Clave restaurada correctamente.", "Exito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarDatos();
            }
            else
            {
                MessageBox.Show("No se pudo restaurar la clave.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.RowCount) return;
            DataGridViewRow fila = grid.Rows[e.RowIndex];
            if (fila.IsNewRow) return;
            DataGridViewCell cell = fila.Cells["ColEvento"];
            if (cell == null || cell.Value == null) return;

            string ev = cell.Value.ToString();
            if (ev == "ALTA")
            {
                e.CellStyle.BackColor = Color.FromArgb(230, 255, 230);
                e.CellStyle.ForeColor = Color.FromArgb(0, 120, 0);
            }
            else if (ev == "MODIFICACION")
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 250, 220);
                e.CellStyle.ForeColor = Color.FromArgb(150, 100, 0);
            }
            else if (ev == "RESTAURACION")
            {
                e.CellStyle.BackColor = Color.FromArgb(225, 235, 255);
                e.CellStyle.ForeColor = Color.FromArgb(30, 60, 160);
            }
            else
            {
                e.CellStyle.BackColor = Color.White;
                e.CellStyle.ForeColor = Color.Black;
            }
        }
    }
}
