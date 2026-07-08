using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UI
{
    public class FormBitacora : Form
    {
        private Label        lblTitulo;
        private Label        lblBuscar;
        private TextBox      txtBuscar;
        private Label        lblUsuario;
        private ComboBox     cmbUsuario;
        private Label        lblNivel;
        private ComboBox     cmbNivel;
        private Label        lblFechaDesde;
        private DateTimePicker dtpDesde;
        private Label        lblFechaHasta;
        private DateTimePicker dtpHasta;
        private Button       btnFiltrar;
        private Button       btnLimpiar;
        private Button       btnExportar;
        private Button       btnCerrar;
        private DataGridView grid;
        private Label        lblTotal;

        private List<EntradaLog> _todos = new List<EntradaLog>();

        public FormBitacora()
        {
            ConstruirUI();
            CargarLogs();
        }

        private class EntradaLog
        {
            public string Fecha     { get; set; }
            public string Hora      { get; set; }
            public string Usuario   { get; set; }
            public string Nivel     { get; set; }
            public string Categoria { get; set; }
            public string Mensaje   { get; set; }
            public DateTime FechaHora { get; set; }
        }

        private void ConstruirUI()
        {
            this.Text            = "Bitacora del Sistema";
            this.ClientSize      = new Size(1050, 620);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize     = new Size(900, 520);
            this.Font            = new Font("Segoe UI", 9);
            this.BackColor       = Color.FromArgb(245, 246, 250);

            lblTitulo           = new Label();
            lblTitulo.Text      = "Bitacora del Sistema";
            lblTitulo.Font      = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(40, 70, 130);
            lblTitulo.Location  = new Point(16, 12);
            lblTitulo.AutoSize  = true;

            lblBuscar          = new Label();
            lblBuscar.Text     = "Buscar:";
            lblBuscar.Location = new Point(16, 52);
            lblBuscar.Size     = new Size(50, 24);
            lblBuscar.TextAlign = ContentAlignment.MiddleLeft;

            txtBuscar          = new TextBox();
            txtBuscar.Location = new Point(68, 50);
            txtBuscar.Size     = new Size(200, 24);
            txtBuscar.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) AplicarFiltro(); };

            lblUsuario          = new Label();
            lblUsuario.Text     = "Usuario:";
            lblUsuario.Location = new Point(278, 52);
            lblUsuario.Size     = new Size(55, 24);
            lblUsuario.TextAlign = ContentAlignment.MiddleLeft;

            cmbUsuario = new ComboBox();
            cmbUsuario.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUsuario.Location      = new Point(335, 50);
            cmbUsuario.Size          = new Size(150, 24);

            lblNivel          = new Label();
            lblNivel.Text     = "Nivel:";
            lblNivel.Location = new Point(495, 52);
            lblNivel.Size     = new Size(40, 24);
            lblNivel.TextAlign = ContentAlignment.MiddleLeft;

            cmbNivel = new ComboBox();
            cmbNivel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNivel.Location      = new Point(537, 50);
            cmbNivel.Size          = new Size(90, 24);
            cmbNivel.Items.AddRange(new object[] { "Todos", "INFO", "WARN", "ERROR" });
            cmbNivel.SelectedIndex = 0;

            lblFechaDesde          = new Label();
            lblFechaDesde.Text     = "Desde:";
            lblFechaDesde.Location = new Point(16, 84);
            lblFechaDesde.Size     = new Size(50, 24);
            lblFechaDesde.TextAlign = ContentAlignment.MiddleLeft;

            dtpDesde          = new DateTimePicker();
            dtpDesde.Format   = DateTimePickerFormat.Short;
            dtpDesde.Location = new Point(68, 82);
            dtpDesde.Size     = new Size(120, 24);
            dtpDesde.Value    = DateTime.Today.AddMonths(-1);

            lblFechaHasta          = new Label();
            lblFechaHasta.Text     = "Hasta:";
            lblFechaHasta.Location = new Point(200, 84);
            lblFechaHasta.Size     = new Size(50, 24);
            lblFechaHasta.TextAlign = ContentAlignment.MiddleLeft;

            dtpHasta          = new DateTimePicker();
            dtpHasta.Format   = DateTimePickerFormat.Short;
            dtpHasta.Location = new Point(252, 82);
            dtpHasta.Size     = new Size(120, 24);
            dtpHasta.Value    = DateTime.Today;

            btnFiltrar   = CrearBoton("Filtrar",   new Point(390, 82), 80,  Color.FromArgb(25, 118, 210));
            btnLimpiar   = CrearBoton("Limpiar",   new Point(478, 82), 80,  Color.FromArgb(100, 100, 100));
            btnExportar  = CrearBoton("Exportar",  new Point(566, 82), 90,  Color.FromArgb(46, 125, 50));
            btnCerrar    = CrearBoton("Cerrar",    new Point(930, 578), 100, Color.FromArgb(180, 40, 40));

            btnFiltrar.Click  += (s, e) => AplicarFiltro();
            btnLimpiar.Click  += BtnLimpiar_Click;
            btnExportar.Click += BtnExportar_Click;
            btnCerrar.Click   += (s, e) => this.Close();
            btnCerrar.Anchor   = AnchorStyles.Bottom | AnchorStyles.Right;

            grid                       = new DataGridView();
            grid.Location              = new Point(16, 118);
            grid.Size                  = new Size(1018, 450);
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
            grid.Font                  = new Font("Consolas", 8.5f);
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.CellFormatting       += Grid_CellFormatting;

            var c1 = new DataGridViewTextBoxColumn(); c1.Name = "ColFecha";     c1.HeaderText = "Fecha";     c1.FillWeight = 10; grid.Columns.Add(c1);
            var c2 = new DataGridViewTextBoxColumn(); c2.Name = "ColHora";      c2.HeaderText = "Hora";      c2.FillWeight = 8;  grid.Columns.Add(c2);
            var c3 = new DataGridViewTextBoxColumn(); c3.Name = "ColUsuario";   c3.HeaderText = "Usuario";   c3.FillWeight = 14; grid.Columns.Add(c3);
            var c4 = new DataGridViewTextBoxColumn(); c4.Name = "ColNivel";     c4.HeaderText = "Nivel";     c4.FillWeight = 7;  grid.Columns.Add(c4);
            var c5 = new DataGridViewTextBoxColumn(); c5.Name = "ColCategoria"; c5.HeaderText = "Categoria"; c5.FillWeight = 11; grid.Columns.Add(c5);
            var c6 = new DataGridViewTextBoxColumn(); c6.Name = "ColMensaje";   c6.HeaderText = "Mensaje";   c6.FillWeight = 50; grid.Columns.Add(c6);

            lblTotal          = new Label();
            lblTotal.Location = new Point(16, 576);
            lblTotal.Size     = new Size(500, 20);
            lblTotal.Anchor   = AnchorStyles.Bottom | AnchorStyles.Left;
            lblTotal.ForeColor = Color.FromArgb(80, 80, 80);

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblBuscar);    this.Controls.Add(txtBuscar);
            this.Controls.Add(lblUsuario);   this.Controls.Add(cmbUsuario);
            this.Controls.Add(lblNivel);     this.Controls.Add(cmbNivel);
            this.Controls.Add(lblFechaDesde); this.Controls.Add(dtpDesde);
            this.Controls.Add(lblFechaHasta); this.Controls.Add(dtpHasta);
            this.Controls.Add(btnFiltrar);   this.Controls.Add(btnLimpiar);
            this.Controls.Add(btnExportar);  this.Controls.Add(btnCerrar);
            this.Controls.Add(grid);
            this.Controls.Add(lblTotal);
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

        private void CargarLogs()
        {
            _todos.Clear();
            string carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(carpeta))
            {
                lblTotal.Text = "No se encontro la carpeta de logs.";
                return;
            }

            string[] archivos = Directory.GetFiles(carpeta, "*.log");
            Array.Sort(archivos);

            var usuarios = new System.Collections.Generic.HashSet<string>();
            usuarios.Add("Todos");

            foreach (string archivo in archivos)
            {
                try
                {
                    string[] lineas = File.ReadAllLines(archivo);
                    foreach (string linea in lineas)
                    {
                        var e = ParsearLinea(linea);
                        if (e != null)
                        {
                            _todos.Add(e);
                            if (!string.IsNullOrEmpty(e.Usuario))
                                usuarios.Add(e.Usuario.Trim());
                        }
                    }
                }
                catch { }
            }

            cmbUsuario.Items.Clear();
            foreach (var u in usuarios)
                cmbUsuario.Items.Add(u);
            cmbUsuario.SelectedIndex = 0;

            AplicarFiltro();
        }

        private static EntradaLog ParsearLinea(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea) || !linea.StartsWith("[")) return null;
            try
            {
                int i = 0;
                string fecha    = ExtraerCampo(linea, ref i);
                string hora     = ExtraerCampo(linea, ref i);
                string usuario  = ExtraerCampo(linea, ref i);
                string nivel    = ExtraerCampo(linea, ref i);
                string cat      = ExtraerCampo(linea, ref i);
                string mensaje  = i < linea.Length ? linea.Substring(i).Trim() : "";

                if (string.IsNullOrEmpty(nivel)) return null;

                DateTime fh = DateTime.MinValue;
                DateTime.TryParse(fecha + " " + hora, out fh);

                return new EntradaLog
                {
                    Fecha     = fecha,
                    Hora      = hora,
                    Usuario   = usuario.Trim(),
                    Nivel     = nivel.Trim(),
                    Categoria = cat.Trim(),
                    Mensaje   = mensaje,
                    FechaHora = fh
                };
            }
            catch { return null; }
        }

        private static string ExtraerCampo(string linea, ref int pos)
        {
            int inicio = linea.IndexOf('[', pos);
            if (inicio < 0) return "";
            int fin = linea.IndexOf(']', inicio);
            if (fin < 0) return "";
            pos = fin + 1;
            return linea.Substring(inicio + 1, fin - inicio - 1);
        }

        private void AplicarFiltro()
        {
            string buscar  = txtBuscar.Text.Trim().ToLower();
            string usuario = cmbUsuario.SelectedIndex > 0 ? cmbUsuario.SelectedItem.ToString().Trim() : "";
            string nivel   = cmbNivel.SelectedIndex  > 0 ? cmbNivel.SelectedItem.ToString()           : "";
            DateTime desde = dtpDesde.Value.Date;
            DateTime hasta = dtpHasta.Value.Date.AddDays(1).AddSeconds(-1);

            grid.Rows.Clear();
            int count = 0;

            foreach (var e in _todos)
            {

                if (e.FechaHora != DateTime.MinValue)
                {
                    if (e.FechaHora < desde || e.FechaHora > hasta) continue;
                }

                if (!string.IsNullOrEmpty(usuario) &&
                    !e.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase)) continue;

                if (!string.IsNullOrEmpty(nivel) && e.Nivel != nivel) continue;

                if (!string.IsNullOrEmpty(buscar))
                {
                    bool match = e.Mensaje.ToLower().Contains(buscar)
                              || e.Categoria.ToLower().Contains(buscar)
                              || e.Usuario.ToLower().Contains(buscar);
                    if (!match) continue;
                }

                grid.Rows.Add(e.Fecha, e.Hora, e.Usuario, e.Nivel, e.Categoria, e.Mensaje);
                count++;
            }

            lblTotal.Text = "Mostrando " + count + " de " + _todos.Count + " registros.";
            if (grid.Rows.Count > 0)
                grid.FirstDisplayedScrollingRowIndex = grid.Rows.Count - 1;
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            cmbUsuario.SelectedIndex = 0;
            cmbNivel.SelectedIndex   = 0;
            dtpDesde.Value           = DateTime.Today.AddMonths(-1);
            dtpHasta.Value           = DateTime.Today;
            AplicarFiltro();
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
            var fila = grid.Rows[e.RowIndex];
            var nivelVal = fila.Cells["ColNivel"].Value;
            if (nivelVal == null) return;
            string nivel = nivelVal.ToString().Trim();
            if (nivel == "ERROR")
            {
                fila.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                fila.DefaultCellStyle.ForeColor = Color.FromArgb(180, 0, 0);
            }
            else if (nivel == "WARN")
            {
                fila.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 220);
                fila.DefaultCellStyle.ForeColor = Color.FromArgb(150, 100, 0);
            }
            else
            {
                fila.DefaultCellStyle.BackColor = Color.White;
                fila.DefaultCellStyle.ForeColor = Color.Black;
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            var dlg      = new SaveFileDialog();
            dlg.Title    = "Exportar bitacora";
            dlg.Filter   = "Archivo de texto (*.txt)|*.txt";
            dlg.FileName = "bitacora_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                var lineas = new List<string>();
                lineas.Add("BITACORA EXPORTADA - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                lineas.Add(new string('=', 100));
                lineas.Add(string.Format("{0,-12} {1,-10} {2,-16} {3,-7} {4,-14} {5}",
                    "Fecha", "Hora", "Usuario", "Nivel", "Categoria", "Mensaje"));
                lineas.Add(new string('-', 100));

                foreach (DataGridViewRow fila in grid.Rows)
                {
                    if (fila.IsNewRow) continue;
                    string fecha = fila.Cells["ColFecha"].Value     != null ? fila.Cells["ColFecha"].Value.ToString()     : "";
                    string hora  = fila.Cells["ColHora"].Value      != null ? fila.Cells["ColHora"].Value.ToString()      : "";
                    string usr   = fila.Cells["ColUsuario"].Value   != null ? fila.Cells["ColUsuario"].Value.ToString()   : "";
                    string niv   = fila.Cells["ColNivel"].Value     != null ? fila.Cells["ColNivel"].Value.ToString()     : "";
                    string cat   = fila.Cells["ColCategoria"].Value != null ? fila.Cells["ColCategoria"].Value.ToString() : "";
                    string msg   = fila.Cells["ColMensaje"].Value   != null ? fila.Cells["ColMensaje"].Value.ToString()   : "";
                    lineas.Add(string.Format("{0,-12} {1,-10} {2,-16} {3,-7} {4,-14} {5}",
                        fecha, hora, usr, niv, cat, msg));
                }

                File.WriteAllLines(dlg.FileName, lineas.ToArray());
                MessageBox.Show("Exportado correctamente.", "Exito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
