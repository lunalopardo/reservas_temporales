# PROYECTO ALQUILERES TEMPORALES

> Sistema para la gestión integral de alquileres temporarios de propiedades inmuebles desarrollado para una agencia inmobiliaria.

---

## 👥 Integrantes del Grupo

* **Luna Lopardo** - *luna.lopardo@gmail.com* - [@lunalopardo](https://github.com/lunalopardo) - Discord: `slotherin`
* **Myriam Alvarez** - *myriamalvarez1006@gmail.com* - [@myriamalvarez](https://github.com/myriamalvarez) - Discord: `myriamalvarez1006`
* **Leandra Campos** - *camposleandra149@gmail.com* - [@Leandra25](https://github.com/Leandra25) - Discord: `Leandra 7827`

---

## 📦 Entrega 1: Alcance del Proyecto

Para esta primera entrega, el sistema cuenta con la implementación de manejo de Propietarios e Inquilinos.

### 📌 Lo entregado:
* **Diseño e implementación de la Base de Datos**: Estructura relacional creada e integrada mediante MySQL.
* **Módulos ABM completos**: Funcionalidades de Alta, Baja, Modificación y Lectura para Propietarios e Inquilinos.
* **Interfaz de Usuario (Vistas MVC)**: Vistas dinámicas para listados, formularios de creación/edición y pantallas de confirmación de eliminación.

---

## 📐 Modelado de Datos

A continuación se presenta el esquema del modelo de datos correspondiente a la aplicación:

### Diagrama Entidad-Relación (DER) / Diagrama de Clases

![Diagrama del Proyecto](/docs/DER.png)

---

## 🚀 Guía de Instalación y Ejecución

Seguí estos pasos para clonar, configurar y ejecutar el proyecto localmente:

### 1. Clonar el repositorio

**Opción A: Usando GitHub Desktop (Recomendada)**
1. Abrí **GitHub Desktop**.
2. Tocá en **File** > **Clone repository...** (o `Ctrl + Shift + O`).
3. Andá a la pestaña **URL** e ingresá: `https://github.com/lunalopardo/reservas_temporales.git`
4. Seleccioná la carpeta de tu PC donde quieras guardarlo y hacé clic en **Clone**.
5. Al finalizar, hacé clic en el botón **Open in Visual Studio Code** (o **Open in Explorer** para abrir la carpeta).
6. Escribí: `cd reservas_temporales\src` en la terminal para ir a la carpeta correcta.

**Opción B: A través de la consola del VSC:**
1. Abrí una terminal y ejecutá
```
git clone https://github.com/lunalopardo/reservas_temporales.git
```
2. Ejecutá lo siguiente `cd reservas_temporales` si no estás dentro del proyecto y luego `code .` para abrir la carpeta en el VSC.
3. Luego, para poder correr la aplicación, entrá a 'src' ejecutando `cd src` en la consola.

---

### 2. Configurar la Base de Datos (XAMPP / phpMyAdmin)

1. Abrí XAMPP Control Panel e iniciá los servicios de Apache y MySQL.
2. Ingresá a phpMyAdmin desde tu navegador (http://localhost/phpmyadmin).
3. Importá el script SQL ubicado en la carpeta del proyecto (ej: /database/script_bd.sql). Va a crear una nueva base de datos y a cargar algunos datos de prueba.

> IMPORTANTE: No tener otra base de datos con el nombre "inmobiliaria_db", ya que va a eliminarla primero y a volver a crearla.

---

### 3. Configurar la cadena de conexión

Abrí el archivo appsettings.json en la raíz del proyecto y verificá/actualizá tu cadena de conexión a MySQL según la configuración de tu XAMPP:

```
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=inmobiliaria_db;User=root;Password=;"
}
```

> Nota: Por defecto en XAMPP, el usuario es root y la contraseña suele estar en blanco. Lo tenemos configurado así.

---

### 4. Restaurar dependencias y compilar

En la terminal, dentro de la carpeta raíz del proyecto (reservas_temporales\src), ejecutá:
```
dotnet restore
dotnet build
```
---

### 5. Ejecutar la aplicación

Para iniciar el servidor de desarrollo:

`dotnet run`
o `dotnet run --urls "http://localhost:5000"` si querés elegir el puerto, reemplazando '5000' por el puerto elegido.

La terminal te indicará la URL local (http://localhost:5226 por defecto). Abrí esa dirección en tu navegador para probar el sistema.