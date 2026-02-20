import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { DynamicFormPage } from '../pages/dynamic/DynamicFormPage';
import { DynamicRecordList } from '../pages/dynamic/DynamicRecordList';
import { StudioHub } from '../pages/studio/StudioHub';
import { StudioDesigner } from '../pages/studio/StudioDesigner';
import WorkflowDesigner from '../pages/studio/WorkflowDesigner';

// Simplified Router for Dynamic Enterprise Modules
export const AppRouter: React.FC = () => {

  return (
    <BrowserRouter>
      <Routes>
        
        {/* Foundation Landing routes */}
        <Route path="/" element={
           <div className="min-h-screen flex items-center justify-center p-20 flex-col">
              <h1 className="text-4xl font-bold bg-gradient-to-r from-indigo-500 via-purple-500 to-pink-500 text-transparent bg-clip-text mb-6">ServiceAxis Command Center</h1>
              <p className="text-gray-500 mb-8 max-w-lg text-center">
                 Welcome to the dynamically rendered evaluation environment. Navigate via direct paths to view configured apps. 
              </p>
              
              <div className="grid grid-cols-2 gap-4">
                 <a href="/app/incident" className="px-6 py-3 bg-white border border-gray-200 text-gray-700 rounded-lg shadow-sm hover:bg-gray-50 font-medium">All Incidents</a>
                 <a href="/app/incident/new" className="px-6 py-3 bg-indigo-50 border border-indigo-200 text-indigo-700 rounded-lg shadow-sm hover:bg-indigo-100 font-medium">Create Incident</a>
                 <a href="/app/studio" className="col-span-2 px-6 py-3 bg-slate-900 border border-slate-700 text-white rounded-lg shadow-sm hover:bg-slate-800 font-medium flex justify-center items-center gap-2">
                    Access Platform Studio <span className="opacity-50 text-xs">(Admin Only)</span>
                 </a>
              </div>
           </div>
        } />

        {/* Dynamic Schema Rendering Routes (The Control Plane Integrations) */}
        
        {/* Create Mode */}
        <Route 
           path="/app/:tableName/new" 
           element={<DynamicFormPage />} 
        />
        
        {/* View / Edit Mode */}
        <Route 
           path="/app/:tableName/:recordId" 
           element={<DynamicFormPage />} 
        />
        
        {/* List Mode */}
        <Route 
           path="/app/:tableName" 
           element={<DynamicRecordList />} 
        />
        
        {/* Foundation: Form Builder UI Studio */}
        <Route 
           path="/app/studio" 
           element={<StudioHub />} 
        />
        <Route 
           path="/app/studio/designer/:tableName" 
           element={<StudioDesigner />} 
        />
        <Route 
           path="/app/studio/workflow/:id" 
           element={<WorkflowDesigner />} 
        />

        
        <Route path="*" element={<Navigate to="/" />} />
      </Routes>
    </BrowserRouter>
  );
};
