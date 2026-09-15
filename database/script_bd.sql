CREATE DATABASE IF NOT EXISTS `inmobiliaria_db` 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE `inmobiliaria_db`;

-- LIMPIEZA DE TABLAS
DROP TABLE IF EXISTS `Pago`;
DROP TABLE IF EXISTS `Reserva`;
DROP TABLE IF EXISTS `Inmueble`;
DROP TABLE IF EXISTS `TipoInmueble`;
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

CREATE TABLE `TipoInmueble` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `nombre` VARCHAR(50) NOT NULL,
    `activo` TINYINT NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Inmueble` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `id_propietario` INT NOT NULL,
    `id_tipo_inmueble` INT NOT NULL,
    `direccion` VARCHAR(200) NOT NULL,
    `cupo` INT NOT NULL,
    `coord` VARCHAR(100) NULL,
    `precio` DECIMAL(12, 2) NOT NULL,
    `porcentaje_sena` DECIMAL(5, 2) NOT NULL DEFAULT 10.00,
    `foto_portada` LONGTEXT NULL,
    `fotos` LONGTEXT NULL,
    `activo` TINYINT NOT NULL DEFAULT 1,
    CONSTRAINT `fk_inmueble_propietario` 
        FOREIGN KEY (`id_propietario`) REFERENCES `Propietario` (`id`) 
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT `fk_inmueble_tipo` 
        FOREIGN KEY (`id_tipo_inmueble`) REFERENCES `TipoInmueble` (`id`) 
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

INSERT INTO `TipoInmueble` (`id`, `nombre`, `activo`) VALUES
(1, 'Casa', 1),
(2, 'Departamento', 1),
(3, 'Cabaña', 1),
(4, 'Chalet', 1),
(5, 'Duplex', 1),
(6, 'Monoambiente', 1),
(7, 'Loft', 1);

INSERT INTO `Propietario` (`id`, `nombre`, `apellido`, `dni`, `email`, `telefono`, `activo`) VALUES
(1, 'Alberto', 'Fernández', '25111222', 'alberto.f@gmail.com', '1144556677', 1),
(2, 'Beatriz', 'López', '28333444', 'beatriz_lopez@hotmail.com', '1133221100', 1),
(3, 'Claudio', 'García', '31555666', 'cgarcia@yahoo.com', '1166778899', 1),
(4, 'Alberto', 'García', '29777888', 'alberto.garcia@gmail.com', '1155443322', 1),
(5, 'Eduardo', 'Torres', '33999000', 'etorres@outlook.com', '1122334455', 1),
(6, 'Maria', 'López', '30111222', 'maria.lopez@gmail.com', '1144332211', 1),
(7, 'Carlos', 'Fernández', '27444555', 'c.fernandez@hotmail.com', '1188990011', 1),
(8, 'Beatriz', 'Gómez', '32666777', 'bgomez@yahoo.com', '1177889900', 1),
(9, 'Gonzalo', 'Pérez', '35888999', 'gperez@gmail.com', '1133445566', 1),
(10, 'Sofia', 'Torres', '34222333', 'storres@outlook.com', '1166554477', 1);

INSERT INTO `Inquilino` (`id`, `nombre`, `apellido`, `dni`, `email`, `telefono`, `activo`) VALUES
(1, 'Federico', 'Morales', '38123456', 'fede.morales@gmail.com', '1199887766', 1),
(2, 'Gabriela', 'Sosa', '40987654', 'gaby.sosa@live.com', '1188776655', 1),
(3, 'Lucía', 'Sosa', '45975677', 'lucia.sosa@gmail.com', '1198784675', 1),
(4, 'Hernán', 'Benítez', '36555444', 'hernan_b@gmail.com', '1177665544', 1),
(5, 'Inés', 'Acosta', '39111333', 'ines.acosta@gmail.com', '1166554433', 1),
(6, 'Federico', 'Benítez', '37888999', 'fede.benitez@hotmail.com', '1144668800', 1),
(7, 'Mariana', 'Morales', '41222333', 'mmorales@live.com', '1133557799', 1),
(8, 'Gabriela', 'Acosta', '42444555', 'gaby.acosta@gmail.com', '1122446688', 1), 
(9, 'Lucas', 'Martínez', '43666777', 'lucas.m@gmail.com', '1155779911', 1),
(10, 'Lucía', 'Martínez', '44888999', 'lucia.m@outlook.com', '1166880022', 1); 

INSERT INTO `Inmueble` (`id`, `id_propietario`, `id_tipo_inmueble`, `direccion`, `cupo`, `coord`, `precio`, `porcentaje_sena`, `foto_portada`, `fotos`, `activo`) VALUES
(1, 1, 2, 'Av. Corrientes 1234, CABA', 4, '-34.6037,-58.3816', 45000.00, 15.00, NULL, NULL, 1),
(2, 2, 1, 'Calle 50 #432, La Plata', 6, '-34.9214,-57.9545', 75000.00, 20.00, NULL, NULL, 1),
(3, 3, 3, 'Los Alerces 850, Bariloche', 5, '-41.1335,-71.3103', 120000.00, 25.00, NULL, NULL, 1),
(4, 4, 2, 'Av. Libertador 4500, CABA', 2, '-34.5667,-58.4312', 60000.00, 15.00, NULL, NULL, 1),
(5, 5, 4, 'Los Cerros 120, Tandil', 8, '-37.3287,-59.1369', 110000.00, 30.00, NULL, NULL, 1),
(6, 6, 6, 'Av. San Martín 780, Mendoza', 2, '-32.8895,-68.8458', 35000.00, 10.00, NULL, NULL, 1),
(7, 7, 5, 'Los Paraísos 45, Villa Carlos Paz', 6, '-31.4201,-64.4992', 95000.00, 20.00, NULL, NULL, 1),
(8, 8, 7, 'Av. Colón 1520, Córdoba', 3, '-31.4135,-64.1811', 50000.00, 15.00, NULL, NULL, 1),
(9, 9, 1, 'Los Sauces 310, Salta', 5, '-24.7859,-65.4117', 80000.00, 20.00, NULL, NULL, 1),
(10, 10, 2, 'Av. Santa Fe 2340, CABA', 4, '-34.5951,-58.4026', 70000.00, 15.00, NULL, NULL, 1),
(11, 1, 3, 'Los Pinos 99, San Martín de los Andes', 4, '-40.1579,-71.3533', 130000.00, 25.00, NULL, NULL, 1),
(12, 3, 6, 'Av. Belgrano 600, Rosario', 2, '-32.9468,-60.6393', 38000.00, 10.00, NULL, NULL, 1),
(13, 5, 5, 'Los Jacarandás 512, Merlo', 5, '-32.3444,-65.0139', 85000.00, 20.00, NULL, NULL, 1),
(14, 7, 4, 'Av. Pellegrini 1890, Rosario', 7, '-32.9572,-60.6554', 105000.00, 25.00, NULL, NULL, 1),
(15, 9, 1, 'Los Lapachos 74, Jujuy', 6, '-24.1858,-65.2995', 78000.00, 15.00, NULL, NULL, 1);

INSERT INTO `Reserva` (`id`, `id_inmueble`, `id_inquilino`, `fecha_desde`, `fecha_hasta`, `monto_diario`, `creado_por_user_id`, `terminado_por_user_id`, `activo`) VALUES
(1, 1, 1, '2026-09-10', '2026-09-15', 45000.00, 1, NULL, 1),
(2, 2, 2, '2026-10-01', '2026-10-07', 75000.00, 1, NULL, 1),
(3, 3, 3, '2026-11-15', '2026-11-22', 120000.00, 1, NULL, 1),
(4, 5, 4, '2026-12-20', '2026-12-27', 110000.00, 1, NULL, 1),
(5, 7, 5, '2027-01-05', '2027-01-12', 95000.00, 1, NULL, 1),
(6, 9, 6, '2027-01-15', '2027-01-20', 80000.00, 1, NULL, 1),
(7, 11, 7, '2027-02-01', '2027-02-10', 130000.00, 1, NULL, 1);