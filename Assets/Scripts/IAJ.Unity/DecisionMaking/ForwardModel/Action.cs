using Assets.Scripts.Game;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    public class Action
    {
        private static int ActionID = 0; 
        public string Name { get; set; }
        public int ID { get; set; }
        private Dictionary<Goal, float> GoalEffects { get; set; }
        public float Duration { get; set; }

        public Action(string name)
        {
            this.ID = Action.ActionID++;
            this.Name = name;
            this.GoalEffects = new Dictionary<Goal, float>();
            this.Duration = 0.0f;

        }

        public void AddEffect(Goal goal, float goalChange)
        {
            this.GoalEffects[goal] = goalChange;
        }

        // Used for GOB Decison Making
        public virtual float GetGoalChange(Goal goal)
        {
            if (this.GoalEffects.ContainsKey(goal))
            {
                return this.GoalEffects[goal];
            }
            else return 0.0f;
        }

        public virtual float GetDuration()
        {
            return this.Duration;
        }

        public virtual float GetDuration(WorldModel worldModel)
        {
            return this.Duration;
        }

        public virtual bool CanExecute(WorldModel woldModel)
        {
            return true;
        }


        public virtual bool CanExecute()
        {
            return true;
        }

        public virtual void Execute()
        {
        }

        // Used for GOAP Decison Making
        public virtual void ApplyActionEffects(WorldModel worldModel)
        {
        }

        // Used for MCTS Biased
        public virtual float GetHValue(WorldModel worldModel)
        {
            var MAX_HP = Convert.ToInt32(worldModel.GetProperty(Properties.MAXHP));
            var HP = Convert.ToInt32(worldModel.GetProperty(Properties.HP));

            var money = Convert.ToInt32(worldModel.GetProperty(Properties.MONEY));
            var time = Convert.ToInt32(worldModel.GetProperty(Properties.TIME));

            if (HP <= 0 || time >= 150)
            {
                return 1000;
            }
            if (HP > 0 && time < 150 && money == 25)
            {
                return -1000;
            }
            return - HP / MAX_HP - money / 25;
        }
        public static float GetHValueFinal(WorldModel worldModel)
        {
            var MAX_HP = Convert.ToInt32(worldModel.GetProperty(Properties.MAXHP));
            var HP = Convert.ToInt32(worldModel.GetProperty(Properties.HP));

            var money = Convert.ToInt32(worldModel.GetProperty(Properties.MONEY));
            var time = Convert.ToInt32(worldModel.GetProperty(Properties.TIME));

            if (HP <= 0 || time >= 150)
            {
                return 1000;
            }
            if (HP > 0 && time < 150 && money == 25)
            {
                return -1000;
            }
            return -HP / MAX_HP - money / 25;
        }

        public virtual float GetHValue(WorldModel worldModel, float duration)
        {
            return Action.GetHValueFinal(worldModel) + duration / 150;
        }

    }
}
