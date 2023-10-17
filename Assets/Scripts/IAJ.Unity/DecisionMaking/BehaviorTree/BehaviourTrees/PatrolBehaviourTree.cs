using Assets.Scripts.Game.NPCs;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.EnemyTasks;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees
{
    public class PatrolBehaviourTree : Selector
    {
        public PatrolBehaviourTree(Orc character, GameObject target)
        {
            // To create a new tree you need to create each branch which is done using the constructors of different tasks
            // Additionally it is possible to create more complex behaviour by combining different tasks and composite tasks...
            this.children = new List<Task>()
            {
                new Sequence(new List<Task>() {
                    new PatrolAndPursue(character, target,character.PatrolPoint1, character.PatrolPoint2 ,0.01f),
                    //new Pursue(character, target.transform.position , character.enemyStats.WeaponRange),
                    new LightAttack(character)}),                
               
        
            };
        }
    }
}