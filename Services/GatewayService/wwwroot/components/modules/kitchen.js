/* ============================================================
   RestoPulse — Kitchen Display System (KDS)
   modules/kitchen.js
   ============================================================
   GET  /api/kitchen/queue
   Response: [{
     "id": "KI-201", "orderId": "ORD-0090", "tableNo": 3,
     "itemName": "Butter Chicken", "qty": 2, "notes": "Less spicy",
     "status": "Pending|Preparing|Ready",
     "orderedAt": "ISO", "prepStartedAt": "ISO|null",
     "priority": "Normal|Rush",
     "category": "Main|Starter|Dessert|Beverage"
   }]

   PATCH /api/kitchen/items/:id/status
   Body:  { "status": "Preparing|Ready" }

   POST  /api/kitchen/items/:id/bump
   (Marks as done and removes from queue)
   ============================================================ */

Router.register('kitchen', async () => {
  const container = document.getElementById('page-kitchen');
  container.innerHTML = `
    <div style="height:100%;display:flex;flex-direction:column;background:var(--bg-base)">
      <!-- KDS Toolbar -->
      <div style="padding:10px 20px;background:var(--bg-surface);border-bottom:1px solid var(--border-subtle);display:flex;align-items:center;gap:12px">
        <div class="flex items-center gap-2">
          <span class="live-dot"></span>
          <span style="font-size:13px;color:var(--text-secondary)">Live Queue</span>
        </div>
        <div style="margin-left:auto;display:flex;gap-6px;gap:6px;align-items:center">
          <div style="font-size:13px;color:var(--text-muted)">Filter:</div>
          ${['All','Pending','Preparing','Ready'].map(s =>
            `<button class="btn btn-ghost btn-sm kds-filter ${s==='All'?'active-filter':''}" onclick="kdsFilter('${s}',this)">${s}</button>`
          ).join('')}
          <div class="divider" style="width:1px;height:20px;margin:0 6px"></div>
          ${['All','Main','Starter','Dessert','Beverage'].map(s =>
            `<button class="btn btn-ghost btn-sm kds-cat ${s==='All'?'active-filter':''}" onclick="kdsCatFilter('${s}',this)">${s}</button>`
          ).join('')}
          <div class="divider" style="width:1px;height:20px;margin:0 6px"></div>
          <span id="kds-count" style="font-family:var(--font-mono);font-size:13px;color:var(--text-muted)">0 tickets</span>
        </div>
      </div>

      <!-- KDS Grid -->
      <div id="kds-grid" style="flex:1;overflow-y:auto;padding:16px 20px;display:grid;grid-template-columns:repeat(auto-fill,minmax(260px,1fr));gap:14px;align-content:start">
      </div>
    </div>`;

  window._kdsData        = [];
  window._kdsFilter      = 'All';
  window._kdsCatFilter   = 'All';
  window._kdsTimers      = {};

  await loadKitchenQueue();
  // Auto-refresh every 15s
  window._kdsInterval = setInterval(loadKitchenQueue, 15000);
});

// Clean up timer when leaving
document.addEventListener('click', (e) => {
  const nav = e.target.closest('.nav-item');
  if (nav && nav.dataset.page !== 'kitchen' && window._kdsInterval) {
    clearInterval(window._kdsInterval);
    Object.values(window._kdsTimers || {}).forEach(clearInterval);
  }
});

async function loadKitchenQueue() {
  if (document.getElementById('page-kitchen')?.classList.contains('active') === false) return;
  let data;
  try { data = await API.kitchenQueue(); }
  catch { data = MOCK_KDS.queue(); }
  window._kdsData = data;
  renderKDS();
  updateBadge('kitchen', data.filter(i => i.status !== 'Ready').length);
}

function renderKDS() {
  let data = window._kdsData;
  if (window._kdsFilter !== 'All')    data = data.filter(i => i.status === window._kdsFilter);
  if (window._kdsCatFilter !== 'All') data = data.filter(i => i.category === window._kdsCatFilter);

  const el = document.getElementById('kds-grid');
  document.getElementById('kds-count').textContent = `${data.length} ticket${data.length!==1?'s':''}`;

  if (!data.length) {
    el.innerHTML = `<div class="empty-state" style="grid-column:1/-1;padding:60px 20px">
      <div class="empty-icon">✓</div><p>All clear — no pending items</p></div>`;
    return;
  }

  // Sort: Rush first, then by time
  data.sort((a,b) => {
    if (a.priority === 'Rush' && b.priority !== 'Rush') return -1;
    if (b.priority === 'Rush' && a.priority !== 'Rush') return 1;
    return new Date(a.orderedAt) - new Date(b.orderedAt);
  });

  el.innerHTML = data.map(item => kdsTicketHTML(item)).join('');

  // Start timers
  data.forEach(item => {
    if (window._kdsTimers[item.id]) clearInterval(window._kdsTimers[item.id]);
    window._kdsTimers[item.id] = setInterval(() => {
      const el = document.getElementById(`timer-${item.id}`);
      if (el) el.textContent = elapsedSecs(item.prepStartedAt || item.orderedAt);
    }, 1000);
  });
}

function kdsTicketHTML(item) {
  const elapsed  = elapsedSecs(item.prepStartedAt || item.orderedAt);
  const urgency  = getUrgency(item);
  const catColor = { Main:'--rp-brand', Starter:'--amber', Dessert:'--purple', Beverage:'--blue' }[item.category] || '--text-muted';

  const statusConfig = {
    Pending:   { label:'Start',  action:'Preparing', btnClass:'btn-primary',   nextLabel:'Mark Ready' },
    Preparing: { label:'Ready',  action:'Ready',     btnClass:'btn-secondary', nextLabel:'Bump' },
    Ready:     { label:'Bump',   action:'bump',      btnClass:'btn-ghost',     nextLabel:'' },
  };
  const sc = statusConfig[item.status] || statusConfig.Pending;

  return `
    <div id="kds-${item.id}"
      style="background:var(--bg-surface);border:1.5px solid ${urgency.border};
             border-radius:var(--radius-lg);overflow:hidden;
             box-shadow: ${urgency.shadow}">
      <!-- Ticket header -->
      <div style="padding:10px 14px;background:var(--bg-raised);border-bottom:1px solid var(--border-subtle);display:flex;align-items:center;justify-content:space-between">
        <div class="flex items-center gap-2">
          <span style="font-family:var(--font-mono);font-size:12px;color:var(--text-muted)">${item.orderNo || item.orderId}</span>
          ${item.priority === 'Rush' ? `<span class="badge badge-red pulse">RUSH</span>` : ''}
        </div>
        <span style="font-size:13px;font-weight:600;color:var(${urgency.color})" id="timer-${item.id}">${elapsed}</span>
      </div>
      <!-- Item info -->
      <div style="padding:14px">
        <div style="display:flex;align-items:flex-start;justify-content:space-between;gap:8px">
          <div>
            <div style="font-size:16px;font-weight:600;margin-bottom:2px">${item.itemName}</div>
            <div style="font-size:12px;color:var(--text-muted)">Table ${item.tableNo}</div>
          </div>
          <div style="text-align:right;flex-shrink:0">
            <div style="font-size:28px;font-weight:700;color:var(${catColor});line-height:1">×${item.qty}</div>
            <div style="font-size:11px;color:var(--text-muted)">${item.category}</div>
          </div>
        </div>
        ${item.notes ? `
          <div style="margin-top:8px;padding:6px 10px;background:var(--amber-soft);border-radius:var(--radius-sm);
               border-left:3px solid var(--amber);font-size:12px;color:var(--amber)">
            ⚠ ${item.notes}
          </div>` : ''}
      </div>
      <!-- Actions -->
      <div style="padding:10px 14px;border-top:1px solid var(--border-subtle);display:flex;gap:8px">
        <span class="badge badge-${item.status==='Pending'?'blue':item.status==='Preparing'?'amber':'green'}" style="margin-right:auto">${item.status}</span>
        ${item.status !== 'Ready' ? `
          <button class="btn ${sc.btnClass} btn-sm" onclick="kdsSetStatus('${item.id}','${sc.action}')">
            ${item.status === 'Pending' ? '▶ Start' : '✓ Ready'}
          </button>` : `
          <button class="btn btn-primary btn-sm" onclick="kdsBump('${item.id}')">
            ✓ Bump
          </button>`}
      </div>
    </div>`;
}

/* ── Urgency thresholds ─────────────────────────────────────── */
function getUrgency(item) {
  const mins = (Date.now() - new Date(item.orderedAt)) / 60000;
  if (item.priority === 'Rush' || mins > 20) return {
    color: '--red', border: 'var(--red)', shadow: '0 0 0 1px rgba(239,68,68,0.2)'
  };
  if (mins > 12) return {
    color: '--amber', border: 'var(--amber)', shadow: '0 0 0 1px rgba(245,158,11,0.15)'
  };
  return {
    color: '--green', border: 'var(--border-subtle)', shadow: 'none'
  };
}

function elapsedSecs(iso) {
  const secs = Math.floor((Date.now() - new Date(iso)) / 1000);
  const m = Math.floor(secs / 60), s = secs % 60;
  return `${m}:${String(s).padStart(2,'0')}`;
}

/* ── KDS Actions ──────────────────────────────────────────── */
window.kdsSetStatus = async (id, status) => {
  if (status === 'bump') { kdsBump(id); return; }
  try {
    await API.kitchenItemStatus(id, status);
    const item = window._kdsData.find(i => i.id == id);
    if (item) {
      item.status = status;
      if (status === 'Preparing') item.prepStartedAt = new Date().toISOString();
    }
    renderKDS();
    Toast.success(`Ticket ${id} → ${status}`);
  } catch { }
};

window.kdsBump = async (id) => {
  try {
    await API.kitchenItemBump(id);
    window._kdsData = window._kdsData.filter(i => i.id != id);
    if (window._kdsTimers[id]) { clearInterval(window._kdsTimers[id]); delete window._kdsTimers[id]; }
    renderKDS();
    Toast.success('Ticket bumped');
  } catch { }
};

/* ── Filters ──────────────────────────────────────────────── */
window.kdsFilter = (s, btn) => {
  window._kdsFilter = s;
  document.querySelectorAll('.kds-filter').forEach(b => b.classList.remove('active-filter'));
  btn.classList.add('active-filter');
  renderKDS();
};
window.kdsCatFilter = (s, btn) => {
  window._kdsCatFilter = s;
  document.querySelectorAll('.kds-cat').forEach(b => b.classList.remove('active-filter'));
  btn.classList.add('active-filter');
  renderKDS();
};

/* ── Mock data ────────────────────────────────────────────── */
const MOCK_KDS = {
  queue: () => {
    const t = (m) => new Date(Date.now() - m*60000).toISOString();
    return [
      {id:'KI-201',orderId:'ORD-0090',tableNo:3, itemName:'Butter Chicken',qty:2,notes:'Less spicy',   status:'Preparing',orderedAt:t(14),prepStartedAt:t(10),priority:'Normal',category:'Main'},
      {id:'KI-202',orderId:'ORD-0090',tableNo:3, itemName:'Dal Makhani',   qty:1,notes:'',             status:'Preparing',orderedAt:t(14),prepStartedAt:t(10),priority:'Normal',category:'Main'},
      {id:'KI-203',orderId:'ORD-0087',tableNo:9, itemName:'Biryani',       qty:2,notes:'No onion',     status:'Pending',  orderedAt:t(4), prepStartedAt:null, priority:'Normal',category:'Main'},
      {id:'KI-204',orderId:'ORD-0087',tableNo:9, itemName:'Lassi',         qty:2,notes:'',             status:'Pending',  orderedAt:t(4), prepStartedAt:null, priority:'Normal',category:'Beverage'},
      {id:'KI-205',orderId:'ORD-0093',tableNo:2, itemName:'Paneer Tikka',  qty:1,notes:'Extra chutney',status:'Pending',  orderedAt:t(2), prepStartedAt:null, priority:'Rush',  category:'Starter'},
      {id:'KI-206',orderId:'ORD-0085',tableNo:11,itemName:'Gulab Jamun',   qty:3,notes:'',             status:'Ready',    orderedAt:t(22),prepStartedAt:t(18),priority:'Normal',category:'Dessert'},
      {id:'KI-207',orderId:'ORD-0094',tableNo:4, itemName:'Naan',          qty:6,notes:'Garlic naan',  status:'Preparing',orderedAt:t(8), prepStartedAt:t(6), priority:'Normal',category:'Main'},
    ];
  }
};
