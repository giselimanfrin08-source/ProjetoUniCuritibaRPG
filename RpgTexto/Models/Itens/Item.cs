using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Itens;

public enum CategoriaItem
{
    Pocao,
    Arma,
    Armadura
}

/// <summary>
/// Classe base abstrata de todos os itens do jogo.
/// </summary>
public abstract class Item
{
    protected Item(string id, string nome, int preco, string descricao)
    {
        Id = id;
        Nome = nome;
        Preco = Math.Max(0, preco);
        Descricao = descricao;
    }

    public string Id { get; }

    public string Nome { get; }

    public int Preco { get; }

    public int PrecoVenda => Preco / 2;

    public string Descricao { get; }

    public abstract CategoriaItem Categoria { get; }

    public virtual bool UsavelEmBatalha => false;

    /// <summary>Texto curto com o efeito do item (ex.: "+5 ATQ").</summary>
    public abstract string DescreverBonus(Textos textos);

    /// <summary>
    /// Usa o item no personagem. Cada tipo de item define o que "usar" significa:
    /// a poção recupera atributos e o equipamento é equipado.
    /// </summary>
    public abstract ResultadoAcao Usar(Personagem alvo, Inventario inventario, Textos textos);
}
