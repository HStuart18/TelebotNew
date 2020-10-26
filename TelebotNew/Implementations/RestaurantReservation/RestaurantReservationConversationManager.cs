using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelebotNew.Models.Conversation;
using System.IO;
using TelebotNew.Implementations.RestaurantReservation;
using Newtonsoft.Json;
using TelebotNew.Models.Local;
using TelebotNew;

namespace TelebotNew.Implementations.RestaurantReservation
{
    public class RestaurantReservationConversationManager : IConversationManager
    {
        public List<Entity> Entities { get; set; }
        public List<Goal> Goals { get; set; }

        public List<Intent> Intents { get; set; }

        public RestaurantReservationConversationManager()
        {
            Entities = LoadEntities();
            Goals = LoadGoals(new List<string>(new string[] { "PromptForResponse" }));
            Intents = LoadIntents();
        }

        public List<Entity> LoadEntities() // Loads entities from file
        {
            List<EntityFromFile> entitiesFromFile;

            using (StreamReader r = new StreamReader("Implementations/RestaurantReservation/entities.json"))
            {
                string json = r.ReadToEnd();
                entitiesFromFile = JsonConvert.DeserializeObject<List<EntityFromFile>>(json);
            }

            Dictionary<string, Func<object, Dictionary<string, object>>> updateValueFuncs = new EntityUpdateValueFuncs().Funcs; // Loads entity update value funcs

            // Check to make sure that the IDs in updateValueFuncs is the same as in entitiesFromFile
            if (updateValueFuncs.Count != updateValueFuncs.Keys.Union(entitiesFromFile.Select(x => x.ID)).Count())
            {
                throw new Exception("Discrepancy between entities from file and updateValueFuncs");
            }

            List<Entity> entities = new List<Entity>();

            foreach (EntityFromFile entityFromFile in entitiesFromFile)
            {
                Type type;

                if (entityFromFile.DType == "string")
                {
                    type = typeof(string);
                }
                else
                {
                    type = typeof(Type);
                }

                entities.Add(new Entity()
                {
                    ID = entityFromFile.ID,
                    DType = type,
                    UpdateValueFunc = updateValueFuncs[entityFromFile.ID]
                });
            }

            return entities;
        }

        public List<Goal> LoadGoals(List<string> requiredGoalIDs)
        {
            List<GoalFromFile> goalsFromFile;

            using (StreamReader r = new StreamReader("Implementations/RestaurantReservation/goals.json"))
            {
                string json = r.ReadToEnd();
                goalsFromFile = JsonConvert.DeserializeObject<List<GoalFromFile>>(json);
            }

            Dictionary<string, Func<Dictionary<string, object>, bool>> isActiveFuncs = new GoalIsActiveFuncs().Funcs; // Loads goal IsActvie funcs

            // Check to make sure that the IDs in IsActiveFuncs is the same as in goalsFromFile
            if (isActiveFuncs.Count != isActiveFuncs.Keys.Union(goalsFromFile.Select(x => x.ID)).Count())
            {
                throw new Exception("Discrepancy between goals from file and IsActive funcs");
            }

            List<Goal> goals = new List<Goal>();

            foreach (GoalFromFile goalFromFile in goalsFromFile)
            {
                if (goalFromFile.GoalType == "CompositeGoal")
                {
                    goals.Add(new CompositeGoal()
                    {
                        ID = goalFromFile.ID,
                        ParentID = goalFromFile.ParentID,
                        SubgoalIDs = goalFromFile.SubgoalIDs,
                        TrySkip = goalFromFile.TrySkip,
                        Text = goalFromFile.Text,
                        IsActiveFunc = isActiveFuncs[goalFromFile.ID],
                        ResponseType = goalFromFile.ResponseType
                    });
                }
                else if (goalFromFile.GoalType == "AtomicRetrievalGoal")
                {
                    if (!Entities.Select(x => x.ID).Contains(goalFromFile.EntityID))
                    {
                        throw new Exception("Goal references an entity that doesn not exist");
                    }

                    goals.Add(new AtomicRetrievalGoal()
                    {
                        ID = goalFromFile.ID,
                        ParentID = goalFromFile.ParentID,
                        Text = goalFromFile.Text,
                        IsActiveFunc = isActiveFuncs[goalFromFile.ID],
                        EntityID = goalFromFile.EntityID,
                        ResponseType = goalFromFile.ResponseType
                    });
                }
                else if (goalFromFile.GoalType == "AtomicInformativeGoal")
                {
                    goals.Add(new AtomicInformativeGoal()
                    {
                        ID = goalFromFile.ID,
                        ParentID = goalFromFile.ParentID,
                        Text = goalFromFile.Text,
                        IsActiveFunc = isActiveFuncs[goalFromFile.ID],
                        DoContinue = goalFromFile.DoContinue,
                        ResponseType = goalFromFile.ResponseType
                    });
                }
            }

            foreach (string goalID in requiredGoalIDs)
            {
                if (!goals.Select(x => x.ID).Contains(goalID))
                {
                    throw new Exception("A required goal has not been implemented");
                }
            }

            return goals;
        }

        public List<Intent> LoadIntents()
        {
            List<IntentFromFile> intentsFromFile;

            using (StreamReader r = new StreamReader("Implementations/RestaurantReservation/intents.json"))
            {
                string json = r.ReadToEnd();
                intentsFromFile = JsonConvert.DeserializeObject<List<IntentFromFile>>(json);
            }

            foreach (IntentFromFile intentFromFile in intentsFromFile)
            {
                if (!Goals.Select(x => x.ID).Contains(intentFromFile.StartGoalID))
                {
                    throw new Exception("Intent references a goal that does not exist");
                }
            }

            List<Intent> intents = new List<Intent>();

            foreach (IntentFromFile intentFromFile in intentsFromFile)
            {
                intents.Add(new Intent()
                {
                    ID = intentFromFile.ID,
                    StartGoalID = intentFromFile.StartGoalID
                });
            }

            return intents;
        }

        public async Task<UserResponse> ParseUserText(string userText, List<Response> responses)
        {
            string responseString = await AppServiceClient.client.GetStringAsync($"https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/0006203c-aabd-4bd2-8665-e463ed9314c1/slots/staging/predict?subscription-key=be53086b4a72494c90eeb673aeb10860&verbose=true&show-all-intents=true&log=true&query={userText}");
            dynamic json = JsonConvert.DeserializeObject(responseString);

            string responseType = "";
            List<string> intentIDs = new List<string>();
            Dictionary<string, object> entityValues = new Dictionary<string, object>();

            if (json.prediction.topIntent == "Greeting")
            {
                intentIDs.Add("Greet");
            }
            else if (json.prediction.topIntent == "None")
            {
                intentIDs.Add("Fallback");
            }
            else if (json.prediction.topIntent == "Unsure")
            {
                AgentResponse recentAgentResponse = (AgentResponse)responses.FindLast(x => x.GetType() == typeof(AgentResponse));
                intentIDs.Add($"Unsure{recentAgentResponse.GoalIDs.Last()}");
            }
            else if (json.prediction.topIntent == "No")
            {
                AgentResponse recentAgentResponse = (AgentResponse)responses.FindLast(x => x.GetType() == typeof(AgentResponse));
                Goal goal = Goals.FindLast(x => recentAgentResponse.GoalIDs.Contains(x.ID) && x.GetType() == typeof(AtomicRetrievalGoal));

                if (goal == null)
                {
                    intentIDs.Add($"Unsure{recentAgentResponse.GoalIDs.Last()}");
                }
                else
                {
                    foreach (string goalID in recentAgentResponse.GoalIDs)
                    {
                        if (Goals.Find(x => x.ID == goalID).GetType() == typeof(AtomicRetrievalGoal))
                        {
                            intentIDs.Add($"Validate{goalID}");
                        }
                    }
                }
            }

            // Fix up this method lmao

            return new UserResponse();
        }

        // Finds necessary entity updates as per UserResponse
        public Dictionary<string, object> GetEntityData(UserResponse userResponse) // Remember that UserResponse is created by ParseUserText
        {
            Dictionary<string, object> entityValues = new Dictionary<string, object>();

            foreach (string entityId in userResponse.EntityValues.Keys)
            {
                Dictionary<string, object> newEntityValues = Entities.ToList().Find(x => x.ID == entityId).UpdateValue(userResponse.EntityValues[entityId], Entities);
                newEntityValues.ToList().ForEach(x => entityValues[x.Key] = x.Value);
            }

            return entityValues;
        }

        // Traverses the graph of goals to find the next list of goals
        private List<Goal> TraverseGoalGraph(string nextGoalID, Dictionary<string, object> entityValues, List<Response> responses)
        {
            List<Goal> goalList = new List<Goal>();

            Goal currentGoal;

            bool canTraverse = true;

            while (canTraverse)
            {
                currentGoal = Goals.Find(x => x.ID == nextGoalID);

                if (!currentGoal.IsActive(entityValues) || currentGoal.IsAchieved(Goals, entityValues, responses))
                {
                    nextGoalID = currentGoal.ParentID;
                    continue;
                }

                if (currentGoal.GetType() == typeof(CompositeGoal))
                {
                    if (((CompositeGoal)currentGoal).DoSkip(Goals, entityValues, responses))
                    {
                        foreach (string subgoalID in ((CompositeGoal)currentGoal).SubgoalIDs)
                        {
                            Goal subgoal = Goals.Find(x => x.ID == subgoalID);

                            if (subgoal.IsActive(entityValues) && !subgoal.IsAchieved(Goals, entityValues, responses))
                            {
                                nextGoalID = subgoalID;
                                break;
                            }
                        }
                    }
                    else
                    {
                        goalList.Add(currentGoal);
                        canTraverse = false;
                    }
                }
                else if (currentGoal.GetType() == typeof(AtomicInformativeGoal))
                {
                    goalList.Add(currentGoal);
                    canTraverse = ((AtomicInformativeGoal)currentGoal).DoContinue; // If DoConitnue == True, then will continue traversing graph. Otherwise it will stop.
                }
                else
                {
                    goalList.Add(currentGoal);
                    canTraverse = false;
                }
            }

            return goalList;
        }

        // Identifies the next goal/s that the agent will try to achieve 
        private List<Goal> FindResponseGoals(List<Response> responses, Dictionary<string, object> entityValues, List<Intent> intents)
        {
            UserResponse recentUserResponse = (UserResponse)responses.FindLast(x => x.GetType() == typeof(UserResponse));
            AgentResponse recentAgentResponse = (AgentResponse)responses.FindLast(x => x.GetType() == typeof(AgentResponse));
            List<Intent> recentUserIntents = intents.FindAll(x => recentUserResponse.IntentIDs.Contains(x.ID));

            List<Goal> responseGoals = new List<Goal>();

            string startGoalID;

            // User hasn't responded
            if (recentUserResponse.ResponseType == "Interact.NoResponse")
            {
                startGoalID = "PromptForResponse";

                List<Goal> nextGoals = TraverseGoalGraph(startGoalID, entityValues, responses);

                foreach (Goal goal in nextGoals)
                {
                    if (!responseGoals.Contains(goal))
                    {
                        responseGoals.Add(goal);
                    }
                }

                return responseGoals;
            }

            responseGoals.AddRange(TraverseGoalGraph(recentUserIntents.Last().StartGoalID, entityValues, responses));

            return responseGoals;
        }

        // Creates AgentResponse object for communication back to the client/user
        public AgentResponse Respond(List<Response> responses, Dictionary<string, object> entityValues, List<Intent> intents)
        {
            List<Goal> responseGoals = FindResponseGoals(responses, entityValues, intents);
            string responseText = "";
            string responseType;

            if (responseGoals.Count == 0)
            {
                responseType = "EndConversation";
            }
            else
            {
                responseType = responseGoals.Last().ResponseType;
            }

            foreach (Goal goal in responseGoals)
            {
                responseText = responseText + goal.Text + " ";
            }

            AgentResponse agentResponse = new AgentResponse()
            {
                Text = responseText,
                ResponseType = responseType,
                GoalIDs = responseGoals.Select(x => x.ID).ToList()
            };

            return agentResponse;
        }
    }
}
