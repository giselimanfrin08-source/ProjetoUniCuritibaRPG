namespace RpgTexto.Models.Personagens;

/// <summary>
/// Inimigo lido do arquivo monstros.txt. Os monstros do catálogo servem de modelo
/// e cada batalha usa cópias deles.
/// </summary>
public class Monstro : Combatente
{
    private const int ChanceAtacarMaisFraco = 30;
    private readonly HashSet<string> areas;

    public Monstro(
        string id,
        string nome,
        int vida,
        int ataque,
        int defesa,
        int velocidade,
        int xpRecompensa,
        int ouroRecompensa,
        IEnumerable<string> areas)
        : base(nome, vida, ataque, defesa, velocidade)
    {
        Id = id;
        XpRecompensa = Math.Max(0, xpRecompensa);
        OuroRecompensa = Math.Max(0, ouroRecompensa);
        this.areas = new HashSet<string>(areas, StringComparer.OrdinalIgnoreCase);
    }

    public string Id { get; }

    public int XpRecompensa { get; }

    public int OuroRecompensa { get; }

    /// <summary>Ids dos campos e cavernas onde o monstro aparece.</summary>
    public IReadOnlyCollection<string> Areas => areas;

    public bool ApareceEm(string areaId) => areas.Contains(areaId);

    /// <summary>Cria uma cópia com a vida cheia. O sufixo diferencia monstros iguais (Lobo A, Lobo B).</summary>
    public Monstro Clonar(string? sufixo = null)
    {
        string nome = sufixo is null ? Nome : $"{Nome} {sufixo}";
        return new Monstro(Id, nome, VidaMaxima, AtaqueBase, DefesaBase, VelocidadeBase, XpRecompensa, OuroRecompensa, areas);
    }

    /// <summary>Escolhe quem atacar: às vezes o personagem mais ferido, senão um aleatório.</summary>
    public Personagem? EscolherAlvo(IReadOnlyList<Personagem> grupo, Random rng)
    {
        var vivos = grupo.Where(p => p.EstaVivo).ToList();
        if (vivos.Count == 0)
        {
            return null;
        }

        return rng.Next(100) < ChanceAtacarMaisFraco
            ? vivos.MinBy(p => p.Vida)
            : vivos[rng.Next(vivos.Count)];
    }
}
