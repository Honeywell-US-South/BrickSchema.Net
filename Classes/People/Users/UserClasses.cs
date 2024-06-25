using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes.People.Users
{
    

    public class SystemAdmin : User { }
    public class TenantAdmin : User { }
    public class EntityAdmin : User { }
    public class EntityUser : User { }
    public class EntityUserViewOnly : User { }
    public class EntityUserRestricted : User { }
}
