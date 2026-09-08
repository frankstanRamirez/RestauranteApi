# 🍽️ RestaurantFlow API - Villa El Paraíso

API completa para la gestión integral de restaurantes con tecnologías modernas.

## 🚀 Inicio Rápido

### ⚠️ Si tienes errores de base de datos:

**Paso 1: Ejecuta la configuración automática**
```bash
# Doble click en este archivo para arreglar todo:
SETUP_DATABASE.bat
```

**Paso 2: Si el Paso 1 falla (SQL Server no disponible)**
```bash
# Doble click para usar SQLite como alternativa:
EMERGENCY_SETUP.bat
```

### ✅ Una vez configurado:

**Opción A: Doble click y listo**
```bash
# Ejecuta este archivo para iniciar todo automáticamente
DEMO_START.bat
```

**Opción B: Desde Visual Studio**
1. Abre `RestaurantFlow.sln` en Visual Studio
2. Presiona `F5` o click en "▶️ Start"
3. Se abrirá automáticamente Swagger en tu navegador

**Opción C: Desde terminal**
```bash
cd RestaurantFlow.Api
dotnet run --launch-profile https
```

## 📋 URLs de Acceso

- **🏠 Inicio:** https://localhost:7297 (redirige a Swagger)
- **📖 Swagger UI:** https://localhost:7297/swagger
- **🔌 API Base:** https://localhost:7297/api
- **💓 Health Check:** https://localhost:7297/health
- **🔔 SignalR Hub:** https://localhost:7297/hubs/restaurant

## 🔐 Autenticación

### Usuarios de Prueba
| Usuario | Contraseña | Rol |
|---------|------------|-----|
| `admin` | `admin123` | Administrador |
| `mesero` | `mesero123` | Mesero |
| `cocina` | `cocina123` | Cocina |

### Como autenticarte en Swagger:
1. Ve a `/api/Auth/login` en Swagger
2. Usa las credenciales de arriba
3. Copia el `token` que recibes
4. Haz click en 🔒 **Authorize** (arriba derecha)
5. Pega: `Bearer tu-token-aquí`
6. ¡Listo! Ya puedes probar todos los endpoints protegidos

## 🏗️ Tecnologías Incluidas

- **🎯 .NET 9** - Framework principal
- **📊 Entity Framework Core** - ORM para base de datos
- **🔒 JWT Authentication** - Autenticación segura
- **📖 Swagger/OpenAPI** - Documentación automática
- **🔔 SignalR** - Notificaciones en tiempo real
- **📝 Serilog** - Logging estructurado
- **🗄️ SQL Server** - Base de datos

## 📚 Funcionalidades Principales

### 🎯 Módulos Disponibles
- **👥 Autenticación** - Login/logout con JWT
- **📋 Gestión de Órdenes** - CRUD completo de pedidos
- **🪑 Mesas y Reservaciones** - Control de disponibilidad
- **🍕 Productos y Categorías** - Catálogo completo
- **💰 Procesamiento de Pagos** - Múltiples métodos
- **🍳 Módulo de Cocina** - Estados de preparación
- **📊 Dashboard** - Estadísticas en tiempo real
- **🔔 Notificaciones** - Push notifications con SignalR

### 🔥 Características Especiales
- ✅ **Datos de demo incluidos** - No necesitas configurar nada
- ✅ **Base de datos automática** - Se crea sola al iniciar
- ✅ **Swagger completamente configurado** - Prueba todos los endpoints
- ✅ **Autenticación JWT integrada** - Seguridad lista para producción
- ✅ **SignalR funcionando** - Notificaciones en tiempo real
- ✅ **CORS configurado** - Listo para frontend
- ✅ **Logging profesional** - Con Serilog

## 🛠️ Configuración de Base de Datos

### ✅ Configuración Automática (Recomendado)
```bash
# Ejecutar para crear/configurar BD automáticamente:
SETUP_DATABASE.bat
```

### 🚨 Si SQL Server no funciona
```bash
# Usar SQLite como alternativa:
EMERGENCY_SETUP.bat
```

### SQL Server Local (Manual)
El proyecto usa SQL Server LocalDB por defecto:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=RestaurantFlowDb;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

### SQLite (Alternativa)
Si SQL Server falla, el proyecto puede usar SQLite automáticamente.

## 🐛 Resolución de Problemas

### Error de Base de Datos
```bash
# Regenerar base de datos
cd RestaurantFlow.Api
dotnet ef database drop --force
dotnet ef database update
```

### Error de Puertos
Si los puertos están ocupados, edita `launchSettings.json` y cambia:
```json
"applicationUrl": "https://localhost:NUEVO_PUERTO;http://localhost:OTRO_PUERTO"
```

### Error de Certificados HTTPS
```bash
# Confiar en certificados de desarrollo
dotnet dev-certs https --trust
```

## 📞 Contacto y Soporte

- **Demo:** Villa El Paraíso, Caluco
- **Desarrollado por:** [Tu Nombre]
- **Email:** demo@restaurantflow.com
- **Documentación completa:** En Swagger UI

---

## 🎯 Para Presentaciones

**Script de Demo:**
1. Ejecuta `DEMO_START.bat`
2. Muestra Swagger automático
3. Haz login con `admin/admin123`
4. Demuestra endpoints protegidos
5. Muestra notificaciones en tiempo real
6. Presenta dashboard y estadísticas

**¡Todo funciona out-of-the-box! 🚀**