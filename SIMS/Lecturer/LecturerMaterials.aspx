<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerMaterials.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerMaterials"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .materials-header {
            background: linear-gradient(135deg, #7c3aed 0%, #6d28d9 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
        }

        .materials-header h3 {
            margin: 0 0 5px 0;
        }

        .upload-section {
            background: white;
            border: 2px dashed #cbd5e1;
            border-radius: 8px;
            padding: 30px;
            margin-bottom: 30px;
            text-align: center;
        }

        .upload-section.drag-over {
            border-color: #7c3aed;
            background: #faf5ff;
        }

        .upload-icon {
            font-size: 48px;
            color: #7c3aed;
            margin-bottom: 15px;
        }

        .upload-text {
            font-size: 16px;
            color: #1e293b;
            margin-bottom: 8px;
            font-weight: bold;
        }

        .upload-hint {
            font-size: 13px;
            color: #64748b;
            margin-bottom: 20px;
        }

        .upload-form {
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 30px;
        }

        .form-row {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
            margin-bottom: 20px;
        }

        .form-group label {
            display: block;
            font-weight: bold;
            color: #1e293b;
            margin-bottom: 8px;
            font-size: 14px;
        }

        .form-group input,
        .form-group textarea,
        .form-group select {
            width: 100%;
            padding: 10px 12px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            font-size: 14px;
            font-family: inherit;
        }

        .form-group textarea {
            resize: vertical;
            min-height: 80px;
        }

        .form-group input:focus,
        .form-group textarea:focus,
        .form-group select:focus {
            outline: none;
            border-color: #7c3aed;
            box-shadow: 0 0 0 3px rgba(124, 58, 237, 0.1);
        }

        .file-input-wrapper {
            position: relative;
            overflow: hidden;
            display: inline-block;
            width: 100%;
        }

        .file-input-wrapper input[type="file"] {
            position: absolute;
            left: -9999px;
        }

        .file-input-label {
            display: block;
            padding: 10px 12px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            background: white;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .file-input-label:hover {
            border-color: #7c3aed;
            background: #faf5ff;
        }

        .file-name {
            display: inline-block;
            font-size: 13px;
            color: #7c3aed;
            font-weight: bold;
            margin-left: 10px;
        }

        .form-actions {
            display: flex;
            gap: 10px;
        }

        .form-actions .btn {
            padding: 10px 20px;
            font-size: 14px;
        }

        .materials-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }

        .material-card {
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            overflow: hidden;
            transition: all 0.3s ease;
        }

        .material-card:hover {
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            transform: translateY(-2px);
        }

        .material-icon {
            background: #faf5ff;
            padding: 15px;
            text-align: center;
            font-size: 36px;
            color: #7c3aed;
            border-bottom: 1px solid #e2e8f0;
        }

        .material-content {
            padding: 15px;
        }

        .material-title {
            font-weight: bold;
            color: #1e293b;
            margin-bottom: 8px;
            font-size: 15px;
            word-break: break-word;
        }

        .material-info {
            display: flex;
            gap: 10px;
            font-size: 12px;
            color: #64748b;
            margin-bottom: 12px;
            flex-wrap: wrap;
        }

        .material-info-item {
            display: flex;
            align-items: center;
            gap: 4px;
        }

        .material-description {
            font-size: 13px;
            color: #64748b;
            margin-bottom: 12px;
            line-height: 1.4;
            max-height: 60px;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .material-actions {
            display: flex;
            gap: 8px;
            padding-top: 12px;
            border-top: 1px solid #e2e8f0;
        }

        .material-actions .btn {
            flex: 1;
            padding: 6px 10px;
            font-size: 12px;
        }

        .materials-table {
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            overflow: hidden;
            margin-bottom: 30px;
        }

        .table-sims {
            width: 100%;
            border-collapse: collapse;
        }

        .table-sims thead {
            background: #f1f5f9;
            border-bottom: 2px solid #cbd5e1;
        }

        .table-sims th {
            padding: 12px 15px;
            text-align: left;
            font-weight: bold;
            color: #1e293b;
            font-size: 14px;
        }

        .table-sims td {
            padding: 12px 15px;
            border-bottom: 1px solid #e2e8f0;
            font-size: 14px;
            color: #475569;
        }

        .table-sims tbody tr:hover {
            background: #f8fafc;
        }

        .file-type-badge {
            display: inline-block;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 11px;
            font-weight: bold;
            text-transform: uppercase;
            background: #f1f5f9;
            color: #475569;
        }

        .status-visible {
            background: #dcfce7;
            color: #166534;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: bold;
        }

        .status-hidden {
            background: #fee2e2;
            color: #991b1b;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: bold;
        }

        .no-materials {
            text-align: center;
            padding: 40px 20px;
            color: #64748b;
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
        }

        .no-materials i {
            font-size: 48px;
            color: #cbd5e1;
            display: block;
            margin-bottom: 15px;
        }

        .success-message {
            background: #dcfce7;
            border: 1px solid #86efac;
            color: #166534;
            padding: 12px 15px;
            border-radius: 6px;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .error-message {
            background: #fee2e2;
            border: 1px solid #fca5a5;
            color: #991b1b;
            padding: 12px 15px;
            border-radius: 6px;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        @media (max-width: 768px) {
            .form-row {
                grid-template-columns: 1fr;
            }

            .materials-grid {
                grid-template-columns: 1fr;
            }

            .form-actions {
                flex-direction: column;
            }
        }
            .upload-form { background: white; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; margin-bottom: 20px; }
            .form-group { display:flex; flex-direction:column; margin-bottom:12px; }
            .form-group label { font-weight:600; margin-bottom:6px; }
            .form-actions { display:flex; gap:10px; justify-content:flex-end; margin-top:12px; }
            .materials-table { background:white; border:1px solid #e2e8f0; border-radius:8px; overflow:hidden; }
            .table-sims th, .table-sims td { padding:10px 12px; border-bottom:1px solid #e2e8f0; }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <div class="materials-header">
        <h3><i class="fa fa-file" style="margin-right:8px;"></i>Course Materials</h3>
        <p style="margin:0;"><asp:Literal ID="litCourseHeader" runat="server" Text="Loading..." /></p>
    </div>

    <asp:Panel ID="pnlMaterialSuccess" runat="server" Visible="false" CssClass="success-message">
        <i class="fa fa-check-circle"></i>
        <span><asp:Literal ID="litMaterialSuccessMsg" runat="server" /></span>
    </asp:Panel>

    <asp:Panel ID="pnlMaterialError" runat="server" Visible="false" CssClass="error-message">
        <i class="fa fa-exclamation-circle"></i>
        <span><asp:Literal ID="litMaterialErrorMsg" runat="server" /></span>
    </asp:Panel>

    <div class="upload-form">
        <!-- Read-only course & assignment -->
        <div class="form-group">
            <label>Course</label>
            <asp:Literal ID="litCourseName" runat="server" />
        </div>

        <div style="display:flex; gap:16px; margin-bottom:12px;">
            <div class="form-group" style="flex:1;">
                <label>Academic Year</label>
                <asp:Literal ID="litAcademicYear" runat="server" />
            </div>
            <div class="form-group" style="width:160px;">
                <label>Semester</label>
                <asp:Literal ID="litSemester" runat="server" />
            </div>
        </div>

        <div class="form-group">
            <label>Material Title</label>
            <asp:TextBox ID="txtTitle" runat="server" placeholder="Enter material title" />
        </div>

        <div class="form-group">
            <label>Description (optional)</label>
            <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" placeholder="Enter description" />
        </div>

        <div class="form-group">
            <label>Upload File</label>
            <asp:FileUpload ID="fuMaterial" runat="server" />
            <small style="color:#64748b;">Allowed: PDF, DOC, DOCX, PPT, PPTX, XLS, XLSX, TXT, ZIP (Max 50MB)</small>
        </div>

        <div class="form-group">
            <asp:CheckBox ID="chkIsVisible" runat="server" Checked="true" Text="Make visible to students" />
        </div>

        <div class="form-actions">
            <asp:Button ID="btnUpload" runat="server" Text="Upload Material" CssClass="btn btn-primary" OnClick="btnUpload_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-outline-secondary" OnClick="btnClear_Click" />
        </div>

        <!-- Hidden fields -->
        <asp:HiddenField ID="hidCourseId" runat="server" />
        <asp:HiddenField ID="hidAcademicYear" runat="server" />
        <asp:HiddenField ID="hidSemester" runat="server" />
    </div>

    <h4 style="margin-top:8px;">Uploaded Materials</h4>
    <div class="materials-table" style="margin-top:8px;">
        <table class="table-sims" style="width:100%; border-collapse:collapse;">
            <thead>
                <tr>
                    <th>Title</th>
                    <th>File Type</th>
                    <th>Uploaded At</th>
                    <th style="width:120px;">Visibility</th>
                    <th style="width:160px;">Action</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptMaterials" runat="server" OnItemDataBound="rptMaterials_ItemDataBound">
                    <ItemTemplate>
                        <tr>
                            <td><strong><%# Eval("Title") %></strong><div style="font-size:12px;color:#64748b;"><%# Eval("Description") %></div></td>
                            <td><%# Eval("FileType") %></td>
                            <td><%# ((DateTime)Eval("UploadedAt")).ToString("dd MMM yyyy HH:mm") %></td>
                            <td>
                                <asp:Label ID="lblVisible" runat="server" Text='<%# Convert.ToBoolean(Eval("IsVisible")) ? "Visible" : "Hidden" %>' />
                            </td>
                            <td>
                                <asp:HyperLink ID="hlDownload" runat="server" NavigateUrl='<%# Eval("FileUrl") %>' Text="Download" Target="_blank" CssClass="btn btn-sm btn-outline-primary"></asp:HyperLink>
                                <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-sm btn-danger" CommandArgument='<%# Eval("MaterialId") %>' OnClick="btnDelete_Click" OnClientClick="return confirm('Delete this material?');" />
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>

        <asp:Panel ID="pnlNoMaterials" runat="server" Visible="false" CssClass="no-materials" style="padding:16px;">
            <i class="fa fa-folder-open" style="font-size:28px;color:#cbd5e1;display:block;margin-bottom:8px;"></i>
            <strong>No materials uploaded for this course.</strong>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var successEl = document.getElementById('<%= pnlMaterialSuccess.ClientID %>');
            var errorEl = document.getElementById('<%= pnlMaterialError.ClientID %>');

            [successEl, errorEl].forEach(function (el) {
                if (!el) return;
                // If element is visible (rendered and not display:none), hide after 5s
                if (el.offsetParent !== null) {
                    setTimeout(function () { el.style.display = 'none'; }, 5000);
                }
            });
        });
    </script>

</asp:Content>