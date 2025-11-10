document.addEventListener('DOMContentLoaded', function () {
    const printModal = new bootstrap.Modal(document.getElementById('printSettingsModal'));
    let currentPrintUrl = '';

    const printAllRadio = document.getElementById('printAllDepartments');
    const printSelectedRadio = document.getElementById('printSelectedDepartments');
    const printDepartmentSelection = document.getElementById('printDepartmentSelection');
    const printDepartmentsSelect = document.getElementById('printDepartmentsSelect');

    document.querySelectorAll('[data-bs-toggle="modal"][data-bs-target="#printSettingsModal"]').forEach(button => {
        button.addEventListener('click', function () {
            currentPrintUrl = this.getAttribute('data-print-url');
            const isLogs = currentPrintUrl && (currentPrintUrl.indexOf('/Pdf/LoginLogs') === 0 || currentPrintUrl.indexOf('/Pdf/Logs') === 0);

            if (isLogs) {
                if (printDepartmentSelection) {
                    printDepartmentSelection.style.display = 'none';
                }
                if (printAllRadio && printSelectedRadio) {
                    printAllRadio.closest('.mb-3').style.display = 'none';
                }
            } else {
                if (printAllRadio && printSelectedRadio) {
                    printAllRadio.closest('.mb-3').style.display = '';
                    printAllRadio.checked = true;
                }
                if (printDepartmentSelection) {
                    printDepartmentSelection.style.display = 'none';
                }
                loadDepartmentsForPrint();
            }
        });
    });

    printAllRadio.addEventListener('change', function() {
        if (this.checked) {
            printDepartmentSelection.style.display = 'none';
        }
    });

    printSelectedRadio.addEventListener('change', function() {
        if (this.checked) {
            printDepartmentSelection.style.display = 'block';
        }
    });

    function loadDepartmentsForPrint() {
        fetch('/api/search/departments')
            .then(response => response.json())
            .then(departments => {
                printDepartmentsSelect.innerHTML = '';
                departments.forEach(dept => {
                    const option = document.createElement('option');
                    option.value = dept.id;
                    option.textContent = dept.fullName || dept.name;
                    printDepartmentsSelect.appendChild(option);
                });
            })
            .catch(error => {
                console.error('Ошибка загрузки отделов для печати:', error);
            });
    }

    document.getElementById('confirmPrint').addEventListener('click', function () {
        const orientation = document.getElementById('printOrientation').value;
        const isLogs = currentPrintUrl && (currentPrintUrl.indexOf('/Pdf/LoginLogs') === 0 || currentPrintUrl.indexOf('/Pdf/Logs') === 0);
        const isAllDepartments = isLogs ? true : printAllRadio.checked;

        const url = new URL(currentPrintUrl, window.location.origin);
        url.searchParams.set('orientation', orientation);
        
        if (!isLogs && !isAllDepartments) {
            const selectedOptions = Array.from(printDepartmentsSelect.selectedOptions);
            const departmentIds = selectedOptions.map(option => option.value);
            
            if (departmentIds.length === 0) {
                alert('Выберите хотя бы один отдел для печати');
                return;
            }
            
            url.searchParams.set('departments', departmentIds.join(','));
        }
        // Pass filters for logs pages
        const loginLogsForm = document.getElementById('loginLogsFilterForm');
        const systemLogsForm = document.getElementById('logsFilterForm');
        const activeLogsForm = loginLogsForm || systemLogsForm;
        if (activeLogsForm) {
            const q = activeLogsForm.querySelector('input[name="q"]').value.trim();
            const from = activeLogsForm.querySelector('input[name="from"]').value;
            const to = activeLogsForm.querySelector('input[name="to"]').value;
            if (q) url.searchParams.set('search', q);
            if (from) url.searchParams.set('startDate', from);
            if (to) url.searchParams.set('endDate', to);
        }

        window.open(url.toString(), '_blank');

        printModal.hide();
    });
});