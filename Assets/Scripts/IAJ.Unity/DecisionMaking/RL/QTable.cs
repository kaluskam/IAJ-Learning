using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.RL
{
    public class QTable
    {
        private RLState[] states; // TODO filling this array
        private Dictionary<(RLState, Action), float> values;
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
            values.Add((state, action), value);
        }

        public RLState GetRandomState()
        {
            var idx = randomGenerator.Next(0, states.Length);
            return states[idx];

        }


        public Action GetBestAction(RLState state)
        {
            RLState futureState = null;
            Action[] possibleActions = null;
            float bestQValue = 0;
            Action bestAction = possibleActions[0];
            foreach (var a in possibleActions)
            {
                var QValue = this.GetQValue(futureState, a);
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
   
            Action[] possibleActions = null; //GET ACTIONS
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