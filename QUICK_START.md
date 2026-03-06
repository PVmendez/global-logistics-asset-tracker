# ⚡ GLAT Quick Start


## Prerequisites Check

Before starting, verify you have:

```bash
# Check if Docker is installed and running
docker --version
docker-compose --version

# Ensure ports 3000, 5000, 1433 are available
# (Check nothing else is running on these ports)
```

✅ **All good?** Continue below.

---

## 3-Step Deployment

### 1️⃣ Navigate to Project

```bash
cd /GLAT
```

### 2️⃣ Start the System

```bash
docker-compose up --build
```

**Wait for output like**:
```
api    | ... Application started. Press Ctrl+C to shut down.
web    | ✓ Ready in 2s
```

⏱️ **Takes ~3-5 minutes on first run** (building containers)

### 3️⃣ Access the System

| Service | URL | Credentials |
|---------|-----|-------------|
| **Frontend** | http://localhost:3000 | demo / demo123 |
| **API Docs** | http://localhost:5000/swagger | — |
| **Database** | localhost:1433 | sa / P@ssw0rd123! |

---

## Demo Credentials

```
Username: demo
Password: demo123
```

These are automatically created on first run.

---

## Stop Everything

When you're done testing:

```bash
docker-compose down

# To delete all data (not needed for next test run):
docker-compose down -v
```

---

**Version**: 1.0  
**Last Updated**: March 3, 2026  