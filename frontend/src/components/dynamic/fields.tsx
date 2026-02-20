import React from 'react';

export interface FieldComponentProps {
  name: string;
  value: any;
  onChange: (e: any) => void;
  onBlur?: (e: any) => void;
  label: string;
  readOnly?: boolean;
  required?: boolean;
  error?: string;
  helpText?: string;
  options?: any[]; // For selects/lookups
  [key: string]: any;
}

// ─── Base Input Styling ───
const inputClassNames = "w-full rounded-md border border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 disabled:bg-gray-100 disabled:text-gray-500";

export const TextField: React.FC<FieldComponentProps> = ({ value, onChange, onBlur, name, readOnly, required }) => (
  <input
    type="text"
    name={name}
    value={value || ''}
    onChange={(e) => onChange(e.target.value)}
    onBlur={onBlur}
    disabled={readOnly}
    required={required}
    className={inputClassNames}
  />
);

export const NumberField: React.FC<FieldComponentProps> = ({ value, onChange, onBlur, name, readOnly, required }) => (
  <input
    type="number"
    name={name}
    value={value || ''}
    onChange={(e) => onChange(e.target.value ? Number(e.target.value) : null)}
    onBlur={onBlur}
    disabled={readOnly}
    required={required}
    className={inputClassNames}
  />
);

export const SelectField: React.FC<FieldComponentProps> = ({ value, onChange, onBlur, name, readOnly, required, options = [] }) => (
  <select
    name={name}
    value={value || ''}
    onChange={(e) => onChange(e.target.value)}
    onBlur={onBlur}
    disabled={readOnly}
    required={required}
    className={inputClassNames}
  >
    <option value="">-- Select --</option>
    {options.map((opt: any, idx: number) => (
      <option key={idx} value={opt.value}>
        {opt.label}
      </option>
    ))}
  </select>
);

export const DateField: React.FC<FieldComponentProps> = ({ value, onChange, onBlur, name, readOnly, required }) => (
  <input
    type="date"
    name={name}
    value={value ? value.split('T')[0] : ''} // basic format
    onChange={(e) => onChange(e.target.value)}
    onBlur={onBlur}
    disabled={readOnly}
    required={required}
    className={inputClassNames}
  />
);

export const TextAreaField: React.FC<FieldComponentProps> = ({ value, onChange, onBlur, name, readOnly, required }) => (
  <textarea
    name={name}
    value={value || ''}
    onChange={(e) => onChange(e.target.value)}
    onBlur={onBlur}
    disabled={readOnly}
    required={required}
    rows={3}
    className={inputClassNames}
  />
);

// Generic placeholder for lookup matching platform relations
export const LookupField: React.FC<FieldComponentProps> = ({ value, name, readOnly }) => (
  <div className="flex rounded-md shadow-sm">
    <input
      type="text"
      name={name}
      readOnly
      value={value?.displayValue || value || ''}
      className={inputClassNames + " rounded-r-none border-r-0"}
      placeholder="Select reference..."
    />
    <button
      type="button"
      disabled={readOnly}
      onClick={() => alert(`Opening lookup for ${name} (Implementation Pending)`)}
      className="inline-flex items-center rounded-r-md border border-gray-300 bg-gray-50 px-3 text-sm text-gray-500 hover:bg-gray-100 disabled:bg-gray-50 disabled:text-gray-400"
    >
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
         <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
      </svg>
    </button>
  </div>
);
