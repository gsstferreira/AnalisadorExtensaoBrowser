using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Classes
{
    public class Permission
    {
        public string Name { get; set; }
        public PermissionType Type { get; set; }

        public Permission(string name, PermissionType type) 
        {
            Name = name;
            Type = type;
        }
    }
}
