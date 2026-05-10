# xPOSx - Sistema de Punto de Venta y Gestión de Inventario

xPOSx es un sistema de Punto de Venta (POS) e inventario moderno, diseñado para ser rápido, escalable y fácil de usar. Este proyecto representa la migración de un sistema legado en PHP/Laravel hacia una arquitectura moderna y robusta.

## 🏗️ Arquitectura del Sistema

El proyecto está dividido en dos partes principales, cada una con su propia arquitectura:

### Backend (.NET 10)
El backend está construido con **.NET 10** (C#) y sigue los principios de la **Clean Architecture** (Arquitectura Limpia) para garantizar la separación de responsabilidades, testabilidad y mantenibilidad.

Se divide en las siguientes capas (proyectos):
*   **XPos.Domain**: Contiene la lógica de negocio central, entidades del dominio, modelos, DTOs y las interfaces para los repositorios y servicios. No tiene dependencias externas.
*   **XPos.Data**: Implementa el acceso a datos. Utiliza **Dapper** (Micro-ORM) para consultas SQL optimizadas de alto rendimiento sobre una base de datos **PostgreSQL**. Implementa el patrón **Unit of Work** para asegurar la integridad transaccional en operaciones complejas (ej. crear una venta y descontar stock simultáneamente).
*   **XPos.Api**: Es la capa de presentación (Controladores REST). Gestiona la autenticación/autorización con JWT, inyección de dependencias y el enrutamiento HTTP.

*Gestión de Base de Datos:* Las migraciones de base de datos se manejan mediante **FluentMigrator**, permitiendo el control de versiones del esquema desde código C#.

### Frontend (Vue 3 + Quasar)
La interfaz de usuario es una Single Page Application (SPA) construida con:
*   **Vue 3** (Composition API con `<script setup>`).
*   **Quasar Framework** para una biblioteca rica de componentes UI (Material Design) y grillas responsivas.
*   **Vite** como bundler ultrarrápido.
*   **Pinia** para la gestión del estado global (ej. sesión de usuario, almacén activo).
*   **TypeScript** para tipado estricto en todo el frontend.

El código fuente está modularizado, con alias de rutas (`@/`) para importaciones limpias y servicios dedicados para la comunicación con la API. Además, la generación de comprobantes y reportes en PDF se realiza directamente en el cliente (client-side) usando `jsPDF`.

---

## 🚀 Módulos Principales Implementados

*   **Punto de Venta (POS)**: Interfaz ágil para cajeros.
*   **Ventas y Devoluciones**: Registro detallado con generación de comprobantes fiscales (Vouchers) dinámicos.
*   **Compras y Devoluciones de Compras**: Gestión de abastecimiento con proveedores y actualización de costos.
*   **Inventario**: Control multi-almacén, transferencias, ajustes, categorías y alertas de stock bajo.
*   **Catálogos**: Gestión de Productos (con variantes), Unidades de medida, Clientes y Proveedores.
*   **Finanzas**: Registro de gastos y cuentas por cobrar/pagar.
*   **Reportes**: Panel de control con gráficas, pérdidas y ganancias (P&L), reportes detallados de ventas, compras, clientes, productos top y un log general de actividades.
*   **Administración**: Gestión de Usuarios, Roles, Permisos y configuraciones globales (Moneda, Pasarelas de Pago, Servidor de Correo, SMS).

---

## 🛠️ Requisitos Previos

*   **Docker** y **Docker Compose** (Recomendado para levantar todo el entorno fácilmente).
*   *Para desarrollo local (sin Docker):*
    *   .NET 10 SDK
    *   Node.js (v18 o superior)
    *   PostgreSQL (v15 o superior)
    *   pnpm (Recomendado para el frontend)

---

## 💻 Instalación y Ejecución (vía Docker)

El proyecto incluye archivos `docker-compose` para simplificar la ejecución.

1.  **Clonar el repositorio:**
    ```bash
    git clone <url-del-repositorio>
    cd xPOSx
    ```

2.  **Configurar variables de entorno:**
    *   Copia o renombra el archivo `.env.example` a `.env` (si existe) y ajusta las credenciales de la base de datos y JWT según sea necesario.

3.  **Levantar los contenedores:**
    ```bash
    docker compose up --build -d
    ```
    *Este comando levantará la base de datos PostgreSQL, compilará y ejecutará el backend de .NET (ejecutando las migraciones automáticamente), y servirá el frontend con Vite.*

4.  **Acceder a la aplicación:**
    *   **Frontend:** `http://localhost:5173` (o el puerto configurado en vite).
    *   **Backend API (Swagger UI):** `http://localhost:5000/swagger`

---

## 🧪 Notas de Desarrollo

*   **Aliases del Frontend:** Se ha configurado el alias `@` para apuntar a `/frontend/src`. Para importar componentes, usa `import MiComponente from '@/components/MiComponente.vue'`.
*   **Transacciones Backend:** Cualquier operación que modifique múltiples entidades (ej. Inventario + Finanzas) DEBE ser gestionada a través de un `DomainService` utilizando `IUnitOfWork.BeginTransaction()`. No guardar directamente desde el controlador.
*   **Tipos TS Modulares:** Los tipos TypeScript están agrupados por dominio dentro de `frontend/src/types/` (ej. `auth.ts`, `inventory.ts`) y se re-exportan desde `frontend/src/types/index.ts`.

---
*Documentación generada tras la migración exitosa a Clean Architecture.*
