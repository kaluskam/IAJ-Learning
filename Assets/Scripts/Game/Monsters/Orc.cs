using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees;
//using Assets.Scripts.IAJ.Unity.Formations;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Assets.Scripts.Game.NPCs
{

    public class Orc : Monster
    {
        //New variables
        public Vector3 PatrolPoint1 { get; set; }
        public Vector3 PatrolPoint2 { get; set; }

        public AudioSource shout {  get; set; }
        public bool inShoutChase { get; set; }

        public bool inChase { get; set; }
        public Orc()
        {
            this.enemyStats.Type = "Orc";
            this.enemyStats.XPvalue = 8;
            this.enemyStats.AC = 14;
            this.baseStats.HP = 15;
            this.DmgRoll = () => RandomHelper.RollD10() + 2;
            this.enemyStats.SimpleDamage = 6;
            this.enemyStats.AwakeDistance = 10;
            this.enemyStats.WeaponRange = 3;
            this.inShoutChase = false;
            this.inChase = false;  
        }

        public override void InitializeBehaviourTree()
        {
            var patrols = GameObject.FindGameObjectsWithTag("Patrol");

            float pos = float.MaxValue;
            GameObject closest = null;
            foreach (var p in patrols)
            {   

                if (Vector3.Distance(this.agent.transform.position, p.transform.position) < pos)
                {
                    pos = Vector3.Distance(this.agent.transform.position, p.transform.position);
                    closest = p;
                }

            }

            this.shout = gameObject.AddComponent<AudioSource>();
            
            var position1 = closest.transform.GetChild(0).position;
            var position2 = closest.transform.GetChild(1).position;
            this.PatrolPoint1 = position1;
            this.PatrolPoint2 = position2;
            this.currentTarget = position1;
            //TODO Create a Behavior tree that combines Patrol with other behaviors...
            //var mainTree = new Patrol(this, position1, position2);


            this.BehaviourTree = new PatrolBehaviourTree(this, Target);
        }


    }
}
