using RpgTexto.Models;
using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;
using RpgTexto.Views;

namespace RpgTexto.Controllers;

/// <summary>
/// Inventário fora de batalha: usar poções e equipar armas e armaduras.
/// </summary>
public class InventarioController
{
    private readonly Textos textos;
    private readonly ItemView view;

    public InventarioController(Textos textos, ItemView view)
    {
        this.textos = textos;
        this.view = view;
    }

    public void Abrir(Grupo grupo)
    {
        while (true)
        {
            view.Limpar();
            view.ExibirTitulo(textos.Obter("inventario.titulo"));
            view.ExibirEquipamentos(grupo);
            view.ExibirLinhaEmBranco();

            IReadOnlyList<ItemAgrupado> itens = grupo.Inventario.Agrupar();
            if (itens.Count == 0)
            {
                view.ExibirAviso(textos.Obter("inventario.vazio"));
                view.Pausar();
                return;
            }

            int indiceItem = view.EscolherItemAgrupado(textos.Obter("inventario.pergunta"), itens, exibirPrecoVenda: false);
            if (indiceItem < 0)
            {
                return;
            }

            Item item = itens[indiceItem].Item;
            view.ExibirTexto(item.Descricao);
            int indicePersonagem = view.EscolherPersonagem(textos.Obter("inventario.quem", item.Nome), grupo.Membros);
            if (indicePersonagem < 0)
            {
                continue;
            }

            view.ExibirResultado(item.Usar(grupo.Membros[indicePersonagem], grupo.Inventario, textos));
            view.Pausar();
        }
    }
}
