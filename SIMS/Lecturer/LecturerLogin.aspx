<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerLogin.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerLogin" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>SIMS – Lecturer Login</title>

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"
          rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css"
          rel="stylesheet" />

    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }

        .login-container {
            width: 100%;
            max-width: 450px;
            background: white;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
            overflow: hidden;
        }

        .login-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px 20px;
            text-align: center;
        }

        .login-header .logo {
            font-size: 48px;
            margin-bottom: 15px;
            display: block;
        }

        .login-header h1 {
            font-size: 28px;
            font-weight: bold;
            margin: 0 0 5px 0;
        }

        .login-header p {
            margin: 0;
            opacity: 0.9;
            font-size: 14px;
        }

        .login-body {
            padding: 40px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-group label {
            display: block;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 8px;
            font-size: 14px;
        }

        .form-group input {
            width: 100%;
            padding: 12px 15px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            font-size: 14px;
            font-family: inherit;
            transition: all 0.3s ease;
        }

        .form-group input:focus {
            outline: none;
            border-color: #667eea;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
        }

        .form-group input::placeholder {
            color: #94a3b8;
        }

        .remember-forgot {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
            font-size: 14px;
        }

        .remember-forgot input[type="checkbox"] {
            width: auto;
            margin-right: 5px;
        }

        .remember-forgot label {
            margin: 0;
            color: #475569;
            cursor: pointer;
        }

        .remember-forgot a {
            color: #667eea;
            text-decoration: none;
            font-weight: 600;
        }

        .remember-forgot a:hover {
            text-decoration: underline;
        }

        .btn-login {
            width: 100%;
            padding: 12px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 6px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            margin-bottom: 15px;
        }

        .btn-login:hover {
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
        }

        .btn-login:active {
            transform: translateY(0);
        }

        .btn-login:disabled {
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }

        .alert-message {
            padding: 12px 15px;
            border-radius: 6px;
            margin-bottom: 20px;
            font-size: 14px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .alert-success {
            background: #dcfce7;
            border: 1px solid #86efac;
            color: #166534;
        }

        .alert-danger {
            background: #fee2e2;
            border: 1px solid #fca5a5;
            color: #991b1b;
        }

        .alert-warning {
            background: #fef3c7;
            border: 1px solid #fde68a;
            color: #92400e;
        }

        .login-footer {
            padding: 20px 40px;
            text-align: center;
            background: #f8fafc;
            border-top: 1px solid #e2e8f0;
            font-size: 13px;
            color: #64748b;
        }

        .login-footer p {
            margin: 0;
        }

        .loading {
            display: none;
            text-align: center;
            padding: 10px;
            color: #667eea;
        }

        .loading i {
            margin-right: 8px;
        }

        .form-group.error input {
            border-color: #dc2626;
            background: #fef2f2;
        }

        .error-text {
            color: #dc2626;
            font-size: 12px;
            margin-top: 5px;
            display: none;
        }

        .error-text.show {
            display: block;
        }

        @media (max-width: 480px) {
            .login-header {
                padding: 30px 20px;
            }

            .login-header h1 {
                font-size: 24px;
            }

            .login-body {
                padding: 30px 20px;
            }

            .login-footer {
                padding: 15px 20px;
            }
        }
    </style>
</head>
<body>
    <div class="login-container">
        <!-- Header -->
        <div class="login-header">
            <span class="logo">🎓</span>
            <h1>SIMS</h1>
            <p>Student Information Management System</p>
        </div>

        <!-- Body -->
        <div class="login-body">
            <form runat="server">
                <!-- Error Alert -->
                <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert-message alert-danger">
                    <i class="fa fa-exclamation-circle"></i>
                    <span>
                        <asp:Literal ID="litErrorMsg" runat="server" />
                    </span>
                </asp:Panel>

                <!-- Warning Alert -->
                <asp:Panel ID="pnlWarning" runat="server" Visible="false" CssClass="alert-message alert-warning">
                    <i class="fa fa-info-circle"></i>
                    <span>
                        <asp:Literal ID="litWarningMsg" runat="server" />
                    </span>
                </asp:Panel>

                <!-- Email/Staff Number -->
                <div class="form-group">
                    <label for="txtEmail">Email or Staff Number</label>
                    <asp:TextBox ID="txtEmail" runat="server"
                        placeholder="Enter your email or staff number"
                        TextMode="SingleLine" />
                    <div class="error-text" id="emailError">Please enter a valid email or staff number</div>
                </div>

                <!-- Password -->
                <div class="form-group">
                    <label for="txtPassword">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server"
                        placeholder="Enter your password"
                        TextMode="Password" />
                    <div class="error-text" id="passwordError">Please enter your password</div>
                </div>

                <!-- Remember & Forgot -->
                <div class="remember-forgot">
                    <label>
                        <asp:CheckBox ID="chkRememberMe" runat="server" />
                        <span style="margin-left: 5px;">Remember me</span>
                    </label>
                    <a href="LecturerForgotPassword.aspx">Forgot Password?</a>
                </div>

                <!-- Login Button -->
                <asp:Button ID="btnLogin" runat="server"
                    Text="Sign In"
                    CssClass="btn-login"
                    OnClick="btnLogin_Click" />

                <!-- Loading Indicator -->
                <div class="loading" id="loadingDiv">
                    <i class="fa fa-spinner fa-spin"></i>
                    <span>Signing in...</span>
                </div>
            </form>
        </div>

        <!-- Footer -->
        <div class="login-footer">
            <p>
                <i class="fa fa-lock"></i> Your credentials are secure and encrypted.
            </p>
        </div>
    </div>

    <script type="text/javascript">
        // Simple client-side validation
        function validateForm() {
            var email = document.getElementById('<%= txtEmail.ClientID %>').value;
            var password = document.getElementById('<%= txtPassword.ClientID %>').value;
            var isValid = true;

            // Reset error messages
            document.getElementById('emailError').classList.remove('show');
            document.getElementById('passwordError').classList.remove('show');

            if (!email) {
                document.getElementById('emailError').classList.add('show');
                isValid = false;
            }

            if (!password) {
                document.getElementById('passwordError').classList.add('show');
                isValid = false;
            }

            return isValid;
        }

        // Show loading indicator
        function showLoading() {
            document.getElementById('loadingDiv').style.display = 'block';
            document.getElementById('<%= btnLogin.ClientID %>').disabled = true;
        }
    </script>
</body>
</html>