using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelebotNew.Implementations.RestaurantReservation
{
    public class GoalIsActiveFuncs
    {
        public Dictionary<string, Func<Dictionary<string, object>, bool>> Funcs { get; set; }
        public GoalIsActiveFuncs()
        {
            Funcs = new Dictionary<string, Func<Dictionary<string, object>, bool>>();

            Funcs.Add("GetName", delegate (Dictionary<string, object> entityValues) {
                return true;
            });
            Funcs.Add("GetFirstName", delegate (Dictionary<string, object> entityValues) {
                return true;
            });
            Funcs.Add("Greet", delegate (Dictionary<string, object> entityValues) {
                return true;
            });
            Funcs.Add("PromptForResponse", delegate (Dictionary<string, object> entityValues) {
                return true;
            });
        }
    }
}
