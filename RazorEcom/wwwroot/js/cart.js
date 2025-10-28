docdocument.addEventListener('DOMContentLoaded', () => {
    const cartToggle = document.getElementById('cartToggle');
    const cartSidebar = document.getElementById('cartSidebar');
    const closeCart = document.getElementById('closeCart');

    cartToggle.addEventListener('click', (e) => {
        e.preventDefault();
        cartSidebar.classList.add('active'); // bật sidebar
    });

    closeCart.addEventListener('click', () => {
        cartSidebar.classList.remove('active'); // tắt sidebar
    });

    // Click ra ngoài sidebar đóng nó
    document.addEventListener('click', (e) => {
        if (!cartSidebar.contains(e.target) && !cartToggle.contains(e.target)) {
            cartSidebar.classList.remove('active');
        }
    });
});

const cartCount = document.getElementById('cartCount');

function updateCartCount(count) {
    cartCount.textContent = count;
}
