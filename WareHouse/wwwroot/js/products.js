function showLowStockProducts() {
    // Получаем состояние чекбокса
    var showLowStock = $('#showLowStockProducts').prop('checked');
    dynamicSearch(); // Вызываем dynamicSearch, чтобы обновить результаты
}

function resetProductFilter() {
    // Сбрасываем значения полей фильтров
    $('#searchInput').val('');
    $('#categoryFilter').val('');
    $('#minPrice').val('');
    $('#maxPrice').val('');
    $('#showLowStockProducts').prop('checked', false); // Сбрасываем чекбокс

    dynamicSearch(); // Обновляем результаты после сброса фильтров
}

function dynamicSearch() {
    var searchText = $('#searchInput').val();
    var selectedCategoryId = $('#categoryFilter').val();
    var minPrice = $('#minPrice').val() || 0;
    var maxPrice = $('#maxPrice').val() || Number.MAX_SAFE_INTEGER;
    var showLowStock = $('#showLowStockProducts').prop('checked'); // Получаем состояние чекбокса

    var url = '/Products/DynamicSearch?searchText=' + encodeURIComponent(searchText) +
        '&categoryId=' + (selectedCategoryId ? selectedCategoryId : "") +
        '&minPrice=' + minPrice +
        '&maxPrice=' + maxPrice +
        '&showLowStock=' + showLowStock; // Добавляем параметр showLowStock

    console.log(url);

    $.ajax({
        url: url,
        type: 'GET',
        success: function (data) {
            $('#productTableBody').html(data);
            attachEditButtonHandlers();
        },
        error: function (xhr, status, error) {
            console.error("Ошибка при поиске:", error);
            $('#productTableBody').html("<tr><td colspan='5'>Произошла ошибка при поиске.</td></tr>");
        }
    });
}

function attachEditButtonHandlers() {
    $('.edit-product-btn').click(function () {
        var productId = $(this).data('product-id');
        var productName = $(this).data('product-name');
        var productPrice = parseFloat($(this).data('product-price'));
        var productCount = $(this).data('product-count');
        var productCategoryId = $(this).data('product-category-id');

        $('#editProductId').val(productId);
        $('#editProductName').val(productName);
        $('#editProductPrice').val(productPrice);
        $('#editProductCount').val(productCount);
        $('#editProductCategory').val(productCategoryId);

        $('#editProductModal').modal('show');
    });
}

$('#editProductForm').submit(function (e) {
    e.preventDefault();
    var form = $(this);
    $.ajax({
        url: '/Products/EditProduct',
        type: 'POST',
        data: form.serialize(), // фикс
        success: function (data) {
            if (data.success) {
                $('#editProductModal').modal('hide');
                dynamicSearch();
                alert(data.message);
            } else {
                alert('Ошибка при сохранении изменений.');
            }
        },
        error: function (xhr, status, error) {
            alert('Произошла ошибка при сохранении: ' + error);
        }
    });
});

// Навешиваем обработчики для динамического поиска
$('#searchInput').on('input', dynamicSearch);
$('#categoryFilter').on('change', dynamicSearch);

// Инициализация
dynamicSearch();
