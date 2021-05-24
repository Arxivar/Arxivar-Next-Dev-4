using System;
using System.IO;
using System.Text;
using Abletech.Arxivar.Entities;
using Abletech.Arxivar.Server.Contracts;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ProgettoServer
{
    public class PluginServerEsempioCorso : Abletech.Arxivar.Server.PlugIn.AbstractPlugin
    {
        public override string Name
        {
            get { return "Plugin Server - Esempio Corso"; }
        }

        public override string Version
        {
            get { return "Beta 1"; }
        }

        private string _workingDirectory;

        public string GetAuditFilePath()
        {
            return Path.Combine(_workingDirectory, "Audit.txt");
        }

        public override void On_Services_Started()
        {
            // Creo la cartella di lavoro
            _workingDirectory = @"c:\temp\CorsoDev";

            if (!Directory.Exists(_workingDirectory))
            {
                Directory.CreateDirectory(_workingDirectory);
            }

            base.On_Services_Started();
        }

        public override System.IO.Stream On_Rest_Command(System.Collections.Specialized.NameValueCollection nameValueCollection)
        {
            // Per vedere che il servizio rest funziona
            // return rest_ReturnString("Hello Rest");
            using (Abletech.Arxivar.Client.WCFConnectorManager manager = new Abletech.Arxivar.Client.WCFConnectorManager(ARX_Push, new ArxLogonRequest
            {
                ClientId = "NEXTDEV",
                ClientSecret = "5A36511B135D4A0B",
                Username = "Admin",
                Password = "123"
            }))
            {
                string action = nameValueCollection["ACTION"];
                switch (action)
                {
                    case "DOWNLOAD":
                        {
                            int docNumber = System.Convert.ToInt32(nameValueCollection["DOCNUMBER"]);
                            var file = manager.ARX_DOCUMENTI.Dm_Profile_GetDocument(docNumber);
                            return rest_ReturnFile(file.FileName, file.ToMemoryStream());

                        }
                    case "INFOPROFILE":
                        {

                            int docNumber = System.Convert.ToInt32(nameValueCollection["DOCNUMBER"]);
                            using (var profilo = manager.ARX_DATI.Dm_Profile_GetData_By_DocNumber(docNumber))
                            {
                                string html = @"
                                                <table border=1>
                                                    <CAPTION><EM>ARXivar Plugin Informazioni di profilo</EM></CAPTION>
                                                    <tr>
                                                        <td>AOO</td>
                                                        <td>" + profilo.AOO + @"</td>
                                                    </tr>
                                                    <tr>
                                                        <td>OGGETTO</td>
                                                        <td>" + profilo.DOCNAME + @"</td>
                                                    </tr>
                                                </table>";
                                return rest_ReturnString(html);
                            }
                        }
                    default:
                        {

                            var search = new Abletech.Arxivar.Entities.Search.Dm_Profile_Search();
                            var select = new Abletech.Arxivar.Entities.Search.Dm_Profile_Select();
                            select.DOCNUMBER.Selected = true;
                            select.DOCNAME.Selected = true;
                            var datasource = manager.ARX_SEARCH.Dm_Profile_GetData(search, select, 10);

                            string html = @"
                                            <table border=1>
                                            <CAPTION><EM>ARXivar Plugin Rest</EM></CAPTION>
                                            <tr>
                                                <td>DOCNUMBER</td>
                                                <td>DESCRIZIONE</td>
                                                <td colspan=2>COMANDO</td>
                                             </tr>";

                            foreach (System.Data.DataRow row in datasource.GetDataTable(0).Rows)
                            {
                                string formato = @"
                                <tr>
                                    <td>{0}</td>
                                    <td>{1}</td><td><button onclick=""location.href='{2}'"">DOWNLOAD</button></td>
                                    <td><button onclick=""location.href='{3}'"">INFO</button></td>
                                 </tr>";

                                string urlDownload = string.Format(@"{0}?ACTION=DOWNLOAD&DOCNUMBER={1}", ARX_Rest_PluginUrl(), row["DOCNUMBER"]);
                                string urlInfo = string.Format(@"{0}?ACTION=INFOPROFILE&DOCNUMBER={1}", ARX_Rest_PluginUrl(), row["DOCNUMBER"]);

                                html += string.Format(formato, row["DOCNUMBER"], row["DOCNAME"], urlDownload, urlInfo);
                            }

                            html += "</table>";
                            return rest_ReturnString(html);
                        }
                }
            }
        }

        public override Abletech.Arxivar.Entities.Dm_Utenti On_Before_Dm_Utenti_Insert(Abletech.Arxivar.Server.Contracts.BaseRegisteredClient client, Abletech.Arxivar.Entities.Dm_Utenti dmUtenti, Abletech.Arxivar.Entities.ARXCancelEventArgs e)
        {
            if (dmUtenti.EMAIL.IndexOf("@abletech.it") == -1)
            {
                e.Message = "Su questo impianto non è possibile creare utenti senza mail o con mail non Abletech";
                e.Cancel = true;

                // Non voglio inserire nulla
                return null;
            }
            return base.On_Before_Dm_Utenti_Insert(client, dmUtenti, e);
        }

        public override Arx_File On_Before_Dm_Profile_SetDocument(BaseRegisteredClient client, Dm_Profile dmProfile, Arx_File file, bool insertDocument, ARXCancelEventArgs e)
        {
            // RESTRIZIONI:
            // Voglio che per le bolle (DOCUMENTTYPE == 2) il documento debba essere un pdf
            if (dmProfile.DOCUMENTTYPE == 2)
            {
                if (!file.FileName.EndsWith(".pdf", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Mi salvo una copia del file vietato
                    var copyFilePath = Path.Combine(_workingDirectory, file.FileName);

                    if (File.Exists(copyFilePath))
                    {
                        File.Delete(copyFilePath);
                    }

                    File.Copy(file.CurrentFile, copyFilePath);

                    // Riporto l'incidente
                    string logFilePath = GetAuditFilePath();
                    File.AppendAllText(logFilePath, string.Format("{0} SetDocument !!VIETATO!!: {1} [{2} Bytes] Docnumber: {3}/{4} user: {5} SoftwareType: {6} ({7}){8}"
                        , DateTime.Now.ToString("O")
                        , file.FileName
                        , file.FileLength
                        , dmProfile.DOCNUMBER
                        , dmProfile.REVISIONE
                        , client.Utente.ToString()
                        , client.SoftwareType
                        , client.SoftwareName
                        , Environment.NewLine));

                    e.Message = "Il documento deve essere un pdf";
                    e.Cancel = true;
                    return null;
                }
            }

            return base.On_Before_Dm_Profile_SetDocument(client, dmProfile, file, insertDocument, e);
        }

        public override void On_After_Dm_Profile_SetDocument(Abletech.Arxivar.Server.Contracts.BaseRegisteredClient client, Abletech.Arxivar.Entities.Dm_Profile dmProfile, Abletech.Arxivar.Entities.Arx_File file, bool insertDocument)
        {
            // Audit dell'evento di modifica di un documento
            string logFilePath = GetAuditFilePath();
            File.AppendAllText(logFilePath, string.Format("{0} SetDocument: {1} [{2} Bytes] Docnumber: {3}/{4} user: {5} SoftwareType: {6} ({7}){8}"
                , DateTime.Now.ToString("O")
                , file.FileName
                , file.FileLength
                , dmProfile.DOCNUMBER
                , dmProfile.REVISIONE
                , client.Utente.ToString()
                , client.SoftwareType
                , client.SoftwareName
                , Environment.NewLine));

            // Lascio proseguire la logica di default
            base.On_After_Dm_Profile_SetDocument(client, dmProfile, file, insertDocument);
        }

        public override Arx_File On_Dm_Profile_GetDocument(BaseRegisteredClient client, Dm_Profile dmProfile, Arx_File file, ARXCancelEventArgs e)
        {
            // Faccio la trasformazione del documento in uscita per i PDF
            // Audit dell'evento di modifica di un documento
            string logFilePath = GetAuditFilePath();
            File.AppendAllText(logFilePath, string.Format("{0} GetDocument: {1} [{2} Bytes] Docnumber: {3}/{4} user: {5} SoftwareType: {6} ({7}){8}"
                , DateTime.Now.ToString("O")
                , file.FileName
                , file.FileLength
                , dmProfile.DOCNUMBER
                , dmProfile.REVISIONE
                , client.Utente.ToString()
                , client.SoftwareType
                , client.SoftwareName
                , Environment.NewLine));

            // Trasformazione dei documenti PDF per le bolle DOCUMENTTYPE = 2
            if (dmProfile.DOCUMENTTYPE == 2)
            {
                if (file != null && file.FileName.EndsWith(".pdf", StringComparison.CurrentCultureIgnoreCase))
                {
                    string originalFile = file.CurrentFile;
                    string outputWatermarkFileName = Path.GetFileName(file.FileName);
                    string outputWatermarkFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Substring(9) + outputWatermarkFileName);

                    // Voglio creare un barcode con il valore del campo numero del profilo
                    string codeText = dmProfile.NUMERO ?? string.Empty;

                    PdfReader originalFileReader = new PdfReader(originalFile);
                    using (FileStream fs = new FileStream(outputWatermarkFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (PdfStamper stamper = new PdfStamper(originalFileReader, fs))
                    {
                        // https://www.codeproject.com/Questions/865666/Adding-comment-on-pdf-layer-created-using-iTextsha
                        int pageCount = originalFileReader.NumberOfPages;

                        // Create New Layer for Watermark
                        PdfLayer layer = new PdfLayer("WatermarkLayer", stamper.Writer);
                        // Loop through each Page
                        for (int i = pageCount; i <= pageCount; i++)
                        {
                            // Getting the Page Size
                            Rectangle rect = originalFileReader.GetPageSize(i);

                            var posY = rect.Height - 50;
                            var posX = 50;

                            // Get the ContentByte object
                            PdfContentByte cb = stamper.GetUnderContent(i);

                            // Tell the cb that the next commands should be "bound" to this new layer
                            cb.BeginLayer(layer);
                            cb.SetFontAndSize(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 55);

                            PdfGState gState = new PdfGState();
                            cb.SetGState(gState);

                            string codbartest = codeText;
                            BarcodePDF417 bcpdf417 = new BarcodePDF417();
                            //Asigna el código de barras en base64 a la propiedad text del objeto..
                            bcpdf417.Text = ASCIIEncoding.ASCII.GetBytes(codbartest);
                            Image imgpdf417 = bcpdf417.GetImage();
                            imgpdf417.SetAbsolutePosition(posX, posY);
                            imgpdf417.ScalePercent(200);
                            cb.AddImage(imgpdf417);
                            // Close the layer
                            cb.EndLayer();
                        }

                        var pdfArxFile = new Arx_File(outputWatermarkFilePath)
                        {
                            FileName = outputWatermarkFileName
                        };

                        // Restituisco il file elaborato
                        return pdfArxFile;
                    }
                }
            }

            return file;
        }

        private readonly string _settingsFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PluginCorsoImpostazioni.xml");

        public override string On_ClientSentCustomAdvancedCommand(
            Abletech.Arxivar.Server.Contracts.BaseRegisteredClient client,
            string command,
            string parameters)
        {
            switch (command)
            {
                case "Get":
                    {
                        var impostazioni = new LibreriaComune.ClasseImpostazioni();
                        if (System.IO.File.Exists(_settingsFile))
                        {
                            impostazioni = Abletech.Utility.UnicodeConvert.From_String_To_Object(System.IO.File.ReadAllText(_settingsFile), typeof(LibreriaComune.ClasseImpostazioni)) as LibreriaComune.ClasseImpostazioni;
                            if (impostazioni == null)
                            {
                                impostazioni = new LibreriaComune.ClasseImpostazioni();
                            }
                        }


                        return Abletech.Utility.UnicodeConvert.From_Object_To_String(impostazioni, typeof(LibreriaComune.ClasseImpostazioni));
                    }
                case "Set":
                    {
                        var impostazioni = Abletech.Utility.UnicodeConvert.From_String_To_Object(parameters, typeof(LibreriaComune.ClasseImpostazioni)) as LibreriaComune.ClasseImpostazioni;
                        var valoreStringa = Abletech.Utility.UnicodeConvert.From_Object_To_String(impostazioni, typeof(LibreriaComune.ClasseImpostazioni));
                        System.IO.File.WriteAllText(_settingsFile, valoreStringa);
                        return valoreStringa;
                    }

                default:
                    return base.On_ClientSentCustomAdvancedCommand(client, command, parameters);
            }
        }
    }
}