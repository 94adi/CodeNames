export function openModal(modalId, title = '', body = '') {

    const modal = document.getElementById(modalId);
    const modalTitle = document.getElementById(`${modalId}-title`);
    const modalBody = document.getElementById(`${modalId}-body`);

    if (modal) {
        if (title) modalTitle.textContent = title;
        if (body) modalBody.textContent = body;
        modal.style.display = "block";
    }
}

document.addEventListener("DOMContentLoaded", function () {
    const closeButtons = document.querySelectorAll(".close");

    closeButtons.forEach(button => {
        button.onclick = function () {
            const modal = button.closest(".modal");
            if (modal) {
                modal.style.display = "none";
            }
        };
    });

    window.onclick = function (event) {
        if (event.target.classList.contains("modal")) {
            event.target.style.display = "none";
        }
    };
});
