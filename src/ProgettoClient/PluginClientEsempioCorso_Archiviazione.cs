using ProgettoWorkflow;
using System.Windows.Forms;

namespace ProgettoClient
{
    public class PluginClientEsempioCorso_Archiviazione : Abletech.Arxivar.Client.PlugIn.AbstractPluginRibbonArchiviazione
    {
        public PluginClientEsempioCorso_Archiviazione(string codice, Abletech.Arxivar.Client.WCFConnectorManager manager)
            : base(codice, manager)
        {
        }

        public override string Label
        {
            get { return "Protocollo interno"; }
        }

        public override string ToolTipText
        {
            get { return "Gestione del protocollo interno da gestionale"; }
        }

        public override Abletech.Arxivar.Entities.Arx_KeyValue[] Execute(Abletech.Arxivar.Entities.Arx_KeyValue[] keys)
        {
            var impostazioni = new LibreriaComune.ClasseImpostazioni();
            foreach (var pluginsLoaded in WcfConnectorManager.PluginsLoaded)
            {
                if (pluginsLoaded.DllName == "ProgettoServer" && pluginsLoaded.ClassName == "ProgettoServer.PluginServerEsempioCorso")
                {
                    string valore = WcfConnectorManager.ARX_DATI.SendAdvancedCommandToServerPlugin(pluginsLoaded.RuntimeId, "Get", "");
                    if (!string.IsNullOrEmpty(valore))
                        impostazioni = Abletech.Utility.UnicodeConvert.From_String_To_Object(valore, typeof(LibreriaComune.ClasseImpostazioni)) as LibreriaComune.ClasseImpostazioni;
                }
            }

            var archiviazioneForm = new FormArchiviazione(this.ProfileInsert, impostazioni);
            var dialogResult = archiviazioneForm.ShowDialog();

            // Interfaccia di base -> non restituisce nulla
            return null;
        }

        public override string Class_Id
        {
            get { return "966FF072-2EE9-4EA5-B428-75EA7DDAC4A2"; }
        }

        public override string Descrizione
        {
            get { return Label; }
        }

        public override string Nome
        {
            get { return "Plugin Import data"; }
        }

        public override System.Drawing.Image GetIcon()
        {
            return Immagini.gear;
        }

        public override bool Enabled
        {
            get { return base.Enabled; }
        }

        public override bool Visible
        {
            get
            {
                return
                    ProfileInsert != null &&
                    ProfileInsert.DmMask != null &&
                    ProfileInsert.DmMask.EXTERNAL_ID == "CORSOARXDEV";
            }
        }
    }
}
