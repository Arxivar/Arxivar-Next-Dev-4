using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProgettoClient
{
    public class PluginClientEsempioCorso_ComandiProfili : Abletech.Arxivar.Client.PlugIn.AbstractPluginTaskPanelDocuments
    {
        public PluginClientEsempioCorso_ComandiProfili(string codice, Abletech.Arxivar.Client.WCFConnectorManager manager)
            : base(codice, manager)
        {
        }

        public override string Label
        {
            get { return "Avvio procedura Plugin Corso"; }
        }

        public override string ToolTipText
        {
            get { return "Cliccare qui per avviare la procedura"; }
        }

        public override Abletech.Arxivar.Entities.Arx_KeyValue[] Execute(Abletech.Arxivar.Entities.Arx_KeyValue[] keys)
        {
            LibreriaComune.ClasseImpostazioni impostazioni = new LibreriaComune.ClasseImpostazioni();
            foreach (var pluginsLoaded in WcfConnectorManager.PluginsLoaded)
            {
                if (pluginsLoaded.DllName == "ProgettoServer" && pluginsLoaded.ClassName == "ProgettoServer.PluginServerEsempioCorso")
                {
                    string valore = WcfConnectorManager.ARX_DATI.SendAdvancedCommandToServerPlugin(pluginsLoaded.RuntimeId, "Get", "");
                    if (!string.IsNullOrEmpty(valore))
                        impostazioni = Abletech.Utility.UnicodeConvert.From_String_To_Object(valore, typeof(LibreriaComune.ClasseImpostazioni)) as LibreriaComune.ClasseImpostazioni;
                }
            }

            // Reset della progress bar
            RaiseResetProgress();

            int ciclo = 0;
            int count = Ids.Count;
            foreach (int docNumber in Ids)
            {
                ciclo++;

                var percentuale = (int)((float)ciclo / (float)(count) * 100);

                RaiseUpdateProgress(percentuale, "Aggiornamento profilo: " + docNumber);

                using (var dmProfileForUpdate = WcfConnectorManager.ARX_DATI.Dm_Profile_ForUpdate_GetNewInstance(docNumber))
                {
                    dmProfileForUpdate.DocName = string.Format("{0}: {1}", "Aggiornamento da plugin", impostazioni.Etichetta);

                    var result = WcfConnectorManager.ARX_DATI.Dm_Profile_Update(dmProfileForUpdate, "");
                    if (result.EXCEPTION != Abletech.Arxivar.Entities.Enums.Security_Exception.Nothing)
                    {
                        MessageBox.Show("Errore durante l'aggiornamento del profilo " + docNumber + ": " + result.MESSAGE, Label, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }

            RaiseResetProgress();
            RaiseRefresh();

            // Comportamento classe base
            return null;
        }

        public override string Class_Id
        {
            get { return "129B353E-04BC-43E6-9914-27619DD30022"; }
        }

        public override string Descrizione
        {
            get { return Label; }
        }

        public override string Nome
        {
            get { return "Plugin Corso Lista comandi profilo"; }
        }

        public override System.Drawing.Image GetIcon()
        {
            return Immagini.flash;
        }

        public override bool Enabled
        {
            get
            {
                return Ids != null && Ids.Count > 0;
            }
        }

        public override bool Visible
        {
            get { return base.Visible; }
        }
    }
}
