// ============================================================
// RESERVAS.JS
// CREATE + EDIT
// ============================================================


// ============================================================
// ELEMENTOS
// ============================================================

const inmuebleSelect =
    document.getElementById("IdInmueble");

const fechaDesdeInput =
    document.getElementById("fechaDesde");

const fechaHastaInput =
    document.getElementById("fechaHasta");


// ============================================================
// ID DE LA RESERVA ACTUAL
// ============================================================

const idReservaInput =
    document.getElementById("IdReserva");

const idReserva =
    idReservaInput
        ? idReservaInput.value
        : "";


// ============================================================
// RESERVAS DE OTRAS RESERVAS
// ============================================================

let reservasOcupadas = [];


// ============================================================
// RESERVA QUE ESTAMOS EDITANDO
// ============================================================

let reservaActual = null;


// ============================================================
// CONVERTIR FECHA A YYYY-MM-DD
// ============================================================

function convertirFecha(fecha) {

    return fecha.getFullYear() +
        "-" +
        String(fecha.getMonth() + 1).padStart(2, "0") +
        "-" +
        String(fecha.getDate()).padStart(2, "0");
}


// ============================================================
// ¿LA FECHA PERTENECE A LA RESERVA ACTUAL?
// ============================================================

function esReservaActual(fecha) {

    if (!reservaActual) {
        return false;
    }

    const fechaTexto =
        convertirFecha(fecha);

    return (
        fechaTexto >= reservaActual.fechaDesde &&
        fechaTexto <= reservaActual.fechaHasta
    );
}


// ============================================================
// ¿LA FECHA ESTA OCUPADA POR OTRA RESERVA?
// ============================================================

function estaOcupada(fecha) {

    const fechaTexto =
        convertirFecha(fecha);

    return reservasOcupadas.some(
        reserva =>
            fechaTexto >= reserva.fechaDesde &&
            fechaTexto <= reserva.fechaHasta
    );
}


// ============================================================
// CARGAR RESERVAS DEL INMUEBLE
// ============================================================

async function cargarReservas(idInmueble) {

    reservasOcupadas = [];

    if (!idInmueble) {

        actualizarCalendarios();

        return;
    }


    try {

        let url =
            `/Reservas/FechasOcupadas?idInmueble=${idInmueble}`;


        // --------------------------------------------------------
        // EDIT
        // Enviamos el ID de nuestra reserva para excluirla
        // de las reservas ocupadas.
        // --------------------------------------------------------

        if (idReserva) {

            url +=
                `&idReserva=${idReserva}`;
        }


        const respuesta =
            await fetch(url);


        if (!respuesta.ok) {

            console.error(
                "No se pudieron cargar las reservas."
            );

            return;
        }


        // ========================================================
        // IMPORTANTE
        // El controlador devuelve:
        //
        // {
        //     reservasOtras: [...],
        //     reservaActual: {...}
        // }
        // ========================================================

        const datos =
            await respuesta.json();


        // --------------------------------------------------------
        // Guardar reservas de OTROS
        // --------------------------------------------------------

        reservasOcupadas =
            datos.reservasOtras || [];


        // --------------------------------------------------------
        // Guardar MI reserva
        // --------------------------------------------------------

        if (datos.reservaActual) {

            reservaActual =
                datos.reservaActual;
        }


        // --------------------------------------------------------
        // CREATE
        // En Create no existe reservaActual.
        // --------------------------------------------------------

        else if (!idReserva) {

            reservaActual = null;
        }


        // ========================================================
        // MOSTRAR EN CONSOLA PARA COMPROBAR
        // ========================================================

        console.log(
            "Reservas de otros:",
            reservasOcupadas
        );

        console.log(
            "Reserva actual:",
            reservaActual
        );


        // ========================================================
        // ACTUALIZAR COLORES
        // ========================================================

        actualizarCalendarios();

    }
    catch (error) {

        console.error(
            "Error al consultar las reservas:",
            error
        );
    }
}


// ============================================================
// CONFIGURAR CALENDARIO
// ============================================================

function configurarCalendario(calendario) {

    if (!calendario) {
        return;
    }


    // ========================================================
    // DESHABILITAR DIAS OCUPADOS
    // ========================================================

    calendario.set(
        "disable",
        [

            function(fecha) {

                // ------------------------------------------------
                // MI reserva NO se bloquea
                // ------------------------------------------------

                if (esReservaActual(fecha)) {

                    return false;
                }


                // ------------------------------------------------
                // Reservas de otros SI se bloquean
                // ------------------------------------------------

                return estaOcupada(fecha);
            }

        ]
    );


    // ========================================================
    // COLORES
    // ========================================================

    calendario.set(
        "onDayCreate",
        [

            function(
                dObj,
                dStr,
                fp,
                dayElem
            ) {

                const fecha =
                    dayElem.dateObj;


                // ------------------------------------------------
                // LIMPIAR CLASES ANTERIORES
                // ------------------------------------------------

                dayElem.classList.remove(
                    "dia-disponible"
                );

                dayElem.classList.remove(
                    "dia-ocupado"
                );

                dayElem.classList.remove(
                    "dia-reserva-actual"
                );


                // =================================================
                // AMARILLO
                // MI RESERVA
                // =================================================

                if (
                    esReservaActual(fecha)
                ) {

                    dayElem.classList.add(
                        "dia-reserva-actual"
                    );

                    return;
                }


                // =================================================
                // ROJO
                // RESERVA DE OTRO
                // =================================================

                if (
                    estaOcupada(fecha)
                ) {

                    dayElem.classList.add(
                        "dia-ocupado"
                    );

                    return;
                }


                // =================================================
                // VERDE
                // DISPONIBLE
                // =================================================

                const hoy =
                    new Date();

                hoy.setHours(
                    0,
                    0,
                    0,
                    0
                );


                if (
                    fecha >= hoy
                ) {

                    dayElem.classList.add(
                        "dia-disponible"
                    );
                }

            }

        ]
    );


    // ========================================================
    // REDIBUJAR
    // ========================================================

    calendario.redraw();
}


// ============================================================
// ACTUALIZAR AMBOS CALENDARIOS
// ============================================================

function actualizarCalendarios() {

    if (
        typeof calendarioDesde !==
        "undefined"
    ) {

        configurarCalendario(
            calendarioDesde
        );
    }


    if (
        typeof calendarioHasta !==
        "undefined"
    ) {

        configurarCalendario(
            calendarioHasta
        );
    }
}


// ============================================================
// CALENDARIO FECHA DESDE
// ============================================================

const calendarioDesde =
    flatpickr(
        fechaDesdeInput,
        {

            locale: "es",

            dateFormat: "Y-m-d",

            minDate: "today",

            allowInput: false,

            disable: [],


            // ====================================================
            // CAMBIO DE FECHA DESDE
            // ====================================================

            onChange:
                function(selectedDates) {

                    if (
                        selectedDates.length === 0
                    ) {

                        return;
                    }


                    const fechaInicio =
                        selectedDates[0];


                    // ------------------------------------------------
                    // Fecha minima de finalizacion
                    // ------------------------------------------------

                    const fechaMinima =
                        new Date(
                            fechaInicio
                        );

                    fechaMinima.setDate(
                        fechaMinima.getDate() + 1
                    );


                    calendarioHasta.set(
                        "minDate",
                        fechaMinima
                    );


                    // ------------------------------------------------
                    // Comprobar fecha hasta
                    // ------------------------------------------------

                    if (
                        calendarioHasta
                            .selectedDates
                            .length > 0
                    ) {

                        const fechaFinal =
                            calendarioHasta
                                .selectedDates[0];


                        // ------------------------------------------------
                        // Si la fecha final es invalida
                        // ------------------------------------------------

                        if (
                            fechaFinal <= fechaInicio
                        ) {

                            calendarioHasta.clear();

                        }


                        // ------------------------------------------------
                        // Si pertenece a otra reserva
                        // ------------------------------------------------

                        else if (
                            estaOcupada(fechaFinal) &&
                            !esReservaActual(fechaFinal)
                        ) {

                            calendarioHasta.clear();
                        }
                    }

                }

        }
    );


// ============================================================
// CALENDARIO FECHA HASTA
// ============================================================

const calendarioHasta =
    flatpickr(
        fechaHastaInput,
        {

            locale: "es",

            dateFormat: "Y-m-d",

            minDate:
                new Date(
                    new Date().setDate(
                        new Date().getDate() + 1
                    )
                ),

            allowInput: false,

            disable: []

        }
    );


// ============================================================
// CAMBIO DE INMUEBLE
// ============================================================

if (inmuebleSelect) {

    inmuebleSelect.addEventListener(
        "change",
        async function() {

            // ------------------------------------------------
            // Si cambia el inmueble,
            // dejamos de considerar la reserva actual.
            // ------------------------------------------------

            reservaActual = null;


            // ------------------------------------------------
            // Limpiar fechas
            // ------------------------------------------------

            calendarioDesde.clear();

            calendarioHasta.clear();


            // ------------------------------------------------
            // Restaurar fecha minima
            // ------------------------------------------------

            calendarioHasta.set(
                "minDate",
                new Date(
                    new Date().setDate(
                        new Date().getDate() + 1
                    )
                )
            );


            // ------------------------------------------------
            // Cargar reservas del nuevo inmueble
            // ------------------------------------------------

            await cargarReservas(
                this.value
            );

        }
    );
}


// ============================================================
// INICIALIZAR
// ============================================================

async function inicializar() {

    // ========================================================
    // EDIT
    // ========================================================

    if (
        idReserva &&
        inmuebleSelect &&
        inmuebleSelect.value
    ) {

        // ----------------------------------------------------
        // LEER FECHAS QUE VIENEN DEL SERVIDOR
        // ----------------------------------------------------

        const fechaDesde =
            fechaDesdeInput.value;

        const fechaHasta =
            fechaHastaInput.value;


        console.log(
            "EDIT - ID reserva:",
            idReserva
        );

        console.log(
            "EDIT - Fecha desde:",
            fechaDesde
        );

        console.log(
            "EDIT - Fecha hasta:",
            fechaHasta
        );


        // ----------------------------------------------------
        // Guardamos inicialmente la reserva actual
        // ----------------------------------------------------

        if (
            fechaDesde &&
            fechaHasta
        ) {

            reservaActual = {

                fechaDesde:
                    fechaDesde,

                fechaHasta:
                    fechaHasta

            };


            console.log(
                "Reserva actual inicial:",
                reservaActual
            );
        }


        // ----------------------------------------------------
        // Cargar reservas del inmueble
        // ----------------------------------------------------

        await cargarReservas(
            inmuebleSelect.value
        );


        // ----------------------------------------------------
        // Restaurar Fecha Desde
        // ----------------------------------------------------

        if (fechaDesde) {

            calendarioDesde.setDate(
                fechaDesde,
                false
            );


            const fechaInicio =
                calendarioDesde
                    .selectedDates[0];


            if (fechaInicio) {

                const fechaMinima =
                    new Date(
                        fechaInicio
                    );

                fechaMinima.setDate(
                    fechaMinima.getDate() + 1
                );


                calendarioHasta.set(
                    "minDate",
                    fechaMinima
                );
            }
        }


        // ----------------------------------------------------
        // Restaurar Fecha Hasta
        // ----------------------------------------------------

        if (fechaHasta) {

            calendarioHasta.setDate(
                fechaHasta,
                false
            );
        }


        // ----------------------------------------------------
        // Actualizar colores
        // ----------------------------------------------------

        actualizarCalendarios();

    }


    // ========================================================
    // CREATE
    // ========================================================

    else if (
        inmuebleSelect &&
        inmuebleSelect.value
    ) {

        await cargarReservas(
            inmuebleSelect.value
        );
    }


    // ========================================================
    // ACTUALIZACION FINAL
    // ========================================================

    actualizarCalendarios();
}


// ============================================================
// EJECUTAR
// ============================================================

inicializar();

