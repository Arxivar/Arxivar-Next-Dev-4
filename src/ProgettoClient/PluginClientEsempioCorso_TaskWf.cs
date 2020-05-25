using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProgettoClient
{
    public class PluginClientEsempioCorso_TaskWf: Abletech.Arxivar.Client.PlugIn.AbstractPluginCmdFrmTask
    {
        public PluginClientEsempioCorso_TaskWf(string codice, Abletech.Arxivar.Client.WCFConnectorManager manager)
            : base(codice, manager)
        {
        }

        public override Abletech.Arxivar.Entities.Arx_KeyValue[] Execute(Abletech.Arxivar.Entities.Arx_KeyValue[] keys)
        {
            using (FormTaskRichiestaFerie form = new FormTaskRichiestaFerie(WcfConnectorManager, DmTaskWorkId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return null;
                }
                
            }
            return null;
        }

        public override string Class_Id
        {
            get { return "FF82D9DD-EE69-4AEB-89A3-0D46BCA6DE26"; }
        }

        public override string Descrizione
        {
            get { return Nome; }
        }

        public override string Nome
        {
            get { return "Plugin Corso - Gestione task"; }
        }

        public override string ExternalId
        {
            get
            {
                return "RICHIESTAFERIE";
            }
        }
    }
}
