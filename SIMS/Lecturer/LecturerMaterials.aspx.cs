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

            // required for FileUpload to work reliably
            if (Page.Form != null && string.IsNullOrEmpty(Page.Form.Enctype))
                Page.Form.Enctype = "multipart/form-data";

            if (!IsPostBack)
            {
                // Expect CourseID from query string (like Attendance/Grades)
                if (string.IsNullOrEmpty(Request.QueryString["CourseID"]) || !int.TryParse(Request.QueryString["CourseID"], out int courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                // Verify lecturer has assignment for this course (any year/semester)
                if (!LecturerTeachesCourse(courseId))
                {
                    // unauthorized or no assignment
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                hidCourseId.Value = courseId.ToString();

                LoadCourseHeader(courseId);
                // derive the most recent academic year & semester for this lecturer assignment
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

                if (!fuMaterial.HasFile)
                {
                    ShowMaterialError("Please select a file to upload.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    ShowMaterialError("Please enter a material title.");
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
                txtTitle.Text = "";
                txtDescription.Text = "";
                LoadMaterials();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Upload error: {ex.Message}");
                ShowMaterialError("Error uploading material: " + ex.Message);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtTitle.Text = "";
            txtDescription.Text = "";
            chkIsVisible.Checked = true;
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                int materialId = int.Parse(((Button)sender).CommandArgument);

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
                LoadMaterials();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete error: {ex.Message}");
                ShowMaterialError("Error deleting material: " + ex.Message);
            }
        }

        protected void rptMaterials_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // optional: wire client-side download links or format rows
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
