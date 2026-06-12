/* ============================================================
   RestoPulse — Users & Shifts Module
   modules/users.js
   ============================================================ */

Router.register('users', async () => {
  const container = document.getElementById('page-users');
  container.innerHTML = `
    <div class="page-toolbar" style="padding:14px 24px;border-bottom:1px solid var(--border-subtle);display:flex;flex-wrap:wrap;align-items:center;gap:12px;background:var(--bg-surface)">
      <div style="display:flex;gap:6px">
        <button class="btn btn-ghost btn-sm tab-filter active-filter" data-tab="accounts" onclick="switchUsersTab('accounts', this)">👥 Staff Accounts</button>
        <button class="btn btn-ghost btn-sm tab-filter" data-tab="schedules" onclick="switchUsersTab('schedules', this)">📅 Shift Schedules</button>
        <button class="btn btn-ghost btn-sm tab-filter" data-tab="reports" onclick="switchUsersTab('reports', this)">📊 Hours Reports</button>
      </div>
      <div style="margin-left:auto;display:flex;gap:8px" id="users-toolbar-actions">
        <button class="btn btn-primary btn-sm" onclick="openAddStaffModal()">+ Add Staff</button>
      </div>
    </div>
    
    <div class="scroll-area">
      <div id="users-tab-content">
        <!-- Dynamic tab content renders here -->
      </div>
    </div>
    
    ${addStaffModalHTML()}
    ${editStaffModalHTML()}
    ${resetPasswordModalHTML()}
  `;

  // Default tab
  await loadUsersTab('accounts');
});

// Tab routing
window.switchUsersTab = async (tab, btn) => {
  document.querySelectorAll('.tab-filter').forEach(b => b.classList.remove('active-filter'));
  btn.classList.add('active-filter');
  
  const actions = document.getElementById('users-toolbar-actions');
  if (tab === 'accounts') {
    actions.innerHTML = `<button class="btn btn-primary btn-sm" onclick="openAddStaffModal()">+ Add Staff</button>`;
  } else {
    actions.innerHTML = '';
  }
  
  await loadUsersTab(tab);
};

async function loadUsersTab(tab) {
  const content = document.getElementById('users-tab-content');
  content.innerHTML = `<div class="empty-state"><div class="skeleton" style="width:100%;height:150px;border-radius:12px;background:var(--bg-raised)"></div></div>`;
  
  try {
    if (tab === 'accounts') {
      const users = await API.usersList();
      renderStaffAccounts(users, content);
    } else if (tab === 'schedules') {
      await renderShiftSchedules(content);
    } else if (tab === 'reports') {
      await renderHoursReports(content);
    }
  } catch (e) {
    content.innerHTML = `<div class="empty-state"><p>Error loading content: ${e.message}</p></div>`;
  }
}

/* ── Tab 1: Staff Accounts ─────────────────────────────────── */
function renderStaffAccounts(users, el) {
  if (!users.length) {
    el.innerHTML = '<div class="empty-state"><div class="empty-icon">👥</div><p>No staff accounts found.</p></div>';
    return;
  }
  
  const statusBadge = active => active 
    ? `<span class="badge badge-green">Active</span>`
    : `<span class="badge badge-red">Deactivated</span>`;

  el.innerHTML = `
    <div class="card" style="overflow-x:auto;">
      <table class="rp-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Username</th>
            <th>Role</th>
            <th>Status</th>
            <th>Joined On</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          ${users.map(u => `
            <tr>
              <td><strong>${u.fullName}</strong> ${u.id === State.user.id ? '<span style="font-size:10px;color:var(--rp-brand);background:var(--rp-brand-soft);padding:1px 5px;border-radius:4px;margin-left:4px;">You</span>' : ''}</td>
              <td class="mono">${u.username}</td>
              <td><span class="badge badge-blue">${u.role}</span></td>
              <td>${statusBadge(u.isActive)}</td>
              <td class="text-muted">${Fmt.date(u.createdAt)}</td>
              <td>
                <div class="flex gap-2">
                  <button class="btn btn-secondary btn-sm" onclick="openEditStaffModal(${u.id}, '${u.fullName.replace(/'/g, "\\'")}', '${u.role}')" title="Edit Profile">✎ Edit</button>
                  <button class="btn btn-secondary btn-sm" onclick="openResetPasswordModal(${u.id}, '${u.fullName.replace(/'/g, "\\'")}')" title="Reset Password">🔑 Password</button>
                  ${u.id !== State.user.id ? `
                    <button class="btn ${u.isActive ? 'btn-danger' : 'btn-primary'} btn-sm" onclick="toggleStaffStatus(${u.id}, ${u.isActive})">
                      ${u.isActive ? 'Deactivate' : 'Activate'}
                    </button>
                  ` : ''}
                </div>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    </div>
  `;
}

window.toggleStaffStatus = async (id, currentStatus) => {
  const action = currentStatus ? 'deactivate' : 'activate';
  if (!confirm(`Are you sure you want to ${action} this user account?`)) return;
  
  try {
    await API.userToggleStatus(id, !currentStatus);
    Toast.success(`User successfully ${currentStatus ? 'deactivated' : 'activated'}`);
    await loadUsersTab('accounts');
  } catch (e) { }
};

// Add Staff
window.openAddStaffModal = () => {
  document.getElementById('add-staff-form').reset();
  Modal.open('modal-add-staff');
};

window.submitAddStaff = async () => {
  const username = document.getElementById('new-staff-username').value.trim();
  const fullName = document.getElementById('new-staff-fullname').value.trim();
  const password = document.getElementById('new-staff-password').value;
  const role = document.getElementById('new-staff-role').value;
  
  if (!username || !fullName || !password || !role) {
    Toast.error('Please fill in all fields');
    return;
  }
  
  try {
    await API.userCreate({ username, password, fullName, role });
    Modal.close('modal-add-staff');
    Toast.success('Staff member registered successfully');
    await loadUsersTab('accounts');
  } catch (e) { }
};

// Edit Staff
window.openEditStaffModal = (id, fullName, role) => {
  document.getElementById('edit-staff-id').value = id;
  document.getElementById('edit-staff-fullname').value = fullName;
  document.getElementById('edit-staff-role').value = role;
  
  // Prevent changing own role to avoid locking oneself out of features
  const isSelf = id === State.user.id;
  document.getElementById('edit-staff-role').disabled = isSelf;
  
  Modal.open('modal-edit-staff');
};

window.submitEditStaff = async () => {
  const id = parseInt(document.getElementById('edit-staff-id').value);
  const fullName = document.getElementById('edit-staff-fullname').value.trim();
  const role = document.getElementById('edit-staff-role').value;
  
  if (!fullName) {
    Toast.error('Please enter a name');
    return;
  }
  
  try {
    await API.userUpdate(id, { fullName, role });
    Modal.close('modal-edit-staff');
    Toast.success('Staff profile updated');
    
    // If updating self, update State and navbar
    if (id === State.user.id) {
      State.user.fullName = fullName;
      State.user.role = role;
      localStorage.setItem('rp_user', JSON.stringify(State.user));
      document.getElementById('footer-username').textContent = fullName;
      document.getElementById('footer-role').textContent = role;
      document.getElementById('dropdown-fullname').textContent = fullName;
      applyRolePermissions();
    }
    
    await loadUsersTab('accounts');
  } catch (e) { }
};

// Admin reset password
window.openResetPasswordModal = (id, fullName) => {
  document.getElementById('reset-pwd-id').value = id;
  document.getElementById('reset-pwd-title').textContent = `Reset Password for ${fullName}`;
  document.getElementById('reset-pwd-form').reset();
  Modal.open('modal-reset-password');
};

window.submitResetPassword = async () => {
  const id = parseInt(document.getElementById('reset-pwd-id').value);
  const newPass = document.getElementById('reset-new-password').value;
  const confirm = document.getElementById('reset-confirm-password').value;
  
  if (newPass.length < 4) {
    Toast.error('Password must be at least 4 characters long');
    return;
  }
  
  if (newPass !== confirm) {
    Toast.error('Passwords do not match');
    return;
  }
  
  try {
    await API.userChangePassword(id, { currentPassword: null, newPassword: newPass });
    Modal.close('modal-reset-password');
    Toast.success('Password updated successfully');
  } catch (e) { }
};

/* ── Tab 2: Shift Schedules ────────────────────────────────── */
async function renderShiftSchedules(el) {
  const todayStr = new Date().toISOString().split('T')[0];
  
  el.innerHTML = `
    <div class="card mb-4">
      <div class="card-header" style="flex-wrap:wrap;gap:10px;">
        <span class="card-title">Weekly Planner</span>
        <div style="display:flex;align-items:center;gap:8px;">
          <label class="form-label" style="margin:0;">Select Date:</label>
          <input class="form-input" type="date" id="schedule-date-input" value="${todayStr}" style="width:160px;padding:4px 8px;font-size:12px;" onchange="loadScheduleDate(this.value)">
        </div>
      </div>
      <div class="card-body" id="schedule-grid-container" style="overflow-x:auto;">
        <!-- Loaded via API -->
      </div>
    </div>
  `;
  
  await loadScheduleDate(todayStr);
}

window.loadScheduleDate = async (dateStr) => {
  const container = document.getElementById('schedule-grid-container');
  container.innerHTML = '<div class="skeleton" style="height:120px;border-radius:8px;background:var(--bg-raised)"></div>';
  
  try {
    const shiftTypes = await API.getShiftTypes();
    const schedules = await API.getSchedules(dateStr);
    const users = await API.usersList();
    
    // Filter only active users for scheduling
    const activeUsers = users.filter(u => u.isActive);
    
    if (!activeUsers.length) {
      container.innerHTML = '<div class="empty-state"><p>No active staff members available to schedule.</p></div>';
      return;
    }
    
    container.innerHTML = `
      <table class="rp-table">
        <thead>
          <tr>
            <th>Staff Member</th>
            <th>Role</th>
            <th>Assigned Shift Type</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          ${activeUsers.map(u => {
            const sched = schedules.find(s => s.userId === u.id);
            const activeTypeId = sched ? sched.shiftTypeId : 0;
            
            return `
              <tr>
                <td><strong>${u.fullName}</strong></td>
                <td><span class="badge badge-gray">${u.role}</span></td>
                <td>
                  <select class="form-select" id="sched-select-${u.id}" style="width:200px;font-size:12px;padding:4px 8px;">
                    <option value="0" ${activeTypeId === 0 ? 'selected' : ''}>No Shift Scheduled</option>
                    ${shiftTypes.map(st => `
                      <option value="${st.id}" ${activeTypeId === st.id ? 'selected' : ''}>${st.name} (${st.startTime} - ${st.endTime})</option>
                    `).join('')}
                  </select>
                </td>
                <td>
                  <button class="btn btn-primary btn-sm" onclick="saveStaffSchedule(${u.id}, '${dateStr}')">Save</button>
                </td>
              </tr>
            `;
          }).join('')}
        </tbody>
      </table>
    `;
  } catch (e) {
    container.innerHTML = `<div class="empty-state"><p>Failed to load schedule grid: ${e.message}</p></div>`;
  }
};

window.saveStaffSchedule = async (userId, dateStr) => {
  const select = document.getElementById(`sched-select-${userId}`);
  const shiftTypeId = parseInt(select.value);
  
  try {
    await API.setSchedule({ userId, date: dateStr, shiftTypeId });
    Toast.success('Schedule updated successfully');
    await loadScheduleDate(dateStr);
  } catch (e) { }
};

/* ── Tab 3: Hours Reports ──────────────────────────────────── */
async function renderHoursReports(el) {
  const now = new Date();
  const currentMonth = now.getMonth() + 1; // 1-indexed
  const currentYear = now.getFullYear();
  
  let users = [];
  try {
    users = await API.usersList();
  } catch (e) { }

  const months = [
    {v:1, n:'January'}, {v:2, n:'February'}, {v:3, n:'March'}, {v:4, n:'April'},
    {v:5, n:'May'}, {v:6, n:'June'}, {v:7, n:'July'}, {v:8, n:'August'},
    {v:9, n:'September'}, {v:10, n:'October'}, {v:11, n:'November'}, {v:12, n:'December'}
  ];

  el.innerHTML = `
    <div class="card mb-4 no-print">
      <div class="card-header"><span class="card-title">Report Filters</span></div>
      <div class="card-body">
        <form id="report-filter-form" onsubmit="generateHoursReport(event)" style="display:flex;flex-wrap:wrap;gap:12px;align-items:flex-end;">
          <div class="form-group" style="width:200px;">
            <label class="form-label">Staff Member</label>
            <select class="form-select" id="rep-user-id" required>
              ${users.map(u => `<option value="${u.id}" ${u.id===State.user.id?'selected':''}>${u.fullName} (${u.role})</option>`).join('')}
            </select>
          </div>
          <div class="form-group" style="width:140px;">
            <label class="form-label">Month</label>
            <select class="form-select" id="rep-month" required>
              ${months.map(m => `<option value="${m.v}" ${m.v===currentMonth?'selected':''}>${m.n}</option>`).join('')}
            </select>
          </div>
          <div class="form-group" style="width:100px;">
            <label class="form-label">Year</label>
            <input class="form-input" type="number" id="rep-year" value="${currentYear}" min="2025" max="2035" required>
          </div>
          <button class="btn btn-primary" type="submit" style="height:35px;">Generate Report</button>
        </form>
      </div>
    </div>
    
    <div id="report-output-container">
      <!-- Report details loaded here -->
    </div>
  `;
}

window.generateHoursReport = async (event) => {
  if (event) event.preventDefault();
  
  const uid = parseInt(document.getElementById('rep-user-id').value);
  const m = parseInt(document.getElementById('rep-month').value);
  const y = parseInt(document.getElementById('rep-year').value);
  const container = document.getElementById('report-output-container');
  
  container.innerHTML = '<div class="skeleton" style="height:250px;border-radius:12px;background:var(--bg-raised)"></div>';
  
  try {
    const r = await API.userMonthlyReport(uid, m, y);
    
    const fmtHrs = min => {
      const h = Math.floor(min/60);
      const m = min%60;
      return `${h}h ${m}m`;
    };

    container.innerHTML = `
      <div class="print-area">
        <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:14px;">
          <div>
            <h2 style="font-size:18px;font-weight:600;color:var(--text-primary);">${r.fullName} — Monthly Hours Report</h2>
            <p class="text-muted" style="font-size:12px;">Month: ${document.getElementById('rep-month').options[m-1].text} ${y}</p>
          </div>
          <button class="btn btn-secondary btn-sm no-print" onclick="window.print()">🖨 Print Report</button>
        </div>
        
        <div class="grid-4 mb-4">
          <div class="stat-card">
            <div class="stat-label">Total Minutes Worked</div>
            <div class="stat-value" style="font-size:22px;">${fmtHrs(r.totalMinutesWorked)}</div>
            <div class="stat-change text-muted">Clocked in/out logs</div>
          </div>
          <div class="stat-card">
            <div class="stat-label">Regular Hours</div>
            <div class="stat-value" style="font-size:22px;color:var(--green);">${fmtHrs(r.totalRegularMinutes)}</div>
            <div class="stat-change text-muted">Up to scheduled shift duration</div>
          </div>
          <div class="stat-card">
            <div class="stat-label">Overtime Hours</div>
            <div class="stat-value" style="font-size:22px;color:var(--purple);">${fmtHrs(r.totalOvertimeMinutes)}</div>
            <div class="stat-change text-muted">Hours exceeding scheduled shift</div>
          </div>
          <div class="stat-card">
            <div class="stat-label">Late In Count</div>
            <div class="stat-value" style="font-size:22px;color:${r.lateInCount > 0 ? 'var(--red)' : 'var(--text-primary)'};">${r.lateInCount}</div>
            <div class="stat-change text-muted">Clock-in > 15m past shift start</div>
          </div>
        </div>
        
        <div class="card" style="overflow-x:auto;">
          <div class="card-header"><span class="card-title">Daily Shifts Summary</span></div>
          <table class="rp-table">
            <thead>
              <tr>
                <th>Date</th>
                <th>Clock In</th>
                <th>Clock Out</th>
                <th>Shift Schedule</th>
                <th>Regular</th>
                <th>Overtime</th>
                <th>Tardiness</th>
                <th>Notes</th>
              </tr>
            </thead>
            <tbody>
              ${r.shifts.length === 0 ? '<tr><td colspan="8" style="text-align:center;color:var(--text-muted);">No shifts logged for this period.</td></tr>' : ''}
              ${r.shifts.map(s => `
                <tr>
                  <td class="mono">${Fmt.date(s.date)}</td>
                  <td class="mono">${Fmt.time(s.clockInTime)}</td>
                  <td class="mono">${s.clockOutTime ? Fmt.time(s.clockOutTime) : '—'}</td>
                  <td><span class="badge badge-gray">${s.shiftName || 'Unscheduled'}</span></td>
                  <td>${fmtHrs(s.regularMinutes)}</td>
                  <td style="color:${s.overtimeMinutes > 0 ? 'var(--purple)' : 'inherit'};font-weight:${s.overtimeMinutes > 0 ? '600' : 'normal'};">${fmtHrs(s.overtimeMinutes)}</td>
                  <td>
                    ${s.isLate 
                      ? '<span class="badge badge-red">Late In</span>' 
                      : '<span class="badge badge-green">On Time</span>'}
                  </td>
                  <td style="font-size:12px;color:var(--text-secondary);max-width:180px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;" title="${s.notes || ''}">${s.notes || '—'}</td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (e) {
    container.innerHTML = `<div class="empty-state"><p>Failed to generate report: ${e.message}</p></div>`;
  }
};

/* ── Modal HTML Builders ───────────────────────────────────── */
function addStaffModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-add-staff">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">Register Staff Member</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-add-staff')">✕</button>
        </div>
        <form id="add-staff-form" onsubmit="event.preventDefault();submitAddStaff();">
          <div class="modal-body">
            <div class="form-group mb-3">
              <label class="form-label">Full Name</label>
              <input class="form-input" id="new-staff-fullname" placeholder="e.g. Ranveer Brar" required>
            </div>
            <div class="form-group mb-3">
              <label class="form-label">Username</label>
              <input class="form-input" id="new-staff-username" placeholder="e.g. chef_ranveer" required autocomplete="username">
            </div>
            <div class="form-group mb-3">
              <label class="form-label">Initial Password</label>
              <input class="form-input" id="new-staff-password" type="password" placeholder="Set temporary password" required autocomplete="new-password">
            </div>
            <div class="form-group">
              <label class="form-label">Role</label>
              <select class="form-select" id="new-staff-role">
                <option value="Server">Server</option>
                <option value="Chef">Chef</option>
                <option value="Manager">Manager</option>
                <option value="Owner">Owner</option>
              </select>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" onclick="Modal.close('modal-add-staff')">Cancel</button>
            <button type="submit" class="btn btn-primary">Create Account</button>
          </div>
        </form>
      </div>
    </div>
  `;
}

function editStaffModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-edit-staff">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">Edit Staff Profile</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-edit-staff')">✕</button>
        </div>
        <form id="edit-staff-form" onsubmit="event.preventDefault();submitEditStaff();">
          <input type="hidden" id="edit-staff-id">
          <div class="modal-body">
            <div class="form-group mb-3">
              <label class="form-label">Full Name</label>
              <input class="form-input" id="edit-staff-fullname" required>
            </div>
            <div class="form-group">
              <label class="form-label">Role</label>
              <select class="form-select" id="edit-staff-role">
                <option value="Server">Server</option>
                <option value="Chef">Chef</option>
                <option value="Manager">Manager</option>
                <option value="Owner">Owner</option>
              </select>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" onclick="Modal.close('modal-edit-staff')">Cancel</button>
            <button type="submit" class="btn btn-primary">Save Changes</button>
          </div>
        </form>
      </div>
    </div>
  `;
}

function resetPasswordModalHTML() {
  return `
    <div class="modal-backdrop" id="modal-reset-password">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title" id="reset-pwd-title">Reset Password</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-reset-password')">✕</button>
        </div>
        <form id="reset-pwd-form" onsubmit="event.preventDefault();submitResetPassword();">
          <input type="hidden" id="reset-pwd-id">
          <div class="modal-body">
            <div class="form-group mb-3">
              <label class="form-label">New Password</label>
              <input class="form-input" id="reset-new-password" type="password" placeholder="Enter new password" required autocomplete="new-password">
            </div>
            <div class="form-group">
              <label class="form-label">Confirm New Password</label>
              <input class="form-input" id="reset-confirm-password" type="password" placeholder="Confirm new password" required autocomplete="new-password">
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" onclick="Modal.close('modal-reset-password')">Cancel</button>
            <button type="submit" class="btn btn-primary">Update Password</button>
          </div>
        </form>
      </div>
    </div>
  `;
}

/* ── Custom stylesheet additions ───────────────────────────── */
const reportPrintStyles = document.createElement('style');
reportPrintStyles.textContent = `
  @media print {
    body * { display: none !important; }
    .print-area, .print-area * { display: block !important; background: transparent !important; color: #000 !important; }
    .print-area { position: absolute; left: 0; top: 0; width: 100%; }
    .no-print { display: none !important; }
    .rp-table { border: 1px solid #ddd !important; border-collapse: collapse !important; width: 100% !important; }
    .rp-table th, .rp-table td { border: 1px solid #ddd !important; padding: 6px !important; font-size: 11px !important; }
    .stat-card { border: 1px solid #ddd !important; display: inline-block !important; width: 22% !important; margin-right: 2% !important; padding: 10px !important; box-sizing: border-box !important; }
    .stat-value { font-size: 16px !important; }
    .stat-label { font-size: 10px !important; }
    .badge { border: 1px solid #aaa !important; background: none !important; color: #000 !important; padding: 2px 4px !important; }
  }
`;
document.head.appendChild(reportPrintStyles);
