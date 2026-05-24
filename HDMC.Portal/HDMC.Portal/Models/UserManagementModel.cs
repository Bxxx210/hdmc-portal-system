namespace HDMC.Portal.Models
{
    public class UserManagementModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public bool IsActive { get; set; }

        public string Company { get; set; }

        public string Companies { get; set; }

        public string Applications { get; set; }

        public bool IsAllCompanies { get; set; }

        public string[] SelectedCompanyCodes { get; set; }

        public int[] SelectedAppIds { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public string SearchText { get; set; }
    }
}
