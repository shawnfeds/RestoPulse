/* ============================================================
   RestoPulse — Dashboard Module
   modules/dashboard.js
   ============================================================
   GET /api/dashboard/summary
   Response:
   {
     "todayRevenue": 48250.00,
     "ordersToday": 87,
     "avgOrderValue": 554.60,
     "tablesOccupied": 12,
     "totalTables": 24,
     "pendingKitchen": 6,
     "lowStockAlerts": 3,
     "revenueChange": 12.4,
     "ordersChange": -3.2,
     "recentOrders": [
       { "id": "ORD-0091", "tableNo": 7, "items": 4,
         "total": 1240.00, "status": "Served", "time": "2024-01-15T13:42:00Z" }
     ],
     "hourlyRevenue": [
       { "hour": "12:00", "revenue": 4200 },
       ...
     ]
   }
   ============================================================ */

Router.register('dashboard', async () => {
  const container = document.getElementById('page-dashboard');
  container.innerHTML = `
    <div class="scroll-area">
      <div id="dash-stats" class="grid-4 mb-4">
        ${[0,1,2,3].map(() => `<div class="stat-card"><div class="skeleton" style="height:60px;border-radius:8px;background:var(--bg-raised)"></div></div>`).join('')}
      </div>
      <div class="grid-2 mb-4">
        <div class="card">
          <div class="card-header"><span class="card-title">Hourly Revenue</span>
            <span class="flex items-center gap-2 text-sm text-muted"><span class="live-dot"></span>Live</span>
          </div>
          <div class="card-body" id="dash-chart" style="height:180px;position:relative"></div>
        </div>
        <div class="card">
          <div class="card-header"><span class="card-title">Table Occupancy</span></div>
          <div class="card-body" id="dash-occupancy"></div>
        </div>
      </div>
      <div class="card">
        <div class="card-header">
          <span class="card-title">Recent Orders</span>
          <button class="btn btn-secondary btn-sm" onclick="Router.navigate('orders')">View all</button>
        </div>
        <div id="dash-recent-orders"></div>
      </div>
    </div>`;

  // ── Fetch summary ──────────────────────────────────────
  let data;
  try {
    data = await API.dashboardSummary();
  } catch {
    // Use mock data when API is not yet wired
    data = MOCK.dashboardSummary();
  }

  renderStats(data);
  renderChart(data.hourlyRevenue);
  renderOccupancy(data.tablesOccupied, data.totalTables);
  renderRecentOrders(data.recentOrders);
});

function renderStats(d) {
  document.getElementById('dash-stats').innerHTML = `
    <div class="stat-card">
      <div class="stat-label">Today's Revenue</div>
      <div class="stat-value">${Fmt.currency(d.todayRevenue)}</div>
      <div class="stat-change stat-${d.revenueChange >= 0 ? 'up':'down'}">
        ${d.revenueChange >= 0 ? '↑' : '↓'} ${Math.abs(d.revenueChange)}% vs yesterday
      </div>
    </div>
    <div class="stat-card">
      <div class="stat-label">Orders Today</div>
      <div class="stat-value">${Fmt.number(d.ordersToday)}</div>
      <div class="stat-change stat-${d.ordersChange >= 0 ? 'up':'down'}">
        ${d.ordersChange >= 0 ? '↑' : '↓'} ${Math.abs(d.ordersChange)}% vs yesterday
      </div>
    </div>
    <div class="stat-card">
      <div class="stat-label">Avg Order Value</div>
      <div class="stat-value">${Fmt.currency(d.avgOrderValue)}</div>
      <div class="stat-change text-muted">Per cover</div>
    </div>
    <div class="stat-card">
      <div class="stat-label">Alerts</div>
      <div class="stat-value">${d.pendingKitchen + d.lowStockAlerts}</div>
      <div class="stat-change text-muted">
        ${d.pendingKitchen} kitchen · ${d.lowStockAlerts} stock
      </div>
    </div>`;
}

function renderChart(hourly) {
  const el = document.getElementById('dash-chart');
  if (!hourly || !hourly.length) { el.innerHTML = '<div class="empty-state"><p>No data</p></div>'; return; }
  const max = Math.max(...hourly.map(h => h.revenue));
  const bars = hourly.map(h => {
    const pct = max > 0 ? (h.revenue / max * 100) : 0;
    return `
      <div style="flex:1;display:flex;flex-direction:column;align-items:center;gap:4px;justify-content:flex-end">
        <div style="font-size:10px;color:var(--text-muted)">${Fmt.currency(h.revenue)}</div>
        <div style="width:100%;background:var(--rp-brand);border-radius:4px 4px 0 0;height:${pct}%;min-height:4px;opacity:0.85;transition:height 0.5s ease"></div>
        <div style="font-size:10px;color:var(--text-muted);white-space:nowrap">${h.hour}</div>
      </div>`;
  }).join('');
  el.innerHTML = `<div style="display:flex;gap:4px;align-items:flex-end;height:100%;padding-bottom:20px">${bars}</div>`;
}

function renderOccupancy(occupied, total) {
  const el = document.getElementById('dash-occupancy');
  const free = total - occupied;
  const pct  = Math.round((occupied / total) * 100);
  const dots  = Array.from({length: total}, (_, i) =>
    `<div style="width:28px;height:28px;border-radius:6px;background:${i < occupied ? 'var(--rp-brand)' : 'var(--bg-raised)'};border:1px solid ${i < occupied ? 'var(--rp-brand-dark)' : 'var(--border-mid)'}"></div>`
  ).join('');
  el.innerHTML = `
    <div class="flex items-center justify-between mb-3">
      <div><span style="font-size:28px;font-weight:600">${occupied}</span><span style="color:var(--text-muted)"> / ${total}</span></div>
      <div style="text-align:right">
        <div style="font-size:22px;font-weight:600;color:var(--rp-brand)">${pct}%</div>
        <div style="font-size:11px;color:var(--text-muted)">Occupancy</div>
      </div>
    </div>
    <div style="display:flex;flex-wrap:wrap;gap:5px;margin-bottom:10px">${dots}</div>
    <div class="flex gap-3 text-sm">
      <span><span class="dot dot-red" style="background:var(--rp-brand)"></span> Occupied: ${occupied}</span>
      <span><span class="dot" style="background:var(--bg-raised);border:1px solid var(--border-mid)"></span> Free: ${free}</span>
    </div>`;
}

function renderRecentOrders(orders) {
  const el = document.getElementById('dash-recent-orders');
  if (!orders || !orders.length) { el.innerHTML = '<div class="empty-state"><p>No recent orders</p></div>'; return; }
  const statusBadge = s => {
    const map = { Served: 'green', Preparing: 'amber', New: 'blue', Billed: 'purple', Void: 'red' };
    return `<span class="badge badge-${map[s]||'gray'}">${s}</span>`;
  };
  el.innerHTML = `
    <table class="rp-table">
      <thead><tr>
        <th>Order ID</th><th>Table</th><th>Items</th>
        <th>Total</th><th>Status</th><th>Time</th>
      </tr></thead>
      <tbody>
        ${orders.map(o => `
          <tr style="cursor:pointer" onclick="Router.navigate('orders')">
            <td class="mono">${o.id}</td>
            <td>Table ${o.tableNo}</td>
            <td>${o.items} items</td>
            <td style="font-weight:500">${Fmt.currency(o.total)}</td>
            <td>${statusBadge(o.status)}</td>
            <td class="text-muted">${Fmt.time(o.time)}</td>
          </tr>`).join('')}
      </tbody>
    </table>`;
}

/* ── Mock data (remove once backend is wired) ─────────────── */
const MOCK = {
  dashboardSummary: () => ({
    todayRevenue: 48250, ordersToday: 87, avgOrderValue: 554.6,
    tablesOccupied: 14, totalTables: 24, pendingKitchen: 6, lowStockAlerts: 3,
    revenueChange: 12.4, ordersChange: -3.2,
    hourlyRevenue: [
      {hour:'11am',revenue:1800},{hour:'12pm',revenue:5200},{hour:'1pm',revenue:8900},
      {hour:'2pm',revenue:7100},{hour:'3pm',revenue:2400},{hour:'4pm',revenue:1600},
      {hour:'5pm',revenue:3100},{hour:'6pm',revenue:6800},{hour:'7pm',revenue:9200},
      {hour:'8pm',revenue:8100},{hour:'9pm',revenue:4050}
    ],
    recentOrders: [
      {id:'ORD-0091',tableNo:7, items:4,total:1240,status:'Served',  time: new Date(Date.now()-8*60000).toISOString()},
      {id:'ORD-0090',tableNo:3, items:2,total:680, status:'Preparing',time: new Date(Date.now()-14*60000).toISOString()},
      {id:'ORD-0089',tableNo:12,items:6,total:2350,status:'Billed',  time: new Date(Date.now()-22*60000).toISOString()},
      {id:'ORD-0088',tableNo:5, items:3,total:990, status:'Served',  time: new Date(Date.now()-35*60000).toISOString()},
      {id:'ORD-0087',tableNo:9, items:5,total:1870,status:'New',     time: new Date(Date.now()-4*60000).toISOString()},
    ]
  })
};
