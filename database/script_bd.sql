CREATE DATABASE IF NOT EXISTS `inmobiliaria_db` 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE `inmobiliaria_db`;

-- LIMPIEZA DE TABLAS

DROP TABLE IF EXISTS `Pago`;
DROP TABLE IF EXISTS `Reserva`;
DROP TABLE IF EXISTS `Inmueble`;
DROP TABLE IF EXISTS `Inquilino`;
DROP TABLE IF EXISTS `Propietario`;
DROP TABLE IF EXISTS `Usuario`;

-- CREACIÓN DE TABLAS

CREATE TABLE `Usuario` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `nombre_usuario` VARCHAR(50) NOT NULL UNIQUE,
    `nombre` VARCHAR(100) NOT NULL,
    `apellido` VARCHAR(100) NOT NULL,
    `email` VARCHAR(150) NOT NULL UNIQUE,
    `password` VARCHAR(255) NOT NULL,
    `avatar` VARCHAR(255) NULL,
    `rol` VARCHAR(20) NOT NULL,
    `activo` TINYINT NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


CREATE TABLE `Propietario` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `nombre` VARCHAR(100) NOT NULL,
    `apellido` VARCHAR(100) NOT NULL,
    `dni` VARCHAR(20) NOT NULL UNIQUE,
    `email` VARCHAR(150) NOT NULL UNIQUE,
    `telefono` VARCHAR(30) NOT NULL,
    `activo` TINYINT NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


CREATE TABLE `Inquilino` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `nombre` VARCHAR(100) NOT NULL,
    `apellido` VARCHAR(100) NOT NULL,
    `dni` VARCHAR(20) NOT NULL UNIQUE,
    `email` VARCHAR(150) NOT NULL UNIQUE,
    `telefono` VARCHAR(30) NOT NULL,
    `activo` TINYINT NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


CREATE TABLE `Inmueble` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `id_propietario` INT NOT NULL,
    `tipo` VARCHAR(50) NOT NULL,
    `direccion` VARCHAR(200) NOT NULL,
    `cupo` INT NOT NULL,
    `coord` VARCHAR(100) NULL,
    `precio` DECIMAL(12, 2) NOT NULL,
    `foto_portada` VARCHAR(255) NULL,
    `fotos` VARCHAR(500) NULL,
    `activo` TINYINT NOT NULL DEFAULT 1,
    CONSTRAINT `fk_inmueble_propietario` 
        FOREIGN KEY (`id_propietario`) REFERENCES `Propietario` (`id`) 
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


CREATE TABLE `Reserva` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `id_inmueble` INT NOT NULL,
    `id_inquilino` INT NOT NULL,
    `fecha_desde` DATE NOT NULL,
    `fecha_hasta` DATE NOT NULL,
    `monto_diario` DECIMAL(12, 2) NOT NULL,
    `creado_por_user_id` INT NOT NULL,
    `terminado_por_user_id` INT NULL,
    `activo` TINYINT NOT NULL DEFAULT 1,
    CONSTRAINT `fk_reserva_inmueble` 
        FOREIGN KEY (`id_inmueble`) REFERENCES `Inmueble` (`id`) 
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT `fk_reserva_inquilino` 
        FOREIGN KEY (`id_inquilino`) REFERENCES `Inquilino` (`id`) 
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT `fk_reserva_user_creado` 
        FOREIGN KEY (`creado_por_user_id`) REFERENCES `Usuario` (`id`) 
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT `fk_reserva_user_terminado` 
        FOREIGN KEY (`terminado_por_user_id`) REFERENCES `Usuario` (`id`) 
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


CREATE TABLE `Pago` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `id_reserva` INT NOT NULL,
    `concepto` VARCHAR(150) NOT NULL,
    `fecha_pago` DATE NOT NULL,
    `importe` DECIMAL(12, 2) NOT NULL,
    `creado_por_user_id` INT NOT NULL,
    `anulado_por_user_id` INT NULL,
    `activo` TINYINT NOT NULL DEFAULT 1,
    CONSTRAINT `fk_pago_reserva` 
        FOREIGN KEY (`id_reserva`) REFERENCES `Reserva` (`id`) 
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT `fk_pago_user_creado` 
        FOREIGN KEY (`creado_por_user_id`) REFERENCES `Usuario` (`id`) 
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT `fk_pago_user_anulado` 
        FOREIGN KEY (`anulado_por_user_id`) REFERENCES `Usuario` (`id`) 
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- SEEDERS 

INSERT INTO `Propietario` (`nombre`, `apellido`, `dni`, `email`, `telefono`, `activo`) VALUES
('Alberto', 'Fernández', '25111222', 'alberto.f@gmail.com', '1144556677', 1),
('Beatriz', 'López', '28333444', 'beatriz_lopez@hotmail.com', '1133221100', 1),
('Claudio', 'García', '31555666', 'cgarcia@yahoo.com', '1166778899', 1),
('Diana', 'Rossi', '29777888', 'diana.rossi@gmail.com', '1155443322', 1),
('Eduardo', 'Torres', '33999000', 'etorres@outlook.com', '1122334455', 1);

INSERT INTO `Inquilino` (`nombre`, `apellido`, `dni`, `email`, `telefono`, `activo`) VALUES
('Federico', 'Morales', '38123456', 'fede.morales@gmail.com', '1199887766', 1),
('Gabriela', 'Sosa', '40987654', 'gaby.sosa@live.com', '1188776655', 1),
('Hernán', 'Benítez', '36555444', 'hernan_b@gmail.com', '1177665544', 1),
('Inés', 'Acosta', '39111333', 'ines.acosta@gmail.com', '1166554433', 1),
('Javier', 'Ríos', '37222444', 'jrios@hotmail.com', '1155667788', 1);