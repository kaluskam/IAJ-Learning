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

    public class Dragon : Monster
    {
        public Dragon()
        {
            this.enemyStats.Type = "Dragon";
            this.enemyStats.XPvalue = 20;
            this.enemyStats.AC = 16;
            this.baseStats.HP = 30;
            this.DmgRoll = () => RandomHelper.RollD12() + RandomHelper.RollD12()+3;
            this.enemyStats.SimpleDamage = 15;
            this.enemyStats.AwakeDistance = 15;
            this.enemyStats.WeaponRange = 7;
        }

        public override void InitializeBehaviourTree()
        {
            this.BehaviourTree = new BasicTree(this, Target);
        }



    }
}
