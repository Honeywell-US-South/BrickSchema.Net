using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes.Collections.Groups.UserGroups
{
    public class SystemAdminUserGroup : UserGroup { }
    public class TenantAdminUserGroup : UserGroup { }
    public class CustomerAdminUserGroup : UserGroup { }
    public class CustomerUserGroup : UserGroup { }
    public class CustomerUserViewOnlyGroup : UserGroup { }
    public class CustomerUserRestrictedGroup : UserGroup { }
}
