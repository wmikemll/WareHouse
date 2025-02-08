function openEditEmployeeModal(employeeId, employeeName, employeeRole) {
    // Заполняем поля модального окна данными сотрудника
    document.getElementById('editEmployeeId').value = employeeId;
    document.getElementById('editEmployeeName').value = employeeName;
    document.getElementById('editEmployeeRole').value = employeeRole;

    // Открываем модальное окно
    $('#editEmployeeModal').modal('show');
}

function saveEmployeeChanges() {
    // Получаем данные из формы
    var employeeId = document.getElementById('editEmployeeId').value;
    var employeeName = document.getElementById('editEmployeeName').value;
    var employeeEmail = document.getElementById('editEmployeeEmail').value;
    var employeeRole = document.getElementById('editEmployeeRole').value;

    // Отправляем данные на сервер (AJAX)
    $.ajax({
        url: '/Employees/Edit', //  URL для отправки данных на сервер
        type: 'POST', //  Тип запроса
        //  Передаем данные в параметре data
        {
            Id: employeeId,
            Name: employeeName,
            Email: employeeEmail,
            Role: employeeRole
        },
        success: function (data) {
            //  Обработка успешного ответа от сервера
            $('#editEmployeeModal').modal('hide'); //  Закрываем модальное окно
            //  Обновляем таблицу с сотрудниками
            location.reload(); //  Перезагружаем страницу (или можно обновить таблицу динамически)
        },
        error: function (jqXHR, textStatus, errorThrown) {
            //  Обработка ошибки
            //console.error('Ошибка при сохранении изменений:', textStatus, errorThrown);
            var errorMessage = jqXHR.responseText || 'Произошла ошибка при сохранении изменений.';
            alert('Ошибка при сохранении изменений: ' + errorMessage);

            // Дополнительно можно вывести сообщение об ошибке в консоль
            console.error('Ошибка при сохранении изменений:', errorMessage);
        }
    });
}