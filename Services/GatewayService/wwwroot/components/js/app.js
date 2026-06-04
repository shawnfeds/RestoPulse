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
  usersList:          ()     => API.get('/users'),
  userCreate:         (body) => API.post('/users', body),
};

/* ── Global State ────────────────────────────────────────── */
const State = {
  token:        localStorage.getItem('rp_token') || null,
  role:         localStorage.getItem('rp_role') || null,
  fullName:     localStorage.getItem('rp_fullname') || null,
  username:     localStorage.getItem('rp_username') || null,
  currentPage:  'dashboard',
  outlet:       { name: 'The Grand Table' },
  orderBadge:   0,
  kitchenBadge: 0,
};

const RolePages = {
  Owner:   ['dashboard', 'tables', 'orders', 'kitchen', 'billing', 'menu', 'inventory', 'users', 'reports'],
  Manager: ['dashboard', 'tables', 'orders', 'kitchen', 'billing', 'menu', 'inventory', 'users', 'reports'],
  Chef:    ['kitchen', 'menu', 'inventory'],
  Server:  ['tables', 'orders', 'billing']
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
    if (!State.token) {
      document.getElementById('login-overlay').style.display = 'flex';
      return;
    }

    const allowedPages = RolePages[State.role] || [];
    if (!allowedPages.includes(id)) {
      if (allowedPages.length > 0) {
        Router.navigate(allowedPages[0]);
      }
      return;
    }

    // Close sidebar drawer if open on mobile
    document.body.classList.remove('sidebar-open');

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
      users:      { title: 'User Management',  sub: 'Manage roles and credentials' },
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

/* ── RBAC / UI Toggles ───────────────────────────────────── */
function initUserView() {
  if (!State.token) {
    document.getElementById('login-overlay').style.display = 'flex';
    return;
  }
  
  document.getElementById('login-overlay').style.display = 'none';

  const nameEl = document.getElementById('user-fullname');
  const roleEl = document.getElementById('user-role');
  const avatarEl = document.getElementById('user-avatar');
  const topbarAvatarEl = document.getElementById('topbar-avatar');

  if (nameEl) nameEl.textContent = State.fullName || 'User';
  if (roleEl) roleEl.textContent = State.role || 'Role';
  if (avatarEl) {
    avatarEl.textContent = (State.fullName || 'U').substring(0, 1).toUpperCase();
  }
  if (topbarAvatarEl) {
    topbarAvatarEl.textContent = (State.fullName || 'U').substring(0, 1).toUpperCase();
  }

  const allowedPages = RolePages[State.role] || [];
  document.querySelectorAll('.nav-item[data-page]').forEach(el => {
    const pageId = el.dataset.page;
    if (allowedPages.includes(pageId)) {
      el.style.display = 'flex';
    } else {
      el.style.display = 'none';
    }
  });

  // Handle section labels: if all items under a label are hidden, hide the label
  const overviewLabel = document.querySelector('.nav-section-label:nth-of-type(1)');
  const overviewItems = ['dashboard'];
  toggleSectionLabel(overviewLabel, overviewItems, allowedPages);

  const opsLabel = document.querySelector('.nav-section-label:nth-of-type(2)');
  const opsItems = ['tables', 'orders', 'kitchen', 'billing'];
  toggleSectionLabel(opsLabel, opsItems, allowedPages);

  const mgmtLabel = document.querySelector('.nav-section-label:nth-of-type(3)');
  const mgmtItems = ['menu', 'inventory', 'users', 'reports'];
  toggleSectionLabel(mgmtLabel, mgmtItems, allowedPages);

  // Navigate to current or first allowed page
  if (allowedPages.length > 0) {
    if (allowedPages.includes(State.currentPage)) {
      Router.navigate(State.currentPage);
    } else {
      Router.navigate(allowedPages[0]);
    }
  }
}

function toggleSectionLabel(labelEl, items, allowedPages) {
  if (!labelEl) return;
  const anyVisible = items.some(item => allowedPages.includes(item));
  labelEl.style.display = anyVisible ? 'block' : 'none';
}

/* ── Init ────────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
  // Wire nav clicks
  document.querySelectorAll('.nav-item[data-page]').forEach(el => {
    el.addEventListener('click', () => Router.navigate(el.dataset.page));
  });

  // Wire sidebar mobile toggles
  const toggleBtn = document.getElementById('sidebar-toggle');
  const overlay = document.getElementById('sidebar-overlay');
  
  if (toggleBtn) {
    toggleBtn.addEventListener('click', () => {
      document.body.classList.toggle('sidebar-open');
    });
  }
  
  if (overlay) {
    overlay.addEventListener('click', () => {
      document.body.classList.remove('sidebar-open');
    });
  }

  // Wire Login Form
  const loginForm = document.getElementById('login-form');
  if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      const usernameInput = document.getElementById('login-username');
      const passwordInput = document.getElementById('login-password');
      const username = usernameInput.value.trim();
      const password = passwordInput.value;

      try {
        const response = await API.post('/users/login', { username, password });
        if (response && response.token) {
          State.token = response.token;
          State.role = response.role;
          State.fullName = response.fullName;
          State.username = response.username;

          localStorage.setItem('rp_token', response.token);
          localStorage.setItem('rp_role', response.role);
          localStorage.setItem('rp_fullname', response.fullName);
          localStorage.setItem('rp_username', response.username);

          usernameInput.value = '';
          passwordInput.value = '';

          initUserView();
          Toast.success(`Welcome back, ${response.fullName}!`);
        } else {
          Toast.error('Invalid server response');
        }
      } catch (err) {
        Toast.error('Login failed. Please check your credentials.');
      }
    });
  }

  // Wire Logout Button
  const logoutBtn = document.getElementById('btn-logout');
  if (logoutBtn) {
    logoutBtn.addEventListener('click', () => {
      State.token = null;
      State.role = null;
      State.fullName = null;
      State.username = null;

      localStorage.removeItem('rp_token');
      localStorage.removeItem('rp_role');
      localStorage.removeItem('rp_fullname');
      localStorage.removeItem('rp_username');

      document.getElementById('login-overlay').style.display = 'flex';
      Toast.info('Signed out successfully.');
    });
  }

  // Boot UI
  if (State.token) {
    initUserView();
  } else {
    document.getElementById('login-overlay').style.display = 'flex';
  }
});
