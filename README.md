# Sistema de Usuarios - Ingeniería de Software
### UAI - Ingeniería en Sistemas | Oven Pereyra

---

## ¿Qué es este sistema?

Sistema de gestión de usuarios con login y ABM (Alta, Baja, Modificación) desarrollado en **C# Windows Forms** con **ADO.NET** y **SQL Server**. Utiliza arquitectura en capas (Presentación, Lógica de Negocio y Datos).

---

## Requisitos previos

Antes de ejecutar el sistema, la computadora debe tener instalado:

- **Visual Studio 2019 o superior** (con soporte para .NET Framework)
- **SQL Server** (cualquier versión: Express, Developer, Standard, etc.)
- **SQL Server Management Studio (SSMS)** (opcional, para verificar la base de datos)
- Paquete NuGet: `System.Data.SqlClient` (se instala desde Visual Studio)

---

## Estructura del proyecto

```
Ingenieria de Software - Oven Pereyra/
├── Datos/
│   └── UsuarioDAO.cs          → Capa de datos (SQL, conexión a BD)
├── Forms/
│   └── FormABM.cs             → Formulario ABM de usuarios
├── Logica/
│   └── UsuarioService.cs      → Capa de lógica de negocio
├── Modelos/
│   └── Usuario.cs             → Modelo de datos
├── Form1.cs                   → Pantalla principal (Login)
├── Program.cs                 → Punto de entrada, inicializa la BD
├── App.config                 → Configuración de conexión a la BD
└── script_bd.sql              → Script SQL de respaldo
```

---

## Pasos para ejecutar en cualquier computadora con SQL Server

### Paso 1: Clonar o descargar el proyecto

Desde GitHub, copiá la URL del repositorio y ejecutá:
```
git clone <URL_DEL_REPOSITORIO>
```
O descargá el ZIP desde GitHub y descomprimilo.

### Paso 2: Abrir el proyecto en Visual Studio

1. Abrí Visual Studio
2. Click en **Abrir un proyecto o solución**
3. Navegá hasta la carpeta del proyecto y seleccioná el archivo `.sln`

### Paso 3: Instalar el paquete NuGet

En Visual Studio:
`Herramientas → Administrador de paquetes NuGet → Consola del administrador de paquetes`

Ejecutá:
```
Install-Package System.Data.SqlClient
```

### Paso 4: Verificar que SQL Server está corriendo

Esto es **obligatorio** antes de ejecutar el sistema. Si SQL Server no está activo, el sistema no va a poder conectarse a la base de datos.

#### Opción A: Desde el buscador de Windows (más fácil)
1. Presioná la tecla **Windows**
2. Escribí **"Servicios"** y presioná Enter
3. Se abre una ventana con todos los servicios del sistema
4. Buscá en la lista **"SQL Server (MSSQLSERVER)"** o **"SQL Server (SQLEXPRESS)"**
5. Fijate en la columna **"Estado"**:
   - Si dice **"En ejecución"** → ya está activo, podés continuar
   - Si está vacío o dice **"Detenido"** → hacé click derecho → **Iniciar**

#### Opción B: Desde Ejecutar (Windows + R)
1. Presioná **Windows + R** al mismo tiempo
2. Escribí `services.msc` y presioná **Enter**
3. Seguí los pasos 4 y 5 de la Opción A

#### Opción C: Desde el símbolo del sistema (CMD)
1. Presioná la tecla **Windows**
2. Escribí **"cmd"**
3. Click derecho sobre **"Símbolo del sistema"** → **"Ejecutar como administrador"**
4. Escribí el siguiente comando y presioná Enter:

Para SQL Server Express:
```
net start MSSQL$SQLEXPRESS
```
Para SQL Server estándar:
```
net start MSSQLSERVER
```
5. Si dice **"El servicio ya está en ejecución"** o **"El servicio se ha iniciado correctamente"** podés continuar

#### Para que SQL Server arranque automáticamente siempre:
1. Abrí **Servicios** (con cualquier opción de las anteriores)
2. Encontrá el servicio de SQL Server
3. Click derecho → **Propiedades**
4. En **"Tipo de inicio"** seleccioná **"Automático"**
5. Click en **Aceptar**

Así no tenés que iniciarlo manualmente cada vez que prendés la computadora.

---

### Paso 5: Obtener el nombre del servidor SQL

1. Abrí **SQL Server Management Studio (SSMS)**
2. Va a aparecer la pantalla de conexión
3. Copiá el valor del campo **"Nombre del servidor"**

Ejemplos comunes:
- `DESKTOP-ABC123\SQLEXPRESS`
- `NOMBRE-PC\SQLEXPRESS`
- `localhost`
- `NOMBRE-PC`

### Paso 6: Configurar la conexión en App.config

Abrí el archivo `App.config` del proyecto en Visual Studio y modificá el `connectionString`:

```xml
<connectionStrings>
  <add name="BaseDatos"
       connectionString="Server=TU_SERVIDOR;Database=IngenieriaSoftware;Integrated Security=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Reemplazá `TU_SERVIDOR` por el nombre que obtuviste en el Paso 5. Por ejemplo:
```xml
connectionString="Server=DESKTOP-ABC123\SQLEXPRESS;Database=IngenieriaSoftware;Integrated Security=True;"
```

> **Importante:** En el archivo App.config usá `\` simple. En el código C# se usa `\\` doble, pero en XML va simple.

### Paso 7: Ejecutar el sistema

Presioná **F5** en Visual Studio. El sistema automáticamente:
- ✅ Crea la base de datos `IngenieriaSoftware` si no existe
- ✅ Crea la tabla `Usuarios` si no existe
- ✅ Agrega la columna `Rol` si faltaba
- ✅ Crea el usuario administrador por defecto si no existe

**No es necesario ejecutar ningún script SQL manualmente.**

---

## Credenciales por defecto

| Campo   | Valor      |
|---------|------------|
| Usuario | `admin`    |
| Clave   | `admin123` |
| Rol     | `admin`    |

> Se recomienda cambiar la clave del administrador después del primer inicio de sesión desde el ABM de usuarios.

---

## Funcionamiento del sistema

### Pantalla principal
- Ingresás usuario y contraseña y hacés click en **Iniciar sesión**
- Si las credenciales son correctas, se ocultan los campos de login
- Si el usuario es **admin**, aparece el botón **Administrar Usuarios**
- Todos los usuarios ven el botón **Cerrar sesión**

### ABM de Usuarios (solo admin)
- **Agregar:** completá usuario, contraseña y rol, luego click en Agregar
- **Modificar:** seleccioná un usuario de la grilla, modificá los campos deseados y click en Modificar. Si dejás la clave vacía, se mantiene la clave actual
- **Eliminar:** seleccioná un usuario y click en Eliminar. El usuario `admin` no puede ser eliminado
- **Limpiar:** limpia los campos del formulario

### Roles disponibles
- `usuario` → solo puede iniciar y cerrar sesión
- `admin` → puede iniciar sesión y administrar usuarios

---

## Arquitectura del sistema

El sistema está desarrollado siguiendo el patrón de **arquitectura en capas**:

| Capa | Archivo | Responsabilidad |
|------|---------|-----------------|
| Presentación | `Form1.cs`, `FormABM.cs` | Interfaz visual, interacción con el usuario |
| Lógica de negocio | `UsuarioService.cs` | Reglas del sistema (validaciones, restricciones) |
| Datos | `UsuarioDAO.cs` | Consultas SQL y conexión a la base de datos |
| Modelo | `Usuario.cs` | Definición de la entidad Usuario |

---

## Solución de problemas comunes

| Error | Causa | Solución |
|-------|-------|----------|
| `Error relacionado con la red` | SQL Server no está corriendo | Iniciar el servicio con cualquiera de las opciones del Paso 4 |
| `Cannot open database` | Base de datos no existe o nombre incorrecto | Verificar el `connectionString` en `App.config` |
| `Invalid column name 'Rol'` | La columna Rol no existe en la tabla | El sistema la crea automáticamente al iniciar |
| `Login failed for user` | Autenticación incorrecta | Usar `Integrated Security=True` para Windows Authentication |
| El .exe está bloqueado al compilar | Otro proceso bloquea el archivo | Cerrar el proceso bloqueante desde el Administrador de tareas (Ctrl+Shift+Esc) |

---

## Tecnologías utilizadas

- **Lenguaje:** C# (.NET Framework)
- **Interfaz:** Windows Forms
- **Base de datos:** Microsoft SQL Server
- **Acceso a datos:** ADO.NET (`SqlConnection`, `SqlCommand`)
- **Configuración:** App.config con `ConnectionStrings`
