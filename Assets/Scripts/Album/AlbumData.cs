using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tsakel.Game
{
    public class AlbumEntry
    {
        public string SpeciesId;
        public readonly List<Texture2D> Photos = new List<Texture2D>();
        public readonly List<string> Dates = new List<string>();

        public bool Discovered { get { return Photos.Count > 0; } }
        public int Progress { get { return Mathf.Min(Photos.Count, AlbumData.RequiredPhotos); } }
        public bool Complete { get { return Photos.Count >= AlbumData.RequiredPhotos; } }
    }

    /// <summary>Album de fotos en memoria (sin guardado en disco, como indica el GDD).</summary>
    public static class AlbumData
    {
        public const int RequiredPhotos = 3;

        private static readonly Dictionary<string, AlbumEntry> entries = new Dictionary<string, AlbumEntry>();

        public static event Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            entries.Clear();
            Changed = null;
        }

        public static AlbumEntry Get(string id)
        {
            AlbumEntry e;
            if (!entries.TryGetValue(id, out e))
            {
                e = new AlbumEntry();
                e.SpeciesId = id;
                entries[id] = e;
            }
            return e;
        }

        public static void AddPhoto(string id, Texture2D tex)
        {
            AlbumEntry e = Get(id);
            if (e.Photos.Count >= RequiredPhotos)
            {
                // fotos libres: reemplaza la mas antigua
                UnityEngine.Object.Destroy(e.Photos[0]);
                e.Photos.RemoveAt(0);
                e.Dates.RemoveAt(0);
            }
            e.Photos.Add(tex);
            e.Dates.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            if (Changed != null) Changed();
        }

        public static int CompleteCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < SpeciesCatalog.All.Length; i++)
                    if (Get(SpeciesCatalog.All[i].Id).Complete) n++;
                return n;
            }
        }
    }
}
