/**
 * Notification Manager
 * Sử dụng Bootstrap 5 Alerts để hiển thị thông báo nổi (Toast-like)
 * * Cách sử dụng:
 * Notification.success("Lưu thành công!");
 * Notification.error("Có lỗi xảy ra!");
 * Notification.warning("Cảnh báo...");
 */

const Notification = {
    // ID của container chứa các thông báo
    containerId: 'notification-float-container',

    // Khởi tạo container nếu chưa có
    init: function() {
        if (!document.getElementById(this.containerId)) {
            const container = document.createElement('div');
            container.id = this.containerId;
            
            // Style cố định ở góc trên bên phải
            Object.assign(container.style, {
                position: 'fixed',
                top: '20px',
                right: '20px',
                zIndex: '9999',
                minWidth: '320px',
                maxWidth: '450px',
                display: 'flex',
                flexDirection: 'column',
                gap: '10px'
            });
            
            document.body.appendChild(container);
        }

        // Tự động ẩn các Alert server-side (nếu có sẵn từ TempData) sau 5s
        const serverAlerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        serverAlerts.forEach(alert => {
            setTimeout(() => {
                this.closeAlert(alert);
            }, 5000);
        });
    },

    // Hàm hiển thị chung
    show: function(message, type = 'success') {
        this.init(); // Đảm bảo container tồn tại
        
        const container = document.getElementById(this.containerId);
        
        // Mapping icon dựa trên loại thông báo
        const icons = {
            success: 'bi-check-circle-fill',
            danger: 'bi-x-circle-fill',
            warning: 'bi-exclamation-triangle-fill',
            info: 'bi-info-circle-fill'
        };

        const icon = icons[type] || icons.info;
        
        // Tạo ID ngẫu nhiên cho alert
        const alertId = 'alert-' + Math.random().toString(36).substr(2, 9);

        // Template HTML cho Alert
        const alertHtml = `
            <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show shadow-lg border-0 d-flex align-items-center" role="alert" style="animation: slideInRight 0.3s ease-out;">
                <i class="bi ${icon} fs-4 me-3"></i>
                <div>
                    ${message}
                </div>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
        
        // Chèn vào container
        const wrapper = document.createElement('div');
        wrapper.innerHTML = alertHtml.trim();
        const alertElement = wrapper.firstChild;
        container.appendChild(alertElement);

        // Tự động tắt sau 4 giây
        setTimeout(() => {
            this.closeAlert(alertElement);
        }, 4000);
    },

    // Helper function để đóng alert mượt mà
    closeAlert: function(element) {
        if (element && element.classList.contains('show')) {
            // Sử dụng Bootstrap API để đóng nếu có, hoặc tự remove class
            if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                const bsAlert = new bootstrap.Alert(element);
                bsAlert.close();
            } else {
                element.classList.remove('show');
                element.addEventListener('transitionend', () => element.remove());
            }
        }
    },

    // Các hàm gọi tắt (Shorthands)
    success: function(msg) { this.show(msg, 'success'); },
    error: function(msg) { this.show(msg, 'danger'); },
    warning: function(msg) { this.show(msg, 'warning'); },
    info: function(msg) { this.show(msg, 'info'); }
};

// Thêm keyframes animation vào trang
const styleSheet = document.createElement("style");
styleSheet.innerText = `
    @keyframes slideInRight {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
`;
document.head.appendChild(styleSheet);

// Khởi chạy khi trang load xong
document.addEventListener('DOMContentLoaded', () => {
    Notification.init();
});