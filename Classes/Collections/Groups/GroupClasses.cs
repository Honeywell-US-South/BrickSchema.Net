using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes.Collections.Groups
{
    public class CustomerGroup:Group { }
    public class PeopleGroup:Group { }
    public class  AssetGroup : Group { }
    public class UserGroup : Group { }
    public class SystemAdminUserGroup : UserGroup { }
    public class TenantAdminUserGroup : UserGroup { }
    public class EntityAdminUserGroup : UserGroup { }
    public class EntityUserGroup : UserGroup { }
    public class EntityUserReadOnlyGroup : UserGroup { }

    public class APIGroup : Group { }
    public class GraphQLApiGroup : APIGroup { }
    public class MQTTApiGroup : APIGroup { }
}
