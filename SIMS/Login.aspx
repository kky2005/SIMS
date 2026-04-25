<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SIMS.Login" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>SIMS &mdash; Student Information Management System</title>
    <link href="https://fonts.googleapis.com/css2?family=DM+Serif+Display:ital@0;1&family=DM+Sans:opsz,wght@9..40,300;9..40,400;9..40,500;9..40,600&display=swap" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        :root {
            --navy:       #0d1b2a;
            --navy-mid:   #1b2d42;
            --teal:       #17b8a6;
            --teal-dark:  #0fa898;
            --gold:       #f0c040;
            --text-light: #c8d6e5;
            --text-muted: #7a93ac;
            --border:     rgba(255,255,255,0.08);
        }
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

        body {
            min-height: 100vh;
            background-color: var(--navy);
            background-image:
                radial-gradient(ellipse 80% 60% at 10% 20%, rgba(23,184,166,0.12) 0%, transparent 60%),
                radial-gradient(ellipse 60% 50% at 90% 80%, rgba(240,192,64,0.07) 0%, transparent 55%),
                url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.018'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
            font-family: 'DM Sans', sans-serif;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 2rem 1rem;
        }

        .login-shell {
            width: 100%;
            max-width: 960px;
            display: grid;
            grid-template-columns: 1fr 1fr;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 32px 80px rgba(0,0,0,0.55), 0 0 0 1px var(--border);
            animation: fadeUp .55s ease both;
        }
        @keyframes fadeUp {
            from { opacity:0; transform:translateY(24px); }
            to   { opacity:1; transform:translateY(0); }
        }

        /* ── Left brand panel ── */
        .brand-panel {
            background: linear-gradient(145deg, var(--navy-mid) 0%, #0f2233 100%);
            padding: 3rem 2.5rem;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            position: relative;
            overflow: hidden;
        }
        .brand-panel::before {
            content: '';
            position: absolute; inset: 0;
            background: radial-gradient(ellipse 130% 80% at 50% 110%, rgba(23,184,166,0.18) 0%, transparent 65%);
            pointer-events: none;
        }
        .deco-ring {
            position: absolute;
            border-radius: 50%;
            border: 1px solid rgba(23,184,166,0.14);
            top: -50px; right: -50px;
            width: 230px; height: 230px;
        }
        .deco-ring-2 {
            position: absolute;
            border-radius: 50%;
            border: 1px solid rgba(240,192,64,0.10);
            bottom: 50px; left: -70px;
            width: 190px; height: 190px;
        }

        .brand-logo {
            display: flex;
            align-items: center;
            gap: .75rem;
            position: relative; z-index: 1;
        }
        .brand-logo-icon {
            width: 42px; height: 42px;
            background: linear-gradient(135deg, var(--teal), #0f9e8f);
            border-radius: 10px;
            display: flex; align-items: center; justify-content: center;
            font-size: 1.25rem;
            box-shadow: 0 4px 14px rgba(23,184,166,0.4);
        }
        .brand-logo-name {
            font-family: 'DM Serif Display', serif;
            font-size: 1.4rem;
            color: #fff;
        }
        .brand-logo-name span { color: var(--teal); }

        .brand-copy { position: relative; z-index: 1; }
        .brand-copy h1 {
            font-family: 'DM Serif Display', serif;
            font-size: 2.1rem;
            color: #fff;
            line-height: 1.22;
            margin-bottom: 1rem;
        }
        .brand-copy h1 em { color: var(--teal); font-style: italic; }
        .brand-copy p {
            color: var(--text-muted);
            font-size: .9rem;
            line-height: 1.75;
        }

        .role-list {
            display: flex;
            flex-direction: column;
            gap: .45rem;
            position: relative; z-index: 1;
        }
        .role-item {
            display: flex;
            align-items: center;
            gap: .65rem;
            background: rgba(255,255,255,0.04);
            border: 1px solid var(--border);
            border-radius: 8px;
            padding: .55rem .85rem;
            color: var(--text-light);
            font-size: .81rem;
            transition: background .2s, border-color .2s;
        }
        .role-item:hover { background: rgba(23,184,166,0.08); border-color: rgba(23,184,166,0.3); }
        .role-dot { width: 7px; height: 7px; border-radius: 50%; flex-shrink: 0; }

        /* ── Right form panel ── */
        .form-panel {
            background: var(--navy-mid);
            padding: 3rem 2.5rem;
            display: flex;
            flex-direction: column;
            justify-content: center;
        }
        .form-panel h2 {
            font-family: 'DM Serif Display', serif;
            font-size: 1.8rem;
            color: #fff;
            margin-bottom: .3rem;
        }
        .form-subtitle {
            color: var(--text-muted);
            font-size: .88rem;
            margin-bottom: 2rem;
        }

        /* Alert */
        .sims-alert {
            background: rgba(220,53,69,0.12);
            border: 1px solid rgba(220,53,69,0.3);
            border-radius: 8px;
            color: #f87171;
            font-size: .84rem;
            padding: .7rem 1rem;
            margin-bottom: 1.25rem;
            display: none;
            align-items: center;
            gap: .5rem;
        }
        .sims-alert.visible { display: flex; }

        /* Field */
        .field-group { margin-bottom: 1.2rem; }
        .field-label {
            display: block;
            font-size: .75rem;
            font-weight: 600;
            letter-spacing: .09em;
            text-transform: uppercase;
            color: var(--text-muted);
            margin-bottom: .45rem;
        }
        .field-wrap { position: relative; }
        .fi {
            position: absolute;
            left: .95rem; top: 50%;
            transform: translateY(-50%);
            color: var(--text-muted);
            pointer-events: none;
            transition: color .2s;
            display: flex;
        }
        .field-wrap:focus-within .fi { color: var(--teal); }
        .field-input {
            width: 100%;
            background: rgba(255,255,255,0.05);
            border: 1px solid var(--border);
            border-radius: 10px;
            padding: .75rem 1rem .75rem 2.7rem;
            color: #fff;
            font-size: .9rem;
            font-family: 'DM Sans', sans-serif;
            outline: none;
            transition: border-color .2s, background .2s, box-shadow .2s;
        }
        .field-input::placeholder { color: var(--text-muted); }
        .field-input:focus {
            border-color: var(--teal);
            background: rgba(23,184,166,0.06);
            box-shadow: 0 0 0 3px rgba(23,184,166,0.14);
        }
        .pw-toggle {
            position: absolute;
            right: .9rem; top: 50%;
            transform: translateY(-50%);
            background: none; border: none;
            color: var(--text-muted);
            cursor: pointer; padding: 0;
            display: flex;
            transition: color .2s;
        }
        .pw-toggle:hover { color: var(--teal); }

        .form-options {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1.6rem;
        }
        .check-label {
            display: flex; align-items: center; gap: .45rem;
            color: var(--text-muted); font-size: .84rem;
            cursor: pointer; user-select: none;
        }
        .check-label input { accent-color: var(--teal); }
        .forgot { color: var(--teal); font-size: .84rem; text-decoration: none; }
        .forgot:hover { text-decoration: underline; }

        .btn-signin {
            width: 100%;
            background: linear-gradient(135deg, var(--teal), var(--teal-dark));
            border: none;
            border-radius: 10px;
            padding: .82rem;
            color: var(--navy);
            font-family: 'DM Sans', sans-serif;
            font-weight: 600;
            font-size: .95rem;
            cursor: pointer;
            box-shadow: 0 6px 20px rgba(23,184,166,0.35);
            transition: opacity .2s, transform .15s, box-shadow .2s;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: .5rem;
        }
        .btn-signin:hover { opacity: .9; transform: translateY(-1px); box-shadow: 0 10px 28px rgba(23,184,166,0.42); }
        .btn-signin:active { transform: translateY(0); }
        .btn-signin:disabled { opacity: .7; cursor: not-allowed; transform: none; }
        .spin {
            width: 16px; height: 16px;
            border: 2px solid rgba(13,27,42,.25);
            border-top-color: var(--navy);
            border-radius: 50%;
            animation: spin .6s linear infinite;
            display: none;
        }
        @keyframes spin { to { transform: rotate(360deg); } }

        .form-footer {
            text-align: center;
            margin-top: 1.5rem;
            font-size: .82rem;
            color: var(--text-muted);
        }
        .form-footer a { color: var(--teal); text-decoration: none; }
        .form-footer a:hover { text-decoration: underline; }

        @media (max-width: 680px) {
            .login-shell { grid-template-columns: 1fr; }
            .brand-panel { padding: 2rem 1.75rem; }
            .form-panel  { padding: 2rem 1.75rem; }
            .brand-copy h1 { font-size: 1.65rem; }
        }
    </style>
</head>
<body>

<div class="login-shell">

    <!-- ══ LEFT PANEL ══ -->
    <div class="brand-panel">
        <div class="deco-ring"></div>
        <div class="deco-ring-2"></div>

        <div class="brand-logo">
            <div class="brand-logo-icon">🎓</div>
            <div class="brand-logo-name">SI<span>MS</span></div>
        </div>

        <div class="brand-copy">
            <h1>Academic<br/>excellence,<br/><em>simplified.</em></h1>
            <p>A unified platform for student records, enrolments, performance analytics, and institutional reporting.</p>
        </div>

        <div class="role-list">
            <div class="role-item">
                <div class="role-dot" style="background:#f0c040"></div>
                <strong>Head of Programme</strong>&ensp;&mdash;&ensp;Full administrative control
            </div>
            <div class="role-item">
                <div class="role-dot" style="background:#17b8a6"></div>
                <strong>Lecturer</strong>&ensp;&mdash;&ensp;Grades, attendance &amp; course tools
            </div>
            <div class="role-item">
                <div class="role-dot" style="background:#6ea8fe"></div>
                <strong>Student</strong>&ensp;&mdash;&ensp;Profile, results &amp; academic history
            </div>
        </div>
    </div>

    <!-- ══ RIGHT FORM PANEL ══ -->
    <div class="form-panel">
        <h2>Welcome back</h2>
        <p class="form-subtitle">Sign in to access your dashboard</p>

        <%-- Server-rendered error message --%>
        <div class="sims-alert <%= ShowAlert ? "visible" : "" %>" id="alertBox">
            <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor">
                <path d="M8 1a7 7 0 1 0 0 14A7 7 0 0 0 8 1zm.75 3.5v4a.75.75 0 0 1-1.5 0v-4a.75.75 0 0 1 1.5 0zm-.75 6.5a.75.75 0 1 1 0-1.5.75.75 0 0 1 0 1.5z"/>
            </svg>
            <span><%= AlertMessage %></span>
        </div>

        <form id="loginForm" runat="server">

            <%-- Email --%>
            <div class="field-group">
                <label class="field-label" for="txtEmail">Email Address</label>
                <div class="field-wrap">
                    <span class="fi">
                        <svg width="15" height="15" viewBox="0 0 20 20" fill="currentColor">
                            <path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"/>
                            <path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"/>
                        </svg>
                    </span>
                    <asp:TextBox ID="txtEmail" runat="server"
                        CssClass="field-input"
                        TextMode="Email"
                        placeholder="you@college.edu.my"
                        MaxLength="150" />
                </div>
            </div>

            <%-- Password --%>
            <div class="field-group">
                <label class="field-label" for="txtPassword">Password</label>
                <div class="field-wrap">
                    <span class="fi">
                        <svg width="14" height="14" viewBox="0 0 20 20" fill="currentColor">
                            <path fill-rule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clip-rule="evenodd"/>
                        </svg>
                    </span>
                    <asp:TextBox ID="txtPassword" runat="server"
                        CssClass="field-input"
                        TextMode="Password"
                        placeholder="••••••••"
                        MaxLength="100"
                        ClientIDMode="Static" />
                    <button type="button" class="pw-toggle" onclick="togglePw()" title="Show / hide password" aria-label="Toggle password visibility">
                        <svg id="eyeOpen" width="16" height="16" viewBox="0 0 20 20" fill="currentColor">
                            <path d="M10 12a2 2 0 100-4 2 2 0 000 4z"/>
                            <path fill-rule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clip-rule="evenodd"/>
                        </svg>
                        <svg id="eyeOff" width="16" height="16" viewBox="0 0 20 20" fill="currentColor" style="display:none">
                            <path fill-rule="evenodd" d="M3.707 2.293a1 1 0 00-1.414 1.414l14 14a1 1 0 001.414-1.414l-1.473-1.473A10.014 10.014 0 0019.542 10C18.268 5.943 14.478 3 10 3a9.958 9.958 0 00-4.512 1.074l-1.78-1.781zm4.261 4.26l1.514 1.515a2.003 2.003 0 012.45 2.45l1.514 1.514a4 4 0 00-5.478-5.478z" clip-rule="evenodd"/>
                            <path d="M12.454 16.697L9.75 13.992a4 4 0 01-3.742-3.741L2.335 6.578A9.98 9.98 0 00.458 10c1.274 4.057 5.065 7 9.542 7 .847 0 1.669-.105 2.454-.303z"/>
                        </svg>
                    </button>
                </div>
            </div>

            <%-- Remember me / Forgot --%>
            <div class="form-options">
                <label class="check-label">
                    <asp:CheckBox ID="chkRemember" runat="server" />
                    Remember me
                </label>
                <a href="ForgotPassword.aspx" class="forgot">Forgot password?</a>
            </div>

            <%-- Sign-in button --%>
            <asp:Button ID="btnLogin" runat="server"
                CssClass="btn-signin"
                Text="Sign In"
                OnClick="btnLogin_Click"
                OnClientClick="onSignIn(this)" />

        </form>

        <div class="form-footer">
            Need help? Contact your <a href="mailto:admin@college.edu.my">system administrator</a>
        </div>
    </div>
</div>

<script>
    function togglePw() {
        var pw     = document.getElementById('txtPassword');
        var open   = document.getElementById('eyeOpen');
        var closed = document.getElementById('eyeOff');
        if (pw.type === 'password') {
            pw.type = 'text';
            open.style.display   = 'none';
            closed.style.display = '';
        } else {
            pw.type = 'password';
            open.style.display   = '';
            closed.style.display = 'none';
        }
    }

    function onSignIn(btn) {
        btn.value = 'Signing in…';
        btn.disabled = true;
        // Allow postback to proceed
        return true;
    }
</script>
</body>
</html>
