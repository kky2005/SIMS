<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdmissionChecking.aspx.cs" Inherits="SIMS.AdmissionChecking" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Check Admission Status</title>
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
        body { background: linear-gradient(135deg, #ea7e66 0%, #a24b76 100%); display:flex; align-items:center; justify-content:center; padding:110px 48px 48px 48px; min-height:100vh; }
        .card-check{width:100%;max-width:920px;background:#fff;border-radius:12px;padding:28px;box-shadow:0 30px 60px rgba(0,0,0,0.25);overflow:hidden}
        .card-head{display:flex;align-items:center;gap:14px;padding:22px;background:linear-gradient(90deg,#667eea,#764ba2);color:#fff;border-radius:8px}
        .crest{width:48px;height:48px;border-radius:10px;background:rgba(255,255,255,0.12);display:flex;align-items:center;justify-content:center}
        .status-badge{display:inline-block;padding:8px 12px;border-radius:999px;font-weight:700}
        .status-pending{background:#fff7ed;color:#b9641a;border:1px solid #f5e0c9}
        .status-admitted{background:#ecfdf5;color:#065f46;border:1px solid #d1fae5}
        .status-rejected{background:#fff0f0;color:#7f1d1d;border:1px solid #fce7e7}
        .meta-row{display:flex;gap:18px;align-items:center;margin-top:18px}
        .muted{color:#0f172a;opacity:0.75}
        .btn-refresh{background:linear-gradient(90deg,#667eea,#764ba2);color:#fff;border:none;padding:10px 16px;border-radius:10px}
        @media(max-width:720px){body{padding:18px}.meta-row{flex-direction:column;align-items:flex-start}}
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
    <form runat="server">
        <div class="card-check">
            <h2>Admission Status</h2>
            <p class="muted">View the current status of your admission application.</p>

            <asp:Panel ID="pnlStatus" runat="server" Visible="false">
                <div>
                    <div><strong>Admission ID:</strong> <asp:Label ID="lblAdmissionId" runat="server" /></div>
                    <div style="margin-top:8px"><strong>Status:</strong> <asp:Label ID="lblStatus" runat="server" CssClass="status-badge" /></div>
                    <div class="meta-row">
                        <div><strong>Requested:</strong> <asp:Label ID="lblRequestedAt" runat="server" /></div>
                        <div><strong>Admitted:</strong> <asp:Label ID="lblAdmittedAt" runat="server" /></div>
                        <div><strong>Rejected:</strong> <asp:Label ID="lblRejectedAt" runat="server" /></div>
                    </div>
                    <div style="margin-top:14px">
                        <asp:Button ID="btnRefresh" runat="server" CssClass="btn-refresh" Text="Refresh" OnClick="btnRefresh_Click" />
                        <asp:Button ID="btnBack" runat="server" Text="Back to Dashboard" CssClass="btn-refresh" OnClick="btnBack_Click" />
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlNoApplication" runat="server" Visible="false" CssClass="mt-3" Style="padding-top:12px">
                <div class="alert alert-info" role="alert">
                    <strong>No application found.</strong>
                    <div style="margin-top:8px">You do not currently have an admission application on record. If you would like to apply, click the button below to open the application form. You will be asked to confirm before being redirected.</div>
                </div>
                <div style="margin-top:8px">
                    <asp:Button ID="btnApplyNow" runat="server" CssClass="btn-refresh" Text="Apply now" OnClick="btnApplyNow_Click" OnClientClick="return confirm('You will be redirected to the application form. Continue?');" />
                    <asp:Button ID="btnBackIfNone" runat="server" Text="Back to Dashboard" CssClass="btn-refresh" OnClick="btnBack_Click" />
                </div>
            </asp:Panel>
        </div>
    </form>
</body>
</html>
