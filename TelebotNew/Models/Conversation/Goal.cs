using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelebotNew.Models.Conversation
{
    public abstract class Goal
    {
        public string ID { get; set; } // Uniquely identifies goal
        public string ParentID { get; set; } // Allows for recursion up towards the root goal when traversing goal graph

        public string ResponseType { get; set; } // Allows for meta_data transmission to client

        public string Text { get; set; } // Text to constitute response back to user. Down the line, this should be a function to allow for interpolated strings etc. ("Hello Harry!")

        // A goal is achieved if the agent does not need to try and achieve it.
        // Although this sounds vague, it also allows for external achievement from DB lookups etc.
        public abstract bool IsAchieved(List<Goal> goals, Dictionary<string, object> entityValues, List<Response> responses);

        public Func<Dictionary<string, object>, bool> IsActiveFunc { get; set; }
        // A goal is active (regardless of if it's achieved) if the agent is allowed to try and achieve the goal.
        // A goal can only see entityValues when determining if it is active or not.
        // This may depend on entity values etc. Only ask if the user want delivery if their order is over $50.
        public bool IsActive(Dictionary<string, object> entityValues) 
        {
            return IsActiveFunc.Invoke(entityValues);
        }
    }

    // Child goals add structure to the goal graph, thus not requiring highly complex IsActive functions.
    // For example, DoSkip removes the need to have complex IsActive functions for NameGoal, FirstNameGoal and LastNameGoal
    public class CompositeGoal : Goal
    {
        public List<string> SubgoalIDs { get; set; }
        public bool TrySkip { get; set; }

        public override bool IsAchieved(List<Goal> goals, Dictionary<string, object> entityValues, List<Response> responses)
        {
            List<Goal> activeSubgoals = new List<Goal>();
            List<Goal> activeAndAchievedSubgoals = new List<Goal>();

            // Finds all achieved children
            foreach (Goal goal in goals)
            {
                if (SubgoalIDs.Contains(goal.ID))
                {
                    if (goal.IsActive(entityValues))
                    {
                        activeSubgoals.Add(goal);

                        if (goal.IsAchieved(goals, entityValues, responses))
                        {
                            activeAndAchievedSubgoals.Add(goal);
                        }
                    }
                }
            }

            return activeSubgoals.Count == activeAndAchievedSubgoals.Count; // Achieved if all active subgoals are achieved
        }

        // Remember that goal must not be achieved before considering traversal
        public bool DoSkip(List<Goal> goals, Dictionary<string, object> entityValues, List<Response> responses)
        {
            List<Goal> activeAndAchievedSubgoals = new List<Goal>();

            // Finds all active and achieved children
            foreach (Goal goal in goals)
            {
                if (SubgoalIDs.Contains(goal.ID))
                {
                    if (goal.IsActive(entityValues))
                    {
                        if (goal.IsAchieved(goals, entityValues, responses))
                        {
                            activeAndAchievedSubgoals.Add(goal);
                        }
                    }
                }
            }

            // Composite goal has been partially achieved, so skip.
            if (activeAndAchievedSubgoals.Count > 0)
            {
                return true;
            }
            else
            {
                return TrySkip; // Composite goal is allowed to skip, so leave decision up to TrySkip preference
            }
        }
    }

    // Check validation entity for analogy of AtomicValidationGoal
    public class AtomicRetrievalGoal : Goal
    {
        public string EntityID { get; set; }

        private bool _IsAchieved(Dictionary<string, object> entityValues)
        {
            return entityValues.Keys.Contains(EntityID) && entityValues[EntityID] != null;
        }

        // Achieved if corresponding entity has value
        public override bool IsAchieved(List<Goal> goals, Dictionary<string, object> entityValues, List<Response> responses)
        {
            return _IsAchieved(entityValues);
        }
    }

    public class AtomicInformativeGoal : Goal
    {
        public bool DoContinue { get; set; } // Determines if the agent should response with subsequent goals also.

        private bool _IsAchieved(List<Response> responses)
        {
            foreach (Response response in responses)
            {
                if (response.GetType() == typeof(AgentResponse))
                {
                    if (((AgentResponse)response).GoalIDs.Contains(ID))
                    {
                        return true; // Already informed user
                    }
                }
            }

            return false;
        }

        // Achieved if corresponding entity has value
        public override bool IsAchieved(List<Goal> goals, Dictionary<string, object> entityValues, List<Response> responses)
        {
            return _IsAchieved(responses);
        }
    }
}
