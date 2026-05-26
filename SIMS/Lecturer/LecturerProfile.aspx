<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerProfile.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerProfile"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .profile-header {
            background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .profile-header-content h3 {
            margin: 0 0 5px 0;
        }

        .profile-header-content p {
            margin: 0;
            opacity: 0.9;
            font-size: 14px;
        }

        .profile-actions {
            display: flex;
            gap: 10px;
        }

        .btn-edit, .btn-save, .btn-cancel {
            padding: 10px 20px;
            border: none;
            border-radius: 6px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .btn-edit {
            background: rgba(255, 255, 255, 0.2);
            color: white;
            border: 1px solid rgba(255, 255, 255, 0.3);
        }

        .btn-edit:hover {
            background: rgba(255, 255, 255, 0.3);
        }

        .btn-save {
            background: #16a34a;
            color: white;
        }

        .btn-save:hover {
            background: #15803d;
        }

        .btn-cancel {
            background: #64748b;
            color: white;
        }

        .btn-cancel:hover {
            background: #475569;
        }

        .profile-section {
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            padding: 20px;
            margin-bottom: 20px;
        }

        .profile-section h5 {
            color: #1e293b;
            margin: 0 0 15px 0;
            font-weight: bold;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .profile-section h5 i {
            color: #6366f1;
        }

        .profile-item {
            display: grid;
            grid-template-columns: 200px 1fr;
            gap: 20px;
            margin-bottom: 12px;
            padding-bottom: 12px;
            border-bottom: 1px solid #e2e8f0;
            align-items: center;
        }

        .profile-item:last-child {
            border-bottom: none;
            margin-bottom: 0;
            padding-bottom: 0;
        }

        .profile-label {
            font-weight: bold;
            color: #475569;
            font-size: 14px;
        }

        .profile-value {
            color: #1e293b;
            font-size: 14px;
        }

        .profile-form {
            display: none;
        }

        .profile-form.show {
            display: block;
        }

        .profile-view {
            display: block;
        }

        .profile-view.hide {
            display: none;
        }

        .form-row {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
            margin-bottom: 20px;
        }

        .form-row.full {
            grid-template-columns: 1fr;
        }

        .form-group {
            display: flex;
            flex-direction: column;
            margin-bottom: 0;
        }

        .form-group label {
            font-weight: bold;
            color: #1e293b;
            margin-bottom: 8px;
            font-size: 14px;
        }

        .form-group input,
        .form-group textarea,
        .form-group select {
            padding: 10px 12px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            font-size: 14px;
            font-family: inherit;
            transition: all 0.3s ease;
        }

        .form-group input:focus,
        .form-group textarea:focus,
        .form-group select:focus {
            outline: none;
            border-color: #6366f1;
            box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
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

        .form-actions {
            display: flex;
            gap: 10px;
            justify-content: flex-end;
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid #e2e8f0;
        }

        .info-badge {
            display: inline-block;
            background: #dbeafe;
            color: #1e40af;
            padding: 4px 10px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: bold;
        }

        .read-only {
            background: #f1f5f9;
            cursor: not-allowed;
            color: #64748b;
        }

        @media (max-width: 768px) {
            .form-row {
                grid-template-columns: 1fr;
            }

            .profile-header {
                flex-direction: column;
                align-items: flex-start;
                gap: 15px;
            }

            .profile-actions {
                width: 100%;
            }

            .profile-item {
                grid-template-columns: 1fr;
                gap: 5px;
            }
        }
    </style>

    <script type="text/javascript">
        // Toggle between view and edit modes
        function toggleEditMode() {
            var profileView = document.getElementById('profileView');
            var profileForm = document.getElementById('profileForm');
            var btnEdit = document.getElementById('<%= btnEdit.ClientID %>');
            var btnSave = document.getElementById('<%= btnSave.ClientID %>');
            var btnCancel = document.getElementById('<%= btnCancel.ClientID %>');

            var isEditMode = profileForm.classList.contains('show');

            if (isEditMode) {
                // Switch to view mode
                profileView.classList.remove('hide');
                profileForm.classList.remove('show');
                btnEdit.style.display = 'inline-block';
                btnSave.style.display = 'none';
                btnCancel.style.display = 'none';
            } else {
                // Switch to edit mode
                profileView.classList.add('hide');
                profileForm.classList.add('show');
                btnEdit.style.display = 'none';
                btnSave.style.display = 'inline-block';
                btnCancel.style.display = 'inline-block';
            }
        }

        // Initialize event handlers when document is ready
        document.addEventListener('DOMContentLoaded', function () {
            var btnEdit = document.getElementById('<%= btnEdit.ClientID %>');
            var btnCancel = document.getElementById('<%= btnCancel.ClientID %>');

            if (btnEdit) {
                btnEdit.addEventListener('click', function (e) {
                    e.preventDefault();
                    toggleEditMode();
                    return false;
                });
            }

            if (btnCancel) {
                btnCancel.addEventListener('click', function (e) {
                    e.preventDefault();
                    toggleEditMode();
                    return false;
                });
            }
        });
    </script>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Header -->
    <div class="profile-header">
        <div class="profile-header-content">
            <h3>
                <i class="fa fa-user" style="margin-right: 10px;"></i>My Profile
            </h3>
            <p>Manage your personal information and account details</p>
        </div>
        <div class="profile-actions">
            <asp:Button ID="btnEdit" runat="server" Text="Edit Profile" CssClass="btn-edit" OnClick="btnEdit_Click" />
            <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn-save" style="display:none;" OnClick="btnSave_Click" />
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-cancel" style="display:none;" OnClick="btnCancel_Click" />
        </div>
    </div>

    <!-- Success/Error Messages -->
    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="alert-message alert-success">
        <i class="fa fa-check-circle"></i>
        <span><asp:Literal ID="litSuccessMsg" runat="server" /></span>
    </asp:Panel>

    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert-message alert-danger">
        <i class="fa fa-exclamation-circle"></i>
        <span><asp:Literal ID="litErrorMsg" runat="server" /></span>
    </asp:Panel>

    <!-- VIEW MODE -->
    <div class="profile-view" id="profileView">
        <!-- Personal Information -->
        <div class="profile-section">
            <h5>
                <i class="fa fa-id-card"></i> Personal Information
            </h5>
            <div class="profile-item">
                <div class="profile-label">Full Name:</div>
                <div class="profile-value">
                    <asp:Literal ID="litFullName" runat="server" />
                </div>
            </div>
            <div class="profile-item">
                <div class="profile-label">Email:</div>
                <div class="profile-value">
                    <asp:Literal ID="litEmail" runat="server" />
                </div>
            </div>
            <div class="profile-item">
                <div class="profile-label">Staff No:</div>
                <div class="profile-value">
                    <asp:Literal ID="litStaffNo" runat="server" />
                </div>
            </div>
            <div class="profile-item">
                <div class="profile-label">Department:</div>
                <div class="profile-value">
                    <asp:Literal ID="litDepartment" runat="server" />
                </div>
            </div>
            <div class="profile-item">
                <div class="profile-label">Specialisation:</div>
                <div class="profile-value">
                    <asp:Literal ID="litSpecialisation" runat="server" Text="Not specified" />
                </div>
            </div>
        </div>

        <!-- Contact Information -->
        <div class="profile-section">
            <h5>
                <i class="fa fa-phone"></i> Contact Information
            </h5>
            <div class="profile-item">
                <div class="profile-label">Phone:</div>
                <div class="profile-value">
                    <asp:Literal ID="litPhone" runat="server" Text="Not provided" />
                </div>
            </div>
            <div class="profile-item">
                <div class="profile-label">Photo:</div>
                <div class="profile-value">
                    <asp:Literal ID="litPhotoStatus" runat="server" Text="No profile picture" />
                </div>
            </div>
        </div>

        <!-- Employment Information -->
        <div class="profile-section">
            <h5>
                <i class="fa fa-briefcase"></i> Employment Information
            </h5>
            <div class="profile-item">
                <div class="profile-label">Employment Status:</div>
                <div class="profile-value">
                    <span class="info-badge"><asp:Literal ID="litEmploymentStatus" runat="server" /></span>
                </div>
            </div>
            <div class="profile-item">
                <div class="profile-label">Member Since:</div>
                <div class="profile-value">
                    <asp:Literal ID="litMemberSince" runat="server" />
                </div>
            </div>
            <div class="profile-item">
                <div class="profile-label">Last Login:</div>
                <div class="profile-value">
                    <asp:Literal ID="litLastLogin" runat="server" Text="Never" />
                </div>
            </div>
        </div>
    </div>

    <!-- EDIT MODE -->
    <div class="profile-form" id="profileForm">
        <div class="profile-section">
            <h5>
                <i class="fa fa-edit"></i> Edit Profile Information
            </h5>

            <div class="form-row">
                <div class="form-group">
                    <label for="txtFullName">Full Name *</label>
                    <asp:TextBox ID="txtFullName" runat="server" />
                </div>
                <div class="form-group">
                    <label for="txtEmail">Email *</label>
                    <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" />
                </div>
            </div>

            <div class="form-row">
                <div class="form-group">
                    <label for="txtPhone">Phone Number</label>
                    <asp:TextBox ID="txtPhone" runat="server" TextMode="Phone" placeholder="e.g., +1 (555) 123-4567" />
                </div>
                <div class="form-group">
                    <label for="txtSpecialisation">Specialisation</label>
                    <asp:TextBox ID="txtSpecialisation" runat="server" placeholder="e.g., Computer Science, Mathematics" />
                </div>
            </div>

            <div class="form-row full">
                <div class="form-group">
                    <label>Staff No (Read-only)</label>
                    <asp:TextBox ID="txtStaffNo" runat="server" CssClass="read-only" ReadOnly="true" />
                </div>
            </div>

            <div class="form-row full">
                <div class="form-group">
                    <label>Department (Read-only)</label>
                    <asp:TextBox ID="txtDepartment" runat="server" CssClass="read-only" ReadOnly="true" />
                </div>
            </div>

            <div class="form-row full">
                <div class="form-group">
                    <label>Employment Status (Read-only)</label>
                    <asp:TextBox ID="txtEmploymentStatus" runat="server" CssClass="read-only" ReadOnly="true" />
                </div>
            </div>

            <div class="form-actions">
                <!-- Buttons handled via JavaScript toggle -->
            </div>
        </div>
    </div>

</asp:Content>