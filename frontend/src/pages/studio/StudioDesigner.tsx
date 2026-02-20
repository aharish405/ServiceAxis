import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { DynamicFormRenderer } from '../../components/dynamic/DynamicFormRenderer';
import { UiRuleDesigner } from './UiRuleDesigner';
import { api } from '../../services/api';

interface SysFieldDto {
  id: string;
  fieldName: string;
  displayName: string;
  dataType: string;
  isRequired: boolean;
  defaultValue?: string;
  helpText?: string;
  lookupTableName?: string;
}

interface SysTableDto {
  id: string;
  name: string;
  displayName: string;
  fields: SysFieldDto[];
}

export const StudioDesigner: React.FC = () => {
  const { tableName } = useParams<{ tableName: string }>();
  const navigate = useNavigate();

  const [table, setTable] = useState<SysTableDto | null>(null);
  const [activeTab, setActiveTab] = useState<'fields' | 'layout' | 'rules' | 'preview'>('fields');
  const [selectedField, setSelectedField] = useState<SysFieldDto | null>(null);
  
  // New Field State
  const [isAddingField, setIsAddingField] = useState(false);
  const [newField, setNewField] = useState<Partial<SysFieldDto>>({
    dataType: 'Text',
    isRequired: false
  });

  const loadSchema = async () => {
    try {
      const data = await api.get(`/metadata/tables/${tableName}`) as unknown as SysTableDto;
      setTable(data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadSchema();
  }, [tableName]);

  const handleAddField = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.post(`/metadata/tables/${tableName}/fields`, newField);
      setIsAddingField(false);
      setNewField({ dataType: 'Text', isRequired: false });
      await loadSchema();
    } catch (e: any) {
      console.error(e);
      alert("Failed to add field. " + (e.response?.data?.message || e.message));
    }
  };

  const publishLayout = async () => {
    if (!table) return;
    
    // Auto-generate a simple 1-column layout for all active fields as a baseline
    // In a full implementation, this would read from the visual drag-and-drop state.
    const layoutPayload = {
      context: 'default',
      sections: [
        {
          title: 'General Information',
          displayOrder: 10,
          columns: 1,
          isCollapsed: false,
          fields: table.fields.map((f, i) => ({
            fieldId: f.id,
            displayOrder: (i + 1) * 10,
            colSpan: 1,
            isReadOnlyOverride: false
          }))
        }
      ]
    };

    try {
      await api.post(`/ui-rules/layout/${table.id}`, layoutPayload);
      alert("Schema and Layout Published Successfully!");
      // We force standard ui refresh via the component cache busting
      setActiveTab('preview');
    } catch (e: any) {
      console.error(e);
      alert("Failed to publish layout. " + (e.response?.data?.message || e.message));
    }
  };

  if (!table) return <div className="p-10 text-white">Loading designer...</div>;

  return (
    <div className="min-h-screen flex flex-col bg-slate-50">
      {/* Studio Topbar */}
      <header className="bg-slate-900 shadow-sm z-10 flex-none border-b border-indigo-500/30">
        <div className="flex h-14 items-center justify-between px-4 sm:px-6 lg:px-8">
          <div className="flex items-center gap-4">
             <button onClick={() => navigate('/app/studio')} className="text-slate-400 hover:text-white">
                &larr; Hub
             </button>
             <h1 className="text-sm font-semibold text-white bg-slate-800 px-3 py-1 rounded-full border border-slate-700">
               {table.displayName} <span className="text-slate-500 font-mono ml-2">[{table.name}]</span>
             </h1>
          </div>
          <div className="flex items-center space-x-4">
            <button
               onClick={publishLayout}
               className="inline-flex items-center gap-x-1.5 rounded-md bg-indigo-500 px-3 py-1.5 text-sm font-semibold text-white shadow-sm hover:bg-indigo-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500"
            >
               Publish Model
            </button>
          </div>
        </div>
      </header>

      {/* Main Designer Area */}
      <div className="flex flex-1 overflow-hidden">
        
        {/* Left Sidebar - Palette / Tools */}
        <aside className="w-80 overflow-y-auto border-r border-slate-200 bg-white shadow-sm z-10 flex flex-col">
          <div className="border-b border-slate-200 px-4 py-3 flex gap-2 text-sm font-medium">
             <button onClick={() => setActiveTab('fields')} className={`px-3 py-1.5 rounded flex-1 text-center ${activeTab === 'fields' ? 'bg-indigo-50 text-indigo-600' : 'text-slate-600 hover:bg-slate-50'}`}>Fields</button>
             <button onClick={() => setActiveTab('layout')} className={`px-3 py-1.5 rounded flex-1 text-center ${activeTab === 'layout' ? 'bg-indigo-50 text-indigo-600' : 'text-slate-600 hover:bg-slate-50'}`}>Layout</button>
             <button onClick={() => setActiveTab('rules')} className={`px-3 py-1.5 rounded flex-1 text-center ${activeTab === 'rules' ? 'bg-indigo-50 text-indigo-600' : 'text-slate-600 hover:bg-slate-50'}`}>Rules</button>
             <button onClick={() => setActiveTab('preview')} className={`px-3 py-1.5 rounded flex-1 text-center ${activeTab === 'preview' ? 'bg-indigo-50 text-indigo-600' : 'text-slate-600 hover:bg-slate-50'}`}>Preview</button>
          </div>

          {activeTab === 'fields' && (
             <div className="p-4 flex-1 overflow-y-auto">
                <button
                   onClick={() => setIsAddingField(true)}
                   className="w-full mb-6 relative block w-full rounded-lg border-2 border-dashed border-slate-300 p-4 text-center hover:border-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                >
                   <span className="mt-2 block text-sm font-semibold text-slate-900">+ Add New Field</span>
                </button>

                <h4 className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3">Existing Schema Fields</h4>
                <ul className="space-y-2">
                   {table.fields.map(f => (
                      <li 
                         key={f.id} 
                         onClick={() => setSelectedField(f)}
                         className={`group flex items-center justify-between rounded-md px-3 py-2 text-sm cursor-pointer border ${selectedField?.id === f.id ? 'bg-indigo-50 border-indigo-200 text-indigo-700' : 'bg-white border-slate-200 text-slate-700 hover:border-indigo-300'}`}
                      >
                         <div className="flex flex-col min-w-0">
                            <span className="truncate font-medium">{f.displayName}</span>
                            <span className="truncate text-xs text-slate-500 font-mono">{f.fieldName}</span>
                         </div>
                         <span className="inline-flex items-center rounded-md bg-slate-50 px-2 py-1 text-xs font-medium text-slate-600 ring-1 ring-inset ring-slate-500/10">
                           {f.dataType}
                         </span>
                      </li>
                   ))}
                </ul>
             </div>
          )}
        </aside>

        {/* Center Canvas */}
        <main className="flex-1 overflow-y-auto bg-slate-100 p-8 flex justify-center items-start">
           
           {activeTab === 'preview' ? (
              <div className="w-full max-w-4xl bg-white shadow-xl rounded-xl border border-slate-200 overflow-hidden">
                 <div className="bg-slate-50 px-6 py-3 border-b border-slate-200 text-sm font-semibold text-slate-700 flex justify-between items-center">
                    <span>Live Render Evaluation</span>
                    <span className="flex h-2 w-2"><span className="animate-ping absolute inline-flex h-2 w-2 rounded-full bg-green-400 opacity-75"></span><span className="relative inline-flex rounded-full h-2 w-2 bg-green-500"></span></span>
                 </div>
                 {/* Live Form Renderer Mount Point */}
                 <DynamicFormRenderer
                    module={tableName!}
                    mode="create"
                    recordId={undefined}
                 />
              </div>
           ) : activeTab === 'rules' ? (
              <div className="w-full h-full bg-white shadow-xl rounded-xl border border-slate-200 overflow-hidden">
                  <UiRuleDesigner tableId={table.id} tableName={tableName!} />
              </div>
           ) : (
              <div className="w-full max-w-2xl">
                 <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-8 min-h-[500px]">
                    
                    {activeTab === 'layout' && (
                       <div className="text-center py-20 text-slate-500">
                          <svg className="mx-auto h-12 w-12 text-slate-300 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                             <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M4 5a1 1 0 011-1h14a1 1 0 011 1v2a1 1 0 01-1 1H5a1 1 0 01-1-1V5zM4 13a1 1 0 011-1h6a1 1 0 011 1v6a1 1 0 01-1 1H5a1 1 0 01-1-1v-6zM16 13a1 1 0 011-1h2a1 1 0 011 1v6a1 1 0 01-1 1h-2a1 1 0 01-1-1v-6z" />
                          </svg>
                          <h3 className="text-lg font-medium text-slate-900">Drag & Drop Layout Engine</h3>
                          <p className="mt-1 text-sm">Visual Layout mode handles positional mapping. Hit <strong>Publish Model</strong> to auto-generate the base layout mapping using all active fields for now.</p>
                       </div>
                    )}

                    {activeTab === 'fields' && !isAddingField && !selectedField && (
                        <div className="text-center py-20 text-slate-500">
                          Select a field from the left palette to view properties, or click <strong>Add New Field</strong>.
                       </div>
                    )}

                    {activeTab === 'fields' && isAddingField && (
                       <div className="animate-in fade-in slide-in-from-bottom-4 duration-300">
                           <h3 className="text-lg font-medium text-slate-900 mb-6">Create Property Definition</h3>
                           <form onSubmit={handleAddField} className="space-y-6">
                              <div className="grid grid-cols-2 gap-6">
                                 <div className="col-span-2">
                                    <label className="block text-sm font-medium leading-6 text-slate-900">Display Label</label>
                                    <input 
                                       type="text" required 
                                       onChange={(e) => {
                                          const val = e.target.value;
                                          setNewField({
                                             ...newField, 
                                             displayName: val,
                                             fieldName: val.toLowerCase().replace(/[^a-z0-9]/g, '_')
                                          });
                                       }}
                                       className="mt-2 block w-full rounded-md border-0 py-1.5 text-slate-900 shadow-sm ring-1 ring-inset ring-slate-300 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6" 
                                    />
                                 </div>
                                 <div className="col-span-2">
                                    <label className="block text-sm font-medium leading-6 text-slate-900">Database API Name</label>
                                    <input 
                                       type="text" required readOnly value={newField.fieldName || ''}
                                       className="mt-2 block w-full rounded-md border-0 bg-slate-50 py-1.5 text-slate-500 shadow-sm ring-1 ring-inset ring-slate-300 sm:text-sm sm:leading-6 font-mono" 
                                    />
                                    <p className="mt-1 text-xs text-slate-500">Auto-generated dictionary property key.</p>
                                 </div>
                                 <div>
                                    <label className="block text-sm font-medium leading-6 text-slate-900">Data Type Validation</label>
                                    <select 
                                       value={newField.dataType}
                                       onChange={(e) => setNewField({...newField, dataType: e.target.value})}
                                       className="mt-2 block w-full rounded-md border-0 py-1.5 text-slate-900 shadow-sm ring-1 ring-inset ring-slate-300 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                                    >
                                       <option value="Text">String [Text]</option>
                                       <option value="LongText">Notes [LongText]</option>
                                       <option value="Number">Integer [Number]</option>
                                       <option value="Boolean">True/False [Boolean]</option>
                                       <option value="Choice">Dropdown [Choice]</option>
                                       <option value="Lookup">Reference [Lookup]</option>
                                       <option value="Date">Date Only</option>
                                    </select>
                                 </div>
                                 <div className="flex items-center">
                                    <input 
                                       id="required" type="checkbox"
                                       checked={newField.isRequired}
                                       onChange={(e) => setNewField({...newField, isRequired: e.target.checked})}
                                       className="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-600" 
                                    />
                                    <label htmlFor="required" className="ml-2 block text-sm font-medium leading-6 text-slate-900">Mandatory requirement policy</label>
                                 </div>
                              </div>
                              <div className="pt-5 border-t border-slate-200 flex justify-end gap-3">
                                 <button type="button" onClick={() => setIsAddingField(false)} className="rounded-md bg-white px-3 py-2 text-sm font-semibold text-slate-900 shadow-sm ring-1 ring-inset ring-slate-300 hover:bg-slate-50">Cancel</button>
                                 <button type="submit" className="rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500">Inject Field Definition</button>
                              </div>
                           </form>
                       </div>
                    )}
                    
                    {activeTab === 'fields' && selectedField && !isAddingField && (
                        <div className="animate-in fade-in duration-300">
                           <h3 className="text-lg font-medium text-slate-900 mb-6">Inspecting: {selectedField.displayName}</h3>
                           <div className="space-y-6 opacity-75">
                              {/* Read only view for now */}
                              <div className="bg-slate-50 p-4 rounded-md border border-slate-200">
                                 <dl className="divide-y divide-slate-200">
                                    <div className="px-4 py-3 text-sm flex justify-between"><dt className="font-medium text-slate-900">API Name</dt><dd className="text-slate-700 font-mono">{selectedField.fieldName}</dd></div>
                                    <div className="px-4 py-3 text-sm flex justify-between"><dt className="font-medium text-slate-900">SysId</dt><dd className="text-slate-700 font-mono text-xs">{selectedField.id}</dd></div>
                                    <div className="px-4 py-3 text-sm flex justify-between"><dt className="font-medium text-slate-900">Data Type</dt><dd className="text-slate-700">{selectedField.dataType}</dd></div>
                                    <div className="px-4 py-3 text-sm flex justify-between"><dt className="font-medium text-slate-900">Required</dt><dd className="text-slate-700">{selectedField.isRequired ? 'Yes' : 'No'}</dd></div>
                                 </dl>
                              </div>
                              <button onClick={() => setSelectedField(null)} className="text-sm text-indigo-600 font-medium hover:text-indigo-800">Close Inspector</button>
                           </div>
                        </div>
                    )}
                 </div>
              </div>
           )}
        </main>
      </div>
    </div>
  );
};
