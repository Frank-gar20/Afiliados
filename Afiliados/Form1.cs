using OfficeOpenXml;
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
                        rowCount = rowCount + 1;
                        afiliados++;
                    }
                    rowCount = rowCount + 3;
                    for (int i = 2; i < rowCount; i++)
                    {
                        DataRow row = dt.NewRow();
                        municipios.Add("TODOS");
                        string muni = worksheet.Cells[i, 3].Text;
                        if (!string.IsNullOrEmpty(muni))
                        {
                            municipios.Add(muni);
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
            //habilitamos segun el checkbox
            dtpInicio.Enabled = chbFecha.Checked;
            dtpfin.Enabled = chbFecha.Checked;
            lblFechaInicio.Enabled = chbFecha.Checked;
            lblFechaFin.Enabled = chbFecha.Checked;
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
                dv.RowFilter = "Municipio = ''";
                dgvInformacion.DataSource = dv;
            }
            else
            {
                DataView dv = new DataView(dt);
                dv.RowFilter = $"Municipio = '{seleccion}'"; // usamos filtro para "leer" la columna
                dgvInformacion.DataSource = dv;
            }
            txtAfiliados.Text = dgvInformacion.Rows.Count.ToString();
        }

        private void reiniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtEstado.Text = string.Empty;
            txtArchivo.Text = string.Empty;
            txtAfiliados.Text = string.Empty;
            chbFecha.Checked = false;
            municipios.Clear();
            cbMunicipio.DataSource = municipios.ToList();
            cbMunicipio.Text = string.Empty;
            
            dt.Clear();
        }
    }
}
