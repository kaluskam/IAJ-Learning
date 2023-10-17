using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class GOBDecisionMaking
    {
        public bool InProgress { get; set; }
        private List<Goal> goals { get; set; }
        private List<Action> actions { get; set; }

        public Dictionary<Action,float> ActionDiscontentment { get; set; }

        public Action secondBestAction;
        public Action thirdBestAction;

        // Utility based GOB
        public GOBDecisionMaking(List<Action> _actions, List<Goal> goals)
        {
            this.actions = _actions;
            this.goals = goals;
            secondBestAction = new Action("yo");
            thirdBestAction = new Action("yo too");
            this.ActionDiscontentment = new Dictionary<Action,float>();
        }


        public static float CalculateDiscontentment(Action action, List<Goal> goals)
        {
            // Keep a running total
            var discontentment = 0.0f;
            var duration = action.GetDuration();

            foreach (var goal in goals)
            {
               // Calculate the new value after the action
                var newValue = goal.InsistenceValue + action.GetGoalChange(goal);

                // The change rate is how much the goals changes per time
                newValue += duration * goal.ChangeRate;

                //Here is a bug: Insistence varies between 0-10, it should be normalized
                discontentment += goal.GetDiscontentment(newValue);
            }

            return discontentment;
        }

        public Action ChooseAction()
        {
            // Find the action leading to the lowest discontentment
            InProgress = true;
            Action bestAction = null;
            var bestValue = float.PositiveInfinity;
            var secondBestValue = float.PositiveInfinity;
            var thirdBestValue = float.PositiveInfinity;
            ActionDiscontentment.Clear();

            //TODO implement                
            foreach (var action in actions) 
            {
                if (action.CanExecute()) {
                    var value = CalculateDiscontentment(action, goals);
                    ActionDiscontentment[action] = value;
                    if (value < bestValue) 
                    {
                        thirdBestValue = secondBestValue;
                        thirdBestAction = secondBestAction;
                    
                        secondBestAction = bestAction;
                        secondBestValue = bestValue;

                        bestValue = value;
                        bestAction = action;
                    } else if (value < secondBestValue)
                    {
                        thirdBestValue = secondBestValue;
                        thirdBestAction = secondBestAction;

                        secondBestAction = action;
                        secondBestValue = value;
                    } else if (value < thirdBestValue)
                    {
                        thirdBestValue = value;
                        thirdBestAction = action;
                    }
                }
            }

            
            
            InProgress = false;
            return bestAction;
        }
    }
}
