namespace RpgTexto.Models.Personagens;

public enum TipoEspecial
{
    DanoUnico,
    DanoTodos,
    CuraAliado,
    CuraGrupo,
    DrenarVida
}

public enum AlvoEspecial
{
    Inimigo,
    Aliado,
    Nenhum
}

/// <summary>
/// Golpe especial de uma classe, lido do arquivo classes.txt.
/// </summary>
public class Especial
{
    public Especial(string nome, int custoMana, int poder, TipoEspecial tipo, string descricao)
    {
        Nome = nome;
        CustoMana = Math.Max(0, custoMana);
        Poder = Math.Max(0, poder);
        Tipo = tipo;
        Descricao = descricao;
    }

    public string Nome { get; }

    public int CustoMana { get; }

    public int Poder { get; }

    public TipoEspecial Tipo { get; }

    public string Descricao { get; }

    /// <summary>Indica se o jogador precisa escolher um inimigo, um aliado ou nenhum alvo.</summary>
    public AlvoEspecial Alvo => Tipo switch
    {
        TipoEspecial.DanoUnico or TipoEspecial.DrenarVida => AlvoEspecial.Inimigo,
        TipoEspecial.CuraAliado => AlvoEspecial.Aliado,
        _ => AlvoEspecial.Nenhum
    };
}
