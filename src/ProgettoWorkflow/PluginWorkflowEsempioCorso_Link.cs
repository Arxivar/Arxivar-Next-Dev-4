using System;
using Abletech.Arxivar.Client;
using Abletech.Arxivar.Client.PlugIn.Attributes;
using Abletech.Arxivar.Entities;

namespace ProgettoWorkflow
{
    public class PluginWorkflowEsempioCorso_Link : Abletech.Arxivar.Client.PlugIn.Workflow.AbstractPluginOperationLink
    {
        public PluginWorkflowEsempioCorso_Link(string codice, WCFConnectorManager manager)
            : base(codice, manager)
        {
        }

        public override string Class_Id
        {
            get
            {
                return "318B002A-5391-47CD-892C-074FBB34F53C";

            }
        }

        public override string Descrizione
        {
            get { return "Plugin link esempio"; }
        }

        public override string Nome
        {
            get { return "Plugin link esempio"; }
        }


        #region Proprietà

        [ShowInConfigurator("Numero tentativi", ShowInConfigurator.ParameterDirectionEnum.InputOutput)]
        public int NumeroTentativi { get; set; }

        [ShowInConfigurator("Messaggio", ShowInConfigurator.ParameterDirectionEnum.Output)]
        public string Messaggio { get; set; }

        [ShowInConfigurator("Ultima esecuzione", ShowInConfigurator.ParameterDirectionEnum.Output)]
        public DateTime UltimaEsecuzione { get; set; }



        #endregion

        public override Arx_KeyValue[] Execute(Arx_KeyValue[] keys)
        {
            NumeroTentativi++;
            Messaggio = "Sono il LINK eseguito alle: " + DateTime.Now.ToString();
            UltimaEsecuzione = DateTime.Now;

            return null;
        }
    }
}
