using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using Mapper;

namespace UI
{
    public class FormIdiomas : Form, IObservadorIdioma
    {
        private readonly GestorIdioma _gestor = GestorIdioma.Instancia;

        private Label        lblTitulo;
        private DataGridView gridIdiomas;
        private Label        lblNuevoIdioma;
        private TextBox      txtNuevoIdioma;
        private Button       btnAgregar;
        private Button       btnRenombrar;
        private Button       btnEliminar;
        private GroupBox     grpTraducciones;
        private DataGridView gridTraducciones;
        private Button       btnGuardarTrad;
        private Button       btnCerrar;

        public FormIdiomas()
        {
            ConstruirUI();
            _gestor.Suscribir(this);
            CargarGrillaIdiomas();
        }

        private void ConstruirUI()
        {
            this.Text          = "Gestión de Idiomas";
            this.Size          = new Size(780, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize   = new Size(700, 520);
            this.Font          = new Font("Segoe UI", 9);
            this.BackColor     = Color.FromArgb(245, 246, 250);

            lblTitulo = new Label
            {
                Text      = "Gestión de Idiomas",
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 70, 130),
                Location  = new Point(20, 12),
                AutoSize  = true
            };

            // ── Grilla de idiomas ──────────────────────────────────────
            gridIdiomas = new DataGridView
            {
                Location              = new Point(20, 55),
                Size                  = new Size(360, 200),
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            gridIdiomas.SelectionChanged += GridIdiomas_SelectionChanged;
            gridIdiomas.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColId",     HeaderText = "ID",     Width = 50 });
            gridIdiomas.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColNombre", HeaderText = "Idioma" });

            // ── Nuevo idioma ───────────────────────────────────────────
            lblNuevoIdioma = new Label
            {
                Text      = "Nuevo idioma:",
                Location  = new Point(20, 268),
                Size      = new Size(110, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtNuevoIdioma = new TextBox
            {
                Location = new Point(135, 266),
                Size     = new Size(180, 24)
            };

            btnAgregar   = CrearBoton("Agregar idioma", new Point(325, 265), 130, Color.FromArgb(46, 125, 50));
            btnRenombrar = CrearBoton("Renombrar",      new Point(20,  305), 130, Color.FromArgb(25, 118, 210));
            btnEliminar  = CrearBoton("Eliminar",       new Point(165, 305), 130, Color.FromArgb(198, 40, 40));

            btnAgregar.Click   += BtnAgregar_Click;
            btnRenombrar.Click += BtnRenombrar_Click;
            btnEliminar.Click  += BtnEliminar_Click;

            // ── GroupBox Traducciones ──────────────────────────────────
            grpTraducciones = new GroupBox
            {
                Text      = "Traducciones del idioma seleccionado",
                Location  = new Point(400, 55),
                Size      = new Size(355, 290),
                ForeColor = Color.FromArgb(40, 70, 130),
                Font      = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            gridTraducciones = new DataGridView
            {
                Location              = new Point(10, 22),
                Size                  = new Size(335, 225),
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.CellSelect,
                MultiSelect           = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None
            };
            gridTraducciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColTag",  HeaderText = "Tag (clave)", ReadOnly = true });
            gridTraducciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColTrad", HeaderText = "Traducción" });

            btnGuardarTrad = CrearBoton("Guardar traducciones", new Point(10, 254), 200, Color.FromArgb(46, 125, 50));
            btnGuardarTrad.Click += BtnGuardarTrad_Click;

            grpTraducciones.Controls.Add(gridTraducciones);
            grpTraducciones.Controls.Add(btnGuardarTrad);

            // ── Cerrar ─────────────────────────────────────────────────
            btnCerrar        = CrearBoton("Cerrar", new Point(640, 500), 110, Color.FromArgb(80, 80, 80));
            btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                lblTitulo, gridIdiomas,
                lblNuevoIdioma, txtNuevoIdioma, btnAgregar,
                btnRenombrar, btnEliminar,
                grpTraducciones, btnCerrar
            });
        }

        private static Button CrearBoton(string texto, Point loc, int ancho, Color color)
        {
            return new Button
            {
                Text      = texto,
                Location  = loc,
                Size      = new Size(ancho, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
        }

        // ── Lógica ────────────────────────────────────────────────────

        private void CargarGrillaIdiomas()
        {
            gridIdiomas.Rows.Clear();
            foreach (var id in _gestor.ObtenerIdiomas())
                gridIdiomas.Rows.Add(id.Id, id.Nombre);
            gridTraducciones.Rows.Clear();
        }

        private Idioma IdiomaSeleccionado()
        {
            if (gridIdiomas.SelectedRows.Count == 0) return null;
            var fila = gridIdiomas.SelectedRows[0];
            return new Idioma
            {
                Id     = (int)fila.Cells["ColId"].Value,
                Nombre = fila.Cells["ColNombre"].Value.ToString()
            };
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            string nombre = txtNuevoIdioma.Text.Trim();
            string msg;
            bool ok = _gestor.AgregarIdioma(nombre, out msg);
            MessageBox.Show(msg, ok ? "Éxito" : "Error",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (ok) { txtNuevoIdioma.Clear(); CargarGrillaIdiomas(); }
        }

        private void BtnRenombrar_Click(object sender, EventArgs e)
        {
            var sel = IdiomaSeleccionado();
            if (sel == null) { Aviso("Seleccione un idioma de la lista."); return; }

            string nuevo = PedirTexto($"Nuevo nombre para '{sel.Nombre}':", sel.Nombre);
            if (string.IsNullOrWhiteSpace(nuevo)) return;

            string msg;
            bool ok = _gestor.RenombrarIdioma(sel.Id, nuevo, out msg);
            MessageBox.Show(msg, ok ? "Éxito" : "Error",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (ok) CargarGrillaIdiomas();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var sel = IdiomaSeleccionado();
            if (sel == null) { Aviso("Seleccione un idioma de la lista."); return; }

            var conf = MessageBox.Show(
                $"¿Eliminar el idioma '{sel.Nombre}' y todas sus traducciones?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (conf != DialogResult.Yes) return;

            string msg;
            bool ok = _gestor.EliminarIdioma(sel.Id, out msg);
            MessageBox.Show(msg, ok ? "Éxito" : "Error",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (ok) CargarGrillaIdiomas();
        }

        private void GridIdiomas_SelectionChanged(object sender, EventArgs e)
        {
            var sel = IdiomaSeleccionado();
            if (sel == null) { gridTraducciones.Rows.Clear(); return; }

            gridTraducciones.Rows.Clear();
            var trad = _gestor.ObtenerIdiomaDAL().ObtenerTraducciones(sel.Id);
            foreach (var kv in trad)
                gridTraducciones.Rows.Add(kv.Key, kv.Value);

            grpTraducciones.Text = $"Traducciones — {sel.Nombre}";
        }

        private void BtnGuardarTrad_Click(object sender, EventArgs e)
        {
            var sel = IdiomaSeleccionado();
            if (sel == null) { Aviso("Seleccione un idioma de la lista."); return; }

            int errores = 0;
            foreach (DataGridViewRow fila in gridTraducciones.Rows)
            {
                if (fila.IsNewRow) continue;
                string tag  = fila.Cells["ColTag"].Value?.ToString()  ?? "";
                string text = fila.Cells["ColTrad"].Value?.ToString() ?? "";
                if (!_gestor.GuardarTraduccion(sel.Id, tag, text)) errores++;
            }

            if (errores == 0)
                MessageBox.Show("Traducciones guardadas correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show($"Se produjeron {errores} errores al guardar.", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (_gestor.IdiomaActual != null)
                _gestor.CambiarIdioma(_gestor.IdiomaActual.Id);
        }

        // ── Helper: InputBox propio (reemplaza Microsoft.VisualBasic) ─

        private static string PedirTexto(string pregunta, string valorInicial = "")
        {
            string resultado = null;
            using (var dlg = new Form())
            {
                dlg.Text          = "Ingrese valor";
                dlg.Size          = new Size(360, 140);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox   = false;
                dlg.MinimizeBox   = false;

                var lbl = new Label  { Text = pregunta, Location = new Point(12, 12), Size = new Size(320, 20) };
                var txt = new TextBox { Text = valorInicial, Location = new Point(12, 36), Size = new Size(320, 22) };
                var btnOk     = new Button { Text = "Aceptar", DialogResult = DialogResult.OK,     Location = new Point(170, 68), Size = new Size(80, 26) };
                var btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(255, 68), Size = new Size(80, 26) };

                dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog() == DialogResult.OK)
                    resultado = txt.Text.Trim();
            }
            return resultado;
        }

        private static void Aviso(string msg) =>
            MessageBox.Show(msg, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // ── Observer ───────────────────────────────────────────────────

        public void ActualizarIdioma()
        {
            if (_gestor.IdiomaActual != null)
                this.Text = $"Gestión de Idiomas — activo: {_gestor.IdiomaActual.Nombre}";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _gestor.Desuscribir(this);
            base.OnFormClosed(e);
        }
    }
}
