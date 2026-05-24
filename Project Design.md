# Project Design Update
Date: 2026-05-14

## Current Architecture

System split into 2 MVC applications:

### 1. HDMC.Portal
Purpose:
- Login
- Company selection
- User management
- Central access control

### 2. HDMC.HardwareMinAlarm
Purpose:
- Scan part
- Hardware min alarm workflow
- Status transaction
- Upload item master
- Upload history

Repository:
- GitHub monorepo created
- Repo: hdmc-portal-system
- Structure:

HDMC.System
│
├── HDMC.Portal
│   └── HDMC.Portal.sln
│
├── HDMC.HardwareMinAlarm
│   └── HDMC.HardwareMinAlarm.sln

---

# Database Progress

## Shared Auth DB

Completed:
- Users
- Roles
- User_Role
- User_Company
- Companies

Company table added:
- 3047
- 3048
- 3049

FK:
- User_Company.company
→ Companies.company_code

---

## Hardware DB

Using existing:
- Input_hardware
- Item_master

Added:
- Item_Master_Import_Log

Purpose:
- Track upload history
- File name
- Success/fail rows
- Uploaded date

---

## Auth + Portal

Partially Completed

Completed:
- Login page
- Company selection
- Redirect Portal → Hardware
- Session bridge using query string
- BaseController auth check

Temporary workaround:
- Password validation currently bypassed
- Hardcoded password logic still exists

Current flow:

Portal Login
→ Company Select
→ Hardware Entry
→ Scan page

Remaining:
- Fix BCrypt verification
- Remove temporary password bypass

---

## Hardware Scan Flow

Completed:
- Scan part
- Search from Item_master
- Latest transaction lookup from Input_hardware
- Save status
- Insert/update transaction
- Insert log
- Reset workflow after status 900

Business logic:

If latest status != 900
→ UPDATE latest transaction

If latest status == 900
→ INSERT new transaction

---

## Current Status UI

Updated UX:

Show:
- Current Status (status code)
- From Location (human readable)
- Move To Location (dropdown)

Example:
- Current Status: 600
- From Location: ETA Today

If latest status = 900:
- Current Status = blank
- From Location = blank

---

## Upload Module

Completed:
- UploadController
- UploadService
- Upload Item Master page
- Excel import using ClosedXML
- Insert/update Item_master
- Validation
- Upload history logging

Validation:
- Empty company
- Empty part
- Invalid company

Upload history page completed.

---

## User Management

Completed:
- User list
- Create user
- Company assignment
- Role assignment

Current roles:
- Admin IT
- Super User
- User

---

# Important Technical Decisions

## Item_master is Source of Truth

Search flow:
1. Check Item_master first
2. Then get latest transaction

Reason:
- New part may not exist in transaction table yet

---

## Companies Hardcoded in DB Only

No enum/static company list in code.

Reason:
- Future company expansion
- Better maintenance

---

## Status Mapping Hardcoded in Controller/Service

Current status mapping:
100 → Request Replenish
200 → Kanban from MFG
300 → Stock min <10 days
400 → Location discrepancy
500 → Check Shipment ETA
600 → ETA Today
900 → Replenish Completed

Reason:
- Fixed business rule
- Avoid overengineering

---

# Remaining MUST DO

## 1. Fix Password Hash

Current:
- Temporary plain password logic

Need:
- BCrypt verification
- Remove hardcoded bypass

Priority:
HIGH

---

## 2. Session Stability Review

Current:
- Session bridge using query string

Need:
- Re-test session expiration
- Validate multi-tab behavior

Priority:
MEDIUM

---

## 3. IIS Publish Test

Need:
- Publish both projects to IIS
- Validate routing
- Validate Url.Action paths
- Validate virtual directory support

Priority:
HIGH

---

## 4. UI Polish

Need:
- Better spacing
- Scan-first UX
- Mobile/TC58 readability
- Loading state
- Prevent double submit

Priority:
MEDIUM

---

## 5. GitHub + MacBook Workflow

Completed:
- GitHub repo created
- Initial push success

Next:
- Clone on MacBook
- Use VS Code + Codex for review/refactor workflow

---

# Important Lessons Learned

- Business logic is more important than syntax
- Transaction history must not be overwritten
- Item master != transaction table
- UX naming matters for operators
- Avoid overengineering
- Deployment path issues must be considered early
- Use Url.Action instead of hardcoded paths