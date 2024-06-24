using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes.APIs
{
    public class API : BrickClass
    {
    }

    public class MQTTConnector : API { }
    public class MQTTInterface:MQTTConnector { }

    public class GraphQLConnector : API { }
    public class GraphQLInterface : GraphQLConnector { }

}
