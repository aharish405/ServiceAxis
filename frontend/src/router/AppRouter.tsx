import React from 'react';
import { ShieldCheck } from 'lucide-react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { DynamicFormPage } from '../pages/dynamic/DynamicFormPage';
import { DynamicRecordList } from '../pages/dynamic/DynamicRecordList';
import { StudioHub } from '../pages/studio/StudioHub';
import { StudioDesigner } from '../pages/studio/StudioDesigner';
import WorkflowDesigner from '../pages/studio/WorkflowDesigner';
import { LoginPage } from '../pages/auth/LoginPage';
import { ProtectedRoute } from '../components/auth/ProtectedRoute';
import { AppShell } from '../components/layout/AppShell';

// Placeholder Home/Dashboard component
const Dashboard: React.FC = () => (
    <div className="space-y-6">
        <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Platform Command Center</h1>
            <div className="flex items-center gap-2 text-sm font-medium text-slate-500">
                <span>Last Scan: {new Date().toLocaleTimeString()}</span>
            </div>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {[
                { title: 'Total Incidents', value: '1,284', change: '+12%', color: 'indigo' },
                { title: 'Active Assets', value: '45,021', change: '+2.4%', color: 'emerald' },
                { title: 'System Health', value: '99.98%', change: 'Stable', color: 'blue' },
            ].map(stat => (
                <div key={stat.title} className="p-6 bg-white rounded-2xl border border-slate-100 shadow-sm shadow-slate-200/50">
                    <p className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">{stat.title}</p>
                    <div className="flex items-end justify-between">
                        <h3 className="text-3xl font-extrabold text-slate-900 tracking-tighter">{stat.value}</h3>
                        <span className={`text-xs font-bold px-2 py-1 rounded-lg bg-${stat.color}-50 text-${stat.color}-700`}>
                            {stat.change}
                        </span>
                    </div>
                </div>
            ))}
        </div>

        <div className="p-12 bg-white rounded-3xl border border-slate-100 shadow-sm flex flex-col items-center text-center">
            <div className="h-16 w-16 bg-slate-50 rounded-2xl flex items-center justify-center mb-6">
                <ShieldCheck className="text-slate-200 h-8 w-8" />
            </div>
            <h2 className="text-xl font-bold text-slate-800 mb-2">Welcome to ServiceAxis</h2>
            <p className="text-slate-500 max-w-md mx-auto mb-8">
                Your enterprise platform is running in development mode. Use the sidebar to explore registered modules or access the Platform Studio.
            </p>
            <div className="flex gap-4">
                <a href="/app/incident" className="px-6 py-2.5 bg-indigo-600 text-white rounded-xl font-bold shadow-lg shadow-indigo-100 hover:bg-indigo-700 transition-all">View All Incidents</a>
                <a href="/app/studio" className="px-6 py-2.5 border border-slate-200 text-slate-600 rounded-xl font-bold hover:bg-slate-50 transition-all">Access Studio</a>
            </div>
        </div>
    </div>
);



export const AppRouter: React.FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public Routes */}
        <Route path="/login" element={<LoginPage />} />
        
        {/* Protected Application Routes */}
        <Route path="/*" element={
          <ProtectedRoute>
            <AppShell>
              <Routes>
                <Route path="/" element={<Dashboard />} />
                
                {/* Dynamic Schema Rendering */}
                <Route path="app/:tableName/new" element={<DynamicFormPage />} />
                <Route path="app/:tableName/:recordId" element={<DynamicFormPage />} />
                <Route path="app/:tableName" element={<DynamicRecordList />} />
                
                {/* Studio (Admin Protected) */}
                <Route path="app/studio" element={
                  <ProtectedRoute requiredRoles={['Admin', 'SuperAdmin']}>
                    <StudioHub />
                  </ProtectedRoute>
                } />
                <Route path="app/studio/designer/:tableName" element={
                  <ProtectedRoute requiredRoles={['Admin', 'SuperAdmin']}>
                    <StudioDesigner />
                  </ProtectedRoute>
                } />
                <Route path="app/studio/workflow/:id" element={
                  <ProtectedRoute requiredRoles={['Admin', 'SuperAdmin']}>
                    <WorkflowDesigner />
                  </ProtectedRoute>
                } />

                <Route path="*" element={<Navigate to="/" replace />} />
              </Routes>
            </AppShell>
          </ProtectedRoute>
        } />
      </Routes>
    </BrowserRouter>
  );
};
