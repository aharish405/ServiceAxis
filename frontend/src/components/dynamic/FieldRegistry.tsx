import React from 'react';
import { 
  TextField, 
  TextAreaField, 
  NumberField, 
  SelectField, 
  LookupField 
} from './fields';

interface BooleanFieldProps {
  name: string;
  value: any;
  onChange: (val: any) => void;
  readOnly?: boolean;
}

const BooleanField: React.FC<BooleanFieldProps> = ({ name, value, onChange, readOnly }) => {
  return (
    <input 
      type="checkbox" 
      name={name}
      checked={!!value} 
      onChange={(e) => onChange(e.target.checked)} 
      disabled={readOnly} 
      className="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-600"
    />
  );
};

export const FieldRegistry: Record<string, React.FC<any>> = {
  Text: TextField,
  Number: NumberField,
  Choice: SelectField, 
  LongText: TextAreaField,
  Lookup: LookupField,
  Boolean: BooleanField,
};
