# Multi-Tenant Authentication Configuration Guide

## Azure Entra ID — Cross-Tenant Access with App Roles & Admin Consent

---

## Overview

This guide walks through configuring a multi-tenant application in Microsoft Entra ID (Azure AD) that allows users from **Tenant 2** (customer/partner) to access an application registered in **Tenant 1** (app owner) — without requiring B2B guest user accounts or individual user consent.

### Architecture

```
┌──────────────────────────────────────┐       ┌──────────────────────────────────────┐
│           TENANT 1 (App Owner)       │       │         TENANT 2 (Customer)          │
│                                      │       │                                      │
│  ┌────────────────────────────────┐  │       │  ┌────────────────────────────────┐  │
│  │      App Registration          │  │       │  │     Enterprise Application     │  │
│  │                                │  │       │  │     (Service Principal)        │  │
│  │  • Multi-tenant enabled        │  │◄──────│  │                                │  │
│  │  • App Roles defined           │  │       │  │  • Admin consent granted       │  │
│  │  • API permissions declared    │  │       │  │  • Security groups assigned    │  │
│  └────────────────────────────────┘  │       │  │    to App Roles                │  │
│                                      │       │  └────────────────────────────────┘  │
└──────────────────────────────────────┘       └──────────────────────────────────────┘
                    │
                    ▼
        ┌───────────────────────┐
        │    Demo Application   │
        │    (Angular SPA)      │
        │                       │
        │  • MSAL Auth          │
        │  • Role-based access  │
        │  • No user consent    │
        └───────────────────────┘
```

### Key Decisions

| Aspect | Choice |
|--------|--------|
| Tenant topology | Multiple Entra ID Tenants |
| Authorization model | App Roles (role-based) |
| Consent model | Tenant 2 Admin grants consent for all users |
| Guest users required | No |

---

## Part 1: Tenant 1 Configuration (App Owner)

> **Who performs these steps:** Administrator of Tenant 1 (the organization that owns/deploys the application)

### Step 1.1 — Register the Application

1. Sign in to [Azure Portal](https://portal.azure.com) as a Tenant 1 administrator
2. Navigate to **Microsoft Entra ID** → **App registrations** → **New registration**
3. Configure:
   - **Name:** `<Your Application Name>`
   - **Supported account types:** Select **"Accounts in any organizational directory (Any Microsoft Entra ID tenant - Multitenant)"**
   - **Redirect URI:**
     - Platform: **Single-page application (SPA)**
     - URI: `http://localhost:4200` (for development) or your production URL
4. Click **Register**
5. **Copy and save** the **Application (client) ID** — you'll need this later

### Step 1.2 — Configure API Permissions

1. In the app registration, go to **API permissions**
2. Click **Add a permission** → **Microsoft Graph** → **Delegated permissions**
3. Add the following permissions:
   - `User.Read` (Sign in and read user profile)
   - Add any additional permissions your app needs
4. Click **Grant admin consent for [Tenant 1]** (optional — this is for your own tenant)

### Step 1.3 — Create App Roles

App Roles define what level of access users/groups can be assigned to.

1. Go to **App roles** → **Create app role**
2. Create the following role(s):

   | Field | Value |
   |-------|-------|
   | Display name | `Application User` |
   | Allowed member types | **Users/Groups** |
   | Value | `App.User` |
   | Description | `Standard access to the application` |
   | Enable this app role | ✅ Checked |

3. Click **Apply**
4. (Optional) Create additional roles for different access levels:

   | Display name | Value | Description |
   |-------------|-------|-------------|
   | Application Admin | `App.Admin` | Administrative access |
   | Application Reader | `App.Reader` | Read-only access |

### Step 1.4 — Configure Token Settings

1. Go to **Token configuration** → **Add optional claim**
2. Token type: **ID**
3. Select: `email`, `preferred_username`
4. Click **Add**

> **Note:** App Role assignments are automatically included in the `roles` claim of the ID token — no additional token configuration is needed for roles.

### Step 1.5 — Configure Authentication Settings

1. Go to **Authentication**
2. Under **Single-page application** redirect URIs, ensure your URIs are listed:
   - `http://localhost:4200` (development)
   - `https://your-production-url.com` (production)
3. Under **Implicit grant and hybrid flows:**
   - Leave both checkboxes **UNCHECKED** (the app uses Auth Code Flow with PKCE)
4. Under **Supported account types:**
   - Confirm **"Accounts in any organizational directory"** is selected
5. Click **Save**

### Step 1.6 — Note Values for Tenant 2

Provide the following to the Tenant 2 administrator:

| Item | Value | Where to find |
|------|-------|---------------|
| Application (Client) ID | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` | App Registration → Overview |
| App Role Values | `App.User`, `App.Admin`, etc. | App Registration → App roles |
| Admin Consent URL | See Step 2.1 below | Constructed URL |

---

## Part 2: Tenant 2 Configuration (Customer)

> **Who performs these steps:** Administrator of Tenant 2 (the customer organization whose users need access)

### Step 2.1 — Grant Admin Consent for All Users

The Tenant 2 administrator grants consent on behalf of all users so that **no individual user consent is required**.

**Option A: Using the Admin Consent URL (Recommended)**

Open the following URL in a browser while signed in as a **Tenant 2 Global Administrator or Cloud Application Administrator**:

```
https://login.microsoftonline.com/{TENANT_2_TENANT_ID}/adminconsent?client_id={APPLICATION_CLIENT_ID}&redirect_uri={REDIRECT_URI}
```

**Example:**
```
https://login.microsoftonline.com/contoso.onmicrosoft.com/adminconsent?client_id=af13a2cf-5834-412b-9aef-13ecaa345760&redirect_uri=http://localhost:4200
```

> Replace `{TENANT_2_TENANT_ID}` with Tenant 2's tenant ID or primary domain, and `{APPLICATION_CLIENT_ID}` with the Client ID from Step 1.1.

When prompted:
1. Review the permissions being requested
2. Click **Accept**
3. You'll be redirected to the redirect URI — this confirms consent was granted

**Option B: Using Azure Portal**

1. Sign in to [Azure Portal](https://portal.azure.com) as a Tenant 2 administrator
2. Navigate to **Microsoft Entra ID** → **Enterprise applications**
3. Search for the application by Client ID or name
4. Go to **Permissions** → **Grant admin consent for [Tenant 2]**
5. Click **Yes** to confirm

### Step 2.2 — Verify the Enterprise Application Exists

After admin consent:
1. Go to **Microsoft Entra ID** → **Enterprise applications**
2. Search for the application name or Client ID
3. Confirm it appears with **Application type: Enterprise Application**

### Step 2.3 — Assign Entra ID Security Groups to App Roles

This step controls **which users from Tenant 2 can access the application** and what role they receive.

1. In **Enterprise applications**, click on the application
2. Go to **Users and groups** → **Add user/group**
3. Under **Users and groups**, click **None selected**
4. Search for and select the **Entra ID Security Group** that should have access
   - Example: `App-MultiTenant-Users` (a security group in Tenant 2)
5. Under **Select a role**, click **None selected**
6. Choose the appropriate role: `Application User`
7. Click **Assign**

Repeat for additional groups/roles as needed:

| Group | Assigned Role |
|-------|---------------|
| `App-MultiTenant-Users` | Application User |
| `App-MultiTenant-Admins` | Application Admin |

> **Important:** Only users who are members of the assigned groups will receive the `roles` claim in their token. Users not in any assigned group will authenticate but will NOT have the role — your application should deny access.

### Step 2.4 — (Optional) Restrict User Assignment

To ensure that **only** assigned users/groups can access the app:

1. In the Enterprise application, go to **Properties**
2. Set **"Assignment required?"** to **Yes**
3. Click **Save**

> When enabled, users not assigned (directly or via group) will receive an error: `AADSTS50105: The signed in user is not assigned to a role for the application.`

---

## Part 3: Verification & Testing

### Step 3.1 — Test with a Tenant 2 User in the Assigned Group

1. Open the application URL (e.g., `http://localhost:4200`)
2. Click **Login**
3. Sign in with a Tenant 2 user who **is** a member of an assigned group
4. Expected result:
   - User signs in **without** any consent prompt
   - User is redirected back to the application
   - Profile page shows the user's `tid` (Tenant 2 ID) and `roles` claim containing `App.User`

### Step 3.2 — Test with a Tenant 2 User NOT in the Assigned Group

1. Sign in with a Tenant 2 user who is **not** in any assigned group
2. Expected result:
   - If "Assignment required" is **Yes**: User gets `AADSTS50105` error
   - If "Assignment required" is **No**: User signs in but `roles` claim is empty — application shows "Access Denied"

### Step 3.3 — Verify Token Claims

After successful login, the ID token should contain:

```json
{
  "aud": "af13a2cf-5834-412b-9aef-13ecaa345760",
  "iss": "https://login.microsoftonline.com/{TENANT_2_ID}/v2.0",
  "tid": "{TENANT_2_TENANT_ID}",
  "name": "Jane Doe",
  "preferred_username": "jane@contoso.com",
  "roles": [
    "App.User"
  ]
}
```

Key claims to verify:
- `tid` — Should be Tenant 2's tenant ID (confirms cross-tenant auth)
- `roles` — Should contain the assigned app role(s)
- `iss` — Issuer includes Tenant 2's ID

---

## Part 4: Ongoing Management

### Adding New Users

Users are managed entirely through **Entra ID Security Groups in Tenant 2**:
- To grant access: Add user to the assigned security group
- To revoke access: Remove user from the group
- No changes needed in Tenant 1 or the application

### Adding a New Tenant (Tenant 3, 4, etc.)

Repeat **Part 2** for each new tenant:
1. Tenant admin grants admin consent (Step 2.1)
2. Tenant admin assigns groups to roles (Step 2.3)
3. Users can sign in immediately

### Revoking Tenant Access

To revoke an entire tenant's access:
1. The Tenant admin can delete the Enterprise application from their tenant
2. Or: The App owner (Tenant 1) can implement tenant-allowlist logic in the application

### Modifying Roles

If new roles are added to the App Registration (Tenant 1):
1. Add the role in **App Registration → App roles**
2. Tenant 2 admin assigns groups to the new role in **Enterprise applications → Users and groups**

---

## Part 5: Key Application Configuration (For Developers)

> The full working source code is provided in the `multi-tenant-app/` project. Below are the **3 critical configuration points** that make multi-tenant + role-based auth work.

### 5.1 — MSAL Authority Must Use `/organizations`

This single setting is what enables multi-tenant login. The authority must **not** contain a specific tenant ID.

```typescript
// In environment.ts
authority: 'https://login.microsoftonline.com/organizations'
```

| Authority Value | Behavior |
|----------------|----------|
| `/organizations` | Any Azure AD tenant can sign in ✅ |
| `/common` | Any Azure AD + personal Microsoft accounts |
| `/{tenant-id}` | Only that specific tenant (single-tenant) ❌ |

### 5.2 — Authorized Roles Must Match App Registration

The application checks the `roles` claim in the ID token. These values must exactly match the **App Role Values** created in Step 1.3:

```typescript
// In environment.ts
authorizedRoles: ['App.User', 'App.Admin']
```

When a user from Tenant 2 signs in:
- If their group is assigned to the `App.User` role → token contains `"roles": ["App.User"]` → access granted
- If their group is NOT assigned → token has no `roles` claim → access denied

### 5.3 — The App Reads the `roles` Claim from the ID Token

The application's route guard inspects the token to enforce access:

```typescript
// Simplified logic in the route guard
const claims = account.idTokenClaims;
const userRoles = claims['roles'];  // e.g., ["App.User"]

const allowed = authorizedRoles.some(role => userRoles.includes(role));
if (!allowed) {
  // Redirect to "Access Denied" page
}
```

This means:
- **Authentication** is handled by Entra ID (MSAL redirects to Microsoft login)
- **Authorization** is handled by the app (checking the `roles` claim)
- **Role assignment** is managed by the Tenant 2 admin (assigning groups in Enterprise Apps)

### Summary: What Each Party Controls

| Responsibility | Who | Where |
|---------------|-----|-------|
| Define available roles | App Owner (Tenant 1) | App Registration → App roles |
| Set authority to `/organizations` | Developer | Application config |
| Check `roles` claim in code | Developer | Route guard |
| Grant admin consent | Customer Admin (Tenant 2) | Admin consent URL |
| Assign groups to roles | Customer Admin (Tenant 2) | Enterprise Applications → Users and groups |
| Add/remove users from groups | Customer Admin (Tenant 2) | Entra ID → Groups |

---

## Appendix A: Required Azure AD Roles

| Action | Required Role (Tenant) |
|--------|----------------------|
| Register multi-tenant app | Application Administrator or Global Administrator (Tenant 1) |
| Grant admin consent | Cloud Application Administrator or Global Administrator (Tenant 2) |
| Assign groups to app roles | Cloud Application Administrator or Application Administrator (Tenant 2) |
| Manage security groups | Groups Administrator or User Administrator (Tenant 2) |

## Appendix B: Troubleshooting

| Error Code | Meaning | Resolution |
|-----------|---------|------------|
| AADSTS50011 | Redirect URI mismatch | Ensure `http://localhost:4200` is registered as a SPA redirect URI |
| AADSTS700016 | Application not found | Verify the Client ID in `environment.ts` |
| AADSTS65001 | Consent not granted | Tenant 2 admin must grant admin consent (Step 2.1) |
| AADSTS50105 | User not assigned to a role | Add user's group to the app role assignment (Step 2.3) |
| AADSTS650051 | Service principal conflict | Delete existing enterprise app in the tenant and re-consent |
| No `roles` claim in token | Group not assigned to role | Complete Step 2.3 — assign group to app role |
| "Access Denied" page | User authenticated but lacks required role | Assign user's group to the correct role in Enterprise Apps |

---
