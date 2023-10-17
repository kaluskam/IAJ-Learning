using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AI;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.EnemyTasks
{   
    //Changed to work with vector 3, instead of gameObject and added 1 constructor
    class Pursue : Task
    {
        protected Monster Character { get; set; }

        public GameObject Target { get; set; }

        public float range;

        public Pursue(Monster character, GameObject target, float _range)
        {
            this.Character = character;
            this.Target = target;
            range = _range;
        }
    
        public override Result Run()
        {
            if (Target == null)
                return Result.Failure;
            var distance = Vector3.Distance(Character.transform.position, this.Target.transform.position);

            if (distance <= range)
            {
                return Result.Success;
            }
            else if (distance > 3f * Character.enemyStats.AwakeDistance)
            {
                return Result.Failure;
            }
            else
            {
                Character.StartPathfinding(this.Target.transform.position);
                return Result.Running;
            }

        }

    }
}
