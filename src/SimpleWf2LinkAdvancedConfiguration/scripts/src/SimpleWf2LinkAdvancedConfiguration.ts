/* eslint-disable no-useless-constructor */
import { ConfigurationDataTypeEnum } from "../Interfaces";

const pluginId = '4937397e-00a1-4368-b5d5-897e9722c71c';

export type ModelItem = {
	name: string;	
	value: string | number | boolean | Date;
	placeholder?: string;
}

export type PluginModel = {
	inputTest: ModelItem;	
}

class SimpleWf2LinkAdvancedConfiguration {
	viewerMode: boolean;
	enableSave: ({ enable }: { enable: boolean }) => void;
	configuration: IConfiguration[];
	diagramId: string;
	model: PluginModel;
	readonly saver: { onSave: () => IConfiguration[] };	
	constructor(readonly workflowResourceService: IWorkflowResourceService, readonly _: ILoDash, readonly arxivarRouteService: IArxivarRouteService) {
	}
	
	$onInit(): void {		
		this.model = this.createModel(this.configuration);
		this.saver.onSave = () => {
			return [{ 
				name: this.model.inputTest.name,
				value: this.model.inputTest.value as string,
				dataType: ConfigurationDataTypeEnum.String
			}]};
		}		
		
		$onDestroy(): void {
		}		
		
		createModel(configuration: IConfiguration[]): PluginModel {
			// model con valori di default
			let model: PluginModel = {
				inputTest: {name: 'inputTest'
				, value: 'Initial test value'
				, placeholder: 'Initial test value'
			}			
		};
		// associo la configurazione, se presente
		if (configuration?.length > 0) {
			const items = Object.values(model);
			configuration.forEach(c => {
				items.forEach(m => {
					if (m.name === c.name) {
						m.value = c.value;
					}
				});
			});
			model = items.reduce((acc, curr: ModelItem) => (acc[curr.name] = curr, acc), ({} as PluginModel));
		}
		return model;
	}
	
	executeCommand() {
		const that = this;
		
		// eseguo il comando
		const data = [
			{
				key: this.model.inputTest.name,
				value: this.model.inputTest.value
			}];
			
			this.workflowResourceService.getPost(this.arxivarRouteService.getPartialURLPluginLinkExecuteCommand(pluginId), data, {})
			.then((results: {data: {inputTest: string}}) => {				
				that.model.inputTest.value = results.data.inputTest;	
			});
		}
	}	
	
	
	angular.module('arxivar.pluginoperations')
	.component('4937397e00a14368b5d5897e9722c71c', {
		bindings: {
			configuration: '<',
			enableSave: '&',
			saver: '<',
			viewerMode: '<',
			diagramId: '<'
		},
		controllerAs: 'ctrl',
		controller: ['workflowResourceService','_','arxivarRouteService',SimpleWf2LinkAdvancedConfiguration],
		template: `
		<div ng-include="'4937397e00a14368b5d5897e9722c71c.html'"> 
		</div>
		`
	});
	