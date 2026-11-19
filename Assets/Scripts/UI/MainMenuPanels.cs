using UnityEngine;
using UnityEngine.UI;

namespace Tsakel.Game
{
    public partial class MainMenu
    {
        private void BuildSettings()
        {
            PanelParts panel;
            settingsRoot = NewModal("Ajustes", "Ajustes", 1200f, 760f, out panel);
            Vector2 tl = new Vector2(0f, 1f);
            string[] labels = new string[] { "Volumen maestro", "Volumen animales", "Volumen música", "Sensibilidad" };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                float y = -150f - i * 110f;
                Text l = UIKit.NewText(panel.Face, "Etiqueta" + i, labels[i], 42, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold, false);
                UIKit.Anchor(l.rectTransform, tl, tl, new Vector2(400f, 70f), new Vector2(80f, y));
                float value = GetSetting(i);
                Text pct = UIKit.NewText(panel.Face, "Valor" + i, Mathf.RoundToInt(value * 100f) + "%", 42, Palette.Wood, TextAnchor.MiddleRight, FontStyle.Bold, false);
                UIKit.Anchor(pct.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(150f, 70f), new Vector2(-80f, y));
                Slider s = UIKit.NewSlider(panel.Face, "Barra" + i, value, v =>
                {
                    SetSetting(idx, v);
                    pct.text = Mathf.RoundToInt(v * 100f) + "%";
                });
                UIKit.Anchor(s.GetComponent<RectTransform>(), tl, tl, new Vector2(500f, 60f), new Vector2(500f, y - 5f));
            }
            Button back = UIKit.LeafButton(panel.Face, "Volver", "Volver", 40, ShowMain);
            UIKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(320f, 88f), new Vector2(0f, 44f));
        }

        private static float GetSetting(int i)
        {
            switch (i)
            {
                case 0: return GameSettings.Master;
                case 1: return GameSettings.Animals;
                case 2: return GameSettings.Music;
                default: return GameSettings.Sensitivity;
            }
        }

        private static void SetSetting(int i, float v)
        {
            switch (i)
            {
                case 0: GameSettings.Master = v; break;
                case 1: GameSettings.Animals = v; break;
                case 2: GameSettings.Music = v; break;
                default: GameSettings.Sensitivity = v; break;
            }
        }

        private void BuildCredits()
        {
            PanelParts panel;
            creditsRoot = NewModal("Creditos", "CRÉDITOS", 1300f, 820f, out panel);
            Vector2 tl = new Vector2(0f, 1f);
            Text thanks = UIKit.NewText(panel.Face, "Gracias", "¡Gracias por jugar!", 48, Palette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UIKit.Anchor(thanks.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1000f, 70f), new Vector2(0f, -80f));
            Text team = UIKit.NewText(panel.Face, "Equipo", "XOLOMOTION", 44, Palette.LeafDark, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UIKit.Anchor(team.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1000f, 60f), new Vector2(0f, -150f));

            string[] names = new string[] {
                "María Fernanda Estrada Gutiérrez", "Yajcdiel Colorado Vargas", "Joaquín Miguel Palafox Ruíz",
                "Dulce Mariel Martinez Uriostegui", "Jaime Antonio Navarro Trujeque", "Irene Juarez Coutiño" };
            string[] roles = new string[] {
                "Director Creativo", "Diseñador Gráfico (Supervisor)", "Programador (Supervisor)",
                "Diseñador Audiovisual", "Programador", "Diseñador Gráfico" };
            for (int i = 0; i < names.Length; i++)
            {
                float y = -225f - i * 62f;
                Text n = UIKit.NewText(panel.Face, "Nombre" + i, names[i], 36, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold, false);
                UIKit.Anchor(n.rectTransform, tl, tl, new Vector2(640f, 56f), new Vector2(80f, y));
                UIKit.BestFit(n, 20, 36);
                Text r = UIKit.NewText(panel.Face, "Cargo" + i, roles[i], 32, Palette.Wood, TextAnchor.MiddleLeft, FontStyle.Bold, false);
                UIKit.Anchor(r.rectTransform, tl, tl, new Vector2(460f, 56f), new Vector2(740f, y));
                UIKit.BestFit(r, 18, 32);
            }
            Button back = UIKit.LeafButton(panel.Face, "Volver", "Volver", 40, ShowMain);
            UIKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(320f, 88f), new Vector2(0f, 40f));
        }
    }
}
