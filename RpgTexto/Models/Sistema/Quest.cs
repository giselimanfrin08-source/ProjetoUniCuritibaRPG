using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Sistema;

public enum TipoQuest
{
    /// <summary>Derrotar uma quantidade de um monstro específico.</summary>
    Derrotar,

    /// <summary>Derrotar uma quantidade de monstros quaisquer em um local.</summary>
    DerrotarEm,

    /// <summary>Visitar um local (ex.: outra cidade).</summary>
    Visitar
}

public enum StatusQuest
{
    Disponivel,
    Ativa,
    Concluida,
    Entregue
}

public enum TipoEvento
{
    MonstroDerrotado,
    CenarioVisitado
}

/// <summary>Dados fixos de uma quest, lidos do arquivo quests.txt.</summary>
public sealed record DefinicaoQuest(
    string Id,
    string TavernaId,
    TipoQuest Tipo,
    string Alvo,
    int Quantidade,
    int RecompensaXp,
    int RecompensaOuro,
    string? RecompensaItemId);

/// <summary>
/// Missão aceita na taverna. Avança com os eventos do jogo e é entregue
/// na taverna de origem para receber a recompensa.
/// </summary>
public class Quest
{
    private readonly DefinicaoQuest definicao;

    public Quest(DefinicaoQuest definicao, string titulo, string descricao, Item? recompensaItem)
    {
        this.definicao = definicao;
        Titulo = titulo;
        Descricao = descricao;
        RecompensaItem = recompensaItem;
        Status = StatusQuest.Disponivel;
    }

    public string Id => definicao.Id;

    public string TavernaId => definicao.TavernaId;

    public TipoQuest Tipo => definicao.Tipo;

    public string Alvo => definicao.Alvo;

    public int Quantidade => Math.Max(1, definicao.Quantidade);

    public int RecompensaXp => definicao.RecompensaXp;

    public int RecompensaOuro => definicao.RecompensaOuro;

    public Item? RecompensaItem { get; }

    public string Titulo { get; }

    public string Descricao { get; }

    public int Progresso { get; private set; }

    public StatusQuest Status { get; private set; }

    public bool Aceitar()
    {
        if (Status != StatusQuest.Disponivel)
        {
            return false;
        }

        Status = StatusQuest.Ativa;
        Progresso = 0;
        return true;
    }

    /// <summary>Verifica se o evento conta para a quest. Retorna true se houve progresso.</summary>
    public bool RegistrarEvento(TipoEvento evento, string alvoEvento, string cenarioId)
    {
        if (Status != StatusQuest.Ativa)
        {
            return false;
        }

        bool conta = Tipo switch
        {
            TipoQuest.Derrotar => evento == TipoEvento.MonstroDerrotado && Igual(alvoEvento, Alvo),
            TipoQuest.DerrotarEm => evento == TipoEvento.MonstroDerrotado && Igual(cenarioId, Alvo),
            TipoQuest.Visitar => evento == TipoEvento.CenarioVisitado && Igual(alvoEvento, Alvo),
            _ => false
        };

        if (!conta)
        {
            return false;
        }

        Progresso = Math.Min(Progresso + 1, Quantidade);
        if (Progresso >= Quantidade)
        {
            Status = StatusQuest.Concluida;
        }

        return true;
    }

    /// <summary>Entrega a recompensa ao grupo e retorna quem subiu de nível.</summary>
    public IReadOnlyList<EvolucaoPersonagem> Entregar(Grupo grupo)
    {
        if (Status != StatusQuest.Concluida)
        {
            return Array.Empty<EvolucaoPersonagem>();
        }

        Status = StatusQuest.Entregue;
        grupo.ReceberDinheiro(RecompensaOuro);
        if (RecompensaItem is not null)
        {
            grupo.Inventario.Adicionar(RecompensaItem);
        }

        return grupo.DistribuirXp(RecompensaXp);
    }

    public void Restaurar(StatusQuest status, int progresso)
    {
        Status = status;
        Progresso = Math.Clamp(progresso, 0, Quantidade);
    }

    private static bool Igual(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
