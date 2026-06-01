/* ============================================================
   RestoPulse — Table Management Module
   modules/tables.js
   ============================================================
   GET    /api/tables
   Response: [{ "id":1, "tableNo":1, "capacity":4, "status":"Available|Occupied|Reserved|Cleaning",
                "currentOrderId":null, "section":"Main Hall", "assignedStaff":"Priya S." }]

   POST   /api/tables
   Body:   { "tableNo":int, "capacity":int, "section":string }

   PUT    /api/tables/:id
   Body:   { "tableNo":int, "capacity":int, "section":string }

   PATCH  /api/tables/:id/status
   Body:   { "status":"Available|Occupied|Reserved|Cleaning" }

   DELETE /api/tables/:id
   ============================================================ */

Router.register('tables', async () => {
  const container = document.getElementById('page-tables');
  container.innerHTML = `
    <div class="page-toolbar" style="padding:14px 24px;border-bottom:1px solid var(--border-subtle);display:flex;align-items:center;gap:10px;background:var(--bg-surface)">
      <div style="display:flex;gap:6px">
        ${['All','Available','Occupied','Reserved','Cleaning'].map(s =>
          `<button class="btn btn-ghost btn-sm table-filter ${s==='All'?'active-filter':''}" data-filter="${s}" onclick="tablesFilter('${s}',this)">${s}</button>`
        ).join('')}
      </div>
      <div style="margin-left:auto;display:flex;gap:8px">
        <button class="btn btn-secondary btn-sm" id="btn-view-list" onclick="setTableView('list')">☰ List</button>
        <button class="btn btn-secondary btn-sm" id="btn-view-grid" onclick="setTableView('grid')">⊞ Floor</button>
        <button class="btn btn-primary btn-sm" onclick="openAddTableModal()">+ Add Table</button>
      </div>
    </div>
    <div class="scroll-area">
      <div id="tables-container"></div>
    </div>
    ${addTableModalHTML()}
    ${tableDetailModalHTML()}`;

  window._tableView  = 'grid';
  window._tableData  = [];
  window._tableFilter = 'All';

  let data;
  try { data = await API.tablesList(); }
  catch { data = MOCK_TABLES.list(); }
  window._tableData = data;
  renderTables(data, 'grid');
});

/* ── Filter ───────────────────────────────────────────────── */
window.tablesFilter = (status, btn) => {
  window._tableFilter = status;
  document.querySelectorAll('.table-filter').forEach(b => b.classList.remove('active-filter'));
  btn.classList.add('active-filter');
  const filtered = status === 'All' ? window._tableData : window._tableData.filter(t => t.status === status);
  renderTables(filtered, window._tableView);
};

/* ── View toggle ──────────────────────────────────────────── */
window.setTableView = (view) => {
  window._tableView = view;
  const filtered = window._tableFilter === 'All' ? window._tableData : window._tableData.filter(t => t.status === window._tableFilter);
  renderTables(filtered, view);
};

/* ── Render ───────────────────────────────────────────────── */
function renderTables(tables, view) {
  const el = document.getElementById('tables-container');
  if (!tables.length) { el.innerHTML = '<div class="empty-state"><div class="empty-icon">🪑</div><p>No tables found</p></div>'; return; }
  if (view === 'grid') renderFloorPlan(tables, el);
  else renderTableList(tables, el);
}

function statusColor(s) {
  return { Available:'--green', Occupied:'--rp-brand', Reserved:'--amber', Cleaning:'--blue' }[s] || '--text-muted';
}
function statusBadgeClass(s) {
  return { Available:'green', Occupied:'red', Reserved:'amber', Cleaning:'blue' }[s] || 'gray';
}

function renderFloorPlan(tables, el) {
  el.style.padding = '20px 24px';
  el.innerHTML = `
    <div style="display:grid;grid-template-columns:repeat(auto-fill,minmax(150px,1fr));gap:14px">
      ${tables.map(t => `
        <div class="table-card" onclick="openTableDetail(${t.id})"
          style="background:var(--bg-surface);border:2px solid var(${statusColor(t.status)});
                 border-radius:var(--radius-lg);padding:14px;cursor:pointer;
                 transition:all var(--transition);position:relative;user-select:none"
          onmouseenter="this.style.transform='scale(1.02)'" onmouseleave="this.style.transform='scale(1)'">
          <div style="font-size:11px;color:var(--text-muted);margin-bottom:4px">${t.section}</div>
          <div style="font-size:26px;font-weight:700;color:var(${statusColor(t.status)})">T${t.tableNo}</div>
          <div style="font-size:12px;color:var(--text-secondary);margin:2px 0">${t.capacity} seats</div>
          <span class="badge badge-${statusBadgeClass(t.status)}" style="margin-top:6px;display:inline-flex">${t.status}</span>
          ${t.currentOrderId ? `<div style="font-size:11px;color:var(--text-muted);margin-top:4px;font-family:var(--font-mono)">${t.currentOrderId}</div>` : ''}
          ${t.assignedStaff ? `<div style="font-size:11px;color:var(--text-muted)">👤 ${t.assignedStaff}</div>` : ''}
        </div>`).join('')}
    </div>`;
}

function renderTableList(tables, el) {
  el.style.padding = '20px 24px';
  el.innerHTML = `
    <div class="card">
      <table class="rp-table">
        <thead><tr><th>Table</th><th>Section</th><th>Capacity</th><th>Status</th><th>Order</th><th>Staff</th><th>Actions</th></tr></thead>
        <tbody>
          ${tables.map(t => `
            <tr>
              <td><strong>T${t.tableNo}</strong></td>
              <td>${t.section}</td>
              <td>${t.capacity} seats</td>
              <td><span class="badge badge-${statusBadgeClass(t.status)}">${t.status}</span></td>
              <td class="mono">${t.currentOrderId || '—'}</td>
              <td>${t.assignedStaff || '—'}</td>
              <td>
                <div class="flex gap-2">
                  <select class="form-select" style="width:130px;padding:4px 8px;font-size:12px" onchange="quickSetStatus(${t.id},this.value)">
                    ${['Available','Occupied','Reserved','Cleaning'].map(s => `<option ${s===t.status?'selected':''}>${s}</option>`).join('')}
                  </select>
                  <button class="btn btn-ghost btn-sm" onclick="openTableDetail(${t.id})">✎</button>
                  <button class="btn btn-danger btn-sm" onclick="deleteTable(${t.id})">✕</button>
                </div>
              </td>
            </tr>`).join('')}
        </tbody>
      </table>
    </div>`;
}

/* ── Actions ──────────────────────────────────────────────── */
window.quickSetStatus = async (id, status) => {
  try {
    await API.tableSetStatus(id, status);
    const t = window._tableData.find(x => x.id === id);
    if (t) t.status = status;
    Toast.success(`Table status updated to ${status}`);
  } catch { }
};

window.deleteTable = async (id) => {
  if (!confirm('Delete this table?')) return;
  try {
    await API.tableDelete(id);
    window._tableData = window._tableData.filter(t => t.id !== id);
    renderTables(window._tableData, window._tableView);
    Toast.success('Table removed');
  } catch { }
};

window.openAddTableModal = () => {
  document.getElementById('add-table-form').reset();
  Modal.open('modal-add-table');
};

window.openTableDetail = (id) => {
  const t = window._tableData.find(x => x.id === id);
  if (!t) return;
  document.getElementById('detail-title').textContent = `Table ${t.tableNo}`;
  document.getElementById('detail-body').innerHTML = `
    <div class="grid-2 mb-3">
      <div class="form-group"><label class="form-label">Table No</label>
        <input class="form-input" id="det-no" value="${t.tableNo}"></div>
      <div class="form-group"><label class="form-label">Capacity</label>
        <input class="form-input" type="number" id="det-cap" value="${t.capacity}"></div>
    </div>
    <div class="form-group mb-3"><label class="form-label">Section</label>
      <input class="form-input" id="det-sec" value="${t.section}"></div>
    <div class="form-group"><label class="form-label">Status</label>
      <select class="form-select" id="det-status">
        ${['Available','Occupied','Reserved','Cleaning'].map(s => `<option ${s===t.status?'selected':''}>${s}</option>`).join('')}
      </select>
    </div>`;
  document.getElementById('btn-save-detail').onclick = () => saveTableDetail(t.id);
  Modal.open('modal-table-detail');
};

window.saveTableDetail = async (id) => {
  const body = {
    tableNo:  parseInt(document.getElementById('det-no').value),
    capacity: parseInt(document.getElementById('det-cap').value),
    section:  document.getElementById('det-sec').value,
  };
  const status = document.getElementById('det-status').value;
  try {
    await API.tableUpdate(id, body);
    await API.tableSetStatus(id, status);
    Modal.close('modal-table-detail');
    // Refresh
    const data = await API.tablesList().catch(() => MOCK_TABLES.list());
    window._tableData = data;
    const filtered = window._tableFilter === 'All' ? data : data.filter(t => t.status === window._tableFilter);
    renderTables(filtered, window._tableView);
    Toast.success('Table updated');
  } catch { }
};

window.submitAddTable = async () => {
  const body = {
    tableNo:  parseInt(document.getElementById('new-table-no').value),
    capacity: parseInt(document.getElementById('new-capacity').value),
    section:  document.getElementById('new-section').value,
  };
  try {
    await API.tableCreate(body);
    Modal.close('modal-add-table');
    const data = await API.tablesList().catch(() => MOCK_TABLES.list());
    window._tableData = data;
    renderTables(data, window._tableView);
    Toast.success('Table added');
  } catch { }
};

/* ── Modal HTML ───────────────────────────────────────────── */
function addTableModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-add-table">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">Add New Table</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-add-table')">✕</button>
        </div>
        <form id="add-table-form">
          <div class="modal-body">
            <div class="grid-2 mb-3">
              <div class="form-group"><label class="form-label">Table Number</label>
                <input class="form-input" id="new-table-no" type="number" min="1" placeholder="e.g. 15" required></div>
              <div class="form-group"><label class="form-label">Capacity</label>
                <input class="form-input" id="new-capacity" type="number" min="1" placeholder="e.g. 4" required></div>
            </div>
            <div class="form-group"><label class="form-label">Section</label>
              <select class="form-select" id="new-section">
                <option>Main Hall</option><option>Terrace</option>
                <option>Private Dining</option><option>Bar</option><option>Lounge</option>
              </select>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" onclick="Modal.close('modal-add-table')">Cancel</button>
            <button type="button" class="btn btn-primary" onclick="submitAddTable()">Add Table</button>
          </div>
        </form>
      </div>
    </div>`;
}

function tableDetailModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-table-detail">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title" id="detail-title">Table Detail</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-table-detail')">✕</button>
        </div>
        <div class="modal-body" id="detail-body"></div>
        <div class="modal-footer">
          <button class="btn btn-secondary" onclick="Modal.close('modal-table-detail')">Cancel</button>
          <button class="btn btn-primary" id="btn-save-detail">Save Changes</button>
        </div>
      </div>
    </div>`;
}

/* ── Active filter style ──────────────────────────────────── */
const activeFilterStyle = document.createElement('style');
activeFilterStyle.textContent = `
  .active-filter { background: var(--rp-brand-soft) !important; color: var(--rp-brand) !important; border-color: var(--rp-brand-glow) !important; }
`;
document.head.appendChild(activeFilterStyle);

/* ── Mock data ────────────────────────────────────────────── */
const MOCK_TABLES = {
  list: () => [
    {id:1, tableNo:1,  capacity:2, status:'Available', section:'Bar',          currentOrderId:null,        assignedStaff:null},
    {id:2, tableNo:2,  capacity:4, status:'Occupied',  section:'Main Hall',    currentOrderId:'ORD-0091',  assignedStaff:'Priya S.'},
    {id:3, tableNo:3,  capacity:4, status:'Occupied',  section:'Main Hall',    currentOrderId:'ORD-0090',  assignedStaff:'Rahul M.'},
    {id:4, tableNo:4,  capacity:6, status:'Reserved',  section:'Main Hall',    currentOrderId:null,        assignedStaff:'Anita K.'},
    {id:5, tableNo:5,  capacity:4, status:'Occupied',  section:'Terrace',      currentOrderId:'ORD-0088',  assignedStaff:'Priya S.'},
    {id:6, tableNo:6,  capacity:2, status:'Available', section:'Terrace',      currentOrderId:null,        assignedStaff:null},
    {id:7, tableNo:7,  capacity:4, status:'Cleaning',  section:'Terrace',      currentOrderId:null,        assignedStaff:'Rahul M.'},
    {id:8, tableNo:8,  capacity:8, status:'Available', section:'Private Dining',currentOrderId:null,       assignedStaff:null},
    {id:9, tableNo:9,  capacity:4, status:'Occupied',  section:'Main Hall',    currentOrderId:'ORD-0087',  assignedStaff:'Anita K.'},
    {id:10,tableNo:10, capacity:6, status:'Reserved',  section:'Lounge',       currentOrderId:null,        assignedStaff:'Priya S.'},
    {id:11,tableNo:11, capacity:2, status:'Available', section:'Bar',          currentOrderId:null,        assignedStaff:null},
    {id:12,tableNo:12, capacity:4, status:'Occupied',  section:'Main Hall',    currentOrderId:'ORD-0089',  assignedStaff:'Rahul M.'},
  ]
};
