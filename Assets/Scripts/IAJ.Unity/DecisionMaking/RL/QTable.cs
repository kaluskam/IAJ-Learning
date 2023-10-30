using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.RL
{
    [System.Serializable]
    public class QTable
    {         
        private Dictionary<(RLState, Action), float> values = new Dictionary<(RLState, Action), float>();
        [System.NonSerialized]
        private System.Random randomGenerator;
        public QTable()
        {
            randomGenerator = new System.Random();
        }

        public float GetQValue(RLState state, Action action)
        {
            return values.GetValueOrDefault((state, action), 0);
        }

        public void SetQValue(RLState state, Action action, float value)
        {
            values[(state, action)] = value;
        }

        public Action[] GetActions()
        {
            return values.Keys.Select(k => k.Item2).Distinct().ToArray();
        }

        public RLState[] GetStates()
        {
            return values.Keys.Select(k => k.Item1).Distinct().ToArray();
        }

        private Action[] GetActions(RLState state)
        {
            return this.values.Keys.Where(k => k.Item1 == state)
                .Select(k => k.Item2)
                .ToArray();
        }

        public Action GetBestAction(RLState state, Action[] executableActions)
        {
            Action[] possibleActions = executableActions;
            float bestQValue = 0;
            if (possibleActions.Length == 0) {
                return null;
            }
            Action bestAction = possibleActions[0];
            foreach (var a in possibleActions)
            {
                var QValue = this.GetQValue(state, a);
                if (QValue > bestQValue)
                {
                    bestQValue = QValue;
                    bestAction = a;
                }
            }
            return bestAction;
        }
        public Action GetBestAction(RLState state)
        {
            Action[] possibleActions = GetActions(state);
            float bestQValue = 0;
            Action bestAction = possibleActions[0];
            foreach (var a in possibleActions)
            {
                var QValue = this.GetQValue(state, a);
                if (QValue > bestQValue)
                {
                    bestQValue = QValue;
                    bestAction = a;
                }
            }
            return bestAction;
        }

        public float GetBestQValue(RLState state)
        {
   
            Action[] possibleActions = GetActions(state); //GET ACTIONS
            float bestQValue = 0;
            foreach (var a in possibleActions)
            {
                var QValue = this.GetQValue(state, a);
                if (QValue > bestQValue)
                {
                    bestQValue = QValue;
                }
            }
            return bestQValue;
        }
    }
}