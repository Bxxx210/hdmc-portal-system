namespace HDMC.Portal.Models
{
    public class UserManagementModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public bool IsActive { get; set; }

        public string Company { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public string SearchText { get; set; }
    }
}
