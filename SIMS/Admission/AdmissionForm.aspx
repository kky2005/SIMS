<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdmissionForm.aspx.cs" Inherits="SIMS.AdmissionForm" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Admission Application — College Portal</title>
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
        body { background: linear-gradient(135deg, #ea7e66 0%, #a24b76 100%); color:#0f172a; padding:110px 48px 48px 48px; min-height:100vh; display:flex; align-items:flex-start; justify-content:center }
        .adm-card{width:100%;max-width:960px;background:white;border-radius:12px;overflow:hidden;box-shadow:0 30px 60px rgba(0,0,0,0.25)}
        .adm-header{display:flex;align-items:center;gap:16px;padding:32px;background:linear-gradient(90deg,#667eea,#764ba2);color:#fff}
        .crest{width:64px;height:64px;border-radius:12px;background:rgba(255,255,255,0.12);display:flex;align-items:center;justify-content:center;color:#fff;font-size:26px}
        .title h1{margin:0;font-size:20px;font-weight:700}
        .title .muted{opacity:0.95;margin-top:4px}
        .adm-body{padding:32px;background:#fafbff}
        label{display:block;font-weight:600;color:#0f172a;margin-bottom:8px}
        .req{color:#ffeced;margin-left:6px}
        input[type=text], input[type=date], input[type=email], select, .aspNetTextBox { width:100%;padding:12px 14px;border:1px solid #e6eefc;border-radius:10px;font-size:1rem;background:#fff;outline:none;transition:box-shadow .12s,border-color .12s }
        input:focus, select:focus{box-shadow:0 12px 30px rgba(102,126,234,0.12);border-color:#667eea}
        .row{display:grid;gap:16px}
        .row.cols-2{grid-template-columns:1fr 1fr}
        .field-error{color:#c0392b;font-size:0.85rem;margin-top:6px;display:block}
        .alert{padding:12px 14px;border-radius:8px;margin-bottom:12px}
        .alert.success{background:#eaf5f0;color:#0b6b3f;border-left:4px solid #1e7c52}
        .alert.error{background:#fff0f0;color:#9b1d1d;border-left:4px solid #c0392b}
        .btn-submit{background:linear-gradient(90deg,#667eea,#764ba2);color:#fff;padding:12px 18px;border-radius:12px;border:none;font-weight:700}
        @media(max-width:720px){.row.cols-2{grid-template-columns:1fr}.adm-card{padding:12px}}
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
    <form id="admissionForm" runat="server">

        <div class="adm-dashboard">
            <div class="adm-card" style="max-width:820px;margin:28px auto;">
                <header class="adm-header">
                    <div class="crest">🎓</div>
                    <div class="title">
                        <h1>Admission Application</h1>
                        <p class="muted">Fill in your details below to submit an application.</p>
                    </div>
                    <div style="margin-left:auto;">
                        <asp:Button ID="btnBackToDashboard" runat="server" Text="Back to dashboard" CssClass="btn-submit" OnClick="btnBackToDashboard_Click" />
                    </div>
                </header>

                <div class="adm-body">
                    <!-- Alerts -->
                    <asp:ValidationSummary ID="vsErrors" runat="server" ValidationGroup="AdmissionGroup" CssClass="field-error" HeaderText="Please fix the following errors:" DisplayMode="BulletList" EnableClientScript="true" />
                    <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert success" Visible="false">
                        <asp:Label ID="lblSuccess" runat="server" />
                    </asp:Panel>
                    <asp:Panel ID="pnlError" runat="server" CssClass="alert error" Visible="false">
                        <asp:Label ID="lblError" runat="server" />
                    </asp:Panel>

                    <div style="display:grid;grid-template-columns:1fr;gap:12px">
                        <div class="row cols-2">
                            <div class="field">
                                <label for="txtFullName">Full name <span class="req">*</span></label>
                                <asp:TextBox ID="txtFullName" runat="server" placeholder="e.g. Ahmad bin Ali" />
                                <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName" ErrorMessage="Full name is required." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                                <div class="muted" style="font-size:0.85rem;margin-top:6px">Enter your full legal name as on official documents.</div>
                            </div>
                            <div class="field">
                                <label for="txtDateOfBirth">Date of birth <span class="req">*</span></label>
                                <asp:TextBox ID="txtDateOfBirth" runat="server" TextMode="Date" placeholder="YYYY-MM-DD" />
                                <asp:RequiredFieldValidator ID="rfvDOB" runat="server" ControlToValidate="txtDateOfBirth" ErrorMessage="Date of birth is required." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                                <div class="muted" style="font-size:0.85rem;margin-top:6px">You must be at least 16 years old.</div>
                            </div>
                        </div>

                        <div class="row cols-2">
                            <div class="field">
                                <label for="ddlGender">Gender <span class="req">*</span></label>
                                <asp:DropDownList ID="ddlGender" runat="server" ValidationGroup="AdmissionGroup">
                                    <asp:ListItem Value="" Text="— Select gender —" />
                                    <asp:ListItem Value="Male" Text="Male" />
                                    <asp:ListItem Value="Female" Text="Female" />
                                    <asp:ListItem Value="Other" Text="Other" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="rfvGender" runat="server" ControlToValidate="ddlGender" InitialValue="" ErrorMessage="Select gender." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                            </div>
                            <div class="field">
                                <label for="txtNationalId">National ID <span class="req">*</span></label>
                                <asp:TextBox ID="txtNationalId" runat="server" placeholder="e.g. 123456789101" />
                                <asp:RequiredFieldValidator ID="rfvNationalId" runat="server" ControlToValidate="txtNationalId" ErrorMessage="National ID is required." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                                <div class="muted" style="font-size:0.85rem;margin-top:6px">Enter the national/identity card number shown on your ID document.</div>
                            </div>
                        </div>

                        <div class="row cols-2">
                            <div class="field">
                                <label for="txtNationality">Nationality <span class="req">*</span></label>
                                <asp:TextBox ID="txtNationality" runat="server" placeholder="e.g. Malaysian" />
                                <asp:RequiredFieldValidator ID="rfvNationality" runat="server" ControlToValidate="txtNationality" ErrorMessage="Nationality is required." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                            </div>
                            <div class="field">
                                <label for="txtPhoneNumber">Phone number <span class="req">*</span></label>
                                <asp:TextBox ID="txtPhoneNumber" runat="server" placeholder="e.g. 0123456789" />
                                <asp:RequiredFieldValidator ID="rfvPhone" runat="server" ControlToValidate="txtPhoneNumber" ErrorMessage="Phone number is required." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                                <div class="muted" style="font-size:0.85rem;margin-top:6px">Include country code for SMS notifications.</div>
                            </div>
                        </div>

                        <div class="field">
                            <label for="txtPreviousInstitution">Previous institution <span class="req">*</span></label>
                            <asp:TextBox ID="txtPreviousInstitution" runat="server" placeholder="e.g. ABC College" />
                            <asp:RequiredFieldValidator ID="rfvPreviousInstitution" runat="server" ControlToValidate="txtPreviousInstitution" ErrorMessage="Previous institution is required." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                        </div>

                        <div class="row cols-2">
                            <div class="field">
                                <label for="txtHighestQualification">Highest qualification <span class="req">*</span></label>
                                <asp:TextBox ID="txtHighestQualification" runat="server" placeholder="e.g. Diploma in IT" />
                                <asp:RequiredFieldValidator ID="rfvHighestQualification" runat="server" ControlToValidate="txtHighestQualification" ErrorMessage="Highest qualification is required." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                            </div>
                            <div class="field">
                                <label for="txtPreviousCGPA">Previous CGPA</label>
                                <asp:TextBox ID="txtPreviousCGPA" runat="server" placeholder="e.g. 3.20" />
                                <div class="muted" style="font-size:0.85rem;margin-top:6px">Optional – leave blank if not applicable.</div>
                            </div>
                        </div>
                        <div class="row cols-2">
                            <div class="field">
                                <label for="ddlCourse">Course <span class="req">*</span></label>
                                <asp:DropDownList ID="ddlCourse" runat="server" ValidationGroup="AdmissionGroup">
                                    <asp:ListItem Value="" Text="— Select a course —" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="rfvCourse" runat="server" ControlToValidate="ddlCourse" InitialValue="" ErrorMessage="Please select a course." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                            </div>
                            <div class="field">
                                <label for="ddlIntakeYear">Intake Year <span class="req">*</span></label>
                                <asp:DropDownList ID="ddlIntakeYear" runat="server" ValidationGroup="AdmissionGroup">
                                    <asp:ListItem Value="" Text="— Select year —" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="rfvIntakeYear" runat="server" ControlToValidate="ddlIntakeYear" InitialValue="" ErrorMessage="Select intake year." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                            </div>
                        </div>

                        <div class="row cols-2">
                            <div class="field">
                                <label for="ddlIntakeSemester">Intake Semester <span class="req">*</span></label>
                                <asp:DropDownList ID="ddlIntakeSemester" runat="server" ValidationGroup="AdmissionGroup">
                                    <asp:ListItem Value="" Text="— Select semester —" />
                                    <asp:ListItem Value="1" Text="Semester 1 (January)" />
                                    <asp:ListItem Value="2" Text="Semester 2 (May)" />
                                    <asp:ListItem Value="3" Text="Semester 3 (September)" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="rfvIntakeSemester" runat="server" ControlToValidate="ddlIntakeSemester" InitialValue="" ErrorMessage="Select intake semester." CssClass="field-error" Display="Dynamic" ValidationGroup="AdmissionGroup" />
                            </div>
                        </div>

                        <div class="row cols-2">
                            <div class="field">
                                <label>Requested At</label>
                                <asp:Label ID="lblRequestedAt" runat="server" CssClass="muted" />
                            </div>
                            <div class="field" style="display:flex;align-items:flex-end;justify-content:flex-end">
                                <asp:Button ID="btnSubmit" runat="server" Text="Submit Application" CssClass="btn-submit" ValidationGroup="AdmissionGroup" OnClick="btnSubmit_Click" />
                            </div>
                        </div>
                    </div>

                </div>
            </div>
        </div>
    </form>
</body>
</html>
