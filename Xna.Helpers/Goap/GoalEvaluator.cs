namespace Xna.Helpers.Goap
{
    /// <summary>
    /// Makes new Goal evaluator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GoalEvaluator<T>
    {


        /// <summary>
        ///When the desirability score for a goal has been evaluated it is multiplied 
        /// by this value. It can be used to create bots with preferences based upon
        /// their personality
        /// </summary>
        protected float CharacterBias;

        /// <summary>
        /// Creates new GoalEvaluator
        /// </summary>
        /// <param name="characterBias"></param>
        protected GoalEvaluator(float characterBias)
        {
            CharacterBias = characterBias;
        }
        /// <summary>
        /// Disposes the evaluator
        /// </summary>
        public virtual void Dispose()
        {
        }


        /// <summary>
        /// Returns a score between 0 and 1 representing the desirability of the
        /// strategy the concrete subclass represents
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract float CalculateDesirability(T character);

        
        /// <summary>
        /// Adds the appropriate goal to the given bot's brain
        /// </summary>
        /// <param name="character"></param>
        public abstract void SetGoal(T character);

        //used to provide debugging/tweaking support
        //public abstract void RenderInfo(Vector2D Position, Raven_Bot pBot);
    }
}
