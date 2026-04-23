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
            loadDepartmentsForPrint();
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
        const isAllDepartments = printAllRadio.checked;

        const url = new URL(currentPrintUrl, window.location.origin);
        url.searchParams.set('orientation', orientation);
        
        if (!isAllDepartments) {
            const selectedOptions = Array.from(printDepartmentsSelect.selectedOptions);
            const departmentIds = selectedOptions.map(option => option.value);
            
            if (departmentIds.length === 0) {
                alert('Выберите хотя бы один отдел для печати');
                return;
            }
            
            url.searchParams.set('departments', departmentIds.join(','));
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