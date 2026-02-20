import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

interface TableDto {
  id: string;
  name: string;
  displayName: string;
  schemaName: string;
  auditEnabled: boolean;
  createdAt: string;
}

interface CreateTableRequest {
  name: string;
  displayName: string;
  schemaName: string;
  auditEnabled: boolean;
  allowAttachments: boolean;
}

export const StudioHub: React.FC = () => {
  const [tables, setTables] = useState<TableDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreating, setIsCreating] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [formData, setFormData] = useState<CreateTableRequest>({
    name: '',
    displayName: '',
    schemaName: 'platform',
    auditEnabled: true,
    allowAttachments: true,
  });
  
  const navigate = useNavigate();

  const fetchTables = async () => {
    try {
      setIsLoading(true);
      const res = await fetch('/api/v1/metadata/tables?pageSize=50', {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
      });
      if (res.ok) {
        const data = await res.json();
        setTables(data.items || []);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchTables();
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsCreating(true);
      const res = await fetch('/api/v1/metadata/tables', {
        method: 'POST',
        headers: { 
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
      });
      
      if (res.ok) {
        setShowModal(false);
        await fetchTables();
        navigate(`/app/studio/designer/${formData.name}`);
      } else {
        alert("Failed to create table. " + await res.text());
      }
    } catch (err) {
      console.error(err);
    } finally {
      setIsCreating(false);
    }
  };

  // Convert Display Name to backend 'name' safely
  const handleNameChange = (val: string) => {
    setFormData({
      ...formData,
      displayName: val,
      name: val.toLowerCase().replace(/[^a-z0-9]/g, '_')
    });
  };

  return (
    <div className="min-h-screen bg-slate-900 text-white">
      {/* Studio Header */}
      <div className="border-b border-white/10 bg-slate-800/50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-5 flex items-center justify-between">
          <div className="flex items-center gap-3">
             <div className="bg-indigo-500 rounded p-1.5">
                <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                   <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4m0 5c0 2.21-3.582 4-8 4s-8-1.79-8-4" />
                </svg>
             </div>
             <div>
                <h1 className="text-xl font-bold tracking-tight">ServiceAxis Studio</h1>
                <p className="text-xs text-slate-400 font-medium">Enterprise Data Model Engine</p>
             </div>
          </div>
          <button
            onClick={() => setShowModal(true)}
            className="rounded-md bg-indigo-500 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500"
          >
            + Create Module
          </button>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        <div className="flex items-center gap-2 mb-6">
           <h2 className="text-lg font-bold">Data Modules</h2>
           <div className="h-px flex-grow bg-white/10"></div>
        </div>

        {isLoading ? (
           <div className="flex items-center justify-center h-64">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-500"></div>
           </div>
        ) : (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {tables.map(table => (
              <div key={table.id} className="bg-slate-800 rounded-xl border border-white/10 shadow-lg overflow-hidden flex flex-col transition hover:border-indigo-500/50 hover:bg-slate-800/80">
                <div className="p-6 flex-grow">
                  <div className="flex items-center justify-between mb-4">
                     <h3 className="text-lg font-semibold text-white truncate px-2 border-l-2 border-indigo-500">{table.displayName}</h3>
                     <span className="inline-flex items-center rounded-md bg-slate-700/50 px-2 py-1 text-xs font-medium text-slate-300 ring-1 ring-inset ring-white/10">
                        {table.schemaName}
                     </span>
                  </div>
                  <p className="text-sm text-slate-400 font-mono mb-6">api: {table.name}</p>
                  
                  <div className="flex items-center gap-4 text-xs font-medium text-slate-400">
                     <div className="flex items-center gap-1">
                        <svg className="w-4 h-4 text-indigo-400" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 2a4 4 0 00-4 4v1H5a1 1 0 00-.994.89l-1 9A1 1 0 004 18h12a1 1 0 00.994-1.11l-1-9A1 1 0 0015 7h-1V6a4 4 0 00-4-4zm2 5V6a2 2 0 10-4 0v1h4zm-6 3a1 1 0 112 0 1 1 0 01-2 0zm7-1a1 1 0 100 2 1 1 0 000-2z" clipRule="evenodd" /></svg>
                        Auditing {table.auditEnabled ? 'On' : 'Off'}
                     </div>
                  </div>
                </div>
                <div className="bg-slate-900/50 px-6 py-4 flex gap-3 border-t border-white/5">
                  <button 
                    onClick={() => navigate(`/app/studio/designer/${table.name}`)}
                    className="flex-1 rounded bg-white/10 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-white/20"
                  >
                    Open Designer
                  </button>
                  <button 
                    onClick={() => window.open(`/app/${table.name}`, '_blank')}
                    className="flex-none rounded bg-transparent px-3 py-2 text-sm font-semibold text-indigo-400 hover:bg-indigo-500/10"
                  >
                    Preview App
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}

        <div className="mt-16 mb-6 flex items-center gap-2">
           <h2 className="text-lg font-bold">Business Workflows</h2>
           <div className="h-px flex-grow bg-white/10"></div>
        </div>

        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
           <div className="bg-slate-800 rounded-xl border border-white/10 p-6 flex items-center justify-between hover:border-indigo-500/50 transition cursor-pointer"
                onClick={() => navigate('/app/studio/workflow/incident-approval')}>
              <div className="flex items-center gap-4">
                 <div className="p-3 bg-indigo-500/10 rounded-lg text-indigo-400">
                    <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                    </svg>
                 </div>
                 <div>
                    <h3 className="font-semibold">Standard Approval Flow</h3>
                    <p className="text-xs text-slate-400">Multi-stage manager approval with notifications</p>
                 </div>
              </div>
              <div className="text-indigo-400">
                 <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                 </svg>
              </div>
           </div>

           <div className="bg-slate-800 rounded-xl border border-white/10 p-6 flex items-center justify-between hover:border-indigo-500/50 transition cursor-pointer"
                onClick={() => navigate('/app/studio/workflow/auto-routing')}>
              <div className="flex items-center gap-4">
                 <div className="p-3 bg-emerald-500/10 rounded-lg text-emerald-400">
                    <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4-4m-4 4l4 4" />
                    </svg>
                 </div>
                 <div>
                    <h3 className="font-semibold">Auto-Assignment Logic</h3>
                    <p className="text-xs text-slate-400">Dynamic routing based on category and priority</p>
                 </div>
              </div>
              <div className="text-indigo-400">
                 <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                 </svg>
              </div>
           </div>
        </div>
      </div>

      {/* Create Modal */}
      {showModal && (
        <div className="relative z-50" aria-labelledby="modal-title" role="dialog" aria-modal="true">
          <div className="fixed inset-0 bg-black/80 transition-opacity"></div>
          <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
            <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
              <div className="relative transform overflow-hidden rounded-lg bg-slate-800 px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6 border border-white/10">
                <form onSubmit={handleCreate}>
                  <div>
                    <div className="mt-3 sm:mt-5">
                      <h3 className="text-xl font-semibold leading-6 text-white mb-6" id="modal-title">Create New Module</h3>
                      
                      <div className="space-y-4">
                         <div>
                            <label className="block text-sm font-medium leading-6 text-slate-300">Display Name</label>
                            <input
                               type="text"
                               required
                               placeholder="e.g. Employee Devices"
                               value={formData.displayName}
                               onChange={(e) => handleNameChange(e.target.value)}
                               className="mt-1 block w-full rounded-md border-0 bg-slate-900/50 py-1.5 text-white shadow-sm ring-1 ring-inset ring-white/10 focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
                            />
                         </div>
                         <div>
                            <label className="block text-sm font-medium leading-6 text-slate-300">API Table Name (immutable)</label>
                            <input
                               type="text"
                               required
                               readOnly
                               value={formData.name}
                               className="mt-1 block w-full rounded-md border-0 bg-slate-900 py-1.5 text-slate-500 shadow-sm ring-1 ring-inset ring-white/5 cursor-not-allowed sm:text-sm sm:leading-6 font-mono"
                            />
                         </div>
                      </div>
                    </div>
                  </div>
                  <div className="mt-8 sm:flex sm:flex-row-reverse">
                    <button
                      type="submit"
                      disabled={isCreating}
                      className="inline-flex w-full justify-center rounded-md bg-indigo-500 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-400 sm:ml-3 sm:w-auto disabled:opacity-50"
                    >
                      {isCreating ? 'Provisioning...' : 'Create & Open Designer'}
                    </button>
                    <button
                      type="button"
                      onClick={() => setShowModal(false)}
                      className="mt-3 inline-flex w-full justify-center rounded-md bg-transparent px-4 py-2 text-sm font-semibold text-slate-300 shadow-sm ring-1 ring-inset ring-slate-600 hover:bg-slate-700 sm:mt-0 sm:w-auto"
                    >
                      Cancel
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
