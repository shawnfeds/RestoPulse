/* ============================================================
   RestoPulse — Core App
   app.js  |  Router · API Client · Toast · Global State · Auth
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
    if (State.token) {
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

  /* ── Endpoint catalogue ──────────────────────────────── */

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

  // UserService (Authentication & Shifts)
  login:              (body) => API.post('/users/login', body),
  usersList:          ()     => API.get('/users'),
  userCreate:         (body) => API.post('/users', body),
  userUpdate:         (id,b) => API.put(`/users/${id}`, b),
  userToggleStatus:   (id,a) => API.put(`/users/${id}/status?isActive=${a}`, {}),
  userChangePassword: (id,b) => API.put(`/users/${id}/password`, b),
  
  clockIn:            (notes) => API.post(`/users/clock-in?notes=${notes ? encodeURIComponent(notes) : ''}`, {}),
  clockOut:           ()      => API.post('/users/clock-out', {}),
  getClockStatus:     ()      => API.get('/users/clock-status'),
  getShiftTypes:      ()      => API.get('/users/shift-types'),
  getShiftsList:      (q)     => API.get(`/users/shifts?${new URLSearchParams(q)}`),
  setSchedule:        (body)  => API.post('/users/schedule', body),
  getSchedules:       (date)  => API.get(`/users/schedules?date=${date}`),
  userMonthlyReport:  (uid,m,y) => API.get(`/users/reports/monthly?targetUserId=${uid}&month=${m}&year=${y}`),
};

/* ── Global State ────────────────────────────────────────── */
const State = {
  token:        localStorage.getItem('rp_token') || null,
  user:         JSON.parse(localStorage.getItem('rp_user')) || null,
  currentPage:  'dashboard',
  outlet:       { name: 'The Grand Table', role: 'Manager' },
  orderBadge:   0,
  kitchenBadge: 0,
  activeShift:  null
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
    // Guard: if not logged in, show login screen and bail
    if (!State.user) {
      document.getElementById('app').style.display = 'none';
      document.getElementById('login-container').style.display = 'flex';
      return;
    }

    // Role-based access control — prevent infinite recursion by only
    // redirecting when the fallback is actually different from the target
    if (!checkPageAllowed(id)) {
      const fallback = getFallbackPage();
      if (fallback !== id) this.navigate(fallback);
      return;
    }

    // Hide all pages
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));

    // Show target
    const page = document.getElementById(`page-${id}`);
    if (page) page.classList.add('active');

    const nav = document.querySelector(`[data-page="${id}"]`);
    if (nav) nav.classList.add('active');

    // Close mobile drawer on navigation
    const sidebar = document.getElementById('sidebar');
    if (sidebar.classList.contains('open')) {
      toggleMobileSidebar();
    }

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
      users:      { title: 'Users & Shifts',   sub: 'Staff accounts & schedule' },
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

/* ── Authentication & Role Management ───────────────────── */

function checkPageAllowed(pageId) {
  if (!State.user) return false;
  const role = State.user.role;
  const permissions = {
    Owner:   ['dashboard', 'tables', 'orders', 'kitchen', 'billing', 'menu', 'inventory', 'reports', 'users'],
    Manager: ['dashboard', 'tables', 'orders', 'kitchen', 'billing', 'menu', 'inventory', 'users'],
    Chef:    ['kitchen', 'inventory'],
    Server:  ['tables', 'orders', 'menu']
  };
  const allowed = permissions[role] || [];
  return allowed.includes(pageId);
}

function getFallbackPage() {
  if (!State.user) return 'dashboard';
  const role = State.user.role;
  if (role === 'Chef') return 'kitchen';
  if (role === 'Server') return 'tables';
  return 'dashboard';
}

function applyRolePermissions() {
  if (!State.user) return;
  const role = State.user.role;
  
  // Show/Hide sidebar links
  document.querySelectorAll('.nav-item[data-page]').forEach(el => {
    const page = el.dataset.page;
    if (checkPageAllowed(page)) {
      el.style.display = 'flex';
    } else {
      el.style.display = 'none';
    }
  });

  // Toggle Nav Labels based on visibility
  const toggleSection = (lblId, pagesInSection) => {
    const lbl = document.getElementById(lblId);
    if (!lbl) return;
    const anyVisible = pagesInSection.some(p => checkPageAllowed(p));
    lbl.style.display = anyVisible ? 'block' : 'none';
  };

  toggleSection('lbl-overview', ['dashboard']);
  toggleSection('lbl-operations', ['tables', 'orders', 'kitchen', 'billing']);
  toggleSection('lbl-management', ['menu', 'inventory', 'reports']);
  toggleSection('lbl-staff', ['users']);
}

// ── Profile Dropdown UI ──────────────────────────────────────
window.toggleProfileMenu = (event) => {
  event.stopPropagation();
  const menu = document.getElementById('profile-dropdown');
  if (menu) menu.classList.toggle('open');
  const topbarMenu = document.getElementById('topbar-profile-dropdown');
  if (topbarMenu) topbarMenu.classList.remove('open');
};

window.toggleTopbarProfileMenu = (event) => {
  event.stopPropagation();
  const menu = document.getElementById('topbar-profile-dropdown');
  if (menu) menu.classList.toggle('open');
  const sidebarMenu = document.getElementById('profile-dropdown');
  if (sidebarMenu) sidebarMenu.classList.remove('open');
};

document.addEventListener('click', () => {
  const menu = document.getElementById('profile-dropdown');
  if (menu) menu.classList.remove('open');
  const topbarMenu = document.getElementById('topbar-profile-dropdown');
  if (topbarMenu) topbarMenu.classList.remove('open');
});

// ── Authentication Handlers ──────────────────────────────────
window.handleLoginSubmit = async (event) => {
  event.preventDefault();
  const userEl = document.getElementById('login-username');
  const passEl = document.getElementById('login-password');
  
  try {
    const res = await API.login({ username: userEl.value, password: passEl.value });
    
    // Set authentication state
    localStorage.setItem('rp_token', res.token);
    localStorage.setItem('rp_user', JSON.stringify(res.user));
    
    State.token = res.token;
    State.user = res.user;
    
    // Reset login form
    userEl.value = '';
    passEl.value = '';
    
    // Load dynamic UI
    bootAuthenticatedUser();
    Toast.success(`Welcome back, ${State.user.fullName}!`);
  } catch (e) {
    // Error is handled inside API.request toast
  }
};

window.handleLogout = (event) => {
  if (event) event.stopPropagation();
  localStorage.removeItem('rp_token');
  localStorage.removeItem('rp_user');
  State.token = null;
  State.user = null;
  State.activeShift = null;
  
  // Show login layout, hide app
  document.getElementById('app').style.display = 'none';
  document.getElementById('login-container').style.display = 'flex';
  
  // Close profile dropdowns
  const sidebarMenu = document.getElementById('profile-dropdown');
  if (sidebarMenu) sidebarMenu.classList.remove('open');
  const topbarMenu = document.getElementById('topbar-profile-dropdown');
  if (topbarMenu) topbarMenu.classList.remove('open');
  
  Toast.info('Logged out successfully');
};

// ── Change Password Handlers ────────────────────────────────
window.openChangePasswordModal = (event) => {
  if (event) event.stopPropagation();
  document.getElementById('change-password-form').reset();
  
  // Clean profile dropdowns
  const sidebarMenu = document.getElementById('profile-dropdown');
  if (sidebarMenu) sidebarMenu.classList.remove('open');
  const topbarMenu = document.getElementById('topbar-profile-dropdown');
  if (topbarMenu) topbarMenu.classList.remove('open');
  
  // Show current password field since this is self-service
  document.getElementById('group-current-password').style.display = 'flex';
  document.getElementById('pwd-current').required = true;
  
  // Bind ID to form submit
  document.getElementById('change-password-form').dataset.userId = State.user.id;
  
  Modal.open('modal-change-password');
};

window.submitChangePassword = async (event) => {
  event.preventDefault();
  const userId = parseInt(event.target.dataset.userId);
  const current = document.getElementById('pwd-current').value;
  const valNew = document.getElementById('pwd-new').value;
  const confirm = document.getElementById('pwd-confirm').value;
  
  if (valNew !== confirm) {
    Toast.error('Passwords do not match');
    return;
  }
  
  try {
    await API.userChangePassword(userId, { currentPassword: current || null, newPassword: valNew });
    Modal.close('modal-change-password');
    Toast.success('Password updated successfully');
  } catch (e) { }
};

// ── Clock Operations ─────────────────────────────────────────
window.checkClockStatus = async () => {
  if (!State.user) return;
  try {
    const res = await API.getClockStatus();
    State.activeShift = res.isClockedIn ? res.activeShift : null;
    updateClockUI();
  } catch (e) { }
};

function updateClockUI() {
  const btn = document.getElementById('btn-clock-in');
  const badge = document.getElementById('dropdown-clock-status');
  
  if (State.activeShift) {
    btn.textContent = '⏱ Clock Out';
    btn.className = 'dropdown-item warning-item'; // yellow/red accent for clock out
    badge.textContent = 'Clocked In';
    badge.className = 'clock-status-badge active';
  } else {
    btn.textContent = '⏱ Clock In';
    btn.className = 'dropdown-item';
    badge.textContent = 'Clocked Out';
    badge.className = 'clock-status-badge';
  }
}

window.handleClockInOutBtn = async (event) => {
  event.stopPropagation();
  const sidebarMenu = document.getElementById('profile-dropdown');
  if (sidebarMenu) sidebarMenu.classList.remove('open');
  const topbarMenu = document.getElementById('topbar-profile-dropdown');
  if (topbarMenu) topbarMenu.classList.remove('open');
  
  if (State.activeShift) {
    // Clock out
    if (!confirm('Are you sure you want to clock out?')) return;
    try {
      await API.clockOut();
      State.activeShift = null;
      updateClockUI();
      Toast.success('Clocked out successfully');
    } catch (e) { }
  } else {
    // Clock in
    const notes = prompt('Enter clock-in notes (optional):');
    if (notes === null) return; // Cancelled
    try {
      const shift = await API.clockIn(notes);
      State.activeShift = shift;
      updateClockUI();
      if (shift.isLate) {
        Toast.show('Clocked in! (Marked as late in)', 'info');
      } else {
        Toast.success('Clocked in successfully');
      }
    } catch (e) { }
  }
};

// ── Mobile Menu Toggles ──────────────────────────────────────
window.toggleMobileSidebar = () => {
  const sidebar = document.getElementById('sidebar');
  const overlay = document.getElementById('sidebar-overlay');
  
  sidebar.classList.toggle('open');
  overlay.classList.toggle('open');
};

function bootAuthenticatedUser() {
  // Hide login screen, show app
  document.getElementById('login-container').style.display = 'none';
  document.getElementById('app').style.display = 'flex';
  
  // Hydrate profile details
  document.getElementById('footer-username').textContent = State.user.fullName;
  document.getElementById('footer-role').textContent = State.user.role;
  document.getElementById('dropdown-fullname').textContent = State.user.fullName;
  const topbarFullname = document.getElementById('topbar-dropdown-fullname');
  if (topbarFullname) topbarFullname.textContent = State.user.fullName;
  const topbarRole = document.getElementById('topbar-dropdown-role');
  if (topbarRole) topbarRole.textContent = State.user.role;
  
  const initials = State.user.fullName.split(' ').map(n => n[0]).join('').substring(0,2).toUpperCase();
  document.getElementById('header-avatar-badge').textContent = initials;
  document.getElementById('footer-avatar').textContent = initials;
  
  applyRolePermissions();
  checkClockStatus();
  
  // Route to entry page
  const entry = getFallbackPage();
  Router.navigate(entry);
}

/* ── Init ────────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
  // Wire nav clicks
  document.querySelectorAll('.nav-item[data-page]').forEach(el => {
    el.addEventListener('click', () => Router.navigate(el.dataset.page));
  });

  // Both token AND user object must exist — a stale token without the user
  // object (e.g. after a hard-reload that lost localStorage) would crash.
  if (State.token && State.user) {
    bootAuthenticatedUser();
  } else {
    // Clear any partial / stale auth data
    localStorage.removeItem('rp_token');
    localStorage.removeItem('rp_user');
    State.token = null;
    State.user  = null;
    // Show login screen
    document.getElementById('app').style.display = 'none';
    document.getElementById('login-container').style.display = 'flex';
  }
});
