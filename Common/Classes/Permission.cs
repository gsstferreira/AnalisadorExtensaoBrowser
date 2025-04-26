using Common.Enums;

namespace Common.Classes
{
    public class Permission
    {
        public string Name { get; set; }
        public PermissionType Type { get; set; }

        public Permission() 
        {
            Name = string.Empty;
            Type = PermissionType.OptionalPermission;
        }
        public Permission(string name, PermissionType type)
        {
            Name = name;
            Type = type;
        }
    }
}
