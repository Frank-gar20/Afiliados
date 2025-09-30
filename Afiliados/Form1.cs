using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Afiliados
{
    public partial class FRMafiliados : Form
    {
        List<string> columnas;
        public int afiliados = 0;
        DataTable dt;
        HashSet<String> municipios;//usamos hashset para una busqueda rapida y eliminar duplicados
        public FRMafiliados()
        {
            InitializeComponent();
            columnas = new List<string> {
                "ID",
                "Entidad",
                "Municipio",
                "Nombre",
                "Fecha de afiliacion",
                "Estatus" };
            municipios = new HashSet<string>();
            dt = new DataTable();
            foreach (var col in columnas)
            {
                dt.Columns.Add(col);
            }
            cbMunicipio.SelectedIndexChanged += cbMunicipio_SelectedIndexChanged;
            txtBusquedaAfi.ReadOnly = true;
        }

        private void FRMafiliados_Load(object sender, EventArgs e)
        {

        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void importarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("Ya hay datos cargados. Reinicia antes de importar otro Excel.", "SISTEMA",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (ofdAbrir.ShowDialog() == DialogResult.OK)
            {
                string archivo = ofdAbrir.FileName;
                CargarExcel(archivo);
            }
        }

        private void CargarExcel(String path)
        {
            afiliados = 1;
            ExcelPackage.License.SetNonCommercialPersonal("Francisco Garcia");
            try
            {
                using (var package = new ExcelPackage(new System.IO.FileInfo(path)))
                {
                    if (package.Workbook.Worksheets.Count == 0)
                    {
                        MessageBox.Show("El archivo de Excel no contiene hojas de trabajo.");
                        return;//por si no hay hojas
                    }

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    int rowCount = 0;
                    for (int i = 2; i < worksheet.Dimension.End.Row; i++)
                    {
                        rowCount++;
                        afiliados++;
                    }
                    rowCount = rowCount + 3;
                    for (int i = 2; i < rowCount; i++)
                    {
                        DataRow row = dt.NewRow();
                        municipios.Add("TODOS");
                        string m = worksheet.Cells[i, 3].Text;
                        if (!string.IsNullOrEmpty(m))
                        {
                            municipios.Add(m);
                        }
                        else
                        {
                            municipios.Add("NINGUNO");
                        }

                        for (int j = 1; j < dt.Columns.Count + 1; j++)
                        {
                            row[j - 1] = worksheet.Cells[i, j].Text;
                        }
                        dt.Rows.Add(row);
                    }

                    cbMunicipio.Invoke((MethodInvoker)delegate
                    {
                        cbMunicipio.DataSource = municipios.ToList();
                    });

                    dgvInformacion.Invoke((MethodInvoker)delegate
                    {
                        dgvInformacion.DataSource = null;
                        dgvInformacion.Columns.Clear();
                        dgvInformacion.AutoGenerateColumns = true;
                        dgvInformacion.DataSource = dt;
                        dgvInformacion.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    });

                    this.Invoke((MethodInvoker)delegate
                    {
                        txtAfiliados.Text = afiliados.ToString();
                        txtEstado.Text = dgvInformacion.Rows[0].Cells[1].Value.ToString();
                        txtArchivo.Text = ofdAbrir.SafeFileName;
                    });
                    txtBusquedaAfi.ReadOnly = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message, "Error al cargar el Excel");
            }
        }
        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void chbFecha_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                //habilitamos segun el checkbox
                dtpInicio.Enabled = chbFecha.Checked;
                dtpfin.Enabled = chbFecha.Checked;
                lblFechaInicio.Enabled = chbFecha.Checked;
                lblFechaFin.Enabled = chbFecha.Checked;
                btnBuscar.Enabled = chbFecha.Checked;

                cbMunicipio.Enabled = !chbFecha.Checked;
                cbMunicipio.SelectedIndex = 0;
                txtBusquedaAfi.Enabled = !chbFecha.Checked;
                txtBusquedaAfi.Text = String.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: "+ex.Message+" \nAbre un Archivo", "Sistema",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void cbMunicipio_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMunicipio.SelectedItem == null) return;

            string seleccion = cbMunicipio.SelectedItem.ToString();

            if (seleccion == "TODOS")
            {
                dgvInformacion.DataSource = dt;
            }
            else if (seleccion == "NINGUNO")
            {
                DataView dv = new DataView(dt);
                dv.RowFilter = "Municipio = ''";//filtro vacilo ' '
                dgvInformacion.DataSource = dv;
            }
            else
            {
                DataView dv = new DataView(dt);
                dv.RowFilter = $"Municipio = '{seleccion}'"; // usamos filtro para "leer" la columna
                dgvInformacion.DataSource = dv;
            }
            txtAfiliados.Text = (dgvInformacion.AllowUserToAddRows ? dgvInformacion.Rows.Count - 1 : dgvInformacion.Rows.Count).ToString();
        }

        private void reiniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtEstado.Text = string.Empty;
            txtArchivo.Text = string.Empty;
            txtAfiliados.Text = string.Empty;
            txtBusquedaAfi.Text = string.Empty;
            chbFecha.Checked = false;
            dtpInicio.Value = DateTime.Now;
            dtpfin.Value = DateTime.Now;
            txtBusquedaAfi.Enabled = false;
            txtBusquedaAfi.Text = string.Empty;

            municipios.Clear();
            cbMunicipio.DataSource = null;
            cbMunicipio.Items.Clear();
            cbMunicipio.Text = string.Empty;


            dt.Clear();
            dgvInformacion.DataSource = null;

        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime fInicio = dtpInicio.Value.Date;
                DateTime ffin = dtpfin.Value.Date;
                DataView dv = new DataView(dt);

                string inicio = fInicio.ToString("MM/dd/yyyy");
                string fin = ffin.ToString("MM/dd/yyyy");
                dv.RowFilter = $"CONVERT([Fecha de afiliacion], 'System.DateTime') >= #{inicio}# AND CONVERT([Fecha de afiliacion], 'System.DateTime') <= #{fin}#";

                dgvInformacion.DataSource = dv;
                txtAfiliados.Text = (dgvInformacion.AllowUserToAddRows ? dgvInformacion.Rows.Count - 1 : dgvInformacion.Rows.Count).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:"+ex, "Sistema", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtBusquedaAfi_TextChanged(object sender, EventArgs e)
        {
            cbMunicipio.SelectedIndex = 0;
            dgvInformacion.DataSource = dt;
            DataView dv = dt.DefaultView;
            if (txtBusquedaAfi.Text == "")
            {
                dv.RowFilter = "";
            }
            else
            {
                dv.RowFilter = $"ID='{txtBusquedaAfi.Text}'";
            }
            dgvInformacion.DataSource = dv;
            txtAfiliados.Text = (dgvInformacion.AllowUserToAddRows ? dgvInformacion.Rows.Count - 1 : dgvInformacion.Rows.Count).ToString();
        }

        private void txtBusquedaAfi_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                e.Handled = false;
                return;
            }

            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = false; //Prmite el dígito
            }
            else
            {
                e.Handled = true; //No permite el caracter
            }
        }
    }
}