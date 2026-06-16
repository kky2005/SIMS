<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="AdmissionDashboard.aspx.cs"
    Inherits="SIMS.AdmissionDashboard" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Admission Dashboard</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        .navbar {
    background: linear-gradient(90deg, #1f2937, #111827);
}

.navbar .nav-link {
    color: #e5e7eb !important;
    margin-right: 8px;
    transition: 0.2s;
}

.navbar .nav-link:hover {
    color: #ffffff !important;
}

.navbar-brand {
    color: #fff !important;
}
        html, body { height: 100%; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }
        .navbar.fixed-top { position: fixed; top: 0; left: 0; right: 0; z-index: 1050; }
        body { background: linear-gradient(135deg, #ea7e66 0%, #a24b76 100%); display: flex; align-items: center; justify-content: center; padding: 110px 48px 48px 48px; min-height: 100vh; }
        .adm-card { width: 100%; max-width: 980px; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 30px 60px rgba(0,0,0,0.25); }
        .adm-header { display:flex; align-items:center; gap:18px; padding:36px; background: linear-gradient(90deg,#667eea,#764ba2); color:#fff; }
        .crest{width:64px;height:64px;border-radius:12px;background:rgba(255,255,255,0.12);display:flex;align-items:center;justify-content:center;color:#fff;font-size:26px}
        .title h1{margin:0;font-size:22px;font-weight:700}
        .title .muted{opacity:0.9;font-weight:500}
        .actions{margin-left:auto}
        .btn-logout{background:transparent;border:1px solid rgba(255,255,255,0.2);color:#fff;padding:8px 14px;border-radius:10px}
        .adm-body{padding:40px;background:#fafbff}
        .adm-actions{display:flex;gap:20px;flex-wrap:wrap;justify-content:flex-start}
        .adm-card-btn{display:flex;gap:18px;align-items:center;padding:22px;border-radius:12px;text-decoration:none;background:#fff;border:1px solid #eef2ff;min-width:260px;box-shadow:0 12px 30px rgba(12,18,28,0.06);color:#0f172a}
        .adm-card-btn .icon{font-size:26px}
        .adm-card-btn .label{font-weight:700}
        .adm-card-btn .hint{font-size:0.95rem}
        .adm-card-btn.primary{background:linear-gradient(90deg,#667eea,#764ba2);color:#fff;border:none}
        .adm-card-btn.primary .hint{color:#A7BFC9;opacity:0.95}
        @media (max-width:720px){ .adm-actions{flex-direction:column} .adm-header{padding:18px} .adm-body{padding:18px} }
    </style>
</head>
<body>
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark px-3 fixed-top">
    <a class="navbar-brand fw-bold" href="AdmissionDashboard.aspx">
        🎓 SIMS Admission
    </a>

    <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
        <span class="navbar-toggler-icon"></span>
    </button>

    <div class="collapse navbar-collapse" id="navbarNav">
        <ul class="navbar-nav ms-auto">

            <li class="nav-item">
                <a class="nav-link" href="AdmissionDashboard.aspx">Dashboard</a>
            </li>

            <li class="nav-item">
                <a class="nav-link" href="AdmissionForm.aspx">Apply</a>
            </li>

            <li class="nav-item">
                <a class="nav-link" href="AdmissionChecking.aspx">Check Status</a>
            </li>

        </ul>
    </div>
</nav>
    <form id="form1" runat="server">
        <div class="adm-dashboard">
            <div class="adm-card">
                <header class="adm-header">
                    <div class="crest">🎓</div>
                    <div class="title">
                        <h1>Admission Dashboard</h1>
                        <p class="muted">Official Admission Site</p>
                    </div>
                    <div class="actions">
                        <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="btn-logout" OnClick="btnLogout_Click" />
                    </div>
                </header>

                <div class="adm-body">
                    <div class="adm-actions">
                        <a class="adm-card-btn primary" href="AdmissionForm.aspx">
                            <div class="icon">📄</div>
                            <div class="meta">
                                <div class="label">Applications</div>
                                <div class="hint">Write applications</div>
                            </div>
                        </a>

                        <a class="adm-card-btn secondary" href="AdmissionChecking.aspx">
                            <div class="icon">👥</div>
                            <div class="meta">
                                <div class="label">Checking</div>
                                <div class="hint" style="color:#475569">Check admission status</div>
                            </div>
                        </a>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
