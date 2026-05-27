<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Login.aspx.cs"
    Inherits="SIMS.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>SIMS – Login</title>

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />

    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        html, body { height: 100%; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }
        body { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); display: flex; align-items: center; justify-content: center; padding: 20px; min-height: 100vh; }
        .login-wrapper { width: 100%; max-width: 1000px; display: grid; grid-template-columns: 1fr 1fr; gap: 0; border-radius: 12px; overflow: hidden; box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3); }
        .login-left { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 60px 40px; display: flex; flex-direction: column; justify-content: center; align-items: center; text-align: center; }
        .login-left .logo { font-size: 80px; margin-bottom: 20px; display: block; }
        .login-left h1 { font-size: 32px; font-weight: bold; margin-bottom: 10px; }
        .login-left p { font-size: 14px; opacity: 0.9; margin-bottom: 30px; }
        .login-right { background: white; padding: 60px 40px; display: flex; flex-direction: column; justify-content: center; }
        .login-right h2 { font-size: 24px; font-weight: bold; color: #1e293b; margin-bottom: 30px; text-align: center; }
        .form-group { margin-bottom: 20px; }
        .form-group label { display: block; font-weight: 600; color: #1e293b; margin-bottom: 8px; font-size: 14px; }
        .form-group input { width: 100%; padding: 12px 15px; border: 1px solid #cbd5e1; border-radius: 6px; font-size: 14px; font-family: inherit; transition: all 0.3s ease; }
        .form-group input:focus { outline: none; border-color: #667eea; box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1); }
        .form-group input::placeholder { color: #94a3b8; }
        .form-group.checkbox-group { display: flex; justify-content: space-between; align-items: center; margin-bottom: 25px; }
        .form-group.checkbox-group input[type="checkbox"] { width: auto; margin-right: 8px; }
        .form-group.checkbox-group label { margin: 0; display: flex; align-items: center; color: #475569; font-weight: 500; cursor: pointer; }
        .btn-login { width: 100%; padding: 12px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border: none; border-radius: 6px; font-size: 16px; font-weight: 600; cursor: pointer; transition: all 0.3s ease; margin-bottom: 15px; }
        .btn-login:hover { transform: translateY(-2px); box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4); }
        .btn-login:disabled { opacity: 0.6; cursor: not-allowed; transform: none; }
        .alert-message { padding: 12px 15px; border-radius: 6px; margin-bottom: 20px; font-size: 14px; display: flex; align-items: center; gap: 10px; }
        .alert-danger { background: #fee2e2; border: 1px solid #fca5a5; color: #991b1b; }
        .loading { display: none; text-align: center; padding: 10px; color: #667eea; }
        .loading i { margin-right: 8px; }
        @media (max-width: 768px) { .login-wrapper { grid-template-columns: 1fr; } .login-left { padding: 40px 20px; display: none; } .login-right { padding: 40px 20px; } }
    </style>
</head>
<body>
    <div class="login-wrapper">
        <!-- Left Panel -->
        <div class="login-left">
            <span class="logo">🎓</span>
            <h1>SIMS</h1>
            <p>Student Information Management System</p>
        </div>

        <!-- Right Panel -->
        <div class="login-right">
            <h2>Sign In</h2>

            <form runat="server">
                <!-- Error Alert -->
                <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert-message alert-danger">
                    <i class="fa fa-exclamation-circle"></i>
                    <span><asp:Literal ID="litErrorMsg" runat="server" /></span>
                </asp:Panel>

                <!-- Email -->
                <div class="form-group">
                    <label for="txtEmail">Email or ID Number</label>
                    <asp:TextBox ID="txtEmail" runat="server" placeholder="Enter your email or ID number" TextMode="SingleLine" />
                </div>

                <!-- Password -->
                <div class="form-group">
                    <label for="txtPassword">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" placeholder="Enter your password" TextMode="Password" />
                </div>

                <!-- Remember Me -->
                <div class="form-group checkbox-group">
                    <label>
                        <asp:CheckBox ID="chkRememberMe" runat="server" />
                        <span>Remember me</span>
                    </label>
                </div>

                <!-- Login Button -->
                <asp:Button ID="btnLogin" runat="server" Text="Sign In" CssClass="btn-login" OnClick="btnLogin_Click" />

                <!-- Loading Indicator -->
                <div class="loading" id="loadingDiv">
                    <i class="fa fa-spinner fa-spin"></i>
                    <span>Signing in...</span>
                </div>
            </form>
        </div>
    </div>
</body>
</html>