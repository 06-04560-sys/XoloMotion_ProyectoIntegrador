using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tsakel.Game
{
    /// <summary>Menu principal provisional: Iniciar (con tutorial), Album, Ajustes y Creditos.</summary>
    public partial class MainMenu : MonoBehaviour
    {
        [SerializeField] private Texture backgroundTexture;
        [SerializeField] private string gameSceneName = "Nivel1";

        private Canvas canvas;
        private RawImage background;
        private RectTransform mainRoot;
        private RectTransform tutorialRoot;
        private RectTransform settingsRoot;
        private RectTransform creditsRoot;
        private AlbumView album;
        private readonly RectTransform[] tutorialPages = new RectTransform[2];
        private Button tutorialBack;
        private Button tutorialNext;
        private Button tutorialPlay;
        private int lastW;
        private int lastH;

        private void Awake()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameSettings.Apply();
            UIKit.EnsureEventSystem();
            canvas = UIKit.CreateCanvas("MenuCanvas", 10, transform);
            BuildBackground();
            BuildMain();
            BuildTutorial();
            BuildSettings();
            BuildCredits();
            album = new AlbumView(canvas.transform, CloseAlbum);
            ShowMain();
        }

        private void Update()
        {
            if (Screen.width != lastW || Screen.height != lastH) FitBackground();
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                if (album.IsOpen) CloseAlbum();
                else if (!mainRoot.gameObject.activeSelf || tutorialRoot.gameObject.activeSelf || settingsRoot.gameObject.activeSelf || creditsRoot.gameObject.activeSelf) ShowMain();
            }
        }

        private void BuildBackground()
        {
            Image fallback = UIKit.NewImage(canvas.transform, "FondoColor", Palette.LeafDark, null, false);
            UIKit.Stretch(fallback.rectTransform);
            background = UIKit.NewRawImage(canvas.transform, "Fondo");
            UIKit.Stretch(background.rectTransform);
            if (backgroundTexture != null) background.texture = backgroundTexture;
            else background.enabled = false;
            FitBackground();
            Image tint = UIKit.NewImage(canvas.transform, "Velo", new Color(0.1f, 0.12f, 0.05f, 0.35f), null, false);
            UIKit.Stretch(tint.rectTransform);
        }

        private void FitBackground()
        {
            lastW = Screen.width;
            lastH = Screen.height;
            if (background == null || backgroundTexture == null) return;
            float ta = backgroundTexture.width / (float)backgroundTexture.height;
            float sa = Screen.width / (float)Mathf.Max(1, Screen.height);
            Rect uv = new Rect(0f, 0f, 1f, 1f);
            if (sa > ta) { uv.height = ta / sa; uv.y = (1f - uv.height) * 0.5f; }
            else { uv.width = sa / ta; uv.x = (1f - uv.width) * 0.5f; }
            background.uvRect = uv;
        }

        private void BuildMain()
        {
            mainRoot = UIKit.NewRect("Principal", canvas.transform);
            UIKit.Stretch(mainRoot);
            Vector2 top = new Vector2(0.5f, 1f);
            Vector2 c = new Vector2(0.5f, 0.5f);

            Sprite logo = UIKit.LoadLogo();
            if (logo != null)
            {
                Image img = UIKit.NewImage(mainRoot, "Logo", Color.white, logo, false);
                img.preserveAspect = true;
                UIKit.Anchor(img.rectTransform, top, top, new Vector2(1000f, 400f), new Vector2(0f, -40f));
            }
            else
            {
                PanelParts sign = UIKit.NewPanel(mainRoot, "Letrero", Palette.Wood, Palette.Ink, 8f);
                UIKit.Anchor(sign.Root, top, top, new Vector2(1000f, 340f), new Vector2(0f, -40f));
                Text small = UIKit.NewText(sign.Face, "ElLibroDe", "EL LIBRO DE", 52, Palette.LeafLight, TextAnchor.MiddleCenter, FontStyle.Bold, true);
                UIKit.Anchor(small.rectTransform, top, top, new Vector2(900f, 70f), new Vector2(0f, -20f));
                string title = "<color=#7A9A2E>TS</color><color=#F5EAD0>A</color><color=#F29A3A>K</color><color=#A8C84A>EL</color>";
                Text big = UIKit.NewText(sign.Face, "Titulo", title, 210, Palette.Leaf, TextAnchor.MiddleCenter, FontStyle.Bold, true);
                UIKit.Anchor(big.rectTransform, top, top, new Vector2(940f, 230f), new Vector2(0f, -85f));
            }

            Button start = UIKit.LeafButton(mainRoot, "Iniciar", "INICIAR", 48, ShowTutorial);
            UIKit.Anchor(start.GetComponent<RectTransform>(), c, c, new Vector2(460f, 96f), new Vector2(0f, 40f));
            Button alb = UIKit.WoodButton(mainRoot, "Album", "ÁLBUM", 44, OpenAlbum);
            UIKit.Anchor(alb.GetComponent<RectTransform>(), c, c, new Vector2(460f, 90f), new Vector2(0f, -70f));
            Button set = UIKit.WoodButton(mainRoot, "Ajustes", "AJUSTES", 44, ShowSettings);
            UIKit.Anchor(set.GetComponent<RectTransform>(), c, c, new Vector2(460f, 90f), new Vector2(0f, -176f));
            Button cred = UIKit.WoodButton(mainRoot, "Creditos", "CRÉDITOS", 44, ShowCredits);
            UIKit.Anchor(cred.GetComponent<RectTransform>(), c, c, new Vector2(460f, 90f), new Vector2(0f, -282f));
            Button quit = UIKit.WoodButton(mainRoot, "Salir", "Salir", 30, QuitGame);
            UIKit.Anchor(quit.GetComponent<RectTransform>(), c, c, new Vector2(220f, 64f), new Vector2(0f, -378f));

            Text team = UIKit.NewText(mainRoot, "Equipo", "XOLOMOTION", 34, Palette.Cream, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UIKit.Anchor(team.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(500f, 50f), new Vector2(40f, 24f));
            Text ver = UIKit.NewText(mainRoot, "Version", "Demo v0.1", 28, Palette.Cream, TextAnchor.MiddleRight, FontStyle.Bold, true);
            UIKit.Anchor(ver.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(400f, 50f), new Vector2(-40f, 24f));
        }

        private RectTransform NewModal(string name, string title, float w, float h, out PanelParts panel)
        {
            Vector2 c = new Vector2(0.5f, 0.5f);
            RectTransform root = UIKit.NewRect(name, canvas.transform);
            UIKit.Stretch(root);
            Image dim = UIKit.NewImage(root, "Oscuro", new Color(0f, 0f, 0f, 0.6f), null, false);
            dim.raycastTarget = true;
            UIKit.Stretch(dim.rectTransform);
            panel = UIKit.NewPanel(root, "Panel", Palette.Cream, Palette.WoodDark, 10f);
            UIKit.Anchor(panel.Root, c, c, new Vector2(w, h), Vector2.zero);
            PanelParts ribbon = UIKit.NewPanel(panel.Root, "Cinta", Palette.Leaf, Palette.Ink, 5f);
            UIKit.Anchor(ribbon.Root, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(720f, 100f), Vector2.zero);
            Text t = UIKit.NewText(ribbon.Face, "Texto", title, 60, Palette.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Stretch(t.rectTransform);
            return root;
        }

        private void HideAll()
        {
            mainRoot.gameObject.SetActive(false);
            tutorialRoot.gameObject.SetActive(false);
            settingsRoot.gameObject.SetActive(false);
            creditsRoot.gameObject.SetActive(false);
            if (album != null) album.Close();
        }

        private void ShowMain()
        {
            HideAll();
            mainRoot.gameObject.SetActive(true);
        }

        private void ShowSettings()
        {
            HideAll();
            mainRoot.gameObject.SetActive(true);
            settingsRoot.gameObject.SetActive(true);
        }

        private void ShowCredits()
        {
            HideAll();
            mainRoot.gameObject.SetActive(true);
            creditsRoot.gameObject.SetActive(true);
        }

        private void OpenAlbum()
        {
            HideAll();
            mainRoot.gameObject.SetActive(true);
            album.Open();
        }

        private void CloseAlbum()
        {
            ShowMain();
        }

        private void ShowTutorial()
        {
            HideAll();
            mainRoot.gameObject.SetActive(true);
            tutorialRoot.gameObject.SetActive(true);
            SetTutorialPage(0);
        }

        private void StartGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void GoPage0() { SetTutorialPage(0); }
        private void GoPage1() { SetTutorialPage(1); }

        private void BuildTutorial()
        {
            PanelParts panel;
            tutorialRoot = NewModal("Tutorial", "¿Cómo jugar?", 1400f, 820f, out panel);
            Vector2 tl = new Vector2(0f, 1f);

            tutorialPages[0] = UIKit.NewRect("Reglas", panel.Face);
            UIKit.Stretch(tutorialPages[0]);
            Text rt = UIKit.NewText(tutorialPages[0], "Subtitulo", "Reglas", 56, Palette.Orange, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UIKit.Anchor(rt.rectTransform, tl, tl, new Vector2(1200f, 70f), new Vector2(100f, -90f));
            string rules =
                "•  Recorre la selva con calma y encuentra a los animales de Chiapas.\n\n" +
                "•  Levanta tu cámara, enfoca al animal y toma 3 fotos para completar su ficha en el álbum.\n\n" +
                "•  Los animales son tímidos: se esconden y vuelven a aparecer. ¡Observa sus patrones!\n\n" +
                "•  Cada ronda tiene 3 animales. Complétalos para ver tu resumen.";
            Text rb = UIKit.NewText(tutorialPages[0], "Texto", rules, 40, Palette.Ink, TextAnchor.UpperLeft, FontStyle.Normal, false);
            UIKit.Anchor(rb.rectTransform, tl, tl, new Vector2(1200f, 480f), new Vector2(100f, -180f));
            UIKit.BestFit(rb, 24, 40);

            tutorialPages[1] = UIKit.NewRect("Controles", panel.Face);
            UIKit.Stretch(tutorialPages[1]);
            Text ct = UIKit.NewText(tutorialPages[1], "Subtitulo", "Controles", 56, Palette.Orange, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UIKit.Anchor(ct.rectTransform, tl, tl, new Vector2(1200f, 70f), new Vector2(100f, -90f));
            string[] labels = new string[] { "Caminar", "Mirar", "Levantar la cámara", "Zoom", "Tomar la foto", "Álbum", "Pausa" };
            string[] values = new string[] { "W  A  S  D", "Mouse", "Clic derecho (mantener)  o  C", "Rueda del mouse  o  Q / E", "Clic izquierdo  o  Espacio", "Tab", "Esc" };
            for (int i = 0; i < labels.Length; i++)
            {
                float y = -190f - i * 62f;
                Text l = UIKit.NewText(tutorialPages[1], "Accion" + i, labels[i], 38, Palette.Wood, TextAnchor.MiddleLeft, FontStyle.Bold, false);
                UIKit.Anchor(l.rectTransform, tl, tl, new Vector2(460f, 56f), new Vector2(100f, y));
                Text v = UIKit.NewText(tutorialPages[1], "Tecla" + i, values[i], 38, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold, false);
                UIKit.Anchor(v.rectTransform, tl, tl, new Vector2(700f, 56f), new Vector2(580f, y));
                UIKit.BestFit(v, 22, 38);
            }

            Vector2 bl = new Vector2(0f, 0f);
            Vector2 br = new Vector2(1f, 0f);
            tutorialBack = UIKit.WoodButton(panel.Face, "Atras", "Atrás", 38, GoPage0);
            UIKit.Anchor(tutorialBack.GetComponent<RectTransform>(), bl, bl, new Vector2(300f, 84f), new Vector2(60f, 40f));
            tutorialNext = UIKit.LeafButton(panel.Face, "Siguiente", "Siguiente", 38, GoPage1);
            UIKit.Anchor(tutorialNext.GetComponent<RectTransform>(), br, br, new Vector2(340f, 84f), new Vector2(-60f, 40f));
            tutorialPlay = UIKit.LeafButton(panel.Face, "AJugar", "¡A jugar!", 40, StartGame);
            UIKit.Anchor(tutorialPlay.GetComponent<RectTransform>(), br, br, new Vector2(340f, 84f), new Vector2(-60f, 40f));
            Button close = UIKit.WoodButton(panel.Face, "Volver", "Volver", 32, ShowMain);
            UIKit.Anchor(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(240f, 70f), new Vector2(0f, 46f));
        }

        private void SetTutorialPage(int page)
        {
            tutorialPages[0].gameObject.SetActive(page == 0);
            tutorialPages[1].gameObject.SetActive(page == 1);
            tutorialBack.gameObject.SetActive(page == 1);
            tutorialNext.gameObject.SetActive(page == 0);
            tutorialPlay.gameObject.SetActive(page == 1);
        }
    }
}
