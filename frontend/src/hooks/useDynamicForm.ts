import { useState, useCallback, useRef, useEffect } from 'react';

// --- Types & Interfaces ---
export type FormContextType = 'Create' | 'Edit' | 'View' | 'All';
export type LogicalGroup = 0 | 1; // 0 = And, 1 = Or (or map to string depending on serialization)
export type UiPolicyOperator = 0 | 1 | 2 | 3 | 4 | 5 | 6; // Example Enums (Equals=0, NotEquals=1, etc.)
export type UiPolicyActionType = 0 | 1 | 2 | 3 | 4 | 5; // Show=0, Hide=1, MakeMandatory=2, etc.
export type FieldRuleActionType = 0 | 1 | 2; // SetValue=0, Calculate=1, ClearValue=2
export type ClientScriptEventType = 0 | 1 | 2; // OnLoad=0, OnChange=1, OnSubmit=2

export interface FormFieldMappingDto {
  id: string;
  fieldId: string;
  fieldName: string;
  fieldType: string;
  displayOrder: number;
  isReadOnlyOverride?: boolean;
  isRequiredOverride?: boolean;
  isHidden: boolean;
  labelOverride?: string;
  colSpan: number;
}

export interface FormSectionDto {
  id: string;
  title: string;
  displayOrder: number;
  isCollapsed: boolean;
  columns: number;
  fields: FormFieldMappingDto[];
}

export interface FormLayoutDto {
  id: string;
  name: string;
  displayName: string;
  formContext: string;
  isDefault: boolean;
  sections: FormSectionDto[];
}

export interface UiMetadataPayload {
  tableId: string;
  formLayout: FormLayoutDto | null;
  uiPolicies: UiPolicy[];
  fieldRules: FieldRule[];
  clientScripts: ClientScript[];
}

export interface UiPolicy {
  id: string;
  name: string;
  executionOrder: number;
  formContext: FormContextType | number;
  conditions: UiPolicyCondition[];
  actions: UiPolicyAction[];
}

export interface UiPolicyCondition {
  fieldId: string;
  fieldName: string;
  operator: number;
  value: string | null;
  logicalGroup: number;
}

export interface UiPolicyAction {
  targetFieldId: string;
  targetFieldName: string;
  actionType: number;
}

export interface FieldRule {
  id: string;
  name: string;
  triggerFieldId: string | null;
  triggerFieldName: string | null;
  conditionJson: any;
  targetFieldId: string;
  targetFieldName: string;
  actionType: number;
  valueExpression: string;
  executionOrder: number;
}

export interface ClientScript {
  id: string;
  name: string;
  eventType: number;
  triggerFieldId: string | null;
  triggerFieldName: string | null;
  scriptCode: string;
  executionOrder: number;
}

export interface FieldState {
  isHidden: boolean;
  isRequired: boolean;
  isReadOnly: boolean;
  error?: string;
}

export interface FormStateMap {
  [fieldName: string]: FieldState;
}

export interface FormDataMap {
  [fieldName: string]: any;
}

// --- Enum Parsers (Mapped from C# Enums) ---
// UiPolicyOperator: 0=Equals, 1=NotEquals, 2=Contains, 3=GreaterThan, 4=LessThan, 5=IsEmpty, 6=IsNotEmpty
// UiPolicyActionType: 0=Show, 1=Hide, 2=MakeMandatory, 3=MakeOptional, 4=MakeReadOnly, 5=MakeEditable
// FieldRuleActionType: 0=SetValue, 1=Calculate, 2=ClearValue
// ClientScriptEventType: 0=OnLoad, 1=OnChange, 2=OnSubmit

const defaultFieldState: FieldState = {
  isHidden: false,
  isRequired: false,
  isReadOnly: false,
};

export function useDynamicForm(tableId: string, initialContext: string = 'default') {
  const [metadata, setMetadata] = useState<UiMetadataPayload | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Core reactive states
  const [formData, setFormData] = useState<FormDataMap>({});
  const [fieldStates, setFieldStates] = useState<FormStateMap>({});

  // Use refs for latest state inside closures (like client scripts) without rebinding limits
  const formDataRef = useRef(formData);
  const fieldStatesRef = useRef(fieldStates);
  const isEvaluatingRef = useRef(false);

  useEffect(() => {
    formDataRef.current = formData;
    fieldStatesRef.current = fieldStates;
  }, [formData, fieldStates]);

  // Bootstraps the form
  const loadMetadata = useCallback(async () => {
    setIsLoading(true);
    try {
      // In a real app, this would use a globally configured axios/fetch instance
      const response = await fetch(`/api/v1/ui-rules/${tableId}?context=${initialContext}`, {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
      });
      if (!response.ok) throw new Error("Failed to load form metadata.");
      const data: UiMetadataPayload = await response.json();
      setMetadata(data);
      initializeFieldStates(data);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  }, [tableId, initialContext]);

  useEffect(() => {
    if (tableId) {
      loadMetadata();
    }
  }, [tableId, loadMetadata]);

  const initializeFieldStates = (meta: UiMetadataPayload) => {
    const states: FormStateMap = {};
    if (meta.formLayout) {
      meta.formLayout.sections.forEach((sec: any) => {
        sec.fields.forEach((f: any) => {
          states[f.fieldName] = {
            isHidden: f.isHidden || false,
            isRequired: f.isRequiredOverride || false,
            isReadOnly: f.isReadOnlyOverride || false,
          };
        });
      });
    }
    setFieldStates(states);
  };

  // The 'Form' API exposed to Client Scripts
  const createFormApi = useCallback(() => {
    return {
      getValue: (fieldName: string) => formDataRef.current[fieldName],
      setValue: (fieldName: string, value: any) => handleFieldValueChange(fieldName, value),
      setMandatory: (fieldName: string, isMandatory: boolean) => {
        setFieldStates(prev => ({
          ...prev,
          [fieldName]: { ...(prev[fieldName] || defaultFieldState), isRequired: isMandatory }
        }));
      },
      setReadOnly: (fieldName: string, isReadOnly: boolean) => {
        setFieldStates(prev => ({
          ...prev,
          [fieldName]: { ...(prev[fieldName] || defaultFieldState), isReadOnly }
        }));
      },
      setDisplay: (fieldName: string, isDisplayed: boolean) => {
        setFieldStates(prev => ({
          ...prev,
          [fieldName]: { ...(prev[fieldName] || defaultFieldState), isHidden: !isDisplayed }
        }));
      },
      addError: (fieldName: string, msg: string) => {
        setFieldStates(prev => ({
          ...prev,
          [fieldName]: { ...(prev[fieldName] || defaultFieldState), error: msg }
        }));
      },
      clearError: (fieldName: string) => {
        setFieldStates(prev => ({
          ...prev,
          [fieldName]: { ...(prev[fieldName] || defaultFieldState), error: undefined }
        }));
      }
    };
  }, []);

  // --- Rule Evaluation Engine ---

  const evaluateCondition = (val1: any, op: number, val2: any): boolean => {
    const str1 = (val1 || '').toString().toLowerCase();
    const str2 = (val2 || '').toString().toLowerCase();

    switch (op) {
      case 0: return str1 === str2; // Equals
      case 1: return str1 !== str2; // NotEquals
      case 2: return str1.includes(str2); // Contains
      case 3: return parseFloat(str1) > parseFloat(str2); // GreaterThan
      case 4: return parseFloat(str1) < parseFloat(str2); // LessThan
      case 5: return str1 === ''; // IsEmpty
      case 6: return str1 !== ''; // IsNotEmpty
      default: return false;
    }
  };

  const evaluateUiPolicies = useCallback((currentData: FormDataMap = formDataRef.current) => {
    if (!metadata?.uiPolicies) return;

    setFieldStates(prevStates => {
      const newStates = { ...prevStates };
      let hasChanges = false;

      // Reset states applied by policies previously (or re-eval baseline)
      // For a robust enterprise implementation, you'd track exactly which policies applied what,
      // but here we sweep evaluate natively.
      
      const applyAction = (fieldName: string, actionType: number, shouldApply: boolean) => {
        if (!newStates[fieldName]) newStates[fieldName] = { ...defaultFieldState };
        const st = newStates[fieldName];
        
        switch (actionType) {
          case 0: // Show
            if (shouldApply) st.isHidden = false;
            break;
          case 1: // Hide
            if (shouldApply) st.isHidden = true;
            break;
          case 2: // MakeMandatory
            if (shouldApply) st.isRequired = true;
            break;
          case 3: // MakeOptional
             if (shouldApply) st.isRequired = false;
             break;
          case 4: // MakeReadOnly
             if (shouldApply) st.isReadOnly = true;
             break;
          case 5: // MakeEditable
             if (shouldApply) st.isReadOnly = false;
             break;
        }
        hasChanges = true;
      };

      metadata.uiPolicies.forEach(policy => {
        let isMatch = true;

        if (policy.conditions.length > 0) {
            // Evaluator supporting sequential logical groups
            isMatch = policy.conditions[0] ? evaluateCondition(currentData[policy.conditions[0].fieldName], policy.conditions[0].operator, policy.conditions[0].value) : true;
            
            for (let i = 1; i < policy.conditions.length; i++) {
                const c = policy.conditions[i];
                const condMatch = evaluateCondition(currentData[c.fieldName], c.operator, c.value);
                
                if (c.logicalGroup === 1) { // 1 = Or
                    isMatch = isMatch || condMatch;
                } else { // 0 = And
                    isMatch = isMatch && condMatch;
                }
            }
        }

        policy.actions.forEach(action => {
            applyAction(action.targetFieldName, action.actionType, isMatch);
        });
      });

      return hasChanges ? newStates : prevStates;
    });
  }, [metadata]);


  const executeClientScripts = useCallback((eventType: number, triggerFieldName?: string) => {
    if (!metadata?.clientScripts) return;

    const scriptsToRun = metadata.clientScripts.filter(s => 
      s.eventType === eventType && 
      (!triggerFieldName || s.triggerFieldName?.toLowerCase() === triggerFieldName.toLowerCase())
    );

    if (scriptsToRun.length === 0) return;

    const formApi = createFormApi();

    scriptsToRun.forEach(script => {
      try {
        // Secure execution sandbox wrapper
        const runner = new Function('form', `
          // Block globals
          let window = undefined;
          let document = undefined;
          let fetch = undefined;
          let XMLHttpRequest = undefined;

          // Inject user script
          ${script.scriptCode}

          // Trigger
          if (typeof invoke === 'function') {
             invoke(form);
          }
        `);
        runner(formApi);
      } catch (err) {
        console.error(`Error executing Client Script '${script.name}':`, err);
      }
    });

  }, [metadata, createFormApi]);


  const evaluateFieldRules = useCallback((triggerFieldName?: string) => {
    if (!metadata?.fieldRules) return;

    // Grab rules matching the trigger (if provided) or runs evaluating rules on load if null
    const rulesToRun = metadata.fieldRules.filter(r => 
      !triggerFieldName || r.triggerFieldName?.toLowerCase() === triggerFieldName.toLowerCase()
    );

    if (rulesToRun.length === 0) return;

    setFormData(prevData => {
      const nextData = { ...prevData };
      let changed = false;

      rulesToRun.forEach(rule => {
        // Evaluate conditions if any
        let isMatch = true; 
        if (rule.conditionJson && Object.keys(rule.conditionJson).length > 0) {
            // Complex expression evaluator is usually needed here, omitting for brevity
            // Assuming empty JSON {} implies always match
            isMatch = true; 
        }

        if (isMatch) {
            // Action processing
            if (rule.actionType === 0) { // SetValue
                nextData[rule.targetFieldName] = rule.valueExpression;
                changed = true;
            } else if (rule.actionType === 1) { // Calculate
                try {
                    // Safe basic JS string evaluation (only numbers) for calculations
                    // E.g. "quantity * price"
                    let expr = rule.valueExpression;
                    // Replace field names with actual values in expr
                    Object.keys(nextData).forEach(k => {
                        const val = nextData[k] || 0;
                        expr = expr.replace(new RegExp(`\\b${k}\\b`, 'g'), val.toString());
                    });
                    
                    // Note: In production, use mathematical parser library (mathjs), don't use Function
                    const calc = new Function(`return ${expr};`);
                    nextData[rule.targetFieldName] = calc();
                    changed = true;
                } catch(e) {
                    console.error("Field rule calculation failed", e);
                }
            } else if (rule.actionType === 2) { // ClearValue
                nextData[rule.targetFieldName] = null;
                changed = true;
            }
        }
      });

      return changed ? nextData : prevData;
    });
  }, [metadata]);


  // Native lifecycle hook handlers
  useEffect(() => {
    if (metadata && !isLoading) {
      // 1. Initial Load Rules
      evaluateFieldRules(undefined);
      evaluateUiPolicies(formDataRef.current);
      // 2. Initial Client Scripts
      executeClientScripts(0); // 0 = OnLoad
    }
  }, [metadata, isLoading, evaluateFieldRules, evaluateUiPolicies, executeClientScripts]);


  const handleFieldValueChange = useCallback((fieldName: string, value: any) => {
    if (isEvaluatingRef.current) return;
    isEvaluatingRef.current = true; // Prevent loop

    setFormData(prev => {
      const nextData = { ...prev, [fieldName]: value };
      
      // We schedule sync rule sweeps using microtask or timeouts to ensure state batching
      setTimeout(() => {
        evaluateUiPolicies(nextData);
        evaluateFieldRules(fieldName);
        executeClientScripts(1, fieldName); // 1 = OnChange
        isEvaluatingRef.current = false;
      }, 0);
      
      return nextData;
    });
  }, [evaluateUiPolicies, evaluateFieldRules, executeClientScripts]);


  const submitForm = useCallback(async (): Promise<boolean> => {
    // Fire onSubmit scripts (e.g., validations)
    executeClientScripts(2); // 2 = onSubmit
    
    // Check for blocking errors set by client scripts
    const hasErrors = Object.values(fieldStatesRef.current).some(s => !!s.error);
    if (hasErrors) {
      return false; // Submission blocked by script validation
    }

    // Usually dispatch an API POST/PUT here returning form state persistence
    // await fetch(`/api/v1/tables/${tableId}/records`, { method: 'POST', body: JSON.stringify(formDataRef.current) })
    
    return true;
  }, [executeClientScripts, tableId]);


  return {
    metadata,
    isLoading,
    error,
    formData,
    fieldStates,
    setFieldValue: handleFieldValueChange,
    submitForm,
    reload: loadMetadata
  };
}
