using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Abletech.Arxivar.Client;
using Abletech.Arxivar.Entities;

namespace ProgettoClient
{
    public class PluginClientEsempioCorso_RibbonMain : Abletech.Arxivar.Client.PlugIn.AbstractPluginRibbonMain
    {
        public PluginClientEsempioCorso_RibbonMain(string codice, Abletech.Arxivar.Client.WCFConnectorManager manager)
            : base(codice, manager)
        {
        }

        public override string Label
        {
            get { return "Plugin Esempio Corso"; }
        }

        public override string ToolTipText
        {
            get
            {
                return "Cliccare qui per avviare il Plugin Esempio Corso";
            }
        }

        public override Abletech.Arxivar.Entities.Arx_KeyValue[] Execute(Abletech.Arxivar.Entities.Arx_KeyValue[] keys)
        {
            Abletech.Arxivar.Entities.ArxServerPluginsLoaded plugin = null;
            foreach (Abletech.Arxivar.Entities.ArxServerPluginsLoaded pluginsLoaded in WcfConnectorManager.PluginsLoaded)
            {
                if (pluginsLoaded.DllName == "ProgettoServer" && pluginsLoaded.ClassName == "ProgettoServer.PluginServerEsempioCorso")
                {
                    plugin = pluginsLoaded;
                    break;

                }
            }

            if (plugin == null)
            {
                MessageBox.Show("Nessun plugin server trovato!", Label, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            LibreriaComune.ClasseImpostazioni impostazioni = new LibreriaComune.ClasseImpostazioni();
            string valore = WcfConnectorManager.ARX_DATI.SendAdvancedCommandToServerPlugin(plugin.RuntimeId, "Get", "");
            if (!string.IsNullOrEmpty(valore))
                impostazioni = Abletech.Utility.UnicodeConvert.From_String_To_Object(valore, typeof(LibreriaComune.ClasseImpostazioni)) as LibreriaComune.ClasseImpostazioni;

            using (FormConfigurazioneImpostazioni form = new FormConfigurazioneImpostazioni(impostazioni))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    valore = Abletech.Utility.UnicodeConvert.From_Object_To_String(impostazioni, typeof(LibreriaComune.ClasseImpostazioni));
                    WcfConnectorManager.ARX_DATI.SendAdvancedCommandToServerPlugin(plugin.RuntimeId, "Set", valore);
                }
            }

            return null;
        }

        public override string Class_Id
        {
            get { return "D70579EB274547A08B1791E68D919126"; }
        }

        public override string Descrizione
        {
            get { return Label; }
        }

        public override string Nome
        {
            get { return Label; }
        }

        public override System.Drawing.Image GetIcon()
        {
            return Immaginette.address_book2;
        }

        public override bool Enabled
        {
            get { return WcfConnectorManager.ARX_SECURITY.IsAdmin(); }
        }

        public override bool Visible
        {
            get
            {
                // Il plugin è visibile solo in orario di lavoro
                return DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 18;
            }
        }
    }
}
