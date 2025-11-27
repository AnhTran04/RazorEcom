document.addEventListener("DOMContentLoaded", () => {

    const tokenInput = document.querySelector("input[name='__RequestVerificationToken']");
    const token = tokenInput ? tokenInput.value : null;

    if (token) {
        document.querySelectorAll(".btn-add-to-cart").forEach(button => {
            button.addEventListener("click", async () => {
                const variantId = button.dataset.variantId;

                try {
                    const response = await fetch("?handler=AddToCart", {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                            "RequestVerificationToken": token
                        },
                        body: JSON.stringify({ variantId: Number(variantId) })
                    });

                    if (response.ok) {
                        const result = await response.json();
                        if (result.success) {
                            Notification.success(result.message);
                        } else {
                            Notification.error(result.message);
                        }
                    } else {
                        Notification.error("Đã xảy ra lỗi khi thêm vào giỏ hàng.");
                    }
                } catch (error) {
                    console.error("Error adding to cart:", error);
                    Notification.error("Không thể kết nối đến máy chủ.");
                }
            });
        });
    }
});
