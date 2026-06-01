/* ============================================================
   RestoPulse — Core App
   app.js  |  Router · API Client · Toast · Global State
   ============================================================ */

/* ── API Base URL ────────────────────────────────────────── */
const API = {
  BASE: '/api',

  /* ── Generic request wrapper ──────────────────────────── */
  async request(method, endpoint, body = null) {
    const opts = {
      method,
      headers: {
        'Content-Type': 'application/json'
      }
    };
    // Only send Authorization header when a real token is present
    if (State.token && State.token !== 'demo-token') {
      opts.headers['Authorization'] = `Bearer ${State.token}`;
    }
    if (body) opts.body = JSON.stringify(body);
    try {
      const res = await fetch(this.BASE + endpoint, opts);
      if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.message || `HTTP ${res.status}`);
      }
      if (res.status === 204) return null;
      return await res.json();
    } catch (e) {
      Toast.error(e.message || 'Network error');
      throw e;
    }
  },

  get   (ep)       { return this.request('GET',    ep); },
  post  (ep, body) { return this.request('POST',   ep, body); },
  put   (ep, body) { return this.request('PUT',    ep, body); },
  patch (ep, body) { return this.request('PATCH',  ep, body); },
  del   (ep)       { return this.request('DELETE', ep); },

  /* ── Endpoint catalogue ────────────────────────────────
     All endpoints used by the frontend, grouped by module.
     Backend team: implement these routes.
  ────────────────────────────────────────────────────── */

  // Dashboard
  dashboardSummary:   ()     => API.get('/dashboard/summary'),

  // Tables
  tablesList:         ()     => API.get('/tables'),
  tableGet:           (id)   => API.get(`/tables/${id}`),
  tableCreate:        (body) => API.post('/tables', body),
  tableUpdate:        (id,b) => API.put(`/tables/${id}`, b),
  tableDelete:        (id)   => API.del(`/tables/${id}`),
  tableSetStatus:     (id,s) => API.patch(`/tables/${id}/status`, { status: s }),

  // Orders
  ordersList:         (q)    => API.get(`/orders?${new URLSearchParams(q)}`),
  orderGet:           (id)   => API.get(`/orders/${id}`),
  orderCreate:        (body) => API.post('/orders', body),
  orderAddItem:       (id,b) => API.post(`/orders/${id}/items`, b),
  orderUpdateItem:    (oid,iid,b) => API.put(`/orders/${oid}/items/${iid}`, b),
  orderRemoveItem:    (oid,iid)   => API.del(`/orders/${oid}/items/${iid}`),
  orderSetStatus:     (id,s) => API.patch(`/orders/${id}/status`, { status: s }),
  orderVoid:          (id)   => API.patch(`/orders/${id}/void`, {}),

  // Kitchen
  kitchenQueue:       ()     => API.get('/kitchen/queue'),
  kitchenItemStatus:  (id,s) => API.patch(`/kitchen/items/${id}/status`, { status: s }),
  kitchenItemBump:    (id)   => API.post(`/kitchen/items/${id}/bump`, {}),

  // Menu
  menuCategories:     ()     => API.get('/menu/categories'),
  menuItems:          (cat)  => API.get(`/menu/items?categoryId=${cat||''}`),
  menuItemCreate:     (body) => API.post('/menu/items', body),
  menuItemUpdate:     (id,b) => API.put(`/menu/items/${id}`, b),
  menuItemToggle:     (id)   => API.patch(`/menu/items/${id}/toggle`, {}),

  // Billing
  billCreate:         (body) => API.post('/bills', body),
  billGet:            (id)   => API.get(`/bills/${id}`),
  billSettle:         (id,b) => API.post(`/bills/${id}/settle`, b),
  billSplit:          (id,b) => API.post(`/bills/${id}/split`, b),
  billsList:          (q)    => API.get(`/bills?${new URLSearchParams(q)}`),

  // Inventory
  inventoryList:      ()     => API.get('/inventory'),
  inventoryItem:      (id)   => API.get(`/inventory/${id}`),
  inventoryAdjust:    (id,b) => API.post(`/inventory/${id}/adjust`, b),
  inventoryLowStock:  ()     => API.get('/inventory/low-stock'),

  // Reports
  reportsRevenue:     (q)    => API.get(`/reports/revenue?${new URLSearchParams(q)}`),
  reportsTopItems:    (q)    => API.get(`/reports/top-items?${new URLSearchParams(q)}`),
};

/* ── Global State ────────────────────────────────────────── */
const State = {
  // Do not default to a fake token; keep null when not authenticated
  token:        localStorage.getItem('rp_token') || null,
  currentPage:  'dashboard',
  outlet:       { name: 'The Grand Table', role: 'Manager' },
  orderBadge:   0,
  kitchenBadge: 0,
};

/* ── Toast ───────────────────────────────────────────────── */
const Toast = {
  show(msg, type = 'info', duration = 3000) {
    const icon = { success:'✓', error:'✕', info:'ℹ' }[type] || 'ℹ';
    const el = document.createElement('div');
    el.className = `toast ${type}`;
    el.innerHTML = `<span style="font-weight:600">${icon}</span><span>${msg}</span>`;
    document.getElementById('toast-container').appendChild(el);
    setTimeout(() => el.remove(), duration);
  },
  success: (m) => Toast.show(m, 'success'),
  error:   (m) => Toast.show(m, 'error', 4000),
  info:    (m) => Toast.show(m, 'info'),
};

/* ── Router ──────────────────────────────────────────────── */
const Router = {
  pages: {},

  register(id, initFn) {
    this.pages[id] = initFn;
  },

  navigate(id) {
    // Hide all pages
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));

    // Show target
    const page = document.getElementById(`page-${id}`);
    if (page) page.classList.add('active');

    const nav = document.querySelector(`[data-page="${id}"]`);
    if (nav) nav.classList.add('active');

    // Update topbar
    const titles = {
      dashboard:  { title: 'Dashboard',       sub: 'Live overview' },
      tables:     { title: 'Table Management', sub: 'Floor plan & status' },
      orders:     { title: 'Orders',           sub: 'Active & recent orders' },
      kitchen:    { title: 'Kitchen Display',  sub: 'Live order queue' },
      billing:    { title: 'Billing & Invoices', sub: 'Settlements & receipts' },
      menu:       { title: 'Menu Manager',     sub: 'Items, categories & pricing' },
      inventory:  { title: 'Inventory',        sub: 'Stock levels & adjustments' },
      reports:    { title: 'Reports',          sub: 'Revenue & analytics' },
    };
    const t = titles[id] || { title: id, sub: '' };
    document.getElementById('topbar-title').textContent = t.title;
    document.getElementById('topbar-sub').textContent   = t.sub;

    State.currentPage = id;

    // Init page
    if (this.pages[id]) this.pages[id]();
  }
};

/* ── Helpers ─────────────────────────────────────────────── */
const Fmt = {
  currency: (n) => '₹' + Number(n).toFixed(2),
  number:   (n) => Number(n).toLocaleString('en-IN'),
  time:     (d) => new Date(d).toLocaleTimeString('en-IN', { hour:'2-digit', minute:'2-digit' }),
  date:     (d) => new Date(d).toLocaleDateString('en-IN', { day:'2-digit', month:'short', year:'numeric' }),
  datetime: (d) => `${Fmt.date(d)}, ${Fmt.time(d)}`,
  elapsed:  (d) => {
    const mins = Math.floor((Date.now() - new Date(d)) / 60000);
    if (mins < 1)  return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    return `${Math.floor(mins/60)}h ${mins%60}m ago`;
  },
  duration: (secs) => {
    const m = Math.floor(secs/60), s = secs%60;
    return `${m}:${String(s).padStart(2,'0')}`;
  }
};

/* ── Modal helper ────────────────────────────────────────── */
const Modal = {
  open(id)  { document.getElementById(id).classList.add('open'); },
  close(id) { document.getElementById(id).classList.remove('open'); },
  closeAll() {
    document.querySelectorAll('.modal-backdrop').forEach(m => m.classList.remove('open'));
  }
};

/* Close modal on backdrop click */
document.addEventListener('click', e => {
  if (e.target.classList.contains('modal-backdrop')) Modal.closeAll();
});

/* ── Badge updates ───────────────────────────────────────── */
function updateBadge(nav, count) {
  const item = document.querySelector(`[data-page="${nav}"]`);
  if (!item) return;
  let badge = item.querySelector('.nav-badge');
  if (count > 0) {
    if (!badge) { badge = document.createElement('span'); badge.className = 'nav-badge'; item.appendChild(badge); }
    badge.textContent = count;
  } else if (badge) badge.remove();
}

/* ── Init ────────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
  // Wire nav clicks
  document.querySelectorAll('.nav-item[data-page]').forEach(el => {
    el.addEventListener('click', () => Router.navigate(el.dataset.page));
  });

  // Boot to dashboard
  Router.navigate('dashboard');
});
