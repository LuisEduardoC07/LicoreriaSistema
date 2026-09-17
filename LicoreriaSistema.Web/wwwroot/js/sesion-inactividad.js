let inactivityTimer = null;
let warningTimer = null;
let countdownTimer = null;
let keepAliveTimer = null;

let inactivityLimit = 5 * 60 * 1000;
let warningBefore = 30 * 1000;
let keepAliveInterval = 60 * 1000;

let running = false;
let lastActivity = Date.now();
let lastKeepAlive = 0;
let activityScheduled = false;

const activityEvents = [
    "click",
    "keydown",
    "mousemove",
    "mousedown",
    "touchstart",
    "scroll",
    "pointerdown"
];

function elementosAviso() {
    return {
        warning:
            document.getElementById(
                "session-timeout-warning"),

        seconds:
            document.getElementById(
                "session-timeout-seconds")
    };
}

function registrarActividad() {
    if (!running) {
        return;
    }

    lastActivity = Date.now();

    ocultarAviso();

    reiniciarTemporizadores();

    if (!activityScheduled) {
        activityScheduled = true;

        window.setTimeout(
            async () => {
                activityScheduled = false;

                await renovarSesion();
            },
            1000);
    }
}

function reiniciarTemporizadores() {
    if (inactivityTimer !== null) {
        window.clearTimeout(
            inactivityTimer);
    }

    if (warningTimer !== null) {
        window.clearTimeout(
            warningTimer);
    }

    inactivityTimer =
        window.setTimeout(
            cerrarPorInactividad,
            inactivityLimit);

    warningTimer =
        window.setTimeout(
            mostrarAviso,
            Math.max(
                0,
                inactivityLimit -
                warningBefore));
}

function mostrarAviso() {
    const { warning, seconds } =
        elementosAviso();

    if (!warning || !seconds || !running) {
        return;
    }

    warning.hidden = false;

    let restantes =
        Math.ceil(
            warningBefore / 1000);

    seconds.textContent =
        restantes.toString();

    if (countdownTimer !== null) {
        window.clearInterval(
            countdownTimer);
    }

    countdownTimer =
        window.setInterval(
            () => {
                restantes--;

                if (restantes <= 0) {
                    window.clearInterval(
                        countdownTimer);

                    countdownTimer = null;

                    return;
                }

                seconds.textContent =
                    restantes.toString();
            },
            1000);
}

function ocultarAviso() {
    const { warning } =
        elementosAviso();

    if (warning) {
        warning.hidden = true;
    }

    if (countdownTimer !== null) {
        window.clearInterval(
            countdownTimer);

        countdownTimer = null;
    }
}

async function renovarSesion() {
    if (!running) {
        return;
    }

    const ahora = Date.now();

    if (
        ahora - lastKeepAlive <
        keepAliveInterval
    ) {
        return;
    }

    if (
        ahora - lastActivity >=
        inactivityLimit
    ) {
        return;
    }

    try {
        const response =
            await fetch(
                "/api/auth/actividad",
                {
                    method: "POST",
                    credentials: "same-origin"
                });

        if (response.status === 401) {
            running = false;

            window.location.href =
                "/login";

            return;
        }

        if (response.ok) {
            lastKeepAlive = ahora;
        }
    }
    catch {
        // El temporizador local continúa vigente.
    }
}

async function cerrarPorInactividad() {
    if (!running) {
        return;
    }

    running = false;

    ocultarAviso();

    try {
        await fetch(
            "/api/auth/logout",
            {
                method: "POST",
                credentials: "same-origin"
            });
    }
    catch {
        // Aunque falle la petición, el usuario
        // será enviado a la pantalla de login.
    }

    window.location.href =
        "/login?motivo=inactividad";
}

function iniciarControles() {
    for (const eventName of activityEvents) {
        document.addEventListener(
            eventName,
            registrarActividad,
            {
                passive: true
            });
    }

    const botonContinuar =
        document.getElementById(
            "session-continue-button");

    if (botonContinuar) {
        botonContinuar.addEventListener(
            "click",
            registrarActividad);
    }

    keepAliveTimer =
        window.setInterval(
            renovarSesion,
            15000);

    lastActivity = Date.now();
    lastKeepAlive = Date.now();

    reiniciarTemporizadores();
}

function detenerControles() {
    for (const eventName of activityEvents) {
        document.removeEventListener(
            eventName,
            registrarActividad);
    }

    if (inactivityTimer !== null) {
        window.clearTimeout(
            inactivityTimer);

        inactivityTimer = null;
    }

    if (warningTimer !== null) {
        window.clearTimeout(
            warningTimer);

        warningTimer = null;
    }

    if (countdownTimer !== null) {
        window.clearInterval(
            countdownTimer);

        countdownTimer = null;
    }

    if (keepAliveTimer !== null) {
        window.clearInterval(
            keepAliveTimer);

        keepAliveTimer = null;
    }

    ocultarAviso();

    running = false;
}

export function iniciar(
    limiteInactividad,
    avisoAntes,
    intervaloKeepAlive
) {
    detenerControles();

    inactivityLimit =
        Number(limiteInactividad) ||
        5 * 60 * 1000;

    warningBefore =
        Number(avisoAntes) ||
        30 * 1000;

    keepAliveInterval =
        Number(intervaloKeepAlive) ||
        60 * 1000;

    running = true;

    lastActivity = Date.now();
    lastKeepAlive = Date.now();

    iniciarControles();
}

export function detener() {
    detenerControles();
}
