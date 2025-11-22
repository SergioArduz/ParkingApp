// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("searchPlate");
    const statusFilter = document.getElementById("filterStatus");
    const rows = document.querySelectorAll(".parking-row");

    function filterTable() {
        const searchText = searchInput.value.toLowerCase();
        const statusValue = statusFilter.value;

        rows.forEach(row => {
            const plate = row.querySelector(".plate").textContent.toLowerCase();
            const status = row.getAttribute("data-status");

            const matchesPlate = plate.includes(searchText);
            const matchesStatus = statusValue === "" || status === statusValue;

            row.style.display = (matchesPlate && matchesStatus) ? "" : "none";
        });
    }

    searchInput.addEventListener("keyup", filterTable);
    statusFilter.addEventListener("change", filterTable);
});

