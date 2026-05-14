# InventorySaaS - Core API

Motor transaccional de alta disponibilidad para la gestión centralizada de inventarios, optimizado para entornos multi-empresa y multi-almacén.

##  Descripción breve
**InventorySaaS - Core API** proporciona la infraestructura lógica necesaria para el control de stock, gestión de almacenes y trazabilidad de productos. Construida bajo los principios de **Clean Architecture**, esta API asegura escalabilidad, facilidad de mantenimiento y un desacoplamiento efectivo entre la lógica de negocio y los detalles de infraestructura.

##  Alcance funcional implementado
Gestión centralizada de recursos físicos y reglas de negocio asociadas a la existencia de productos en múltiples ubicaciones, cumpliendo con estándares industriales de control y auditoría.

## Lista de funcionalidades desarrolladas
- **Arquitectura Limpia (Clean Architecture):** Implementación de 4 capas para una separación clara de responsabilidades.
- **Gestión Multi-Almacén:** Control de existencias segregado por ubicación física.
- **Catálogo de Productos Avanzado:** Clasificación por categorías, unidades de medida y gestión de stock mínimo/máximo.
- **Trazabilidad Continua:** Registro histórico detallado de movimientos de entrada, salida y transferencias internas.
- **Exportación de Reportes:** Generación de archivos Excel dinámicos utilizando **EPPlus** para análisis externo.
- **Documentación de API:** Documentación interactiva completa con **Swagger/OpenAPI**.

## Tecnologías utilizadas
- **Runtime:** [.NET 9.0](https://dotnet.microsoft.com/)
- **ORM:** [Entity Framework Core 9.0](https://learn.microsoft.com/ef/core/)
- **Base de Datos:** [PostgreSQL](https://www.postgresql.org/)
- **Reportes:** [EPPlus](https://www.epplussoftware.com/)
- **Documentación:** [Swashbuckle (Swagger)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

## Instrucciones de ejecucion

### Requisitos previos
- .NET 10.0 SDK
- PostgreSQL 15+ instalado y configurado

### Ejecucion con Docker (recomendado)
Desde la raiz del workspace:
```bash
docker compose up --build
```
La API queda disponible en http://localhost:5140.

### Archivo compose
Desde la raiz del workspace:
```bash
services:
  db:
    image: postgres:15
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  inventory-api:
    build:
      context: ./InventorySaaSBackend
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DefaultConnection: >
        Host=db;
        Port=5432;
        Database=${POSTGRES_DB};
        Username=${POSTGRES_USER};
        Password=${POSTGRES_PASSWORD}
    ports:
      - "5140:8080"
    depends_on:
      - db

  ventas-api:
    build:
      context: ./VentasBackend
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DefaultConnection: >
        Host=db;
        Port=5432;
        Database=${POSTGRES_DB};
        Username=${POSTGRES_USER};
        Password=${POSTGRES_PASSWORD}
      InventoryApi__BaseUrl: http://inventory-api:8080
      Integration__InventarioBaseUrl: http://inventory-api:8080
    ports:
      - "5005:8080"
    depends_on:
      - db
      - inventory-api

  compras-api:
    build:
      context: ./ComprasBackend
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DefaultConnection: >
        Host=db;
        Port=5432;
        Database=${POSTGRES_DB};
        Username=${POSTGRES_USER};
        Password=${POSTGRES_PASSWORD}
      InventoryApi__BaseUrl: http://inventory-api:8080
    ports:
      - "5006:8080"
    depends_on:
      - db
      - inventory-api

volumes:
  pgdata:
```

### Pasos para iniciar el servidor (sin Docker)
1. **Configurar la base de datos:**
   Modifica la cadena de conexión en `appsettings.json` o `appsettings.Development.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=tulocal;Database=tudb;Username=tu_usuario;Password=tu_password"
   }
   ```
2. **Restaurar dependencias:**
   ```bash
   dotnet restore
   ```
3. **Aplicar migraciones:**
   ```bash
   dotnet ef database update
   ```
4. **Correr seeds (opcional):**
  ```bash
  dotnet run -- --seed
  ```
5. **Ejecutar la aplicación:**
   ```bash
   dotnet run
   ```
   La API estara activa en http://localhost:5140 (por defecto) y puedes acceder a Swagger en /swagger.

##  Estructura general del repositorio
```text
├── Application/    # Casos de uso, DTOs y reglas de aplicación
├── Domain/         # Entidades base y definiciones del dominio
├── Infrastructure/ # Persistencia de base de datos y servicios externos
├── Presentation/   # Controladores API y configuración de la aplicación
└── Filters/        # Middlewares y filtros de excepción personalizados
```
