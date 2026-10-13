// Muestra la tapa de un disco en vivo mientras el administrador escribe la URL
document.addEventListener('DOMContentLoaded', function () {
    var input = document.querySelector('[data-preview-tapa]');
    var img = document.getElementById('previewTapa');
    if (!input || !img) return;

    function actualizar() {
        var url = input.value.trim();
        img.hidden = url.length === 0;
        if (url.length > 0) img.src = url;
    }

    input.addEventListener('input', actualizar);
    actualizar();
});
