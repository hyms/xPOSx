import api from '@/api'

export interface CashRegister {
  id: number;
  name: string;
  warehouseId: number;
  isActive: boolean;
  isMatriz: boolean;
}

export interface CashShift {
  id: number;
  cashRegisterId: number;
  userId: number;
  status: string;
  openedAt: string;
  closedAt?: string;
  startingCash: number;
  endingCashExpected?: number;
  endingCashActual?: number;
  discrepancy?: number;
  closingNotes?: string;
}

export interface OpenShiftPayload {
  registerId: number;
  startingCash: number;
}

export interface CloseShiftPayload {
  shiftId: number;
  actualCash: number;
  notes?: string;
}

export interface CashTransactionPayload {
  shiftId: number;
  transactionType: string; // 'FLOAT_IN', 'FLOAT_OUT', 'CASH_DROP', 'EXPENSE'
  amount: number;
  notes?: string;
  recipientName?: string;
}

export const cashShiftService = {
  getRegistersByWarehouse: (warehouseId: number) => 
    api.get<CashRegister[]>(`/CashRegisters/warehouse/${warehouseId}`),
  
  getAllRegisters: () =>
    api.get<CashRegister[]>('/CashRegisters'),
  
  createRegister: (register: Omit<CashRegister, 'id'>) =>
    api.post<CashRegister>('/CashRegisters', register),
  
  updateRegister: (id: number, register: CashRegister) =>
    api.put<CashRegister>(`/CashRegisters/${id}`, register),
  
  deleteRegister: (id: number) =>
    api.delete<{ message: string }>(`/CashRegisters/${id}`),
  
  getActiveShift: () => 
    api.get<CashShift | null>('/CashShifts/active'),
  
  openShift: (payload: OpenShiftPayload) => 
    api.post<{ shiftId: number; message: string }>('/CashShifts/open', payload),
  
  closeShift: (payload: CloseShiftPayload) => 
    api.post<{ message: string }>('/CashShifts/close', payload),
  
  executeTransaction: (payload: CashTransactionPayload) => 
    api.post<{ voucherNumber: string; message: string }>('/CashShifts/transaction', payload),
    
  getReceiptPayload: (shiftId: number) =>
    api.get<{ formattedText: string }>(`/CashShifts/print/${shiftId}`)
}
