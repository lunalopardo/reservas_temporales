document.addEventListener("DOMContentLoaded", function () {
    var alerta = document.getElementById("alerta-mensaje");
    if (alerta) {
        setTimeout(function () {
            alerta.style.opacity = "0";
            alerta.style.transition = "opacity 0.5s ease";
            setTimeout(function () {
                alerta.style.display = "none";
            }, 500);
        }, 2000);
    }
});