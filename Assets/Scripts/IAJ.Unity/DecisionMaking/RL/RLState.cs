using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.RL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Xml.XPath;
using UnityEngine;
using static CodeMonkey.Utils.UI_TextComplex;
using UnityEngine.UIElements;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.RL
{
    [Serializable]
    public class RLState
    {
        enum HP
        {
            VERY_LOW, LOW, OK, HIGH
        }
        enum MANA
        {
            LOW, MEDIUM, HIGH
        }

        enum XP
        {
            LOW, HIGH
        }

        enum TIME { 
            LESS_38, LESS_75, LESS_112, LESS_149, MORE_150
        }
        enum POSITION { 
            LEFT_UP, LEFT_DOWN, RIGHT_UP, RIGHT_DOWN
        }


        private HP hp;
        private MANA mana;
        private XP xp;
        private short level;
        private TIME time;
        private POSITION position;
        private short money;
        private short[] stateValues = new short[6];
        /* 
         * 0 - HP
         * 1 - Mana
         * 2 - Level
         * 3 - XP
         * 4 - Time
         * 5 - Position
         * 
         * 6 - Orc1Alive
         * 7 - Orc2Alive
         * 8 - Orc3Alive
         * 9 - Skeleton1Alive
         * 10 - Skeleton2Alive
         * 11 - Skeleton3Alive
         * 12 - Skeleton4Alive
         * 13 - Skeleton5Alive
         * 14 - DragonAlive
         * 15 - Chest1Open
         * 16 - Chest2Open
         * 17 - Chest3Open
         * 18 - Chest4Open
         * 19 - Chest5Open
         * 20 - ManaPotion1Open
         * 21 - ManaPotion2Open
         * 22 - HealthPotion1Open
         * 23 - HealthPotion2Open
         */
        public RLState()
        {

        }

        public static RLState Create(WorldModel worldModel)
        {
            var state = new RLState();
            state.CreateFrom(worldModel);
            return state;
        }

        public void CreateFrom(WorldModel worldModel)
        {
            hp = ConvertHP(Convert.ToInt32(worldModel.GetProperty(Properties.HP)));
            mana = ConvertMana(Convert.ToInt32(worldModel.GetProperty(Properties.MANA)));
            level = Convert.ToInt16(worldModel.GetProperty(Properties.LEVEL));
            xp = ConvertXP(Convert.ToInt32(worldModel.GetProperty(Properties.XP)));
            time = ConvertTime((float)worldModel.GetProperty(Properties.TIME));
            position = ConvertPosition((Vector3)worldModel.GetProperty(Properties.POSITION));
            money = Convert.ToInt16(worldModel.GetProperty(Properties.MONEY));
        }
        private HP ConvertHP(int hp)
        {
            if (hp <= 3) return HP.VERY_LOW;
            if (hp <= 6) return HP.LOW;
            if (hp <= 12) return HP.OK;
            return HP.HIGH;
        }

        private MANA ConvertMana(int mana)
        {
            if (mana <= 1) return MANA.LOW;
            if (mana <= 4) return MANA.MEDIUM;
            return MANA.HIGH;
        }

        private XP ConvertXP(int xp)
        {
            if (xp <= 9) return XP.LOW;
            return XP.HIGH;
        }

        private TIME ConvertTime(float time)
        {
            if (time <= 38) return TIME.LESS_38;
            if (time <= 75) return TIME.LESS_75;
            if (time <= 112) return TIME.LESS_112;
            if (time <= 149) return TIME.LESS_149;
            return TIME.MORE_150;
        }

        private POSITION ConvertPosition(Vector3 position)
        {
            if (position.z < 49)
            {
                if (position.x < 54) return POSITION.LEFT_DOWN; // DOWN LEFT
                return POSITION.RIGHT_DOWN; // DOWN RIGHT

            }
            else
            {
                if (position.x < 64) return POSITION.LEFT_UP; // UP LEFT
                return POSITION.RIGHT_UP; // UP RIGHT
            }
        }

        public override string ToString()
        {
            return "HP: " + hp + " Mana: " + mana + " Level: " + level + " XP: " + xp +
                " Money: " + money + " Position: " + position + " Time: " + time;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            else 
            {
                RLState other = obj as RLState;
                return other.hp == hp && other.mana == mana &&
                    other.xp == xp && other.level == level &&
                    other.money == money && other.position == position &&
                    other.time == time;
            }
        }

        public override int GetHashCode()
        {

            string hashString = "1";
            hashString += (int)hp;
            hashString += (int)mana;
            hashString += (int)level;
            hashString += (int)money;
            hashString += (int)position;
            hashString += (int)time;
            hashString += (int)xp;
            return System.Convert.ToInt32(hashString); 
        }


        //public void CreateFrom(WorldModel worldModel)
        //{
        //    stateValues[0] = ConvertHP(Convert.ToInt32(worldModel.GetProperty(Properties.HP)));
        //    stateValues[1] = ConvertMana(Convert.ToInt32(worldModel.GetProperty(Properties.MANA)));
        //    stateValues[2] = Convert.ToInt16(worldModel.GetProperty(Properties.LEVEL));
        //    stateValues[3] = ConvertXP(Convert.ToInt32(worldModel.GetProperty(Properties.XP)));
        //    stateValues[4] = ConvertTime((float)worldModel.GetProperty(Properties.TIME));
        //    stateValues[5] = ConvertPosition((Vector3)worldModel.GetProperty(Properties.POSITION));


        //}
        //private short ConvertHP(int hp)
        //{
        //    if (hp <= 3) return 0;
        //    if (hp <= 6) return 1;
        //    if (hp <= 12) return 2;
        //    return 3;
        //}

        //private short ConvertMana(int mana)
        //{
        //    if (mana <= 1) return 0;
        //    if (mana <= 4) return 1;
        //    return 2;
        //}

        //private short ConvertXP(int xp)
        //{
        //    if (xp <= 9) return 0;
        //    return 1;
        //}

        //private short ConvertTime(float time)
        //{
        //    if (time <= 38) return 0;
        //    if (time <= 75) return 1;
        //    if (time <= 112) return 2;
        //    if (time <= 149) return 3;
        //    return 4;
        //}

        //private short ConvertPosition(Vector3 position)
        //{
        //    if (position.z < 49)
        //    {
        //        if (position.x < 54) return 0; // DOWN LEFT
        //        return 1; // DOWN RIGHT

        //    }
        //    else
        //    {
        //        if (position.x < 64) return 2; // UP LEFT
        //        return 3; // UP RIGHT
        //    }
        //}
    }
}



public class RLStateComparer : IEqualityComparer<RLState>
{

    public bool Equals(RLState x, RLState y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.Equals(y);
    }


    public int GetHashCode(RLState obj) => obj.GetHashCode();
}