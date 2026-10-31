using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Web.Script.Serialization;

public class Transaccion
{
    public int Id { get; set; }
    public string Fecha { get; set; }
    public string Tipo { get; set; } // "Ingreso" o "Egreso"
    public decimal Monto { get; set; }
    public string Categoria { get; set; }
    public string Descripcion { get; set; }
}

public class FinopticonForm : Form
{
    private string rutaArchivo = "finopticon_data.json";
    private List<Transaccion> listaTransacciones = new List<Transaccion>();

    private TabControl tabControlPrincipal;
    private TabPage tabMovimientos;
    private TabPage tabResumen;

    private TextBox txtMonto;
    private TextBox txtDescripcion;
    private ComboBox comboTipo;
    private ComboBox comboCategoria;
    private ListView listTransacciones;
    private Label lblSaldoTotal;
    private Label lblEstado;
    
    private TextBox txtBuscar;
    private ComboBox comboFiltroTipo;

    public FinopticonForm()
    {
        this.Text = "Finopticon - Gestión Financiera Local";
        this.Size = new Size(960, 600);
        this.MinimumSize = new Size(820, 520);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(30, 30, 30);

        try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch {}

        tabControlPrincipal = new TabControl()
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5f)
        };

        tabMovimientos = new TabPage(" Movimientos ") { BackColor = Color.FromArgb(30, 30, 30) };
        tabResumen = new TabPage(" Resumen y Saldos ") { BackColor = Color.FromArgb(30, 30, 30) };

        tabControlPrincipal.TabPages.Add(tabMovimientos);
        tabControlPrincipal.TabPages.Add(tabResumen);
        tabControlPrincipal.SelectedIndexChanged += (s, e) => ActualizarResumen();

        ConstruirPestanaMovimientos();
        ConstruirPestanaResumen();
        this.Controls.Add(tabControlPrincipal);

        CargarDatosLocales();
    }

    private void ConstruirPestanaMovimientos()
    {
        Label lblTitulo = new Label() { Text = "Nueva Transacción", ForeColor = Color.LightGray, Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
        
        Label lblTipo = new Label() { Text = "Tipo:", ForeColor = Color.White, Location = new Point(20, 50), AutoSize = true };
        comboTipo = new ComboBox() { Location = new Point(20, 75), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
        comboTipo.Items.AddRange(new string[] { "Ingreso", "Egreso" });
        comboTipo.SelectedIndex = 1;

        Label lblMonto = new Label() { Text = "Monto:", ForeColor = Color.White, Location = new Point(20, 115), AutoSize = true };
        txtMonto = new TextBox() { Location = new Point(20, 140), Width = 200, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        Label lblCategoria = new Label() { Text = "Categoría:", ForeColor = Color.White, Location = new Point(20, 180), AutoSize = true };
        comboCategoria = new ComboBox() { Location = new Point(20, 205), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
        comboCategoria.Items.AddRange(new string[] { "Alimentación", "Servicios", "Transporte", "Vivienda", "Salud", "Entretenimiento", "Otros" });
        comboCategoria.SelectedIndex = 0;

        Label lblDesc = new Label() { Text = "Descripción:", ForeColor = Color.White, Location = new Point(20, 245), AutoSize = true };
        txtDescripcion = new TextBox() { Location = new Point(20, 270), Width = 200, Height = 60, Multiline = true, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        Button btnGuardar = new Button() { Text = "Registrar", Location = new Point(20, 345), Width = 200, Height = 35, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnGuardar.Click += BtnGuardar_Click;

        Button btnEliminar = new Button() { Text = "Eliminar Seleccionado", Location = new Point(20, 395), Width = 200, Height = 30, BackColor = Color.FromArgb(180, 50, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnEliminar.Click += BtnEliminar_Click;

        Button btnExportar = new Button() { Text = "Exportar CSV", Location = new Point(20, 435), Width = 200, Height = 30, BackColor = Color.FromArgb(90, 90, 90), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnExportar.Click += BtnExportar_Click;

        // Filtros y Listado
        Label lblBusc = new Label() { Text = "Buscar:", ForeColor = Color.White, Location = new Point(240, 15), AutoSize = true };
        txtBuscar = new TextBox() { Location = new Point(295, 12), Width = 150, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        txtBuscar.TextChanged += (s, e) => RefrescarTabla();

        Label lblFilt = new Label() { Text = "Tipo:", ForeColor = Color.White, Location = new Point(465, 15), AutoSize = true };
        comboFiltroTipo = new ComboBox() { Location = new Point(505, 12), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
        comboFiltroTipo.Items.AddRange(new string[] { "Todos", "Ingreso", "Egreso" });
        comboFiltroTipo.SelectedIndex = 0;
        comboFiltroTipo.SelectedIndexChanged += (s, e) => RefrescarTabla();

        listTransacciones = new ListView()
        {
            Location = new Point(240, 45),
            Size = new Size(680, 425),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BackColor = Color.FromArgb(40, 40, 40),
            ForeColor = Color.White,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        listTransacciones.Columns.Add("ID", 45);
        listTransacciones.Columns.Add("Fecha", 95);
        listTransacciones.Columns.Add("Tipo", 80);
        listTransacciones.Columns.Add("Categoría", 110);
        listTransacciones.Columns.Add("Monto", 90);
        listTransacciones.Columns.Add("Descripción", 230);

        lblEstado = new Label() { Text = "Sistema listo.", ForeColor = Color.Gray, Location = new Point(240, 480), AutoSize = true, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

        tabMovimientos.Controls.AddRange(new Control[] { 
            lblTitulo, lblTipo, comboTipo, lblMonto, txtMonto, lblCategoria, comboCategoria, 
            lblDesc, txtDescripcion, btnGuardar, btnEliminar, btnExportar, 
            lblBusc, txtBuscar, lblFilt, comboFiltroTipo, listTransacciones, lblEstado 
        });
    }

    private void ConstruirPestanaResumen()
    {
        Label lblTit = new Label() { Text = "Estado General de Cuentas", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, Location = new Point(30, 30), AutoSize = true };
        
        Panel cardSaldo = new Panel() { Location = new Point(30, 80), Size = new Size(400, 100), BackColor = Color.FromArgb(45, 45, 45) };
        Panel barra = new Panel() { Location = new Point(0, 0), Size = new Size(6, 100), BackColor = Color.FromArgb(0, 122, 204), Dock = DockStyle.Left };
        Label lblT1 = new Label() { Text = "SALDO NETO ACTUAL", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.LightGray, Location = new Point(20, 15), AutoSize = true };
        lblSaldoTotal = new Label() { Text = "$0.00", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 40), AutoSize = true };
        
        cardSaldo.Controls.AddRange(new Control[] { barra, lblT1, lblSaldoTotal });
        tabResumen.Controls.AddRange(new Control[] { lblTit, cardSaldo });
    }

    private void ActualizarResumen()
    {
        decimal totalIngresos = listaTransacciones.Where(item => item.Tipo == "Ingreso").Sum(item => item.Monto);
        decimal totalEgresos = listaTransacciones.Where(item => item.Tipo == "Egreso").Sum(item => item.Monto);
        decimal saldoNeto = totalIngresos - totalEgresos;

        lblSaldoTotal.Text = saldoNeto.ToString("C2");
        lblSaldoTotal.ForeColor = saldoNeto >= 0 ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);
    }

    private void BtnGuardar_Click(object sender, EventArgs e)
    {
        decimal monto;
        if (!decimal.TryParse(txtMonto.Text.Trim(), out monto) || monto <= 0)
        {
            MessageBox.Show("Por favor, ingrese un monto válido mayor a cero.", "Dato Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int nuevoId = listaTransacciones.Count > 0 ? listaTransacciones.Max(item => item.Id) + 1 : 1;

        Transaccion nueva = new Transaccion()
        {
            Id = nuevoId,
            Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            Tipo = comboTipo.SelectedItem.ToString(),
            Monto = monto,
            Categoria = comboCategoria.SelectedItem.ToString(),
            Descripcion = string.IsNullOrEmpty(txtDescripcion.Text) ? "-" : txtDescripcion.Text.Trim().Replace("\r\n", " ")
        };

        listaTransacciones.Add(nueva);
        GuardarDatosLocales();
        RefrescarTabla();

        txtMonto.Clear();
        txtDescripcion.Clear();
        lblEstado.Text = "Transacción registrada exitosamente.";
    }

    private void BtnEliminar_Click(object sender, EventArgs e)
    {
        if (listTransacciones.SelectedItems.Count == 0)
        {
            MessageBox.Show("Seleccione una transacción de la lista para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        int id = int.Parse(listTransacciones.SelectedItems[0].Text);
        var itemEncontrado = listaTransacciones.FirstOrDefault(item => item.Id == id);
        if (itemEncontrado != null)
        {
            if (MessageBox.Show("¿Desea eliminar este registro?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listaTransacciones.Remove(itemEncontrado);
                GuardarDatosLocales();
                RefrescarTabla();
                lblEstado.Text = "Transacción eliminada.";
            }
        }
    }

    private void BtnExportar_Click(object sender, EventArgs e)
    {
        try
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Archivo CSV (*.csv)|*.csv";
                sfd.FileName = "finopticon_reporte.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("ID,Fecha,Tipo,Categoría,Monto,Descripción");
                    foreach (var trans in listaTransacciones)
                    {
                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},\"{5}\"", trans.Id, trans.Fecha, trans.Tipo, trans.Categoria, trans.Monto, trans.Descripcion.Replace("\"", "\"\"")));
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Reporte exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        catch (Exception ex) 
        { 
            MessageBox.Show("Error al exportar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
        }
    }

    private void RefrescarTabla()
    {
        listTransacciones.Items.Clear();
        string busqueda = txtBuscar.Text.Trim().ToLower();
        string filtroTipo = comboFiltroTipo.SelectedItem != null ? comboFiltroTipo.SelectedItem.ToString() : "Todos";

        var filtrados = listaTransacciones.Where(item =>
            (string.IsNullOrEmpty(busqueda) || item.Categoria.ToLower().Contains(busqueda) || item.Descripcion.ToLower().Contains(busqueda)) &&
            (filtroTipo == "Todos" || item.Tipo == filtroTipo)
        ).ToList();

        foreach (var trans in filtrados)
        {
            ListViewItem item = new ListViewItem(trans.Id.ToString());
            item.SubItems.Add(trans.Fecha);
            item.SubItems.Add(trans.Tipo);
            item.SubItems.Add(trans.Categoria);
            item.SubItems.Add(trans.Monto.ToString("N2"));
            item.SubItems.Add(trans.Descripcion);
            listTransacciones.Items.Add(item);
        }
        ActualizarResumen();
    }

    private void GuardarDatosLocales()
    {
        try
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            File.WriteAllText(rutaArchivo, js.Serialize(listaTransacciones), Encoding.UTF8);
        }
        catch (Exception ex) { MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void CargarDatosLocales()
    {
        if (File.Exists(rutaArchivo))
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                listaTransacciones = js.Deserialize<List<Transaccion>>(File.ReadAllText(rutaArchivo, Encoding.UTF8)) ?? new List<Transaccion>();
            }
            catch { listaTransacciones = new List<Transaccion>(); }
        }
        RefrescarTabla();
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new FinopticonForm());
    }
}