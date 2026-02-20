import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useDynamicRecords } from '../../hooks/useDynamicRecords';


export const DynamicRecordList: React.FC = () => {
  const { tableName } = useParams<{ tableName: string }>();
  const navigate = useNavigate();
  
  const { data, isLoading, error, page, loadPage } = useDynamicRecords(tableName || 'incident');

  const moduleNameDisplay = tableName ? tableName.charAt(0).toUpperCase() + tableName.slice(1) : 'Module';

  // Fallback icon component 
  const PlusIconSVG = () => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" className="w-5 h-5">
      <path d="M10.75 4.75a.75.75 0 00-1.5 0v4.5h-4.5a.75.75 0 000 1.5h4.5v4.5a.75.75 0 001.5 0v-4.5h4.5a.75.75 0 000-1.5h-4.5v-4.5z" />
    </svg>
  );

  return (
    <div className="min-h-screen bg-slate-50 py-10 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto mb-8 sm:flex sm:items-center sm:justify-between">
        <div className="sm:flex-auto">
          <h1 className="text-xl font-semibold leading-6 text-gray-900">{moduleNameDisplay} Records</h1>
          <p className="mt-2 text-sm text-gray-700">
            A dynamic list of all generic records securely stored mapping into the platform engine.
          </p>
        </div>
        <div className="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
          <button
            type="button"
            onClick={() => navigate(`/app/${tableName}/new`)}
            className="block rounded-md bg-indigo-600 px-3 py-2 text-center text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 flex items-center gap-1"
          >
            <PlusIconSVG />
            New Record
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center space-x-2 text-indigo-600">
          <div className="h-6 w-6 animate-spin rounded-full border-b-2 border-indigo-600"></div>
          <span className="font-semibold tracking-wide">Fetching Database Rows...</span>
        </div>
      ) : error ? (
        <div className="rounded-md bg-red-50 p-4 max-w-7xl mx-auto">
          <h3 className="text-sm font-medium text-red-800">Failed to load Records</h3>
          <p className="mt-2 text-sm text-red-700">{error}</p>
        </div>
      ) : (
        <div className="max-w-7xl mx-auto mt-8 flow-root">
          <div className="-mx-4 -my-2 overflow-x-auto sm:-mx-6 lg:-mx-8">
            <div className="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
              <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 sm:rounded-lg">
                <table className="min-w-full divide-y divide-gray-300">
                  <thead className="bg-gray-50">
                    <tr>
                      <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                        Number
                      </th>
                      <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                        Short Description
                      </th>
                      <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                        State
                      </th>
                      <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                        Priority
                      </th>
                      <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                        Created
                      </th>
                      <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                        <span className="sr-only">Access</span>
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200 bg-white">
                    {data?.items && data.items.map((record) => (
                      <tr key={record.id} className="hover:bg-gray-50 cursor-pointer" onClick={() => navigate(`/app/${tableName}/${record.id}`)}>
                        <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-indigo-600 sm:pl-6">
                          {record.recordNumber || 'N/A'}
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                          {record.shortDescription || 'No description provided'}
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                          <span className="inline-flex items-center rounded-md bg-green-50 px-2 py-1 text-xs font-medium text-green-700 ring-1 ring-inset ring-green-600/20">
                            {record.state}
                          </span>
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                           {record.priority ?? '-'}
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                          {new Date(record.createdAt).toLocaleDateString()}
                        </td>
                        <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                          <a href="#" className="text-indigo-600 hover:text-indigo-900">
                            Open<span className="sr-only">, {record.recordNumber}</span>
                          </a>
                        </td>
                      </tr>
                    ))}
                    {(!data?.items || data.items.length === 0) && (
                      <tr>
                        <td colSpan={6} className="py-8 text-center text-gray-500 text-sm">
                           No records found for this table.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
                
                {data && data.totalCount > data.pageSize && (
                  <div className="flex items-center justify-between border-t border-gray-200 bg-white px-4 py-3 sm:px-6">
                     <div className="flex items-center gap-2">
                       <button
                         onClick={() => loadPage(page - 1)}
                         disabled={page === 1}
                         className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:bg-gray-100 disabled:opacity-50"
                       >
                         Previous
                       </button>
                       <span className="text-sm text-gray-600">
                         Showing {(page - 1) * data.pageSize + 1} to {Math.min(page * data.pageSize, data.totalCount)} of {data.totalCount} results
                       </span>
                       <button
                         onClick={() => loadPage(page + 1)}
                         disabled={page * data.pageSize >= data.totalCount}
                         className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:bg-gray-100 disabled:opacity-50"
                       >
                         Next
                       </button>
                     </div>
                  </div>
                )}
                
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
