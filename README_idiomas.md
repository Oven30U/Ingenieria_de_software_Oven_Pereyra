# Cambios — Sistema de Idiomas con BD

## Qué se modificó / agregó

### Archivos NUEVOS

| Archivo | Capa | Descripción |
|---------|------|-------------|
| `Mapper/Modelos/Idioma.cs` | Mapper | Modelo: `Id`, `Nombre` |
| `Mapper/Modelos/Palabra.cs` | Mapper | Modelo: `Tag` (clave de traducción) |
| `Mapper/Modelos/Traduccion.cs` | Mapper | Modelo: `IdIdioma`, `Tag`, `Texto` |
| `DAL/Datos/IdiomaDAL.cs` | DAL | CRUD para las 3 tablas + seed automático |
| `UI/Forms/FormIdiomas.cs` | UI | ABM completo de idiomas + editor de traducciones |
| `idiomas_schema.sql` | SQL | Script de creación de tablas + datos semilla |

### Archivos MODIFICADOS

| Archivo | Qué cambió |
|---------|-----------|
| `BLL/Logica/GestorIdioma.cs` | Reescrito: lee BD, expone `T(tag)`, gestiona ABM |
| `UI/Form1.cs` | Reemplaza el botón toggle por un **ComboBox** de idiomas + botón ⚙ Idiomas |

---

## Arquitectura de BD

```
Idiomas          Palabras
──────────       ────────
Id (PK IDENTITY) Tag (PK)
Nombre
       ↘              ↙
          Traducciones
          ────────────
          IdIdioma (FK → Idiomas.Id)
          Tag      (FK → Palabras.Tag)
          Traduccion
          PK compuesta (IdIdioma, Tag)
```

### Ejemplo de datos (formato pedido: `esp = 1,inicio,home`)

```sql
-- Abreviatura = primeras 3 letras del nombre del idioma
esp = 1, btn_login,    Iniciar sesión
eng = 2, btn_login,    Log in
esp = 1, titulo_principal, Sistema de Usuarios
eng = 2, titulo_principal, User System
```

---

## Cómo funciona el flujo

1. **Al arrancar** → `GestorIdioma.Instancia` llama a `IdiomaDAL.InicializarTablas()`
   automáticamente desde `UsuarioDAL.InicializarBaseDatos()` (agregar esa llamada en `Program.cs`).
2. Los idiomas se cargan en un `ComboBox` en la esquina superior derecha de `Form1`.
3. Al seleccionar un idioma del combo → `GestorIdioma.CambiarIdioma(id)` → carga traducciones
   desde BD → llama `Notificar()` → todos los formularios suscritos actualizan sus textos.
4. El botón **⚙ Idiomas** abre `FormIdiomas` donde podés:
   - Ver todos los idiomas en una grilla.
   - **Agregar** un idioma nuevo (crea filas vacías en Traducciones para todos los tags).
   - **Renombrar** un idioma seleccionado.
   - **Eliminar** un idioma (con confirmación; no permite eliminar si es el único).
   - **Editar traducciones** del idioma seleccionado en una grilla lateral y guardarlas.

---

## Pasos de integración

### 1. Agregar modelos al proyecto Mapper
Copiar en `Mapper/Modelos/`:
- `Idioma.cs`
- `Palabra.cs`
- `Traduccion.cs`

### 2. Agregar DAL al proyecto DAL
Copiar `DAL/Datos/IdiomaDAL.cs` y agregar al `.csproj` (o incluir en VS como "Existing Item").

### 3. Reemplazar GestorIdioma en BLL
Reemplazar `BLL/Logica/GestorIdioma.cs` con el nuevo.

### 4. Modificar Program.cs — inicializar tablas de idiomas
```csharp
// En Program.cs, antes de Application.Run(new Form1()):
var usuarioDAL = new DAL.UsuarioDAL();
usuarioDAL.InicializarBaseDatos();

var idiomaDAL = new DAL.IdiomaDAL();   // ← AGREGAR ESTA LÍNEA
idiomaDAL.InicializarTablas();          // ← Y ESTA

Application.Run(new Form1());
```

### 5. Reemplazar Form1.cs
Reemplazar `UI/Form1.cs` con el nuevo (mantiene los `button1`–`button4` y los `textBox1–2`
del Designer; solo se reemplaza el code-behind).

### 6. Agregar FormIdiomas al proyecto UI
Copiar `UI/Forms/FormIdiomas.cs`.

> **Nota sobre `Microsoft.VisualBasic`**: `FormIdiomas` usa `InputBox` para el renombrado.
> Si preferís no referenciar ese ensamblado, reemplazá esa llamada por un pequeño Form
> personalizado o un `TextBox` inline en la grilla.

### 7. Ejecutar el SQL (opcional)
El script `idiomas_schema.sql` crea las tablas y datos semilla manualmente.
Si usás `InicializarTablas()` desde código, el script es solo para referencia o para
cargar los datos en un servidor ya existente.

---

## Agregar traducciones a otros Forms

En `FormABM` y `FormComposite` ya implementás `IObservadorIdioma`. Solo cambiá las llamadas:

```csharp
// Antes:
gestor.T("Agregar", "Add")

// Después:
gestor.T("abm_agregar")
```

Para agregar un tag nuevo al sistema:
1. Insertarlo en `Palabras`.
2. Insertar su traducción en `Traducciones` para cada idioma.
3. Usar `gestor.T("mi_tag_nuevo")` en el código.
