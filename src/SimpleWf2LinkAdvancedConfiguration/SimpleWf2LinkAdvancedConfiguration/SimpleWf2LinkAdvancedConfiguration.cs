using Abletech.Workflow.Plugins.Attributes;
using Abletech.Workflow.Plugins.Link;
using Abletech.Workflow.Plugins.Services;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abletech.Workflow.Plugins;
using Abletech.Workflow.Plugins.Configuration;
using System.Linq;

namespace SimpleWf2LinkAdvancedConfiguration
{
    [Plugin("4937397e-00a1-4368-b5d5-897e9722c71c", "Simple Workflow V2 link plugin Advanced configuration", "1.1.0", Description = "Simple Workflow V2 link plugin advanced configuration", Icon = "far fa-user-check", UseAdvancedConfiguration = true)]
    public class SimpleWf2LinkAdvancedConfiguration : WorkflowPluginLink, IWorkflowPlugin
    {
        const string InputTestName = "inputTest";

        /// <summary>
        /// string type output parameter named Outcome
        /// </summary>
        [OutputParameter(DisplayName = "Outcome message", Description = "Outcome message")]
        public string OutcomeMessage { get; set; }

        /// <summary>
        /// DateTime type output parameter named LastExecution
        /// </summary>
        [OutputParameter(DisplayName = "LastExecution", Description = "Please insert a description")]
        public DateTime LastExecution { get; set; }


        [InputParameter(DisplayName = "ExecutionCount", Description = "Execution counter input", DisplayOrder = 0)]
        [OutputParameter(DisplayName = "ExecutionCount", Description = "Execution counter output")]
        public int ExecutionCount { get; set; }

        /// <summary>
        /// Variable from advanced configuration
        /// </summary>
        private string _inputTest;

        [Injected]
        public Abletech.WebApi.Client.Arxivar.Client.Configuration MyConfiguration { get; set; }

        [Injected]
        public Abletech.WebApi.Client.ArxivarManagement.Client.Configuration MyManagementConfiguration { get; set; }

        [Injected]
        public Abletech.WebApi.Client.ArxivarWorkflow.Client.Configuration MyWorkFlowConfiguration { get; set; }

        [Injected]
        public IAuthProvider MyIAuthProvider { get; set; }


        /// <summary>
        /// Plugin is initialized
        /// </summary>
        /// <returns></returns>
        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        /// <summary>
        /// Validating logic
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ValidationResult> OnValidate()
        {
            return base.OnValidate();
        }

        /// <summary>
		/// Load the advanced configuration
		/// </summary>
		/// <param name="configurationItems"></param>
		public void EnableAdvancedConfiguration(IEnumerable<WorkflowPluginConfigurationItem> configurationItems)
        {
            if (configurationItems == null || !configurationItems.Any())
            {
                throw new ArgumentException($"No parameters provided for the advanced configuration");
            }

            var confDictionary = configurationItems.ToDictionary(x => x.Name, y => y.GetValue(), StringComparer.OrdinalIgnoreCase);

            if (!confDictionary.ContainsKey(InputTestName))
                throw new ArgumentException($"Missing {InputTestName }");

            _inputTest = confDictionary[InputTestName].ToString();
        }

        /// <summary>
        /// Receives commands from frontend advanced configuration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public override Task<WorkflowAdvancedConfigurationCommandResponse> ExecuteAdvancedConfigurationCommandAsync(WorkflowAdvancedConfigurationCommandRequest request)
        {

            if (!request.Content.ContainsKey(InputTestName))
                throw new ArgumentException($"Missing {InputTestName }");

            var inputTestValue = request.Content[InputTestName].ToString();

            var validatedInputTest = $"Validated: {inputTestValue}";

            return Task.FromResult(new WorkflowAdvancedConfigurationCommandResponse
            {
                Result = new Dictionary<string, object> { { InputTestName, validatedInputTest } }
            });
        }

        /// <summary>
        /// Execute logic
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task ExecuteAsync(WorkflowPluginLinkContext context)
        {
            try
            {
                ExecutionCount++;
                OutcomeMessage = $"Plugin advanced configuration: {_inputTest} [{ExecutionCount}]";
            }
            catch (Exception ex)
            {
                OutcomeMessage = $"Error: {ex.Message}";
            }
            finally
            {
                LastExecution = DateTime.Now;
            }
            
            return Task.CompletedTask;
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
