/* ============================================================
   RestoPulse — User Management Module
   modules/users.js
   ============================================================ */

Router.register('users', async () => {
  const container = document.getElementById('page-users');
  container.innerHTML = `
    <div style="padding:24px; display:flex; flex-direction:column; height:100%; overflow-y:auto">
      <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:24px; flex-wrap:wrap; gap:16px;">
        <div>
          <h2 style="margin:0; font-size:22px; font-weight:600; color:var(--text-main);">Team Members</h2>
          <p style="margin:4px 0 0 0; font-size:13px; color:var(--text-muted);">Manage employee user accounts and access roles</p>
        </div>
        <button class="btn btn-primary" onclick="openAddUserModal()">+ Add User</button>
      </div>

      <div style="background:var(--bg-card); border:1px solid var(--border-subtle); border-radius:var(--radius-lg); overflow:hidden">
        <table style="width:100%; border-collapse:collapse; text-align:left;">
          <thead>
            <tr style="border-bottom:1px solid var(--border-subtle); background:var(--bg-surface);">
              <th style="padding:16px; font-size:11px; font-weight:600; color:var(--text-muted); text-transform:uppercase; letter-spacing:0.5px">Full Name</th>
              <th style="padding:16px; font-size:11px; font-weight:600; color:var(--text-muted); text-transform:uppercase; letter-spacing:0.5px">Username</th>
              <th style="padding:16px; font-size:11px; font-weight:600; color:var(--text-muted); text-transform:uppercase; letter-spacing:0.5px">Role</th>
              <th style="padding:16px; font-size:11px; font-weight:600; color:var(--text-muted); text-transform:uppercase; letter-spacing:0.5px">Date Joined</th>
            </tr>
          </thead>
          <tbody id="user-table-body">
            <!-- Dynamic rows -->
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add User Modal -->
    <div class="modal-backdrop" id="modal-add-user">
      <div class="modal">
        <div class="modal-header">
          <span class="modal-title">Add New Team Member</span>
          <button class="btn btn-ghost btn-icon" onclick="Modal.close('modal-add-user')">✕</button>
        </div>
        <form id="add-user-form" onsubmit="submitAddUser(event)">
          <div class="modal-body">
            <div class="form-group mb-3">
              <label class="form-label" for="add-user-fullname">Full Name</label>
              <input class="form-input" id="add-user-fullname" required placeholder="e.g. Sam Server">
            </div>
            <div class="form-group mb-3">
              <label class="form-label" for="add-user-username">Username</label>
              <input class="form-input" id="add-user-username" required placeholder="e.g. samserver">
            </div>
            <div class="form-group mb-3">
              <label class="form-label" for="add-user-password">Password</label>
              <input class="form-input" type="password" id="add-user-password" required placeholder="••••••••">
            </div>
            <div class="form-group">
              <label class="form-label" for="add-user-role">Role</label>
              <select class="form-select" id="add-user-role" required>
                <option value="Owner">Owner</option>
                <option value="Manager">Manager</option>
                <option value="Chef">Chef</option>
                <option value="Server">Server</option>
              </select>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" onclick="Modal.close('modal-add-user')">Cancel</button>
            <button type="submit" class="btn btn-primary">Add User</button>
          </div>
        </form>
      </div>
    </div>
  `;

  await loadUsers();
});

async function loadUsers() {
  const tbody = document.getElementById('user-table-body');
  tbody.innerHTML = `
    <tr>
      <td colspan="4" style="text-align:center; padding:32px; color:var(--text-muted)">
        <span>⏳</span> Loading team members...
      </td>
    </tr>
  `;

  let users = [];
  try {
    users = await API.usersList();
  } catch (err) {
    // If backend isn't responding or on failure, load fallback mock data
    users = MOCK_USERS;
  }

  renderUsersList(users);
}

function renderUsersList(users) {
  const tbody = document.getElementById('user-table-body');
  if (!users || !users.length) {
    tbody.innerHTML = `
      <tr>
        <td colspan="4" style="text-align:center; padding:32px; color:var(--text-muted)">
          No user accounts found.
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = users.map(u => {
    const roleBadges = {
      Owner: 'badge-purple',
      Manager: 'badge-blue',
      Chef: 'badge-amber',
      Server: 'badge-green'
    };
    const badgeClass = roleBadges[u.role] || 'badge-gray';
    
    return `
      <tr style="border-bottom:1px solid var(--border-subtle); transition:background 0.2s; cursor:default;" onmouseover="this.style.background='var(--bg-raised)'" onmouseout="this.style.background='transparent'">
        <td style="padding:16px; font-weight:500; color:var(--text-main); display:flex; align-items:center; gap:10px;">
          <div style="width:32px; height:32px; background:var(--bg-raised); border-radius:50%; display:flex; align-items:center; justify-content:center; font-weight:600; color:var(--rp-brand); font-size:12px; border:1px solid var(--border-subtle)">
            ${u.fullName.substring(0,1).toUpperCase()}
          </div>
          ${u.fullName}
        </td>
        <td style="padding:16px; color:var(--text-secondary); font-family:var(--font-mono); font-size:13px;">${u.username}</td>
        <td style="padding:16px;">
          <span class="badge ${badgeClass}">${u.role}</span>
        </td>
        <td style="padding:16px; color:var(--text-muted); font-size:13px;">${Fmt.date(u.createdAt)}</td>
      </tr>
    `;
  }).join('');
}

window.openAddUserModal = () => {
  document.getElementById('add-user-form').reset();
  Modal.open('modal-add-user');
};

window.submitAddUser = async (e) => {
  e.preventDefault();
  const fullName = document.getElementById('add-user-fullname').value.trim();
  const username = document.getElementById('add-user-username').value.trim();
  const password = document.getElementById('add-user-password').value;
  const role = document.getElementById('add-user-role').value;

  try {
    await API.userCreate({ fullName, username, password, role });
    Modal.close('modal-add-user');
    Toast.success(`User "${fullName}" added successfully`);
    await loadUsers();
  } catch (err) {
    Toast.error(err.message || 'Failed to add user');
  }
};

const MOCK_USERS = [
  { id: 1, username: 'owner', fullName: 'Admin Owner', role: 'Owner', createdAt: '2024-01-01T00:00:00Z' },
  { id: 2, username: 'manager', fullName: 'Jane Manager', role: 'Manager', createdAt: '2024-01-01T00:00:00Z' },
  { id: 3, username: 'chef', fullName: 'Chef Pierre', role: 'Chef', createdAt: '2024-01-01T00:00:00Z' },
  { id: 4, username: 'server', fullName: 'Sam Server', role: 'Server', createdAt: '2024-01-01T00:00:00Z' }
];
