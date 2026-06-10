# Sistema de Gestión de Usuarios
### UAI - Ingeniería en Sistemas | Oven Pereyra

---

## ¿Qué hace este sistema?

Sistema de gestión de usuarios con login, ABM (Alta, Baja, Modificación) y administración de permisos en árbol. Desarrollado en **C# Windows Forms** con **ADO.NET** y **SQL Server**.

**Funcionalidades principales:**
- Login con contraseña encriptada (SHA-256)
- ABM completo de usuarios (solo admin)
- Sistema de permisos jerárquico con patrón **Composite**
- Cambio de idioma en tiempo de ejecución (Español / Inglés) con patrón **Observer**
- Bitácora (log) automática de todas las acciones
- Inicialización automática de la base de datos al arrancar

---

## Arquitectura en capas

```
Ingenieria de Software - Oven Pereyra.sln
│
├── UI/                         → Capa de Presentación (Windows Forms)
│   ├── Program.cs              → Punto de entrada; inicializa BD y Bitácora
│   ├── Form1.cs                → Pantalla principal (Login / Logout)
│   ├── Forms/FormABM.cs        → ABM de usuarios
│   ├── Forms/FormComposite.cs  → Editor visual de árbol de permisos
│   └── App.config              → Cadena de conexión a SQL Server
│
├── BLL/                        → Capa de Lógica de Negocio
│   ├── UsuarioService.cs       → Reglas de negocio: login, ABM, árbol Composite
│   ├── Bitacora.cs             → Singleton de logging en archivos .log
│   ├── GestorIdioma.cs         → Subject del patrón Observer (cambio de idioma)
│   ├── PermisoSerializer.cs    → Serialización/deserialización del árbol a texto plano
│   └── SesionManager.cs        → Singleton que guarda el usuario logueado
│
├── DAL/                        → Capa de Datos
│   └── UsuarioDAL.cs           → Acceso a SQL Server con ADO.NET; encriptación SHA-256
│
└── Mapper/                     → Capa de Modelos (entidades compartidas)
    ├── Modelos/Usuario.cs           → Entidad Usuario
    ├── Modelos/GrupoPermiso.cs      → Nodo compuesto (patrón Composite)
    ├── Modelos/PermisoLeaf.cs       → Nodo hoja (patrón Composite)
    ├── Modelos/IComponentePermiso.cs → Interfaz del Composite
    ├── Modelos/Resultado.cs          → DTO de respuesta (Ok + Mensaje)
    └── Modelos/IObservadorIdioma.cs  → Interfaz Observer para cambio de idioma
```

---

## Patrones de diseño implementados

| Patrón | Clase(s) | Descripción |
|--------|----------|-------------|
| **Singleton** | `Bitacora`, `GestorIdioma`, `SesionManager` | Una única instancia global por clase |
| **Composite** | `GrupoPermiso`, `PermisoLeaf`, `IComponentePermiso` | Árbol jerárquico de permisos |
| **Observer** | `GestorIdioma` (Subject), `IObservadorIdioma` (Observer), `Form1`, `FormABM`, `FormComposite` (Observers) | Cambio de idioma en cascada |

---

## Requisitos previos

- **Visual Studio 2019 o superior** (con soporte para .NET Framework 4.7.2)
- **SQL Server** (Express, Developer o Standard)
- **SQL Server Management Studio** (opcional, para verificar la BD)
- Paquete NuGet: `System.Data.SqlClient`

---

## Pasos para ejecutar

### 1. Clonar o descargar el proyecto

```
git clone <URL_DEL_REPOSITORIO>
```

### 2. Abrir en Visual Studio

Abrí el archivo `Ingenieria de Software - Oven Pereyra.sln`.

### 3. Instalar el paquete NuGet

```
Herramientas → Administrador de paquetes NuGet → Consola
Install-Package System.Data.SqlClient
```

### 4. Verificar que SQL Server está corriendo

Desde CMD como administrador:
```
net start MSSQLSERVER          # SQL Server estándar
net start MSSQL$SQLEXPRESS     # SQL Server Express
```

### 5. Configurar la cadena de conexión

Editá `App.config` con el nombre de tu servidor:

```xml
<connectionStrings>
  <add name="BaseDatos"
       connectionString="Server=TU_SERVIDOR;Database=IngenieriaSoftware;Integrated Security=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Ejemplos de servidor: `localhost`, `.\SQLEXPRESS`, `NOMBRE-PC\SQLEXPRESS`

### 6. Ejecutar (F5)

El sistema crea automáticamente la base de datos, la tabla `Usuarios` y el usuario administrador. **No hace falta correr ningún script SQL.**

---

## Credenciales por defecto

| Campo   | Valor      |
|---------|------------|
| Usuario | `admin`    |
| Clave   | `admin123` |
| Rol     | `admin`    |

La clave se guarda como hash SHA-256 (64 caracteres hex). Nunca en texto plano.

---

## Flujo de uso

### Login
1. Ingresás usuario y contraseña → **Iniciar sesión**
2. Si las credenciales son correctas, aparecen los botones de administración (solo si sos admin)

### ABM de Usuarios (solo admin)
- **Agregar:** completá usuario, contraseña y rol → Agregar
- **Modificar:** seleccioná de la grilla, editá → Modificar (clave vacía = no cambia)
- **Eliminar:** seleccioná → Eliminar (el usuario `admin` no puede eliminarse)

### Gestión de Permisos
Desde el ABM, botón **Editar Permisos** → abre `FormComposite` con el árbol del usuario seleccionado. Podés agregar familias (grupos), parientes (hojas), enlazarlos y guardar.

### Cambio de idioma
Botón **English / Español** en la esquina superior derecha — actualiza todos los formularios abiertos en tiempo real.

---

## Base de datos

Tabla `Usuarios` en la base `IngenieriaSoftware`:

| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | INT IDENTITY | PK autoincremental |
| Usuario | NVARCHAR(100) UNIQUE | Nombre de usuario |
| Clave | NVARCHAR(64) | Hash SHA-256 |
| Rol | NVARCHAR(50) | `admin` o `usuario` |
| Permisos | NVARCHAR(MAX) | Árbol Composite serializado |
| TipoPermiso | NVARCHAR(100) | Nombre de la familia principal |

---

## Bitácora (logs)

Cada ejecución genera un archivo nuevo en `bin/Debug/logs/` con formato:
```
log_2026-06-09_21-00-51.log
```

Registra: login exitoso/fallido, logout, alta/baja/modificación de usuarios, cambios de permisos, cambios de idioma y errores de base de datos.

---

## Solución de problemas comunes

| Error | Causa | Solución |
|-------|-------|----------|
| `Error relacionado con la red` | SQL Server no está corriendo | Iniciar el servicio (ver paso 4) |
| `Cannot open database` | Nombre de servidor incorrecto | Verificar `connectionString` en `App.config` |
| `Invalid column name 'Rol'` | Tabla desactualizada | El sistema la agrega automáticamente al iniciar |
| `Login failed for user` | Autenticación de Windows | Usar `Integrated Security=True` |
| .exe bloqueado al compilar | Otro proceso lo usa (ej: antivirus) | Cerrar el proceso desde el Administrador de tareas |

---

## Tecnologías utilizadas

- **Lenguaje:** C# (.NET Framework 4.7.2)
- **Interfaz:** Windows Forms
- **Base de datos:** Microsoft SQL Server
- **Acceso a datos:** ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataReader`)
- **Encriptación:** SHA-256 (`System.Security.Cryptography`)
- **Configuración:** `App.config` con `ConnectionStrings`
