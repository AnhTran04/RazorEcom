document.addEventListener("DOMContentLoaded", () => {

    const forms = document.querySelectorAll("form[asp-page-handler='AddToCart']");
    const toast = document.getElementById("cart-toast");

    forms.forEach(form => {
        form.addEventListener("submit", event => {
            showToast("Đã thêm vào giỏ hàng!");
        });
    });

    function showToast(message) {
        toast.textContent = message;
        toast.classList.add("show");

        setTimeout(() => {
            toast.classList.remove("show");
        }, 2000);
    }
});
