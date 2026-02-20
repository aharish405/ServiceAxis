import React from 'react';
import { FieldRegistry } from './FieldRegistry';
import type { FormFieldMappingDto } from '../../hooks/useDynamicForm';

interface DynamicFieldProps {
  fieldConfig: FormFieldMappingDto;
  value: any;
  fieldState: {
    isHidden: boolean;
    isRequired: boolean;
    isReadOnly: boolean;
    error?: string;
  };
  onChange: (fieldName: string, value: any) => void;
  onBlur?: (fieldName: string) => void;
  options?: any[]; // Passed down to Select/Choices
}

export const DynamicField: React.FC<DynamicFieldProps> = React.memo(({ 
  fieldConfig, 
  value, 
  fieldState, 
  onChange,
  onBlur,
  options 
}) => {
  // 1. Reactive Visibility Rendering
  if (fieldState.isHidden) {
    return null; // Do not render hidden fields entirely
  }

  // 2. Resolve native component map
  const Component = FieldRegistry[fieldConfig.fieldType] || FieldRegistry['Text']; // Fallback

  const handleChange = (newVal: any) => {
    onChange(fieldConfig.fieldName, newVal);
  };

  const handleBlur = () => {
    if (onBlur) onBlur(fieldConfig.fieldName);
  };

  return (
    <div className={`col-span-${fieldConfig.colSpan || 1} sm:col-span-${fieldConfig.colSpan || 1}`}>
      <label 
        htmlFor={fieldConfig.fieldName} 
        className="block text-sm font-medium text-slate-700 mb-1"
      >
        {fieldConfig.labelOverride || fieldConfig.fieldName}
        {fieldState.isRequired && <span className="text-red-500 ml-1">*</span>}
      </label>
      
      <Component
        name={fieldConfig.fieldName}
        value={value}
        onChange={handleChange}
        onBlur={handleBlur}
        readOnly={fieldState.isReadOnly}
        required={fieldState.isRequired}
        options={options}
      />
      
      {fieldState.error && (
        <p className="mt-1 text-sm text-red-600 font-medium">
          {fieldState.error}
        </p>
      )}
    </div>
  );
});

// Setting a display name for debug tools since we're using memo
DynamicField.displayName = 'DynamicField';
