# RestaurantFlow API

API REST profesional y completa para la gestión integral de restaurantes, desarrollada con .NET 10 siguiendo los principios de Clean Architecture.

## 📋 Descripción

RestaurantFlow proporciona una solución backend completa para la administración de restaurantes, incluyendo:

- ✅ Autenticación y autorización mediante JWT
- ✅ Gestión de usuarios por rol (Admin, Waiter, Kitchen, Cashier)
- ✅ Gestión completa de menú (categorías y productos)
- ✅ Gestión de mesas con control de estado
- ✅ Sistema de órdenes con detalles de items
- ✅ Sistema de reservaciones con disponibilidad
- ✅ Dashboard con estadísticas en tiempo real
- ✅ Reportes de ventas y revenue
- ✅ Notificaciones en tiempo real vía SignalR
- ✅ Documentación interactiva con Swagger

## 🏗️ Arquitectura

El proyecto sigue Clean Architecture con 4 capas:

| Proyecto | Responsabilidad |
|---|---|
| `RestaurantFlow.Domain` | Entidades, enums y reglas de negocio |
| `RestaurantFlow.Application` | DTOs, interfaces, servicios de aplicación |
| `RestaurantFlow.Infrastructure` | EF Core, persistencia, autenticación |
| `RestaurantFlow.Api` | Controladores HTTP, configuración |

## 🛠️ Tecnologías

- .NET 10 / ASP.NET Core
- Entity Framework Core (SQLite / SQL Server)
- JWT Authentication
- BCrypt para hashing de contraseñas
- SignalR para notificaciones en tiempo real
- Swagger/OpenAPI para documentación
- Serilog para logging

## ⚙️ Requisitos previos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQLite (desarrollo local) o SQL Server (producción)

## 🚀 Inicio rápido

```bash
# Clonar repositorio
git clone https://github.com/frankstanRamirez/RestauranteApi.git
cd RestauranteApi

# Restaurar dependencias
dotnet restore

# Ejecutar la API
cd RestaurantFlow.Api
dotnet run
```

La API estará disponible en: `https://localhost:5001`  
Swagger estará en: `https://localhost:5001/swagger`

## 🔐 Autenticación

Todos los endpoints (excepto `/api/Auth/login`) requieren un token JWT.

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin",
  "password": "admin123"
}
```

**Respuesta:**
```json
{
  "token": "eyJhbGc...",
  "userId": 1,
  "name": "Administrador",
  "email": "admin",
  "role": "ADMIN",
  "restaurantId": 1,
  "restaurantName": "Villa El Paraíso"
}
```

Usar el token en el header: `Authorization: Bearer {token}`

## 📚 API Endpoints

### 👥 **Users Controller** - Gestión de Usuarios (ADMIN only)

| Método | Endpoint | Descripción |
|--------|----------|------------|
| GET | `/api/users` | Listar todos los usuarios del restaurante |
| GET | `/api/users/{id}` | Obtener usuario por ID |
| POST | `/api/users` | Crear nuevo usuario |
| PUT | `/api/users/{id}` | Editar usuario |
| PUT | `/api/users/{id}/deactivate` | Desactivar usuario |

**Crear Usuario:**
```http
POST /api/users
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Juan Mesero",
  "email": "juan@villaelparaiso.com",
  "password": "password123",
  "role": "WAITER"
}
```

Roles: `ADMIN`, `WAITER`, `KITCHEN`, `CASHIER`

---

### 🍽️ **Products Controller** - Gestión de Productos

| Método | Endpoint | Descripción | Rol |
|--------|----------|-------------|-----|
| GET | `/api/products` | Listar todos los productos | Todos |
| GET | `/api/products/{id}` | Obtener producto por ID | Todos |
| POST | `/api/products` | Crear producto | ADMIN |
| PUT | `/api/products/{id}` | Editar producto | ADMIN |
| DELETE | `/api/products/{id}` | Eliminar producto | ADMIN |

**Crear Producto:**
```http
POST /api/products
Authorization: Bearer {token}
Content-Type: application/json

{
  "categoryId": 1,
  "name": "Pupusas Revueltas",
  "description": "Pupusas de queso con chicharrón",
  "price": 0.75,
  "imageUrl": "https://...",
  "isAvailable": true
}
```

---

### 📂 **Categories Controller** - Gestión de Categorías

| Método | Endpoint | Descripción | Rol |
|--------|----------|-------------|-----|
| GET | `/api/categories` | Listar categorías activas | Todos |
| GET | `/api/categories/{id}` | Obtener categoría por ID | Todos |
| POST | `/api/categories` | Crear categoría | ADMIN |
| PUT | `/api/categories/{id}` | Editar categoría | ADMIN |
| DELETE | `/api/categories/{id}` | Desactivar categoría | ADMIN |

**Crear Categoría:**
```http
POST /api/categories
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Platos Típicos",
  "description": "Comida tradicional salvadoreña"
}
```

---

### 🪑 **Tables Controller** - Gestión de Mesas

| Método | Endpoint | Descripción | Rol |
|--------|----------|-------------|-----|
| GET | `/api/tables` | Listar todas las mesas | Todos |
| GET | `/api/tables/{id}` | Obtener mesa por ID | Todos |
| POST | `/api/tables` | Crear mesa | ADMIN |
| PUT | `/api/tables/{id}` | Editar mesa | ADMIN |
| PUT | `/api/tables/{id}/status` | Cambiar estado de mesa | ADMIN, WAITER |

**Estados de mesa:** `AVAILABLE`, `OCCUPIED`, `RESERVED`, `MAINTENANCE`

**Crear Mesa:**
```http
POST /api/tables
Authorization: Bearer {token}
Content-Type: application/json

{
  "number": 1,
  "capacity": 4,
  "positionX": 100,
  "positionY": 100,
  "width": 80,
  "height": 80,
  "shape": "SQUARE"
}
```

**Cambiar Estado:**
```http
PUT /api/tables/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "OCCUPIED"
}
```

---

### 📦 **Orders Controller** - Gestión de Órdenes

| Método | Endpoint | Descripción | Rol |
|--------|----------|-------------|-----|
| GET | `/api/orders` | Listar órdenes (filtrar por `?status=PENDING`) | Todos |
| GET | `/api/orders/{id}` | Obtener orden con detalles | Todos |
| GET | `/api/orders/table/{tableId}` | Órdenes de una mesa | Todos |
| POST | `/api/orders` | Crear orden | WAITER, ADMIN |
| PUT | `/api/orders/{id}/status` | Cambiar estado de orden | Todos |
| POST | `/api/orders/{id}/items` | Agregar items a orden | WAITER, ADMIN |
| DELETE | `/api/orders/{id}/items/{itemId}` | Remover item de orden | WAITER, ADMIN |

**Estados de orden:** `PENDING`, `SENT_TO_KITCHEN`, `PREPARING`, `READY`, `DELIVERED`, `WAITING_PAYMENT`, `PAID`, `CANCELLED`

**Crear Orden:**
```http
POST /api/orders
Authorization: Bearer {token}
Content-Type: application/json

{
  "tableId": 1,
  "notes": "Sin cebolla",
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "notes": "Extra queso"
    },
    {
      "productId": 3,
      "quantity": 1
    }
  ]
}
```

**Cambiar Estado:**
```http
PUT /api/orders/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "PREPARING"
}
```

**Agregar Items:**
```http
POST /api/orders/{id}/items
Authorization: Bearer {token}
Content-Type: application/json

[
  {
    "productId": 2,
    "quantity": 1,
    "notes": "Normal"
  }
]
```

---

### 📅 **Reservations Controller** - Gestión de Reservaciones

| Método | Endpoint | Descripción | Rol |
|--------|----------|-------------|-----|
| GET | `/api/reservations` | Listar reservaciones | Todos |
| GET | `/api/reservations/{id}` | Obtener reservación | Todos |
| GET | `/api/reservations/upcoming?days=7` | Próximas reservaciones | Todos |
| POST | `/api/reservations` | Crear reservación | Todos |
| PUT | `/api/reservations/{id}` | Editar reservación | WAITER, ADMIN |
| PUT | `/api/reservations/{id}/confirm` | Confirmar reservación | WAITER, ADMIN |
| PUT | `/api/reservations/{id}/cancel` | Cancelar reservación | WAITER, ADMIN |
| PUT | `/api/reservations/{id}/seat` | Sentar cliente (check-in) | WAITER, ADMIN |
| POST | `/api/reservations/check-availability` | Verificar disponibilidad | Todos |

**Crear Reservación:**
```http
POST /api/reservations
Authorization: Bearer {token}
Content-Type: application/json

{
  "tableId": 1,
  "customerName": "Roberto García",
  "customerPhone": "+503 7123-4567",
  "customerEmail": "roberto@email.com",
  "numberOfPeople": 4,
  "reservationDate": "2026-09-15",
  "startTime": "19:00:00",
  "endTime": "21:00:00",
  "notes": "Cumpleaños"
}
```

**Verificar Disponibilidad:**
```http
POST /api/reservations/check-availability
Authorization: Bearer {token}
Content-Type: application/json

{
  "date": "2026-09-15",
  "startTime": "19:00:00",
  "endTime": "21:00:00",
  "numberOfPeople": 4
}
```

---

### 📊 **Dashboard Controller** - Reportes y Estadísticas

| Endpoint | Descripción |
|----------|------------|
| GET `/api/dashboard/summary` | Resumen general (ventas hoy, órdenes, mesas) |
| GET `/api/dashboard/orders-summary` | Órdenes por estado |
| GET `/api/dashboard/revenue?period=month` | Ingresos por período |
| GET `/api/dashboard/top-products` | Productos más vendidos |
| GET `/api/dashboard/sales?startDate=2026-09-01&endDate=2026-09-08` | Ventas por rango de fechas |
| GET `/api/dashboard/tables-stats` | Estadísticas de mesas |
| GET `/api/dashboard/metrics` | Métricas generales |

**Parámetros de Revenue:**
- `period`: `day` (mes/año), `week`, `month` (año), `year`
- Ejemplos:
  - `?period=month&year=2026&month=9` - Diario de septiembre 2026
  - `?period=week` - Semanal actual
  - `?period=month&year=2026` - Mensual de 2026
  - `?period=year` - Últimos 5 años

**Respuesta Summary:**
```json
{
  "salesToday": 250.50,
  "salesThisMonth": 5230.75,
  "ordersToday": 15,
  "pendingOrders": 3,
  "ordersInKitchen": 2,
  "tablesOccupied": 5,
  "tablesAvailable": 5,
  "totalTables": 10,
  "reservationsToday": 2
}
```

---

## 🔑 Roles y Permisos

| Rol | Permisos |
|-----|----------|
| **ADMIN** | Gestión completa de usuarios, productos, categorías, mesas, órdenes, reservaciones |
| **WAITER** | Crear órdenes, cambiar estado, crear/editar/confirmar reservaciones, sentar clientes |
| **KITCHEN** | Ver órdenes PENDING, cambiar a PREPARING/READY |
| **CASHIER** | Ver órdenes, procesar pagos, cambiar a PAID |

---

## 🔒 Seguridad

- ✅ JWT Bearer Token de 7 días de validez
- ✅ Contraseñas hasheadas con BCrypt
- ✅ Filtrado automático por RestaurantId (multi-tenant seguro)
- ✅ CORS configurado para localhost:3000, :5173
- ✅ Validación de datos en todos los endpoints
- ✅ No se exponen datos de otros restaurantes

---

## 📝 Validaciones

### Usuarios
- Email único por restaurante
- Contraseña mínimo 6 caracteres
- Rol debe ser válido (ADMIN, WAITER, KITCHEN, CASHIER)

### Productos
- Precio > 0
- Categoría debe pertenecer al restaurante
- Nombre requerido

### Órdenes
- Mínimo 1 item
- Solo se pueden editar órdenes no pagadas
- Total = Subtotal + (Subtotal * 13% IVA)

### Reservaciones
- Cliente y teléfono requeridos
- Fecha no puede ser pasada
- Mesa debe tener capacidad suficiente
- No puede haber solapamientos en misma mesa/hora

---

## 🗄️ Base de Datos

### Seed Data Inicial
Al iniciar, se crea automáticamente:
- 1 Restaurante: "Villa El Paraíso"
- 3 Usuarios: admin (admin123), mesero (mesero123), cocina (cocina123)
- 10 Mesas con configuración visual
- 5 Categorías de productos
- 20 Productos típicos salvadoreños

---

## 📖 Documentación Interactiva

Acceder a Swagger en: **https://localhost:5001/swagger**

Swagger permite:
- Ver todos los endpoints
- Probar endpoints directamente
- Autenticar con JWT
- Ver esquemas de request/response

---

## 🐛 Códigos de Error

| Código | Significado |
|--------|-----------|
| 200 | OK - Operación exitosa |
| 201 | Created - Recurso creado |
| 400 | Bad Request - Datos inválidos |
| 401 | Unauthorized - Token requerido o inválido |
| 403 | Forbidden - Sin permisos |
| 404 | Not Found - Recurso no existe |
| 500 | Internal Server Error - Error del servidor |

---

## 📧 Support

Para reportar bugs o sugerencias, crear un issue en el repositorio.

## 📄 Licencia

Este proyecto se distribuye bajo licencia MIT.
