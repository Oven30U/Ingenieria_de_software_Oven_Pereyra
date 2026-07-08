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

        private Label         lblAlertaManipulacion;

        public FormIdiomas()
        {
            ConstruirUI();
            _gestor.Suscribir(this);
            CargarGrillaIdiomas();
        }

        private void ConstruirUI()
        {
            this.Text          = "Gestión de Idiomas";
            this.Size          = new Size(780, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize   = new Size(700, 580);
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
            gridIdiomas.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColCodigo", HeaderText = "Código", Width = 80 });

            lblNuevoIdioma = new Label
            {
                Text      = "Nuevo idioma:",
                Location  = new Point(20, 292),
                Size      = new Size(110, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtNuevoIdioma = new TextBox
            {
                Location = new Point(20, 314),
                Size     = new Size(220, 24)
            };

            btnAgregar   = CrearBoton("Agregar idioma", new Point(250, 313), 130, Color.FromArgb(46, 125, 50));
            btnRenombrar = CrearBoton("Renombrar",      new Point(20,  355), 130, Color.FromArgb(25, 118, 210));
            btnEliminar  = CrearBoton("Eliminar",       new Point(165, 355), 130, Color.FromArgb(198, 40, 40));

            btnAgregar.Click   += BtnAgregar_Click;
            btnRenombrar.Click += BtnRenombrar_Click;
            btnEliminar.Click  += BtnEliminar_Click;

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

            lblAlertaManipulacion = new Label
            {
                Text      = "",
                Location  = new Point(20, 400),
                Size      = new Size(360, 60),
                ForeColor = Color.FromArgb(180, 30, 30),
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                Visible   = false
            };

            btnCerrar        = CrearBoton("Cerrar", new Point(640, 560), 110, Color.FromArgb(80, 80, 80));
            btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                lblTitulo, gridIdiomas,
                lblNuevoIdioma, txtNuevoIdioma, btnAgregar,
                btnRenombrar, btnEliminar,
                grpTraducciones, lblAlertaManipulacion, btnCerrar
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

        private void CargarGrillaIdiomas()
        {
            gridIdiomas.Rows.Clear();
            bool huboManipulacion = false;

            foreach (var id in _gestor.ObtenerIdiomas())
            {
                int fila = gridIdiomas.Rows.Add(id.Id, id.Nombre, id.Codigo);

                bool codigoOk = !string.IsNullOrEmpty(id.Codigo) && DigitoVerificador.Validar(id.Codigo);
                if (!codigoOk)
                {
                    gridIdiomas.Rows[fila].DefaultCellStyle.BackColor = Color.FromArgb(255, 205, 205);
                    gridIdiomas.Rows[fila].DefaultCellStyle.ForeColor = Color.FromArgb(140, 0, 0);
                    huboManipulacion = true;
                    Bitacora.Instancia.Error("IDIOMA",
                        "El codigo del idioma '" + id.Nombre + "' (Id " + id.Id + ") no coincide con su digito verificador. Posible manipulacion directa de la base de datos.");
                }
            }

            lblAlertaManipulacion.Visible = huboManipulacion;
            lblAlertaManipulacion.Text = huboManipulacion
                ? "⚠ Se detectó un código de idioma alterado directamente en la base\n(fila marcada en rojo). Quedó registrado en la Bitácora."
                : "";

            gridTraducciones.Rows.Clear();
        }

        private Idioma IdiomaSeleccionado()
        {
            if (gridIdiomas.SelectedRows.Count == 0) return null;
            var fila = gridIdiomas.SelectedRows[0];
            return new Idioma
            {
                Id     = (int)fila.Cells["ColId"].Value,
                Nombre = fila.Cells["ColNombre"].Value.ToString(),
                Codigo = fila.Cells["ColCodigo"].Value != null ? fila.Cells["ColCodigo"].Value.ToString() : null
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
