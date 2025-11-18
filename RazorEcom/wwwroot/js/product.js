document.addEventListener("DOMContentLoaded", () => {

    const token = document.querySelector("#anti-fg input[name='__RequestVerificationToken']").value;

    document.querySelectorAll(".btn-add-to-cart").forEach(button => {
        button.addEventListener("click", async () => {
            const variantId = button.dataset.variantId;

            const response = await fetch("?handler=AddToCart", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify({ variantId: Number(variantId) })
            });

            const result = await response.json();

            showToast(result.message, result.success);
        });
    });
});


function showToast(message, success) {
    const toast = document.getElementById("cart-toast");
    toast.innerText = message;
    toast.classList.add("show");

    setTimeout(() => toast.classList.remove("show"), 3000);
}
