import React, { useState } from 'react';
import { Header } from './Header';
import { Sidebar } from './Sidebar';

interface AppShellProps {
  children: React.ReactNode;
}

export const AppShell: React.FC<AppShellProps> = ({ children }) => {
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);

  return (
    <div className="min-h-screen bg-slate-50 font-sans">
      <Header onToggleSidebar={() => setIsSidebarCollapsed(!isSidebarCollapsed)} />
      
      <div className="flex">
        <Sidebar isCollapsed={isSidebarCollapsed} />
        
        <main 
          className={`flex-1 min-w-0 transition-all duration-300 ease-in-out ${
            isSidebarCollapsed ? 'ml-20' : 'ml-72'
          }`}
        >
          <div className="p-8 max-w-[1600px] mx-auto min-h-[calc(100vh-64px)]">
             {children}
          </div>
        </main>
      </div>
    </div>
  );
};
