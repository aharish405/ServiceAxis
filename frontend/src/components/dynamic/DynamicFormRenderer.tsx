import React, { useCallback, useMemo, useState } from 'react';
import { useDynamicForm } from '../../hooks/useDynamicForm';
import type { FormFieldMappingDto, FormSectionDto } from '../../hooks/useDynamicForm';
import { useFormValidation } from '../../hooks/useFormValidation';
import { DynamicField } from './DynamicField';

interface DynamicFormRendererProps {
  module: string; // The table name (e.g., 'incident', 'asset')
  recordId?: string; // Optional if loading an existing record
  mode: 'create' | 'edit' | 'view';
}

export const DynamicFormRenderer: React.FC<DynamicFormRendererProps> = ({ module, recordId, mode }) => {
  // 1. Initialize Runtime Schema Engine
  const {
    metadata,
    isLoading,
    error,
    formData,
    fieldStates,
    setFieldValue,
    submitForm,
  } = useDynamicForm(module, mode);

  const { validateForm, errors } = useFormValidation();
  
  // Custom states outside the schema engine (e.g., submitting locks, external errors)
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [globalError, setGlobalError] = useState<string | null>(null);

  // Parse Debug query parameter (e.g. ?debugForm=true)
  const isDebug = new URLSearchParams(window.location.search).get('debugForm') === 'true';

  // Flat list of fields for the validation hook loop
  const allFieldsFlat = useMemo(() => {
    if (!metadata?.formLayout) return [];
    const fields: FormFieldMappingDto[] = [];
    metadata.formLayout.sections.forEach((sec: FormSectionDto) => {
      sec.fields.forEach((f: FormFieldMappingDto) => fields.push(f));
    });
    return fields;
  }, [metadata]);

  // Handle Input Changes Native React Pattern
  const handleChange = useCallback((fieldName: string, value: any) => {
    setFieldValue(fieldName, value);
    // Execute onChange logic inherently covered natively inside setFieldValue -> executeClientScripts !
  }, [setFieldValue]);

  // Handle Form Submission Pipeline
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setGlobalError(null);

    // 1. Run Validation Hook directly against the Layout state and Form Data
    const isValid = validateForm(formData, fieldStates, allFieldsFlat);
    
    if (!isValid) {
      setGlobalError("Please correct the errors in the form before submitting.");
      setIsSubmitting(false);
      return;
    }

    // 2. Client Scripts Execution (e.g., onSubmit interceptors)
    const canSubmit = await submitForm();
    if (!canSubmit) {
      setGlobalError("Form submission was blocked by Client Scripts.");
      setIsSubmitting(false);
      return;
    }

    // 3. Normalized Payload Submitting via API
    try {
      const url = recordId ? `/api/v1/records/${module}/${recordId}` : `/api/v1/records/${module}`;
      const method = recordId ? 'PUT' : 'POST';
      
      const res = await fetch(url, {
        method,
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(formData)
      });
      
      if (!res.ok) throw new Error("Failed to save record.");
      
      alert('Record saved successfully!'); // Optional navigation redirect
    } catch (err: any) {
      setGlobalError(err.message || 'Unknown network error occurred.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
       <div className="flex h-64 items-center justify-center space-x-2 text-indigo-600">
         <div className="h-6 w-6 animate-spin rounded-full border-b-2 border-indigo-600"></div>
         <span className="font-semibold tracking-wide">Evaluating Schema...</span>
       </div>
    );
  }

  if (error || !metadata?.formLayout) {
    return (
      <div className="rounded-md bg-red-50 p-4">
        <h3 className="text-sm font-medium text-red-800">Failed to load Form Engine</h3>
        <p className="mt-2 text-sm text-red-700">{error || "Missing form layout. Ask an admin to map fields via Form Designer."}</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col md:flex-row w-full gap-4 max-w-7xl mx-auto items-start relative pb-20">
      <main className="flex-1 bg-white shadow-sm ring-1 ring-gray-900/5 sm:rounded-xl">
        
        {/* Header / Title */}
        <div className="border-b border-gray-200 px-6 py-5">
           <h2 className="text-xl font-semibold leading-7 text-gray-900">
             {metadata.formLayout.displayName} {recordId ? `(${recordId.substring(0,8)})` : '- New Record'}
           </h2>
        </div>

        {/* Form Body Rendering Loop */}
        <form onSubmit={handleSubmit} className="px-6 py-6 space-y-12 shrink-0">
          
          {globalError && (
             <div className="rounded border-l-4 border-red-500 bg-red-50 p-4 text-red-700">
               {globalError}
             </div>
          )}

          {metadata.formLayout.sections.map((section: FormSectionDto) => {
            // Apply native grid columns depending on Section properties
            const gridColsClass = section.columns === 1 ? 'grid-cols-1' :
                                  section.columns === 2 ? 'grid-cols-1 md:grid-cols-2' :
                                  section.columns === 3 ? 'grid-cols-1 md:grid-cols-3' : 'grid-cols-1 md:grid-cols-4';
            
            return (
              <div key={section.id} className="border-b border-gray-900/10 pb-10 last:border-0 last:pb-0">
                {section.title && (
                  <h2 className="text-base font-semibold leading-7 text-slate-900 mb-5 pb-2 border-b">
                    {section.title}
                  </h2>
                )}
                <div className={`grid gap-x-6 gap-y-8 ${gridColsClass}`}>
                  {section.fields.map((fieldConfig: FormFieldMappingDto) => {
                      const state = fieldStates[fieldConfig.fieldName] || {
                          isHidden: false,
                          isRequired: false,
                          isReadOnly: false
                      };
                      
                      // Inject custom validation errors onto the standard field states cleanly
                      if (errors[fieldConfig.fieldName]) {
                          state.error = errors[fieldConfig.fieldName];
                      }

                      return (
                        <DynamicField 
                          key={fieldConfig.id}
                          fieldConfig={fieldConfig}
                          value={formData[fieldConfig.fieldName]}
                          fieldState={state}
                          onChange={handleChange}
                        />
                      );
                  })}
                </div>
              </div>
            );
          })}
          
          {/* Form Actions Footer */}
          <div className="mt-6 flex items-center justify-end gap-x-6 border-t pt-5 border-gray-100">
            {mode !== 'view' && (
               <button
                 type="submit"
                 disabled={isSubmitting}
                 className="rounded-md bg-indigo-600 px-6 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 disabled:opacity-50"
               >
                 {isSubmitting ? 'Saving...' : 'Save Record'}
               </button>
            )}
          </div>
        </form>
      </main>

      {/* Developer Debug Overlay Panel */}
      {isDebug && (
        <aside className="w-96 shrink-0 bg-slate-900 text-slate-300 rounded-xl p-6 text-xs shadow-2xl overflow-y-auto max-h-[85vh] sticky top-6">
          <h3 className="text-white font-bold uppercase tracking-widest text-sm mb-4 border-b border-slate-700 pb-2">
            Control Plane Diagnostics
          </h3>
          <div className="space-y-6">
            
            <div>
              <h4 className="font-semibold text-indigo-400 mb-2">Live UI States</h4>
              <pre className="bg-slate-950 p-2 rounded overflow-x-auto">
                {JSON.stringify(fieldStates, null, 2)}
              </pre>
            </div>

            <div>
              <h4 className="font-semibold text-green-400 mb-2">FormData Buffer</h4>
              <pre className="bg-slate-950 p-2 rounded overflow-x-auto text-green-300">
                {JSON.stringify(formData, null, 2)}
              </pre>
            </div>
            
            <div>
              <h4 className="font-semibold text-orange-400 mb-2">Loaded Policies / Rules</h4>
              <ul className="list-disc pl-4 space-y-1 mt-1 text-slate-400">
                 <li>Policies: {metadata.uiPolicies?.length || 0} Evaluators Active</li>
                 <li>Scripts: {metadata.clientScripts?.length || 0} Bindings Mounted</li>
                 <li>Rules: {metadata.fieldRules?.length || 0} Auto-Calculators</li>
              </ul>
            </div>

          </div>
        </aside>
      )}
    </div>
  );
};
