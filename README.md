# SIMS – Developer Quick-Start Guide
## Role-Based Access Control (RBAC) for Teammates

---

### How the auth system works

```
Browser  →  Login.aspx  →  AuthService.ValidateUser()
                              ↓ (success)
                         Session + FormsAuth cookie set
                              ↓
                         RoleRedirectHelper.RedirectByRole()
                              ↓
             /Admin/Dashboard.aspx  OR
             /Lecturer/Dashboard.aspx  OR
             /Student/Dashboard.aspx
```

---

### ✅ How to protect any new page (ONE LINE)

Add this at the **very top** of your `Page_Load` method, before any other code:

```csharp
// Admin-only page
CurrentUserInfo user = RoleGuard.Require(this, "Admin");

// Lecturer-only page
CurrentUserInfo user = RoleGuard.Require(this, "Lecturer");

// Student-only page
CurrentUserInfo user = RoleGuard.Require(this, "Student");

// Admin OR Lecturer can access
CurrentUserInfo user = RoleGuard.Require(this, "Admin", "Lecturer");

// Any logged-in user
CurrentUserInfo user = RoleGuard.RequireAny(this);
```

`RoleGuard.Require()` will:
1. Check if the user is logged in → redirect to `Login.aspx` if not.
2. Check if their role matches → redirect them to their own dashboard if wrong role.
3. Return a `CurrentUserInfo` object you can use in the page.

---

### CurrentUserInfo properties

```csharp
user.UserId       // int    – database PK
user.FullName     // string – e.g. "Ahmad Faris"
user.Email        // string – e.g. "faris@college.edu.my"
user.Role         // string – "Admin" | "Lecturer" | "Student"
user.ProgrammeId  // int?   – null for Students

// Convenience booleans
user.IsAdmin       // true if Admin
user.IsLecturer    // true if Lecturer
user.IsStudent     // true if Student
```

---

### Example: Lecturer page code-behind

```csharp
// Lecturer/MyCourses.aspx.cs
using SIMS.App_Code;

public partial class Lecturer_MyCourses : System.Web.UI.Page
{
    private CurrentUserInfo _user;

    protected void Page_Load(object sender, EventArgs e)
    {
        // ONE LINE – does everything
        _user = RoleGuard.Require(this, "Lecturer");

        if (!IsPostBack)
            LoadCourses(_user.UserId);
    }

    private void LoadCourses(int lecturerId)
    {
        // your data access here
    }
}
```

---

### Example: Admin page code-behind

```csharp
// Admin/ManageStudents.aspx.cs
using SIMS.App_Code;

public partial class Admin_ManageStudents : System.Web.UI.Page
{
    protected CurrentUserInfo CurrentUser; // expose to ASPX if needed

    protected void Page_Load(object sender, EventArgs e)
    {
        CurrentUser = RoleGuard.Require(this, "Admin");

        if (!IsPostBack)
            LoadStudents();
    }
}
```

---

### Folder structure convention

```
SIMS/
├── Login.aspx                  ← Public (no auth required)
├── ForgotPassword.aspx         ← Public
│
├── Admin/
│   ├── Dashboard.aspx          ← RoleGuard.Require(this, "Admin")
│   ├── ManageStudents.aspx
│   └── Reports.aspx
│
├── Lecturer/
│   ├── Dashboard.aspx          ← RoleGuard.Require(this, "Lecturer")
│   ├── Attendance.aspx
│   └── Grades.aspx
│
├── Student/
│   ├── Dashboard.aspx          ← RoleGuard.Require(this, "Student")
│   ├── MyCourses.aspx
│   └── Results.aspx
│
└── App_Code/
    ├── RoleGuard.cs            ← Auth helper (this file)
    ├── AuthService.cs          ← DB credential verification
    └── ...
```

---

### Shared session values (available anywhere)

```csharp
// Read manually if needed (RoleGuard.Require already does this for you)
int    userId  = (int)Session[SessionKeys.UserId];
string name    = (string)Session[SessionKeys.UserName];
string email   = (string)Session[SessionKeys.UserEmail];
string role    = (string)Session[SessionKeys.UserRole];
int?   progId  = Session[SessionKeys.ProgrammeId] as int?;
```

---

### Database connection string

Edit `Web.config`:
```xml
<add name="SimsDB"
     connectionString="Data Source=YOUR_SERVER;Initial Catalog=SimsDB;Integrated Security=True;"
     providerName="System.Data.SqlClient" />
```

---

### Password hashing (for creating/updating users)

```csharp
// In Admin/ManageUsers.aspx.cs
string hash = AuthService.HashPassword(txtNewPassword.Text);
// Store 'hash' in the Users.PasswordHash column
```

---

### NuGet packages required

| Package | Purpose |
|---------|---------|
| `BCrypt.Net-Next` | Password hashing & verification |

Install via Package Manager Console:
```
Install-Package BCrypt.Net-Next
```
