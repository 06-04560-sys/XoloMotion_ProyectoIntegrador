using UnityEngine;
using UnityEngine.UI;

namespace Tsakel.Game
{
    /// <summary>Album de dos paginas: una especie por doble pagina (foto, fotos extra, ficha). Se usa en el menu y en el juego.</summary>
    public class AlbumView
    {
        public RectTransform Root { get; private set; }
        public bool IsOpen { get { return Root != null && Root.gameObject.activeSelf; } }

        private readonly System.Action onClose;
        private int index;
        private int selected = -1;

        private Text counterText;
        private Text leftTitle;
        private Text questionMark;
        private Text progressText;
        private Text pageText;
        private Text sciText;
        private Text summaryText;
        private Text curiosityLabel;
        private Text curiosityText;
        private RawImage bigPhoto;
        private RectTransform progressFill;
        private readonly RawImage[] thumbImages = new RawImage[3];

        public AlbumView(Transform parent, System.Action onClose)
        {
            this.onClose = onClose;
            Build(parent);
            Root.gameObject.SetActive(false);
        }

        public void Open()
        {
            Root.gameObject.SetActive(true);
            selected = -1;
            Refresh();
        }

        public void Close()
        {
            Root.gameObject.SetActive(false);
        }

        private void Build(Transform parent)
        {
            Root = UIKit.NewRect("AlbumView", parent);
            UIKit.Stretch(Root);

            Image dim = UIKit.NewImage(Root, "Dim", new Color(0f, 0f, 0f, 0.74f), null, false);
            dim.raycastTarget = true;
            UIKit.Stretch(dim.rectTransform);

            counterText = UIKit.NewText(Root, "Contador", "", 36, Palette.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Anchor(counterText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1000f, 60f), new Vector2(0f, -28f));

            PanelParts book = UIKit.NewPanel(Root, "Libro", Palette.WoodDark, Palette.Ink, 8f);
            UIKit.Anchor(book.Root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1560f, 820f), new Vector2(0f, 30f));
            PanelParts pages = UIKit.NewPanel(book.Face, "Paginas", Palette.Cream, Palette.Ink, 4f);
            UIKit.Inset(pages.Root, 24f, 24f, 24f, 24f);

            RectTransform spine = UIKit.NewImage(pages.Face, "Lomo", Palette.Alpha(Palette.Ink, 0.55f), null, false).rectTransform;
            spine.anchorMin = new Vector2(0.5f, 0f);
            spine.anchorMax = new Vector2(0.5f, 1f);
            spine.pivot = new Vector2(0.5f, 0.5f);
            spine.sizeDelta = new Vector2(6f, 0f);
            spine.anchoredPosition = Vector2.zero;

            RectTransform left = UIKit.NewRect("PaginaIzq", pages.Face);
            left.anchorMin = new Vector2(0f, 0f);
            left.anchorMax = new Vector2(0.5f, 1f);
            left.offsetMin = new Vector2(24f, 16f);
            left.offsetMax = new Vector2(-24f, -16f);
            RectTransform right = UIKit.NewRect("PaginaDer", pages.Face);
            right.anchorMin = new Vector2(0.5f, 0f);
            right.anchorMax = new Vector2(1f, 1f);
            right.offsetMin = new Vector2(24f, 16f);
            right.offsetMax = new Vector2(-24f, -16f);

            // ---- pagina izquierda
            leftTitle = UIKit.NewText(left, "Titulo", "", 52, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UIKit.Anchor(leftTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(704f, 70f), new Vector2(0f, -6f));
            UIKit.BestFit(leftTitle, 30, 52);

            PanelParts big = UIKit.NewPanel(left, "FotoGrande", Palette.CreamDark, Palette.Ink, 5f);
            UIKit.Anchor(big.Root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(430f, 430f), new Vector2(0f, -86f));
            bigPhoto = UIKit.NewRawImage(big.Face, "Foto");
            UIKit.Inset(bigPhoto.rectTransform, 6f, 6f, 6f, 6f);
            questionMark = UIKit.NewText(big.Face, "Interrogacion", "?", 220, Palette.WoodLight, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Stretch(questionMark.rectTransform);

            float[] xs = new float[] { -140f, 0f, 140f };
            for (int i = 0; i < 3; i++)
            {
                int captured = i;
                PanelParts th = UIKit.NewPanel(left, "Miniatura" + i, Color.white, Palette.Ink, 4f);
                th.RootImage.raycastTarget = true;
                th.FaceImage.raycastTarget = true;
                UIKit.Anchor(th.Root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(120f, 120f), new Vector2(xs[i], -536f));
                Button b = th.Root.gameObject.AddComponent<Button>();
                b.targetGraphic = th.FaceImage;
                ColorBlock cb = b.colors;
                cb.normalColor = Palette.CreamDark;
                cb.highlightedColor = Palette.Cream;
                cb.pressedColor = Palette.WoodLight;
                cb.selectedColor = Palette.CreamDark;
                cb.fadeDuration = 0.08f;
                b.colors = cb;
                b.onClick.AddListener(() => Select(captured));
                thumbImages[i] = UIKit.NewRawImage(th.Face, "Foto");
                UIKit.Inset(thumbImages[i].rectTransform, 4f, 4f, 4f, 4f);
            }

            PanelParts track = UIKit.NewPanel(left, "Progreso", Palette.CreamDark, Palette.Ink, 4f);
            UIKit.Anchor(track.Root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(500f, 36f), new Vector2(0f, 6f));
            Image fill = UIKit.NewImage(track.Face, "Relleno", Palette.Leaf, UIKit.Rounded, true);
            progressFill = fill.rectTransform;
            progressFill.anchorMin = Vector2.zero;
            progressFill.anchorMax = new Vector2(0f, 1f);
            progressFill.offsetMin = Vector2.zero;
            progressFill.offsetMax = Vector2.zero;
            progressText = UIKit.NewText(track.Face, "Texto", "", 24, Palette.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Stretch(progressText.rectTransform);

            // ---- pagina derecha
            sciText = UIKit.NewText(right, "Cientifico", "", 40, Palette.LeafDark, TextAnchor.MiddleLeft, FontStyle.BoldAndItalic, false);
            UIKit.Anchor(sciText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(704f, 56f), new Vector2(0f, -10f));
            UIKit.BestFit(sciText, 24, 40);

            summaryText = UIKit.NewText(right, "Resumen", "", 34, Palette.Ink, TextAnchor.UpperLeft, FontStyle.Normal, false);
            UIKit.Anchor(summaryText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(704f, 300f), new Vector2(0f, -80f));
            UIKit.BestFit(summaryText, 22, 34);

            curiosityLabel = UIKit.NewText(right, "SabiasQue", "¿Sabías que…?", 38, Palette.Wood, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UIKit.Anchor(curiosityLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(704f, 50f), new Vector2(0f, -400f));

            curiosityText = UIKit.NewText(right, "Curiosidad", "", 32, Palette.Ink, TextAnchor.UpperLeft, FontStyle.Normal, false);
            UIKit.Anchor(curiosityText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(704f, 250f), new Vector2(0f, -455f));
            UIKit.BestFit(curiosityText, 22, 32);

            // ---- navegacion
            Button prev = UIKit.WoodButton(Root, "Anterior", "<", 56, Prev);
            UIKit.Anchor(prev.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120f, 90f), new Vector2(-260f, -455f));
            Button next = UIKit.WoodButton(Root, "Siguiente", ">", 56, Next);
            UIKit.Anchor(next.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120f, 90f), new Vector2(260f, -455f));
            pageText = UIKit.NewText(Root, "Pagina", "", 38, Palette.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Anchor(pageText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(340f, 60f), new Vector2(0f, -455f));

            Button close = UIKit.WoodButton(Root, "Cerrar", "Cerrar", 34, Close_);
            UIKit.Anchor(close.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(220f, 70f), new Vector2(-40f, -30f));

            Text hint = UIKit.NewText(Root, "Ayuda", "Tab o Esc para cerrar", 26, Palette.CreamDark, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Anchor(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(700f, 40f), new Vector2(0f, 10f));
        }

        private void Close_()
        {
            if (onClose != null) onClose();
        }

        private void Prev()
        {
            index = (index - 1 + SpeciesCatalog.Count) % SpeciesCatalog.Count;
            selected = -1;
            Refresh();
        }

        private void Next()
        {
            index = (index + 1) % SpeciesCatalog.Count;
            selected = -1;
            Refresh();
        }

        private void Select(int i)
        {
            AlbumEntry e = AlbumData.Get(SpeciesCatalog.All[index].Id);
            if (i < e.Photos.Count)
            {
                selected = i;
                Refresh();
            }
        }

        public void Refresh()
        {
            SpeciesInfo info = SpeciesCatalog.All[index];
            AlbumEntry e = AlbumData.Get(info.Id);

            counterText.text = "Especies completas: " + AlbumData.CompleteCount + " de " + SpeciesCatalog.Count;
            pageText.text = (index + 1) + " / " + SpeciesCatalog.Count;
            leftTitle.text = e.Discovered ? info.CommonName : "???";

            if (e.Discovered)
            {
                if (selected < 0 || selected >= e.Photos.Count) selected = e.Photos.Count - 1;
                bigPhoto.texture = e.Photos[selected];
                bigPhoto.gameObject.SetActive(true);
                questionMark.gameObject.SetActive(false);
                sciText.text = info.ScientificName;
                summaryText.text = info.Summary;
                curiosityLabel.gameObject.SetActive(true);
                curiosityText.text = info.Curiosity;
            }
            else
            {
                bigPhoto.gameObject.SetActive(false);
                questionMark.gameObject.SetActive(true);
                sciText.text = "Especie sin descubrir";
                summaryText.text = "Aún no has fotografiado a este animal. Búscalo durante el recorrido y toma sus fotos para completar esta página.";
                curiosityLabel.gameObject.SetActive(false);
                curiosityText.text = "";
            }

            for (int i = 0; i < 3; i++)
            {
                bool has = i < e.Photos.Count;
                thumbImages[i].gameObject.SetActive(has);
                if (has) thumbImages[i].texture = e.Photos[i];
            }

            progressFill.anchorMax = new Vector2(Mathf.Clamp01(e.Progress / (float)AlbumData.RequiredPhotos), 1f);
            progressText.text = "Fotos " + e.Progress + "/" + AlbumData.RequiredPhotos;
        }
    }
}
