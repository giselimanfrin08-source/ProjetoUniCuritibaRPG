using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Cenarios;

/// <summary>
/// Loja da cidade: vende poções, armas e armaduras e compra itens do grupo pela metade do preço.
/// </summary>
public class Loja
{
    private readonly List<Item> estoque;

    public Loja(IEnumerable<Item> estoque)
    {
        this.estoque = estoque.OrderBy(i => i.Categoria).ThenBy(i => i.Preco).ToList();
    }

    public IReadOnlyList<Item> Estoque => estoque;

    public IReadOnlyList<Item> ListarPorCategoria(CategoriaItem categoria)
    {
        return estoque.Where(i => i.Categoria == categoria).ToList();
    }

    public ResultadoAcao Comprar(Item item, Grupo grupo, Textos textos)
    {
        if (!estoque.Contains(item))
        {
            return ResultadoAcao.Falha(textos.Obter("loja.item_indisponivel", item.Nome));
        }

        if (!grupo.Gastar(item.Preco))
        {
            return ResultadoAcao.Falha(textos.Obter("loja.sem_dinheiro", item.Nome, item.Preco, grupo.DinheiroTotal));
        }

        grupo.Inventario.Adicionar(item);
        return ResultadoAcao.Ok(textos.Obter("loja.comprado", item.Nome, item.Preco));
    }

    public ResultadoAcao Vender(Item item, Grupo grupo, Textos textos)
    {
        if (!grupo.Inventario.Remover(item))
        {
            return ResultadoAcao.Falha(textos.Obter("item.nao_encontrado", item.Nome));
        }

        grupo.ReceberDinheiro(item.PrecoVenda);
        return ResultadoAcao.Ok(textos.Obter("loja.vendido", item.Nome, item.PrecoVenda));
    }
}
