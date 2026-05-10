import { useQuasar } from 'quasar'

export function useConfirm() {
  const $q = useQuasar()

  const confirmDelete = (entityName: string, onOk: () => Promise<void>) => {
    return $q.dialog({
      title: 'Confirmar Eliminación',
      message: `¿Estás seguro de que deseas eliminar <strong>${entityName}</strong>? Esta acción no se puede deshacer.`,
      html: true,
      ok: {
        label: 'Eliminar',
        color: 'negative',
        unelevated: true,
        icon: 'delete'
      },
      cancel: {
        label: 'Cancelar',
        color: 'primary',
        flat: true
      },
      persistent: true,
      style: 'min-width: 350px'
    }).onOk(async () => {
      try {
        await onOk()
        $q.notify({
          color: 'positive',
          message: 'Eliminado correctamente',
          icon: 'check'
        })
      } catch (error: any) {
        $q.notify({
          color: 'negative',
          message: error.response?.data?.message || 'Error al eliminar',
          icon: 'error'
        })
      }
    })
  }

  return {
    confirmDelete
  }
}
