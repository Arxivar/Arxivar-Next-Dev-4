using System;
using System.Windows.Forms;
using Abletech.Arxivar.Client;
using Abletech.Arxivar.Entities;

namespace ProgettoClient
{
    public partial class FormDocumentiFascicolo : Form
    {
        private readonly Dm_Fascicoli _fascicolo;
        private readonly Arx_DataSource _searchResult;
        private readonly WCFConnectorManager _connectorManager;

        public FormDocumentiFascicolo(Dm_Fascicoli fascicolo, Arx_DataSource searchResult, WCFConnectorManager connectorManager)
        {
            _fascicolo = fascicolo;
            _searchResult = searchResult;
            _connectorManager = connectorManager;
            InitializeComponent();

            Binding();
        }

        private void Binding()
        {
            dataGridView.DataSource = _searchResult.GetDataTable(0);
            lblFascicolo.Text = _fascicolo.NOME;

            DataGridViewButtonColumn removeButtonColumn = new DataGridViewButtonColumn();
            removeButtonColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            removeButtonColumn.Name = "remove_column";
            removeButtonColumn.HeaderText = "Remove";
            removeButtonColumn.Text = "Remove";
            
            int columnIndex = 5;
            if (dataGridView.Columns["remove_column"] == null)
            {
                dataGridView.Columns.Insert(columnIndex, removeButtonColumn);
            }
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            { return; }

            try
            {
                if (e.ColumnIndex == dataGridView.Columns["remove_column"].Index)
                {
                    // Rimuovo il docnumber dal fascicolo
                    int columnIndex = 4;
                    int rowIndex = e.RowIndex;
                    var cellValue = dataGridView[columnIndex, rowIndex].Value;
                    var docnumber = int.Parse(cellValue.ToString());
                    _connectorManager.ARX_DATI.Dm_FileInFolder_Delete_By_Folderid_Docnumber(_fascicolo.ID, docnumber);

                    dataGridView.Rows.RemoveAt(e.RowIndex);
                    dataGridView.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
