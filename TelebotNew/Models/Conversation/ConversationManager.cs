using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TelebotNew.Models.Local;
using System.IO;
using TelebotNew.Implementations.RestaurantReservation;

namespace TelebotNew.Models.Conversation
{
    public interface IConversationManager
    {
        public List<Entity> Entities { get; set; }

        public List<Goal> Goals { get; set; }

        public List<Intent> Intents { get; set; }

        public List<Entity> LoadEntities();

        public List<Goal> LoadGoals(List<string> requiredGoalIDs);

        public List<Intent> LoadIntents();

        public Task<UserResponse> ParseUserText(string userText, List<Response> responses);

        public Dictionary<string, object> GetEntityData(UserResponse userResponse); // Make lookup/autofill calls here

        public AgentResponse Respond(List<Response> responses, Dictionary<string, object> entityData, List<Intent> intents);
    }
}
