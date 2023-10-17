using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class DivineSmite : WalkToTargetAndExecuteAction
    {
        private float expectedXPChange;
        private float XPChange = 0;

        public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite", character, target)
        {
            if(target.tag.Equals("Skeleton"))
            {
                this.expectedXPChange = 2.7f;
                this.XPChange = 3;
            }
            else if (target.tag.Equals("Orc"))
            {
                this.expectedXPChange = 0;
            }
            else if (target.tag.Equals("Dragon"))
            {
                this.expectedXPChange = 0;
            }
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return Character.baseStats.Mana >= 2;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;
            if (this.Target == null) return false;
            return (int)worldModel.GetProperty(Properties.MANA) >= 2;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.DivineSmite(this.Target);
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change -= this.expectedXPChange;
            }

            return change;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);
            var mana = (int)worldModel.GetProperty(Properties.MANA);
            var xp = Convert.ToInt32(worldModel.GetProperty(Properties.XP));

            worldModel.SetProperty(Properties.MANA, mana - 2);
            worldModel.SetProperty(Properties.XP, xp + this.XPChange);

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, goalValue - this.XPChange);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            //TODO
            return base.GetHValue(worldModel);
        }
    }
}
