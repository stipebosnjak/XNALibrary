

namespace Xna.Helpers.Goap
{
    /// <summary>
    /// Class for creating a atomic goal , all atomic goals derive from this class
    /// </summary>
    /// <typeparam name="T">Type of the entity that has a brain :D</typeparam>
    public class Goal<T>
    {
        /// <summary>
        /// Contains the goal statuses
        /// </summary>
        public enum GoalStatus
        {
            /// <summary>
            /// The Goal is active
            /// </summary>
            Active,
            /// <summary>
            /// The Goal is inactive
            /// </summary>
            Inactive,
            /// <summary>
            /// The Goal is completed
            /// </summary>
            Completed,
            /// <summary>
            /// The Goal failed
            /// </summary>
            Failed,
            /// <summary>
            /// The Goal got stuck
            /// </summary>
            Stuck,
            /// <summary>
            /// There is no goal
            /// </summary>
            None
        }
        /// <summary>
        /// Type of the goal
        /// </summary>
        public enum GoalType
        {
            /// <summary>
            /// The goal that represents itself
            /// </summary>
            Atomic,
            /// <summary>
            /// The goal that contains subgoals
            /// </summary>
            Composite
        }
        /// <summary>
        /// 
        /// </summary>
        protected GoalStatus Status;
        /// <summary>
        /// 
        /// </summary>
        protected GoalType Type;
        /// <summary>
        /// 
        /// </summary>
        protected T CharOwner;

        /// <summary>
        /// Creates new goal
        /// </summary>
        /// <param name="character">The entity that has the goals</param>
        /// <param name="type">Type of the goal</param>
        public Goal(T character, GoalType type)
        {
            Type = type;
            CharOwner = character;
            Status = GoalStatus.Inactive;
           
        }
        /// <summary>
        /// Activates the specified goal
        /// </summary>
        public virtual void Activate() { }
        /// <summary>
        /// Processes the goal requirements
        /// </summary>
        /// <returns>Returns processed goal status</returns>
        public virtual GoalStatus Process() { return GoalStatus.None; }

        /// <summary>
        /// Terminates the goal
        /// </summary>
        public virtual void Terminate() { }

        public string Note { get; set; }

        #region Methods
 
      
 /// <summary>
 /// Activates the goal if it is inactive
 /// </summary>
        public void ActivateIfInactive()
        {
            if (IsInactive)
            {
                Activate();
            }
        }
 /// <summary>
 /// Sets the goal status to inactive if it is failed
 /// </summary>
        public void  ReactivateIfFailed()
        {
          if (HasFailed)
          {
             Status = GoalStatus.Inactive;
          }
        }
 /// <summary>
 /// Returns is the goal complete
 /// </summary>
        public bool IsComplete
        {
            get { return Status == GoalStatus.Completed; }
        }
        /// <summary>
        /// Returns is the goal complete
        /// </summary>
        public bool IsActive
        {
            get { return Status == GoalStatus.Active; }
        }
        /// <summary>
        /// Returns is the goal inactive
        /// </summary>
        public bool IsInactive
        {
            get { return Status == GoalStatus.Inactive; }
        }
        /// <summary>
        /// Returns is the goal failed
        /// </summary>
        public bool HasFailed
        {
            get { return Status == GoalStatus.Failed; }
        }
        /// <summary>
        /// Returns is the goal stuck
        /// </summary>
        public bool HasStuck
        {
            get { return Status == GoalStatus.Stuck; }
        }
        /// <summary>
        /// Goal type of the goal
        /// </summary>
        public GoalType GoalTypeP
        {
            get { return Type; }
        }
        /// <summary>
        /// Goal type of the goal in plain text
        /// </summary>
        public string GoalTypeString
        {
            get { return Type.ToString(); }
        }
        /// <summary>
        /// The status of the goal
        /// </summary>
        public GoalStatus GoalStatusP
        {
            get { return Status; }
            set { Status = value; }
        }
        /// <summary>
        /// The status of the goal in plain text
        /// </summary>
        public string GetGoalTypeString()
        {
            return Type.ToString();
        }
 
    
        #endregion
    }


    
}
