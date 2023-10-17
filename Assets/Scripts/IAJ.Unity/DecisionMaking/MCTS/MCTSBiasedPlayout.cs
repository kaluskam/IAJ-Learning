using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity
{
    public class MCTSBiasedPlayout : MCTS
    {
        public int MaxPlayoutDepthConstraint { get; set; }

        public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
            this.MaxPlayoutDepthConstraint = 13;
        }

        protected override float Playout(WorldModel initialStateForPlayout)
        {
            var scores = new List<float>();

            for (int i = 0; i < NumberOfPlayouts; i++)
            {
                var currentState = initialStateForPlayout.GenerateChildWorldModel();
                var currentDepth = 0;

                while (!currentState.IsTerminal())
                {
                    if (this.LimitedDepth && currentDepth >= MaxPlayoutDepthConstraint)
                    {
                        break;
                    }

                    Action[] executableActions = currentState.GetExecutableActions();

                    var action = ChooseBestHAction(executableActions, currentState);
                    //action.ApplyActionEffects(currentState);
                    WorldModel newState = currentState.GenerateChildWorldModel();
                    action.ApplyActionEffects(newState);
                    newState.CalculateNextPlayer();
                    currentState = newState;

                    currentDepth++;
                }
                this.MaxPlayoutDepthReached = Mathf.Max(this.MaxPlayoutDepthReached, currentDepth);
                scores.Add(currentState.GetScore());
            }

            return scores.Average();
        }

        private Action ChooseBestHAction(Action[] actions, WorldModel world)
        {
            var bestAction = actions[0];
            var bestH = bestAction.GetHValue(world);
            foreach(var action in actions)
            {
                if (action.GetHValue(world) < bestH)
                {
                    bestAction = action;
                    bestH = action.GetHValue(world);
                }
            }

            return bestAction;
        }
    }
}