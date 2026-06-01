/* ============================================================
   RestoPulse — Menu Manager Module
   modules/menu.js
   ============================================================
   GET  /api/menu/categories
   Response: [{ "id":1, "name":"Starters", "itemCount":8 }]

   GET  /api/menu/items?categoryId=
   Response: [{ "id":1, "name":"Paneer Tikka", "description":"...",
                "price":280, "categoryId":1, "isAvailable":true,
                "preparationTime":15, "taxRate":18 }]

   POST /api/menu/items
   Body: { "name","description","price","categoryId","preparationTime","taxRate" }

   PUT  /api/menu/items/:id

   PATCH /api/menu/items/:id/toggle
   (Toggles isAvailable)
   ============================================================ */

Router.register('menu', async () => {
  const container = document.getElementById('page-menu');
  container.innerHTML = `
    <div style="display:flex;height:100%;overflow:hidden">
      <!-- Category sidebar -->
      <div style="width:200px;flex-shrink:0;border-right:1px solid var(--border-subtle);display:flex;flex-direction:column;background:var(--bg-surface)">
        <div style="padding:12px 10px;border-bottom:1px solid var(--border-subtle)">
          <div style="font-size:11px;font-weight:600;text-transform:uppercase;letter-spacing:0.5px;color:var(--text-muted);padding:0 6px 8px">Categories</div>
          <div id="menu-categories"></div>
        </div>
      </div>
      <!-- Items -->
      <div style="flex:1;display:flex;flex-direction:column;overflow:hidden">
        <div style="padding:12px 18px;border-bottom:1px solid var(--border-subtle);background:var(--bg-surface);display:flex;align-items:center;gap:10px">
          <input class="form-input" placeholder="Search menu items…" style="height:34px;font-size:12px;max-width:260px" oninput="menuSearch(this.value)">
          <div style="margin-left:auto;display:flex;gap:8px">
            <button class="btn btn-ghost btn-sm" id="menu-toggle-avail" onclick="menuToggleAvailFilter(this)">Show unavailable</button>
            <button class="btn btn-primary btn-sm" onclick="openAddMenuItemModal()">+ Add Item</button>
          </div>
        </div>
        <div id="menu-items-grid" class="scroll-area" style="display:grid;grid-template-columns:repeat(auto-fill,minmax(240px,1fr));gap:14px;align-content:start"></div>
      </div>
    </div>
    ${menuItemModalHTML()}`;

  window._menuCatId       = null;
  window._menuSearch      = '';
  window._menuShowUnavail = false;
  window._menuItems       = [];
  window._menuCats        = [];

  await loadMenuCategories();
});

async function loadMenuCategories() {
  let cats;
  try { cats = await API.menuCategories(); }
  catch { cats = MOCK_MENU.categories(); }
  window._menuCats = cats;
  const el = document.getElementById('menu-categories');
  el.innerHTML = [{ id: null, name: 'All', itemCount: cats.reduce((s,c) => s+c.itemCount,0) }, ...cats].map(c => `
    <div class="nav-item ${c.id === window._menuCatId ? 'active' : ''}" onclick="selectMenuCat(${c.id === null ? 'null' : c.id},this)"
      style="justify-content:space-between">
      <span>${c.name}</span>
      <span style="font-size:11px;color:var(--text-muted)">${c.itemCount}</span>
    </div>`).join('');
  await loadMenuItems();
}

window.selectMenuCat = async (catId, el) => {
  window._menuCatId = catId;
  document.querySelectorAll('#menu-categories .nav-item').forEach(n => n.classList.remove('active'));
  el.classList.add('active');
  await loadMenuItems();
};

async function loadMenuItems() {
  let items;
  try { items = await API.menuItems(window._menuCatId); }
  catch { items = MOCK_MENU.items(window._menuCatId); }
  window._menuItems = items;
  renderMenuItems();
}

function renderMenuItems() {
  let items = window._menuItems;
  if (window._menuSearch) items = items.filter(i => i.name.toLowerCase().includes(window._menuSearch) || i.description.toLowerCase().includes(window._menuSearch));
  if (!window._menuShowUnavail) items = items.filter(i => i.isAvailable);

  const el = document.getElementById('menu-items-grid');
  if (!items.length) { el.innerHTML = '<div class="empty-state" style="grid-column:1/-1"><div class="empty-icon">🍽</div><p>No items</p></div>'; return; }

  el.innerHTML = items.map(i => `
    <div style="background:var(--bg-surface);border:1px solid var(--border-subtle);border-radius:var(--radius-lg);overflow:hidden;
         opacity:${i.isAvailable?1:0.55}">
      <div style="padding:14px 16px">
        <div class="flex items-center justify-between mb-1">
          <span style="font-weight:600;font-size:14px">${i.name}</span>
          <span style="font-weight:700;color:var(--rp-brand)">${Fmt.currency(i.price)}</span>
        </div>
        <p style="font-size:12px;color:var(--text-muted);margin-bottom:10px;line-height:1.4">${i.description || '—'}</p>
        <div class="flex items-center gap-2">
          <span class="badge badge-gray">${i.preparationTime}m prep</span>
          <span class="badge badge-gray">${i.taxRate}% tax</span>
        </div>
      </div>
      <div style="padding:8px 16px;border-top:1px solid var(--border-subtle);display:flex;align-items:center;justify-content:space-between">
        <span class="badge badge-${i.isAvailable?'green':'red'}">${i.isAvailable?'Available':'Unavailable'}</span>
        <div class="flex gap-2">
          <button class="btn btn-ghost btn-sm" onclick="toggleMenuAvail(${i.id},${i.isAvailable})">${i.isAvailable?'Disable':'Enable'}</button>
          <button class="btn btn-ghost btn-sm" onclick="openEditMenuItemModal(${i.id})">✎</button>
        </div>
      </div>
    </div>`).join('');
}

window.menuSearch        = (q) => { window._menuSearch = q.toLowerCase(); renderMenuItems(); };
window.menuToggleAvailFilter = (btn) => {
  window._menuShowUnavail = !window._menuShowUnavail;
  btn.textContent = window._menuShowUnavail ? 'Hide unavailable' : 'Show unavailable';
  renderMenuItems();
};

window.toggleMenuAvail = async (id, current) => {
  try {
    await API.menuItemToggle(id);
    const i = window._menuItems.find(x => x.id === id);
    if (i) i.isAvailable = !current;
    renderMenuItems();
    Toast.success(`Item ${!current ? 'enabled' : 'disabled'}`);
  } catch { }
};

window.openAddMenuItemModal = () => {
  document.getElementById('menu-item-form').reset();
  document.getElementById('menu-item-modal-title').textContent = 'Add Menu Item';
  document.getElementById('menu-item-id').value = '';
  const sel = document.getElementById('menu-item-cat');
  sel.innerHTML = window._menuCats.map(c => `<option value="${c.id}">${c.name}</option>`).join('');
  Modal.open('modal-menu-item');
};

window.openEditMenuItemModal = (id) => {
  const i = window._menuItems.find(x => x.id === id);
  if (!i) return;
  document.getElementById('menu-item-modal-title').textContent = 'Edit Item';
  document.getElementById('menu-item-id').value = i.id;
  document.getElementById('menu-item-name').value = i.name;
  document.getElementById('menu-item-desc').value = i.description || '';
  document.getElementById('menu-item-price').value = i.price;
  document.getElementById('menu-item-prep').value = i.preparationTime;
  document.getElementById('menu-item-tax').value = i.taxRate;
  const sel = document.getElementById('menu-item-cat');
  sel.innerHTML = window._menuCats.map(c => `<option value="${c.id}" ${c.id===i.categoryId?'selected':''}>${c.name}</option>`).join('');
  Modal.open('modal-menu-item');
};

window.submitMenuItemForm = async () => {
  const id = document.getElementById('menu-item-id').value;
  const body = {
    name:            document.getElementById('menu-item-name').value,
    description:     document.getElementById('menu-item-desc').value,
    price:           parseFloat(document.getElementById('menu-item-price').value),
    categoryId:      parseInt(document.getElementById('menu-item-cat').value),
    preparationTime: parseInt(document.getElementById('menu-item-prep').value),
    taxRate:         parseFloat(document.getElementById('menu-item-tax').value),
  };
  try {
    if (id) await API.menuItemUpdate(id, body);
    else    await API.menuItemCreate(body);
    Modal.close('modal-menu-item');
    await loadMenuItems();
    Toast.success(id ? 'Item updated' : 'Item added');
  } catch { }
};

function menuItemModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-menu-item">
      <div class="modal" style="max-width:560px">
        <div class="modal-header">
          <span class="modal-title" id="menu-item-modal-title">Add Menu Item</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-menu-item')">✕</button>
        </div>
        <div class="modal-body">
          <input type="hidden" id="menu-item-id">
          <div class="form-group mb-3">
            <label class="form-label">Item Name</label>
            <input class="form-input" id="menu-item-name" placeholder="e.g. Butter Chicken" required>
          </div>
          <div class="form-group mb-3">
            <label class="form-label">Description</label>
            <input class="form-input" id="menu-item-desc" placeholder="Short description…">
          </div>
          <div class="grid-2 mb-3">
            <div class="form-group"><label class="form-label">Price (₹)</label>
              <input class="form-input" id="menu-item-price" type="number" step="0.5" min="0"></div>
            <div class="form-group"><label class="form-label">Category</label>
              <select class="form-select" id="menu-item-cat"></select></div>
          </div>
          <div class="grid-2">
            <div class="form-group"><label class="form-label">Prep Time (min)</label>
              <input class="form-input" id="menu-item-prep" type="number" min="1" value="15"></div>
            <div class="form-group"><label class="form-label">Tax Rate (%)</label>
              <input class="form-input" id="menu-item-tax" type="number" value="18"></div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" onclick="Modal.close('modal-menu-item')">Cancel</button>
          <button class="btn btn-primary" onclick="submitMenuItemForm()">Save Item</button>
        </div>
      </div>
    </div>`;
}

const MOCK_MENU = {
  categories: () => [
    {id:1,name:'Starters',itemCount:5},{id:2,name:'Main Course',itemCount:8},
    {id:3,name:'Breads',itemCount:4},{id:4,name:'Desserts',itemCount:4},{id:5,name:'Beverages',itemCount:6}
  ],
  items: (catId) => {
    const all = [
      {id:1,name:'Paneer Tikka',description:'Marinated cottage cheese grilled in tandoor',price:280,categoryId:1,isAvailable:true,preparationTime:15,taxRate:18},
      {id:2,name:'Chicken Seekh',description:'Minced chicken kebab with aromatic spices',price:320,categoryId:1,isAvailable:true,preparationTime:12,taxRate:18},
      {id:3,name:'Butter Chicken',description:'Creamy tomato-based chicken curry',price:320,categoryId:2,isAvailable:true,preparationTime:20,taxRate:18},
      {id:4,name:'Dal Makhani',description:'Slow-cooked black lentils with butter and cream',price:220,categoryId:2,isAvailable:true,preparationTime:15,taxRate:18},
      {id:5,name:'Biryani',description:'Fragrant basmati rice with spiced chicken or veg',price:380,categoryId:2,isAvailable:false,preparationTime:25,taxRate:18},
      {id:6,name:'Naan',description:'Leavened flatbread baked in tandoor',price:60,categoryId:3,isAvailable:true,preparationTime:8,taxRate:5},
      {id:7,name:'Garlic Naan',description:'Naan topped with garlic and butter',price:80,categoryId:3,isAvailable:true,preparationTime:8,taxRate:5},
      {id:8,name:'Gulab Jamun',description:'Soft milk solids in rose-flavored syrup',price:120,categoryId:4,isAvailable:true,preparationTime:5,taxRate:5},
      {id:9,name:'Mango Kulfi',description:'Dense frozen dessert with mango',price:150,categoryId:4,isAvailable:true,preparationTime:2,taxRate:5},
      {id:10,name:'Lassi',description:'Chilled yogurt drink — sweet or salted',price:80,categoryId:5,isAvailable:true,preparationTime:3,taxRate:5},
      {id:11,name:'Masala Chai',description:'Spiced Indian tea with milk',price:60,categoryId:5,isAvailable:true,preparationTime:5,taxRate:5},
    ];
    return catId ? all.filter(i => i.categoryId === catId) : all;
  }
};


/* ============================================================
   RestoPulse — Inventory Module
   modules/inventory (appended here for convenience)
   ============================================================
   GET  /api/inventory
   Response: [{ "id":1,"name":"Chicken","unit":"kg","currentStock":12.5,
                "minThreshold":5,"costPerUnit":180,"lastUpdated":"ISO" }]

   GET  /api/inventory/low-stock
   POST /api/inventory/:id/adjust
   Body: { "type":"Addition|Deduction|Correction","quantity":float,"reason":string }
   ============================================================ */

Router.register('inventory', async () => {
  const container = document.getElementById('page-inventory');
  container.innerHTML = `
    <div class="scroll-area">
      <div class="flex items-center justify-between mb-4">
        <div class="flex gap-2">
          ${['All','Low Stock','OK'].map(s =>
            `<button class="btn btn-ghost btn-sm inv-filter ${s==='All'?'active-filter':''}" onclick="invFilter('${s}',this)">${s}</button>`
          ).join('')}
        </div>
        <input class="form-input" placeholder="Search inventory…" style="height:34px;font-size:12px;width:220px" oninput="invSearch(this.value)">
      </div>
      <div id="inv-low-stock-banner" style="display:none;margin-bottom:14px"></div>
      <div class="card">
        <div id="inv-table-wrap"></div>
      </div>
    </div>
    ${invAdjustModalHTML()}`;

  window._invData   = [];
  window._invFilter = 'All';
  window._invSearch = '';

  await loadInventory();
});

async function loadInventory() {
  let data, lowStock;
  try {
    data     = await API.inventoryList();
    lowStock = await API.inventoryLowStock();
  } catch {
    data     = MOCK_INV.list();
    lowStock = data.filter(i => i.currentStock <= i.minThreshold);
  }
  window._invData = data;

  if (lowStock.length > 0) {
    document.getElementById('inv-low-stock-banner').style.display = 'block';
    document.getElementById('inv-low-stock-banner').innerHTML = `
      <div style="background:var(--amber-soft);border:1px solid rgba(245,158,11,0.3);border-radius:var(--radius-md);padding:10px 14px;display:flex;align-items:center;gap:10px">
        <span style="color:var(--amber);font-size:16px">⚠</span>
        <div>
          <span style="font-weight:600;color:var(--amber)">${lowStock.length} items</span>
          <span style="color:var(--text-secondary)"> below minimum threshold: </span>
          <span style="color:var(--text-secondary)">${lowStock.map(i=>i.name).join(', ')}</span>
        </div>
      </div>`;
    updateBadge('inventory', lowStock.length);
  }
  renderInventory();
}

function renderInventory() {
  let data = window._invData;
  if (window._invFilter === 'Low Stock') data = data.filter(i => i.currentStock <= i.minThreshold);
  if (window._invFilter === 'OK')        data = data.filter(i => i.currentStock > i.minThreshold);
  if (window._invSearch) data = data.filter(i => i.name.toLowerCase().includes(window._invSearch));

  const el = document.getElementById('inv-table-wrap');
  if (!data.length) { el.innerHTML = '<div class="empty-state"><p>No items</p></div>'; return; }

  el.innerHTML = `
    <table class="rp-table">
      <thead><tr><th>Item</th><th>Unit</th><th>In Stock</th><th>Min Level</th><th>Status</th><th>Cost/Unit</th><th>Last Updated</th><th>Actions</th></tr></thead>
      <tbody>
        ${data.map(i => {
          const isLow = i.currentStock <= i.minThreshold;
          const pct = Math.min((i.currentStock / (i.minThreshold * 2)) * 100, 100);
          return `
            <tr>
              <td style="font-weight:500">${i.name}</td>
              <td class="text-muted">${i.unit}</td>
              <td>
                <div style="min-width:100px">
                  <div style="font-weight:600;margin-bottom:4px">${i.currentStock} ${i.unit}</div>
                  <div style="height:4px;background:var(--bg-raised);border-radius:2px;overflow:hidden">
                    <div style="height:100%;width:${pct}%;background:${isLow?'var(--red)':'var(--green)'};border-radius:2px"></div>
                  </div>
                </div>
              </td>
              <td class="text-muted">${i.minThreshold} ${i.unit}</td>
              <td><span class="badge badge-${isLow?'red':'green'}">${isLow?'Low Stock':'OK'}</span></td>
              <td>${Fmt.currency(i.costPerUnit)}</td>
              <td class="text-muted text-sm">${Fmt.datetime(i.lastUpdated)}</td>
              <td>
                <div class="flex gap-2">
                  <button class="btn btn-secondary btn-sm" onclick="openInvAdjust(${i.id},'${i.name}',${i.currentStock},'${i.unit}')">Adjust</button>
                </div>
              </td>
            </tr>`;
        }).join('')}
      </tbody>
    </table>`;
}

window.invFilter = (s,btn) => {
  window._invFilter = s;
  document.querySelectorAll('.inv-filter').forEach(b=>b.classList.remove('active-filter'));
  btn.classList.add('active-filter');
  renderInventory();
};
window.invSearch = (q) => { window._invSearch = q.toLowerCase(); renderInventory(); };

window.openInvAdjust = (id, name, stock, unit) => {
  document.getElementById('inv-adj-id').value = id;
  document.getElementById('inv-adj-name').textContent = `${name} (Current: ${stock} ${unit})`;
  Modal.open('modal-inv-adjust');
};
window.submitInvAdjust = async () => {
  const id = document.getElementById('inv-adj-id').value;
  const body = {
    type:     document.getElementById('inv-adj-type').value,
    quantity: parseFloat(document.getElementById('inv-adj-qty').value),
    reason:   document.getElementById('inv-adj-reason').value,
  };
  try {
    await API.inventoryAdjust(id, body);
    Modal.close('modal-inv-adjust');
    await loadInventory();
    Toast.success('Stock adjusted');
  } catch { }
};

function invAdjustModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-inv-adjust">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">Adjust Stock</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-inv-adjust')">✕</button>
        </div>
        <div class="modal-body">
          <input type="hidden" id="inv-adj-id">
          <div style="font-weight:600;margin-bottom:16px" id="inv-adj-name"></div>
          <div class="grid-2 mb-3">
            <div class="form-group"><label class="form-label">Adjustment Type</label>
              <select class="form-select" id="inv-adj-type">
                <option>Addition</option><option>Deduction</option><option>Correction</option>
              </select></div>
            <div class="form-group"><label class="form-label">Quantity</label>
              <input class="form-input" type="number" id="inv-adj-qty" min="0" step="0.1" placeholder="0.0"></div>
          </div>
          <div class="form-group"><label class="form-label">Reason</label>
            <input class="form-input" id="inv-adj-reason" placeholder="e.g. Delivery received, spoilage…"></div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" onclick="Modal.close('modal-inv-adjust')">Cancel</button>
          <button class="btn btn-primary" onclick="submitInvAdjust()">Adjust</button>
        </div>
      </div>
    </div>`;
}

const MOCK_INV = {
  list: () => [
    {id:1, name:'Chicken',      unit:'kg', currentStock:12.5, minThreshold:5,  costPerUnit:180, lastUpdated:new Date(Date.now()-2*3600000).toISOString()},
    {id:2, name:'Paneer',       unit:'kg', currentStock:3.2,  minThreshold:4,  costPerUnit:280, lastUpdated:new Date(Date.now()-1*3600000).toISOString()},
    {id:3, name:'Basmati Rice', unit:'kg', currentStock:25,   minThreshold:10, costPerUnit:90,  lastUpdated:new Date(Date.now()-6*3600000).toISOString()},
    {id:4, name:'Tomatoes',     unit:'kg', currentStock:8,    minThreshold:5,  costPerUnit:40,  lastUpdated:new Date(Date.now()-3*3600000).toISOString()},
    {id:5, name:'Cream',        unit:'L',  currentStock:2.1,  minThreshold:3,  costPerUnit:120, lastUpdated:new Date(Date.now()-4*3600000).toISOString()},
    {id:6, name:'Butter',       unit:'kg', currentStock:4.5,  minThreshold:2,  costPerUnit:450, lastUpdated:new Date(Date.now()-5*3600000).toISOString()},
    {id:7, name:'Flour',        unit:'kg', currentStock:18,   minThreshold:8,  costPerUnit:45,  lastUpdated:new Date(Date.now()-8*3600000).toISOString()},
    {id:8, name:'Milk',         unit:'L',  currentStock:1.5,  minThreshold:5,  costPerUnit:55,  lastUpdated:new Date(Date.now()-1*3600000).toISOString()},
  ]
};


/* ============================================================
   RestoPulse — Reports Module
   modules/reports (appended here for convenience)
   ============================================================
   GET /api/reports/revenue?from=&to=
   Response: { "totalRevenue":48250, "totalOrders":87,
               "avgOrderValue":554.6, "netProfit":19300,
               "daily":[{ "date":"2024-01-15","revenue":8200,"orders":15 }] }

   GET /api/reports/top-items?from=&to=
   Response: [{ "itemId":1,"name":"Butter Chicken","totalSold":82,"revenue":26240,"rank":1 }]
   ============================================================ */

Router.register('reports', async () => {
  const container = document.getElementById('page-reports');
  container.innerHTML = `
    <div class="scroll-area">
      <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:16px">
        <div class="flex gap-2">
          ${['Today','This Week','This Month','Custom'].map((s,i) =>
            `<button class="btn btn-ghost btn-sm rpt-range ${i===1?'active-filter':''}" onclick="setReportRange('${s}',this)">${s}</button>`
          ).join('')}
        </div>
        <div class="flex gap-2" id="custom-range" style="display:none!important">
          <input type="date" class="form-input" id="rpt-from" style="width:140px;height:34px">
          <input type="date" class="form-input" id="rpt-to"   style="width:140px;height:34px">
          <button class="btn btn-primary btn-sm" onclick="loadReports()">Apply</button>
        </div>
      </div>
      <div id="rpt-stats" class="grid-4 mb-4"></div>
      <div class="grid-2 mb-4">
        <div class="card">
          <div class="card-header"><span class="card-title">Daily Revenue</span></div>
          <div class="card-body" id="rpt-daily-chart" style="height:200px"></div>
        </div>
        <div class="card">
          <div class="card-header"><span class="card-title">Top Items</span></div>
          <div id="rpt-top-items"></div>
        </div>
      </div>
    </div>`;

  window._rptRange = 'This Week';
  await loadReports();
});

window.setReportRange = (s, btn) => {
  window._rptRange = s;
  document.querySelectorAll('.rpt-range').forEach(b => b.classList.remove('active-filter'));
  btn.classList.add('active-filter');
  document.getElementById('custom-range').style.display = s === 'Custom' ? 'flex' : 'none';
  if (s !== 'Custom') loadReports();
};

async function loadReports() {
  const q = buildReportQuery();
  let rev, top;
  try {
    [rev, top] = await Promise.all([API.reportsRevenue(q), API.reportsTopItems(q)]);
  } catch {
    rev = MOCK_REPORTS.revenue();
    top = MOCK_REPORTS.topItems();
  }
  renderRptStats(rev);
  renderRptChart(rev.daily);
  renderTopItems(top);
}

function buildReportQuery() {
  const now   = new Date();
  const today = now.toISOString().split('T')[0];
  const ranges = {
    'Today':      { from: today,                                             to: today },
    'This Week':  { from: new Date(now - 7*86400000).toISOString().split('T')[0],  to: today },
    'This Month': { from: new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0], to: today },
    'Custom':     { from: document.getElementById('rpt-from')?.value || today, to: document.getElementById('rpt-to')?.value || today },
  };
  return ranges[window._rptRange] || ranges['This Week'];
}

function renderRptStats(d) {
  document.getElementById('rpt-stats').innerHTML = `
    <div class="stat-card"><div class="stat-label">Total Revenue</div><div class="stat-value">${Fmt.currency(d.totalRevenue)}</div></div>
    <div class="stat-card"><div class="stat-label">Total Orders</div><div class="stat-value">${Fmt.number(d.totalOrders)}</div></div>
    <div class="stat-card"><div class="stat-label">Avg Order Value</div><div class="stat-value">${Fmt.currency(d.avgOrderValue)}</div></div>
    <div class="stat-card"><div class="stat-label">Net Profit</div><div class="stat-value" style="color:var(--green)">${Fmt.currency(d.netProfit)}</div></div>`;
}

function renderRptChart(daily) {
  const el = document.getElementById('rpt-daily-chart');
  if (!daily?.length) { el.innerHTML = '<div class="empty-state"><p>No data</p></div>'; return; }
  const max = Math.max(...daily.map(d => d.revenue));
  el.innerHTML = `<div style="display:flex;gap:4px;align-items:flex-end;height:100%;padding-bottom:20px">
    ${daily.map(d => `
      <div style="flex:1;display:flex;flex-direction:column;align-items:center;gap:4px;justify-content:flex-end" title="${d.date}: ${Fmt.currency(d.revenue)}">
        <div style="width:100%;background:var(--blue);border-radius:3px 3px 0 0;height:${max>0?d.revenue/max*100:0}%;min-height:4px;opacity:0.8"></div>
        <div style="font-size:9px;color:var(--text-muted);white-space:nowrap">${d.date.slice(5)}</div>
      </div>`).join('')}
  </div>`;
}

function renderTopItems(items) {
  const el = document.getElementById('rpt-top-items');
  const max = Math.max(...items.map(i => i.totalSold));
  el.innerHTML = `<div style="padding:12px 18px">
    ${items.slice(0,8).map((i,idx) => `
      <div style="display:flex;align-items:center;gap:10px;padding:7px 0;${idx<items.length-1?'border-bottom:1px solid var(--border-subtle)':''}">
        <span style="font-family:var(--font-mono);font-size:11px;color:var(--text-muted);width:18px">#${i.rank}</span>
        <div style="flex:1;min-width:0">
          <div style="font-size:13px;font-weight:500;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">${i.name}</div>
          <div style="height:3px;background:var(--bg-raised);border-radius:2px;margin-top:4px">
            <div style="height:100%;width:${(i.totalSold/max*100)}%;background:var(--rp-brand);border-radius:2px"></div>
          </div>
        </div>
        <div style="text-align:right;flex-shrink:0">
          <div style="font-size:12px;font-weight:600;color:var(--rp-brand)">${Fmt.currency(i.revenue)}</div>
          <div style="font-size:11px;color:var(--text-muted)">${i.totalSold} sold</div>
        </div>
      </div>`).join('')}
  </div>`;
}

const MOCK_REPORTS = {
  revenue: () => ({
    totalRevenue: 287450, totalOrders: 512, avgOrderValue: 561.4, netProfit: 112450,
    daily: [
      {date:'2024-01-09',revenue:34200,orders:62},{date:'2024-01-10',revenue:41800,orders:74},
      {date:'2024-01-11',revenue:39500,orders:70},{date:'2024-01-12',revenue:52100,orders:91},
      {date:'2024-01-13',revenue:48900,orders:88},{date:'2024-01-14',revenue:28700,orders:51},
      {date:'2024-01-15',revenue:42250,orders:76},
    ]
  }),
  topItems: () => [
    {rank:1,itemId:3,name:'Butter Chicken',  totalSold:142,revenue:45440},
    {rank:2,itemId:4,name:'Biryani',          totalSold:118,revenue:44840},
    {rank:3,itemId:5,name:'Dal Makhani',      totalSold:109,revenue:23980},
    {rank:4,itemId:1,name:'Paneer Tikka',     totalSold:96, revenue:26880},
    {rank:5,itemId:6,name:'Naan',             totalSold:280,revenue:16800},
    {rank:6,itemId:7,name:'Garlic Naan',      totalSold:195,revenue:15600},
    {rank:7,itemId:9,name:'Gulab Jamun',      totalSold:87, revenue:10440},
    {rank:8,itemId:10,name:'Lassi',           totalSold:74, revenue:5920},
  ]
};
