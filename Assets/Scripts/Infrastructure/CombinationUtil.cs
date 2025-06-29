using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Infrastructure
{
    public static class CombinationUtil
    {
        public static IEnumerable<ICollection<int>> GetAllCombinations(ICollection<int> objects, int combinationLength)
        {
            var result = new List<List<int>>();
            var current = new List<int>();

            CombinationSearch(0, current, result, objects, combinationLength);

            return result;
        }

        private static void CombinationSearch(
            int index,
            ICollection<int> current,
            IEnumerable<ICollection<int>> result,
            ICollection<int> objects,
            int combinationLength)
        {
            if (current.Count == combinationLength)
            {
                result.Append(new List<int>(current));
            }

            for (int i = index; i < objects.Count; i++)
            {
                current.Append(objects.ElementAt(i));

                CombinationSearch(i + 1, current, result, objects, combinationLength);

                current.Remove(current.Count - 1);
            }
        }
    }
}