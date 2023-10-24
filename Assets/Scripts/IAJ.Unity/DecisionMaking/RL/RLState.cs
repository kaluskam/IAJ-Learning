using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.RL
{
    public class RLState
    {
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
            stateValues[0] = ConvertHP((int)worldModel.GetProperty(Properties.HP));
            stateValues[1] = ConvertMana((int)worldModel.GetProperty(Properties.MANA));
            stateValues[2] = Convert.ToInt16( worldModel.GetProperty(Properties.LEVEL));
            stateValues[3] = ConvertXP(Convert.ToInt32(worldModel.GetProperty(Properties.XP)));
            stateValues[4] = ConvertTime((float)worldModel.GetProperty(Properties.TIME));
            stateValues[5] = ConvertPosition((Vector3)worldModel.GetProperty(Properties.POSITION));
        }
        private short ConvertHP(int hp)
        {
            if (hp <= 3) return 0;
            if (hp <= 6) return 1;
            if (hp <= 12) return 2;
            return 3;
        }

        private short ConvertMana(int mana)
        {
            if (mana <= 1) return 0;
            if (mana <= 4) return 1;
            return 2;
        }

        private short ConvertXP(int xp)
        {
            if (xp <= 9) return 0;
            return 1;
        }

        private short ConvertTime(float time)
        {
            if (time <= 38) return 0;
            if (time <= 75) return 1;
            if (time <= 112) return 2;
            if (time <= 149) return 3;
            return 4;
        }

        private short ConvertPosition(Vector3 position)
        {
            if (position.z < 49)
            {
                if (position.x < 54) return 0; // DOWN LEFT
                return 1; // DOWN RIGHT

            }
            else
            {
                if (position.x < 64) return 2; // UP LEFT
                return 3; // UP RIGHT
            }
        }






    }
}