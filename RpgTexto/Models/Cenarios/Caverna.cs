using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Cenarios;

/// <summary>
/// Caverna repleta de inimigos, com tesouros escondidos.
/// </summary>
public class Caverna : AreaHostil
{
    private readonly List<Item> tesouros;

    public Caverna(
        string id,
        string nome,
        string descricao,
        IEnumerable<string> conexoes,
        IEnumerable<Monstro> monstrosPossiveis,
        int chanceEncontro,
        int minimoInimigos,
        int maximoInimigos,
        IEnumerable<Item> tesouros)
        : base(id, nome, descricao, conexoes, monstrosPossiveis, chanceEncontro, minimoInimigos, maximoInimigos)
    {
        this.tesouros = tesouros.ToList();
    }

    public IReadOnlyList<Item> Tesouros => tesouros;

    public override IReadOnlyList<OpcaoCenario> ObterOpcoes(Mundo mundo, Textos textos)
    {
        var opcoes = new List<OpcaoCenario>
        {
            new(textos.Obter("caverna.opcao.explorar"), TipoAcaoCenario.Explorar),
            new(textos.Obter("caverna.opcao.tesouro"), TipoAcaoCenario.BuscarTesouro)
        };
        opcoes.AddRange(CriarOpcoesDeViagem(mundo, textos, "caverna.opcao.sair"));
        return opcoes;
    }

    public bool TemTesouroRestante(IReadOnlyCollection<string> coletados)
    {
        return ProximoTesouro(coletados, out _) is not null;
    }

    /// <summary>Retorna o próximo tesouro ainda não coletado e a chave usada para registrá-lo.</summary>
    public Item? ProximoTesouro(IReadOnlyCollection<string> coletados, out string chave)
    {
        for (int i = 0; i < tesouros.Count; i++)
        {
            chave = $"{Id}:{i}";
            if (!coletados.Contains(chave))
            {
                return tesouros[i];
            }
        }

        chave = string.Empty;
        return null;
    }
}
