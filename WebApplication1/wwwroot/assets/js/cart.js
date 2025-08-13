/* assets/js/cart.js
   Giỏ hàng Yummy – có seed dữ liệu ảo, quản lý localStorage, render bảng, +/- số lượng, xóa, xóa toàn bộ, đặt hàng giả lập
*/

// ====== CẤU HÌNH ======
const STORAGE_KEY = "yummy_cart";
const CURRENCY_LOCALE = "vi-VN";
const CURRENCY_CODE = "VND";

// ====== DỮ LIỆU ẢO (SEED) ======
const DUMMY_ITEMS = [
  {
    id: 101,
    sku: "FO-CH-001",
    name: "Cơm gà xối mỡ",
    price: 45000,
    quantity: 2,
    imageUrl: "/assets/img/food/com-ga.jpg",
  },
  {
    id: 102,
    sku: "FO-PH-002",
    name: "Phở bò tái",
    price: 55000,
    quantity: 1,
    imageUrl: "/assets/img/food/pho-bo.jpg",
  },
  {
    id: 103,
    sku: "FO-TR-003",
    name: "Trà đào cam sả",
    price: 30000,
    quantity: 3,
    imageUrl: "/assets/img/food/tra-dao.jpg",
  },
];

// ====== TIỆN ÍCH ======
const fmt = (n) =>
  new Intl.NumberFormat(CURRENCY_LOCALE, { style: "currency", currency: CURRENCY_CODE }).format(
    Math.max(0, Number(n) || 0)
  );

function escapeHtml(str) {
  return String(str || "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
}

function loadCart() {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY)) || [];
  } catch {
    return [];
  }
}

function saveCart(cart) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(cart));
}

function seedIfEmpty() {
  const current = loadCart();
  if (!current || current.length === 0) {
    saveCart(DUMMY_ITEMS);
  }
}

function getSubtotal(cart) {
  return cart.reduce((s, i) => s + i.price * i.quantity, 0);
}

function setCountBadge(cart) {
  const count = cart.reduce((s, i) => s + i.quantity, 0);
  const badge = document.getElementById("cart-count");
  if (badge) badge.textContent = count;
}

// ====== RENDER GIỎ HÀNG ======
function renderCart() {
  const container = document.getElementById("cart-items");
  const subtotalEl = document.getElementById("subtotal");
  const totalEl = document.getElementById("cart-total");
  const cart = loadCart();

  setCountBadge(cart);

  if (!cart.length) {
    container.innerHTML = `
      <div class="empty-state">
        <div class="mb-2"><i class="bi bi-bag-x fs-1 text-secondary"></i></div>
        <h5>Giỏ hàng trống</h5>
        <p class="text-secondary mb-0">Hãy thêm vài món ngon để tiếp tục nhé!</p>
      </div>
    `;
    if (subtotalEl) subtotalEl.textContent = fmt(0);
    if (totalEl) totalEl.textContent = fmt(0);
    return;
  }

  const rows = cart
    .map(
      (item) => `
      <tr data-id="${item.id}">
        <td class="align-middle">
          <img src="${item.imageUrl || "assets/img/placeholder.png"}" alt="${escapeHtml(
        item.name
      )}" class="cart-thumb" />
        </td>
        <td class="align-middle">
          <div class="fw-semibold">${escapeHtml(item.name)}</div>
          <div class="text-secondary small">Mã: ${escapeHtml(item.sku || String(item.id))}</div>
        </td>
        <td class="align-middle">${fmt(item.price)}</td>
        <td class="align-middle">
          <div class="input-group input-group-sm" style="max-width: 140px;">
            <button class="btn btn-outline-secondary btn-qty" data-action="dec" type="button"><i class="bi bi-dash"></i></button>
            <input type="number" class="form-control qty-input" min="1" max="99" value="${item.quantity}" />
            <button class="btn btn-outline-secondary btn-qty" data-action="inc" type="button"><i class="bi bi-plus"></i></button>
          </div>
        </td>
        <td class="align-middle fw-semibold">${fmt(item.price * item.quantity)}</td>
        <td class="align-middle text-end">
          <button class="btn btn-sm btn-outline-danger btn-remove"><i class="bi bi-trash"></i></button>
        </td>
      </tr>`
    )
    .join("");

  container.innerHTML = `
    <div class="card shadow-sm">
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table align-middle mb-0">
            <thead class="table-light">
              <tr>
                <th style="width:80px;">Ảnh</th>
                <th>Sản phẩm</th>
                <th style="width:140px;">Đơn giá</th>
                <th style="width:160px;">Số lượng</th>
                <th style="width:160px;">Thành tiền</th>
                <th style="width:80px;" class="text-end">Thao tác</th>
              </tr>
            </thead>
            <tbody>${rows}</tbody>
          </table>
        </div>
      </div>
    </div>
  `;

  if (subtotalEl) subtotalEl.textContent = fmt(getSubtotal(cart));
  if (totalEl) totalEl.textContent = fmt(getSubtotal(cart));
}

// ====== SỰ KIỆN: BẢNG GIỎ HÀNG ======
document.addEventListener("click", (e) => {
  const container = document.getElementById("cart-items");
  if (!container.contains(e.target)) return;

  const row = e.target.closest("tr[data-id]");
  const id = row ? row.getAttribute("data-id") : null;
  let cart = loadCart();

  // Xóa 1 dòng
  if (e.target.closest(".btn-remove") && id) {
    cart = cart.filter((x) => String(x.id) !== String(id));
    saveCart(cart);
    renderCart();
    return;
  }

  // Nút +/- số lượng
  const btnQty = e.target.closest(".btn-qty");
  if (btnQty && id) {
    const action = btnQty.getAttribute("data-action");
    const item = cart.find((x) => String(x.id) === String(id));
    if (!item) return;

    if (action === "inc") item.quantity = Math.min(99, item.quantity + 1);
    if (action === "dec") item.quantity = Math.max(1, item.quantity - 1);

    saveCart(cart);
    renderCart();
  }
});

// Sửa số lượng trực tiếp trong input
document.addEventListener("change", (e) => {
  if (!e.target.classList.contains("qty-input")) return;
  const row = e.target.closest("tr[data-id]");
  if (!row) return;

  const id = row.getAttribute("data-id");
  const val = Math.max(1, Math.min(99, parseInt(e.target.value || "1", 10)));
  let cart = loadCart();
  const item = cart.find((x) => String(x.id) === String(id));
  if (!item) return;

  item.quantity = val;
  saveCart(cart);
  renderCart();
});

// ====== NÚT XÓA TOÀN BỘ ======
document.addEventListener("click", (e) => {
  if (!e.target.closest("#clear-cart-btn")) return;
  if (!confirm("Bạn có chắc muốn xóa toàn bộ giỏ hàng?")) return;
  saveCart([]);
  renderCart();
});

// ====== ĐẶT HÀNG (GIẢ LẬP) ======
document.addEventListener("click", async (e) => {
  if (!e.target.closest("#checkout-btn")) return;

  const cart = loadCart();
  const messageBox = document.getElementById("order-message");
  const note = document.getElementById("note")?.value?.trim() || "";

  if (cart.length === 0) {
    messageBox.innerHTML = `<div class="alert alert-warning mb-0">Giỏ hàng trống.</div>`;
    return;
  }

  // Chuẩn bị payload
  const payload = {
    items: cart.map((c) => ({
      foodId: c.id,
      name: c.name,
      quantity: c.quantity,
      price: c.price,
    })),
    totalPrice: getSubtotal(cart),
    note,
    createdAt: new Date().toISOString(),
  };

  // Nếu có API thật, dùng fetch:
  // try {
  //   const res = await fetch("/api/orders", {
  //     method: "POST",
  //     headers: { "Content-Type": "application/json" },
  //     body: JSON.stringify(payload),
  //   });
  //   if (!res.ok) throw new Error("Tạo đơn hàng thất bại");
  // } catch (err) { ... }

  // Giả lập gọi API
  messageBox.innerHTML = `<div class="alert alert-info mb-0">Đang xử lý đơn hàng...</div>`;
  await new Promise((r) => setTimeout(r, 800));

  // Thành công
  saveCart([]);
  renderCart();
  messageBox.innerHTML = `
    <div class="alert alert-success mb-0">
      ✅ Đặt hàng thành công! Tổng thanh toán: <strong>${fmt(payload.totalPrice)}</strong>.<br/>
      Mã đơn tạm thời: <code>ORD-${Math.floor(Math.random() * 1e6)
        .toString()
        .padStart(6, "0")}</code>
    </div>
  `;
});

// ====== KHỞI TẠO ======
(function init() {
  seedIfEmpty();   // <— Tạo dữ liệu ảo lần đầu
  renderCart();
})();
