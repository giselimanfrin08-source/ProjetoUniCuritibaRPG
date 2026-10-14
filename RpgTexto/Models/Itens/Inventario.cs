namespace RpgTexto.Models.Itens;

/// <summary>Um item do inventário com a quantidade de cópias iguais.</summary>
public sealed record ItemAgrupado(Item Item, int Quantidade);

/// <summary>
/// Inventário compartilhado pelo grupo: guarda poções e equipamentos.
/// </summary>
public class Inventario
{
    private readonly List<Item> itens = new();

    public IReadOnlyList<Item> Itens => itens;

    public int Quantidade => itens.Count;

    public void Adicionar(Item item) => itens.Add(item);

    public bool Remover(Item item)
    {
        int indice = itens.FindIndex(i => i.Id == item.Id);
        if (indice < 0)
        {
            return false;
        }

        itens.RemoveAt(indice);
        return true;
    }

    /// <summary>Agrupa itens iguais para exibição (ex.: "Poção de Vida x3").</summary>
    public IReadOnlyList<ItemAgrupado> Agrupar(Func<Item, bool>? filtro = null)
    {
        return itens
            .Where(filtro ?? (_ => true))
            .GroupBy(i => i.Id)
            .Select(g => new ItemAgrupado(g.First(), g.Count()))
            .OrderBy(g => g.Item.Categoria)
            .ThenBy(g => g.Item.Nome)
            .ToList();
    }

    public IReadOnlyList<ItemAgrupado> ListarPocoes() => Agrupar(i => i.UsavelEmBatalha);
}
