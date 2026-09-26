using System.Collections.Generic;

namespace DarV2.DTOs.Role
{
    public class RoleDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class RoleAddDTO
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
