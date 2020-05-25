using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Abletech.Arxivar.Entities;
using Abletech.Arxivar.Entities.Libraries;

namespace ProgettoClient
{
    public partial class FormTaskRichiestaFerie : Form
    {
        private Abletech.Arxivar.Client.WCFConnectorManager _manager;
        private int _dmTaskWorkId;
        public FormTaskRichiestaFerie(Abletech.Arxivar.Client.WCFConnectorManager manager, int dmTaskWorkId)
        {
            InitializeComponent();

            _manager = manager;
            _dmTaskWorkId = dmTaskWorkId;

            //recupero il TaskWork (task dell'utente)
            Dm_TaskWork dmTaskWork = manager.ARX_WORKFLOW.Dm_TaskWork_GetData_By_Id(dmTaskWorkId);

            // recupero il codice processo (che è il codice dell'istanza di WF)
            int idProcesso = dmTaskWork.ID_PROCESSO;
            Dm_ProcessDoc[] dmProcessDocList = manager.ARX_WORKFLOW.Dm_ProcessDoc_GetData_By_IdProcesso(idProcesso);


            //recupero l'elenco dei documenti del task
            //if (dmProcessDocList.Length > 0)
            //{
            //    Arx_File file = manager.ARX_DOCUMENTI.Dm_ProcessDoc_GetDocument(dmProcessDocList[0].ID, dmTaskWorkId);
            //}

            GridDocumenti.DataSource = dmProcessDocList;

            Arx_TaskOperations operation = manager.ARX_WORKFLOW.Arx_TaskOperations_GetData_By_DmTaskWorkId(dmTaskWorkId);
            GridOperazioni.DataSource = operation.DmVariabiliProcessoLibs.Select(x => new Arx_KeyValue(x.DmVariabileProcesso.NOME, x.DmVariabileProcesso.ID.ToString())).ToList();

            Arx_TaskChooseAnswer[] esiti = manager.ARX_WORKFLOW.Dm_Taskwork_Get_Esiti_For_Finalize(new int[] { dmTaskWorkId });

            foreach (var esito in esiti)
            {
                ComboEsiti.Items.Add(esito.DmTabelle.VALORE);
            }


            // Esempio.. posso leggere il valori della varibili di processo
            //DmVariabiliProcessoLib[] variabili = manager.ARX_WORKFLOW.Dm_VariabiliProcesso_GetData_By_IdProcesso(idProcesso, "");

            //foreach (var variabile in variabili)
            //{
            //    variabile.DmVariabileProcesso.
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ComboEsiti.SelectedIndex == -1)
                MessageBox.Show("Selezionare l'esito");
            else
            {
                var s = ComboEsiti.SelectedItem.ToString();
                var ex = _manager.ARX_WORKFLOW.Dm_Taskwork_Finalize(new int[] { _dmTaskWorkId }, s, "Eventuale Nota", "", true);
                if (ex.Exception == Abletech.Arxivar.Entities.Enums.Security_Exception.Nothing)
                {
                    MessageBox.Show("Task avanzato correttamente");
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                }
                else
                    MessageBox.Show("Errore: " + ex.Exception + " - " + ex.ErrorMessage);
            }
        }

        private void Apri()
        {
            if (GridDocumenti.SelectedRows.Count != 1)
                return;

            DataGridViewRow r = GridDocumenti.SelectedRows[0];
            if (r == null)
                return;
            Dm_ProcessDoc processDoc = r.DataBoundItem as Dm_ProcessDoc;

            // Carico il documento selezionato specificando il codice del task per poter verificare varie cose fra cui le riservatezze lato server
            Arx_File file = _manager.ARX_DOCUMENTI.Dm_ProcessDoc_GetDocument(processDoc.ID, _dmTaskWorkId);
            file.SaveTo(System.IO.Path.GetTempPath(), file.FileName, true);
            System.Diagnostics.Process.Start(System.IO.Path.Combine(System.IO.Path.GetTempPath(), file.FileName));
        }

        private void GridDocumenti_DoubleClick(object sender, EventArgs e)
        {
            Apri();
        }

        /*
          VARIABILI
         
         var elencoVariabili = manager.ARX_WORKFLOW.Dm_VariabiliProcesso_GetData_By_IdProcesso(idProcesso, "");
            var variabile = elencoVariabili[0];
            var variabileCastInString =  variabile.Value as Aggiuntivo_String;
            if (variabileCastInString != null)
            {
                variabileCastInString.Valore = "Mio Nuovo Valore";
                manager.ARX_WORKFLOW.Dm_VariabiliProcesso_Update_Value(variabile.DmVariabileProcesso.ID, variabileCastInString.Value);
            }
         */
    }
}
