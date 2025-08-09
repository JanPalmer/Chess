using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;


namespace Infrastructure
{
    public static class DiceRoller
    {
        private static System.Random _numberGenerator = new();

        private static readonly int[] AttackDiceSides = new int[] { 0, 0, 0, 1, 1, 2 };

        public static int[] RollCombatDice(int times)
        {
            var result = new int[times];

            for (int i = 0; i < times; i++)
            {
                //var diceIndex = (int)Math.Round(_numberGenerator.NextDouble() * (AttackDiceSides.Length - 1));
                //UnityEngine.Debug.Log($"RollCombatDice - diceIndex: {diceIndex}");
                var diceIndex = 3;
                result[i] = AttackDiceSides[diceIndex];
            }

            // Sort descending
            result = result.OrderBy(x => -x).ToArray();

            return result;
        }
    }
}
