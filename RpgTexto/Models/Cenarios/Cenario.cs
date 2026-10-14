namespace RpgTexto.Models.Cenarios;

public enum TipoAcaoCenario
{
    Taverna,
    Inn,
    Loja,
    Viajar,
    Explorar,
    BuscarTesouro
}

/// <summary>Opção exibida no menu de um cenário.</summary>
public sealed record OpcaoCenario(string Texto, TipoAcaoCenario Acao, string? Destino = null);

/// <summary>
/// Base abstrata de todos os locais do mundo (cidades, campos e cavernas).
/// Cada tipo de cenário define as opções disponíveis para o jogador.
/// </summary>
public abstract class Cenario
{
    private readonly List<string> conexoes;

    protected Cenario(string id, string nome, string descricao, IEnumerable<string> conexoes)
    {
        Id = id;
        Nome = nome;
        Descricao = descricao;
        this.conexoes = conexoes.ToList();
    }

    public string Id { get; }

    public string Nome { get; }

    public string Descricao { get; }

    /// <summary>Ids dos cenários para onde é possível viajar a partir daqui.</summary>
    public IReadOnlyList<string> Conexoes => conexoes;

    /// <summary>Indica se pode ocorrer uma batalha ao chegar neste cenário.</summary>
    public virtual bool GeraEncontroAoEntrar => false;

    public abstract IReadOnlyList<OpcaoCenario> ObterOpcoes(Mundo mundo, Textos textos);

    protected IEnumerable<OpcaoCenario> CriarOpcoesDeViagem(Mundo mundo, Textos textos, string chaveTexto)
    {
        return conexoes
            .Where(mundo.Existe)
            .Select(id => new OpcaoCenario(textos.Obter(chaveTexto, mundo.Obter(id).Nome), TipoAcaoCenario.Viajar, id));
    }
}
