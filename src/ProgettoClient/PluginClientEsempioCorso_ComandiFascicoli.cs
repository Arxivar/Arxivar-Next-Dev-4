using System;
using System.Linq;
using System.Windows.Forms;
using Abletech.Arxivar.Entities;
using Abletech.Arxivar.Entities.Enums;
using Abletech.Arxivar.Entities.Search;

namespace ProgettoClient
{
    public class PluginClientEsempioCorso_ComandiFascicoli : Abletech.Arxivar.Client.PlugIn.AbstractPluginTaskPanelFolders
    {
        public PluginClientEsempioCorso_ComandiFascicoli(string codice, Abletech.Arxivar.Client.WCFConnectorManager manager)
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
            Dm_Fascicoli fascicolo = WcfConnectorManager.ARX_DATI.Dm_Fascicoli_GetData_ById(Ids[0]);
            Dm_FileInFolder[] collezione = WcfConnectorManager.ARX_DATI.Dm_FileInFolder_GetData_ByFolder(Ids[0]);
            
            if(collezione == null || !collezione.Any())
            {
                MessageBox.Show("Il fascicolo è vuoto");
                return null;
            }

            var select = new Dm_Profile_Select();
            select.DOCNUMBER.Selected = true;
            select.DOCNAME.Selected = true;
            select.ORIGINALE.Selected = true;
            select.DATADOC.Selected = true;
            select.CREATION_DATE.Selected = true;
            
            var search = new Dm_Profile_Search();

            var docnumberList = string.Join(";", collezione.Select(x => x.DOCNUMBER.ToString()).ToArray());

            search.DocNumber.SetFilterMultiple(Dm_Base_Search_Operatore_Numerico.Uguale, docnumberList);

            Arx_DataSource searchResult = WcfConnectorManager.ARX_SEARCH.Dm_Profile_GetData(search, select, 0);
            
            FormDocumentiFascicolo formDocumentiFascicolo = new FormDocumentiFascicolo(fascicolo,searchResult,WcfConnectorManager);
            var dialogResult = formDocumentiFascicolo.ShowDialog();

            return null;
        }

        public override string Class_Id
        {
            get { return "7A25FC9A-46D2-4296-988E-DC4CBA147570"; }
        }

        public override string Descrizione
        {
            get { return Label; }
        }

        public override string Nome
        {
            get { return "Plugin Corso Lista comandi fascicolo"; }
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
