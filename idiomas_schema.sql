-- ================================================================
--  SCRIPT SQL — Sistema de Idiomas
--  Base de datos: IngenieriaSoftware
--  Tablas: Idiomas, Palabras, Traducciones
-- ================================================================

USE [IngenieriaSoftware]
GO

-- ── 1. Tabla Idiomas ────────────────────────────────────────────
--  PK: Id (IDENTITY)   •   Unique: Nombre
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Idiomas' AND xtype='U')
CREATE TABLE [dbo].[Idiomas] (
    [Id]     INT           IDENTITY(1,1) PRIMARY KEY,
    [Nombre] NVARCHAR(100) NOT NULL UNIQUE
);
GO

-- ── 2. Tabla Palabras ───────────────────────────────────────────
--  PK: Tag (el código que se usa en el programa para pedir una traducción)
--  Ejemplos de Tag: "btn_login", "lbl_usuario", "msg_bienvenido"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Palabras' AND xtype='U')
CREATE TABLE [dbo].[Palabras] (
    [Tag] NVARCHAR(100) PRIMARY KEY
);
GO

-- ── 3. Tabla Traducciones ───────────────────────────────────────
--  PK compuesta: (IdIdioma, Tag)
--  FK IdIdioma → Idiomas.Id   (CASCADE DELETE: si se elimina un idioma se borran sus traducciones)
--  FK Tag      → Palabras.Tag (CASCADE DELETE: si se elimina una palabra se borran sus traducciones)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Traducciones' AND xtype='U')
CREATE TABLE [dbo].[Traducciones] (
    [IdIdioma]   INT           NOT NULL,
    [Tag]        NVARCHAR(100) NOT NULL,
    [Traduccion] NVARCHAR(500) NOT NULL DEFAULT '',
    PRIMARY KEY ([IdIdioma], [Tag]),
    CONSTRAINT FK_Trad_Idioma  FOREIGN KEY ([IdIdioma]) REFERENCES [Idiomas]([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_Trad_Palabra FOREIGN KEY ([Tag])      REFERENCES [Palabras]([Tag]) ON DELETE CASCADE
);
GO

-- ════════════════════════════════════════════════════════════════
--  DATOS SEMILLA
-- ════════════════════════════════════════════════════════════════

-- Idiomas base
IF NOT EXISTS (SELECT 1 FROM Idiomas WHERE Nombre = 'Español')
    INSERT INTO Idiomas (Nombre) VALUES ('Español');   -- Id = 1
IF NOT EXISTS (SELECT 1 FROM Idiomas WHERE Nombre = 'Inglés')
    INSERT INTO Idiomas (Nombre) VALUES ('Inglés');    -- Id = 2
GO

-- Tags (palabras clave)
INSERT INTO Palabras (Tag)
SELECT v.Tag FROM (VALUES
    ('btn_login'),
    ('btn_usuarios'),
    ('btn_logout'),
    ('btn_composite'),
    ('lbl_usuario'),
    ('lbl_clave'),
    ('msg_bienvenido'),
    ('msg_acceso_ok'),
    ('msg_cred_error'),
    ('titulo_principal'),
    ('abm_titulo'),
    ('abm_agregar'),
    ('abm_modificar'),
    ('abm_eliminar'),
    ('abm_limpiar'),
    ('abm_permisos'),
    ('composite_titulo')
) AS v(Tag)
WHERE NOT EXISTS (SELECT 1 FROM Palabras p WHERE p.Tag = v.Tag);
GO

-- Traducciones — Español (Id=1)
DECLARE @IdEsp INT = (SELECT Id FROM Idiomas WHERE Nombre = 'Español');
INSERT INTO Traducciones (IdIdioma, Tag, Traduccion)
SELECT @IdEsp, v.Tag, v.Trad FROM (VALUES
    ('btn_login',        'Iniciar sesión'),
    ('btn_usuarios',     'Administrar Usuarios'),
    ('btn_logout',       'Cerrar sesión'),
    ('btn_composite',    'Administrar Composite'),
    ('lbl_usuario',      'Usuario:'),
    ('lbl_clave',        'Clave:'),
    ('msg_bienvenido',   'Bienvenido, {0}!'),
    ('msg_acceso_ok',    'Acceso correcto'),
    ('msg_cred_error',   'Usuario o clave incorrectos.'),
    ('titulo_principal', 'Sistema de Usuarios'),
    ('abm_titulo',       'Administrar Usuarios'),
    ('abm_agregar',      'Agregar'),
    ('abm_modificar',    'Modificar'),
    ('abm_eliminar',     'Eliminar'),
    ('abm_limpiar',      'Limpiar'),
    ('abm_permisos',     'Editar Permisos'),
    ('composite_titulo', 'Administrar Composite')
) AS v(Tag, Trad)
WHERE NOT EXISTS (
    SELECT 1 FROM Traducciones t WHERE t.IdIdioma = @IdEsp AND t.Tag = v.Tag
);
GO

-- Traducciones — Inglés (Id=2)
DECLARE @IdEng INT = (SELECT Id FROM Idiomas WHERE Nombre = 'Inglés');
INSERT INTO Traducciones (IdIdioma, Tag, Traduccion)
SELECT @IdEng, v.Tag, v.Trad FROM (VALUES
    ('btn_login',        'Log in'),
    ('btn_usuarios',     'Manage Users'),
    ('btn_logout',       'Log out'),
    ('btn_composite',    'Manage Composite'),
    ('lbl_usuario',      'Username:'),
    ('lbl_clave',        'Password:'),
    ('msg_bienvenido',   'Welcome, {0}!'),
    ('msg_acceso_ok',    'Access granted'),
    ('msg_cred_error',   'Incorrect username or password.'),
    ('titulo_principal', 'User System'),
    ('abm_titulo',       'Manage Users'),
    ('abm_agregar',      'Add'),
    ('abm_modificar',    'Modify'),
    ('abm_eliminar',     'Delete'),
    ('abm_limpiar',      'Clear'),
    ('abm_permisos',     'Edit Permissions'),
    ('composite_titulo', 'Manage Composite')
) AS v(Tag, Trad)
WHERE NOT EXISTS (
    SELECT 1 FROM Traducciones t WHERE t.IdIdioma = @IdEng AND t.Tag = v.Tag
);
GO

-- ════════════════════════════════════════════════════════════════
--  CONSULTA DE VERIFICACIÓN
--  Muestra filas en el formato:  esp = 1,inicio,home
-- ════════════════════════════════════════════════════════════════
SELECT
    LEFT(LOWER(i.Nombre), 3)                            AS Abreviatura,
    i.Id                                                AS IdIdioma,
    t.Tag,
    t.Traduccion,
    -- Formato "esp = 1,inicio,home" agrupado por tag:
    t.Tag + ' → [' + STRING_AGG(
        LEFT(LOWER(i.Nombre),3) + '=' + t.Traduccion, ' | ')
        OVER (PARTITION BY t.Tag) + ']'                 AS Resumen
FROM Traducciones t
JOIN Idiomas      i ON i.Id = t.IdIdioma
ORDER BY t.Tag, i.Id;
GO
