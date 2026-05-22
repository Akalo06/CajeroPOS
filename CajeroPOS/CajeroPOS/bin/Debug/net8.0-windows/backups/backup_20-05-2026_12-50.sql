/*M!999999\- enable the sandbox mode */ 
-- MariaDB dump 10.19-12.0.2-MariaDB, for Win64 (AMD64)
--
-- Host: localhost    Database: testeo
-- ------------------------------------------------------
-- Server version	12.0.2-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*M!100616 SET @OLD_NOTE_VERBOSITY=@@NOTE_VERBOSITY, NOTE_VERBOSITY=0 */;

--
-- Table structure for table `categorias`
--

DROP TABLE IF EXISTS `categorias`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `categorias` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(50) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Nombre` (`Nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categorias`
--

LOCK TABLES `categorias` WRITE;
/*!40000 ALTER TABLE `categorias` DISABLE KEYS */;
set autocommit=0;
INSERT INTO `categorias` VALUES
(6,'aaaa'),
(7,'bbbb'),
(5,'Todas');
/*!40000 ALTER TABLE `categorias` ENABLE KEYS */;
UNLOCK TABLES;
commit;

--
-- Table structure for table `productos`
--

DROP TABLE IF EXISTS `productos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `productos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `Precio` decimal(10,2) NOT NULL,
  `Cantidad` int(11) DEFAULT 0,
  `CategoriaId` int(11) DEFAULT NULL,
  `Imagen` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `CategoriaId` (`CategoriaId`),
  CONSTRAINT `productos_ibfk_1` FOREIGN KEY (`CategoriaId`) REFERENCES `categorias` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `productos`
--

LOCK TABLES `productos` WRITE;
/*!40000 ALTER TABLE `productos` DISABLE KEYS */;
set autocommit=0;
INSERT INTO `productos` VALUES
(3,'a',12.00,0,6,'C:\\Users\\corre\\OneDrive\\Escritorio\\cajero 3\\imagenes\\frente-de-la-tienda.png'),
(4,'b',100.00,0,7,'frente-de-la-tienda.png'),
(5,'c',3.00,0,7,'frente-de-la-tienda.png');
/*!40000 ALTER TABLE `productos` ENABLE KEYS */;
UNLOCK TABLES;
commit;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(50) NOT NULL,
  `PasswordHash` text NOT NULL,
  `Salt` text NOT NULL,
  `Rol` varchar(20) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Nombre` (`Nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
set autocommit=0;
INSERT INTO `usuarios` VALUES
(1,'admin','5IV+Dc9OTHdUMVEZcRpEIA6iEySefz+ZYXF9SAtIAiU=','M9JYlsPVZC123s+cTAiFvw==','Administrador');
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;
commit;

--
-- Table structure for table `ventadetalles`
--

DROP TABLE IF EXISTS `ventadetalles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `ventadetalles` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `VentaId` int(11) NOT NULL,
  `ProductoId` int(11) DEFAULT NULL,
  `Cantidad` int(11) NOT NULL,
  `PrecioUnitario` decimal(10,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `VentaId` (`VentaId`),
  KEY `ProductoId` (`ProductoId`),
  CONSTRAINT `ventadetalles_ibfk_1` FOREIGN KEY (`VentaId`) REFERENCES `ventas` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `ventadetalles_ibfk_2` FOREIGN KEY (`ProductoId`) REFERENCES `productos` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ventadetalles`
--

LOCK TABLES `ventadetalles` WRITE;
/*!40000 ALTER TABLE `ventadetalles` DISABLE KEYS */;
set autocommit=0;
INSERT INTO `ventadetalles` VALUES
(1,1,3,3,12.00),
(2,2,3,2,12.00),
(3,3,3,2,12.00),
(4,3,5,2,3.00),
(5,4,5,1,3.00),
(6,4,3,2,12.00),
(7,5,3,1,12.00),
(8,5,5,2,3.00),
(9,6,3,1,12.00),
(10,6,5,2,3.00),
(11,7,3,1,12.00),
(12,7,5,2,3.00),
(13,8,3,1,12.00),
(14,8,5,3,3.00);
/*!40000 ALTER TABLE `ventadetalles` ENABLE KEYS */;
UNLOCK TABLES;
commit;

--
-- Table structure for table `ventas`
--

DROP TABLE IF EXISTS `ventas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `ventas` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Numero` varchar(20) NOT NULL,
  `Fecha` datetime NOT NULL,
  `UsuarioId` int(11) DEFAULT NULL,
  `Total` decimal(10,2) NOT NULL,
  `Importe` decimal(10,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `UsuarioId` (`UsuarioId`),
  CONSTRAINT `ventas_ibfk_1` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ventas`
--

LOCK TABLES `ventas` WRITE;
/*!40000 ALTER TABLE `ventas` DISABLE KEYS */;
set autocommit=0;
INSERT INTO `ventas` VALUES
(1,'A001','2026-05-19 19:38:52',1,36.00,40.00),
(2,'A002','2026-05-19 19:39:09',1,24.00,32.00),
(3,'A003','2026-05-19 19:41:13',1,30.00,40.00),
(4,'A004','2026-05-20 08:09:37',1,27.00,30.00),
(5,'A005','2026-05-20 11:58:10',1,18.00,20.00),
(6,'A006','2026-05-20 12:00:00',1,18.00,34.00),
(7,'A007','2026-05-20 12:00:15',1,18.00,23.00),
(8,'A008','2026-05-20 12:01:31',1,21.00,24.00);
/*!40000 ALTER TABLE `ventas` ENABLE KEYS */;
UNLOCK TABLES;
commit;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*M!100616 SET NOTE_VERBOSITY=@OLD_NOTE_VERBOSITY */;

-- Dump completed on 2026-05-20 12:50:55
