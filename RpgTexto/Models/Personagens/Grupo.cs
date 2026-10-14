using RpgTexto.Models.Itens;

namespace RpgTexto.Models.Personagens;

/// <summary>
/// Grupo de 4 personagens controlado pelo jogador, com inventário compartilhado.
/// </summary>
public class Grupo
{
    public const int TamanhoMaximo = 4;

    private readonly List<Personagem> membros = new();

    public Grupo()
    {
        Inventario = new Inventario();
    }

    public IReadOnlyList<Personagem> Membros => membros;

    public IReadOnlyList<Personagem> MembrosVivos => membros.Where(m => m.EstaVivo).ToList();

    public Inventario Inventario { get; }

    public bool EstaCompleto => membros.Count == TamanhoMaximo;

    public bool EstaDerrotado => membros.All(m => !m.EstaVivo);

    public int DinheiroTotal => membros.Sum(m => m.Dinheiro);

    public void AdicionarMembro(Personagem personagem)
    {
        if (EstaCompleto)
        {
            throw new InvalidOperationException($"O grupo já possui {TamanhoMaximo} personagens.");
        }

        membros.Add(personagem);
    }

    /// <summary>Paga um valor usando o dinheiro dos membros. Retorna false se não houver o suficiente.</summary>
    public bool Gastar(int valor)
    {
        if (valor < 0 || valor > DinheiroTotal)
        {
            return false;
        }

        int restante = valor;
        foreach (Personagem membro in membros)
        {
            restante -= membro.RetirarDinheiro(restante);
            if (restante == 0)
            {
                break;
            }
        }

        return true;
    }

    /// <summary>Divide o valor igualmente entre os membros.</summary>
    public void ReceberDinheiro(int valor)
    {
        if (valor <= 0 || membros.Count == 0)
        {
            return;
        }

        int parte = valor / membros.Count;
        int resto = valor % membros.Count;
        for (int i = 0; i < membros.Count; i++)
        {
            membros[i].ReceberDinheiro(parte + (i < resto ? 1 : 0));
        }
    }

    /// <summary>Cada membro vivo recebe a experiência. Retorna quem subiu de nível.</summary>
    public IReadOnlyList<EvolucaoPersonagem> DistribuirXp(int xp)
    {
        var evolucoes = new List<EvolucaoPersonagem>();
        foreach (Personagem membro in membros.Where(m => m.EstaVivo))
        {
            int niveis = membro.GanharXp(xp);
            if (niveis > 0)
            {
                evolucoes.Add(new EvolucaoPersonagem(membro, niveis));
            }
        }

        return evolucoes;
    }

    public void DescansarTodos()
    {
        foreach (Personagem membro in membros)
        {
            membro.RestaurarTudo();
        }
    }
}
