using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelebotNew.Models.Conversation
{
    public class Response
    {
        public string Text { get; set; }
    }

    public class AgentResponse : Response
    {
        public string ResponseType { get; set; }
        public List<string> GoalIDs { get; set; }
    }

    public class UserResponse : Response
    {
        public List<string> IntentIDs { get; set; }
        public string ResponseType { get; set; }
        public Dictionary<string, object> EntityValues { get; set; }
    }
}
