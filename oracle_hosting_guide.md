# RestoPulse — Oracle Cloud (OCI) Always Free Hosting Guide 🚀

This guide explains how to set up, configure, and host the entire RestoPulse microservices stack for free on **Oracle Cloud Infrastructure (OCI) Always Free Tier** using Docker and Docker Compose.

---

## 📋 Prerequisites & Account Creation

Oracle Cloud Infrastructure (OCI) offers one of the most generous free tiers in the industry, including up to **4 ARM Ampere Cores and 24 GB of RAM**.

### Step 1: Sign up for OCI Always Free
1. Go to the [Oracle Cloud Free Tier Sign-up](https://www.oracle.com/cloud/free/).
2. Enter your details and select a **Home Region** close to you. 
   > [!IMPORTANT]
   > Ampere ARM VM capacity can sometimes be scarce in certain regions. Pick your home region carefully, as you can only create "Always Free" resources in your designated Home Region.
3. Complete the registration. A credit card is required for identity verification, but you will not be charged unless you manually upgrade to a paid account.

---

## 🪟 Provisioning the Ampere ARM VM

Once your account is active, log into the OCI Console to spin up the virtual machine:

1. Click the top-left menu, navigate to **Compute** -> **Instances**, and click **Create Instance**.
2. **Name**: Enter `restopulse-host`.
3. **Image and Shape**:
   * Click **Edit**.
   * Under **Shape**, click **Change Shape**. Select **Ampere** (ARM64) and check **VM.Standard.A1.Flex**.
   * Configure the resources:
     * **OCPUs**: `4` (or `2` if you want to save resources for other VMs).
     * **Memory (RAM)**: `24 GB` (or `12 GB`).
   * Keep the default Operating System: **Ubuntu 22.04 LTS** (or 24.04).
4. **Networking**: 
   * Ensure **Assign a public IPv4 address** is selected.
5. **Add SSH Keys**:
   * Select **Generate a key pair for me** or paste your existing public key.
   * **MANDATORY**: Click **Save private key** to download the `.key` file. You will need this to connect to your VM.
6. Click **Create** at the bottom. Wait 1-2 minutes for the instance status to show **Running**.

---

## 🌐 Configuring OCI Virtual Network (VCN) Firewall

By default, OCI blocks all incoming internet traffic except for SSH. We must open port `7055` for the YARP Gateway Service:

1. On your instance details page, under **Instance Information**, click the link next to **Virtual Cloud Network** (e.g., `vcn-xxxx`).
2. Click **Security Lists** in the left sidebar, and click your default security list.
3. Click **Add Ingress Rules** and enter:
   * **Source CIDR**: `0.0.0.0/0` (Allows traffic from any IP)
   * **IP Protocol**: `TCP`
   * **Destination Port Range**: `7055`
   * **Description**: `RestoPulse Gateway Service`
4. Click **Add Ingress Rules**.

---

## ⚙️ Environment Setup on the VM

Connect to your VM and install the necessary dependencies:

### 1. SSH into your VM
Replace `your-private-key.key` and `your-instance-ip` with your downloaded key file and instance's public IP:
```bash
chmod 400 your-private-key.key
ssh -i your-private-key.key ubuntu@your-instance-ip
```

### 2. Install Docker & Docker Compose
Run the following commands to install Docker:
```bash
sudo apt update
sudo apt install -y docker.io docker-compose
sudo systemctl enable --now docker
```

To run Docker commands without `sudo` (optional but recommended):
```bash
sudo usermod -aG docker ubuntu
# Log out and log back in for changes to take effect
exit
ssh -i your-private-key.key ubuntu@your-instance-ip
```

### 3. Open Firewall on OS Level
Ubuntu uses `iptables` under the hood on OCI images. Run these commands to open port `7055` locally on the OS:
```bash
sudo iptables -I INPUT 6 -p tcp --dport 7055 -j ACCEPT
sudo netfilter-persistent save
```

---

## 🚢 Deploying RestoPulse

Now clone the code and spin up the Docker Compose stack:

### 1. Clone the Repository
```bash
git clone <your-repository-url> restopulse
cd restopulse
```

### 2. Run the Stack
Run Docker Compose in detached mode. This builds each service's Docker container on-the-fly and starts the environment:
```bash
docker-compose up --build -d
```

### 3. Verify Container Status
Check if all microservices, RabbitMQ, and the database are running:
```bash
docker-compose ps
```

### 4. Database Seeding & Migrations
All C# microservices are pre-configured to auto-run Entity Framework Core migrations and seed mock/test data at startup. Once all containers show `Up`, you can verify they are functional.

---

## 🛠️ Testing Your Deployment
You can access your live instance at:
```
http://your-instance-ip:7055
```
This serves the Vanilla JS frontend directly from the `GatewayService`'s `wwwroot`. 

### Why Azure SQL Edge is used instead of SQL Server:
Microsoft's standard SQL Server Docker image (`mcr.microsoft.com/mssql/server`) is only compiled for `x86-64` CPU architectures and fails to run on ARM64 nodes. To support OCI Always Free ARM VMs, we use **Azure SQL Edge** (`mcr.microsoft.com/azure-sql-edge`), which is a lightweight, SQL-compatible equivalent built natively for ARM64 architectures.
