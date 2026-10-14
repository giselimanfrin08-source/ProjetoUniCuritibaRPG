using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Cenarios;

/// <summary>
/// Estalagem: recupera a vida e a mana do grupo por um custo. Também é onde o jogo é salvo.
/// </summary>
public class Inn
{
    public Inn(int custoDescanso)
    {
        CustoDescanso = Math.Max(0, custoDescanso);
    }

    public int CustoDescanso { get; }

    public ResultadoAcao Descansar(Grupo grupo, Textos textos)
    {
        if (!grupo.Gastar(CustoDescanso))
        {
            return ResultadoAcao.Falha(textos.Obter("inn.sem_dinheiro", CustoDescanso, grupo.DinheiroTotal));
        }

        grupo.DescansarTodos();
        return ResultadoAcao.Ok(textos.Obter("inn.descansou", CustoDescanso));
    }
}
