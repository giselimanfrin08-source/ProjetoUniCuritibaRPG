using RpgTexto.Models;
using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;

namespace RpgTexto.Views;

/// <summary>
/// Exibe listas de itens (loja e inventário) e os equipamentos do grupo.
/// </summary>
public class ItemView : ConsoleView
{
    private readonly Func<string, string> nomeClasse;

    public ItemView(Textos textos, Func<string, string> nomeClasse) : base(textos)
    {
        this.nomeClasse = nomeClasse;
    }

    /// <summary>Lista itens com preço e retorna o índice escolhido, ou -1 para voltar.</summary>
    public int EscolherItem(string titulo, IReadOnlyList<Item> itens, Func<Item, int> preco)
    {
        var opcoes = itens
            .Select(i => DescreverItem(i, preco(i), quantidade: 1))
            .Append(textos.Obter("ui.voltar"))
            .ToList();
        int escolha = Escolher(titulo, opcoes);
        return escolha == itens.Count ? -1 : escolha;
    }

    /// <summary>Lista itens agrupados (com quantidade) e retorna o índice escolhido, ou -1 para voltar.</summary>
    public int EscolherItemAgrupado(string titulo, IReadOnlyList<ItemAgrupado> itens, bool exibirPrecoVenda)
    {
        var opcoes = itens
            .Select(g => DescreverItem(g.Item, exibirPrecoVenda ? g.Item.PrecoVenda : null, g.Quantidade))
            .Append(textos.Obter("ui.voltar"))
            .ToList();
        int escolha = Escolher(titulo, opcoes);
        return escolha == itens.Count ? -1 : escolha;
    }

    public void ExibirEquipamentos(Grupo grupo)
    {
        string nenhum = textos.Obter("status.nenhum");
        foreach (Personagem p in grupo.Membros)
        {
            ExibirTexto(textos.Obter("inventario.equipamento", p.Nome, p.Arma?.Nome ?? nenhum, p.Armadura?.Nome ?? nenhum));
        }
    }

    private string DescreverItem(Item item, int? preco, int quantidade)
    {
        var partes = new List<string>
        {
            quantidade > 1 ? textos.Obter("item.quantidade", item.Nome, quantidade) : item.Nome
        };

        if (preco.HasValue)
        {
            partes.Add(textos.Obter("item.preco", preco.Value));
        }

        string bonus = item.DescreverBonus(textos);
        if (bonus.Length > 0)
        {
            partes.Add(bonus);
        }

        if (item is Equipamento equipamento)
        {
            partes.Add(equipamento.ClassesPermitidas.Count == 0
                ? textos.Obter("item.todas_classes")
                : string.Join(", ", equipamento.ClassesPermitidas.Select(nomeClasse)));
        }

        return string.Join(" | ", partes);
    }
}
