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

        private void CargarExcel(string path)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("Jose Franscisco Garcia Lopez");

                using (var package = new ExcelPackage(new System.IO.FileInfo(path)))
                {
                    if (package.Workbook.Worksheets.Count == 0)
                    {
                        MessageBox.Show("El archivo de Excel no contiene hojas de trabajo.");
                        return; //En caso que no haya hojas
                    }

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    DataTable dt = new DataTable();

                    // Leer los encabezados de columna
                    foreach (var col in columnas)
                    {
                        dt.Columns.Add(col);
                    }

                    // Leer las filas de datos
                    int rowCount = worksheet.Dimension.End.Row;
                    dgvInformacion.Rows.Clear(); //limpiar antes de mostrar

                    for (int i = 2; i < rowCount; i++)
                    {
                        String id = worksheet.Cells[i, 1].Text;
                        if (!String.IsNullOrWhiteSpace(id))
                        {
                            DataRow row = dt.NewRow();

                            id = worksheet.Cells[i, 1].Text;
                            string entidad = worksheet.Cells[i, 2].Text;
                            string municipio = worksheet.Cells[i, 3].Text;
                            string nombre = worksheet.Cells[i, 4].Text;
                            string fecha_af = worksheet.Cells[i, 5].Text;
                            string estatus = worksheet.Cells[i, 6].Text;

                            dt.Rows.Add(row);
                            //ponemos los datos en el DGV
                            dgvInformacion.Rows.Add(id,entidad,municipio,nombre,fecha_af,estatus);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de carga");
            }            
        }
    }
}
