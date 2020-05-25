using Abletech.Arxivar.Entities.Libraries;
using LibreriaComune;
using System;
using System.Windows.Forms;

namespace ProgettoWorkflow
{
    public partial class FormArchiviazione : Form
    {
        private readonly Dm_Profile_Insert_MV _dmProfileInsertMV;
        private readonly ClasseImpostazioni _impostazioni;

        public FormArchiviazione(Dm_Profile_Insert_MV dmProfileInsertMV, ClasseImpostazioni impostazioni)
        {
            InitializeComponent();
            this._dmProfileInsertMV = dmProfileInsertMV;
            this._impostazioni = impostazioni;
        }

        private void btnCalcola_Click(object sender, EventArgs e)
        {
            var number = new Random().Next(1, 1000000);
            txtProtocolloInterno.Text = number.ToString();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // Tutto OK -> riporto il numero di protocollo generato 
            _dmProfileInsertMV.DmProfileDefault.Dm_Profile_Insert_Base.ProtocolloInterno = txtProtocolloInterno.Text;
            _dmProfileInsertMV.DmProfileDefault.Dm_Profile_Insert_Base.DocName = _impostazioni.Etichetta;
        }

        private void btnAnnulla_Click(object sender, EventArgs e)
        {
            // Nessuna modifica effettuata al profilo.
        }
    }
}
