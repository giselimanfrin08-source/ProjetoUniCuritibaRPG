using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;
using RpgTexto.Models.Sistema;

namespace RpgTexto.Models.Cenarios;

/// <summary>
/// Mapa do jogo: 3 cidades, 5 campos abertos e 2 cavernas.
/// Nomes e descrições vêm do textos.txt; monstros e itens vêm dos seus arquivos.
/// </summary>
public class Mundo
{
    public const string CenarioInicialId = "pedra_clara";

    private readonly Dictionary<string, Cenario> cenarios = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<Cenario> Cenarios => cenarios.Values;

    public bool Existe(string id) => cenarios.ContainsKey(id);

    public Cenario Obter(string id)
    {
        return cenarios.TryGetValue(id, out Cenario? cenario)
            ? cenario
            : throw new KeyNotFoundException($"O cenário '{id}' não existe.");
    }

    public void Adicionar(Cenario cenario) => cenarios[cenario.Id] = cenario;

    public static Mundo Criar(Catalogo catalogo, IReadOnlyList<Quest> quests)
    {
        var mundo = new Mundo();
        Textos textos = catalogo.Textos;

        string Nome(string id) => textos.Obter($"cenario.{id}.nome");
        string Descricao(string id) => textos.Obter($"cenario.{id}.descricao");
        IEnumerable<Monstro> MonstrosDe(string id) => catalogo.Monstros.Where(m => m.ApareceEm(id));
        IEnumerable<Item> Itens(params string[] ids) => ids.Select(catalogo.ObterItem).OfType<Item>();

        Cidade CriarCidade(string id, int custoInn, int precoMaximoLoja, params string[] conexoes) =>
            new(id, Nome(id), Descricao(id), conexoes,
                new Inn(custoInn),
                new Loja(catalogo.Itens.Where(i => i.Preco <= precoMaximoLoja)),
                new Taverna(quests.Where(q => q.TavernaId == id)));

        Campo CriarCampo(string id, int chance, int minimo, int maximo, params string[] conexoes) =>
            new(id, Nome(id), Descricao(id), conexoes, MonstrosDe(id), chance, minimo, maximo);

        Caverna CriarCaverna(string id, IEnumerable<Item> tesouros, params string[] conexoes) =>
            new(id, Nome(id), Descricao(id), conexoes, MonstrosDe(id), 50, 2, 3, tesouros);

        // Cidades: id, custo do INN, preço máximo dos itens da loja, conexões
        mundo.Adicionar(CriarCidade("pedra_clara", 30, 120, "campo_dourado", "bosque_sussurrante"));
        mundo.Adicionar(CriarCidade("porto_lume", 50, 300, "campo_dourado", "pantano_cinzento", "costa_naufragios"));
        mundo.Adicionar(CriarCidade("ventor", 80, 700, "bosque_sussurrante", "pantano_cinzento", "planalto_cinzas"));

        // Campos abertos: id, chance de emboscada, mínimo e máximo de inimigos, conexões
        mundo.Adicionar(CriarCampo("campo_dourado", 50, 1, 2, "pedra_clara", "porto_lume"));
        mundo.Adicionar(CriarCampo("bosque_sussurrante", 55, 1, 3, "pedra_clara", "ventor", "mina_abandonada"));
        mundo.Adicionar(CriarCampo("pantano_cinzento", 60, 1, 3, "porto_lume", "ventor"));
        mundo.Adicionar(CriarCampo("costa_naufragios", 55, 1, 3, "porto_lume", "gruta_mares"));
        mundo.Adicionar(CriarCampo("planalto_cinzas", 65, 1, 3, "ventor", "mina_abandonada"));

        // Cavernas: id, tesouros, conexões
        mundo.Adicionar(CriarCaverna("mina_abandonada",
            Itens("cota_ana", "pocao_vida_grande", "grimorio_antigo", "elixir"),
            "bosque_sussurrante", "planalto_cinzas"));
        mundo.Adicionar(CriarCaverna("gruta_mares",
            Itens("tridente_mares", "pocao_mana_grande", "arco_elfico", "elixir"),
            "costa_naufragios"));

        return mundo;
    }
}
