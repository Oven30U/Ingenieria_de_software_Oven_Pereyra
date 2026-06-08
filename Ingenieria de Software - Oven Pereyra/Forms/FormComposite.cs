using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BLL;
using Mapper;

namespace UI
{
    public partial class FormComposite : Form
    {
        private UsuarioService service = new UsuarioService();

        private TreeView treeArbol;
        private TreeView treePreview;

        private Label lblAgregar;
        private ListBox lstFamilias;
        private ListBox lstParientes;
        private ListBox lstSeleccionados;
        private TextBox txtNueva;

        private Button btnAgregar;
        private Button btnAgregarFamilia;
        private Button btnAgregarPariente;
        private Button btnEnlazarPariente;
        private Button btnEliminarSeleccionado;
        private Button btnGuardarFamilia;

        public FormComposite()
        {
            InitializeComponent();
            ConstruirUI();
            RefrescarArbol();
            RefrescarListas();
        }

        private void ConstruirUI()
        {
            this.Text = "Administrar Composite";
            this.Size = new System.Drawing.Size(1000, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);

            // ─── PANEL IZQUIERDO: arbol actual ───────────────────────────────
            var panelIzq = new Panel
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(240, 540),
                BackColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            treeArbol = new TreeView
            {
                Location = new System.Drawing.Point(2, 2),
                Size = new System.Drawing.Size(234, 534),
                BorderStyle = BorderStyle.None
            };
            panelIzq.Controls.Add(treeArbol);

            // ─── PANEL DERECHO ───────────────────────────────────────────────
            var panelDer = new Panel
            {
                Location = new System.Drawing.Point(260, 10),
                Size = new System.Drawing.Size(710, 540),
                BackColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Campo "Nueva familia"
            var lblNueva = new Label
            {
                Text = "Nueva familia",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(120, 20),
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            txtNueva = new TextBox
            {
                Location = new System.Drawing.Point(10, 32),
                Size = new System.Drawing.Size(300, 22),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            txtNueva.TextChanged += (s, e) => ActualizarPreview();

            // Boton Agregar header
            btnAgregar = new Button
            {
                Location = new System.Drawing.Point(10, 60),
                Size = new System.Drawing.Size(680, 26),
                Text = "Agregar",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(200, 215, 230),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
            };
            btnAgregar.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            btnAgregar.Click += BtnAgregar_Click;

            // Lista 1: Familias (grupos)
            lstFamilias = new ListBox
            {
                Location = new System.Drawing.Point(10, 91),
                Size = new System.Drawing.Size(190, 385),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 9),
                SelectionMode = SelectionMode.One
            };
            lstFamilias.SelectedIndexChanged += (s, e) => ActualizarPreview();
            lstFamilias.MouseDown += ListBox_MouseDown;

            // Lista 2: Parientes (hojas)
            lstParientes = new ListBox
            {
                Location = new System.Drawing.Point(210, 91),
                Size = new System.Drawing.Size(230, 385),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 9),
                SelectionMode = SelectionMode.One
            };
            lstParientes.SelectedIndexChanged += (s, e) => ActualizarPreview();
            lstParientes.MouseDown += ListBox_MouseDown;

            // Lista 3: previsualizar eliminacion
            lstSeleccionados = new ListBox
            {
                Location = new System.Drawing.Point(450, 91),
                Size = new System.Drawing.Size(120, 385),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 9),
                SelectionMode = SelectionMode.None
            };

            // Columna 4: TreeView preview
            var lblPreview = new Label
            {
                Text = "Previsualización",
                Location = new System.Drawing.Point(580, 72),
                Size = new System.Drawing.Size(120, 16),
                Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(60, 60, 60)
            };
            treePreview = new TreeView
            {
                Location = new System.Drawing.Point(580, 91),
                Size = new System.Drawing.Size(120, 385),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 8)
            };

            // Botones inferiores
            btnAgregarFamilia = new Button
            {
                Location = new System.Drawing.Point(10, 482),
                Size = new System.Drawing.Size(190, 28),
                Text = "Agregar Familia",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(200, 215, 230),
                Font = new System.Drawing.Font("Segoe UI", 8)
            };
            btnAgregarFamilia.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            btnAgregarFamilia.Click += BtnAgregarFamilia_Click;

            btnAgregarPariente = new Button
            {
                Location = new System.Drawing.Point(210, 482),
                Size = new System.Drawing.Size(230, 28),
                Text = "Agregar Pariente",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(200, 215, 230),
                Font = new System.Drawing.Font("Segoe UI", 8)
            };
            btnAgregarPariente.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            btnAgregarPariente.Click += BtnAgregarPariente_Click;

            btnEnlazarPariente = new Button
            {
                Location = new System.Drawing.Point(210, 514),
                Size = new System.Drawing.Size(230, 28),
                Text = "Enlazar Pariente",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(200, 215, 230),
                Font = new System.Drawing.Font("Segoe UI", 8)
            };
            btnEnlazarPariente.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            btnEnlazarPariente.Click += BtnEnlazarPariente_Click;

            btnEliminarSeleccionado = new Button
            {
                Location = new System.Drawing.Point(450, 514),
                Size = new System.Drawing.Size(120, 28),
                Text = "Eliminar seleccionado",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(200, 215, 230),
                Font = new System.Drawing.Font("Segoe UI", 8)
            };
            btnEliminarSeleccionado.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            btnEliminarSeleccionado.Click += BtnEliminarSeleccionado_Click;

            btnGuardarFamilia = new Button
            {
                Location = new System.Drawing.Point(450, 546),
                Size = new System.Drawing.Size(120, 28),
                Text = "Guardar Familia",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(200, 215, 230),
                Font = new System.Drawing.Font("Segoe UI", 8)
            };
            btnGuardarFamilia.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            btnGuardarFamilia.Click += BtnGuardarFamilia_Click;

            panelDer.Controls.AddRange(new Control[]
            {
                lblNueva, txtNueva, btnAgregar,
                lstFamilias, lstParientes, lstSeleccionados,
                lblPreview, treePreview,
                btnAgregarFamilia, btnAgregarPariente, btnEnlazarPariente, btnEliminarSeleccionado, btnGuardarFamilia
            });

            this.Controls.AddRange(new Control[] { panelIzq, panelDer });
        }

        // ─── ÁRBOL IZQUIERDO ─────────────────────────────────────────────────
        private void RefrescarArbol()
        {
            treeArbol.Nodes.Clear();
            GrupoPermiso raiz = service.ObtenerArbol();
            treeArbol.Nodes.Add(ArmarNodo(raiz));
            treeArbol.ExpandAll();
        }

        private TreeNode ArmarNodo(IComponentePermiso componente)
        {
            var nodo = new TreeNode(componente.Nombre);
            GrupoPermiso grupo = componente as GrupoPermiso;
            if (grupo != null)
                foreach (var hijo in grupo.Hijos())
                    nodo.Nodes.Add(ArmarNodo(hijo));
            return nodo;
        }

        // ─── LISTAS ───────────────────────────────────────────────────────────
        private void RefrescarListas()
        {
            string familiaAntes = lstFamilias.SelectedItem != null ? lstFamilias.SelectedItem.ToString() : null;
            string parienteAntes = lstParientes.SelectedItem != null ? lstParientes.SelectedItem.ToString() : null;

            lstFamilias.Items.Clear();
            lstParientes.Items.Clear();

            GrupoPermiso raiz = service.ObtenerArbol();
            RecolectarGrupos(raiz, lstFamilias);

            // Col 2: parientes del arbol + parientes disponibles (sin duplicar)
            RecolectarHojas(raiz, lstParientes);
            foreach (string p in service.ObtenerParientesDisponibles())
                if (!lstParientes.Items.Contains(p))
                    lstParientes.Items.Add(p);

            // Restaurar seleccion si sigue existiendo
            if (familiaAntes != null && lstFamilias.Items.Contains(familiaAntes))
                lstFamilias.SelectedItem = familiaAntes;
            if (parienteAntes != null && lstParientes.Items.Contains(parienteAntes))
                lstParientes.SelectedItem = parienteAntes;
        }

        private void RecolectarGrupos(GrupoPermiso grupo, ListBox lb)
        {
            lb.Items.Add(grupo.Nombre);
            foreach (var hijo in grupo.Hijos())
            {
                GrupoPermiso g = hijo as GrupoPermiso;
                if (g != null) RecolectarGrupos(g, lb);
            }
        }

        private void RecolectarHojas(GrupoPermiso grupo, ListBox lb)
        {
            foreach (var hijo in grupo.Hijos())
            {
                if (hijo is PermisoLeaf)
                {
                    if (!lb.Items.Contains(hijo.Nombre))
                        lb.Items.Add(hijo.Nombre);
                }
                else
                {
                    GrupoPermiso g = hijo as GrupoPermiso;
                    if (g != null) RecolectarHojas(g, lb);
                }
            }
        }

        // ─── PREVIEW EN TIEMPO REAL ──────────────────────────────────────────
        private void ActualizarPreview()
        {
            treePreview.Nodes.Clear();
            lstSeleccionados.Items.Clear();

            string nombreNueva = txtNueva.Text.Trim();
            string familiaSeleccionada = lstFamilias.SelectedItem != null ? lstFamilias.SelectedItem.ToString() : null;
            string parienteSeleccionado = lstParientes.SelectedItem != null ? lstParientes.SelectedItem.ToString() : null;

            // Col 4: solo mostrar preview si hay algo para mostrar
            bool hayNueva = !string.IsNullOrWhiteSpace(nombreNueva);
            bool hayPadre = !string.IsNullOrWhiteSpace(familiaSeleccionada);
            bool hayPariente = !string.IsNullOrWhiteSpace(parienteSeleccionado);

            if (hayNueva || hayPadre || hayPariente)
            {
                string nombrePadre = hayPadre ? familiaSeleccionada : "(familia padre)";
                TreeNode nodoPadre = new TreeNode(nombrePadre);

                if (hayNueva)
                {
                    TreeNode nodoHijo = new TreeNode(nombreNueva);
                    if (hayPariente)
                        nodoHijo.Nodes.Add(new TreeNode(parienteSeleccionado));
                    nodoPadre.Nodes.Add(nodoHijo);
                }
                else if (hayPariente)
                {
                    nodoPadre.Nodes.Add(new TreeNode(parienteSeleccionado));
                }

                treePreview.Nodes.Add(nodoPadre);
                treePreview.ExpandAll();
            }

            // Mostrar en columna 3 lo que se eliminaria
            if (!string.IsNullOrWhiteSpace(familiaSeleccionada) || !string.IsNullOrWhiteSpace(parienteSeleccionado))
            {
                if (!string.IsNullOrWhiteSpace(familiaSeleccionada))
                {
                    lstSeleccionados.Items.Add("[Familia] " + familiaSeleccionada);
                    // Agregar hijos de esa familia
                    GrupoPermiso raiz = service.ObtenerArbol();
                    GrupoPermiso grupo = BuscarGrupo(raiz, familiaSeleccionada);
                    if (grupo != null)
                        foreach (var hijo in grupo.Hijos())
                            lstSeleccionados.Items.Add("  - " + hijo.Nombre);
                }
                if (!string.IsNullOrWhiteSpace(parienteSeleccionado))
                    lstSeleccionados.Items.Add("[Pariente] " + parienteSeleccionado);
            }
        }

        private GrupoPermiso BuscarGrupo(GrupoPermiso actual, string nombre)
        {
            if (actual.Nombre == nombre) return actual;
            foreach (var hijo in actual.Hijos())
            {
                GrupoPermiso g = hijo as GrupoPermiso;
                if (g != null)
                {
                    GrupoPermiso res = BuscarGrupo(g, nombre);
                    if (res != null) return res;
                }
            }
            return null;
        }

        // ─── BOTONES ─────────────────────────────────────────────────────────

        // Boton "Agregar" del header: agrega la nueva familia con el pariente seleccionado dentro del padre seleccionado
        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            string nombreNueva = txtNueva.Text.Trim();
            string padreNombre = lstFamilias.SelectedItem != null ? lstFamilias.SelectedItem.ToString() : null;
            string parienteNombre = lstParientes.SelectedItem != null ? lstParientes.SelectedItem.ToString() : null;

            if (string.IsNullOrWhiteSpace(nombreNueva))
            {
                MessageBox.Show("Escribí el nombre de la nueva familia.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(padreNombre))
            {
                MessageBox.Show("Seleccioná una familia padre en la primera columna.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Crear la nueva familia bajo el padre seleccionado
            Resultado res = service.AgregarFamilia(nombreNueva, padreNombre);
            if (!res.Ok)
            {
                MessageBox.Show(res.Mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Si hay pariente seleccionado, agregarlo dentro de la nueva familia recien creada
            if (!string.IsNullOrWhiteSpace(parienteNombre))
            {
                Resultado resPariente = service.AgregarPariente(parienteNombre, nombreNueva);
                if (!resPariente.Ok)
                {
                    MessageBox.Show("Familia creada pero no se pudo agregar el pariente: " + resPariente.Mensaje, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            RefrescarArbol();
            RefrescarListas();
            txtNueva.Clear();
            ActualizarPreview();
            MessageBox.Show("Agregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAgregarFamilia_Click(object sender, EventArgs e)
        {
            string nombreNueva = txtNueva.Text.Trim();
            string padreNombre = lstFamilias.SelectedItem != null ? lstFamilias.SelectedItem.ToString() : null;

            if (string.IsNullOrWhiteSpace(nombreNueva))
            {
                MessageBox.Show("Escribí el nombre de la nueva familia.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(padreNombre))
            {
                MessageBox.Show("Seleccioná una familia padre en la primera columna.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Resultado res = service.AgregarFamilia(nombreNueva, padreNombre);
            MessageBox.Show(res.Mensaje, res.Ok ? "Éxito" : "Error", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok) { txtNueva.Clear(); RefrescarArbol(); RefrescarListas(); ActualizarPreview(); }
        }

        private void BtnAgregarPariente_Click(object sender, EventArgs e)
        {
            string nombreNuevo = txtNueva.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombreNuevo))
            {
                MessageBox.Show("Escribí el nombre del pariente en el campo de arriba.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (lstParientes.Items.Contains(nombreNuevo))
            {
                MessageBox.Show("Ya existe un pariente con ese nombre.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Agregar como hoja suelta bajo la raiz para que quede disponible
            service.AgregarParienteLibre(nombreNuevo);
            txtNueva.Clear();
            RefrescarArbol();
            RefrescarListas();
            ActualizarPreview();
        }

        private void BtnEnlazarPariente_Click(object sender, EventArgs e)
        {
            string parienteNombre = lstParientes.SelectedItem != null ? lstParientes.SelectedItem.ToString() : null;
            string padreNombre = lstFamilias.SelectedItem != null ? lstFamilias.SelectedItem.ToString() : null;

            if (string.IsNullOrWhiteSpace(parienteNombre))
            {
                MessageBox.Show("Seleccioná un pariente de la segunda columna.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(padreNombre))
            {
                MessageBox.Show("Seleccioná una familia destino en la primera columna.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Resultado res = service.AgregarPariente(parienteNombre, padreNombre);
            MessageBox.Show(res.Mensaje, res.Ok ? "Éxito" : "Error", MessageBoxButtons.OK, res.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (res.Ok) { RefrescarArbol(); RefrescarListas(); ActualizarPreview(); }
        }

        private void BtnEliminarSeleccionado_Click(object sender, EventArgs e)
        {
            string familiaSeleccionada = lstFamilias.SelectedItem != null ? lstFamilias.SelectedItem.ToString() : null;
            string parienteSeleccionado = lstParientes.SelectedItem != null ? lstParientes.SelectedItem.ToString() : null;

            if (string.IsNullOrWhiteSpace(familiaSeleccionada) && string.IsNullOrWhiteSpace(parienteSeleccionado))
            {
                MessageBox.Show("Seleccioná una familia o pariente para eliminar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mensaje = "¿Confirmas eliminar:\n";
            if (!string.IsNullOrWhiteSpace(familiaSeleccionada))
                mensaje += "  - Familia: " + familiaSeleccionada + " (y todos sus hijos)\n";
            if (!string.IsNullOrWhiteSpace(parienteSeleccionado))
                mensaje += "  - Pariente: " + parienteSeleccionado + "\n";

            if (MessageBox.Show(mensaje, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (!string.IsNullOrWhiteSpace(familiaSeleccionada))
                    service.EliminarFamilia(familiaSeleccionada);
                if (!string.IsNullOrWhiteSpace(parienteSeleccionado))
                    service.EliminarPariente(parienteSeleccionado);

                lstSeleccionados.Items.Clear();
                RefrescarArbol();
                RefrescarListas();
                ActualizarPreview();
            }
        }

        private void BtnGuardarFamilia_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Estructura guardada correctamente.", "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ListBox_MouseDown(object sender, MouseEventArgs e)
        {
            ListBox lb = sender as ListBox;
            int index = lb.IndexFromPoint(e.Location);
            // Si hace click en el item ya seleccionado, deselecciona
            if (index >= 0 && lb.SelectedIndex == index)
            {
                lb.SelectedIndex = -1;
                ActualizarPreview();
            }
        }

    }
}