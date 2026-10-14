using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Cenarios;

/// <summary>
/// Base abstrata para locais com inimigos (campos e cavernas).
/// Os monstros possíveis vêm do monstros.txt, pelo id da área.
/// </summary>
public abstract class AreaHostil : Cenario
{
    private readonly List<Monstro> monstrosPossiveis;

    protected AreaHostil(
        string id,
        string nome,
        string descricao,
        IEnumerable<string> conexoes,
        IEnumerable<Monstro> monstrosPossiveis,
        int chanceEncontro,
        int minimoInimigos,
        int maximoInimigos)
        : base(id, nome, descricao, conexoes)
    {
        this.monstrosPossiveis = monstrosPossiveis.ToList();
        ChanceEncontro = Math.Clamp(chanceEncontro, 0, 100);
        MinimoInimigos = Math.Max(1, minimoInimigos);
        MaximoInimigos = Math.Max(MinimoInimigos, maximoInimigos);
    }

    public IReadOnlyList<Monstro> MonstrosPossiveis => monstrosPossiveis;

    /// <summary>Chance (0 a 100) de um encontro acontecer.</summary>
    public int ChanceEncontro { get; }

    public int MinimoInimigos { get; }

    public int MaximoInimigos { get; }

    public bool TemMonstros => monstrosPossiveis.Count > 0;

    public bool SortearEncontro(Random rng) => TemMonstros && rng.Next(100) < ChanceEncontro;

    /// <summary>Sorteia um grupo de inimigos. Monstros repetidos recebem letras (Lobo A, Lobo B).</summary>
    public List<Monstro> GerarEncontro(Random rng)
    {
        var encontro = new List<Monstro>();
        if (!TemMonstros)
        {
            return encontro;
        }

        int quantidade = rng.Next(MinimoInimigos, MaximoInimigos + 1);
        var modelos = Enumerable.Range(0, quantidade)
            .Select(_ => monstrosPossiveis[rng.Next(monstrosPossiveis.Count)])
            .ToList();

        var repetidos = modelos.GroupBy(m => m.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();
        var contadores = new Dictionary<string, int>();

        foreach (Monstro modelo in modelos)
        {
            string? sufixo = null;
            if (repetidos.Contains(modelo.Id))
            {
                contadores.TryGetValue(modelo.Id, out int contador);
                contadores[modelo.Id] = contador + 1;
                sufixo = ((char)('A' + contador)).ToString();
            }

            encontro.Add(modelo.Clonar(sufixo));
        }

        return encontro;
    }
}
