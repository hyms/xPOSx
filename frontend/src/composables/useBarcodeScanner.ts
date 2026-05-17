import { ref, onMounted, onUnmounted } from 'vue';

interface UseBarcodeScannerOptions {
  onScan: (code: string) => void;
  enabled?: () => boolean;
  timeout?: number;
  minLength?: number;
}

export function useBarcodeScanner(options: UseBarcodeScannerOptions) {
  const { 
    onScan, 
    enabled = () => true, 
    timeout = 50, 
    minLength = 3 
  } = options;

  let buffer = '';
  let lastKeyTime = Date.now();

  const handleKeydown = (e: KeyboardEvent) => {
    if (!enabled()) return;

    // Si el usuario está escribiendo en un input, ignoramos el listener global
    // para no interferir con la escritura manual, a menos que sea un ENTER
    // que viene de una ráfaga rápida.
    const target = e.target as HTMLElement;
    const isInput = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA';

    const currentTime = Date.now();
    
    // Si ha pasado mucho tiempo, reiniciamos el buffer
    if (currentTime - lastKeyTime > timeout) {
      buffer = '';
    }

    if (e.key === 'Enter') {
      if (buffer.length >= minLength) {
        onScan(buffer);
        buffer = '';
        e.preventDefault();
        e.stopPropagation();
      } else {
        buffer = '';
      }
    } else if (e.key.length === 1) {
      // Solo añadimos caracteres imprimibles
      buffer += e.key;
    }

    lastKeyTime = currentTime;
  };

  onMounted(() => {
    window.addEventListener('keydown', handleKeydown, true); // Usamos capture para adelantarnos
  });

  onUnmounted(() => {
    window.removeEventListener('keydown', handleKeydown, true);
  });
}
