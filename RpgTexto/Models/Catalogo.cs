using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;
using RpgTexto.Models.Sistema;

namespace RpgTexto.Models;

/// <summary>
/// Reúne todos os dados lidos dos arquivos de texto (itens, monstros, classes, quests e textos).
/// </summary>
public class Catalogo
{
    private readonly Dictionary<string, Item> itensPorId;
    private readonly Dictionary<string, DadosClasse> classesPorId;
    private readonly List<DefinicaoQuest> definicoesQuest;

    public Catalogo(
        Textos textos,
        IEnumerable<Item> itens,
        IEnumerable<Monstro> monstros,
        IEnumerable<DadosClasse> classes,
        IEnumerable<DefinicaoQuest> quests)
    {
        Textos = textos;
        Itens = itens.ToList();
        Monstros = monstros.ToList();
        Classes = classes.ToList();
        definicoesQuest = quests.ToList();
        itensPorId = Itens.ToDictionary(i => i.Id, StringComparer.OrdinalIgnoreCase);
        classesPorId = Classes.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
    }

    public Textos Textos { get; }

    public IReadOnlyList<Item> Itens { get; }

    public IReadOnlyList<Monstro> Monstros { get; }

    public IReadOnlyList<DadosClasse> Classes { get; }

    public Item? ObterItem(string? id)
    {
        return id is not null && itensPorId.TryGetValue(id, out Item? item) ? item : null;
    }

    public DadosClasse? ObterClasse(string id)
    {
        return classesPorId.TryGetValue(id, out DadosClasse? classe) ? classe : null;
    }

    public string NomeClasse(string id) => ObterClasse(id)?.Nome ?? id;

    /// <summary>
    /// Cria um novo conjunto de quests, todas no estado inicial (disponíveis).
    /// </summary>
    public List<Quest> CriarQuests()
    {
        return definicoesQuest
            .Select(d => new Quest(
                d,
                Textos.Obter($"quest.{d.Id}.titulo"),
                Textos.Obter($"quest.{d.Id}.descricao"),
                ObterItem(d.RecompensaItemId)))
            .ToList();
    }
}
