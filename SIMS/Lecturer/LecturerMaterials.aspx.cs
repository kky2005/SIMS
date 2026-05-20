using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace SIMS.Lecturer
{
    public partial class LecturerMaterials : System.Web.UI.Page
    {
        string connStr = ConfigurationManager
            .ConnectionStrings["SIMS_DB"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LecturerID"] == null)
            {
                Response.Redirect("LecturerLogin.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadCourses();
                LoadMaterials();
            }
        }

        void LoadCourses()
        {
            int lecturerId = (int)Session["LecturerID"];
            int currentYear = DateTime.Now.Year;

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                SELECT DISTINCT
                    c.CourseId,
                    c.CourseCode,
                    c.CourseName
                FROM CourseAssignments ca
                INNER JOIN Courses c ON c.CourseId = ca.CourseId
                WHERE ca.LecturerId = @LecturerId
                  AND ca.AcademicYear = @Year
                ORDER BY c.CourseCode ASC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
            cmd.Parameters.AddWithValue("@Year", currentYear);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            ddlCourse.DataSource = dt;
            ddlCourse.DataTextField = "CourseName";
            ddlCourse.DataValueField = "CourseId";
            ddlCourse.DataBind();
        }

        void LoadMaterials()
        {
            int lecturerId = (int)Session["LecturerID"];
            int currentYear = DateTime.Now.Year;

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                SELECT
                    cm.MaterialId,
                    cm.Title,
                    cm.FileType,
                    c.CourseName,
                    cm.Semester,
                    cm.IsVisible,
                    cm.UploadedAt,
                    cm.FileSizeKB
                FROM CourseMaterials cm
                INNER JOIN Courses c ON c.CourseId = cm.CourseId
                INNER JOIN CourseAssignments ca 
                    ON ca.CourseId = cm.CourseId
                    AND ca.AcademicYear = cm.AcademicYear
                WHERE ca.LecturerId = @LecturerId
                  AND cm.AcademicYear = @Year
                ORDER BY cm.UploadedAt DESC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
            cmd.Parameters.AddWithValue("@Year", currentYear);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                rptMaterials.DataSource = dt;
                rptMaterials.DataBind();
                pnlNoMaterials.Visible = false;
            }
            else
            {
                pnlNoMaterials.Visible = true;
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (!fuMaterial.HasFile)
            {
                pnlError.Visible = true;
                litErrorMsg.Text = "Please select a file to upload.";
                return;
            }

            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                pnlError.Visible = true;
                litErrorMsg.Text = "Please enter a material title.";
                return;
            }

            try
            {
                // Validate file type
                string[] allowedExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".zip" };
                string fileExtension = Path.GetExtension(fuMaterial.FileName).ToLower();

                if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
                {
                    pnlError.Visible = true;
                    litErrorMsg.Text = "File type not allowed. Allowed types: PDF, DOC, DOCX, PPT, PPTX, XLS, XLSX, TXT, ZIP";
                    return;
                }

                // Check file size (50MB max)
                if (fuMaterial.PostedFile.ContentLength > 52428800)
                {
                    pnlError.Visible = true;
                    litErrorMsg.Text = "File size exceeds 50MB limit.";
                    return;
                }

                int lecturerId = (int)Session["LecturerID"];
                int courseId = int.Parse(ddlCourse.SelectedValue);
                int semester = int.Parse(ddlSemester.SelectedValue);
                int academicYear = int.Parse(ddlAcademicYear.SelectedValue);

                // Generate unique filename
                string fileName = Guid.NewGuid().ToString() + fileExtension;
                string filePath = Server.MapPath("~/UploadedMaterials/") + fileName;

                // Create directory if not exists
                if (!Directory.Exists(Server.MapPath("~/UploadedMaterials/")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/UploadedMaterials/"));
                }

                // Save file
                fuMaterial.SaveAs(filePath);

                // Insert into database
                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    INSERT INTO CourseMaterials 
                    (CourseId, UploadedBy, Title, Description, FileUrl, FileType, FileSizeKB, AcademicYear, Semester, IsVisible, UploadedAt)
                    VALUES (@CourseId, @UploadedBy, @Title, @Description, @FileUrl, @FileType, @FileSizeKB, @AcademicYear, @Semester, @IsVisible, @UploadedAt)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CourseId", courseId);
                cmd.Parameters.AddWithValue("@UploadedBy", lecturerId);
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(txtDescription.Text) ? "" : txtDescription.Text);
                cmd.Parameters.AddWithValue("@FileUrl", "/UploadedMaterials/" + fileName);
                cmd.Parameters.AddWithValue("@FileType", fileExtension.Substring(1).ToUpper());
                cmd.Parameters.AddWithValue("@FileSizeKB", fuMaterial.PostedFile.ContentLength / 1024);
                cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                cmd.Parameters.AddWithValue("@Semester", semester);
                cmd.Parameters.AddWithValue("@IsVisible", chkIsVisible.Checked ? 1 : 0);
                cmd.Parameters.AddWithValue("@UploadedAt", DateTime.Now);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                pnlSuccess.Visible = true;
                litSuccessMsg.Text = "Material uploaded successfully!";
                btnClear_Click(null, null);
                LoadMaterials();
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litErrorMsg.Text = "Error uploading file: " + ex.Message;
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtTitle.Text = "";
            txtDescription.Text = "";
            chkIsVisible.Checked = true;
        }

        protected void btnToggleVisibility_Click(object sender, EventArgs e)
        {
            System.Web.UI.WebControls.Button btn = (System.Web.UI.WebControls.Button)sender;
            int materialId = int.Parse(btn.CommandArgument);

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                UPDATE CourseMaterials 
                SET IsVisible = CASE WHEN IsVisible = 1 THEN 0 ELSE 1 END
                WHERE MaterialId = @MaterialId";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MaterialId", materialId);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            pnlSuccess.Visible = true;
            litSuccessMsg.Text = "Material visibility updated successfully!";
            LoadMaterials();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            System.Web.UI.WebControls.Button btn = (System.Web.UI.WebControls.Button)sender;
            int materialId = int.Parse(btn.CommandArgument);

            try
            {
                SqlConnection conn = new SqlConnection(connStr);

                // Get file path first
                string selectSql = "SELECT FileUrl FROM CourseMaterials WHERE MaterialId = @MaterialId";
                SqlCommand selectCmd = new SqlCommand(selectSql, conn);
                selectCmd.Parameters.AddWithValue("@MaterialId", materialId);
                conn.Open();
                object fileUrl = selectCmd.ExecuteScalar();
                conn.Close();

                // Delete from database
                string deleteSql = "DELETE FROM CourseMaterials WHERE MaterialId = @MaterialId";
                SqlCommand deleteCmd = new SqlCommand(deleteSql, conn);
                deleteCmd.Parameters.AddWithValue("@MaterialId", materialId);
                conn.Open();
                deleteCmd.ExecuteNonQuery();
                conn.Close();

                // Delete file from server
                if (fileUrl != null)
                {
                    string filePath = Server.MapPath("~" + fileUrl.ToString());
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                pnlSuccess.Visible = true;
                litSuccessMsg.Text = "Material deleted successfully!";
                LoadMaterials();
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litErrorMsg.Text = "Error deleting material: " + ex.Message;
            }
        }

        protected void rptMaterials_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            // Additional logic for material items if needed
        }
    }
}
