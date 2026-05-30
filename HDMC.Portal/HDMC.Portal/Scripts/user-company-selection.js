(function () {
    var allCompanies = document.getElementById("IsAllCompanies");
    var companyOptions = document.querySelectorAll(".company-option");

    if (!allCompanies) {
        return;
    }

    function syncCompanyOptions() {
        for (var i = 0; i < companyOptions.length; i++) {
            companyOptions[i].disabled = allCompanies.checked;
        }
    }

    allCompanies.addEventListener("change", syncCompanyOptions);
    syncCompanyOptions();
})();
