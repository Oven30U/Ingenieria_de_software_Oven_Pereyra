# Sistema de Usuarios - Ingeniería de Software
### UAI - Ingeniería en Sistemas | Oven Pereyra

---

## ¿Qué es este sistema?

Sistema de gestión de usuarios con login y ABM (Alta, Baja, Modificación) desarrollado en **C# Windows Forms** con **ADO.NET** y **SQL Server**. Utiliza arquitectura en capas (Presentación, Lógica de Negocio, Datos y Modelo) y encriptación **SHA-256** para las contraseñas.

---

## Requisitos previos

- **Visual Studio 2019 o superior** (con soporte para .NET Framework)
- **SQL Server** (cualquier versión: Express, Developer, Standard, etc.)
- **SQL Server Management Studio (SSMS)** (opcional, para verificar la base de datos)
- Paquete NuGet: `System.Data.SqlClient`

---

## Estructura del proyecto

```
Ingenieria de Software - Oven Pereyra/
├── Datos/
│   └── UsuarioDAO.cs          → Capa de datos (SQL, conexión a BD, encriptación SHA-256)
├── Forms/
│   └── FormABM.cs             → Formulario ABM de usuarios
├── Logica/
│   └── UsuarioService.cs      → Capa de lógica de negocio
├── Modelos/
│   └── Usuario.cs             → Modelo de datos
├── Form1.cs                   → Pantalla principal (Login)
├── Program.cs                 → Punto de entrada, inicializa la BD automáticamente
├── App.config                 → Configuración de conexión a la BD
└── script_bd.sql              → Script SQL de respaldo
```

---

## Pasos para ejecutar en cualquier computadora con SQL Server

### Paso 1: Clonar o descargar el proyecto

```
git clone <URL_DEL_REPOSITORIO>
```
O descargá el ZIP desde GitHub y descomprimilo.

### Paso 2: Abrir el proyecto en Visual Studio

1. Abrí Visual Studio
2. Click en **Abrir un proyecto o solución**
3. Seleccioná el archivo `.sln`

### Paso 3: Instalar el paquete NuGet

`Herramientas → Administrador de paquetes NuGet → Consola del administrador de paquetes`

```
Install-Package System.Data.SqlClient
```

### Paso 4: Verificar que SQL Server está corriendo

#### Opción A: Desde el buscador de Windows
1. Presioná la tecla **Windows**
2. Escribí **"Servicios"** y presioná Enter
3. Buscá **"SQL Server (MSSQLSERVER)"** o **"SQL Server (SQLEXPRESS)"**
4. Si está detenido → click derecho → **Iniciar**

#### Opción B: Desde Ejecutar (Windows + R)
1. Presioná **Windows + R**
2. Escribí `services.msc` y presioná **Enter**
3. Seguí los pasos anteriores

#### Opción C: Desde CMD como administrador
Para SQL Server Express:
```
net start MSSQL$SQLEXPRESS
```
Para SQL Server estándar:
```
net start MSSQLSERVER
```

#### Para que arranque automáticamente siempre:
1. Abrí Servicios
2. Click derecho sobre SQL Server → **Propiedades**
3. Tipo de inicio → **Automático** → Aceptar

---

### Paso 5: Obtener el nombre del servidor SQL

1. Abrí **SQL Server Management Studio (SSMS)**
2. Copiá el valor del campo **"Nombre del servidor"**

Ejemplos comunes:
- `DESKTOP-ABC123\SQLEXPRESS`
- `NOMBRE-PC\SQLEXPRESS`
- `localhost`
- `NOMBRE-PC`

### Paso 6: Configurar la conexión en App.config

Abrí `App.config` y modificá el `connectionString`:

```xml
<connectionStrings>
  <add name="BaseDatos"
       connectionString="Server=TU_SERVIDOR;Database=IngenieriaSoftware;Integrated Security=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Reemplazá `TU_SERVIDOR` por el nombre de tu servidor. Ejemplo:
```xml
connectionString="Server=DESKTOP-ABC123\SQLEXPRESS;Database=IngenieriaSoftware;Integrated Security=True;"
```

> **Importante:** En App.config usá `\` simple. En código C# se usa `\\` doble.

### Paso 7: Ejecutar el sistema

Presioná **F5** en Visual Studio. El sistema automáticamente:
- ✅ Crea la base de datos `IngenieriaSoftware` si no existe
- ✅ Crea la tabla `Usuarios` con columna `Clave NVARCHAR(64)` para el hash SHA-256
- ✅ Agrega la columna `Rol` si faltaba
- ✅ Crea el usuario administrador con la clave encriptada en SHA-256
- ✅ Actualiza la clave del admin al hash correcto en cada arranque

**No es necesario ejecutar ningún script SQL manualmente.**

---

## Credenciales por defecto

| Campo   | Valor      |
|---------|------------|
| Usuario | `admin`    |
| Clave   | `admin123` |
| Rol     | `admin`    |

> La clave se almacena encriptada con SHA-256. Nunca se guarda en texto plano.

---

## Funcionamiento del sistema

### Pantalla principal
- Ingresás usuario y contraseña → click en **Iniciar sesión**
- Si las credenciales son correctas, se ocultan los campos de login
- Si el usuario es **admin** → aparece el botón **Administrar Usuarios**
- Todos los usuarios ven el botón **Cerrar sesión**

### ABM de Usuarios (solo admin)
- **Agregar:** completá usuario, contraseña y rol → click en Agregar
- **Modificar:** seleccioná un usuario de la grilla, modificá los campos → click en Modificar. Si dejás la clave vacía, se mantiene la clave actual
- **Eliminar:** seleccioná un usuario → click en Eliminar. El usuario `admin` no puede ser eliminado
- **Limpiar:** limpia los campos del formulario

### Roles disponibles
- `usuario` → solo puede iniciar y cerrar sesión
- `admin` → puede iniciar sesión y administrar usuarios

---

## Encriptación SHA-256

Las contraseñas **nunca se almacenan en texto plano**. El sistema aplica SHA-256 en:
- **Login:** la clave ingresada se hashea antes de comparar con la BD
- **Alta de usuario:** la clave se hashea antes de insertar
- **Modificación:** la nueva clave se hashea antes de actualizar

Ejemplo:
```
Clave ingresada: admin123
Hash SHA-256:    240be518fabd2724ddb6f04eeb1da5967448d7e8f9324d292ec231629a657db6
Lo que se guarda en la BD: el hash (64 caracteres)
```

---

## Arquitectura del sistema

| Capa | Archivo | Responsabilidad |
|------|---------|-----------------|
| Presentación | `Form1.cs`, `FormABM.cs` | Interfaz visual, eventos de usuario |
| Lógica de negocio | `UsuarioService.cs` | Validaciones y reglas del sistema |
| Datos | `UsuarioDAO.cs` | SQL, conexión a BD y encriptación |
| Modelo | `Usuario.cs` | Definición de la entidad Usuario |

---

## Solución de problemas comunes

| Error | Causa | Solución |
|-------|-------|----------|
| `Error relacionado con la red` | SQL Server no está corriendo | Iniciar el servicio (ver Paso 4) |
| `Cannot open database` | Base de datos no existe o nombre incorrecto | Verificar `connectionString` en `App.config` |
| `Invalid column name 'Rol'` | Columna Rol no existe | El sistema la crea automáticamente al iniciar |
| `Login failed for user` | Autenticación incorrecta | Usar `Integrated Security=True` |
| El .exe está bloqueado al compilar | Otro proceso bloquea el archivo (ej: Valorant) | Cerrar el proceso desde Administrador de tareas (Ctrl+Shift+Esc) |
| Usuario o clave incorrectos | Hash en BD no coincide | El sistema actualiza el hash del admin automáticamente al arrancar |

---

## Tecnologías utilizadas

- **Lenguaje:** C# (.NET Framework)
- **Interfaz:** Windows Forms
- **Base de datos:** Microsoft SQL Server
- **Acceso a datos:** ADO.NET (`SqlConnection`, `SqlCommand`)
- **Encriptación:** SHA-256 (`System.Security.Cryptography`)
- **Configuración:** App.config con `ConnectionStrings`
