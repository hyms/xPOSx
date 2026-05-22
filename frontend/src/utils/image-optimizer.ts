/**
 * Optimiza y redimensiona una imagen en el cliente.
 * Garantiza que el archivo final sea ligero y compatible.
 */
export async function compressImage(
  file: File,
  maxWidth = 500,
  maxHeight = 500,
  quality = 0.85
): Promise<File> {
  return new Promise((resolve, reject) => {
    // Si no es una imagen, rechazar
    if (!file.type.startsWith('image/')) {
      return reject(new Error('El archivo seleccionado no es una imagen válida.'));
    }

    const reader = new FileReader();
    reader.readAsDataURL(file);
    
    reader.onload = (event) => {
      const img = new Image();
      img.src = event.target?.result as string;

      img.onload = () => {
        const canvas = document.createElement('canvas');
        let width = img.width;
        let height = img.height;

        // Calcular proporciones manteniendo el aspect ratio
        if (width > height) {
          if (width > maxWidth) {
            height *= maxWidth / width;
            width = maxWidth;
          }
        } else {
          if (height > maxHeight) {
            width *= maxHeight / height;
            height = maxHeight;
          }
        }

        canvas.width = width;
        canvas.height = height;

        const ctx = canvas.getContext('2d');
        if (!ctx) return reject(new Error('No se pudo obtener el contexto del Canvas'));

        // Dibujar con suavizado de imagen de alta calidad
        ctx.imageSmoothingEnabled = true;
        ctx.imageSmoothingQuality = 'high';
        ctx.drawImage(img, 0, 0, width, height);

        // Exportar a Blob y convertir de vuelta a File (JPEG por compatibilidad masiva)
        canvas.toBlob(
          (blob) => {
            if (!blob) return reject(new Error('Error al generar el Blob de la imagen'));
            
            const optimizedFile = new File([blob], file.name.replace(/\.[^/.]+$/, ".jpg"), {
              type: 'image/jpeg',
              lastModified: Date.now(),
            });
            
            resolve(optimizedFile);
          },
          'image/jpeg',
          quality
        );
      };

      img.onerror = (err) => reject(err);
    };
    
    reader.onerror = (err) => reject(err);
  });
}
