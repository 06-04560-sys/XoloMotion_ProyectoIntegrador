using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tsakel.Game
{
    /// <summary>
    /// Maneja la ronda: elige 3 animales, cuenta las fotos validas y avisa cuando los 3 estan completos
    /// (3 fotos validas de cada uno). La primera foto de cada animal en la ronda muestra su ficha.
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        [SerializeField] private int animalsPerRound = 3;
        [SerializeField] private PhotoCameraController photoCamera;

        private readonly List<AnimalSubject> roundAnimals = new List<AnimalSubject>();
        private readonly Dictionary<string, int> roundPhotos = new Dictionary<string, int>();

        public IList<AnimalSubject> RoundAnimals { get { return roundAnimals; } }
        public bool RoundOver { get; private set; }

        /// <summary>animal, foto, numero de fotos validas de ese animal en esta ronda.</summary>
        public event Action<AnimalSubject, Texture2D, int> PhotoRegistered;
        public event Action RoundCompleted;

        private void Awake()
        {
            AnimalSubject[] all = FindObjectsByType<AnimalSubject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Dictionary<string, AnimalSubject> byId = new Dictionary<string, AnimalSubject>();
            List<string> ids = new List<string>();
            for (int i = 0; i < all.Length; i++)
            {
                AnimalSubject a = all[i];
                if (string.IsNullOrEmpty(a.SpeciesId) || SpeciesCatalog.Get(a.SpeciesId) == null || byId.ContainsKey(a.SpeciesId)) continue;
                byId.Add(a.SpeciesId, a);
                ids.Add(a.SpeciesId);
            }

            List<string> picked = RoundState.Pick(ids, animalsPerRound);
            for (int i = 0; i < all.Length; i++) all[i].gameObject.SetActive(false);

            float phase = 0f;
            for (int i = 0; i < picked.Count; i++)
            {
                AnimalSubject a = byId[picked[i]];
                a.gameObject.SetActive(true);
                a.BeginRound(phase);
                phase += 3f;
                roundAnimals.Add(a);
                roundPhotos[picked[i]] = 0;
            }
            if (photoCamera != null) photoCamera.SetAnimals(roundAnimals);
        }

        private void OnEnable()
        {
            if (photoCamera != null) photoCamera.PhotoTaken += RegisterPhoto;
        }

        private void OnDisable()
        {
            if (photoCamera != null) photoCamera.PhotoTaken -= RegisterPhoto;
        }

        public int PhotosThisRound(string speciesId)
        {
            int n;
            return roundPhotos.TryGetValue(speciesId, out n) ? n : 0;
        }

        public void RegisterPhoto(AnimalSubject animal, Texture2D tex)
        {
            if (RoundOver || animal == null) return;
            string id = animal.SpeciesId;
            int n = PhotosThisRound(id) + 1;
            roundPhotos[id] = n;
            AlbumData.AddPhoto(id, tex);
            if (PhotoRegistered != null) PhotoRegistered(animal, tex, n);

            if (AllComplete())
            {
                RoundOver = true;
                if (RoundCompleted != null) RoundCompleted();
            }
        }

        private bool AllComplete()
        {
            if (roundAnimals.Count == 0) return false;
            for (int i = 0; i < roundAnimals.Count; i++)
                if (PhotosThisRound(roundAnimals[i].SpeciesId) < AlbumData.RequiredPhotos) return false;
            return true;
        }
    }
}
