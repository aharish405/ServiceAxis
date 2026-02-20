import { api } from './api';

export interface WorkflowDefinition {
  id: string;
  code: string;
  name: string;
  description?: string;
  category: string;
  version: number;
  isPublished: boolean;
  steps: WorkflowStep[];
  transitions: WorkflowTransition[];
}

export interface WorkflowStep {
  id: string;
  definitionId: string;
  code: string;
  name: string;
  stepType: string;
  order: number;
  isInitial: boolean;
  isTerminal: boolean;
  requiredRole?: string;
  configuration?: string;
  x?: number;
  y?: number;
}

export interface WorkflowTransition {
  id: string;
  definitionId: string;
  fromStepId: string;
  toStepId: string;
  triggerEvent: string;
  condition?: string;
  priority: number;
}

export const workflowService = {
  getDefinitions: async () => {
    return await api.get('/workflowdefinitions');
  },

  getDefinition: async (id: string): Promise<WorkflowDefinition> => {
    return await api.get(`/workflowdefinitions/${id}`);
  },

  createDefinition: async (data: Partial<WorkflowDefinition>) => {
    return await api.post('/workflowdefinitions', data);
  },

  addStep: async (definitionId: string, step: Partial<WorkflowStep>) => {
    return await api.post(`/workflowdefinitions/${definitionId}/steps`, step);
  },

  updateStep: async (definitionId: string, stepId: string, step: Partial<WorkflowStep>) => {
    return await api.put(`/workflowdefinitions/${definitionId}/steps/${stepId}`, step);
  },

  addTransition: async (definitionId: string, transition: Partial<WorkflowTransition>) => {
    return await api.post(`/workflowdefinitions/${definitionId}/transitions`, transition);
  },

  deleteStep: async (definitionId: string, stepId: string) => {
    await api.delete(`/workflowdefinitions/${definitionId}/steps/${stepId}`);
  },

  deleteTransition: async (definitionId: string, transitionId: string) => {
    await api.delete(`/workflowdefinitions/${definitionId}/transitions/${transitionId}`);
  },

  syncDefinition: async (definitionId: string, data: { steps: any[], transitions: any[] }) => {
    return await api.post(`/workflowdefinitions/${definitionId}/sync`, data);
  }
};
