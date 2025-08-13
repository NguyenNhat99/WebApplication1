function Delete(id, cl) {
    Swal.fire({
        title: "Bạn chắc chắn?",
        text: "Bạn sẽ không thể khôi phục khi đã xóa",
        icon: "warning",
        showCancelButton: true,
        cancelButtonColor: "#d33",
        cancelButtonText: "Đóng",
        confirmButtonColor: "#3085d6",
        confirmButtonText: "Xóa"
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/api/${cl}/delete/${id}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    Swal.fire({
                        position: "center-center",
                        icon: "success",
                        title: data.message,
                        showConfirmButton: true,
                        timer: 1500
                    }).then((result) => {
                        if (result.dismiss === Swal.DismissReason.timer || result.isConfirmed) {
                            location.reload();
                        }
                    });
                } else {
                    Swal.fire({
                        position: "center-center",
                        icon: "error",
                        title: data.message,
                        showConfirmButton: true,
                        timer: 1500
                    }).then((result) => {
                        location.reload();
                    });
                }
            })
                .catch(error => {
                    console.log(error);
                alert("Error")
            });
        }
    });
}

function Active(id, cl) {
    fetch(`/api/${cl}/active/${id}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => response.json())
    .then(data => {
        location.reload();
    })
    .catch(error => {
        alert("Error")
    });
}

//Sử dụng cho trạng thái đơn hàng để lấy ra màu của background badge

