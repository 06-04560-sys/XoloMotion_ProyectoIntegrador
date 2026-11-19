using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tsakel.Game
{
    /// <summary>Pantallas sobre el juego: ficha, album (Tab), pausa (Esc) y resumen de la ronda.</summary>
    public class GameFlow : MonoBehaviour
    {
        public enum Overlay { None, Ficha, Album, Pause, Results }

        private struct FichaRequest
        {
            public AnimalSubject animal;
            public Texture2D photo;
        }

        [SerializeField] private Tsakel.Player.PlayerController player;
        [SerializeField] private PhotoCameraController photoCamera;
        [SerializeField] private RoundManager round;
        [SerializeField] private string menuSceneName = "MainMenu";

        private Overlay current = Overlay.None;
        private Overlay albumReturn = Overlay.None;
        private Canvas canvas;
        private GameObject dim;
        private RectTransform fichaRoot;
        private RectTransform pauseRoot;
        private RectTransform resultsRoot;
        private AlbumView album;

        private Text fichaName;
        private Text fichaSci;
        private Text fichaSummary;
        private Text fichaCuriosity;
        private Text fichaProgress;
        private RawImage fichaPhoto;

        private RectTransform resultsCards;
        private Text resultsCounter;
        private Text resultsStamp;

        private readonly Queue<FichaRequest> fichaQueue = new Queue<FichaRequest>();
        private int fichaPending;
        private bool resultsPending;

        private void Awake()
        {
            UIKit.EnsureEventSystem();
            canvas = UIKit.CreateCanvas("OverlayCanvas", 30, transform);
            Image d = UIKit.NewImage(canvas.transform, "Oscuro", new Color(0f, 0f, 0f, 0.65f), null, false);
            d.raycastTarget = true;
            UIKit.Stretch(d.rectTransform);
            dim = d.gameObject;

            BuildFicha();
            BuildPause();
            BuildResults();
            album = new AlbumView(canvas.transform, OnAlbumClose);
            HideAll();
        }

        private void OnEnable()
        {
            if (round != null)
            {
                round.PhotoRegistered += OnPhoto;
                round.RoundCompleted += OnRoundDone;
            }
        }

        private void OnDisable()
        {
            if (round != null)
            {
                round.PhotoRegistered -= OnPhoto;
                round.RoundCompleted -= OnRoundDone;
            }
        }

        private void Start()
        {
            Time.timeScale = 1f;
            GameSettings.Apply();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        // ------------------------------------------------------------------ construccion

        private void BuildFicha()
        {
            PanelParts p = UIKit.NewPanel(canvas.transform, "Ficha", Palette.Cream, Palette.WoodDark, 10f);
            fichaRoot = p.Root;
            UIKit.Anchor(fichaRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1360f, 760f), Vector2.zero);

            PanelParts ribbon = UIKit.NewPanel(fichaRoot, "Cinta", Palette.Orange, Palette.Ink, 5f);
            UIKit.Anchor(ribbon.Root, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(640f, 84f), Vector2.zero);
            Text banner = UIKit.NewText(ribbon.Face, "Texto", "¡Ficha desbloqueada!", 44, Palette.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Stretch(banner.rectTransform);

            PanelParts photo = UIKit.NewPanel(p.Face, "Foto", Palette.CreamDark, Palette.Ink, 6f);
            UIKit.Anchor(photo.Root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(480f, 480f), new Vector2(60f, 10f));
            fichaPhoto = UIKit.NewRawImage(photo.Face, "Imagen");
            UIKit.Inset(fichaPhoto.rectTransform, 8f, 8f, 8f, 8f);
            fichaProgress = UIKit.NewText(p.Face, "Progreso", "", 36, Palette.Wood, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UIKit.Anchor(fichaProgress.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(480f, 50f), new Vector2(60f, 70f));

            Vector2 tr = new Vector2(1f, 1f);
            fichaName = UIKit.NewText(p.Face, "Nombre", "", 66, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UIKit.Anchor(fichaName.rectTransform, tr, tr, new Vector2(680f, 84f), new Vector2(-60f, -66f));
            UIKit.BestFit(fichaName, 34, 66);
            fichaSci = UIKit.NewText(p.Face, "Cientifico", "", 38, Palette.LeafDark, TextAnchor.MiddleLeft, FontStyle.BoldAndItalic, false);
            UIKit.Anchor(fichaSci.rectTransform, tr, tr, new Vector2(680f, 52f), new Vector2(-60f, -150f));
            fichaSummary = UIKit.NewText(p.Face, "Resumen", "", 34, Palette.Ink, TextAnchor.UpperLeft, FontStyle.Normal, false);
            UIKit.Anchor(fichaSummary.rectTransform, tr, tr, new Vector2(680f, 220f), new Vector2(-60f, -214f));
            UIKit.BestFit(fichaSummary, 22, 34);
            Text label = UIKit.NewText(p.Face, "SabiasQue", "¿Sabías que…?", 38, Palette.Wood, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UIKit.Anchor(label.rectTransform, tr, tr, new Vector2(680f, 50f), new Vector2(-60f, -448f));
            fichaCuriosity = UIKit.NewText(p.Face, "Curiosidad", "", 32, Palette.Ink, TextAnchor.UpperLeft, FontStyle.Normal, false);
            UIKit.Anchor(fichaCuriosity.rectTransform, tr, tr, new Vector2(680f, 150f), new Vector2(-60f, -500f));
            UIKit.BestFit(fichaCuriosity, 22, 32);

            Button cont = UIKit.LeafButton(p.Face, "Continuar", "Continuar", 40, CloseFicha);
            UIKit.Anchor(cont.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(360f, 90f), new Vector2(-60f, 36f));
            Text hint = UIKit.NewText(p.Face, "Ayuda", "Enter o Espacio", 24, Palette.Wood, TextAnchor.MiddleRight, FontStyle.Bold, false);
            UIKit.Anchor(hint.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(300f, 40f), new Vector2(-440f, 60f));
        }

        private void BuildPause()
        {
            PanelParts p = UIKit.NewPanel(canvas.transform, "Pausa", Palette.Cream, Palette.WoodDark, 10f);
            pauseRoot = p.Root;
            UIKit.Anchor(pauseRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(640f, 640f), Vector2.zero);
            Text title = UIKit.NewText(p.Face, "Titulo", "PAUSA", 80, Palette.Wood, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UIKit.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(560f, 110f), new Vector2(0f, -30f));

            Vector2 c = new Vector2(0.5f, 0.5f);
            Button resume = UIKit.LeafButton(p.Face, "Reanudar", "Reanudar", 40, Resume);
            UIKit.Anchor(resume.GetComponent<RectTransform>(), c, c, new Vector2(480f, 90f), new Vector2(0f, 50f));
            Button menu = UIKit.WoodButton(p.Face, "Menu", "Menú principal", 40, GoToMenu);
            UIKit.Anchor(menu.GetComponent<RectTransform>(), c, c, new Vector2(480f, 90f), new Vector2(0f, -70f));
            Button quit = UIKit.WoodButton(p.Face, "Salir", "Salir", 40, QuitGame);
            UIKit.Anchor(quit.GetComponent<RectTransform>(), c, c, new Vector2(480f, 90f), new Vector2(0f, -190f));
        }

        private void BuildResults()
        {
            PanelParts p = UIKit.NewPanel(canvas.transform, "Resultados", Palette.Cream, Palette.WoodDark, 10f);
            resultsRoot = p.Root;
            UIKit.Anchor(resultsRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1500f, 880f), Vector2.zero);

            PanelParts ribbon = UIKit.NewPanel(resultsRoot, "Cinta", Palette.Leaf, Palette.Ink, 5f);
            UIKit.Anchor(ribbon.Root, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(940f, 110f), Vector2.zero);
            Text banner = UIKit.NewText(ribbon.Face, "Texto", "¡Animales capturados!", 64, Palette.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Stretch(banner.rectTransform);

            resultsCards = UIKit.NewRect("Tarjetas", p.Face);
            UIKit.Anchor(resultsCards, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1400f, 440f), new Vector2(0f, 60f));

            resultsCounter = UIKit.NewText(p.Face, "Contador", "", 50, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UIKit.Anchor(resultsCounter.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900f, 64f), new Vector2(0f, -200f));
            resultsStamp = UIKit.NewText(p.Face, "Sello", "", 52, Palette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Anchor(resultsStamp.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900f, 64f), new Vector2(0f, -262f));

            Vector2 c = new Vector2(0.5f, 0.5f);
            Button again = UIKit.LeafButton(p.Face, "JugarDeNuevo", "Jugar de nuevo", 38, PlayAgain);
            UIKit.Anchor(again.GetComponent<RectTransform>(), c, c, new Vector2(400f, 90f), new Vector2(-430f, -340f));
            Button alb = UIKit.WoodButton(p.Face, "TuAlbum", "Tu álbum", 38, OpenAlbumFromResults);
            UIKit.Anchor(alb.GetComponent<RectTransform>(), c, c, new Vector2(400f, 90f), new Vector2(0f, -340f));
            Button menu = UIKit.WoodButton(p.Face, "Menu", "Menú principal", 38, GoToMenu);
            UIKit.Anchor(menu.GetComponent<RectTransform>(), c, c, new Vector2(400f, 90f), new Vector2(430f, -340f));
        }

        // ------------------------------------------------------------------ eventos

        private void OnPhoto(AnimalSubject animal, Texture2D tex, int n)
        {
            if (n == 1) StartCoroutine(QueueFicha(animal, tex, 0.9f));
        }

        private void OnRoundDone()
        {
            StartCoroutine(ResultsDelay(1.6f));
        }

        private IEnumerator QueueFicha(AnimalSubject animal, Texture2D tex, float delay)
        {
            fichaPending++;
            yield return new WaitForSecondsRealtime(delay);
            fichaPending--;
            FichaRequest r = new FichaRequest();
            r.animal = animal;
            r.photo = tex;
            fichaQueue.Enqueue(r);
        }

        private IEnumerator ResultsDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            resultsPending = true;
        }

        // ------------------------------------------------------------------ control

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.tabKey.wasPressedThisFrame)
                {
                    if (current == Overlay.None) OpenAlbum(Overlay.None);
                    else if (current == Overlay.Album) OnAlbumClose();
                }
                if (kb.escapeKey.wasPressedThisFrame)
                {
                    switch (current)
                    {
                        case Overlay.None: OpenPause(); break;
                        case Overlay.Pause: Resume(); break;
                        case Overlay.Album: OnAlbumClose(); break;
                        case Overlay.Ficha: CloseFicha(); break;
                    }
                }
                if (current == Overlay.Ficha && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)) CloseFicha();
            }

            if (current == Overlay.None)
            {
                if (fichaQueue.Count > 0) ShowFicha(fichaQueue.Dequeue());
                else if (resultsPending && fichaPending == 0)
                {
                    resultsPending = false;
                    OpenResults();
                }
            }
        }

        private void HideAll()
        {
            dim.SetActive(false);
            fichaRoot.gameObject.SetActive(false);
            pauseRoot.gameObject.SetActive(false);
            resultsRoot.gameObject.SetActive(false);
            if (album != null) album.Close();
        }

        private void Open(Overlay o)
        {
            current = o;
            Time.timeScale = 0f;
            photoCamera.Blocked = true;
            player.SetControlsEnabled(false);
            player.SetCursorLocked(false);
            dim.SetActive(true);
            fichaRoot.gameObject.SetActive(o == Overlay.Ficha);
            pauseRoot.gameObject.SetActive(o == Overlay.Pause);
            resultsRoot.gameObject.SetActive(o == Overlay.Results);
            if (o != Overlay.Album) album.Close();
        }

        private void CloseAll()
        {
            current = Overlay.None;
            Time.timeScale = 1f;
            photoCamera.Blocked = false;
            HideAll();
            player.SetControlsEnabled(true);
            player.SetCursorLocked(true);
        }

        private void OpenPause()
        {
            Open(Overlay.Pause);
        }

        private void Resume()
        {
            CloseAll();
        }

        private void OpenAlbum(Overlay from)
        {
            albumReturn = from;
            Open(Overlay.Album);
            album.Open();
        }

        private void OpenAlbumFromResults()
        {
            OpenAlbum(Overlay.Results);
        }

        private void OnAlbumClose()
        {
            album.Close();
            if (albumReturn == Overlay.Results) Open(Overlay.Results);
            else CloseAll();
        }

        private void ShowFicha(FichaRequest r)
        {
            SpeciesInfo info = SpeciesCatalog.Get(r.animal.SpeciesId);
            Open(Overlay.Ficha);
            fichaPhoto.texture = r.photo;
            fichaName.text = info.CommonName;
            fichaSci.text = info.ScientificName;
            fichaSummary.text = info.Summary;
            fichaCuriosity.text = info.Curiosity;
            int n = Mathf.Min(AlbumData.RequiredPhotos, round.PhotosThisRound(r.animal.SpeciesId));
            fichaProgress.text = "Fotos " + n + "/" + AlbumData.RequiredPhotos + "  ·  toma " + (AlbumData.RequiredPhotos - n) + " más para completarla";
        }

        private void CloseFicha()
        {
            CloseAll();
        }

        private void OpenResults()
        {
            Open(Overlay.Results);
            for (int i = resultsCards.childCount - 1; i >= 0; i--) Destroy(resultsCards.GetChild(i).gameObject);

            IList<AnimalSubject> animals = round.RoundAnimals;
            int n = animals.Count;
            for (int i = 0; i < n; i++)
            {
                SpeciesInfo info = SpeciesCatalog.Get(animals[i].SpeciesId);
                AlbumEntry entry = AlbumData.Get(animals[i].SpeciesId);
                PanelParts card = UIKit.NewPanel(resultsCards, "Tarjeta" + i, Palette.CreamDark, Palette.Ink, 5f);
                UIKit.Anchor(card.Root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(380f, 430f), new Vector2((i - (n - 1) * 0.5f) * 420f, 0f));
                PanelParts photo = UIKit.NewPanel(card.Face, "Foto", Color.white, Palette.Ink, 4f);
                UIKit.Anchor(photo.Root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(320f, 320f), new Vector2(0f, -18f));
                RawImage img = UIKit.NewRawImage(photo.Face, "Imagen");
                UIKit.Inset(img.rectTransform, 4f, 4f, 4f, 4f);
                if (entry.Photos.Count > 0) img.texture = entry.Photos[entry.Photos.Count - 1];
                Text name = UIKit.NewText(card.Face, "Nombre", info != null ? info.CommonName : "", 34, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold, false);
                UIKit.Anchor(name.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(350f, 56f), new Vector2(0f, 14f));
                UIKit.BestFit(name, 18, 34);
            }

            resultsCounter.text = "Álbum: " + AlbumData.CompleteCount + " de " + SpeciesCatalog.Count + " especies completas";
            resultsStamp.text = AlbumData.CompleteCount >= SpeciesCatalog.Count ? "¡CATÁLOGO COMPLETO!" : "";
        }

        // ------------------------------------------------------------------ botones

        private void PlayAgain()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void GoToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
