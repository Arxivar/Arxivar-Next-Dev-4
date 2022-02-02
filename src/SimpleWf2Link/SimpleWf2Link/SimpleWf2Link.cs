using Abletech.Workflow.Plugins.Attributes;
using Abletech.Workflow.Plugins.Link;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimpleWf2Link
{
    /// <summary>
    /// Simple workflow V2 link plugin
    /// </summary>
    [Plugin("f36b697b-bb07-4e61-a79f-689aee648c46", "Simple Workflow V2 link plugin", "1.2.0", Description = "Simple Workflow V2 link plugin", Icon = "far fa-truck", UseAdvancedConfiguration = false)]
    public class SimpleWf2Link : WorkflowPluginLink
    {
        [InputParameter(DisplayName = "ExecutionCount", Description = "Execution counter input", DisplayOrder = 0)]
        [OutputParameter(DisplayName = "ExecutionCount", Description = "Execution counter output")]
        public int ExecutionCount { get; set; }

        [InputParameter(DisplayName = "InputMessage", Description = "Input message", DisplayOrder = 1)]
        public string InputMessage { get; set; }

        [OutputParameter(DisplayName = "Succeeded", Description = "Succeeded")]
        public bool Succeeded { get; set; }

        [OutputParameter(DisplayName = "Last Execution", Description = "Last execution time")]
        public DateTime LastExecution { get; set; }

        [OutputParameter(DisplayName = "Error Message", Description = "Error message")]
        public string ErrorMessage { get; set; }


        [Injected] public Abletech.WebApi.Client.Arxivar.Client.Configuration MyConfiguration { get; set; }

        [Injected] public Abletech.WebApi.Client.ArxivarManagement.Client.Configuration MyManagementConfiguration { get; set; }

        [Injected] public Abletech.WebApi.Client.ArxivarWorkflow.Client.Configuration MyWorkFlowConfiguration { get; set; }

        [Injected] public Abletech.Workflow.Plugins.Services.IMongoDbProvider MongoDbProvider { get; set; }

        [Injected] public Abletech.Workflow.Plugins.Services.IAuthProvider MyIAuthProvider { get; set; }

        [Injected] public Abletech.WebApi.Client.ArxivarWorkflow.Api.IProcessesApi MyProcessApi { get; set; }

        [Injected] public Abletech.WebApi.Client.ArxivarWorkflow.Api.IProcessDocumentsApi MyProcessDocumentApi { get; set; }

        /// <summary>
        /// Plugin is initialized
        /// </summary>
        /// <returns></returns>
        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        /// <summary>
        /// Validate
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ValidationResult> OnValidate()
        {
            List<ValidationResult> validationResult = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(InputMessage))
            {
                validationResult.Add(new ValidationResult("InputMessage is null"));
            }

            return validationResult;
        }

        /// <summary>
        /// Plugin logic
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public override async Task ExecuteAsync(WorkflowPluginLinkContext context)
        {
            try
            {
                Logger.Information("Simple Workflow V2 link plugin");
                // Increment execution counter
                ExecutionCount++;

                // Set authorization bearer token
                MyProcessDocumentApi.Configuration.DefaultHeader.Add("Authorization", $"Bearer {MyIAuthProvider.AccessToken}");
                MyProcessApi.Configuration.DefaultHeader.Add("Authorization", $"Bearer {MyIAuthProvider.AccessToken}");

                System.Collections.Generic.List<Abletech.WebApi.Client.ArxivarWorkflow.Model.ProcessDocumentForDashboardRm> documentList = await MyProcessApi.ApiV1ProcessesProcessIdDocumentsGetAsync(context.Process.Id);

                var primaryDocumentInfo = documentList.FirstOrDefault(x => x.DocumentKind == 0);

                if (primaryDocumentInfo == null)
                {
                    throw new InvalidOperationException("Unable to find primary document");
                }

                if (!string.Equals(Path.GetExtension(primaryDocumentInfo.Filename), ".txt", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new InvalidOperationException("Primary document is not a text file");
                }

                var primaryDocumentFileStream = (await MyProcessDocumentApi.ApiV1ProcessDocumentsProcessDocIdGetAsync(primaryDocumentInfo.Id)) as FileStream;

                if (primaryDocumentFileStream == null)
                    throw new InvalidCastException("primaryDocumentFileStream is not a FileStream");

                var primaryDocumentFileStreamFileName = primaryDocumentFileStream.Name;

                Logger.Information("Plugin Retrieved privary document: {PrimaryDocumentFileStreamFileName}", primaryDocumentFileStreamFileName);

                await primaryDocumentFileStream.DisposeAsync();

                // Modify primary document
                var content = $"{Environment.NewLine}{DateTime.Now} Plugin Link: {InputMessage} [{ExecutionCount}]";
                Logger.Information("Plugin add content to primary document: {Content}", content);

                await File.AppendAllTextAsync(primaryDocumentFileStreamFileName, content);

                await using (Stream modifiedPrimaryDocument = File.OpenRead(primaryDocumentFileStreamFileName))
                {
                    var updateResult = await MyProcessDocumentApi.ApiV1ProcessDocumentsCheckInProcessDocIdPostAsync(0, primaryDocumentInfo.Id, modifiedPrimaryDocument);

                    if (updateResult == null || !updateResult.Value)
                    {
                        throw new InvalidOperationException("Unable to update primary document");
                    }
                }

                File.Delete(primaryDocumentFileStreamFileName);

                ErrorMessage = null;
                Succeeded = true;

                Logger.Information("Simple Workflow V2 link plugin -> completed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Simple Workflow V2 link plugin error: {ErrorMessage}", ex.Message);
                Succeeded = false;
                ErrorMessage = ex.Message;
            }
            finally
            {
                LastExecution = DateTime.Now;
            }
        }

        /// <summary>
        /// Free resources
        /// </summary>
        protected override void OnDisposing()
        {
            base.OnDisposing();
        }
    }
}