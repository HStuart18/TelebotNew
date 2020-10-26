using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelebotNew.Models.Local
{
    public class GoalFromFile
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string ResponseType { get; set; }
        public string GoalType { get; set; }
        public string Text { get; set; }
        public bool TrySkip { get; set; }
        public List<string> SubgoalIDs { get; set; }
        public string EntityID { get; set; }
        public bool DoContinue { get; set; }
    }
}
