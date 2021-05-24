using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Abletech.Arxivar.Client;
using Abletech.Arxivar.Client.PlugIn.Attributes;
using Abletech.Arxivar.Entities;

namespace ProgettoWorkflow
{
    public class PluginWorkflowEsempioCorso_Task : Abletech.Arxivar.Client.PlugIn.Workflow.AbstractPluginOperationTask
    {
        public PluginWorkflowEsempioCorso_Task(string codice, WCFConnectorManager manager)
            : base(codice, manager)
        {
        }

        public override string Class_Id
        {
            get { return "B1FAAB36-9276-4CDC-8BAA-C08E49A09A3F"; }
        }

        public override string Descrizione
        {
            get { return "Plugin task esempio"; }
        }

        public override string Nome
        {
            get { return "Plugin task esempio"; }
        }


        #region Proprietà

        [ShowInConfigurator("Numero tentativi", ShowInConfigurator.ParameterDirectionEnum.InputOutput)]
        public int NumeroTentativi { get; set; }

        [ShowInConfigurator("Messaggio", ShowInConfigurator.ParameterDirectionEnum.Output)]
        public string Messaggio { get; set; }

        [ShowInConfigurator("Ultima esecuzione", ShowInConfigurator.ParameterDirectionEnum.Output)]
        public DateTime UltimaEsecuzione { get; set; }

        [ShowInConfigurator("Protocollo interno", ShowInConfigurator.ParameterDirectionEnum.InputOutput)]
        public string ProtocolloInterno { get; set; }

        #endregion

        public override Arx_KeyValue[] Execute(Arx_KeyValue[] keys)
        {
            FormTask formTask = new FormTask(ProtocolloInterno);
            var formResult = formTask.ShowDialog();

            if (formResult == DialogResult.OK)
            {
                // Mi prendo il valore del protocollo solo se OK
                ProtocolloInterno = formTask.ProtocolloInterno;
            }
            
            NumeroTentativi++;
            Messaggio = "Sono il TASK eseguito alle: " + DateTime.Now.ToString();
            UltimaEsecuzione = DateTime.Now;
            
            return null;
        }
    }
}
