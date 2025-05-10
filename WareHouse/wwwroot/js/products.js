// Очистка формы перед добавлением новой категории
function clearForm() {
    document.getElementById('categoryId').value = 0;
    document.getElementById('categoryName').value = '';
}

// Заполнение формы для редактирования категории
function editCategory(id, name) {
    console.log("products.js загружен");
    document.getElementById('categoryId').value = id;
    document.getElementById('categoryName').value = name;
    $('#addCategoryModal').modal('show'); // Открываем модальное окно
}

// Удаление категории
async function deleteCategory(id) {
    if (!confirm('Вы уверены, что хотите удалить эту категорию?')) {
        return;
    }

    try {
        const response = await fetch(`/Products/DeleteCategory?id=${id}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (response.ok) {
            location.reload();
        } else {
            const errorText = await response.text();
            alert(errorText || 'Произошла ошибка при удалении');
        }
    } catch (error) {
        console.error('Ошибка:', error);
        alert('Не удалось выполнить запрос');
    }
}
$(document).ready(function () {

    
    // Динамический поиск
    function dynamicSearch() {
        var searchText = $('#searchInput').val();
        var selectedCategoryId = $('#categoryFilter').val();
        const urlParams = new URLSearchParams(window.location.search);

        let showHidden = false;
        if (urlParams.has('isHidden')) {
            showHidden = urlParams.get('isHidden') === 'True';
        }

        var url = '/Products/DynamicSearch?searchText=' + encodeURIComponent(searchText) +
            '&categoryId=' + (selectedCategoryId ? selectedCategoryId : "") +
            '&isHidden=' + showHidden;

        $.ajax({
            url: url,
            type: 'GET',
            success: function (data) {
                $('#productTableBody').html(data);
                attachEditButtonHandlers();
            },
            error: function (xhr, status, error) {
                console.error("Ошибка при поиске:", error);
                $('#productTableBody').html("<tr><td colspan='7'>Произошла ошибка при поиске.</td></tr>");
            }
        });
    }

    // Навешиваем обработчики для кнопок редактирования
    function attachEditButtonHandlers() {
        $('.edit-product-btn').off('click').on('click', function () {
            var productId = $(this).data('product-id');
            var productName = $(this).data('product-name');
            var productPrice = parseFloat($(this).data('product-price'));
            var productSpecificGravity = $(this).data('product-specific-gravity');
            var productWeight = $(this).data('product-weight');
            var productMaterialBrand = $(this).data('product-material-brand');
            var productMaterialType = $(this).data('product-material-type');


            $('#editProductId').val(productId);
            $('#editProductName').val(productName);
            $('#editProductPrice').val(productPrice);
            $('#editProductSpecificGravity').val(productSpecificGravity);
            $('#editProductWeight').val(productWeight);
            $('#editProductMaterialBrand').val(productMaterialBrand);
            $('#editProductMaterialType').val(productMaterialType);


            $('#editProductModal').modal('show');
        });
    }

    // Обработка отправки формы редактирования
    $('#editProductForm').submit(function (e) {
        e.preventDefault();
        var form = $(this);
        $.ajax({
            url: '/Products/EditProduct',
            type: 'POST',
            data: form.serialize(),
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
});