using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Xna.Helpers.Goap
{
    /// <summary>
    ///  Class for a creating a composite goal , all composite goals derive from this class
    /// </summary>
    /// <typeparam name="T">Type of the entity that has a brain :D</typeparam>
    public class GoalComposite<T> :Goal<T>
    {
        public ReadOnlyCollection<Goal<T>> SubGoalsReadOnly
        {
            get { return SubGoals.AsReadOnly(); }
        }

        /// <summary>
        /// Contains Subgoals
        /// </summary>
        protected List<Goal<T>> SubGoals;
        /// <summary>
        /// Creates new goal
        /// </summary>
        /// <param name="character">The entity that has the goals</param>
        /// <param name="type">Type of the goal</param>
        public GoalComposite(T character, GoalType type)
            : base(character, type)
        {
            SubGoals = new List<Goal<T>>();

        }

        /// <summary>
        /// Activates the specified goal
        /// </summary>
        public new virtual void Activate()
        {
            base.Activate();
        }

        /// <summary>
        /// Adds new goal to the list
        /// </summary>
        /// <param name="g"></param>
        public virtual void AddSubgoal(Goal<T> g)
        {
            SubGoals.Add(g);
           
        }
        /// <summary>
        /// Processes the goal requirements
        /// </summary>
        /// <returns>Returns processed goal status</returns>
        public new virtual Goal<T>.GoalStatus Process()
        {
            return base.Process();
        }
        /// <summary>
        /// Terminates the goal
        /// </summary>
        public new virtual void Terminate()
        {
            base.Terminate();
        }
        /// <summary>
        /// Processes subgoals
        /// </summary>
        /// <returns></returns>
        public GoalStatus ProcessSubGoals()
        {
            //remove all completed and failed goals from the front of the subgoal list
            while (SubGoals.Count != 0 && (SubGoals[0].IsComplete || SubGoals[0].HasFailed))
            {
                SubGoals[0].Terminate();
              //  SubGoals[0] = null;
                SubGoals.RemoveAt(0);
                
            }

            //if any subgoals remain, process the one at the front of the list
            if (SubGoals.Count!=0)
            {
                //grab the status of the frontmost subgoal
               
                SubGoals[0].Process();
                GoalStatus goalStatus = SubGoals[0].GoalStatusP;
                //we have to test for the special case where the frontmost subgoal
                //reports "completed" and the subgoal list contains additional goals.
                //When this is the case, to ensure the parent keeps processing its
                //subgoal list,the "active" status is returned
                if (goalStatus ==GoalStatus.Completed && SubGoals.Count>1)
                {
                    return GoalStatus.Active;
                }

                return goalStatus;
            }

            //no more subgoals to process return "completed"
            else
            {
                return GoalStatus.Completed;
            }

        }
        /// <summary>
        /// Removes all Subgoals from list
        /// </summary>
        public void RemoveAllSubgoals()
        {
            foreach (Goal<T> subGoal in SubGoals)
            {
                subGoal.Terminate();
            }
            SubGoals.Clear();
        }
        
    }
}
