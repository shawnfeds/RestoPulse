/* ============================================================
   RestoPulse — Billing Module
   modules/billing.js
   ============================================================
   POST /api/bills
   Body: { "orderId": "ORD-0091", "paymentMethod": "Cash|Card|UPI" }
   Response: { "id":"BILL-0042","orderId":"ORD-0091","tableNo":7,
               "subtotal":1040,"tax":187.2,"discount":0,"total":1227.2,
               "paymentMethod":"Card","settledAt":"ISO","items":[...] }

   GET  /api/bills?date=&status=
   GET  /api/bills/:id

   POST /api/bills/:id/settle
   Body: { "paymentMethod":"Cash|Card|UPI", "amountTendered": 1300 }

   POST /api/bills/:id/split
   Body: { "splitBy": 3 }
   Response: { "splits": [{ "amount": 409.07 }, ...] }
   ============================================================ */

Router.register('billing', async () => {
  const container = document.getElementById('page-billing');
  container.innerHTML = `
    <div style="display:flex;height:100%;overflow:hidden">
      <!-- Bill list -->
      <div style="width:360px;flex-shrink:0;border-right:1px solid var(--border-subtle);display:flex;flex-direction:column">
        <div style="padding:12px 14px;border-bottom:1px solid var(--border-subtle);background:var(--bg-surface)">
          <div style="display:flex;gap:6px;margin-bottom:10px;flex-wrap:wrap">
            ${['All','Pending','Settled'].map(s =>
              `<button class="btn btn-ghost btn-sm bill-filter-btn ${s==='All'?'active-filter':''}" onclick="billFilter('${s}',this)">${s}</button>`
            ).join('')}
          </div>
          <input class="form-input" placeholder="Search bills…" oninput="billSearch(this.value)"
            style="height:34px;font-size:12px">
        </div>
        <div id="bill-list" style="flex:1;overflow-y:auto"></div>
        <div style="padding:10px 14px;border-top:1px solid var(--border-subtle)">
          <button class="btn btn-primary w-full" onclick="openNewBillModal()">+ Create Bill</button>
        </div>
      </div>

      <!-- Bill detail / Invoice -->
      <div id="bill-detail" style="flex:1;overflow-y:auto;padding:24px">
        <div class="empty-state" style="height:100%;justify-content:center">
          <div class="empty-icon">💳</div>
          <p>Select a bill or create one</p>
        </div>
      </div>
    </div>

    <!-- Modals -->
    ${newBillModalHTML()}
    ${settleModalHTML()}
    ${splitModalHTML()}`;

  window._billsData    = [];
  window._billFilter   = 'All';
  window._billSearch   = '';
  window._selectedBill = null;

  await loadBills();
});

async function loadBills() {
  let data;
  try { data = await API.billsList({}); }
  catch { data = MOCK_BILLING.list(); }
  window._billsData = data;
  renderBillList(data);
}

/* ── List ─────────────────────────────────────────────────── */
function renderBillList(bills) {
  const el = document.getElementById('bill-list');
  if (!bills.length) { el.innerHTML = '<div class="empty-state"><p>No bills found</p></div>'; return; }
  const pmIcon = { Cash:'💵', Card:'💳', UPI:'📱' };
  el.innerHTML = bills.map(b => `
    <div onclick="selectBill('${b.id}')"
      style="padding:12px 14px;border-bottom:1px solid var(--border-subtle);cursor:pointer;
             background:${b.id===window._selectedBill?'var(--bg-raised)':'transparent'};transition:background var(--transition)">
      <div class="flex items-center justify-between mb-1">
        <span style="font-family:var(--font-mono);font-size:12px;color:var(--text-secondary)">${b.id}</span>
        <span class="badge badge-${b.settledAt?'green':'amber'}">${b.settledAt?'Settled':'Pending'}</span>
      </div>
      <div class="flex items-center justify-between">
        <span style="font-weight:500">Table ${b.tableNo} · ${b.orderId}</span>
        <span style="font-weight:600;color:var(--rp-brand)">${Fmt.currency(b.total)}</span>
      </div>
      <div class="flex items-center justify-between mt-1">
        <span class="text-muted text-sm">${b.paymentMethod ? `${pmIcon[b.paymentMethod]||''} ${b.paymentMethod}` : '—'}</span>
        <span class="text-muted text-sm">${b.settledAt ? Fmt.time(b.settledAt) : Fmt.time(b.createdAt)}</span>
      </div>
    </div>`).join('');
}

window.billFilter = (s, btn) => {
  window._billFilter = s;
  document.querySelectorAll('.bill-filter-btn').forEach(b => b.classList.remove('active-filter'));
  btn.classList.add('active-filter');
  applyBillFilters();
};
window.billSearch = (q) => { window._billSearch = q.toLowerCase(); applyBillFilters(); };
function applyBillFilters() {
  let d = window._billsData;
  if (window._billFilter === 'Settled') d = d.filter(b => !!b.settledAt);
  if (window._billFilter === 'Pending') d = d.filter(b => !b.settledAt);
  if (window._billSearch) d = d.filter(b =>
    b.id.toLowerCase().includes(window._billSearch) ||
    b.orderId.toLowerCase().includes(window._billSearch));
  renderBillList(d);
}

/* ── Invoice view ─────────────────────────────────────────── */
window.selectBill = (id) => {
  window._selectedBill = id;
  applyBillFilters();
  const b = window._billsData.find(x => x.id === id);
  if (!b) return;
  const isPending = !b.settledAt;
  const pmIcon = { Cash:'💵', Card:'💳', UPI:'📱' };

  document.getElementById('bill-detail').innerHTML = `
    <div style="max-width:580px;margin:0 auto">
      <!-- Invoice header -->
      <div style="background:var(--bg-surface);border:1px solid var(--border-subtle);border-radius:var(--radius-lg);padding:24px;margin-bottom:16px">
        <div class="flex items-center justify-between mb-4">
          <div>
            <div style="font-size:11px;font-weight:600;letter-spacing:1px;text-transform:uppercase;color:var(--text-muted)">Invoice</div>
            <div style="font-size:22px;font-weight:700;letter-spacing:-0.5px">${b.id}</div>
          </div>
          <div style="text-align:right">
            <div style="font-size:11px;color:var(--text-muted)">RestoPulse</div>
            <div style="font-size:13px;font-weight:500">The Grand Table</div>
            <div style="font-size:12px;color:var(--text-muted)">GSTIN: 29AAAAA0000A1Z5</div>
          </div>
        </div>
        <div class="grid-2" style="gap:10px;margin-bottom:16px">
          <div style="background:var(--bg-raised);border-radius:var(--radius-md);padding:10px 12px">
            <div style="font-size:11px;color:var(--text-muted)">Table / Order</div>
            <div style="font-weight:600">Table ${b.tableNo}</div>
            <div style="font-size:12px;color:var(--text-secondary);font-family:var(--font-mono)">${b.orderId}</div>
          </div>
          <div style="background:var(--bg-raised);border-radius:var(--radius-md);padding:10px 12px">
            <div style="font-size:11px;color:var(--text-muted)">Date / Time</div>
            <div style="font-weight:600">${Fmt.date(b.createdAt)}</div>
            <div style="font-size:12px;color:var(--text-secondary)">${Fmt.time(b.createdAt)}</div>
          </div>
        </div>

        <!-- Line items -->
        <table style="width:100%;border-collapse:collapse;margin-bottom:16px">
          <thead>
            <tr style="border-bottom:1px solid var(--border-subtle)">
              <th style="text-align:left;padding:6px 0;font-size:11px;color:var(--text-muted);font-weight:600;text-transform:uppercase;letter-spacing:0.5px">Item</th>
              <th style="text-align:center;padding:6px 0;font-size:11px;color:var(--text-muted);font-weight:600;text-transform:uppercase;letter-spacing:0.5px">Qty</th>
              <th style="text-align:right;padding:6px 0;font-size:11px;color:var(--text-muted);font-weight:600;text-transform:uppercase;letter-spacing:0.5px">Rate</th>
              <th style="text-align:right;padding:6px 0;font-size:11px;color:var(--text-muted);font-weight:600;text-transform:uppercase;letter-spacing:0.5px">Amount</th>
            </tr>
          </thead>
          <tbody>
            ${b.items.map(i => `
              <tr style="border-bottom:1px solid var(--border-subtle)">
                <td style="padding:8px 0;font-size:13px">${i.name}</td>
                <td style="padding:8px 0;text-align:center;color:var(--text-secondary)">${i.qty}</td>
                <td style="padding:8px 0;text-align:right;color:var(--text-secondary)">${Fmt.currency(i.price)}</td>
                <td style="padding:8px 0;text-align:right;font-weight:500">${Fmt.currency(i.price*i.qty)}</td>
              </tr>`).join('')}
          </tbody>
        </table>

        <!-- Totals -->
        <div style="border-top:1px solid var(--border-subtle);padding-top:12px">
          <div class="flex justify-between" style="margin-bottom:6px">
            <span style="color:var(--text-muted)">Subtotal</span>
            <span>${Fmt.currency(b.subtotal)}</span>
          </div>
          ${b.discount > 0 ? `
            <div class="flex justify-between" style="margin-bottom:6px">
              <span style="color:var(--green)">Discount</span>
              <span style="color:var(--green)">−${Fmt.currency(b.discount)}</span>
            </div>` : ''}
          <div class="flex justify-between" style="margin-bottom:10px">
            <span style="color:var(--text-muted)">GST (18%)</span>
            <span>${Fmt.currency(b.tax)}</span>
          </div>
          <div class="flex justify-between" style="padding-top:10px;border-top:2px solid var(--border-mid)">
            <span style="font-size:18px;font-weight:700">Total</span>
            <span style="font-size:18px;font-weight:700;color:var(--rp-brand)">${Fmt.currency(b.total)}</span>
          </div>
        </div>

        ${b.settledAt ? `
          <div style="margin-top:14px;padding:10px 14px;background:var(--green-soft);border-radius:var(--radius-md);display:flex;align-items:center;gap:8px">
            <span style="color:var(--green);font-size:16px">✓</span>
            <div>
              <div style="font-size:13px;font-weight:600;color:var(--green)">Settled</div>
              <div style="font-size:12px;color:var(--text-muted)">${pmIcon[b.paymentMethod]||''} ${b.paymentMethod} · ${Fmt.datetime(b.settledAt)}</div>
            </div>
          </div>` : ''}
      </div>

      <!-- Actions -->
      ${isPending ? `
        <div class="flex gap-3">
          <button class="btn btn-primary" style="flex:1" onclick="openSettleModal('${b.id}')">
            💳 Settle Payment
          </button>
          <button class="btn btn-secondary" onclick="openSplitModal('${b.id}',${b.total})">
            ÷ Split Bill
          </button>
          <button class="btn btn-ghost btn-icon" onclick="printBill('${b.id}')" title="Print">🖨</button>
        </div>` : `
        <div class="flex gap-3">
          <button class="btn btn-secondary" onclick="printBill('${b.id}')">🖨 Print Receipt</button>
        </div>`}
    </div>`;
};

/* ── New Bill ──────────────────────────────────────────────── */
window.openNewBillModal = async () => {
  let orders;
  try { orders = await API.ordersList({ status: 'Served' }); }
  catch { orders = MOCK_BILLING.servedOrders(); }
  document.getElementById('bill-order-select').innerHTML = orders.map(o =>
    `<option value="${o.id}">${o.id} — Table ${o.tableNo} (${Fmt.currency(o.total)})</option>`
  ).join('');
  Modal.open('modal-new-bill');
};

window.submitNewBill = async () => {
  const orderId = document.getElementById('bill-order-select').value;
  const paymentMethod = document.getElementById('bill-pm').value;
  try {
    const b = await API.billCreate({ orderId, paymentMethod });
    Modal.close('modal-new-bill');
    await loadBills();
    selectBill(b.id);
    Toast.success(`Bill ${b.id} created`);
  } catch { }
};

/* ── Settle ──────────────────────────────────────────────── */
window.openSettleModal = (billId) => {
  const b = window._billsData.find(x => x.id === billId);
  document.getElementById('settle-total').textContent = Fmt.currency(b.total);
  document.getElementById('settle-bill-id').value = billId;
  Modal.open('modal-settle');
};

window.submitSettle = async () => {
  const billId = document.getElementById('settle-bill-id').value;
  const method = document.getElementById('settle-method').value;
  const tendered = parseFloat(document.getElementById('settle-tendered').value) || 0;
  try {
    await API.billSettle(billId, { paymentMethod: method, amountTendered: tendered });
    Modal.close('modal-settle');
    const b = window._billsData.find(x => x.id === billId);
    if (b) { b.settledAt = new Date().toISOString(); b.paymentMethod = method; }
    applyBillFilters();
    selectBill(billId);
    Toast.success(`Bill settled via ${method}`);
  } catch { }
};

/* ── Split ───────────────────────────────────────────────── */
window.openSplitModal = (billId, total) => {
  document.getElementById('split-bill-id').value = billId;
  document.getElementById('split-total').textContent = Fmt.currency(total);
  calcSplit(total, 2);
  document.getElementById('split-by').oninput = function() { calcSplit(total, parseInt(this.value)||1); };
  Modal.open('modal-split');
};

function calcSplit(total, by) {
  const amt = total / Math.max(by, 1);
  document.getElementById('split-result').textContent = `Each person pays ${Fmt.currency(amt)}`;
}

window.submitSplit = async () => {
  const billId  = document.getElementById('split-bill-id').value;
  const splitBy = parseInt(document.getElementById('split-by').value) || 2;
  try {
    const res = await API.billSplit(billId, { splitBy });
    Modal.close('modal-split');
    Toast.success(`Split into ${splitBy} — ${Fmt.currency(res.splits[0].amount)} each`);
  } catch { }
};

window.printBill = (id) => { Toast.info('Sending to printer…'); };

/* ── Modals ───────────────────────────────────────────────── */
function newBillModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-new-bill">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">Create Bill</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-new-bill')">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group mb-3">
            <label class="form-label">Select Served Order</label>
            <select class="form-select" id="bill-order-select"></select>
          </div>
          <div class="form-group">
            <label class="form-label">Payment Method</label>
            <select class="form-select" id="bill-pm">
              <option>Cash</option><option>Card</option><option>UPI</option>
            </select>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" onclick="Modal.close('modal-new-bill')">Cancel</button>
          <button class="btn btn-primary" onclick="submitNewBill()">Create Bill</button>
        </div>
      </div>
    </div>`;
}

function settleModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-settle">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">Settle Payment</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-settle')">✕</button>
        </div>
        <div class="modal-body">
          <input type="hidden" id="settle-bill-id">
          <div style="text-align:center;padding:10px 0 20px">
            <div style="font-size:11px;color:var(--text-muted)">Amount Due</div>
            <div style="font-size:32px;font-weight:700;color:var(--rp-brand)" id="settle-total"></div>
          </div>
          <div class="form-group mb-3">
            <label class="form-label">Payment Method</label>
            <div style="display:grid;grid-template-columns:1fr 1fr 1fr;gap:8px" id="settle-method-btns">
              ${['Cash','Card','UPI'].map((m,i) => `
                <button class="btn btn-secondary ${i===0?'active-filter':''}" style="justify-content:center;padding:10px"
                  onclick="selectPayMethod('${m}',this)">${{Cash:'💵',Card:'💳',UPI:'📱'}[m]} ${m}</button>`
              ).join('')}
            </div>
            <input type="hidden" id="settle-method" value="Cash">
          </div>
          <div class="form-group">
            <label class="form-label">Amount Tendered (Cash only)</label>
            <input class="form-input" id="settle-tendered" type="number" placeholder="e.g. 1300" step="0.01">
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" onclick="Modal.close('modal-settle')">Cancel</button>
          <button class="btn btn-primary" onclick="submitSettle()">Confirm Payment</button>
        </div>
      </div>
    </div>`;
}

window.selectPayMethod = (m, btn) => {
  document.querySelectorAll('#settle-method-btns .btn').forEach(b => b.classList.remove('active-filter'));
  btn.classList.add('active-filter');
  document.getElementById('settle-method').value = m;
};

function splitModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-split">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">Split Bill</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-split')">✕</button>
        </div>
        <div class="modal-body">
          <input type="hidden" id="split-bill-id">
          <div style="text-align:center;margin-bottom:20px">
            <div style="font-size:11px;color:var(--text-muted)">Total Amount</div>
            <div style="font-size:28px;font-weight:700" id="split-total"></div>
          </div>
          <div class="form-group mb-3">
            <label class="form-label">Split By (people)</label>
            <input class="form-input" type="number" id="split-by" min="2" max="20" value="2">
          </div>
          <div style="text-align:center;padding:14px;background:var(--bg-raised);border-radius:var(--radius-md);font-weight:600;color:var(--rp-brand)" id="split-result"></div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" onclick="Modal.close('modal-split')">Cancel</button>
          <button class="btn btn-primary" onclick="submitSplit()">Apply Split</button>
        </div>
      </div>
    </div>`;
}

/* ── Mock ─────────────────────────────────────────────────── */
const MOCK_BILLING = {
  servedOrders: () => [
    {id:'ORD-0091',tableNo:7,total:1227.2},{id:'ORD-0088',tableNo:5,total:637.2}
  ],
  list: () => {
    const t = (m) => new Date(Date.now() - m*60000).toISOString();
    const items = (n) => Array.from({length:n},(_,i)=>({name:['Butter Chicken','Naan','Lassi','Biryani'][i%4],qty:1+i%2,price:[320,60,80,380][i%4]}));
    return [
      {id:'BILL-0042',orderId:'ORD-0091',tableNo:7, items:items(3),subtotal:1040,discount:0,tax:187.2,total:1227.2,paymentMethod:'Card',createdAt:t(40),settledAt:t(35)},
      {id:'BILL-0041',orderId:'ORD-0089',tableNo:12,items:items(4),subtotal:1880,discount:50,tax:333.0,total:2163,  paymentMethod:'UPI', createdAt:t(90),settledAt:t(80)},
      {id:'BILL-0043',orderId:'ORD-0088',tableNo:5, items:items(2),subtotal:540, discount:0,tax:97.2,  total:637.2, paymentMethod:null,  createdAt:t(10),settledAt:null},
      {id:'BILL-0044',orderId:'ORD-0094',tableNo:2, items:items(2),subtotal:720, discount:0,tax:129.6, total:849.6, paymentMethod:null,  createdAt:t(5), settledAt:null},
    ];
  }
};
