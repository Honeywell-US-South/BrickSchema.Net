using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes.People.Users
{
    

    public class SystemAdmin : User { }
    public class TenantAdmin : User { }
    public class CustomerAdmin : User { }
    public class CustomerUser : User { }
    public class CustomerUserViewOnly : User { }
    public class CustomerUserRestricted : User { }
}
