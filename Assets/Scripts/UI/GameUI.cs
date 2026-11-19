using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tsakel.Game
{
    /// <summary>HUD del juego: insignias, lista de la ronda, visor de la camara, flash y mensajes. Interfaz provisional creada por codigo.</summary>
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private PhotoCameraController photoCamera;
        [SerializeField] private RoundManager round;

        private class RowUI
        {
            public AnimalSubject animal;
            public Text name;
            public Text count;
            public RectTransform fill;
        }

        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform hudRoot;
        private CanvasGroup hudGroup;
        private RectTransform albumBadge;

        private RectTransform vfRoot;
        private CanvasGroup vfGroup;
        private RectTransform barTop, barBottom, barLeft, barRight, frame;
        private readonly Image[] frameLines = new Image[4];
        private Image ringImage;
        private Text hintText;
        private Image redBlink;
        private Image flashImage;
        private CanvasGroup progressGroup;
        private Text progressName;
        private Text progressCount;
        private RectTransform progressFill;
        private RectTransform zoomHandle;
        private Text zoomLabel;

        private readonly List<RowUI> rows = new List<RowUI>();
        private Text toastText;
        private RectTransform flyRoot;
        private RawImage flyImage;

        private float hintTimer;
        private float redTimer;
        private float flashTimer;
        private float toastTimer;

        private void Awake()
        {
            UIKit.EnsureEventSystem();
            canvas = UIKit.CreateCanvas("GameCanvas", 10, transform);
            canvasRect = canvas.GetComponent<RectTransform>();
            BuildHud();
            BuildViewfinder();
            BuildEffects();
        }

        private void Start()
        {
            BuildRoundList();
        }

        private void OnEnable()
        {
            if (round != null) round.PhotoRegistered += OnPhoto;
            if (photoCamera != null) photoCamera.PhotoRejected += OnRejected;
        }

        private void OnDisable()
        {
            if (round != null) round.PhotoRegistered -= OnPhoto;
            if (photoCamera != null) photoCamera.PhotoRejected -= OnRejected;
        }

        // ------------------------------------------------------------------ construccion

        private void BuildHud()
        {
            hudRoot = UIKit.NewRect("HUD", canvas.transform);
            UIKit.Stretch(hudRoot);
            hudGroup = hudRoot.gameObject.AddComponent<CanvasGroup>();
            hudGroup.blocksRaycasts = false;
            hudGroup.interactable = false;

            // insignia de camara (abajo izquierda)
            PanelParts cam = UIKit.NewPanel(hudRoot, "InsigniaCamara", Palette.Wood, Palette.Ink, 5f);
            UIKit.Anchor(cam.Root, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(330f, 100f), new Vector2(40f, 40f));
            BuildCameraIcon(cam.Face);
            Text c1 = UIKit.NewText(cam.Face, "Titulo", "CÁMARA", 32, Palette.Cream, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UIKit.Anchor(c1.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 44f), new Vector2(104f, -8f));
            Text c2 = UIKit.NewText(cam.Face, "Tecla", "Clic derecho  /  C", 22, Palette.CreamDark, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UIKit.Anchor(c2.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 36f), new Vector2(104f, -52f));

            // insignia de album (abajo derecha)
            PanelParts alb = UIKit.NewPanel(hudRoot, "InsigniaAlbum", Palette.Leaf, Palette.Ink, 5f);
            UIKit.Anchor(alb.Root, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(300f, 100f), new Vector2(-40f, 40f));
            albumBadge = alb.Root;
            BuildAlbumIcon(alb.Face);
            Text a1 = UIKit.NewText(alb.Face, "Titulo", "ÁLBUM", 32, Palette.Cream, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UIKit.Anchor(a1.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(190f, 44f), new Vector2(100f, -8f));
            Text a2 = UIKit.NewText(alb.Face, "Tecla", "Tab", 24, Palette.CreamDark, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UIKit.Anchor(a2.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(190f, 36f), new Vector2(100f, -52f));

            // mira
            Image dotBorder = UIKit.NewImage(hudRoot, "MiraBorde", Palette.Ink, UIKit.Circle, false);
            UIKit.Anchor(dotBorder.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(14f, 14f), Vector2.zero);
            Image dot = UIKit.NewImage(hudRoot, "Mira", Palette.Cream, UIKit.Circle, false);
            UIKit.Anchor(dot.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(8f, 8f), Vector2.zero);
        }

        private void BuildCameraIcon(RectTransform parent)
        {
            Vector2 c = new Vector2(0.5f, 0.5f);
            RectTransform box = UIKit.NewRect("Icono", parent);
            UIKit.Anchor(box, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(84f, 70f), new Vector2(10f, 0f));
            Image body = UIKit.NewImage(box, "Cuerpo", Palette.Cream, UIKit.Rounded, true);
            UIKit.Anchor(body.rectTransform, c, c, new Vector2(70f, 46f), new Vector2(0f, -4f));
            Image bump = UIKit.NewImage(box, "Visor", Palette.CreamDark, UIKit.Rounded, false);
            UIKit.Anchor(bump.rectTransform, c, c, new Vector2(26f, 12f), new Vector2(-16f, 22f));
            Image lensOuter = UIKit.NewImage(box, "LenteBorde", Palette.Ink, UIKit.Circle, false);
            UIKit.Anchor(lensOuter.rectTransform, c, c, new Vector2(38f, 38f), new Vector2(0f, -4f));
            Image lens = UIKit.NewImage(box, "Lente", new Color(0.12f, 0.1f, 0.1f, 1f), UIKit.Circle, false);
            UIKit.Anchor(lens.rectTransform, c, c, new Vector2(28f, 28f), new Vector2(0f, -4f));
            Image shine = UIKit.NewImage(box, "Brillo", Color.white, UIKit.Circle, false);
            UIKit.Anchor(shine.rectTransform, c, c, new Vector2(8f, 8f), new Vector2(-5f, 1f));
        }

        private void BuildAlbumIcon(RectTransform parent)
        {
            Vector2 c = new Vector2(0.5f, 0.5f);
            RectTransform box = UIKit.NewRect("Icono", parent);
            UIKit.Anchor(box, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(84f, 70f), new Vector2(10f, 0f));
            Image l = UIKit.NewImage(box, "PaginaIzq", Palette.Cream, UIKit.Rounded, false);
            UIKit.Anchor(l.rectTransform, c, c, new Vector2(32f, 46f), new Vector2(-18f, 0f));
            Image r = UIKit.NewImage(box, "PaginaDer", Palette.Cream, UIKit.Rounded, false);
            UIKit.Anchor(r.rectTransform, c, c, new Vector2(32f, 46f), new Vector2(18f, 0f));
            Image s = UIKit.NewImage(box, "Lomo", Palette.Ink, null, false);
            UIKit.Anchor(s.rectTransform, c, c, new Vector2(5f, 52f), Vector2.zero);
        }

        private void BuildViewfinder()
        {
            vfRoot = UIKit.NewRect("Visor", canvas.transform);
            UIKit.Stretch(vfRoot);
            vfGroup = vfRoot.gameObject.AddComponent<CanvasGroup>();
            vfGroup.blocksRaycasts = false;
            vfGroup.interactable = false;

            Color black = new Color(0f, 0f, 0f, 0.94f);
            barTop = UIKit.NewImage(vfRoot, "BarraArriba", black, null, false).rectTransform;
            barBottom = UIKit.NewImage(vfRoot, "BarraAbajo", black, null, false).rectTransform;
            barLeft = UIKit.NewImage(vfRoot, "BarraIzq", black, null, false).rectTransform;
            barRight = UIKit.NewImage(vfRoot, "BarraDer", black, null, false).rectTransform;

            frame = UIKit.NewRect("Marco", vfRoot);
            UIKit.Anchor(frame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(800f, 800f), Vector2.zero);

            // lineas del marco
            Vector2[] amin = { new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f) };
            Vector2[] amax = { new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
            Vector2[] piv = { new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f) };
            Vector2[] siz = { new Vector2(0f, 6f), new Vector2(0f, 6f), new Vector2(6f, 0f), new Vector2(6f, 0f) };
            for (int i = 0; i < 4; i++)
            {
                Image line = UIKit.NewImage(frame, "Linea" + i, Palette.Cream, null, false);
                RectTransform lr = line.rectTransform;
                lr.anchorMin = amin[i];
                lr.anchorMax = amax[i];
                lr.pivot = piv[i];
                lr.sizeDelta = siz[i];
                lr.anchoredPosition = Vector2.zero;
                frameLines[i] = line;
            }

            // cruz de mira
            Vector2 c = new Vector2(0.5f, 0.5f);
            Vector2[] cpos = { new Vector2(-30f, 0f), new Vector2(30f, 0f), new Vector2(0f, 30f), new Vector2(0f, -30f) };
            Vector2[] csize = { new Vector2(22f, 4f), new Vector2(22f, 4f), new Vector2(4f, 22f), new Vector2(4f, 22f) };
            for (int i = 0; i < 4; i++)
            {
                Image b = UIKit.NewImage(frame, "Cruz" + i, Palette.Cream, null, false);
                UIKit.Anchor(b.rectTransform, c, c, csize[i], cpos[i]);
            }
            Image cdot = UIKit.NewImage(frame, "CruzPunto", Palette.Cream, UIKit.Circle, false);
            UIKit.Anchor(cdot.rectTransform, c, c, new Vector2(8f, 8f), Vector2.zero);

            // anillo de recarga
            ringImage = UIKit.NewImage(frame, "Recarga", Palette.Orange, UIKit.Ring, false);
            ringImage.type = Image.Type.Filled;
            ringImage.fillMethod = Image.FillMethod.Radial360;
            ringImage.fillOrigin = (int)Image.Origin360.Top;
            ringImage.fillClockwise = true;
            UIKit.Anchor(ringImage.rectTransform, c, c, new Vector2(120f, 120f), Vector2.zero);

            // parpadeo rojo
            redBlink = UIKit.NewImage(frame, "Rojo", Palette.Alpha(Palette.Red, 0f), null, false);
            UIKit.Stretch(redBlink.rectTransform);

            // pista (mensaje)
            hintText = UIKit.NewText(frame, "Pista", "", 36, Palette.Cream, TextAnchor.LowerCenter, FontStyle.Bold, true);
            UIKit.Anchor(hintText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(1100f, 70f), new Vector2(0f, 20f));

            // barra de progreso del animal (derecha del visor)
            RectTransform prog = UIKit.NewRect("Progreso", frame);
            UIKit.Anchor(prog, new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(240f, 520f), new Vector2(40f, 0f));
            progressGroup = prog.gameObject.AddComponent<CanvasGroup>();
            progressName = UIKit.NewText(prog, "Nombre", "", 28, Palette.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Anchor(progressName.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(240f, 70f), Vector2.zero);
            UIKit.BestFit(progressName, 18, 28);
            PanelParts track = UIKit.NewPanel(prog, "Pista", Palette.Alpha(Palette.Ink, 0.75f), Palette.Cream, 4f);
            RectTransform tr = track.Root;
            tr.anchorMin = new Vector2(0.5f, 0f);
            tr.anchorMax = new Vector2(0.5f, 1f);
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.offsetMin = new Vector2(-20f, 60f);
            tr.offsetMax = new Vector2(20f, -80f);
            Image fill = UIKit.NewImage(track.Face, "Relleno", Palette.LeafLight, UIKit.Rounded, true);
            progressFill = fill.rectTransform;
            progressFill.anchorMin = Vector2.zero;
            progressFill.anchorMax = new Vector2(1f, 0f);
            progressFill.offsetMin = Vector2.zero;
            progressFill.offsetMax = Vector2.zero;
            progressCount = UIKit.NewText(prog, "Cuenta", "", 36, Palette.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Anchor(progressCount.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(200f, 50f), new Vector2(0f, 6f));

            // zoom (abajo del visor)
            RectTransform zoom = UIKit.NewRect("Zoom", frame);
            UIKit.Anchor(zoom, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(560f, 60f), new Vector2(0f, -28f));
            Image ztrack = UIKit.NewImage(zoom, "Pista", Palette.Ink, UIKit.Rounded, false);
            UIKit.Anchor(ztrack.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, 14f), Vector2.zero);
            Image zline = UIKit.NewImage(zoom, "PistaLinea", Palette.Cream, UIKit.Rounded, false);
            UIKit.Anchor(zline.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(414f, 8f), Vector2.zero);
            Image hb = UIKit.NewImage(zoom, "ManijaBorde", Palette.Ink, UIKit.Circle, false);
            UIKit.Anchor(hb.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(38f, 38f), Vector2.zero);
            zoomHandle = hb.rectTransform;
            Image hf = UIKit.NewImage(hb.transform, "Manija", Palette.Orange, UIKit.Circle, false);
            UIKit.Inset(hf.rectTransform, 6f, 6f, 6f, 6f);
            zoomLabel = UIKit.NewText(zoom, "Etiqueta", "x1.0", 32, Palette.Cream, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UIKit.Anchor(zoomLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f), new Vector2(110f, 40f), new Vector2(240f, 0f));
        }

        private void BuildEffects()
        {
            flashImage = UIKit.NewImage(canvas.transform, "Flash", new Color(1f, 1f, 1f, 0f), null, false);
            UIKit.Stretch(flashImage.rectTransform);

            toastText = UIKit.NewText(canvas.transform, "Aviso", "", 42, Palette.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Anchor(toastText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1200f, 70f), new Vector2(0f, -40f));

            PanelParts fly = UIKit.NewPanel(canvas.transform, "FotoVolando", Palette.Cream, Palette.Ink, 8f);
            flyRoot = fly.Root;
            UIKit.Anchor(flyRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(340f, 340f), Vector2.zero);
            flyImage = UIKit.NewRawImage(fly.Face, "Foto");
            UIKit.Inset(flyImage.rectTransform, 8f, 8f, 8f, 8f);
            flyRoot.gameObject.SetActive(false);
        }

        private void BuildRoundList()
        {
            if (round == null) return;
            IList<AnimalSubject> animals = round.RoundAnimals;
            int n = animals.Count;
            PanelParts panel = UIKit.NewPanel(hudRoot, "Ronda", Palette.Alpha(Palette.WoodDark, 0.93f), Palette.Ink, 5f);
            UIKit.Anchor(panel.Root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, 74f + 62f * n), new Vector2(40f, -40f));
            Text header = UIKit.NewText(panel.Face, "Encabezado", "ANIMALES DE LA RONDA", 24, Palette.CreamDark, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Anchor(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(400f, 40f), new Vector2(0f, -8f));

            for (int i = 0; i < n; i++)
            {
                SpeciesInfo info = SpeciesCatalog.Get(animals[i].SpeciesId);
                float y = -56f - i * 62f;
                RowUI row = new RowUI();
                row.animal = animals[i];
                row.name = UIKit.NewText(panel.Face, "Nombre" + i, info != null ? info.CommonName : animals[i].SpeciesId, 28, Palette.Cream, TextAnchor.MiddleLeft, FontStyle.Bold, true);
                UIKit.Anchor(row.name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(230f, 44f), new Vector2(14f, y));
                UIKit.BestFit(row.name, 16, 28);
                PanelParts bar = UIKit.NewPanel(panel.Face, "Barra" + i, Palette.Alpha(Palette.Ink, 0.7f), Palette.Cream, 3f);
                UIKit.Anchor(bar.Root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(84f, 22f), new Vector2(-84f, y - 11f));
                Image f = UIKit.NewImage(bar.Face, "Relleno", Palette.LeafLight, null, false);
                row.fill = f.rectTransform;
                row.fill.anchorMin = Vector2.zero;
                row.fill.anchorMax = new Vector2(0f, 1f);
                row.fill.offsetMin = Vector2.zero;
                row.fill.offsetMax = Vector2.zero;
                row.count = UIKit.NewText(panel.Face, "Cuenta" + i, "0/3", 26, Palette.Cream, TextAnchor.MiddleRight, FontStyle.Bold, true);
                UIKit.Anchor(row.count.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(64f, 44f), new Vector2(-12f, y));
                rows.Add(row);
            }
        }

        // ------------------------------------------------------------------ actualizacion

        public void ShowToast(string message)
        {
            toastText.text = message;
            toastTimer = 2.2f;
        }

        private void Update()
        {
            if (photoCamera == null) return;
            float dt = Time.unscaledDeltaTime;
            float amount = photoCamera.Amount;
            hudGroup.alpha = 1f - amount;
            bool vfOn = amount > 0.001f;
            if (vfRoot.gameObject.activeSelf != vfOn) vfRoot.gameObject.SetActive(vfOn);
            vfGroup.alpha = amount;
            if (vfOn) UpdateViewfinder();
            UpdateRows();

            if (hintTimer > 0f)
            {
                hintTimer -= dt;
                if (hintTimer <= 0f) hintText.text = "";
            }
            if (redTimer > 0f)
            {
                redTimer -= dt;
                redBlink.color = Palette.Alpha(Palette.Red, Mathf.Clamp01(redTimer / 0.35f) * 0.45f);
            }
            if (flashTimer > 0f)
            {
                flashTimer -= dt;
                flashImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(flashTimer / 0.2f));
            }
            if (toastTimer > 0f)
            {
                toastTimer -= dt;
                toastText.color = Palette.Alpha(Palette.Cream, Mathf.Clamp01(toastTimer / 0.5f));
            }
            else if (toastText.text.Length > 0)
            {
                toastText.text = "";
            }
        }

        private void UpdateViewfinder()
        {
            float side = photoCamera.ViewfinderHeight * canvasRect.rect.height;
            float h = side * 0.5f;
            barTop.anchorMin = new Vector2(0f, 0.5f); barTop.anchorMax = new Vector2(1f, 1f);
            barTop.offsetMin = new Vector2(0f, h); barTop.offsetMax = Vector2.zero;
            barBottom.anchorMin = new Vector2(0f, 0f); barBottom.anchorMax = new Vector2(1f, 0.5f);
            barBottom.offsetMin = Vector2.zero; barBottom.offsetMax = new Vector2(0f, -h);
            barLeft.anchorMin = new Vector2(0f, 0.5f); barLeft.anchorMax = new Vector2(0.5f, 0.5f);
            barLeft.offsetMin = new Vector2(0f, -h); barLeft.offsetMax = new Vector2(-h, h);
            barRight.anchorMin = new Vector2(0.5f, 0.5f); barRight.anchorMax = new Vector2(1f, 0.5f);
            barRight.offsetMin = new Vector2(h, -h); barRight.offsetMax = new Vector2(0f, h);
            frame.sizeDelta = new Vector2(side, side);

            // el marco se pone verde cuando la foto seria valida
            Color lineColor = photoCamera.Live.IsValid ? Palette.LeafLight : Palette.Cream;
            for (int i = 0; i < frameLines.Length; i++) frameLines[i].color = lineColor;

            // recarga
            float cd = photoCamera.CooldownLeft;
            if (cd > 0f)
            {
                ringImage.color = Palette.Alpha(Palette.Orange, 0.95f);
                ringImage.fillAmount = 1f - Mathf.Clamp01(cd / photoCamera.CooldownTotal);
            }
            else
            {
                ringImage.color = Palette.Alpha(Palette.Orange, 0f);
            }

            // zoom
            float t = Mathf.InverseLerp(photoCamera.MinZoom, photoCamera.MaxZoom, photoCamera.Zoom);
            zoomHandle.anchoredPosition = new Vector2(Mathf.Lerp(-200f, 200f, t), 0f);
            zoomLabel.text = "x" + photoCamera.Zoom.ToString("0.0");

            // progreso del animal enfocado
            AnimalSubject target = photoCamera.Target;
            if (target != null && round != null)
            {
                SpeciesInfo info = SpeciesCatalog.Get(target.SpeciesId);
                int n = Mathf.Min(AlbumData.RequiredPhotos, round.PhotosThisRound(target.SpeciesId));
                progressGroup.alpha = 1f;
                progressName.text = info != null ? info.CommonName : "";
                progressCount.text = n + "/" + AlbumData.RequiredPhotos;
                progressFill.anchorMax = new Vector2(1f, n / (float)AlbumData.RequiredPhotos);
            }
            else
            {
                progressGroup.alpha = 0.35f;
                progressName.text = "";
                progressCount.text = "";
                progressFill.anchorMax = new Vector2(1f, 0f);
            }
        }

        private void UpdateRows()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                RowUI r = rows[i];
                int n = Mathf.Min(AlbumData.RequiredPhotos, round.PhotosThisRound(r.animal.SpeciesId));
                r.fill.anchorMax = new Vector2(n / (float)AlbumData.RequiredPhotos, 1f);
                r.count.text = n + "/" + AlbumData.RequiredPhotos;
                r.name.color = n >= AlbumData.RequiredPhotos ? Palette.LeafLight : Palette.Cream;
            }
        }

        // ------------------------------------------------------------------ eventos

        private void OnPhoto(AnimalSubject animal, Texture2D tex, int n)
        {
            flashTimer = 0.2f;
            SpeciesInfo info = SpeciesCatalog.Get(animal.SpeciesId);
            string name = info != null ? info.CommonName : "";
            if (n <= AlbumData.RequiredPhotos) ShowToast("¡Foto guardada!  " + name + "  " + n + "/" + AlbumData.RequiredPhotos);
            else ShowToast("¡Foto guardada!");
            StartCoroutine(FlyRoutine(tex));
        }

        private void OnRejected(PhotoVerdict verdict)
        {
            redTimer = 0.35f;
            hintTimer = 2.2f;
            switch (verdict)
            {
                case PhotoVerdict.TooFar: hintText.text = "Acércate un poco más"; break;
                case PhotoVerdict.TooSmall: hintText.text = "Usa el zoom o acércate"; break;
                case PhotoVerdict.OutOfFrame: hintText.text = "Centra al animal en el visor"; break;
                case PhotoVerdict.Blocked: hintText.text = "Algo tapa al animal"; break;
                default: hintText.text = "No hay ningún animal a la vista"; break;
            }
        }

        private IEnumerator FlyRoutine(Texture2D tex)
        {
            flyRoot.gameObject.SetActive(true);
            flyImage.texture = tex;
            Vector2 target = new Vector2(canvasRect.rect.width * 0.5f - 190f, -canvasRect.rect.height * 0.5f + 90f);
            float hold = 0.25f;
            float move = 0.4f;
            float t = 0f;
            while (t < hold + move)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01((t - hold) / move);
                k = k * k * (3f - 2f * k);
                flyRoot.anchoredPosition = Vector2.Lerp(Vector2.zero, target, k);
                flyRoot.localScale = Vector3.one * Mathf.Lerp(1f, 0.15f, k);
                yield return null;
            }
            flyRoot.gameObject.SetActive(false);

            // rebote de la insignia del album
            float b = 0f;
            while (b < 0.25f)
            {
                b += Time.unscaledDeltaTime;
                float s = 1f + 0.25f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(b / 0.25f));
                albumBadge.localScale = Vector3.one * s;
                yield return null;
            }
            albumBadge.localScale = Vector3.one;
        }
    }
}
