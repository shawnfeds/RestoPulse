/* ============================================================
   RestoPulse — Notification System
   components/js/notifications.js

   Role → what they receive:
   ─────────────────────────────────────────────────────────────
   Chef    / Owner / Manager  ← 'new_order'    (Server placed order)
   Server  / Owner / Manager  ← 'dish_ready'   (Chef bumped ticket)
   Owner   / Manager          ← 'bill_settled' (Bill settled)
   ============================================================ */

const Notifications = (() => {
  /* ── Internal state ─────────────────────────────────────── */
  let _notifs = [];      // Today's notifications for current role

  /* ── Init ────────────────────────────────────────────────── */
  async function init() {
    await poll();
    _renderBell();
  }

  /* ── Mark a single notification read ────────────────────── */
  async function markRead(id) {
    try {
      await API.notificationMarkRead(id);
      const n = _notifs.find(x => x.id === id);
      if (n) { n.read = true; }
      _renderBell();
      _renderPanel();
    } catch (e) {
      console.error("Failed to mark notification read:", e);
    }
  }

  /* ── Mark all read ───────────────────────────────────────── */
  async function markAllRead() {
    try {
      await API.notificationsMarkAllRead();
      _notifs.forEach(n => { n.read = true; });
      _renderBell();
      _renderPanel();
    } catch (e) {
      console.error("Failed to mark all notifications read:", e);
    }
  }

  /* ── Clear all ───────────────────────────────────────────── */
  function clear() {
    _notifs = [];
    _renderBell();
  }

  /* ── Notifications for current user's role ───────────────── */
  function _forCurrentRole() {
    return _notifs;
  }

  function _unreadCount() {
    return _notifs.filter(n => !n.read).length;
  }

  /* ── Bell badge render ───────────────────────────────────── */
  function _renderBell() {
    const bell = document.getElementById('btn-notifications');
    if (!bell) return;
    const count = _unreadCount();
    let badge = document.getElementById('notif-bell-badge');
    if (count > 0) {
      if (!badge) {
        badge = document.createElement('span');
        badge.id = 'notif-bell-badge';
        badge.style.cssText = `
          position:absolute;top:-4px;right:-4px;
          background:var(--red);color:#fff;
          font-size:10px;font-weight:700;
          min-width:16px;height:16px;border-radius:99px;
          display:flex;align-items:center;justify-content:center;
          padding:0 3px;line-height:1;
          box-shadow:0 0 0 2px var(--bg-surface);
          animation: notifPop 0.25s cubic-bezier(0.34,1.56,0.64,1);
        `;
        bell.style.position = 'relative';
        bell.appendChild(badge);
      }
      badge.textContent = count > 99 ? '99+' : count;
    } else if (badge) {
      badge.remove();
    }
  }

  function _animateBell() {
    const bell = document.getElementById('btn-notifications');
    if (!bell) return;
    bell.style.animation = 'none';
    void bell.offsetWidth; // reflow
    bell.style.animation = 'bellShake 0.5s ease';
    setTimeout(() => { bell.style.animation = ''; }, 500);
  }

  /* ── Panel toggle ────────────────────────────────────────── */
  function togglePanel(e) {
    e.stopPropagation();
    // Close profile dropdowns
    document.getElementById('profile-dropdown')?.classList.remove('open');
    document.getElementById('topbar-profile-dropdown')?.classList.remove('open');

    const panel = document.getElementById('notif-panel');
    const isOpen = panel.classList.contains('open');
    if (isOpen) { panel.classList.remove('open'); return; }
    panel.classList.add('open');
    _renderPanel();
  }

  /* ── Panel render ────────────────────────────────────────── */
  function _renderPanel() {
    const panel = document.getElementById('notif-panel');
    if (!panel || !panel.classList.contains('open')) return;

    const items = _forCurrentRole();
    const unread = items.filter(n => !n.read).length;

    const typeIcon = {
      new_order:    '🧾',
      dish_ready:   '🍳',
      bill_settled: '💳',
    };
    const typeColor = {
      new_order:    'var(--blue)',
      dish_ready:   'var(--green)',
      bill_settled: 'var(--rp-brand)',
    };

    panel.innerHTML = `
      <div style="padding:14px 16px;border-bottom:1px solid var(--border-subtle);
                  display:flex;align-items:center;justify-content:space-between">
        <div>
          <div style="font-size:14px;font-weight:600">Notifications</div>
          <div style="font-size:11px;color:var(--text-muted);margin-top:1px">Today only · ${State.user?.role}</div>
        </div>
        ${unread > 0 ? `
          <button onclick="Notifications.markAllRead()"
            style="font-size:11px;color:var(--rp-brand);background:none;border:none;
                   cursor:pointer;font-weight:500;padding:4px 8px;border-radius:var(--radius-sm);
                   transition:background var(--transition)"
            onmouseover="this.style.background='var(--rp-brand-soft)'"
            onmouseout="this.style.background='none'">
            Mark all read
          </button>` : ''}
      </div>

      <div style="max-height:360px;overflow-y:auto">
        ${items.length === 0 ? `
          <div style="padding:40px 20px;text-align:center;color:var(--text-muted)">
            <div style="font-size:28px;margin-bottom:8px;opacity:0.4">🔔</div>
            <div style="font-size:13px">No notifications today</div>
          </div>
        ` : items.map(n => `
          <div onclick="Notifications.markRead(${n.id})"
            style="padding:12px 16px;border-bottom:1px solid var(--border-subtle);
                   cursor:pointer;transition:background var(--transition);
                   background:${n.read ? 'transparent' : 'var(--rp-brand-soft)'};
                   border-left:3px solid ${n.read ? 'transparent' : typeColor[n.type] || 'var(--rp-brand)'}"
            onmouseover="this.style.background='var(--bg-raised)'"
            onmouseout="this.style.background='${n.read ? 'transparent' : 'var(--rp-brand-soft)'}'">
            <div style="display:flex;align-items:flex-start;gap:10px">
              <div style="font-size:18px;flex-shrink:0;margin-top:1px">${typeIcon[n.type] || '🔔'}</div>
              <div style="flex:1;min-width:0">
                <div style="display:flex;align-items:center;justify-content:space-between;gap:8px">
                  <div style="font-size:13px;font-weight:${n.read ? '400' : '600'};
                              color:${n.read ? 'var(--text-secondary)' : 'var(--text-primary)'};
                              white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                    ${n.title}
                  </div>
                  ${!n.read ? `<div style="width:7px;height:7px;border-radius:50%;
                    background:${typeColor[n.type] || 'var(--rp-brand)'};flex-shrink:0"></div>` : ''}
                </div>
                <div style="font-size:12px;color:var(--text-muted);margin-top:2px;
                            white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                  ${n.message}
                </div>
                <div style="font-size:11px;color:var(--text-muted);margin-top:4px">
                  ${_relativeTime(n.timestamp)}
                </div>
              </div>
            </div>
          </div>
        `).join('')}
      </div>

      ${items.length > 0 ? `
        <div style="padding:10px 16px;border-top:1px solid var(--border-subtle);text-align:center">
          <span style="font-size:11px;color:var(--text-muted)">
            Showing ${items.length} notification${items.length !== 1 ? 's' : ''} from today
          </span>
        </div>
      ` : ''}
    `;
  }

  function _relativeTime(iso) {
    const diff = Date.now() - new Date(iso);
    const mins = Math.floor(diff / 60000);
    if (mins < 1)  return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    return `${hrs}h ${mins % 60}m ago`;
  }

  /* ── Polling-based fetch from backend ───────────────────── */
  async function poll() {
    if (!State.user) return;
    try {
      const serverNotifs = await API.notificationsList();
      if (!Array.isArray(serverNotifs)) return;

      const newUnreadCount = serverNotifs.filter(n => !n.read).length;
      const oldUnreadCount = _unreadCount();

      _notifs = serverNotifs;

      _renderBell();
      _renderPanel();

      if (newUnreadCount > oldUnreadCount) {
        _animateBell();
      }
    } catch (e) {
      console.error('Failed to poll notifications:', e);
    }
  }

  /* ── Public API ──────────────────────────────────────────── */
  return {
    init,
    poll,
    markRead,
    markAllRead,
    clear,
    togglePanel,
  };
})();

/* ── Close notification panel on outside click ──────────────── */
document.addEventListener('click', (e) => {
  const panel = document.getElementById('notif-panel');
  const btn   = document.getElementById('btn-notifications');
  if (panel && !panel.contains(e.target) && e.target !== btn && !btn?.contains(e.target)) {
    panel.classList.remove('open');
  }
});
