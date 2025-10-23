document.addEventListener('DOMContentLoaded', function () {
    const printModal = new bootstrap.Modal(document.getElementById('printSettingsModal'));
    let currentPrintUrl = '';

    document.querySelectorAll('[data-bs-toggle="modal"][data-bs-target="#printSettingsModal"]').forEach(button => {
        button.addEventListener('click', function () {
            currentPrintUrl = this.getAttribute('data-print-url');
        });
    });

    document.getElementById('confirmPrint').addEventListener('click', function () {
        const orientation = document.getElementById('printOrientation').value;

        const url = new URL(currentPrintUrl, window.location.origin);
        url.searchParams.set('orientation', orientation);
        
        const departmentsSelect = document.getElementById('departments');
        const departmentSearchInput = document.getElementById('departmentSearch');
        
        if (departmentsSelect) {
            const selectedDepartments = Array.from(departmentsSelect.selectedOptions).map(option => option.value);
            if (selectedDepartments.length > 0) {
                url.searchParams.set('departments', selectedDepartments.join(','));
            }
        }
        
        if (departmentSearchInput && departmentSearchInput.value.trim()) {
            url.searchParams.set('departmentSearch', departmentSearchInput.value.trim());
        }
        
        const logsSearchInput = document.getElementById('logsFilterForm')?.querySelector('input[name="q"]');
        const logsFromInput = document.getElementById('logsFilterForm')?.querySelector('input[name="from"]');
        const logsToInput = document.getElementById('logsFilterForm')?.querySelector('input[name="to"]');
        
        if (logsSearchInput && logsSearchInput.value.trim()) {
            url.searchParams.set('search', logsSearchInput.value.trim());
        }
        if (logsFromInput && logsFromInput.value) {
            url.searchParams.set('startDate', logsFromInput.value);
        }
        if (logsToInput && logsToInput.value) {
            url.searchParams.set('endDate', logsToInput.value);
        }

        window.open(url.toString(), '_blank');

        printModal.hide();
    });
});