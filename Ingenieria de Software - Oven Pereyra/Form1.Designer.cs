namespace UI
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button1  = new System.Windows.Forms.Button();
            this.button2  = new System.Windows.Forms.Button();
            this.button3  = new System.Windows.Forms.Button();
            this.button4  = new System.Windows.Forms.Button();
            this.button5  = new System.Windows.Forms.Button();
            this.button6  = new System.Windows.Forms.Button();
            this.lblTitulo    = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.lblUsuario   = new System.Windows.Forms.Label();
            this.lblClave     = new System.Windows.Forms.Label();
            this.panel1   = new System.Windows.Forms.Panel();
            this.SuspendLayout();

            this.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);

            // panel1
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Location  = new System.Drawing.Point(200, 50);
            this.panel1.Size      = new System.Drawing.Size(360, 500);
            this.panel1.Paint    += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);

            // lblTitulo
            this.lblTitulo.Text      = "Sistema";
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI", 28, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(30, 136, 229);
            this.lblTitulo.Location  = new System.Drawing.Point(0, 20);
            this.lblTitulo.Size      = new System.Drawing.Size(360, 55);
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblSubtitulo
            this.lblSubtitulo.Text      = "Ingresá tus credenciales";
            this.lblSubtitulo.Font      = new System.Drawing.Font("Segoe UI", 9);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.Gray;
            this.lblSubtitulo.Location  = new System.Drawing.Point(0, 72);
            this.lblSubtitulo.Size      = new System.Drawing.Size(360, 20);
            this.lblSubtitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblUsuario
            this.lblUsuario.Text      = "Usuario";
            this.lblUsuario.Font      = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            this.lblUsuario.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.lblUsuario.Location  = new System.Drawing.Point(40, 105);
            this.lblUsuario.Size      = new System.Drawing.Size(280, 18);

            // textBox1
            this.textBox1.Font        = new System.Drawing.Font("Segoe UI", 10);
            this.textBox1.Location    = new System.Drawing.Point(40, 125);
            this.textBox1.Size        = new System.Drawing.Size(280, 28);
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // lblClave
            this.lblClave.Text      = "Contraseña";
            this.lblClave.Font      = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            this.lblClave.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.lblClave.Location  = new System.Drawing.Point(40, 165);
            this.lblClave.Size      = new System.Drawing.Size(280, 18);

            // textBox2
            this.textBox2.Font        = new System.Drawing.Font("Segoe UI", 10);
            this.textBox2.Location    = new System.Drawing.Point(40, 185);
            this.textBox2.Size        = new System.Drawing.Size(280, 28);
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // button1 - Login
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.BackColor = System.Drawing.Color.FromArgb(30, 136, 229);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.button1.Location  = new System.Drawing.Point(40, 235);
            this.button1.Size      = new System.Drawing.Size(280, 38);
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.Text      = "Iniciar sesion";
            this.button1.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.button1.Click    += new System.EventHandler(this.button1_Click);

            // button4 - Administrar Composite (solo admin) Y=235
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.button4.ForeColor = System.Drawing.Color.White;
            this.button4.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.button4.Location  = new System.Drawing.Point(40, 235);
            this.button4.Size      = new System.Drawing.Size(280, 38);
            this.button4.FlatAppearance.BorderSize = 0;
            this.button4.Text      = "Administrar Composite";
            this.button4.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.button4.Visible   = false;
            this.button4.Click    += new System.EventHandler(this.button4_Click);

            // button2 - Administrar Usuarios (solo admin) Y=283
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.BackColor = System.Drawing.Color.FromArgb(30, 136, 229);
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.button2.Location  = new System.Drawing.Point(40, 283);
            this.button2.Size      = new System.Drawing.Size(280, 38);
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.Text      = "Administrar Usuarios";
            this.button2.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.button2.Visible   = false;
            this.button2.Click    += new System.EventHandler(this.button2_Click);

            // button5 - Bitacora (solo admin) Y=331
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.button5.ForeColor = System.Drawing.Color.White;
            this.button5.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.button5.Location  = new System.Drawing.Point(40, 331);
            this.button5.Size      = new System.Drawing.Size(280, 38);
            this.button5.FlatAppearance.BorderSize = 0;
            this.button5.Text      = "Bitacora";
            this.button5.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.button5.Visible   = false;
            this.button5.Click    += new System.EventHandler(this.button5_Click);

            // button6 - Control de Cambios (solo admin) Y=379
            this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button6.BackColor = System.Drawing.Color.FromArgb(123, 31, 162);
            this.button6.ForeColor = System.Drawing.Color.White;
            this.button6.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.button6.Location  = new System.Drawing.Point(40, 379);
            this.button6.Size      = new System.Drawing.Size(280, 38);
            this.button6.FlatAppearance.BorderSize = 0;
            this.button6.Text      = "Control de Cambios";
            this.button6.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.button6.Visible   = false;
            this.button6.Click    += new System.EventHandler(this.button6_Click);

            // button3 - Logout Y=427
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.button3.ForeColor = System.Drawing.Color.White;
            this.button3.Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            this.button3.Location  = new System.Drawing.Point(40, 427);
            this.button3.Size      = new System.Drawing.Size(280, 38);
            this.button3.FlatAppearance.BorderSize = 0;
            this.button3.Text      = "Cerrar sesion";
            this.button3.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.button3.Visible   = false;
            this.button3.Click    += new System.EventHandler(this.button3_Click);

            // Agregar al panel
            this.panel1.Controls.Add(this.lblTitulo);
            this.panel1.Controls.Add(this.lblSubtitulo);
            this.panel1.Controls.Add(this.lblUsuario);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.lblClave);
            this.panel1.Controls.Add(this.textBox2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.button4);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button5);
            this.panel1.Controls.Add(this.button6);
            this.panel1.Controls.Add(this.button3);

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(760, 610);
            this.Controls.Add(this.panel1);
            this.Name          = "Form1";
            this.Text          = "Sistema de Usuarios";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button  button1;
        private System.Windows.Forms.Button  button2;
        private System.Windows.Forms.Button  button3;
        private System.Windows.Forms.Button  button4;
        private System.Windows.Forms.Button  button5;
        private System.Windows.Forms.Button  button6;
        private System.Windows.Forms.Label   lblTitulo;
        private System.Windows.Forms.Label   lblSubtitulo;
        private System.Windows.Forms.Label   lblUsuario;
        private System.Windows.Forms.Label   lblClave;
        private System.Windows.Forms.Panel   panel1;
    }
}
