// wwwroot/js/admin-dashboard.js

// Thiết lập màu chữ mặc định cho Chart.js (để hợp với theme tối)
Chart.defaults.color = '#adb5bd';
Chart.defaults.borderColor = 'rgba(255, 255, 255, 0.1)';

document.addEventListener("DOMContentLoaded", () => {
    // Gọi API (handler OnGetChartDataAsync) để lấy dữ liệu
    fetch('?handler=ChartData')
        .then(response => response.json())
        .then(data => {
            if (data) {
                // Vẽ 2 biểu đồ khi có dữ liệu
                initRevenueChart(data.revenue);
                initOrderStatusChart(data.orderStatus);
            }
        })
        .catch(error => console.error('Error fetching chart data:', error));
});

/**
 * Vẽ Biểu đồ Doanh thu (Line Chart)
 */
function initRevenueChart(revenueData) {
    const ctx = document.getElementById('revenueChart');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: revenueData.labels, // ['11/11', '12/11', ...]
            datasets: [{
                label: 'Doanh thu',
                data: revenueData.data, // [100000, 150000, ...]
                borderColor: 'rgba(26, 179, 148, 1)', // Màu xanh
                backgroundColor: 'rgba(26, 179, 148, 0.1)',
                fill: true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value, index, values) {
                            // Format tiền VND
                            return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
                        }
                    }
                }
            },
            plugins: {
                legend: {
                    display: false // Ẩn legend
                }
            }
        }
    });
}

/**
 * Vẽ Biểu đồ Trạng thái Đơn hàng (Doughnut Chart)
 */
function initOrderStatusChart(orderStatusData) {
    const ctx = document.getElementById('orderStatusChart');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: orderStatusData.labels, // ['Pending', 'Completed', ...]
            datasets: [{
                data: orderStatusData.data, // [10, 25, 2]
                backgroundColor: [
                    'rgba(255, 193, 7, 0.7)',  // Vàng (Pending)
                    'rgba(26, 179, 148, 0.7)', // Xanh (Completed)
                    'rgba(220, 53, 69, 0.7)',  // Đỏ (Cancelled)
                    'rgba(13, 110, 253, 0.7)'  // Xanh dương (Shipping)
                ],
                borderColor: '#212529' // Nền tối
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom', // Hiển thị legend ở dưới
                }
            }
        }
    });
}