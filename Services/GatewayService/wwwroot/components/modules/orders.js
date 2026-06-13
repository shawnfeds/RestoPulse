/* ============================================================
   RestoPulse — Orders Module
   modules/orders.js
   ============================================================
   GET  /api/orders?status=&tableId=&date=
   Response: [{ "id":"ORD-0091", "tableNo":7, "tableId":2, "status":"New|Preparing|Served|Billed|Void",
                "createdAt":"ISO", "items":[{ "id":1,"name":"Butter Chicken","qty":2,"price":320,"notes":"Less spicy" }],
                "subtotal":640, "tax":115.2, "total":755.2, "staffName":"Priya S." }]

   GET  /api/orders/:id

   POST /api/orders
   Body: { "tableId":int, "items":[{"menuItemId":int,"qty":int,"notes":string}], "staffId":int }
   Response: { ...order object }

   POST /api/orders/:id/items
   Body: { "menuItemId":int, "qty":int, "notes":string }

   PUT  /api/orders/:id/items/:itemId
   Body: { "qty":int, "notes":string }

   DELETE /api/orders/:id/items/:itemId

   PATCH /api/orders/:id/status
   Body: { "status":string }

   PATCH /api/orders/:id/void
   ============================================================ */

Router.register('orders', async () => {
  const container = document.getElementById('page-orders');
  container.innerHTML = `
    <div style="display:flex;height:100%;overflow:hidden">
      <!-- Left: order list -->
      <div style="width:380px;flex-shrink:0;border-right:1px solid var(--border-subtle);display:flex;flex-direction:column">
        <div style="padding:12px 14px;border-bottom:1px solid var(--border-subtle);background:var(--bg-surface)">
          <div style="display:flex;gap:6px;margin-bottom:10px">
            ${['All','New','Preparing','Served','Billed'].map(s =>
              `<button class="btn btn-ghost btn-sm order-filter-btn ${s==='All'?'active-filter':''}" onclick="filterOrders('${s}',this)">${s}</button>`
            ).join('')}
          </div>
          <input class="form-input" placeholder="Search orders…" oninput="searchOrders(this.value)"
            style="height:34px;font-size:12px">
        </div>
        <div id="order-list" style="flex:1;overflow-y:auto"></div>
        <div style="padding:10px 14px;border-top:1px solid var(--border-subtle)">
          <button class="btn btn-primary w-full" onclick="openNewOrderModal()">+ New Order</button>
        </div>
      </div>
      <!-- Right: order detail -->
      <div id="order-detail" style="flex:1;overflow-y:auto;padding:24px">
        <div class="empty-state" style="height:100%;justify-content:center">
          <div class="empty-icon">🧾</div>
          <p>Select an order to view details</p>
        </div>
      </div>
    </div>
    ${newOrderModalHTML()}`;

  window._ordersData      = [];
  window._ordersFiltered  = [];
  window._ordersFilter    = 'All';
  window._ordersSearch    = '';
  window._selectedOrderId = null;

  await loadOrders();
});

async function loadOrders() {
  let data;
  try { data = await API.ordersList({}); }
  catch { data = MOCK_ORDERS.list(); }
  window._ordersData     = data;
  window._ordersFiltered = data;
  renderOrderList(data);
  if (typeof updateGlobalBadges === 'function') {
    updateGlobalBadges();
  } else {
    updateBadge('orders', data.filter(o => o.status === 'New').length);
  }
}

/* ── List ─────────────────────────────────────────────────── */
function renderOrderList(orders) {
  const el = document.getElementById('order-list');
  if (!orders.length) { el.innerHTML = '<div class="empty-state"><p>No orders</p></div>'; return; }
  const statusColors = { New:'blue', Preparing:'amber', Served:'green', Billed:'purple', Void:'gray' };
  el.innerHTML = orders.map(o => `
    <div class="order-list-item ${o.id == window._selectedOrderId || o.orderNo == window._selectedOrderId ? 'selected-order' : ''}"
      onclick="selectOrder('${o.orderNo || o.id}')"
      style="padding:12px 14px;border-bottom:1px solid var(--border-subtle);cursor:pointer;
             transition:background var(--transition);background:${(o.id==window._selectedOrderId || o.orderNo == window._selectedOrderId)?'var(--bg-raised)':'transparent'}">
      <div class="flex items-center justify-between mb-1">
        <span style="font-family:var(--font-mono);font-size:12px;color:var(--text-secondary)">${o.orderNo || o.id}</span>
        <span class="badge badge-${statusColors[o.status]||'gray'}">${o.status}</span>
      </div>
      <div class="flex items-center justify-between">
        <span style="font-weight:500">Table ${o.tableNo}</span>
        <span style="font-weight:600;color:var(--rp-brand)">${Fmt.currency(o.total)}</span>
      </div>
      <div class="flex items-center justify-between mt-1">
        <span class="text-muted text-sm">${o.items.length} items · ${o.staffName}</span>
        <span class="text-muted text-sm">${Fmt.elapsed(o.createdAt)}</span>
      </div>
    </div>`).join('');
}

/* ── Filter & Search ──────────────────────────────────────── */
window.filterOrders = (status, btn) => {
  window._ordersFilter = status;
  document.querySelectorAll('.order-filter-btn').forEach(b => b.classList.remove('active-filter'));
  btn.classList.add('active-filter');
  applyOrderFilters();
};
window.searchOrders = (q) => { window._ordersSearch = q.toLowerCase(); applyOrderFilters(); };
function applyOrderFilters() {
  let d = window._ordersData;
  if (window._ordersFilter !== 'All') d = d.filter(o => o.status === window._ordersFilter);
  if (window._ordersSearch) d = d.filter(o =>
    (o.orderNo || o.id.toString()).toLowerCase().includes(window._ordersSearch) ||
    String(o.tableNo).includes(window._ordersSearch)
  );
  window._ordersFiltered = d;
  renderOrderList(d);
}

/* ── Detail view ──────────────────────────────────────────── */
window.selectOrder = (id) => {
  window._selectedOrderId = id;
  applyOrderFilters();
  const o = window._ordersData.find(x => x.id == id || x.orderNo == id);
  if (!o) return;
  const statusActions = {
    New:       ['Preparing', 'Void'],
    Preparing: ['Served',    'Void'],
    Served:    ['Billed'],
    Billed:    [],
    Void:      [],
  };
  const actions = (statusActions[o.status] || []).map(s =>
    `<button class="btn btn-secondary btn-sm" onclick="setOrderStatus('${o.orderNo || o.id}','${s}')">→ ${s}</button>`
  ).join('');

  document.getElementById('order-detail').innerHTML = `
    <div style="max-width:640px">
      <div class="flex items-center justify-between mb-4">
        <div>
          <div class="flex items-center gap-3">
            <span style="font-size:20px;font-weight:700">${o.orderNo || o.id}</span>
            <span class="badge badge-blue">Table ${o.tableNo}</span>
          </div>
          <div class="text-muted text-sm mt-1">${Fmt.datetime(o.createdAt)} · ${o.staffName}</div>
        </div>
        <div class="flex gap-2">${actions}</div>
      </div>

      <div class="card mb-3">
        <div class="card-header">
          <span class="card-title">Items</span>
          <button class="btn btn-secondary btn-sm" onclick="openAddItemModal('${o.id}')">+ Add Item</button>
        </div>
        <table class="rp-table">
          <thead><tr><th>Item</th><th>Notes</th><th>Qty</th><th>Price</th><th>Total</th><th></th></tr></thead>
          <tbody>
            ${o.items.map(i => `
              <tr>
                <td style="font-weight:500">${i.name}</td>
                <td class="text-muted text-sm">${i.notes || '—'}</td>
                <td>
                  <div class="flex items-center gap-2">
                    <button class="btn btn-ghost btn-sm btn-icon" onclick="changeItemQty('${o.id}',${i.id},-1,${i.qty})">−</button>
                    <span style="font-weight:500;min-width:20px;text-align:center">${i.qty}</span>
                    <button class="btn btn-ghost btn-sm btn-icon" onclick="changeItemQty('${o.id}',${i.id},1,${i.qty})">+</button>
                  </div>
                </td>
                <td>${Fmt.currency(i.price)}</td>
                <td style="font-weight:600">${Fmt.currency(i.price * i.qty)}</td>
                <td>
                  ${o.status !== 'Billed' && o.status !== 'Void' ?
                    `<button class="btn btn-danger btn-sm btn-icon" onclick="removeOrderItem('${o.id}',${i.id})">✕</button>` : ''}
                </td>
              </tr>`).join('')}
          </tbody>
        </table>
      </div>

      <div class="card">
        <div class="card-body">
          <div class="flex justify-between mb-2"><span class="text-muted">Subtotal</span><span>${Fmt.currency(o.subtotal)}</span></div>
          <div class="flex justify-between mb-2"><span class="text-muted">Tax (18% GST)</span><span>${Fmt.currency(o.tax)}</span></div>
          <div class="divider"></div>
          <div class="flex justify-between">
            <span style="font-weight:700;font-size:16px">Total</span>
            <span style="font-weight:700;font-size:16px;color:var(--rp-brand)">${Fmt.currency(o.total)}</span>
          </div>
          ${o.status === 'Served' ? `
            <div class="mt-3">
              <button class="btn btn-primary w-full" onclick="Router.navigate('billing')">Generate Bill</button>
            </div>` : ''}
        </div>
      </div>
    </div>`;

  // Add item modal (lazy)
  if (!document.getElementById('modal-add-item')) {
    document.getElementById('page-orders').insertAdjacentHTML('beforeend', addItemModalHTML());
  }
};

/* ── Order actions ────────────────────────────────────────── */
window.setOrderStatus = async (id, status) => {
  try {
    await API.orderSetStatus(id, status);
    const o = window._ordersData.find(x => x.id == id || x.orderNo == id);
    if (o) o.status = status;
    selectOrder(id);
    applyOrderFilters();
    if (typeof updateGlobalBadges === 'function') updateGlobalBadges();
    Toast.success(`Order ${id} → ${status}`);
  } catch { }
};

window.changeItemQty = async (orderId, itemId, delta, currentQty) => {
  const newQty = currentQty + delta;
  if (newQty < 1) { removeOrderItem(orderId, itemId); return; }
  try {
    await API.orderUpdateItem(orderId, itemId, { qty: newQty });
    const o = window._ordersData.find(x => x.id == orderId || x.orderNo == orderId);
    if (o) {
      const item = o.items.find(i => i.id === itemId);
      if (item) {
        item.qty = newQty;
        o.subtotal = o.items.reduce((s,i) => s + i.price*i.qty, 0);
        o.tax   = o.subtotal * 0.18;
        o.total = o.subtotal + o.tax;
      }
    }
    selectOrder(orderId);
    applyOrderFilters();
  } catch { }
};

window.removeOrderItem = async (orderId, itemId) => {
  if (!confirm('Remove this item?')) return;
  try {
    await API.orderRemoveItem(orderId, itemId);
    const o = window._ordersData.find(x => x.id == orderId || x.orderNo == orderId);
    if (o) {
      o.items = o.items.filter(i => i.id !== itemId);
      o.subtotal = o.items.reduce((s,i) => s + i.price*i.qty, 0);
      o.tax   = o.subtotal * 0.18;
      o.total = o.subtotal + o.tax;
    }
    selectOrder(orderId);
    Toast.success('Item removed');
  } catch { }
};

window.openAddItemModal = async (orderId) => {
  window._currentOrderId = orderId;
  document.getElementById('add-item-order-id').textContent = orderId;
  let menuItems;
  try { menuItems = await API.menuItems(); }
  catch { menuItems = MOCK_ORDERS.menuItems(); }
  const sel = document.getElementById('new-item-select');
  sel.innerHTML = menuItems.map(i => `<option value="${i.id}" data-price="${i.price}">${i.name} — ${Fmt.currency(i.price)}</option>`).join('');
  Modal.open('modal-add-item');
};

window.submitAddItem = async () => {
  const menuItemId = parseInt(document.getElementById('new-item-select').value);
  const qty        = parseInt(document.getElementById('new-item-qty').value) || 1;
  const notes      = document.getElementById('new-item-notes').value;
  const orderId    = window._currentOrderId;
  try {
    const res = await API.orderAddItem(orderId, { menuItemId, qty, notes });
    Modal.close('modal-add-item');
    await loadOrders();
    selectOrder(orderId);
    Toast.success('Item added');
  } catch { }
};

/* ── New order modal ──────────────────────────────────────── */
window.openNewOrderModal = async () => {
  let tables;
  try { tables = await API.tablesList(); }
  catch { tables = MOCK_ORDERS.tables(); }
  const avail = tables.filter(t => t.status === 'Available' || t.status === 'Occupied');
  document.getElementById('new-order-table').innerHTML = avail.map(t =>
    `<option value="${t.id}">Table ${t.tableNo} — ${t.section} (${t.status})</option>`
  ).join('');
  Modal.open('modal-new-order');
};

window.submitNewOrder = async () => {
  const tableId = parseInt(document.getElementById('new-order-table').value);
  try {
    const res = await API.orderCreate({ tableId, items: [], staffId: 1 });
    Modal.close('modal-new-order');
    await loadOrders();
    selectOrder(res.id);
    Toast.success(`Order ${res.id} created`);
  } catch { }
};

/* ── Modal HTML ───────────────────────────────────────────── */
function newOrderModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-new-order">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">New Order</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-new-order')">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label">Select Table</label>
            <select class="form-select" id="new-order-table"></select>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" onclick="Modal.close('modal-new-order')">Cancel</button>
          <button class="btn btn-primary" onclick="submitNewOrder()">Create Order</button>
        </div>
      </div>
    </div>`;
}

function addItemModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-add-item">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">Add Item — <span id="add-item-order-id"></span></span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-add-item')">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group mb-3">
            <label class="form-label">Menu Item</label>
            <select class="form-select" id="new-item-select"></select>
          </div>
          <div class="grid-2 mb-3">
            <div class="form-group"><label class="form-label">Qty</label>
              <input class="form-input" type="number" id="new-item-qty" value="1" min="1"></div>
            <div class="form-group"><label class="form-label">Notes</label>
              <input class="form-input" id="new-item-notes" placeholder="e.g. Less spicy"></div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" onclick="Modal.close('modal-add-item')">Cancel</button>
          <button class="btn btn-primary" onclick="submitAddItem()">Add Item</button>
        </div>
      </div>
    </div>`;
}

/* ── Mock data ────────────────────────────────────────────── */
const MOCK_ORDERS = {
  menuItems: () => [
    {id:1,name:'Butter Chicken',price:320},{id:2,name:'Dal Makhani',price:220},
    {id:3,name:'Paneer Tikka',price:280},{id:4,name:'Biryani',price:380},
    {id:5,name:'Naan',price:60},{id:6,name:'Lassi',price:80},
    {id:7,name:'Gulab Jamun',price:120},{id:8,name:'Mango Kulfi',price:150}
  ],
  tables: () => [
    {id:1,tableNo:1,section:'Bar',status:'Available'},
    {id:6,tableNo:6,section:'Terrace',status:'Available'},
    {id:8,tableNo:8,section:'Private Dining',status:'Available'},
  ],
  list: () => {
    const now = new Date();
    const t = (m) => new Date(now - m*60000).toISOString();
    return [
      { id:'ORD-0091', tableNo:7,  tableId:7,  status:'Served',    createdAt:t(40),  staffName:'Priya S.',
        items:[{id:1,name:'Butter Chicken',qty:2,price:320,notes:'Less spicy'},{id:2,name:'Naan',qty:4,price:60,notes:''},{id:3,name:'Lassi',qty:2,price:80,notes:''}],
        subtotal:1040, tax:187.2, total:1227.2 },
      { id:'ORD-0090', tableNo:3,  tableId:3,  status:'Preparing', createdAt:t(14),  staffName:'Rahul M.',
        items:[{id:4,name:'Biryani',qty:1,price:380,notes:'Extra raita'},{id:5,name:'Dal Makhani',qty:1,price:220,notes:''}],
        subtotal:600, tax:108, total:708 },
      { id:'ORD-0089', tableNo:12, tableId:12, status:'Billed',    createdAt:t(90),  staffName:'Rahul M.',
        items:[{id:6,name:'Paneer Tikka',qty:2,price:280,notes:''},{id:7,name:'Butter Chicken',qty:3,price:320,notes:''},{id:8,name:'Naan',qty:6,price:60,notes:''}],
        subtotal:1880, tax:338.4, total:2218.4 },
      { id:'ORD-0088', tableNo:5,  tableId:5,  status:'Served',    createdAt:t(55),  staffName:'Priya S.',
        items:[{id:9,name:'Gulab Jamun',qty:2,price:120,notes:''},{id:10,name:'Mango Kulfi',qty:2,price:150,notes:''}],
        subtotal:540, tax:97.2, total:637.2 },
      { id:'ORD-0087', tableNo:9,  tableId:9,  status:'New',       createdAt:t(4),   staffName:'Anita K.',
        items:[{id:11,name:'Biryani',qty:2,price:380,notes:'No onion'},{id:12,name:'Lassi',qty:2,price:80,notes:''}],
        subtotal:920, tax:165.6, total:1085.6 },
    ];
  }
};
