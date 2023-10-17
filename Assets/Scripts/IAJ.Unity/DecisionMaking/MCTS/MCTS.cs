using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;
using System.Net.WebSockets;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; protected set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<Action> BestActionSequence { get; private set; }
        public WorldModel BestActionSequenceEndState { get; private set; }
        public int NumberOfPlayouts { get; set; }
        public int DepthLimit { get; set; }
        public bool LimitedDepth { get; set; }
        protected int CurrentIterations { get; set; }
        protected int CurrentDepth { get; set; }
        protected int FrameCurrentIterations { get; set; }
        protected CurrentStateWorldModel InitialState { get; set; }
        protected MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }
        

        

        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.InitialState = currentStateWorldModel;
            this.MaxIterations = 5000;
            this.MaxIterationsPerFrame = 5000;
            this.RandomGenerator = new System.Random();
        }


        public void InitializeMCTSearch()
        {
            this.InitialState.Initialize();
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.FrameCurrentIterations = 0;
            this.TotalProcessingTime = 0.0f;
 
            // create root node n0 for state s0
            this.InitialNode = new MCTSNode(this.InitialState)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<Action>();
        }

        public Action ChooseAction()
        {

            MCTSNode selectedNode;
            float reward;

            var startTime = Time.realtimeSinceStartup;

            while (CurrentIterations < MaxIterations)
            {
                if (FrameCurrentIterations >= MaxIterationsPerFrame) 
                {
                    FrameCurrentIterations = 0;
                    return null;
                }
                selectedNode = Selection(this.InitialNode);
                reward = Playout(selectedNode.State);
                Backpropagate(selectedNode, reward);
                FrameCurrentIterations++;
                CurrentIterations++;
            }
            FrameCurrentIterations = 0;
            InProgress = false;

            // return best initial child
            this.TotalProcessingTime = Time.realtimeSinceStartup - startTime;
            return BestAction(this.InitialNode);
        }

        // Selection and Expantion
        protected MCTSNode Selection(MCTSNode initialNode)
        {
            Action nextAction;
            MCTSNode currentNode = initialNode;
            //Whats for
            MCTSNode bestChild;
            var currentDepth = 0;
            
            while (!currentNode.State.IsTerminal() && currentDepth <= 3)
            {
                nextAction = currentNode.State.GetNextAction();
                if (nextAction != null)
                {
                    return Expand(currentNode, nextAction);
                }
                else
                {   
                    currentNode = BestUCTChild(currentNode);
                    currentDepth++;
                }
            }
            this.MaxSelectionDepthReached = Mathf.Max(this.MaxSelectionDepthReached, currentDepth);
            return currentNode;
        }

        protected virtual float Playout(WorldModel initialStateForPlayout)
        {
            var scores = new List<float>();

            for (int i = 0; i < NumberOfPlayouts; i++)
            {
                var currentState = initialStateForPlayout.GenerateChildWorldModel();
                var currentDepth = 0;

                while (!currentState.IsTerminal())
                {
                    Action[] executableActions = currentState.GetExecutableActions();

                    var idx = RandomGenerator.Next(0, executableActions.Length);
                    var action = executableActions[idx];
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

        protected virtual void Backpropagate(MCTSNode node, float reward)
        {
            //ToDo, do not forget to later consider two advesary moves...
            var currentNode = node;
            while (currentNode != null)
            {
                currentNode.N++;
                currentNode.Q += reward;
                currentNode = currentNode.Parent;
            }
        }

        protected MCTSNode Expand(MCTSNode parent, Action action)
        {
            WorldModel newState = parent.State.GenerateChildWorldModel();
            action.ApplyActionEffects(newState);
            var Child = new MCTSNode(newState);
            Child.Action = action;
            parent.ChildNodes.Add(Child);
            Child.Parent = parent;
            
            return Child;
        }

        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            MCTSNode bestChild = node.ChildNodes[0];
            float bestScore = bestChild.UCTScore();
            foreach (var child in node.ChildNodes)
            {
                var score = child.UCTScore();
                if (score > bestScore)
                {
                    bestChild = child;
                    bestScore = score;
                } 
            }
            return bestChild;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        protected MCTSNode BestChild(MCTSNode node)
        {
            if (node.ChildNodes.Count == 0)
            {
                return null;
            }
            MCTSNode bestChild = node.ChildNodes[0];
            float bestScore = node.Q / node.N;
            foreach (var child in node.ChildNodes)
            {
                //if (child.Action.Name == "LevelUp")
                //{
                //    return child;
                //}
                var score = child.Q / child.N;
                if (score > bestScore)
                {
                    bestChild = child;
                    bestScore = score;
                }
            }

            return bestChild;
        }


        protected Action BestAction(MCTSNode node)
        {
            var bestChild = this.BestChild(node);
            if (bestChild == null) return null;

            this.BestFirstChild = bestChild;

            //this is done for debugging proposes only
            this.BestActionSequence = new List<Action>();
            this.BestActionSequence.Add(bestChild.Action);
            node = bestChild;

            while(!node.State.IsTerminal())
            {
                bestChild = this.BestChild(node);
                if (bestChild == null) break;
                this.BestActionSequence.Add(bestChild.Action);
                node = bestChild;
                this.BestActionSequenceEndState = node.State;
            }

            return this.BestFirstChild.Action;
        }

    }
}
