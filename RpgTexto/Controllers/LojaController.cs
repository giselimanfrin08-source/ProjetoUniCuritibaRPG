using RpgTexto.Models;
using RpgTexto.Models.Cenarios;
using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;
using RpgTexto.Views;

namespace RpgTexto.Controllers;

/// <summary>
/// Fluxo de compra e venda de itens na loja da cidade.
/// </summary>
public class LojaController
{
    private readonly Textos textos;
    private readonly ItemView view;

    public LojaController(Textos textos, ItemView view)
    {
        this.textos = textos;
        this.view = view;
    }

    public void Abrir(Loja loja, Grupo grupo)
    {
        while (true)
        {
            view.Limpar();
            view.ExibirTitulo(textos.Obter("loja.titulo"));
            view.ExibirTexto(textos.Obter("loja.boas_vindas"));
            view.ExibirTexto(textos.Obter("status.dinheiro", grupo.DinheiroTotal));
            view.ExibirLinhaEmBranco();

            int escolha = view.Escolher(textos.Obter("loja.pergunta"), new[]
            {
                textos.Obter("loja.opcao.pocoes"),
                textos.Obter("loja.opcao.armas"),
                textos.Obter("loja.opcao.armaduras"),
                textos.Obter("loja.opcao.vender"),
                textos.Obter("ui.voltar")
            });

            switch (escolha)
            {
                case 0:
                    Comprar(loja, grupo, CategoriaItem.Pocao);
                    break;
                case 1:
                    Comprar(loja, grupo, CategoriaItem.Arma);
                    break;
                case 2:
                    Comprar(loja, grupo, CategoriaItem.Armadura);
                    break;
                case 3:
                    Vender(loja, grupo);
                    break;
                default:
                    return;
            }
        }
    }

    private void Comprar(Loja loja, Grupo grupo, CategoriaItem categoria)
    {
        IReadOnlyList<Item> itens = loja.ListarPorCategoria(categoria);
        while (true)
        {
            view.Limpar();
            view.ExibirTitulo(textos.Obter($"categoria.{categoria.ToString().ToLowerInvariant()}"));
            view.ExibirTexto(textos.Obter("status.dinheiro", grupo.DinheiroTotal));
            view.ExibirLinhaEmBranco();

            if (itens.Count == 0)
            {
                view.ExibirAviso(textos.Obter("loja.sem_itens"));
                view.Pausar();
                return;
            }

            int indice = view.EscolherItem(textos.Obter("loja.comprar_pergunta"), itens, i => i.Preco);
            if (indice < 0)
            {
                return;
            }

            Item item = itens[indice];
            view.ExibirTexto(item.Descricao);
            if (view.Confirmar(textos.Obter("loja.confirmar_compra", item.Nome, item.Preco)))
            {
                view.ExibirResultado(loja.Comprar(item, grupo, textos));
                view.Pausar();
            }
        }
    }

    private void Vender(Loja loja, Grupo grupo)
    {
        while (true)
        {
            view.Limpar();
            view.ExibirTitulo(textos.Obter("loja.vender_titulo"));
            view.ExibirTexto(textos.Obter("status.dinheiro", grupo.DinheiroTotal));
            view.ExibirLinhaEmBranco();

            IReadOnlyList<ItemAgrupado> itens = grupo.Inventario.Agrupar();
            if (itens.Count == 0)
            {
                view.ExibirAviso(textos.Obter("inventario.vazio"));
                view.Pausar();
                return;
            }

            int indice = view.EscolherItemAgrupado(textos.Obter("loja.vender_pergunta"), itens, exibirPrecoVenda: true);
            if (indice < 0)
            {
                return;
            }

            view.ExibirResultado(loja.Vender(itens[indice].Item, grupo, textos));
            view.Pausar();
        }
    }
}
