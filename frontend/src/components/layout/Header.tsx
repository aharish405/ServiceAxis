import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { 
  Bell, 
  Search, 
  Menu, 
  User, 
  LogOut, 
  Settings, 
  ShieldCheck,
  ChevronDown,
  Globe
} from 'lucide-react';

interface HeaderProps {
  onToggleSidebar: () => void;
}

export const Header: React.FC<HeaderProps> = ({ onToggleSidebar }) => {
  const { user, logout } = useAuth();
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);

  return (
    <header className="h-16 bg-white border-b border-slate-200 sticky top-0 z-40 px-4 flex items-center justify-between shadow-sm">
      <div className="flex items-center gap-4">
        <button 
          onClick={onToggleSidebar}
          className="p-2 hover:bg-slate-50 rounded-lg text-slate-500 transition-colors"
        >
          <Menu h-5 w-5 />
        </button>
        
        <div className="flex items-center gap-2 group cursor-pointer" onClick={() => window.location.href = '/'}>
          <div className="h-8 w-8 bg-indigo-600 rounded-lg flex items-center justify-center">
            <ShieldCheck className="text-white h-5 w-5" />
          </div>
          <span className="font-bold text-xl text-slate-900 tracking-tight hidden sm:block">ServiceAxis</span>
        </div>

        {/* Environment Badge */}
        <div className="ml-4 hidden md:flex items-center px-2 py-0.5 rounded-full bg-amber-50 border border-amber-200 text-[10px] font-bold text-amber-700 uppercase tracking-wider">
          <span className="h-1.5 w-1.5 rounded-full bg-amber-500 mr-1.5 animate-pulse"></span>
          Development
        </div>
      </div>

      <div className="flex-1 max-w-xl px-8 hidden md:block">
        <div className="relative group">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <Search className="h-4 w-4 text-slate-400 group-focus-within:text-indigo-500 transition-colors" />
          </div>
          <input
            type="text"
            className="block w-full pl-10 pr-3 py-2 border border-slate-200 rounded-xl leading-5 bg-slate-50 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm transition-all"
            placeholder="Search records, users, or studio..."
          />
          <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
            <kbd className="hidden sm:inline-flex items-center px-1.5 py-0.5 border border-slate-200 rounded-md text-[10px] font-medium text-slate-400 bg-white">
              ⌘K
            </kbd>
          </div>
        </div>
      </div>

      <div className="flex items-center gap-2">
        <button className="p-2 text-slate-500 hover:bg-slate-50 rounded-lg relative transition-colors">
          <Bell className="h-5 w-5" />
          <span className="absolute top-2 right-2 h-2 w-2 bg-red-500 rounded-full border-2 border-white"></span>
        </button>

        <button className="p-2 text-slate-500 hover:bg-slate-50 rounded-lg hidden sm:block transition-colors">
          <Globe className="h-5 w-5" />
        </button>

        <div className="h-8 w-px bg-slate-200 mx-1 hidden sm:block"></div>

        <div className="relative">
          <button 
            onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
            className="flex items-center gap-2 p-1.5 hover:bg-slate-50 rounded-xl transition-all"
          >
            <div className="h-8 w-8 rounded-lg bg-indigo-100 flex items-center justify-center border border-indigo-200 shadow-sm overflow-hidden">
                {user?.userName?.charAt(0).toUpperCase() || <User className="h-4 w-4 text-indigo-600" />}
            </div>
            <div className="hidden lg:block text-left">
              <p className="text-xs font-bold text-slate-900 leading-none">{user?.userName || 'User'}</p>
              <p className="text-[10px] font-medium text-slate-500 mt-0.5 leading-none">
                {user?.roles?.[0] || 'Member'}
              </p>
            </div>
            <ChevronDown className={`h-4 w-4 text-slate-400 transition-transform ${isUserMenuOpen ? 'rotate-180' : ''}`} />
          </button>

          {isUserMenuOpen && (
            <div className="absolute right-0 mt-2 w-56 rounded-2xl bg-white shadow-2xl shadow-slate-200 border border-slate-100 py-2 z-50 animate-in fade-in zoom-in-95 duration-100">
               <div className="px-4 py-2 border-b border-slate-50 mb-1">
                 <p className="text-xs font-medium text-slate-400 uppercase tracking-widest leading-loose">Account</p>
                 <p className="text-sm font-bold text-slate-800 truncate">{user?.email}</p>
               </div>
               
               <button className="w-full flex items-center gap-3 px-4 py-2 text-sm text-slate-600 hover:bg-slate-50 hover:text-indigo-600 transition-colors">
                 <User className="h-4 w-4" />
                 Profile
               </button>
               <button className="w-full flex items-center gap-3 px-4 py-2 text-sm text-slate-600 hover:bg-slate-50 hover:text-indigo-600 transition-colors">
                 <Settings className="h-4 w-4" />
                 User Preferences
               </button>
               
               <div className="my-1 border-t border-slate-50"></div>
               
               <button 
                onClick={logout}
                className="w-full flex items-center gap-3 px-4 py-2 text-sm text-red-600 hover:bg-red-50 transition-colors"
               >
                 <LogOut className="h-4 w-4" />
                 Logout
               </button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
};
