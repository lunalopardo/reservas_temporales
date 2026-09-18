# PROYECTO ALQUILERES TEMPORALES

> Sistema para la gestión integral de alquileres temporarios de propiedades inmuebles desarrollado para una agencia inmobiliaria.


---

## 👥 Integrantes del Grupo

* **Luna Lopardo** - *luna.lopardo@gmail.com* - [@lunalopardo](https://github.com/lunalopardo) - Discord: `slotherin`


---

## 📦 Alcance del proyecto y funcionalidades al día de hoy

El sistema es una solución integral para la gestión de alquileres temporales..

### Módulos e Implementaciones:

**Autenticación y autorización**
- El sistema cuenta con autenticación y todas las funcionalidades requieren tener una sesión activa.
- El administrador (user: admin - pw: admin) puede editar los perfiles de los empleados y ver una lista completa de todos los usuarios.
- Los empleados solo pueden ver y editar su propio perfil pero tienen acceso a todas las funcionalidades del sistema.
- Los administradores pueden ver datos extra dentro de los detalles de Pago y Reserva: Quién los creó y, en caso de que aplique, quién los anuló.

**Entidades**
- El sistema permite gestionar (CRUD) propietarios, inquilinos, inmuebles, tipo de inmuebles, reservas y pagos.
- Se pueden buscar inmuebles basándose en cupo de personas, fecha de disponibilidad y tipo de inmueble.
- En la creación de inmuebles se establece una seña que se debe de pagar por adelantado + el monto diario.
- Al crear una reserva, se calcula automáticamente cuanto hay que pagar por día basándose en el monto diario del inmueble multiplicado por el total de días reservados menos la seña que se pagó por adelantado.
- Al finalizar una reserva antes de tiempo (no es lo mismo que la baja lógica de la tabla del index) desde la gestión de la misma, se genera un Pago automático con concepto de **Multa** y se calcula el valor de la misma.
- Se puede **renovar** una reserva desde la vista de detalles de la misma, creando así una nueva reserva con el mismo inquilino e inmueble, distintas fechas y valor.
- Se pueden ver todos los pagos realizados, incluso los "anulados". También se pueden reactivar los pagos desde la lista.

### Falta implementar:
- La lista de informes al final de la narrativa.
- Mejoras de calidad de vida
---

### Usuarios de prueba:

**Administrador:**
- usuario: admin
- contraseña: admin

**Empleado:**
- usuario: empleado2
- contraseña: 123456

> También se pueden crear nuevos usuarios pero solo con el rol de empleado.


---

##  Modelado de Datos

A continuación se presenta el esquema del modelo de datos correspondiente a la aplicación:

### Diagrama Entidad-Relación (DER) / Diagrama de Clases

![Diagrama del Proyecto](/docs/DER.png)

---

## Guía de Instalación y Ejecución

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

El repositorio incluye un dump completo de la base de datos con datos de prueba, registros cargados e imágenes (`/database/inmobiliaria_db.sql`).

**Pasos para importarla:**

1. Abrí **XAMPP Control Panel** e iniciá los servicios de **Apache** y **MySQL**.
2. Ingresá a **phpMyAdmin** desde tu navegador ([http://localhost/phpmyadmin](http://localhost/phpmyadmin)).
3. Creá una nueva base de datos llamada **`inmobiliaria_db`**:
4. Seleccioná la base recién creada (`inmobiliaria_db`) y andá a la pestaña **Importar** en el menú superior.
5. Hacé clic en **Seleccionar archivo** y buscá el archivo `inmobiliaria_db.sql` ubicado dentro de la carpeta `database/` del proyecto.
6. Desplázate hasta el final y hacé clic en **Importar**.

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

### 4. Ejecutar la aplicación

Para iniciar el servidor de desarrollo:

`dotnet run`
o `dotnet run --urls "http://localhost:5000"` si querés elegir el puerto, reemplazando '5000' por el puerto elegido.

La terminal te indicará la URL local (http://localhost:5226 por defecto). Abrí esa dirección en tu navegador para probar el sistema.