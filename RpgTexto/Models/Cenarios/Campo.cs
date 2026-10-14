using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Cenarios;

/// <summary>
/// Campo aberto: local de transição entre cidades. Pode haver emboscada ao chegar.
/// </summary>
public class Campo : AreaHostil
{
    public Campo(
        string id,
        string nome,
        string descricao,
        IEnumerable<string> conexoes,
        IEnumerable<Monstro> monstrosPossiveis,
        int chanceEncontro,
        int minimoInimigos,
        int maximoInimigos)
        : base(id, nome, descricao, conexoes, monstrosPossiveis, chanceEncontro, minimoInimigos, maximoInimigos)
    {
    }

    public override bool GeraEncontroAoEntrar => true;

    public override IReadOnlyList<OpcaoCenario> ObterOpcoes(Mundo mundo, Textos textos)
    {
        var opcoes = new List<OpcaoCenario>
        {
            new(textos.Obter("campo.opcao.explorar"), TipoAcaoCenario.Explorar)
        };
        opcoes.AddRange(CriarOpcoesDeViagem(mundo, textos, "campo.opcao.seguir"));
        return opcoes;
    }
}
