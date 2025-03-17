function filterSales() {
    var userId = $('#userFilter').val();
    var date = $('#dateFilter').val();
    var saleId = $('#saleIdSearch').val();

    var url = '/Sales/FilterSales?userId=' + userId +
        '&date=' + date +
        '&saleId=' + saleId;

    console.log("Filtering sales with URL: " + url);

    $.ajax({
        url: url,
        type: 'GET',
        success: function (data) {
            $('#salesTableBody').html(data);
        },
        error: function (xhr, status, error) {
            console.error("Ошибка при фильтрации продаж:", error);
            $('#salesTableBody').html("<tr><td colspan='5'>Произошла ошибка при фильтрации.</td></tr>");
        }
    });
}
function clearFilters() {
    $('#userFilter').val('');  // Сбрасываем выпадающий список
    $('#dateFilter').val('');    // Очищаем поле даты
    $('#saleIdSearch').val('');  // Очищаем поле поиска по ID

    // Вызываем filterSales, чтобы загрузить все продажи
    filterSales();
}
