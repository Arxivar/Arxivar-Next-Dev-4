using System;
using System.Windows.Forms;

namespace ProgettoWorkflow
{
    public partial class FormTask : Form
    {
        public string ProtocolloInterno { get; private set; }

        public FormTask(string protocolloInterno)
        {
            ProtocolloInterno = protocolloInterno;
            InitializeComponent();

            txtProtocolloInterno.Text = protocolloInterno;
        }

        private void btnValida_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtProtocolloInterno.Text))
            {
                MessageBox.Show("Il protocollo interno è vuoto", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var numero = 0;

            if (!int.TryParse(txtProtocolloInterno.Text, out numero))
            {

                MessageBox.Show("Il protocollo interno deve essere un numero intero", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                MessageBox.Show("Pprotocollo interno corretto", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ProtocolloInterno = txtProtocolloInterno.Text;
            }
        }
    }
}
