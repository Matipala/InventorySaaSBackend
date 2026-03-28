## InventorySaaSBackend

Este proyecto es el backend de InventorySaaS, una solución SaaS para la gestión de inventarios empresariales.

### Tecnologías principales
- .NET 9
- Entity Framework Core
- PostgreSQL
- Arquitectura en capas (Domain, Application, Infrastructure, Presentation)

### Estructura del proyecto
- **Application/**: Lógica de negocio y servicios
- **Domain/**: Entidades, DTOs y contexto de datos
- **Infrastructure/**: Implementaciones, migraciones y servicios de infraestructura
- **Presentation/**: Controladores y endpoints de la API

### Configuración y ejecución
1. Clona el repositorio
2. Configura la cadena de conexión en `appsettings.json`
3. Aplica las migraciones:
	 ```bash
	 dotnet ef database update
	 ```
4. Ejecuta el proyecto:
	 ```bash
	 dotnet run
	 ```

### Scripts útiles
- Crear migración:
	```bash
	dotnet ef migrations add NombreMigracion
	```
- Actualizar base de datos:
	```bash
	dotnet ef database update
	```

