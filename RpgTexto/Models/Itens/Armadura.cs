using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Itens;

public class Armadura : Equipamento
{
    public Armadura(
        string id,
        string nome,
        int preco,
        int bonusDefesa,
        int bonusVelocidade,
        IEnumerable<string> classesPermitidas,
        string descricao)
        : base(id, nome, preco, classesPermitidas, descricao)
    {
        BonusDefesa = bonusDefesa;
        BonusVelocidade = bonusVelocidade;
    }

    public int BonusDefesa { get; }

    /// <summary>Armaduras pesadas podem ter bônus negativo de velocidade.</summary>
    public int BonusVelocidade { get; }

    public override CategoriaItem Categoria => CategoriaItem.Armadura;

    public override string DescreverBonus(Textos textos)
    {
        string bonus = textos.Obter("bonus.defesa", BonusDefesa);
        if (BonusVelocidade != 0)
        {
            bonus += ", " + textos.Obter("bonus.velocidade", BonusVelocidade);
        }

        return bonus;
    }

    protected override Equipamento? EquiparEm(Personagem alvo) => alvo.EquiparArmadura(this);
}
