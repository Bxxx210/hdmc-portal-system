namespace HDMC.Portal.Models
{
    public class UserLoginModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string PasswordHash { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}
