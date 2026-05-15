import { ref } from 'vue';
import api from '@/api/index';

export function useTable<T>(endpoint: string) {
  const data = ref<T[]>([]);
  const loading = ref(false);
  const pagination = ref({
    sortBy: 'id',
    descending: false,
    page: 1,
    rowsPerPage: 10,
    rowsNumber: 0
  });

  async function fetchItems(props?: { pagination: any, filter?: string }) {
    loading.value = true;
    try {
      const { page, rowsPerPage, sortBy, descending } = props?.pagination || pagination.value;
      const filter = props?.filter || '';

      const response = await api.get(endpoint, {
        params: { page, limit: rowsPerPage, sortBy, descending, filter }
      });

      data.value = Array.isArray(response.data) ? response.data : response.data.items || [];
      pagination.value = {
        ...pagination.value,
        page,
        rowsPerPage,
        rowsNumber: Array.isArray(response.data) ? response.data.length : response.data.total || 0
      };
    } finally {
      loading.value = false;
    }
  }

  return {
    data,
    loading,
    pagination,
    fetchItems
  };
}
