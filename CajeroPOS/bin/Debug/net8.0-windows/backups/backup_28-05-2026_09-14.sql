/*M!999999\- enable the sandbox mode */ 
-- MariaDB dump 10.19-12.0.2-MariaDB, for Win64 (AMD64)
--
-- Host: localhost    Database: cajeropos
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
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categorias`
--

LOCK TABLES `categorias` WRITE;
/*!40000 ALTER TABLE `categorias` DISABLE KEYS */;
set autocommit=0;
INSERT INTO `categorias` VALUES
(3,'Bebida'),
(2,'Burguers'),
(4,'Complementos'),
(5,'Postre'),
(1,'Todas');
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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `productos`
--

LOCK TABLES `productos` WRITE;
/*!40000 ALTER TABLE `productos` DISABLE KEYS */;
set autocommit=0;
INSERT INTO `productos` VALUES
(1,'Hamburguesa',4.99,0,2,'burguer.jpg'),
(2,'Patatas',2.00,0,4,'patatas.jpg'),
(3,'Cocacola',3.00,0,3,'bebida.png'),
(4,'Helado',2.00,0,5,'helado.jpg'),
(5,'a',12.00,0,5,'hoo-lee-sheet.png');
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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
set autocommit=0;
INSERT INTO `usuarios` VALUES
(1,'admin','5IV+Dc9OTHdUMVEZcRpEIA6iEySefz+ZYXF9SAtIAiU=','M9JYlsPVZC123s+cTAiFvw==','Administrador'),
(2,'cajero','2tlV/aHL4CH8fuB6GtQBTtqsbbVxwBOhuiaMrcvZLgs=','QEeuH4WJsb5f1Jwav7OfGA==','Cajero'),
(3,'alfred','bVlgstnJ9o8yGuuAdXxPUi3MDfWCt4NAIhTXHd0tOCE=','jXtEs5ISy0qvnj954kB85g==','Cajero');
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
) ENGINE=InnoDB AUTO_INCREMENT=47 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ventadetalles`
--

LOCK TABLES `ventadetalles` WRITE;
/*!40000 ALTER TABLE `ventadetalles` DISABLE KEYS */;
set autocommit=0;
INSERT INTO `ventadetalles` VALUES
(1,15,1,1,4.99),
(2,15,2,2,2.00),
(3,15,3,1,3.00),
(4,16,1,1,4.99),
(5,16,2,2,2.00),
(6,16,3,1,3.00),
(7,17,1,1,4.99),
(8,17,2,1,2.00),
(9,17,3,1,3.00),
(10,18,1,3,4.99),
(11,18,2,1,2.00),
(12,18,3,1,3.00),
(13,19,1,3,4.99),
(14,19,2,1,2.00),
(15,19,4,1,1.00),
(16,20,3,2,3.00),
(17,21,4,1,1.00),
(18,21,3,1,3.00),
(19,22,1,1,4.99),
(20,22,3,1,3.00),
(21,22,2,1,2.00),
(22,22,4,1,1.00),
(23,23,1,1,4.99),
(24,23,2,1,2.00),
(25,23,3,2,3.00),
(26,24,4,3,1.00),
(27,24,3,1,3.00),
(28,24,2,1,2.00),
(29,25,3,1,3.00),
(30,25,2,1,2.00),
(31,25,1,1,4.99),
(32,26,1,3,4.99),
(33,26,2,1,2.00),
(34,26,3,2,3.00),
(35,26,4,1,2.00),
(36,27,4,1,2.00),
(37,27,3,1,3.00),
(38,27,2,1,2.00),
(39,28,1,2,4.99),
(40,28,4,1,2.00),
(41,29,4,2,2.00),
(42,29,3,2,3.00),
(43,29,1,2,4.99),
(44,30,1,1,4.99),
(45,30,2,3,2.00),
(46,30,3,1,3.00);
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
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ventas`
--

LOCK TABLES `ventas` WRITE;
/*!40000 ALTER TABLE `ventas` DISABLE KEYS */;
set autocommit=0;
INSERT INTO `ventas` VALUES
(1,'A001','2026-05-14 11:33:00',1,9.99,10.00),
(2,'A002','2026-05-14 11:33:49',1,16.98,20.00),
(3,'A003','2026-05-14 11:35:26',1,6.99,8.00),
(4,'A004','2026-05-14 11:48:10',1,9.99,1234.00),
(5,'A005','2026-05-14 11:51:21',1,8.99,12.00),
(6,'A006','2026-05-14 11:53:18',1,9.99,12.00),
(7,'A007','2026-05-14 11:55:30',1,6.99,8.00),
(8,'A008','2026-05-14 12:08:08',1,11.99,13.00),
(9,'A009','2026-05-14 12:14:55',1,12.99,15.00),
(10,'A010','2026-05-14 12:23:18',1,9.99,123.00),
(11,'A011','2026-05-14 12:24:19',1,14.99,15.00),
(12,'A012','2026-05-14 12:37:09',1,14.99,15.00),
(13,'A013','2026-05-14 12:42:50',1,14.99,15.00),
(14,'A014','2026-05-14 12:44:01',1,9.99,12.00),
(15,'A015','2026-05-15 09:42:25',1,11.99,15.00),
(16,'A016','2026-05-15 09:48:17',1,11.99,15.00),
(17,'A017','2026-05-15 09:50:02',1,9.99,12.00),
(18,'A018','2026-05-15 09:51:41',1,19.97,20.00),
(19,'A019','2026-05-15 13:25:30',1,17.97,20.00),
(20,'A020','2026-05-15 13:26:31',1,6.00,9.00),
(21,'A021','2026-05-15 13:31:48',1,4.00,5.00),
(22,'A022','2026-05-15 14:08:19',1,10.99,12.00),
(23,'A023','2026-05-15 14:11:39',1,12.99,15.00),
(24,'A024','2026-05-15 14:15:38',1,8.00,9.00),
(25,'A025','2026-05-15 14:17:58',1,9.99,10.00),
(26,'A026','2026-05-22 14:30:47',1,24.97,50.00),
(27,'A027','2026-05-27 19:46:28',1,7.00,10.00),
(28,'A028','2026-05-28 09:04:12',1,11.98,20.00),
(29,'A029','2026-05-28 09:05:27',1,19.98,20.00),
(30,'A030','2026-05-28 09:07:23',1,13.99,20.00);
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

-- Dump completed on 2026-05-28  9:14:08
