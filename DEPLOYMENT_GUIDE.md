# GeniView Cloud — New System Deployment Guide

## Prerequisites

Install the following on the new machine **in this exact order** before copying any files.

> **Order matters** — IIS must be enabled before the .NET Hosting Bundle, otherwise the ASP.NET Core Module won't register with IIS correctly.

---

### 1. IIS (Internet Information Services)

1. Open **Start → Turn Windows features on or off**
2. Check **Internet Information Services**
3. Expand **World Wide Web Services → Application Development Features** and also check:
   - **WebSocket Protocol**
4. Click OK and wait for the install to complete
5. Verify by opening a browser and navigating to `http://localhost` — you should see the IIS default page

---

### 2. .NET 8.0 Hosting Bundle

1. Go to: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
2. Download **"Hosting Bundle"** — not SDK, not Runtime
3. Run the installer
4. After install, open Command Prompt as **Administrator** and run:
   ```
   iisreset
   ```

---

### 3. SQL Server

1. Download **SQL Server 2022 Express** (free): https://www.microsoft.com/en-us/sql-server/sql-server-downloads
2. Run the installer and choose **Basic** installation type
3. Default port `1433` is set automatically
4. After install, open **SQL Server Configuration Manager**:
   - Go to `SQL Server Network Configuration → Protocols for MSSQLSERVER`
   - Enable **TCP/IP**
   - Right-click TCP/IP → Properties → IP Addresses tab → set **TCP Port = 1433** on all IPs
   - Restart the SQL Server service

---

### 4. SQL Server Management Studio — SSMS (Recommended)

Used to verify databases and run queries if needed.

1. Download from: https://aka.ms/ssmsfullsetup
2. Install with default options

---

### 5. Mosquitto MQTT Broker

1. Download the **Windows installer** from: https://mosquitto.org/download/
2. Run the installer — check **"Install as a Windows Service"** during setup
3. After install, configure it — see **Step 3** below for full setup

---

| Software                | Version            | Required    |
| ----------------------- | ------------------ | ----------- |
| IIS                     | Built into Windows | Yes         |
| .NET 8.0 Hosting Bundle | 8.x                | Yes         |
| SQL Server              | 2019 or 2022       | Yes         |
| SSMS                    | Latest             | Recommended |
| Mosquitto MQTT          | Latest             | Yes         |

---

## Step 1 — Copy Published Files

Copy the entire contents of the published folder to the new machine.

**Source (your current machine):**

```
E:\uk\net8_deploy\
```

**Destination (new machine) — recommended path:**

```
C:\inetpub\GeniViewCloud\
```

Copy everything inside `net8_deploy` — DLLs, `wwwroot`, `appsettings.json`, `web.config`, etc.

---

## Step 2 — Configure SQL Server

### 2a. Enable SQL Server TCP/IP on port 1433

1. Open **SQL Server Configuration Manager**
2. Go to `SQL Server Network Configuration → Protocols for MSSQLSERVER`
3. Enable **TCP/IP**
4. Right-click TCP/IP → Properties → IP Addresses tab → set **TCP Port = 1433** on all IPs
5. Restart the SQL Server service

### 2b. Create SQL Login

Open SSMS and run:

```sql
CREATE LOGIN sa WITH PASSWORD = 'password', CHECK_POLICY = OFF;
ALTER SERVER ROLE sysadmin ADD MEMBER sa;
ALTER LOGIN sa ENABLE;
```

> Or use a different username/password and update `appsettings.json` accordingly.

### 2c. Create the three databases

```sql
CREATE DATABASE GeniViewCloudIdentityRepository;
CREATE DATABASE GeniViewCloudDataRepository;
CREATE DATABASE GeniViewCloudHangfire;
```

> **Tables are created automatically** by EF Core migrations when the app starts for the first time. You do not need to run any SQL scripts.

---

## Step 3 — Configure MQTT Broker (Mosquitto)

### 3a. Install Mosquitto

Download and install from https://mosquitto.org/download/

### 3b. Create Mosquitto config

Create or edit `C:\Program Files\mosquitto\mosquitto.conf`:

```
listener 1883
allow_anonymous false
password_file C:\Program Files\mosquitto\passwd.txt
```

### 3c. Create the MQTT user

Open Command Prompt as Administrator:

```
cd "C:\Program Files\mosquitto"
mosquitto_passwd -c passwd.txt geniviewuser
```

When prompted, enter password: `G3niview!@#?`

### 3d. Install Mosquitto as a Windows Service

```
mosquitto install
net start mosquitto
```

---

## Step 4 — Update appsettings.json

Open `C:\inetpub\GeniViewCloud\appsettings.json` on the **new machine** and update:

```json
{
  "ConnectionStrings": {
    "GeniViewCloudIdentityRepository": "Server=localhost,1433;Database=GeniViewCloudIdentityRepository;User Id=sa;Password=passowrd;TrustServerCertificate=True;",
    "GeniViewCloudDataRepository": "Server=localhost,1433;Database=GeniViewCloudDataRepository;User Id=sa;Password=passowrd;TrustServerCertificate=True;",
    "GeniViewCloudHangfireRepository": "Server=localhost,1433;Database=GeniViewCloudHangfire;User Id=sa;Password=passowrd;TrustServerCertificate=True;"
  },
  "AppSettings": {
    "MQTTBroker": "localhost",
    "MQTTPort": 1883,
    "MQTTClientId": "genicloud",
    "MQTTUser": "geniviewuser",
    "MQTTPSW": "password"
  }
}
```

> If the new system uses a different SQL Server address, SA password, or MQTT credentials — update them here. All other settings can stay the same.

---

## Step 5 — Set Up IIS

### 5a. Create Application Pool

1. Open **IIS Manager**
2. Right-click **Application Pools** → Add Application Pool
   - Name: `GeniViewCloud`
   - .NET CLR version: **No Managed Code**
   - Managed pipeline mode: **Integrated**
3. Click OK
4. Right-click the new pool → **Advanced Settings**
   - Identity: `LocalSystem` (or a Windows account with SQL Server access)
   - Start Mode: `AlwaysRunning`
   - Idle Time-out: `0`

### 5b. Create Website

1. Right-click **Sites** → Add Website
   - Site name: `GeniViewCloud`
   - Application pool: `GeniViewCloud` (created above)
   - Physical path: `C:\inetpub\GeniViewCloud`
   - Binding: HTTP, port `80` (or your preferred port)
2. Click OK

### 5c. Set folder permissions

Right-click `C:\inetpub\GeniViewCloud` → Properties → Security → Edit → Add:

- `IIS AppPool\GeniViewCloud` — give **Full Control**
- `IUSR` — give **Read & Execute**

---

## Step 6 — Create Logs Folder

The app writes startup logs to a `logs` folder. Create it manually:

```
mkdir C:\inetpub\GeniViewCloud\logs
```

---

## Step 7 — Firewall Rules

Open the following ports if the system has a firewall:

| Port                       | Purpose          |
| -------------------------- | ---------------- |
| `80` (or your chosen port) | IIS — web access |
| `1433`                     | SQL Server       |
| `1883`                     | MQTT Broker      |

Using PowerShell (run as Administrator):

```powershell
New-NetFirewallRule -DisplayName "GeniView HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
New-NetFirewallRule -DisplayName "GeniView MQTT" -Direction Inbound -Protocol TCP -LocalPort 1883 -Action Allow
New-NetFirewallRule -DisplayName "GeniView SQL" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

---

## Step 8 — Start & Verify

1. In IIS Manager, click your `GeniViewCloud` site → **Start**
2. Open a browser and navigate to `http://localhost` (or `http://<server-ip>`)
3. The login page should appear

**If the site does not start**, check:

- `C:\inetpub\GeniViewCloud\logs\stdout*.log` for errors
- Windows Event Viewer → Windows Logs → Application

---

## Step 9 — Future Publishes

When deploying an update from the development machine:

1. In Visual Studio / VS Code, publish with target: `E:\uk\net8_deploy` (or copy to new machine path)
2. Stop the IIS site before copying (to release file locks):
   ```
   iisreset /stop
   ```
3. Copy updated files to `C:\inetpub\GeniViewCloud\`
4. Restart IIS:
   ```
   iisreset /start
   ```
5. Hard refresh the browser: `Ctrl + Shift + R`

> Do **not** overwrite `appsettings.json` if the new machine has different connection strings or MQTT settings.

---

## Quick Checklist

- [ ] .NET 8.0 Hosting Bundle installed + IIS restarted
- [ ] Published files copied to `C:\inetpub\GeniViewCloud\`
- [ ] SQL Server running on port 1433, three databases created
- [ ] `sa` login enabled in SQL Server
- [ ] Mosquitto running as a service with `geniviewuser` account
- [ ] `appsettings.json` updated with correct server/credentials
- [ ] IIS Application Pool created (No Managed Code)
- [ ] IIS Website created pointing to deploy folder
- [ ] Folder permissions set for `IIS AppPool\GeniViewCloud`
- [ ] `logs` folder created
- [ ] Firewall ports open: 80, 1433, 1883
- [ ] Site starts and login page loads in browser
