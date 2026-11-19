namespace Tsakel.Game
{
    public class SpeciesInfo
    {
        public readonly string Id;
        public readonly string CommonName;
        public readonly string ScientificName;
        public readonly string Summary;
        public readonly string Curiosity;

        public SpeciesInfo(string id, string commonName, string scientificName, string summary, string curiosity)
        {
            Id = id;
            CommonName = commonName;
            ScientificName = scientificName;
            Summary = summary;
            Curiosity = curiosity;
        }
    }

    /// <summary>
    /// Catalogo de especies del album. TEXTOS PROVISIONALES: reemplazar por los definitivos.
    /// Para agregar un animal: nueva entrada aqui + componente AnimalSubject con el mismo id en la escena.
    /// </summary>
    public static class SpeciesCatalog
    {
        public static readonly SpeciesInfo[] All = new SpeciesInfo[]
        {
            new SpeciesInfo("jaguar", "Jaguar", "Panthera onca",
                "El jaguar es el felino más grande de América. Vive solo, es un gran nadador y recorre la selva en silencio, sobre todo al amanecer y al atardecer.",
                "Para los antiguos mayas era un animal sagrado, símbolo de fuerza y de la noche."),
            new SpeciesInfo("tapir", "Tapir centroamericano", "Tapirus bairdii",
                "Es el mamífero terrestre más grande de Centroamérica. Su nariz alargada funciona como una pequeña trompa para tomar hojas y frutos.",
                "Se le llama «jardinero de la selva» porque, al comer frutos, ayuda a esparcir semillas por todo el bosque."),
            new SpeciesInfo("guacamaya", "Guacamaya roja", "Ara macao",
                "Ave de colores intensos que vive en las copas de los árboles. Se alimenta de frutos, semillas y nueces, y se comunica con fuertes graznidos.",
                "Suele formar pareja para toda la vida y puede vivir más de 40 años."),
            new SpeciesInfo("lagartija", "Lagartija de la selva", "Especie por definir",
                "Pequeño reptil muy ágil que toma el sol sobre troncos y piedras y caza insectos. Se camufla muy bien entre las hojas.",
                "Muchas lagartijas pueden soltar la cola para escapar de un depredador, ¡y después les vuelve a crecer!"),
            new SpeciesInfo("quetzal", "Quetzal", "Pharomachrus mocinno",
                "Ave de plumaje verde brillante y pecho rojo que vive en los bosques húmedos de montaña. Se alimenta sobre todo de frutos y a veces de insectos.",
                "El macho luce plumas de la cola de hasta un metro de largo. Para los antiguos mayas era un ave sagrada."),
        };

        public static int Count { get { return All.Length; } }

        public static SpeciesInfo Get(string id)
        {
            for (int i = 0; i < All.Length; i++)
                if (All[i].Id == id) return All[i];
            return null;
        }
    }
}
