using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelebotNew.Models.Conversation
{
    // An entity is a piece of information that the agent can use to select subsequent goals. The set of entities is analagous to human memory.
    public class Entity
    {
        public string ID { get; set; } // Uniquely identifies the entity

        public Type DType { get; set; } // Specifies the datatype of the entity

        private string SerializeValue(object value) // Serialises DType value into string value
        {
            return value.ToString();
        } 

        private object DeserializeValue(string value, Type dtype) // Deserializes string value into object value
        {
            if (dtype == typeof(string))
            {
                return value;
            }
            else
            {
                return Convert.ChangeType(value, dtype);
            }
        } 

        public Func<object, Dictionary<string, object>> UpdateValueFunc { get; set; } // Set at instance level. Determines the change in values of other entities due to the change in current entity.

        public Dictionary<string, object> UpdateValue(object value, List<Entity> entities) // Recursively finds the full extent of entity value changes due to change in current entity value
        {
            Dictionary<string, object> entityValues = UpdateValueFunc.Invoke(value);

            // Be careful not to create infinite loops
            foreach (var entityId in entityValues.Keys)
            {
                if (entityId != ID)
                {
                    Dictionary<string, object> newEntityValues = entities.ToList().Find(x => x.ID == entityId).UpdateValue(entityValues[entityId], entities);
                    newEntityValues.ToList().ForEach(x => entityValues[x.Key] = x.Value);
                }
            }

            return entityValues;
        }
    }
}
