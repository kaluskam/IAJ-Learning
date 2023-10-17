using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.Game;
using System;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class LevelUp : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public LevelUp(AutonomousCharacter character) : base("LevelUp")
        {
            this.Character = character;
            this.Duration = AutonomousCharacter.LEVELING_INTERVAL;
        }

        public override bool CanExecute()
        {
            var level = Character.baseStats.Level;
            var xp = Character.baseStats.XP;

            return xp >= level * 10;
        }
        

        public override bool CanExecute(WorldModel worldModel)
        {
            int xp = Convert.ToInt32(worldModel.GetProperty(Properties.XP));
            int level = (int)worldModel.GetProperty(Properties.LEVEL);

            return xp >= level * 10;
        }

        public override void Execute()
        {
            GameManager.Instance.LevelUp();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            int maxHP = (int)worldModel.GetProperty(Properties.MAXHP);
            int level = (int)worldModel.GetProperty(Properties.LEVEL);

            worldModel.SetProperty(Properties.LEVEL, level + 1);
            worldModel.SetProperty(Properties.MAXHP, maxHP + 10);
            worldModel.SetProperty(Properties.XP, (int)0);
            worldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, 0);
        }

        public override float GetGoalChange(Goal goal)
        {
            float change = 0.0f;

            if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change = -goal.InsistenceValue;
            }
            else if (goal.Name == AutonomousCharacter.BE_QUICK_GOAL)
            {
                change += this.Duration;
            }
            return change;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            //you would be dumb not to level up if possible
            return -1000.0f;
        }
    }
}
