using System;
using System.Text;
using Abletech.Arxivar.Entities;
using Abletech.Arxivar.Entities.Libraries;

namespace ProgettoClient
{
    public class PluginClientEsempioCorso_Evento : Abletech.Arxivar.Client.PlugIn.AbstractPluginEvents
    {
        public PluginClientEsempioCorso_Evento(string codice, Abletech.Arxivar.Client.WCFConnectorManager manager)
            : base(codice, manager)
        {
        }

        public override Abletech.Arxivar.Entities.Arx_KeyValue[] Execute(Abletech.Arxivar.Entities.Arx_KeyValue[] keys)
        {
            throw new NotImplementedException();
        }

        public override string Class_Id
        {
            get { return "975EB855-F766-4938-8AB5-B3BB35193453"; }
        }

        public override string Descrizione
        {
            get { return Nome; }
        }

        public override string Nome
        {
            get { return "Plugin Corso - Gestione eventi client"; }
        }

        #region eventi barcode
        public override bool On_Before_Dm_Barcode_MatchProfile(EntitiesCollection<int> dmBarcodeIds)
        {
            foreach (var dmBarcodeId in dmBarcodeIds)
            {
                Dm_Barcode dmBarcode = WcfConnectorManager.ARX_DATI.Dm_Barcode_GetData_By_IdBarcode(dmBarcodeId);
                Dm_Profile_ForUpdate dmProfileForUpdateGetNewInstance = WcfConnectorManager.ARX_DATI.Dm_Profile_ForUpdate_GetNewInstance(dmBarcode.DOCNUMBER);
                
                // Posso fare l'aggiornamento del profilo con il valore relativo al barcode
                WcfConnectorManager.ARX_DATI.Dm_Profile_Update(dmProfileForUpdateGetNewInstance, "");
            }
            return true;
        }

        public override void On_After_Dm_Barcode_MatchProfile(EntitiesCollection<int> dmBarcodeIds)
        {
            // Dopo aver eseguito il match del barcode
            base.On_After_Dm_Barcode_MatchProfile(dmBarcodeIds);
        }

        #endregion

        // Dopo la profilazione del modello, appena prima che venga aperto per l'editazione
        public override Arx_File On_DmProfile_OpenDocument_From_DmModuliOffice(int dmModuliOffice, int dmProfile, Arx_File arxFile)
        {
            if (arxFile != null && System.IO.Path.GetExtension(arxFile.CurrentFile) == ".txt")
            {
                var profile = WcfConnectorManager.ARX_DATI.Dm_Profile_GetData_By_DocNumber(dmProfile);

                var sb = new StringBuilder();
                sb.AppendLine("MODELLO COMPILATO IN MODO CUSTOM");
                sb.AppendLine("Docnumber: " + profile.DOCNUMBER);
                sb.AppendLine("Oggetto: " + profile.DOCNAME);
                sb.AppendLine("Generato il: " + DateTime.Now.ToString("G"));

                byte[] modelContent = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                var modelArxFile = new Arx_File(modelContent, "CustomModel.txt", DateTime.Now);

                return modelArxFile;
            }

            return base.On_DmProfile_OpenDocument_From_DmModuliOffice(dmModuliOffice, dmModuliOffice, arxFile);
        }
    }
}
