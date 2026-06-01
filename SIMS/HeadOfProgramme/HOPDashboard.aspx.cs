using System;

namespace SIMS.HeadOfProgramme
{
    public partial class Dashboard : HOPBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {

            }
        }
    }
}