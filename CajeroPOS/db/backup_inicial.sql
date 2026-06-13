
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





USE cajeropos;

-- =========================================
-- DATOS DE VENTAS
-- =========================================

INSERT INTO ventas (Id, Numero, Fecha, UsuarioId, Total, Importe) VALUES
(1, 'A001', '2026-05-14 11:33:00', 1, 9.99, 10.00),
(2, 'A002', '2026-05-14 11:33:49', 1, 16.98, 20.00),
(3, 'A003', '2026-05-14 11:35:26', 1, 6.99, 8.00),
(4, 'A004', '2026-05-14 11:48:10', 1, 9.99, 1234.00),
(5, 'A005', '2026-05-14 11:51:21', 1, 8.99, 12.00),
(6, 'A006', '2026-05-14 11:53:18', 1, 9.99, 12.00),
(7, 'A007', '2026-05-14 11:55:30', 1, 6.99, 8.00),
(8, 'A008', '2026-05-14 12:08:08', 1, 11.99, 13.00),
(9, 'A009', '2026-05-14 12:14:55', 1, 12.99, 15.00),
(10, 'A010', '2026-05-14 12:23:18', 1, 9.99, 123.00),
(11, 'A011', '2026-05-14 12:24:19', 1, 14.99, 15.00),
(12, 'A012', '2026-05-14 12:37:09', 1, 14.99, 15.00),
(13, 'A013', '2026-05-14 12:42:50', 1, 14.99, 15.00),
(14, 'A014', '2026-05-14 12:44:01', 1, 9.99, 12.00),
(15, 'A015', '2026-05-15 09:42:25', 1, 11.99, 15.00),
(16, 'A016', '2026-05-15 09:48:17', 1, 11.99, 15.00),
(17, 'A017', '2026-05-15 09:50:02', 1, 9.99, 12.00),
(18, 'A018', '2026-05-15 09:51:41', 1, 19.97, 20.00),
(19, 'A019', '2026-05-15 13:25:30', 1, 17.97, 20.00),
(20, 'A020', '2026-05-15 13:26:31', 1, 6.00, 9.00),
(21, 'A021', '2026-05-15 13:31:48', 1, 4.00, 5.00),
(22, 'A022', '2026-05-15 14:08:19', 1, 10.99, 12.00),
(23, 'A023', '2026-05-15 14:11:39', 1, 12.99, 15.00),
(24, 'A024', '2026-05-15 14:15:38', 1, 8.00, 9.00),
(25, 'A025', '2026-05-15 14:17:58', 1, 9.99, 10.00);

-- =========================================
-- DATOS DE VENTAS DETALLES
-- =========================================

INSERT INTO ventadetalles (Id, VentaId, ProductoId, Cantidad, PrecioUnitario) VALUES
(1, 15, 1, 1, 4.99),
(2, 15, 2, 2, 2.00),
(3, 15, 3, 1, 3.00),
(4, 16, 1, 1, 4.99),
(5, 16, 2, 2, 2.00),
(6, 16, 3, 1, 3.00),
(7, 17, 1, 1, 4.99),
(8, 17, 2, 1, 2.00),
(9, 17, 3, 1, 3.00),
(10, 18, 1, 3, 4.99),
(11, 18, 2, 1, 2.00),
(12, 18, 3, 1, 3.00),
(13, 19, 1, 3, 4.99),
(14, 19, 2, 1, 2.00),
(15, 19, 4, 1, 1.00),
(16, 20, 3, 2, 3.00),
(17, 21, 4, 1, 1.00),
(18, 21, 3, 1, 3.00),
(19, 22, 1, 1, 4.99),
(20, 22, 3, 1, 3.00),
(21, 22, 2, 1, 2.00),
(22, 22, 4, 1, 1.00),
(23, 23, 1, 1, 4.99),
(24, 23, 2, 1, 2.00),
(25, 23, 3, 2, 3.00),
(26, 24, 4, 3, 1.00),
(27, 24, 3, 1, 3.00),
(28, 24, 2, 1, 2.00),
(29, 25, 3, 1, 3.00),
(30, 25, 2, 1, 2.00),
(31, 25, 1, 1, 4.99);



