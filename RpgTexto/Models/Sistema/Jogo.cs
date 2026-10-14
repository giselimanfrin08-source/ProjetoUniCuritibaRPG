using RpgTexto.Models.Cenarios;
using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Sistema;

/// <summary>
/// Estado de uma partida: grupo, mundo, local atual, quests e tesouros já coletados.
/// </summary>
public class Jogo
{
    public const int DinheiroInicialPorPersonagem = 50;

    private readonly List<Quest> quests;
    private readonly HashSet<string> tesourosColetados;

    public Jogo(
        Grupo grupo,
        Mundo mundo,
        IEnumerable<Quest> quests,
        string cenarioAtualId,
        IEnumerable<string>? tesourosColetados = null)
    {
        Grupo = grupo;
        Mundo = mundo;
        this.quests = quests.ToList();
        this.tesourosColetados = new HashSet<string>(tesourosColetados ?? Enumerable.Empty<string>());
        CenarioAtual = mundo.Obter(cenarioAtualId);
    }

    public Grupo Grupo { get; }

    public Mundo Mundo { get; }

    public Cenario CenarioAtual { get; private set; }

    public IReadOnlyList<Quest> Quests => quests;

    public IReadOnlyCollection<string> TesourosColetados => tesourosColetados;

    public static Jogo CriarNovo(Catalogo catalogo, Grupo grupo)
    {
        List<Quest> quests = catalogo.CriarQuests();
        Mundo mundo = Mundo.Criar(catalogo, quests);
        return new Jogo(grupo, mundo, quests, Mundo.CenarioInicialId);
    }

    /// <summary>Muda o local atual e retorna as quests que avançaram com a visita.</summary>
    public IReadOnlyList<Quest> MoverPara(string cenarioId)
    {
        CenarioAtual = Mundo.Obter(cenarioId);
        return RegistrarEvento(TipoEvento.CenarioVisitado, cenarioId);
    }

    /// <summary>Repassa o evento para as quests e retorna as que avançaram.</summary>
    public IReadOnlyList<Quest> RegistrarEvento(TipoEvento evento, string alvo)
    {
        return quests.Where(q => q.RegistrarEvento(evento, alvo, CenarioAtual.Id)).ToList();
    }

    /// <summary>Pega o próximo tesouro da caverna e coloca no inventário. Retorna null se não houver mais.</summary>
    public Item? ColetarTesouro(Caverna caverna)
    {
        Item? item = caverna.ProximoTesouro(tesourosColetados, out string chave);
        if (item is null)
        {
            return null;
        }

        tesourosColetados.Add(chave);
        Grupo.Inventario.Adicionar(item);
        return item;
    }
}
