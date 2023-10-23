using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using CodeMonkey.MonoBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.RL
{
    public class QLearning
    {
        public float alpha;
        public float gamma;
        public float eps;
        public float nu;
        public int MaxIterations;
        public int CurrentIteration; // one iteration is one game, update if game ends
        public QTable qTable;
        public CurrentStateWorldModel initialState;
        public WorldModel currentState;
        public bool learningInProgress;
        private System.Random randomGenerator;
        public QLearning(CurrentStateWorldModel worldModel)
        {
            this.initialState = worldModel;
            this.alpha = 0.5f;
            this.gamma = 0.9f;
            this.MaxIterations = 1000;
            this.nu = 0.1f;
            this.eps = 0.1f;
            this.randomGenerator = new System.Random();
        }

        public void Initialize()
        {
            this.initialState.Initialize();
            this.currentState = this.initialState.GenerateChildWorldModel(); // Take current model from game manager
            this.CurrentIteration = 0;
        }

        public Action ChooseAction() // pass the worldmodel
        {
            

            RLState rlState = RLState.Create(this.currentState);
            if (!learningInProgress)
            {
                return ChooseBestAction(rlState);
            }
            else
            {
                Action[] actions = currentState.GetExecutableActions();
                Action action;
                if (randomGenerator.NextDouble() < this.eps)
                {
                    var idx = randomGenerator.Next(0, actions.Length);
                    action = actions[idx];
                }
                else
                {
                    action = ChooseBestAction(rlState);
                    // TODO - use the available actions
                }
               // PerformAction(rlState, action);
               
                return action;
            }
        }

        private Action ChooseBestAction(RLState state)
        {
            return qTable.GetBestAction(state);
        }

        public void UpdateQTable(RLState state, Action action, float reward, RLState newState)
        {
            // update QTable
            var currentQValue = qTable.GetQValue(state, action);
            var maxFutureQValue = qTable.GetBestQValue(newState);
            qTable.SetQValue(state, action, QValue(reward, currentQValue, maxFutureQValue));
        }

 
        public float QValue(float reward, float currentQValue, float maxFutureQValue)
        {
            return (1 - alpha) * currentQValue + alpha * (reward + gamma * maxFutureQValue);
        }
    }
}