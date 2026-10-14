using RpgTexto.Models.Sistema;

namespace RpgTexto.Models.Cenarios;

/// <summary>
/// Taverna da cidade: oferece quests e recebe as quests concluídas.
/// </summary>
public class Taverna
{
    private readonly List<Quest> quests;

    public Taverna(IEnumerable<Quest> quests)
    {
        this.quests = quests.ToList();
    }

    public IReadOnlyList<Quest> Quests => quests;

    public IReadOnlyList<Quest> QuestsDisponiveis => quests.Where(q => q.Status == StatusQuest.Disponivel).ToList();

    public IReadOnlyList<Quest> QuestsParaEntregar => quests.Where(q => q.Status == StatusQuest.Concluida).ToList();
}
