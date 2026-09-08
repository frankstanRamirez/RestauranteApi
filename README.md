# RestaurantFlow API

API REST para la gestión integral de restaurantes, desarrollada con .NET siguiendo los principios de Clean Architecture.

## Descripción

RestaurantFlow proporciona una solución backend completa para la administración de restaurantes, incluyendo autenticación, gestión de órdenes en tiempo real, reservaciones, notificaciones y reportes operativos. Está diseñada para adaptarse a distintos negocios del sector, sin depender de un cliente específico.

## Funcionalidades

- Autenticación y autorización mediante JWT
- Gestión de órdenes en tiempo real
- Sistema de reservaciones
- Notificaciones en tiempo real vía SignalR (módulo disponible, pendiente de activación hasta contar con cliente frontend)
- Reportes y estadísticas de operación
- Documentación interactiva de la API mediante Swagger

## Arquitectura

El proyecto sigue el patrón de Clean Architecture, organizado en las siguientes capas:

| Proyecto | Responsabilidad |
|---|---|
| `RestaurantFlow.Domain` | Entidades y reglas de negocio del dominio |
| `RestaurantFlow.Application` | Casos de uso, interfaces y lógica de aplicación |
| `RestaurantFlow.Infrastructure` | Implementaciones concretas (persistencia, servicios externos) |
| `RestaurantFlow.Api` | Punto de entrada HTTP, controladores y configuración |

La regla de dependencia se mantiene estricta: `Infrastructure` depende de `Application`, nunca al revés.

## Tecnologías

- .NET 9 / ASP.NET Core
- Entity Framework Core (SQLite / SQL Server)
- FluentValidation
- JWT (System.IdentityModel.Tokens.Jwt)
- SignalR
- Swagger / OpenAPI
- Serilog

## Requisitos previos

- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) o superior
- Un motor de base de datos compatible (SQLite para desarrollo local, SQL Server para producción)

## Inicio rápido

Clonar el repositorio:

```bash
git clone https://github.com/frankstanRamirez/RestauranteApi.git
cd RestauranteApi
```

Restaurar dependencias:

```bash
dotnet restore
```

Configurar la cadena de conexión en `appsettings.Development.json` según el motor de base de datos a utilizar.

Ejecutar la API:

```bash
cd RestaurantFlow.Api
dotnet run
```

La documentación interactiva estará disponible en:

```
https://localhost:{puerto}/swagger
```

## Autenticación

1. Realizar login en `POST /api/Auth/login`
2. Usar el token JWT recibido en el encabezado `Authorization: Bearer {token}`
3. Acceder a los endpoints protegidos

## Licencia

Este proyecto se distribuye bajo licencia MIT.
