function DeleteProductFromCart(id) {
    fetch(`/cart/deleteitemincart/${id}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            location.href = "/cart/index";

        })
        .catch(error => {
            alert("Error")
        });
}

function AddCart(id, quantity) {
    fetch(`/api/shoppingcart/insert/${id}&${quantity}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                Swal.fire({
                    position: "top-end",
                    icon: "success",
                    title: data.message,
                    showConfirmButton: false,
                    timer: 800
                }).then(result => {
                    if (result.dismiss === Swal.DismissReason.timer) {
                        location.reload(true);
                    }
                })
            }
        })
        .catch(error => {
            console.log(error)
        });
}
function AddWishList(id) {
    fetch(`/api/wishlist/insertfavouriteproduct/${id}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                Swal.fire({
                    position: "top-end",
                    icon: "success",
                    title: data.message,
                    showConfirmButton: false,
                    timer: 800
                }).then(result => {
                    if (result.dismiss === Swal.DismissReason.timer) {
                        location.reload(true);
                    }
                })
            } else {
                Swal.fire({
                    position: "top-end",
                    icon: "error",
                    title: data.message,
                    showConfirmButton: false,
                    timer: 800
                }).then(result => {
                    if (result.dismiss === Swal.DismissReason.timer) {
                        location.reload(true);
                    }
                })
            }
        })
        .catch(error => {
            console.log(error)
        });
}