using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class Teleport : Action
    {
        private AutonomousCharacter Character;
        private Vector3 initialPosition = new Vector3 (50, 0, 53.67f);
        public Teleport(AutonomousCharacter character) : base("Teleport")
        {
            this.Character = character;
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return Character.baseStats.Mana >= 5 && Character.baseStats.Level >= 2;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;

            return (int)worldModel.GetProperty(Properties.MANA) >= 5 && (int)worldModel.GetProperty(Properties.LEVEL) >= 2;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.Teleport();
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            return change;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);
            var mana = (int)worldModel.GetProperty(Properties.MANA);
            worldModel.SetProperty(Properties.MANA, mana - 5);

            worldModel.SetProperty(Properties.POSITION, initialPosition);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            //TODO
            return base.GetHValue(worldModel);
        }
    }
}
