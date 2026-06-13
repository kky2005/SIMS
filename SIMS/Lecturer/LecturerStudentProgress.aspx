<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerStudentProgress.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerStudentProgress"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .progress-header {
            background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
            color: white; padding: 20px; border-radius: 8px; margin-bottom: 20px;
        }
        .progress-header h3 { margin: 0 0 5px 0; }
        
        /* Interactive Tab Headers */
        .tab-navigation { display: flex; border-bottom: 2px solid #e2e8f0; margin-bottom: 25px; gap: 5px; }
        .tab-btn { padding: 12px 24px; font-weight: bold; font-size: 14px; border: none; background: none; color: #64748b; cursor: pointer; transition: all 0.2s ease-in-out; border-bottom: 2px solid transparent; margin-bottom: -2px; }
        .tab-btn:hover { color: #1e293b; background-color: #f8fafc; border-radius: 6px 6px 0 0; }
        .active-tab { color: #dc2626 !important; border-bottom: 2px solid #dc2626 !important; background-color: #fff !important; }
        
        /* UI Element Tokens */
        .filter-card { background: white; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; margin-bottom: 20px; box-shadow: 0 1px 3px rgba(0,0,0,0.05); }
        .btn-primary-action { background: #dc2626; color: white; border: none; border-radius: 6px; font-weight: bold; font-size: 14px; cursor: pointer; transition: background 0.15s ease-in-out; }
        .btn-primary-action:hover { background: #b91c1c; }
        .btn-secondary-action { background: #f1f5f9; color: #334155; border: 1px solid #cbd5e1; border-radius: 6px; font-weight: bold; font-size: 14px; cursor: pointer; transition: all 0.15s ease-in-out; }
        .btn-secondary-action:hover { background: #e2e8f0; color: #0f172a; }
        
        /* Data Display Layout Cards */
        .profile-item { background: white; border: 1px solid #e2e8f0; border-radius: 8px; padding: 18px; margin-bottom: 12px; box-shadow: 0 1px 2px rgba(0,0,0,0.02); display: flex; justify-content: space-between; align-items: center; }
        .badge-high { background: #fee2e2; color: #991b1b; padding: 6px 14px; border-radius: 20px; font-size: 12px; font-weight: bold; display: inline-block; text-transform: uppercase; }
        .badge-medium { background: #fef3c7; color: #92400e; padding: 6px 14px; border-radius: 20px; font-size: 12px; font-weight: bold; display: inline-block; text-transform: uppercase; }
        .badge-low { background: #dcfce7; color: #166534; padding: 6px 14px; border-radius: 20px; font-size: 12px; font-weight: bold; display: inline-block; text-transform: uppercase; }
        
        /* Grid Alignment Matrix Styles */
        .report-grid { width: 100%; border-collapse: collapse; margin-top: 10px; background-color: white; }
        .report-grid th { background-color: #f8fafc; color: #475569; font-weight: bold; padding: 12px; text-align: left; border-bottom: 2px solid #e2e8f0; font-size: 12px; text-transform: uppercase; }
        .report-grid td { padding: 12px; border-bottom: 1px solid #f1f5f9; color: #334155; font-size: 13px; text-align: left; vertical-align: middle; }
        .report-grid tr:nth-child(even) td { background-color: #f8fafc; }
    </style>
</asp:Content>

<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <div style="padding: 5px; max-width: 1400px; margin: 0 auto;">
        
        <div class="progress-header">
            <h3>Student Performance Tracker</h3>
            <p style="margin:0; opacity:0.9; font-size:14px;">Review academic progress indices, track attendance dropouts, and export criteria snapshots.</p>
        </div>

        <asp:Label ID="lblStatusMessage" runat="server" Visible="false" Style="display: block; padding: 12px 16px; border-radius: 6px; margin-bottom: 20px; font-size: 14px; font-weight: 500;"></asp:Label>

        <div class="tab-navigation">
            <asp:Button ID="btnTabTracker" runat="server" Text="Student Tracker View" CommandArgument="0" OnClick="SwitchTab_Click" CssClass="tab-btn active-tab" UseSubmitBehavior="false" />
            <asp:Button ID="btnTabReports" runat="server" Text="Report Management Engine" CommandArgument="1" OnClick="SwitchTab_Click" CssClass="tab-btn" UseSubmitBehavior="false" />
        </div>

        <asp:MultiView ID="mvProgressViews" runat="server" ActiveViewIndex="0">
            
            <%-- VIEW INDEX 0: STUDENT PERFORMANCE TRACKER --%>
            <asp:View ID="tabTrackerView" runat="server">
                
                <div class="filter-card">
                    <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 16px; align-items: end;">
                        <div>
                            <label style="display: block; font-size: 11px; font-weight: bold; color: #64748b; margin-bottom: 6px; letter-spacing: 0.5px;">CHOOSE WORKLOAD COURSE</label>
                            <asp:DropDownList ID="ddlCourse" runat="server" Style="width: 100%; height: 38px; border: 1px solid #cbd5e1; border-radius: 6px; padding: 0 10px; color: #334155; background-color: #fff;"></asp:DropDownList>
                        </div>
                        <div>
                            <label style="display: block; font-size: 11px; font-weight: bold; color: #64748b; margin-bottom: 6px; letter-spacing: 0.5px;">RISK CRITERIA SORT</label>
                            <asp:DropDownList ID="ddlRiskLevel" runat="server" Style="width: 100%; height: 38px; border: 1px solid #cbd5e1; border-radius: 6px; padding: 0 10px; color: #334155; background-color: #fff;">
                                <asp:ListItem Text="All Progress Profiles" Value=""></asp:ListItem>
                                <asp:ListItem Text="High Risk Critical Alert" Value="High"></asp:ListItem>
                                <asp:ListItem Text="Medium Risk Watchlist" Value="Medium"></asp:ListItem>
                                <asp:ListItem Text="Low Risk Satisfactory Standards" Value="Low"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div>
                            <asp:Button ID="btnFilter" runat="server" Text="Filter Dataset" OnClick="btnFilter_Click" CssClass="btn-primary-action" Style="width: 100%; height: 38px;" />
                        </div>
                    </div>
                </div>

                <div style="margin-top: 10px;">
                    <asp:Repeater ID="rptStudentProgress" runat="server">
                        <ItemTemplate>
                            <div class="profile-item">
                                <div style="display: flex; gap: 24px; align-items: center; flex: 1;">
                                    <div style="min-width: 220px;">
                                        <h4 style="margin: 0; font-size: 16px; font-weight: bold; color: #0f172a;"><%# Eval("FullName") %></h4>
                                        <p style="margin: 3px 0 0 0; font-size: 13px; color: #64748b;"><%# Eval("StudentNo") %> &middot; <%# Eval("Email") %></p>
                                    </div>
                                    <div style="text-align: center; min-width: 110px; padding: 4px 10px; background: #f8fafc; border-radius: 6px; border: 1px solid #e2e8f0;">
                                        <span style="display: block; font-size: 10px; font-weight: bold; color: #94a3b8; text-transform: uppercase;">Course Assigned</span>
                                        <span style="font-size: 13px; font-weight: bold; color: #334155;"><%# Eval("CourseCode") %></span>
                                    </div>
                                    <div style="text-align: center; min-width: 100px;">
                                        <span style="display: block; font-size: 10px; font-weight: bold; color: #94a3b8; text-transform: uppercase;">Attendance</span>
                                        <span style="font-size: 14px; font-weight: bold; color: <%# Convert.ToDouble(Eval("AttendancePercent")) < 80.0 ? "#dc2626" : "#16a34a" %>;">
                                            <%# Eval("AttendancePercent", "{0:F1}") %>%
                                        </span>
                                    </div>
                                    <div style="text-align: center; min-width: 80px;">
                                        <span style="display: block; font-size: 10px; font-weight: bold; color: #94a3b8; text-transform: uppercase;">Proj. GPA</span>
                                        <span style="font-size: 14px; font-weight: bold; color: #0f172a;"><%# Eval("CurrentGPA", "{0:F2}") %></span>
                                    </div>
                                    <%-- ADDED TOTAL MARKS SECTION IN STUDENT TRACKER VIEW --%>
                                    <div style="text-align: center; min-width: 100px; padding: 4px 10px; background: #faf5ff; border-radius: 6px; border: 1px solid #f3e8ff;">
                                        <span style="display: block; font-size: 10px; font-weight: bold; color: #a855f7; text-transform: uppercase;">Total Marks</span>
                                        <span style="font-size: 14px; font-weight: bold; color: #6b21a8;"><%# Eval("TotalMarksObtained", "{0:F2}") %></span>
                                    </div>
                                    <div style="text-align: center; min-width: 110px;">
                                        <span style="display: block; font-size: 10px; font-weight: bold; color: #94a3b8; text-transform: uppercase;">Assessments</span>
                                        <span style="font-size: 13px; font-weight: 500; color: #475569;"><%# Eval("CompletedSubmissions") %> Graded</span>
                                    </div>
                                    <div style="margin-left: 10px; padding-left: 16px; border-left: 2px solid #f1f5f9; flex: 1;">
                                        <span style="display: block; font-size: 10px; font-weight: bold; color: #94a3b8; text-transform: uppercase;">Anomalies/Diagnostic Note</span>
                                        <p style="margin: 2px 0 0 0; font-size: 12px; color: #475569; line-height: 1.4;"><%# Eval("RiskReason") %></p>
                                    </div>
                                </div>
                                <div style="margin-left: 20px; min-width: 120px; text-align: right;">
                                    <span class='<%# "badge-" + Eval("RiskLevel").ToString().ToLower() %>'>
                                        <%# Eval("RiskLevel") %> Risk
                                    </span>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:Panel ID="pnlNoData" runat="server" Visible="false" Style="background: #f8fafc; border: 1px dashed #cbd5e1; border-radius: 8px; padding: 40px; text-align: center; margin-top: 15px;">
                    <p style="margin: 0; font-size: 14px; color: #64748b; font-weight: 500;">No matching student progress entries located for the active query filter.</p>
                </asp:Panel>
            </asp:View>

            <%-- VIEW INDEX 1: REPORT ENGINE PREVIEW AND EXPORTS --%>
            <asp:View ID="tabReportsView" runat="server">
                
                <div class="filter-card">
                    <h5 style="margin: 0 0 15px 0; color: #1e293b; font-weight: bold; font-size: 15px;">Report Query Target Matrix</h5>
                    
                    <asp:Label ID="lblReportFeedback" runat="server" Visible="false" Style="display: block; margin-bottom: 15px; font-size: 13px; padding: 8px 12px; border-radius: 6px; font-weight: bold;"></asp:Label>

                    <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 16px; align-items: end;">
                        <div>
                            <label style="display: block; font-size: 11px; font-weight: bold; color: #64748b; margin-bottom: 6px; letter-spacing: 0.5px;">ACADEMIC WORKLOAD WORKPLACE</label>
                            <asp:DropDownList ID="ddlReportCourse" runat="server" Style="width: 100%; height: 38px; border: 1px solid #cbd5e1; border-radius: 6px; padding: 0 10px; color: #334155; background-color: #fff;"></asp:DropDownList>
                        </div>
                        <div>
                            <label style="display: block; font-size: 11px; font-weight: bold; color: #64748b; margin-bottom: 6px; letter-spacing: 0.5px;">RISK MATRIX THRESHOLD EXCEPTION</label>
                            <asp:DropDownList ID="ddlReportRisk" runat="server" Style="width: 100%; height: 38px; border: 1px solid #cbd5e1; border-radius: 6px; padding: 0 10px; color: #334155; background-color: #fff;">
                                <asp:ListItem Text="Show All Risk Targets" Value="All"></asp:ListItem>
                                <asp:ListItem Text="High Risk Escalations Only" Value="High"></asp:ListItem>
                                <asp:ListItem Text="Medium Risk Watchlists Only" Value="Medium"></asp:ListItem>
                                <asp:ListItem Text="Low Risk Satisfactory Standards" Value="Low"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div>
                            <asp:Button ID="btnGenerateReport" runat="server" Text="Generate Workload Preview" CssClass="btn-secondary-action" OnClick="btnGenerateReport_Click" Style="width: 100%; height: 38px;" />
                        </div>
                    </div>
                </div>

                <asp:Panel ID="pnlReportWorkspace" runat="server" Visible="false" Style="background: white; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; margin-bottom: 24px; box-shadow: 0 1px 3px rgba(0,0,0,0.05);">
                    <div style="display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #f1f5f9; padding-bottom: 12px; margin-bottom: 15px;">
                        <div>
                            <h4 style="margin: 0; color: #0f172a; font-size: 15px; font-weight: bold;">
                                <asp:Literal ID="litReportTitle" runat="server" Text="Compiled Performance Review Dataset"></asp:Literal>
                            </h4>
                            <p style="margin: 2px 0 0 0; font-size: 12px; color: #64748b;">Verify the dataset records metrics summary profile below before compiling binary data downloads.</p>
                        </div>
                        <div style="width: 240px;">
                            <asp:Button ID="btnCompileCSV" runat="server" Text="Compile & Download CSV Report" CssClass="btn-primary-action" OnClick="btnCompileCSVReport_Click" Style="width: 100%; height: 38px; font-size: 12px;" />
                        </div>
                    </div>
                    
                    <div style="overflow-x: auto; border: 1px solid #e2e8f0; border-radius: 6px;">
                        <%-- EXPLICIT GRIDVIEW COLUMNS DEFINITION TO INCLUDE TOTAL MARKS PREVIEW --%>
                        <asp:GridView ID="gvReportPreview" runat="server" GridLines="None" Width="100%" AutoGenerateColumns="false" CssClass="report-grid" EmptyDataText="No student anomalies met this criteria scope.">
                            <Columns>
                                <asp:BoundField DataField="Student No" HeaderText="Student No" />
                                <asp:BoundField DataField="Full Name" HeaderText="Full Name" />
                                <asp:BoundField DataField="Email" HeaderText="Email" />
                                <asp:BoundField DataField="Course Code" HeaderText="Course Code" />
                                <asp:BoundField DataField="Attendance Rate" HeaderText="Attendance Rate" />
                                <asp:BoundField DataField="Projected GPA" HeaderText="Projected GPA" />
                                <asp:BoundField DataField="Total Marks" HeaderText="Total Marks" DataFormatString="{0:F2}" />
                                <asp:BoundField DataField="Risk Level" HeaderText="Risk Level" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </asp:Panel>

                <div style="background: white; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; box-shadow: 0 1px 3px rgba(0,0,0,0.05);">
                    <h5 style="margin: 0 0 5px 0; color: #1e293b; font-weight: bold;">Report Export Audit Logs</h5>
                    <p style="font-size: 12px; color: #64748b; margin: 0 0 15px 0;">Audit stream showing historical data compile routines executed by your staff entity.</p>
                    
                    <div style="overflow-x: auto; border: 1px solid #e2e8f0; border-radius: 6px;">
                        <asp:GridView ID="gvReportHistory" runat="server" AutoGenerateColumns="False" CssClass="report-grid" GridLines="None" EmptyDataText="No past system reports compiled by this entity profile." OnRowCommand="gvReportHistory_RowCommand">
                            <Columns>
                                <asp:TemplateField HeaderText="Log ID" ItemStyle-Width="90px">
                                    <ItemTemplate>
                                        <%# Eval("ExportId") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Classification" HeaderText="Classification" />
                                <asp:BoundField DataField="ScopeFilters" HeaderText="Applied Restrictions" />
                                <asp:BoundField DataField="ExportedAt" HeaderText="Value Timestamp" DataFormatString="{0:yyyy-MM-dd HH:mm}" ItemStyle-Width="160px" />
                                <asp:TemplateField HeaderText="Transaction State" ItemStyle-Width="130px">
                                    <ItemTemplate>
                                        <span style="display: inline-block; background: #dcfce7; color: #15803d; font-size: 11px; font-weight: bold; padding: 2px 8px; border-radius: 4px; text-transform: uppercase;">
                                            <%# Eval("Status") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Action" ItemStyle-Width="140px" ItemStyle-HorizontalAlign="Right">
                                    <ItemTemplate>
                                        <asp:Button ID="btnDownloadPast" runat="server" Text="Download File" 
                                                    CommandName="DownloadReportFile" 
                                                    CommandArgument='<%# Eval("FilePath") %>' 
                                                    CssClass="btn-secondary-action" 
                                                    Style="padding: 4px 10px; font-size: 12px; border-radius: 4px; height: auto; font-weight: bold;" 
                                                    UseSubmitBehavior="false" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>

            </asp:View>
        </asp:MultiView>
    </div>
</asp:Content>