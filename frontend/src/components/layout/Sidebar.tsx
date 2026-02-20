import React from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { 
  Home, 
  Ticket, 
  Box, 
  Users, 
  BarChart3, 
  Zap, 
  Layers,
  Settings2,
  ChevronRight,
  Sparkles
} from 'lucide-react';

interface SidebarProps {
  isCollapsed: boolean;
}

interface NavItem {
  id: string;
  label: string;
  icon: React.ElementType;
  path: string;
  roles?: string[];
  section?: string;
}

const navItems: NavItem[] = [
  { id: 'dashboard', label: 'Dashboards', icon: Home, path: '/', section: 'Workspace' },
  { id: 'work', label: 'My Work', icon: Zap, path: '/my-work', section: 'Workspace' },
  
  { id: 'incidents', label: 'Incidents', icon: Ticket, path: '/app/incident', section: 'Modules' },
  { id: 'assets', label: 'Assets', icon: Box, path: '/app/asset', section: 'Modules' },
  { id: 'employees', label: 'Employees', icon: Users, path: '/app/employee', section: 'Modules' },
  
  { id: 'reports', label: 'Analytics', icon: BarChart3, path: '/app/reports', section: 'Management' },
  
  { id: 'studio', label: 'Platform Studio', icon: Layers, path: '/app/studio', roles: ['Admin', 'SuperAdmin'], section: 'Design' },
  { id: 'settings', label: 'System Admin', icon: Settings2, path: '/admin/settings', roles: ['SuperAdmin'], section: 'Design' },
];

export const Sidebar: React.FC<SidebarProps> = ({ isCollapsed }) => {
  const { hasRole } = useAuth();

  const filteredItems = navItems.filter(item => 
    !item.roles || hasRole(item.roles)
  );

  const sections = Array.from(new Set(filteredItems.map(item => item.section)));

  return (
    <aside 
      className={`bg-white border-r border-slate-200 h-[calc(100vh-64px)] overflow-y-auto transition-all duration-300 ease-in-out fixed left-0 z-30 ${
        isCollapsed ? 'w-20' : 'w-72'
      }`}
    >
      <div className="py-6 flex flex-col gap-6">
        {sections.map(section => (
          <div key={section} className="px-4">
            {!isCollapsed && (
              <p className="px-4 text-[10px] font-bold text-slate-400 uppercase tracking-[0.2em] mb-4">
                {section}
              </p>
            )}
            <div className="flex flex-col gap-1">
              {filteredItems
                .filter(item => item.section === section)
                .map(item => (
                  <NavLink
                    key={item.id}
                    to={item.path}
                    className={({ isActive }) => `
                      flex items-center gap-3 px-4 py-3 rounded-xl transition-all relative group
                      ${isActive 
                        ? 'bg-indigo-50 text-indigo-700 shadow-sm border border-indigo-100/50' 
                        : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900'}
                    `}
                  >
                    {({ isActive }) => (
                      <>
                        <item.icon className={`h-5 w-5 shrink-0 ${isCollapsed ? 'mx-auto' : ''}`} />
                        {!isCollapsed && <span className="text-sm font-semibold">{item.label}</span>}
                        
                        {isActive && !isCollapsed && (
                            <ChevronRight className="h-4 w-4 ml-auto text-indigo-400" />
                        )}
                        
                        {isCollapsed && (
                            <div className="absolute left-full ml-6 px-3 py-2 bg-slate-900 text-white text-xs font-bold rounded-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all whitespace-nowrap z-50 shadow-xl">
                                {item.label}
                                <div className="absolute top-1/2 -left-1 -translate-y-1/2 border-y-4 border-y-transparent border-r-4 border-r-slate-900"></div>
                            </div>
                        )}
                      </>
                    )}
                  </NavLink>
                ))}
            </div>
          </div>
        ))}
      </div>

      <div className="absolute bottom-6 px-8 w-full">
         {!isCollapsed ? (
           <div className="p-4 rounded-2xl bg-gradient-to-br from-indigo-600 to-indigo-700 text-white shadow-lg shadow-indigo-200">
              <div className="flex items-center gap-2 mb-2">
                 <Sparkles className="h-4 w-4 text-indigo-300" />
                 <span className="text-xs font-bold uppercase tracking-wider opacity-80">Pro Support</span>
              </div>
              <p className="text-xs font-medium leading-relaxed mb-3">
                 Access 24/7 dedicated enterprise architects.
              </p>
              <button className="w-full py-2 bg-white/20 hover:bg-white/30 rounded-lg text-xs font-bold transition-colors">
                 Upgrade Now
              </button>
           </div>
         ) : (
            <div className="flex justify-center">
                <div className="h-10 w-10 bg-indigo-600 rounded-xl flex items-center justify-center text-white shadow-lg shadow-indigo-200">
                    <Sparkles className="h-5 w-5" />
                </div>
            </div>
         )}
      </div>
    </aside>
  );
};
