$(document).ready(function () {
    $('#signInModal').modal('show');
});

document.querySelector("#signInForm").addEventListener("submit", function (event) {
    var mail = document.querySelector(".sing-in-mail");
    var password = document.querySelector(".sing-in-password");
    var elemButton = document.querySelector(".sing-in-button");

    document.querySelector("body").style.pointerEvents = "none";
    elemButton.style.backgroundColor = "#a67dff";
    elemButton.innerHTML = "Загрузка";

    // Снимаем отображение ошибок
    mail.parentElement.classList.remove('has-error');
    password.parentElement.classList.remove('has-error');
    mail.parentElement.querySelector('.error-message').style.display = 'none';
    password.parentElement.querySelector('.error-message').style.display = 'none';
})