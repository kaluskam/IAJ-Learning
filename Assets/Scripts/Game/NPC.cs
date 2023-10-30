﻿using System;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Game
{
    public class NPC : MonoBehaviour
    {
        [Serializable]
        public struct Stats
        {
            public string Name;
            public int HP;
            public int ShieldHP;
            public int MaxHP;
            public int Mana;
            public int XP;
            public float Time;
            public int Money;
            public int Level;
            public override string ToString()
            {
                return HP + "," + Mana + ", " + XP + "," + Time + "," + Money + "," + Level;
            }

            public Stats Copy()
            {
                var copy = new Stats();
                copy.HP = HP;
                copy.ShieldHP = ShieldHP;
                copy.MaxHP = MaxHP;
                copy.Mana = Mana;
                copy.XP = XP;
                copy.Time = Time;
                copy.Money = Money;
                copy.Level = Level;
                copy.Name = Name;
                return copy;
            }
        }

        protected GameObject character;
        protected Vector3 initialPosition;

        // Pathfinding
        protected UnityEngine.AI.NavMeshAgent navMeshAgent;
        private Vector3 previousTarget;
        public Vector3 currentTarget { get; set; }

        public Stats baseStats;


        void Awake()
        {
            previousTarget = new Vector3(0.0f, 0.0f, 0.0f);
            character = this.gameObject;
            navMeshAgent = this.GetComponent<NavMeshAgent>();
        }

        public virtual void ReAwake()
        {
            previousTarget = new Vector3(0.0f, 0.0f, 0.0f);
            character = this.gameObject;
            navMeshAgent = this.GetComponent<NavMeshAgent>();
        }



        #region Navmesh Pathfinding Methods

        public void StartPathfinding(Vector3 targetPosition)
        {
            //if the targetPosition received is the same as a previous target, then this a request for the same target
            //no need to redo the pathfinding search
            if (!this.previousTarget.Equals(targetPosition))
            {

                this.previousTarget = targetPosition;

                navMeshAgent.SetDestination(targetPosition);

            }
        }

        public void StopPathfinding()
        {
            navMeshAgent.isStopped = true;
        }

        //Simple way of calculating distance left to target using Unity's navmesh
        public float GetDistanceToTarget(Vector3 originalPosition, Vector3 targetPosition)
        {
            var distance = 0.0f;

            NavMeshPath result = new NavMeshPath();
            var r = navMeshAgent.CalculatePath(targetPosition, result);
            if (r == true)
            {
                var currentPosition = originalPosition;
                foreach (var c in result.corners)
                {
                    //Rough estimate, it does not account for shortcuts so we have to multiply it
                    distance += Vector3.Distance(currentPosition, c) * 0.65f;
                    currentPosition = c;
                }
                return distance;
            }

            //Default value
            return 100;
        }
        #endregion

    }
}
