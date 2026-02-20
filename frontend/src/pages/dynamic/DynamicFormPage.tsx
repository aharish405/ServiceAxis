import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { DynamicFormRenderer } from '../../components/dynamic/DynamicFormRenderer';

export const DynamicFormPage: React.FC = () => {
  const { tableName, recordId } = useParams<{ tableName: string; recordId?: string }>();
  const navigate = useNavigate();

  // Basic breadcrumb and header UI for the dynamically generated forms
  const moduleNameDisplay = tableName ? tableName.charAt(0).toUpperCase() + tableName.slice(1) : 'Module';

  return (
    <div className="min-h-screen bg-slate-50 py-10 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto mb-8 flex items-center justify-between">
        
        {/* Breadcrumb Navigation */}
        <nav className="flex" aria-label="Breadcrumb">
          <ol role="list" className="flex items-center space-x-4">
            <li>
              <div>
                <button onClick={() => navigate('/')} className="text-gray-400 hover:text-gray-500">
                  <svg className="h-5 w-5 flex-shrink-0" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                    <path fillRule="evenodd" d="M9.293 2.293a1 1 0 011.414 0l7 7A1 1 0 0117 11h-1v6a1 1 0 01-1 1h-2a1 1 0 01-1-1v-3a1 1 0 00-1-1H9a1 1 0 00-1 1v3a1 1 0 01-1 1H5a1 1 0 01-1-1v-6H3a1 1 0 01-.707-1.707l7-7z" clipRule="evenodd" />
                  </svg>
                  <span className="sr-only">Home</span>
                </button>
              </div>
            </li>
            <li>
              <div className="flex items-center">
                <svg className="h-5 w-5 flex-shrink-0 text-gray-300" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
                  <path d="M5.555 17.776l8-16 .894.448-8 16-.894-.448z" />
                </svg>
                <button
                  onClick={() => navigate(`/app/${tableName}`)}
                  className="ml-4 text-sm font-medium text-gray-500 hover:text-gray-700"
                >
                  {moduleNameDisplay}
                </button>
              </div>
            </li>
            <li>
              <div className="flex items-center">
                <svg className="h-5 w-5 flex-shrink-0 text-gray-300" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
                  <path d="M5.555 17.776l8-16 .894.448-8 16-.894-.448z" />
                </svg>
                <span className="ml-4 text-sm font-medium text-gray-700" aria-current="page">
                  {recordId ? recordId : 'New Record'}
                </span>
              </div>
            </li>
          </ol>
        </nav>

        {/* Global Action Extensions */}
        {recordId && (
          <div className="flex gap-3">
             <button className="rounded bg-white px-2.5 py-1.5 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50">
               Audit History
             </button>
             <button className="rounded bg-indigo-50 px-2.5 py-1.5 text-sm font-semibold text-indigo-600 shadow-sm hover:bg-indigo-100">
               + Attachment
             </button>
          </div>
        )}
      </div>

      {/* Primary Dynamic Configuration Injection point */}
      {tableName ? (
        <DynamicFormRenderer 
            module={tableName} 
            mode={recordId ? "edit" : "create"} 
            recordId={recordId} 
        />
      ) : (
        <div className="text-center py-20 text-gray-500">Invalid Module Route</div>
      )}
      
    </div>
  );
};
