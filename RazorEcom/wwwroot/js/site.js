// Global Notification Utility using SweetAlert2
const Notification = {
    // Configuration
    config: {
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer)
            toast.addEventListener('mouseleave', Swal.resumeTimer)
        }
    },

    // Success Toast
    success: function (message) {
        Swal.fire({
            ...this.config,
            icon: 'success',
            title: message
        });
    },

    // Error Toast
    error: function (message) {
        Swal.fire({
            ...this.config,
            icon: 'error',
            title: message
        });
    },

    // Warning Toast
    warning: function (message) {
        Swal.fire({
            ...this.config,
            icon: 'warning',
            title: message
        });
    },

    // Info Toast
    info: function (message) {
        Swal.fire({
            ...this.config,
            icon: 'info',
            title: message
        });
    },

    // Confirmation Dialog
    confirm: function (message, callback, title = 'Bạn có chắc chắn?') {
        Swal.fire({
            title: title,
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#4F46E5', // Primary Color
            cancelButtonColor: '#d33',
            confirmButtonText: 'Đồng ý',
            cancelButtonText: 'Hủy bỏ'
        }).then((result) => {
            if (result.isConfirmed && callback) {
                callback();
            }
        });
    }
};

// Expose to global scope
window.Notification = Notification;
