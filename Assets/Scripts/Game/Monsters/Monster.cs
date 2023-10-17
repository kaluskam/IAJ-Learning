using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees;
using System.Collections.Generic;

namespace Assets.Scripts.Game.NPCs
{

    public class Monster : NPC

    {
        [Serializable]
        public struct EnemyStats
        {
            public string Type;
            public int XPvalue;
            public int AC;
            public int SimpleDamage;
            public float AwakeDistance;
            public float WeaponRange;
        }

        public EnemyStats enemyStats;

        public Func<int> DmgRoll;  //how do you like lambda's in c#?

        protected bool usingBehaviourTree;
        protected float decisionRate = 2.0f;
        protected NavMeshAgent agent;
        public GameObject Target { get; set; }
        public Task BehaviourTree;

        public bool usingFormation;

        public Vector3 DefaultPosition { get; private set; }


        void Start()
        {
            agent = this.GetComponent<NavMeshAgent>();
            this.Target = GameObject.FindGameObjectWithTag("Player");
            this.usingBehaviourTree = GameManager.Instance.BehaviourTreeNPCs;
            this.DefaultPosition = this.transform.position;

            if (!usingBehaviourTree && !GameManager.Instance.SleepingNPCs)
                Invoke("CheckPlayerPosition", 1.0f);

            if (usingBehaviourTree)
                InitializeBehaviourTree();

        }

        public bool InWeaponRange(GameObject target)
        { return Vector3.Distance(this.transform.position, target.transform.position) <= enemyStats.WeaponRange; }

        public virtual void InitializeBehaviourTree()
        {
            // TODO but in the children's class
        }

        void FixedUpdate()
        {
            if (GameManager.Instance.gameEnded) return;
            if (usingBehaviourTree)
            {
                if(this.BehaviourTree != null) 
                    this.BehaviourTree.Run();
                else 
                    this.BehaviourTree = new BasicTree(this,Target);
            }
                    
        }

        // Very basic Enemy AI
        void CheckPlayerPosition()
        {
            if (Vector3.Distance(this.transform.position, this.Target.transform.position) < enemyStats.AwakeDistance)
            {

                if (Vector3.Distance(this.transform.position, this.Target.transform.position) <= enemyStats.WeaponRange)
                {
                    AttackPlayer();
                }

                else
                {
                    // Move towards Main Character
                    this.StartPathfinding(this.Target.transform.position);
                    Invoke("CheckPlayerPosition", 0.5f);
                }
            }
            else
            {

                Invoke("CheckPlayerPosition", 3.0f);
            }
        }

        // Monsters can attack the Main Character
        public void AttackPlayer()
        {
            GameManager.Instance.EnemyAttack(this.gameObject);
        }      

     }


}
