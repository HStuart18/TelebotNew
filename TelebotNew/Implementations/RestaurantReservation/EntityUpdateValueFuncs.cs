using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelebotNew.Implementations.RestaurantReservation
{
    public class EntityUpdateValueFuncs
    {
        public Dictionary<string, Func<object, Dictionary<string, object>>> Funcs { get; set; }
        public EntityUpdateValueFuncs()
        {
            Funcs = new Dictionary<string, Func<object, Dictionary<string, object>>>();

            Funcs.Add("FirstName", delegate(object value) {
                return new Dictionary<string, object>() { { "FirstName", value } };
            });
        }
    }
}
