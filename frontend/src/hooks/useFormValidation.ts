import { useCallback, useState } from 'react';


export interface ValidationRule {
  type: 'required' | 'regex' | 'minLength' | 'maxLength' | 'min' | 'max';
  value?: any;
  message: string;
}

export function useFormValidation() {
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateField = useCallback((
    name: string,
    value: any,
    rules: ValidationRule[],
    isRequired: boolean
  ): string | null => {
    let errorMsg: string | null = null;

    // Check required first based on dynamic state
    if (isRequired && (value === undefined || value === null || value === '')) {
      errorMsg = 'This field is required.';
    } else if (value !== undefined && value !== null && value !== '') {
      // Evaluate other rules only if there's a value
      for (const rule of rules) {
        if (rule.type === 'regex') {
          const regex = new RegExp(rule.value);
          if (!regex.test(value.toString())) {
            errorMsg = rule.message || 'Invalid format.';
            break;
          }
        }
        if (rule.type === 'minLength') {
          if (value.toString().length < rule.value) {
            errorMsg = rule.message || `Minimum length is ${rule.value}.`;
            break;
          }
        }
        if (rule.type === 'maxLength') {
          if (value.toString().length > rule.value) {
            errorMsg = rule.message || `Maximum length is ${rule.value}.`;
            break;
          }
        }
        if (rule.type === 'min') {
          if (Number(value) < rule.value) {
            errorMsg = rule.message || `Minimum value is ${rule.value}.`;
            break;
          }
        }
        if (rule.type === 'max') {
          if (Number(value) > rule.value) {
            errorMsg = rule.message || `Maximum value is ${rule.value}.`;
            break;
          }
        }
      }
    }

    setErrors(prev => ({
      ...prev,
      [name]: errorMsg || ''
    }));

    return errorMsg;
  }, []);

  const validateForm = useCallback((
    formData: Record<string, any>,
    fieldStates: Record<string, any>,
    layoutFields: any[] // Flat list of all fields in the layout
  ): boolean => {
    let isValid = true;
    const newErrors: Record<string, string> = {};

    layoutFields.forEach(field => {
      const state = fieldStates[field.fieldName] || {};
      
      // Hidden fields are generally ignored in validation unless specifically overriding
      if (state.isHidden) return;

      const isRequired = state.isRequired || field.isRequiredOverride;
      const value = formData[field.fieldName];
      
      // Simple required check for full form evaluation
      if (isRequired && (value === undefined || value === null || value === '')) {
        newErrors[field.fieldName] = 'This field is required.';
        isValid = false;
      }
      
      // Add more dynamic rule validation here parsing from field.validationRules if they exist natively in SysField
    });

    setErrors(newErrors);
    return isValid;
  }, []);

  return {
    errors,
    setErrors,
    validateField,
    validateForm
  };
}
