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
    `avatar` LONGTEXT NULL,
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
    `tipo` INT NOT NULL, -- Mapeado como enum TipoInmueble en C# (0=Casa, 1=Departamento, etc.)
    `direccion` VARCHAR(200) NOT NULL,
    `cupo` INT NOT NULL,
    `coord` VARCHAR(100) NULL,
    `precio` DECIMAL(12, 2) NOT NULL,
    `foto_portada` LONGTEXT NULL,
    `fotos` LONGTEXT NULL,
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

INSERT INTO `Usuario` (`id`, `nombre_usuario`, `nombre`, `apellido`, `email`, `password`, `avatar`, `rol`, `activo`) VALUES
(1, 'admin', 'Administrador', 'Sistema', 'admin@gmail.com', '123456', NULL, 'admin', 1);

INSERT INTO `Propietario` (`id`, `nombre`, `apellido`, `dni`, `email`, `telefono`, `activo`) VALUES
(1, 'Alberto', 'Fernández', '25111222', 'alberto.f@gmail.com', '1144556677', 1),
(2, 'Beatriz', 'López', '28333444', 'beatriz_lopez@hotmail.com', '1133221100', 1),
(3, 'Claudio', 'García', '31555666', 'cgarcia@yahoo.com', '1166778899', 1),
(4, 'Diana', 'Rossi', '29777888', 'diana.rossi@gmail.com', '1155443322', 1),
(5, 'Eduardo', 'Torres', '33999000', 'etorres@outlook.com', '1122334455', 1);

INSERT INTO `Inquilino` (`id`, `nombre`, `apellido`, `dni`, `email`, `telefono`, `activo`) VALUES
(1, 'Federico', 'Morales', '38123456', 'fede.morales@gmail.com', '1199887766', 1),
(2, 'Gabriela', 'Sosa', '40987654', 'gaby.sosa@live.com', '1188776655', 1),
(3, 'Lucía', 'Sosa', '45975677', 'lucia.sosa@gmail.com', '1198784675', 1),
(4, 'Hernán', 'Benítez', '36555444', 'hernan_b@gmail.com', '1177665544', 1),
(5, 'Inés', 'Acosta', '39111333', 'ines.acosta@gmail.com', '1166554433', 1);

INSERT INTO `Inmueble` (`id`, `id_propietario`, `tipo`, `direccion`, `cupo`, `coord`, `precio`, `foto_portada`, `fotos`, `activo`) VALUES
(1, 1, 1, 'Av. Corrientes 1234, CABA', 4, '-34.6037,-58.3816', 45000.00, NULL, NULL, 1),
(2, 2, 0, 'Calle 50 #432, La Plata', 6, '-34.9214,-57.9545', 75000.00, NULL, NULL, 1),
(3, 3, 2, 'Ruta 40 Km 12, Bariloche', 5, '-41.1335,-71.3103', 120000.00, NULL, NULL, 1);

INSERT INTO `Reserva` (`id`, `id_inmueble`, `id_inquilino`, `fecha_desde`, `fecha_hasta`, `monto_diario`, `creado_por_user_id`, `terminado_por_user_id`, `activo`) VALUES
(1, 1, 1, '2026-09-10', '2026-09-15', 45000.00, 1, NULL, 1),
(2, 2, 2, '2026-10-01', '2026-10-07', 75000.00, 1, NULL, 1);