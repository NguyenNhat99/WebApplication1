function getBadgeByIdStatus(statusid) {
    switch (statusid) {
        case 5:
            return 'bg-success';
        case 4:
        case 6:
            return 'bg-danger';
        case 2:
            return 'bg-primary';
        default:
            return 'bg-secondary';
    }
}
function getNameStatusOrderById(statusid) {
    switch (statusid) {
        case 5:
            return 'Thành công';
        case 4:
            return 'Đã hủy'
        case 6:
            return 'Thất bại';
        case 2:
            return 'Đang vận chuyển';
        default:
            return 'Chờ xác nhận';
    }
}