using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes.Collections.Groups.UserGroups
{
    public class SystemAdminUserGroup : UserGroup { }
    public class TenantAdminUserGroup : UserGroup { }
    public class EntityAdminUserGroup : UserGroup { }
    public class EntityUserGroup : UserGroup { }
    public class EntityUserViewOnlyGroup : UserGroup { }
    public class EntityUserRestrictedGroup : UserGroup { }
}
