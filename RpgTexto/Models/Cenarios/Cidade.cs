namespace RpgTexto.Models.Cenarios;

/// <summary>
/// Cidade com INN, loja e taverna. É o único lugar seguro do mapa.
/// </summary>
public class Cidade : Cenario
{
    public Cidade(string id, string nome, string descricao, IEnumerable<string> conexoes, Inn inn, Loja loja, Taverna taverna)
        : base(id, nome, descricao, conexoes)
    {
        Inn = inn;
        Loja = loja;
        Taverna = taverna;
    }

    public Inn Inn { get; }

    public Loja Loja { get; }

    public Taverna Taverna { get; }

    public override IReadOnlyList<OpcaoCenario> ObterOpcoes(Mundo mundo, Textos textos)
    {
        var opcoes = new List<OpcaoCenario>
        {
            new(textos.Obter("cidade.opcao.taverna"), TipoAcaoCenario.Taverna),
            new(textos.Obter("cidade.opcao.inn", Inn.CustoDescanso), TipoAcaoCenario.Inn),
            new(textos.Obter("cidade.opcao.loja"), TipoAcaoCenario.Loja)
        };
        opcoes.AddRange(CriarOpcoesDeViagem(mundo, textos, "cidade.opcao.viajar"));
        return opcoes;
    }
}
