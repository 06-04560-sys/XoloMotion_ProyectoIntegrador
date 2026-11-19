using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tsakel.Game
{
    /// <summary>Estado entre rondas (se conserva al recargar la escena).</summary>
    public static class RoundState
    {
        public static int RoundNumber;
        private static readonly List<string> last = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            RoundNumber = 0;
            last.Clear();
        }

        /// <summary>Elige especies para la ronda: primero las incompletas y las que no salieron en la ronda anterior.</summary>
        public static List<string> Pick(List<string> availableIds, int count)
        {
            RoundNumber++;
            List<string> pool = new List<string>(availableIds);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                string t = pool[i];
                pool[i] = pool[j];
                pool[j] = t;
            }
            List<string> picked = pool.OrderBy(id => Score(id)).Take(count).ToList();
            last.Clear();
            last.AddRange(picked);
            return picked;
        }

        private static int Score(string id)
        {
            int s = 0;
            if (AlbumData.Get(id).Complete) s += 2;
            if (last.Contains(id)) s += 1;
            return s;
        }
    }
}
