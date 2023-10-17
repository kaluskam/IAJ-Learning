using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System.Collections.Generic;

namespace Assets.Scripts.Game
{
    //class that represents a world model that corresponds to the current state of the world,
    //all required properties and goals are stored inside the game manager
    public class CurrentStateWorldModel : FutureStateWorldModel
    {
        private Dictionary<string, Goal> Goals { get; set; } 

        public CurrentStateWorldModel(GameManager gameManager, List<Action> actions, List<Goal> goals) : base(gameManager, actions)
        {
            this.Parent = null;
            this.Goals = new Dictionary<string, Goal>();

            foreach (var goal in goals)
            {
                this.Goals.Add(goal.Name,goal);
            }
        }

        public void Initialize()
        {
            this.ActionEnumerator.Reset();
        }

        public override object GetProperty(string propertyName)
        {

            //TIP: this code can be optimized by using a dictionary with lambda functions instead of if's  
            if (propertyName.Equals(Properties.MANA)) return this.GameManager.Character.baseStats.Mana;

            if (propertyName.Equals(Properties.XP)) return this.GameManager.Character.baseStats.XP;

            if (propertyName.Equals(Properties.MAXHP)) return this.GameManager.Character.baseStats.MaxHP;

            if (propertyName.Equals(Properties.HP)) return this.GameManager.Character.baseStats.HP;

            if (propertyName.Equals(Properties.ShieldHP)) return this.GameManager.Character.baseStats.ShieldHP;

            if (propertyName.Equals(Properties.MONEY)) return this.GameManager.Character.baseStats.Money;

            if (propertyName.Equals(Properties.TIME)) return this.GameManager.Character.baseStats.Time;

            if (propertyName.Equals(Properties.LEVEL)) return this.GameManager.Character.baseStats.Level;

            if (propertyName.Equals(Properties.POSITION))
                return this.GameManager.Character.gameObject.transform.position;

            //if an object name is found in the dictionary of disposable objects, then the object still exists. The object has been removed/destroyed otherwise
            return this.GameManager.disposableObjects.ContainsKey(propertyName);
        }

        public override float GetGoalValue(string goalName)
        {
            return this.Goals[goalName].InsistenceValue;
        }

        public override void SetGoalValue(string goalName, float goalValue)
        {
            //this method does nothing, because you should not directly set a goal value of the CurrentStateWorldModel
        }

        public override void SetProperty(string propertyName, object value)
        {
            //this method does nothing, because you should not directly set a property of the CurrentStateWorldModel
        }

        public override int GetNextPlayer()
        {
            //in the current state, the next player is always player 0
            return 0;
        }
    }
}
