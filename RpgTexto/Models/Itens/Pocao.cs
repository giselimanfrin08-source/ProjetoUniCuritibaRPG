using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Itens;

public class Pocao : Item
{
    public Pocao(string id, string nome, int preco, int recuperaVida, int recuperaMana, string descricao)
        : base(id, nome, preco, descricao)
    {
        RecuperaVida = Math.Max(0, recuperaVida);
        RecuperaMana = Math.Max(0, recuperaMana);
    }

    public int RecuperaVida { get; }

    public int RecuperaMana { get; }

    public override CategoriaItem Categoria => CategoriaItem.Pocao;

    public override bool UsavelEmBatalha => true;

    public override string DescreverBonus(Textos textos)
    {
        var partes = new List<string>();
        if (RecuperaVida > 0)
        {
            partes.Add(textos.Obter("bonus.vida", RecuperaVida));
        }

        if (RecuperaMana > 0)
        {
            partes.Add(textos.Obter("bonus.mana", RecuperaMana));
        }

        return string.Join(", ", partes);
    }

    public override ResultadoAcao Usar(Personagem alvo, Inventario inventario, Textos textos)
    {
        if (!alvo.EstaVivo)
        {
            return ResultadoAcao.Falha(textos.Obter("item.alvo_derrotado", alvo.Nome));
        }

        bool fazEfeito = (RecuperaVida > 0 && alvo.Vida < alvo.VidaMaxima)
                         || (RecuperaMana > 0 && alvo.Mana < alvo.ManaMaxima);
        if (!fazEfeito)
        {
            return ResultadoAcao.Falha(textos.Obter("item.sem_efeito", alvo.Nome, Nome));
        }

        if (!inventario.Remover(this))
        {
            return ResultadoAcao.Falha(textos.Obter("item.nao_encontrado", Nome));
        }

        int vida = alvo.Curar(RecuperaVida);
        int mana = alvo.RecuperarMana(RecuperaMana);
        var efeitos = new List<string>();
        if (vida > 0)
        {
            efeitos.Add(textos.Obter("item.recuperou_vida", vida));
        }

        if (mana > 0)
        {
            efeitos.Add(textos.Obter("item.recuperou_mana", mana));
        }

        return ResultadoAcao.Ok(textos.Obter("item.pocao_usada", alvo.Nome, Nome, string.Join(", ", efeitos)));
    }
}
