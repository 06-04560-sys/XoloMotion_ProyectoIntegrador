using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Tsakel.Game
{
    /// <summary>Paleta tomada del logo de Tsakel (verdes, naranja, madera y crema).</summary>
    public static class Palette
    {
        public static readonly Color Ink = Hex("2A1A10");
        public static readonly Color WoodDark = Hex("7A3E22");
        public static readonly Color Wood = Hex("B5603A");
        public static readonly Color WoodLight = Hex("D98E4F");
        public static readonly Color Orange = Hex("F29A3A");
        public static readonly Color LeafDark = Hex("4E6B1E");
        public static readonly Color Leaf = Hex("7A9A2E");
        public static readonly Color LeafLight = Hex("A8C84A");
        public static readonly Color Cream = Hex("F5EAD0");
        public static readonly Color CreamDark = Hex("E0CFA8");
        public static readonly Color Red = Hex("D9482B");

        public static Color Hex(string hex)
        {
            Color c;
            ColorUtility.TryParseHtmlString("#" + hex, out c);
            return c;
        }

        public static Color Alpha(Color c, float a)
        {
            c.a = a;
            return c;
        }
    }

    public class PanelParts
    {
        public RectTransform Root;
        public RectTransform Face;
        public Image RootImage;
        public Image FaceImage;
    }

    /// <summary>Ayudas para construir la interfaz provisional por codigo (sin arte final).</summary>
    public static class UIKit
    {
        private static Sprite rounded;
        private static Sprite circle;
        private static Sprite ring;
        private static Font font;

        public static Font DefaultFont
        {
            get
            {
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        public static Sprite Rounded
        {
            get
            {
                if (rounded == null) rounded = MakeSprite(64, RoundedAlpha, new Vector4(20f, 20f, 20f, 20f));
                return rounded;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (circle == null) circle = MakeSprite(64, CircleAlpha, Vector4.zero);
                return circle;
            }
        }

        public static Sprite Ring
        {
            get
            {
                if (ring == null) ring = MakeSprite(128, RingAlpha, Vector4.zero);
                return ring;
            }
        }

        /// <summary>
        /// Logo del menu: Assets/Resources/UI/Logo.png. Si se importo como textura y no como Sprite,
        /// se crea el Sprite aqui para que el logo aparezca de todos modos.
        /// </summary>
        public static Sprite LoadLogo()
        {
            Sprite sprite = Resources.Load<Sprite>("UI/Logo");
            if (sprite != null) return sprite;
            Texture2D tex = Resources.Load<Texture2D>("UI/Logo");
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float RoundedAlpha(float x, float y)
        {
            float h = 32f;
            float r = 18f;
            float px = Mathf.Abs(x - h) - (h - r);
            float py = Mathf.Abs(y - h) - (h - r);
            float dist = new Vector2(Mathf.Max(px, 0f), Mathf.Max(py, 0f)).magnitude + Mathf.Min(Mathf.Max(px, py), 0f) - r;
            return 0.5f - dist;
        }

        private static float CircleAlpha(float x, float y)
        {
            return 0.5f - (new Vector2(x - 32f, y - 32f).magnitude - 31f);
        }

        private static float RingAlpha(float x, float y)
        {
            float d = new Vector2(x - 64f, y - 64f).magnitude;
            return 0.5f - (Mathf.Abs(d - 58f) - 5f);
        }

        private static Sprite MakeSprite(int size, System.Func<float, float, float> alphaAt, Vector4 border)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color32[] px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float a = Mathf.Clamp01(alphaAt(x + 0.5f, y + 0.5f));
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply();
            Sprite sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            sp.hideFlags = HideFlags.HideAndDontSave;
            return sp;
        }

        // ------------------------------------------------------------------ estructura

        public static Canvas CreateCanvas(string name, int sortingOrder, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            if (parent != null) go.transform.SetParent(parent, false);
            Canvas c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sortingOrder;
            CanvasScaler s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920f, 1080f);
            s.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            s.matchWidthOrHeight = 0.5f;
            return c;
        }

        public static RectTransform NewRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>Margenes hacia adentro (izquierda, abajo, derecha, arriba).</summary>
        public static void Inset(RectTransform rt, float l, float b, float r, float t)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
        }

        public static void Anchor(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
        }

        // ------------------------------------------------------------------ elementos

        public static Image NewImage(Transform parent, string name, Color color, Sprite sprite, bool sliced)
        {
            RectTransform rt = NewRect(name, parent);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            img.type = (sprite != null && sliced) ? Image.Type.Sliced : Image.Type.Simple;
            img.raycastTarget = false;
            return img;
        }

        public static RawImage NewRawImage(Transform parent, string name)
        {
            RectTransform rt = NewRect(name, parent);
            RawImage img = rt.gameObject.AddComponent<RawImage>();
            img.color = Color.white;
            img.raycastTarget = false;
            return img;
        }

        public static PanelParts NewPanel(Transform parent, string name, Color face, Color border, float borderSize)
        {
            Image root = NewImage(parent, name, border, Rounded, true);
            Image faceImg = NewImage(root.transform, "Face", face, Rounded, true);
            RectTransform fr = faceImg.rectTransform;
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.offsetMin = new Vector2(borderSize, borderSize);
            fr.offsetMax = new Vector2(-borderSize, -borderSize);
            PanelParts p = new PanelParts();
            p.Root = root.rectTransform;
            p.Face = fr;
            p.RootImage = root;
            p.FaceImage = faceImg;
            return p;
        }

        public static Text NewText(Transform parent, string name, string content, int size, Color color, TextAnchor align, FontStyle style, bool outline)
        {
            RectTransform rt = NewRect(name, parent);
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = DefaultFont;
            t.text = content;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.supportRichText = true;
            t.raycastTarget = false;
            if (outline)
            {
                Outline o = rt.gameObject.AddComponent<Outline>();
                o.effectColor = Palette.Ink;
                o.effectDistance = new Vector2(2f, -2f);
            }
            return t;
        }

        public static void BestFit(Text t, int min, int max)
        {
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = min;
            t.resizeTextMaxSize = max;
        }

        public static Button NewButton(Transform parent, string name, string label, int fontSize, Color face, Color hover, Color pressed, UnityAction onClick)
        {
            PanelParts panel = NewPanel(parent, name, Color.white, Palette.Ink, 5f);
            panel.RootImage.raycastTarget = true;
            panel.FaceImage.raycastTarget = true;
            Button btn = panel.Root.gameObject.AddComponent<Button>();
            btn.targetGraphic = panel.FaceImage;
            ColorBlock cb = btn.colors;
            cb.normalColor = face;
            cb.highlightedColor = hover;
            cb.pressedColor = pressed;
            cb.selectedColor = face;
            cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
            Text txt = NewText(panel.Face, "Label", label, fontSize, Palette.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            Stretch(txt.rectTransform);
            if (onClick != null) btn.onClick.AddListener(onClick);
            return btn;
        }

        /// <summary>Boton de madera (secundario).</summary>
        public static Button WoodButton(Transform parent, string name, string label, int fontSize, UnityAction onClick)
        {
            return NewButton(parent, name, label, fontSize, Palette.Wood, Palette.WoodLight, Palette.WoodDark, onClick);
        }

        /// <summary>Boton verde (principal).</summary>
        public static Button LeafButton(Transform parent, string name, string label, int fontSize, UnityAction onClick)
        {
            return NewButton(parent, name, label, fontSize, Palette.Leaf, Palette.LeafLight, Palette.LeafDark, onClick);
        }

        public static Slider NewSlider(Transform parent, string name, float value, UnityAction<float> onChange)
        {
            RectTransform root = NewRect(name, parent);
            Slider slider = root.gameObject.AddComponent<Slider>();

            Image bg = NewImage(root, "Background", Palette.Ink, Rounded, true);
            bg.raycastTarget = true;
            RectTransform bgr = bg.rectTransform;
            bgr.anchorMin = new Vector2(0f, 0.5f);
            bgr.anchorMax = new Vector2(1f, 0.5f);
            bgr.sizeDelta = new Vector2(0f, 22f);
            bgr.anchoredPosition = Vector2.zero;

            RectTransform fillArea = NewRect("Fill Area", root);
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.sizeDelta = new Vector2(-24f, 12f);
            fillArea.anchoredPosition = Vector2.zero;

            Image fill = NewImage(fillArea, "Fill", Palette.Orange, Rounded, true);
            RectTransform fr = fill.rectTransform;
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = new Vector2(0f, 1f);
            fr.sizeDelta = Vector2.zero;

            RectTransform handleArea = NewRect("Handle Slide Area", root);
            Stretch(handleArea);
            handleArea.offsetMin = new Vector2(16f, 0f);
            handleArea.offsetMax = new Vector2(-16f, 0f);

            Image handleBorder = NewImage(handleArea, "Handle", Palette.Ink, Circle, false);
            handleBorder.raycastTarget = true;
            RectTransform hr = handleBorder.rectTransform;
            hr.anchorMin = new Vector2(0f, 0.5f);
            hr.anchorMax = new Vector2(0f, 0.5f);
            hr.sizeDelta = new Vector2(42f, 42f);
            Image handleFace = NewImage(hr, "Face", Palette.Cream, Circle, false);
            RectTransform hf = handleFace.rectTransform;
            hf.anchorMin = Vector2.zero;
            hf.anchorMax = Vector2.one;
            hf.offsetMin = new Vector2(5f, 5f);
            hf.offsetMax = new Vector2(-5f, -5f);

            slider.fillRect = fr;
            slider.handleRect = hr;
            slider.targetGraphic = handleBorder;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(value);
            if (onChange != null) slider.onValueChanged.AddListener(onChange);
            return slider;
        }

        /// <summary>Crea el EventSystem con el modulo del nuevo Input System si no existe.</summary>
        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
            GameObject go = new GameObject("EventSystem", typeof(EventSystem));
            InputSystemUIInputModule module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }
}
