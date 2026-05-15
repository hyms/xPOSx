export const rules = {
  required: (val: any) => {
    if (typeof val === 'string') return !!val.trim() || 'Requerido';
    return (val !== null && val !== undefined && val !== '') || 'Requerido';
  },
  email: (val: string) => !val || /.+@.+\..+/.test(val) || 'Email inválido',
  minLength: (length: number) => (val: string) => !val || val.length >= length || `Mínimo ${length} caracteres`
};
