using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Itens;

public class Arma : Equipamento
{
    public Arma(string id, string nome, int preco, int bonusAtaque, IEnumerable<string> classesPermitidas, string descricao)
        : base(id, nome, preco, classesPermitidas, descricao)
    {
        BonusAtaque = bonusAtaque;
    }

    public int BonusAtaque { get; }

    public override CategoriaItem Categoria => CategoriaItem.Arma;

    public override string DescreverBonus(Textos textos) => textos.Obter("bonus.ataque", BonusAtaque);

    protected override Equipamento? EquiparEm(Personagem alvo) => alvo.EquiparArma(this);
}
