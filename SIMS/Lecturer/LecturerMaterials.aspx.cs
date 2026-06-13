using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerMaterials : LecturerBase
    {
        string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString["CourseID"]) || !int.TryParse(Request.QueryString["CourseID"], out int courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                if (!LecturerTeachesCourse(courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                hidCourseId.Value = courseId.ToString();

                LoadCourseHeader(courseId);
                var assigned = GetMostRecentAssignment(courseId);
                int academicYear = assigned.academicYear > 0 ? assigned.academicYear : DateTime.Now.Year;
                int semester = assigned.semester > 0 ? assigned.semester : GetCurrentSemester();

                hidAcademicYear.Value = academicYear.ToString();
                hidSemester.Value = semester.ToString();

                litAcademicYear.Text = academicYear.ToString();
                litSemester.Text = semester.ToString();

                LoadMaterials();
            }
        }

        private void LoadCourseHeader(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT CourseName, CourseCode FROM Courses WHERE CourseId = @CourseId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                string name = r["CourseName"].ToString();
                                string code = r["CourseCode"].ToString();
                                litCourseName.Text = $"{code} - {name}";
                                litCourseHeader.Text = $"{code} - {name} (Materials)";
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading course header: {ex.Message}");
            }
        }

        private (int academicYear, int semester) GetMostRecentAssignment(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT TOP 1 AcademicYear, Semester
                        FROM CourseAssignments
                        WHERE CourseId = @CourseId
                          AND LecturerId = @LecturerId
                        ORDER BY AcademicYear DESC, Semester DESC, AssignedDate DESC";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                int y = r["AcademicYear"] != DBNull.Value ? Convert.ToInt32(r["AcademicYear"]) : 0;
                                int s = r["Semester"] != DBNull.Value ? Convert.ToInt32(r["Semester"]) : 0;
                                return (y, s);
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting assignment: {ex.Message}");
            }
            return (0, 0);
        }

        private bool LecturerTeachesCourse(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT COUNT(1) FROM CourseAssignments WHERE CourseId = @CourseId AND LecturerId = @LecturerId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);
                        conn.Open();
                        int c = Convert.ToInt32(cmd.ExecuteScalar());
                        conn.Close();
                        return c > 0;
                    }
                }
            }
            catch { return false; }
        }

        void LoadMaterials()
        {
            try
            {
                int courseId = int.Parse(hidCourseId.Value);
                int year = int.TryParse(hidAcademicYear.Value, out int y) ? y : DateTime.Now.Year;
                int semester = int.TryParse(hidSemester.Value, out int s) ? s : GetCurrentSemester();

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT MaterialId, Title, Description, FileUrl, FileType, FileSizeKB, AcademicYear, Semester, IsVisible, UploadedAt
                        FROM CourseMaterials
                        WHERE CourseId = @CourseId
                          AND AcademicYear = @Year
                          AND Semester = @Semester
                        ORDER BY UploadedAt DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@Year", year);
                        cmd.Parameters.AddWithValue("@Semester", semester);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        rptMaterials.DataSource = dt;
                        rptMaterials.DataBind();

                        pnlNoMaterials.Visible = (dt.Rows.Count == 0);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading materials: {ex.Message}");
                pnlNoMaterials.Visible = true;
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                int courseId = int.Parse(hidCourseId.Value);
                int year = int.TryParse(hidAcademicYear.Value, out int y) ? y : DateTime.Now.Year;
                int semester = int.TryParse(hidSemester.Value, out int s) ? s : GetCurrentSemester();

                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    ShowMaterialError("Please enter a material title.");
                    return;
                }

                bool isEditing = !string.IsNullOrEmpty(hidEditingMaterialId.Value);

                if (isEditing)
                {
                    int materialId = int.Parse(hidEditingMaterialId.Value);
                    string newFileUrl = null;

                    // Check if user uploaded a replacement
                    if (fuMaterialReplace.HasFile)
                    {
                        string[] allowed = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".zip" };
                        string ext = Path.GetExtension(fuMaterialReplace.FileName).ToLower();

                        if (Array.IndexOf(allowed, ext) < 0) { ShowMaterialError("File type not allowed."); return; }
                        if (fuMaterialReplace.PostedFile.ContentLength > 52428800) { ShowMaterialError("File size exceeds 50MB."); return; }

                        // Save new file
                        string fileName = Guid.NewGuid().ToString() + ext;
                        string folder = Server.MapPath("~/UploadedMaterials/");
                        string path = Path.Combine(folder, fileName);
                        fuMaterialReplace.SaveAs(path);
                        newFileUrl = "/UploadedMaterials/" + fileName;

                        // Optionally delete old file (requires fetching old path first)
                        // Implementation omitted for brevity; verify existing file and delete via File.Delete()
                    }

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        string sql = newFileUrl != null
                            ? "UPDATE CourseMaterials SET Title = @Title, Description = @Description, IsVisible = @IsVisible, FileUrl = @FileUrl, FileType = @FileType WHERE MaterialId = @MaterialId"
                            : "UPDATE CourseMaterials SET Title = @Title, Description = @Description, IsVisible = @IsVisible WHERE MaterialId = @MaterialId";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(txtDescription.Text) ? "" : txtDescription.Text.Trim());
                            cmd.Parameters.AddWithValue("@IsVisible", chkIsVisible.Checked ? 1 : 0);
                            cmd.Parameters.AddWithValue("@MaterialId", materialId);

                            if (newFileUrl != null)
                            {
                                cmd.Parameters.AddWithValue("@FileUrl", newFileUrl);
                                cmd.Parameters.AddWithValue("@FileType", Path.GetExtension(newFileUrl).TrimStart('.').ToUpper());
                            }

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                    ShowMaterialSuccess("Material updated successfully.");
                    ResetFormState();
                    LoadMaterials();
                }
                else
                {
                    // Standard File Creation Flow
                    if (!fuMaterial.HasFile)
                    {
                        ShowMaterialError("Please select a file to upload.");
                        return;
                    }

                    string[] allowed = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".zip" };
                    string ext = Path.GetExtension(fuMaterial.FileName).ToLower();
                    if (Array.IndexOf(allowed, ext) < 0)
                    {
                        ShowMaterialError("File type not allowed.");
                        return;
                    }

                    if (fuMaterial.PostedFile.ContentLength > 52428800)
                    {
                        ShowMaterialError("File size exceeds 50MB.");
                        return;
                    }

                    string fileName = Guid.NewGuid().ToString() + ext;
                    string folder = Server.MapPath("~/UploadedMaterials/");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    string path = Path.Combine(folder, fileName);
                    fuMaterial.SaveAs(path);

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        string sql = @"
                            INSERT INTO CourseMaterials
                            (CourseId, UploadedBy, Title, Description, FileUrl, FileType, FileSizeKB, AcademicYear, Semester, IsVisible, UploadedAt)
                            VALUES (@CourseId, @UploadedBy, @Title, @Description, @FileUrl, @FileType, @FileSizeKB, @AcademicYear, @Semester, @IsVisible, GETDATE())";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@CourseId", courseId);
                            cmd.Parameters.AddWithValue("@UploadedBy", CurrentUserId);
                            cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(txtDescription.Text) ? "" : txtDescription.Text.Trim());
                            cmd.Parameters.AddWithValue("@FileUrl", "/UploadedMaterials/" + fileName);
                            cmd.Parameters.AddWithValue("@FileType", ext.TrimStart('.').ToUpper());
                            cmd.Parameters.AddWithValue("@FileSizeKB", fuMaterial.PostedFile.ContentLength / 1024);
                            cmd.Parameters.AddWithValue("@AcademicYear", year);
                            cmd.Parameters.AddWithValue("@Semester", semester);
                            cmd.Parameters.AddWithValue("@IsVisible", chkIsVisible.Checked ? 1 : 0);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }
                    }

                    ShowMaterialSuccess("Material uploaded successfully.");
                    ResetFormState();
                    LoadMaterials();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing request: {ex.Message}");
                ShowMaterialError("Operation error: " + ex.Message);
            }
        }

        protected void rptMaterials_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int materialId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "ToggleVisibility")
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        string sql = "UPDATE CourseMaterials SET IsVisible = IsVisible ^ 1 WHERE MaterialId = @MaterialId";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaterialId", materialId);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                    ShowMaterialSuccess("Visibility updated successfully.");
                    LoadMaterials();
                }
                catch (Exception ex)
                {
                    ShowMaterialError("Error toggling visibility: " + ex.Message);
                }
            }
            else if (e.CommandName == "EditMaterial")
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        string sql = "SELECT Title, Description, FileUrl, IsVisible FROM CourseMaterials WHERE MaterialId = @MaterialId";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaterialId", materialId);
                            conn.Open();
                            using (SqlDataReader r = cmd.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    txtTitle.Text = r["Title"].ToString();
                                    txtDescription.Text = r["Description"].ToString();
                                    chkIsVisible.Checked = Convert.ToBoolean(r["IsVisible"]);

                                    hidEditingMaterialId.Value = materialId.ToString();
                                    litFormMode.Text = "Edit Material Metadata";
                                    btnUpload.Text = "Save Changes";

                                    divFileUpload.Visible = false;
                                    divFileCurrent.Visible = true;
                                    litCurrentFile.Text = r["FileUrl"].ToString();
                                }
                            }
                            conn.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowMaterialError("Error retrieving material configuration: " + ex.Message);
                }
            }
            else if (e.CommandName == "DeleteMaterial")
            {
                ExecuteDeletion(materialId);
            }
        }

        private void ExecuteDeletion(int materialId)
        {
            try
            {
                string fileUrl = null;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sel = "SELECT FileUrl FROM CourseMaterials WHERE MaterialId = @MaterialId";
                    using (SqlCommand cmd = new SqlCommand(sel, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaterialId", materialId);
                        conn.Open();
                        object o = cmd.ExecuteScalar();
                        conn.Close();
                        if (o != null) fileUrl = o.ToString();
                    }

                    string del = "DELETE FROM CourseMaterials WHERE MaterialId = @MaterialId";
                    using (SqlCommand cmd = new SqlCommand(del, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaterialId", materialId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                if (!string.IsNullOrEmpty(fileUrl))
                {
                    string fp = Server.MapPath("~" + fileUrl);
                    if (File.Exists(fp)) File.Delete(fp);
                }

                ShowMaterialSuccess("Material deleted.");
                if (hidEditingMaterialId.Value == materialId.ToString())
                {
                    ResetFormState();
                }
                LoadMaterials();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete error: {ex.Message}");
                ShowMaterialError("Error deleting material: " + ex.Message);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ResetFormState();
        }

        private void ResetFormState()
        {
            txtTitle.Text = "";
            txtDescription.Text = "";
            chkIsVisible.Checked = true;
            hidEditingMaterialId.Value = "";
            litFormMode.Text = "Upload New Material";
            btnUpload.Text = "Upload Material";
            divFileUpload.Visible = true;
            divFileCurrent.Visible = false;
            litCurrentFile.Text = "";
        }

        protected void rptMaterials_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // Optional layout customization hooks
        }

        private void ShowMaterialSuccess(string msg) { pnlMaterialSuccess.Visible = true; litMaterialSuccessMsg.Text = msg; pnlMaterialError.Visible = false; }
        private void ShowMaterialError(string msg) { pnlMaterialError.Visible = true; litMaterialErrorMsg.Text = msg; pnlMaterialSuccess.Visible = false; }

        private int GetCurrentSemester()
        {
            int m = DateTime.Now.Month;
            if (m <= 4) return 1;
            if (m <= 8) return 2;
            return 3;
        }
    }
}