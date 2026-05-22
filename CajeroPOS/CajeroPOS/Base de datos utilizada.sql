
CREATE DATABASE IF NOT EXISTS cajeropos;
USE cajeropos;

-- =========================================
-- TABLA CATEGORIAS
-- =========================================

CREATE TABLE categorias (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL
);

-- =========================================
-- TABLA USUARIOS
-- =========================================

CREATE TABLE usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    PasswordHash TEXT NOT NULL,
    Salt TEXT NOT NULL,
    Rol VARCHAR(50) NOT NULL
);

-- =========================================
-- TABLA PRODUCTOS
-- =========================================

CREATE TABLE productos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Precio DECIMAL(10,2) NOT NULL,
    Cantidad INT NOT NULL DEFAULT 0,
    CategoriaId INT,
    Imagen VARCHAR(255),

    CONSTRAINT FK_Productos_Categorias
    FOREIGN KEY (CategoriaId)
    REFERENCES categorias(Id)
    ON DELETE SET NULL
);

-- =========================================
-- TABLA VENTAS
-- =========================================

CREATE TABLE ventas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Numero VARCHAR(20) NOT NULL,
    Fecha DATETIME NOT NULL,
    UsuarioId INT NOT NULL,
    Total DECIMAL(10,2) NOT NULL,
    Importe DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_Ventas_Usuarios
    FOREIGN KEY (UsuarioId)
    REFERENCES usuarios(Id)
    ON DELETE CASCADE
);

-- =========================================
-- TABLA VENTADETALLES
-- =========================================

CREATE TABLE ventadetalles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    VentaId INT NOT NULL,
    ProductoId INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_VentaDetalles_Ventas
    FOREIGN KEY (VentaId)
    REFERENCES ventas(Id)
    ON DELETE CASCADE,

    CONSTRAINT FK_VentaDetalles_Productos
    FOREIGN KEY (ProductoId)
    REFERENCES productos(Id)
    ON DELETE CASCADE
);

-- =========================================
-- DATOS INICIALES
-- =========================================

INSERT INTO categorias (Id, Nombre) VALUES
(1, 'Todas'),
(2, 'Burguers'),
(3, 'Bebida'),
(4, 'Complementos'),
(5, 'Postre');

INSERT INTO productos (Id, Nombre, Precio, Cantidad, CategoriaId, Imagen) VALUES
(1, 'Hamburguesa', 4.99, 0, 2, 'burguer.jpg'),
(2, 'Patatas', 2.00, 0, 4, 'patatas.jpg'),
(3, 'Cocacola', 3.00, 0, 3, 'bebida.png'),
(4, 'Helado', 2.00, 0, 5, 'helado.jpg');

INSERT INTO usuarios (Id, Nombre, PasswordHash, Salt, Rol) VALUES
(1, 'admin', '5IV+Dc9OTHdUMVEZcRpEIA6iEySefz+ZYXF9SAtIAiU=', 'M9JYlsPVZC123s+cTAiFvw==', 'Administrador'),
(2, 'empleado1', '2tlV/aHL4CH8fuB6GtQBTtqsbbVxwBOhuiaMrcvZLgs=', 'QEeuH4WJsb5f1Jwav7OfGA==', 'Cajero'),
(3, 'empleado2', 'bVlgstnJ9o8yGuuAdXxPUi3MDfWCt4NAIhTXHd0tOCE=', 'jXtEs5ISy0qvnj954kB85g==', 'Cajero');

