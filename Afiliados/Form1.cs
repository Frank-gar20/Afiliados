using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Afiliados
{
    public partial class FRMafiliados : Form
    {
        List<string> columnas;
        public FRMafiliados()
        {
            InitializeComponent();
            columnas = new List<string>();
            columnas.Add("ID");
            columnas.Add("Entidad");
            columnas.Add("Municipio");
            columnas.Add("Nombre");
            columnas.Add("Fecha Afiliacion");
            columnas.Add("Estatus");

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

        private async void CargarExcel(string path)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("Jose Franscisco Garcia Lopez");

                DataTable dt = new DataTable();

                // Leer los encabezados de columna
                foreach (var col in columnas)
                {
                    dt.Columns.Add(col);
                }

                //hilo secundario
                await Task.Run(() =>
                {
                    using (var package = new ExcelPackage(new System.IO.FileInfo(path)))
                    {
                        if (package.Workbook.Worksheets.Count == 0)
                        {
                            MessageBox.Show("El archivo no contiene hojas de trabajo.");//por si no hay hojas
                            return;
                        }

                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.End.Row;
                        HashSet<string> municipios = new HashSet<string>();

                        for (int i = 2; i <= rowCount; i++)
                        {
                            string id = worksheet.Cells[i, 1].Text;
                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                string entidad = worksheet.Cells[i, 2].Text;
                                string municipio = worksheet.Cells[i, 3].Text;
                                string nombre = worksheet.Cells[i, 4].Text;
                                string fecha_af = worksheet.Cells[i, 5].Text;
                                string estatus = worksheet.Cells[i, 6].Text;

                                DataRow row = dt.NewRow();
                                row[0] = id;
                                row[1] = entidad;
                                row[2] = municipio;
                                row[3] = nombre;
                                row[4] = fecha_af;
                                row[5] = estatus;
                                dt.Rows.Add(row);//agregamos el renglon completo

                                //agregamos el estado solo la primera vuelta
                                if (string.IsNullOrWhiteSpace(txtEstado.Text))
                                {
                                    txtEstado.Invoke(new Action(() =>
                                    {
                                        txtEstado.Text = entidad;
                                    }));
                                }

                                //leemos los municipios del excel; con el hasset ignora los elementos repetidos
                                if (!string.IsNullOrWhiteSpace(municipio))
                                {
                                    municipios.Add(municipio);
                                }
                            }
                        }
                        //aqui cargamos el combo box 
                        cbMunicipio.Invoke(new Action(() =>
                        {
                            cbMunicipio.Items.Clear();
                        cbMunicipio.Items.Add("Todos");

                        foreach (var m in municipios.OrderBy(x => x))
                        {
                            cbMunicipio.Items.Add(m);
                        }

                        cbMunicipio.SelectedIndex = 0; //forzamos a que se muestren todos la primera vez
                        }));
                    }
                });
                //cargamos en el hilo principal
                dgvInformacion.Rows.Clear();

                foreach (DataRow r in dt.Rows)
                {
                    dgvInformacion.Rows.Add(r.ItemArray);
                }
                
                //agregamos la cantidad de afiliados
                txtAfiliados.Invoke(new Action(() =>
                {
                    txtAfiliados.Text = dt.Rows.Count.ToString();
                }));                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de carga: " + ex.Message);
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
        
        private void chbFecha_CheckedChanged(object sender, EventArgs e)
        {
            dtpInicio.Enabled = chbFecha.Checked;
            dtpfin.Enabled = chbFecha.Checked;
            lblFechaInicio.Enabled = chbFecha.Checked;
            lblFechaFin.Enabled = chbFecha.Checked;
        }

        private void cbMunicipio_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMunicipio.SelectedItem == null)
                return;

            string municipio = cbMunicipio.SelectedItem.ToString();

            foreach (DataGridViewRow row in dgvInformacion.Rows)
            {
                if (municipio == "Todos")
                {
                    row.Visible = true;
                }
                else
                {
                    row.Visible = row.Cells[2].Value.ToString() == municipio;
                }
            }
        }
    }
}
