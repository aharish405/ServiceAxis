import React, { useState, useCallback, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ReactFlow,
  MiniMap,
  Controls,
  Background,
  useNodesState,
  useEdgesState,
  addEdge,
  Panel,
  MarkerType,
  type Connection,
  type Edge,
  type Node,
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';
import { 
  Play, 
  Save, 
  Plus, 
  Settings, 
  CheckCircle, 
  AlertCircle, 
  Mail, 
  UserCheck, 
  Database,
  ArrowLeft,
  Trash2,
  Code
} from 'lucide-react';
import { workflowService } from '../../services/workflow.service';
import type { WorkflowDefinition, WorkflowStep, WorkflowTransition } from '../../services/workflow.service';

const WorkflowDesigner: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [definition, setDefinition] = useState<WorkflowDefinition | null>(null);
  const [nodes, setNodes, onNodesChange] = useNodesState<Node>([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState<Edge>([]);
  const [selectedNode, setSelectedNode] = useState<Node | null>(null);
  const [selectedEdge, setSelectedEdge] = useState<Edge | null>(null);
  const [loading, setLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  // Load workflow
  useEffect(() => {
    if (id) {
      loadWorkflow(id);
    }
  }, [id]);

  const loadWorkflow = async (workflowId: string) => {
    try {
      setLoading(true);
      const data = await workflowService.getDefinition(workflowId);
      setDefinition(data);

      // Map steps to nodes
      const steps = data?.steps || [];
      const initialNodes: Node[] = steps.map((step, index) => ({
        id: step.id,
        type: 'default',
        data: { label: step.name, step },
        position: { 
          x: step.x || 250, 
          y: step.y || (index * 120 + 50) 
        },
        style: {
          background: step.isInitial ? '#f0fdf4' : step.isTerminal ? '#fef2f2' : '#fff',
          border: step.isInitial ? '2px solid #22c55e' : step.isTerminal ? '2px solid #ef4444' : '1px solid #e2e8f0',
          borderRadius: '12px',
          padding: '12px',
          fontSize: '12px',
          fontWeight: '600',
          width: 180,
          boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)'
        }
      }));

      // Map transitions to edges
      const transitions = data?.transitions || [];
      const initialEdges: Edge[] = transitions.map((trans) => ({
        id: trans.id,
        source: trans.fromStepId,
        target: trans.toStepId,
        label: trans.triggerEvent,
        markerEnd: { type: MarkerType.ArrowClosed },
        style: { strokeWidth: 2 }
      }));

      setNodes(initialNodes);
      setEdges(initialEdges);
    } catch (error) {
      console.error('Failed to load workflow', error);
    } finally {
      setLoading(false);
    }
  };

  const onConnect = useCallback(
    (params: Connection) => setEdges((eds) => addEdge({ 
      ...params, 
      label: 'New Transition',
      markerEnd: { type: MarkerType.ArrowClosed },
      style: { strokeWidth: 2 }
    }, eds)),
    [setEdges]
  );

  const onSave = async () => {
    if (!definition || !id) return;
    setIsSaving(true);
    try {
      // Map current nodes/edges back to DTOs for sync
      const syncSteps = nodes.map((node, index) => {
        const step = node.data.step as any;
        return {
          code: step.code || `STEP_${index}`,
          name: node.data.label as string,
          stepType: step.stepType || 'Manual',
          order: step.order || (index * 10),
          isInitial: step.isInitial || false,
          isTerminal: step.isTerminal || false,
          requiredRole: step.requiredRole,
          configuration: step.configuration,
          x: node.position.x,
          y: node.position.y
        };
      });

      const syncTransitions = edges.map((edge) => {
        const fromNode = nodes.find(n => n.id === edge.source);
        const toNode = nodes.find(n => n.id === edge.target);
        return {
          fromStepCode: (fromNode?.data.step as any).code,
          toStepCode: (toNode?.data.step as any).code,
          triggerEvent: edge.label as string || 'Auto',
          priority: 10
        };
      });

      await workflowService.syncDefinition(id, { steps: syncSteps, transitions: syncTransitions });
      alert('Workflow published successfully!');
      await loadWorkflow(id);
    } catch (err) {
      console.error(err);
      alert('Failed to publish workflow.');
    } finally {
      setIsSaving(false);
    }
  };

  const addStepNode = (type: string) => {
    const newNodeId = `new_step_${Date.now()}`;
    const code = `NEW_${type.toUpperCase()}_${Math.floor(Math.random() * 1000)}`;
    const newNode: Node = {
      id: newNodeId,
      type: 'default',
      data: { 
        label: `New ${type}`, 
        step: { 
          name: `New ${type}`, 
          stepType: type,
          code: code,
          order: nodes.length * 10
        } 
      },
      position: { x: 100, y: 100 },
      style: {
        background: '#fff',
        border: '1px solid #e2e8f0',
        borderRadius: '12px',
        padding: '12px',
        width: 180,
        boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)'
      }
    };
    setNodes((nds) => nds.concat(newNode));
  };

  const updateStepData = (updates: any) => {
    if (!selectedNode) return;
    setNodes((nds) => nds.map((n) => {
      if (n.id === selectedNode.id) {
        const newStep = { ...n.data.step as any, ...updates };
        return { 
          ...n, 
          data: { ...n.data, label: newStep.name, step: newStep },
          style: {
            ...n.style,
            background: newStep.isInitial ? '#f0fdf4' : newStep.isTerminal ? '#fef2f2' : '#fff',
            border: newStep.isInitial ? '2px solid #22c55e' : newStep.isTerminal ? '2px solid #ef4444' : '1px solid #e2e8f0',
          }
        };
      }
      return n;
    }));
    // Sync local selected state
    setSelectedNode(prev => prev ? { ...prev, data: { ...prev.data, label: updates.name || prev.data.label, step: { ...prev.data.step as any, ...updates } } } : null);
  };

  const deleteSelected = () => {
    if (selectedNode) {
      setNodes(nds => nds.filter(n => n.id !== selectedNode.id));
      setEdges(eds => eds.filter(e => e.source !== selectedNode.id && e.target !== selectedNode.id));
      setSelectedNode(null);
    } else if (selectedEdge) {
      setEdges(eds => eds.filter(e => e.id !== selectedEdge.id));
      setSelectedEdge(null);
    }
  };

  if (loading) return <div className="p-8 text-center text-slate-500">Loading Workflow Canvas...</div>;

  const currentStep = selectedNode?.data.step as any;

  return (
    <div className="flex flex-col h-screen bg-slate-50">
      {/* Designer Header */}
      <div className="flex items-center justify-between px-6 py-4 bg-white border-b border-slate-200 shadow-sm z-10">
        <div className="flex items-center space-x-4">
          <button 
            onClick={() => navigate('/app/studio')}
            className="p-2 transition-colors hover:bg-slate-100 rounded-lg text-slate-400 hover:text-slate-600"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <h1 className="text-lg font-black text-slate-900 tracking-tight">{definition?.name}</h1>
            <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">{definition?.category} • v{definition?.version}</p>
          </div>
        </div>
        <div className="flex items-center space-x-3">
          <button 
            onClick={onSave}
            disabled={isSaving}
            className="flex items-center px-6 py-2.5 space-x-2 text-sm font-bold text-white bg-indigo-600 rounded-xl hover:bg-indigo-700 shadow-lg shadow-indigo-100 transition-all disabled:opacity-50"
          >
            <Save className="w-4 h-4" />
            <span>{isSaving ? 'Publishing...' : 'Publish Workflow'}</span>
          </button>
        </div>
      </div>

      <div className="flex flex-1 overflow-hidden">
        {/* Sidebar Palette */}
        <div className="w-80 bg-white border-r border-slate-200 flex flex-col shadow-xl z-10">
          <div className="p-6 border-b border-slate-50">
            <h2 className="mb-4 text-[10px] font-black tracking-widest text-slate-400 uppercase">Step Palette</h2>
            <div className="grid grid-cols-2 gap-3">
              <button 
                onClick={() => addStepNode('Approval')}
                className="flex flex-col items-center p-4 transition-all bg-slate-50 border border-slate-200 rounded-2xl hover:border-indigo-300 hover:bg-white group"
              >
                <div className="p-2 bg-blue-100 rounded-xl text-blue-600 mb-2 group-hover:scale-110 transition-transform">
                  <UserCheck className="w-5 h-5" />
                </div>
                <span className="text-[10px] font-bold text-slate-600">Approval</span>
              </button>

              <button 
                onClick={() => addStepNode('UpdateField')}
                className="flex flex-col items-center p-4 transition-all bg-slate-50 border border-slate-200 rounded-2xl hover:border-emerald-300 hover:bg-white group"
              >
                <div className="p-2 bg-emerald-100 rounded-xl text-emerald-600 mb-2 group-hover:scale-110 transition-transform">
                  <Database className="w-5 h-5" />
                </div>
                <span className="text-[10px] font-bold text-slate-600">Update</span>
              </button>

              <button 
                onClick={() => addStepNode('Condition')}
                className="flex flex-col items-center p-4 transition-all bg-slate-50 border border-slate-200 rounded-2xl hover:border-amber-300 hover:bg-white group"
              >
                <div className="p-2 bg-amber-100 rounded-xl text-amber-600 mb-2 group-hover:scale-110 transition-transform">
                  <AlertCircle className="w-5 h-5" />
                </div>
                <span className="text-[10px] font-bold text-slate-600">Branch</span>
              </button>

              <button 
                onClick={() => addStepNode('Notification')}
                className="flex flex-col items-center p-4 transition-all bg-slate-50 border border-slate-200 rounded-2xl hover:border-indigo-300 hover:bg-white group"
              >
                <div className="p-2 bg-indigo-100 rounded-xl text-indigo-600 mb-2 group-hover:scale-110 transition-transform">
                  <Mail className="w-5 h-5" />
                </div>
                <span className="text-[10px] font-bold text-slate-600">Notify</span>
              </button>
            </div>
          </div>

          <div className="flex-1 overflow-y-auto p-6 scrollbar-hide">
             <div className="flex items-center justify-between mb-4">
                <h2 className="text-[10px] font-black tracking-widest text-slate-400 uppercase">Configuration</h2>
                {(selectedNode || selectedEdge) && (
                   <button onClick={deleteSelected} className="text-rose-500 hover:text-rose-700 transition-colors">
                      <Trash2 className="w-4 h-4" />
                   </button>
                )}
             </div>
             
             {selectedNode ? (
               <div className="space-y-6 animate-in fade-in slide-in-from-right-2">
                 <div>
                   <label className="block text-[10px] font-black text-slate-400 uppercase mb-2">Display Name</label>
                   <input 
                     type="text" 
                     className="w-full px-4 py-2.5 text-sm font-bold border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500 outline-none bg-slate-50"
                     value={currentStep?.name || ''}
                     onChange={(e) => updateStepData({ name: e.target.value })}
                   />
                 </div>
                 <div>
                   <label className="block text-[10px] font-black text-slate-400 uppercase mb-2">Step Code</label>
                   <input 
                     type="text" 
                     className="w-full px-4 py-2.5 text-xs font-mono border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500 outline-none bg-slate-100"
                     value={currentStep?.code || ''}
                     onChange={(e) => updateStepData({ code: e.target.value.toUpperCase().replace(/\s+/g, '_') })}
                   />
                 </div>
                 <div className="grid grid-cols-2 gap-4">
                    <div className="flex items-center space-x-2 p-3 border border-slate-100 rounded-xl bg-slate-50/50">
                       <input type="checkbox" checked={currentStep?.isInitial || false} onChange={e => updateStepData({isInitial: e.target.checked})} className="rounded text-indigo-600" id="isInitial" />
                       <label htmlFor="isInitial" className="text-[10px] font-bold text-slate-600 cursor-pointer">Start Node</label>
                    </div>
                    <div className="flex items-center space-x-2 p-3 border border-slate-100 rounded-xl bg-slate-50/50">
                       <input type="checkbox" checked={currentStep?.isTerminal || false} onChange={e => updateStepData({isTerminal: e.target.checked})} className="rounded text-rose-600" id="isTerminal" />
                       <label htmlFor="isTerminal" className="text-[10px] font-bold text-slate-600 cursor-pointer">End Node</label>
                    </div>
                 </div>
                 <div>
                   <label className="block text-[10px] font-black text-slate-400 uppercase mb-2">Required Human Role</label>
                   <select 
                     className="w-full px-3 py-2.5 text-xs font-semibold border border-slate-200 rounded-xl outline-none bg-slate-50"
                     value={currentStep?.requiredRole || ''}
                     onChange={e => updateStepData({ requiredRole: e.target.value })}
                   >
                      <option value="">-- No User Interaction --</option>
                      <option value="Manager">Department Manager</option>
                      <option value="Admin">System Admin</option>
                      <option value="Agent">Support Agent</option>
                   </select>
                 </div>
                 <div>
                    <div className="flex items-center justify-between mb-2">
                       <label className="block text-[10px] font-black text-slate-400 uppercase">Step Config (JSON)</label>
                       <Code className="w-3 h-3 text-slate-400" />
                    </div>
                    <textarea 
                      rows={6}
                      className="w-full px-4 py-3 text-xs font-mono border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500 outline-none bg-slate-900 text-indigo-300"
                      value={currentStep?.configuration || ''}
                      onChange={e => updateStepData({ configuration: e.target.value })}
                    />
                 </div>
               </div>
             ) : selectedEdge ? (
                <div className="space-y-6 animate-in fade-in slide-in-from-right-2">
                   <div>
                      <label className="block text-[10px] font-black text-slate-400 uppercase mb-2">Trigger Event</label>
                      <input 
                        type="text" 
                        className="w-full px-4 py-2.5 text-sm font-bold border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500 outline-none bg-slate-50"
                        value={selectedEdge.label as string || ''}
                        onChange={(e) => {
                           const newLabel = e.target.value;
                           setEdges(eds => eds.map(edge => edge.id === selectedEdge.id ? { ...edge, label: newLabel } : edge));
                           setSelectedEdge(prev => prev ? { ...prev, label: newLabel } : null);
                        }}
                      />
                   </div>
                </div>
             ) : (
               <div className="h-40 flex flex-col items-center justify-center text-center opacity-40">
                  <Settings className="w-8 h-8 mb-2 text-slate-400 animate-spin-slow" />
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Global Graph Mode</p>
               </div>
             )}
          </div>
        </div>

        {/* Canvas Area */}
        <div className="flex-1 relative bg-slate-100" style={{ height: 'calc(100vh - 60px)', width: '100%' }}>
          <ReactFlow
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            onNodeClick={(_, node) => { setSelectedNode(node); setSelectedEdge(null); }}
            onEdgeClick={(_, edge) => { setSelectedEdge(edge); setSelectedNode(null); }}
            onPaneClick={() => { setSelectedNode(null); setSelectedEdge(null); }}
            fitView
          >
            <Background color="#cbd5e1" gap={20} />
            <Controls />
            <MiniMap zoomable pannable nodeStrokeColor={(n) => n.id === selectedNode?.id ? '#6366f1' : '#e2e8f0'} />
            <Panel position="top-right" className="bg-white p-2 rounded-2xl shadow-xl border border-slate-200 flex space-x-2">
              <button onClick={() => window.open(`/api/workflow-preview/${id}`, '_blank')} className="p-2 hover:bg-slate-50 text-slate-600 rounded-xl transition-colors">
                <Play className="w-4 h-4" />
              </button>
            </Panel>
            <Panel position="bottom-left" className="bg-white/80 backdrop-blur px-4 py-2 rounded-full border border-slate-200 shadow-sm">
               <span className="text-[10px] font-black text-slate-500 uppercase tracking-tighter">
                  {nodes.length} Steps • {edges.length} Transitions
               </span>
            </Panel>
          </ReactFlow>
        </div>
      </div>
    </div>
  );
};

export default WorkflowDesigner;
