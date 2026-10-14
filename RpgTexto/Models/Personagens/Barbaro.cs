namespace RpgTexto.Models.Personagens;

/// <summary>Especiais escalam com o ataque e o dano físico aumenta quando está ferido (fúria).</summary>
public class Barbaro : Personagem
{
    public Barbaro(string nome, DadosClasse dados) : base(nome, dados)
    {
    }

    public override int CalcularDanoFisico(Combatente alvo, Random rng)
    {
        int dano = base.CalcularDanoFisico(alvo, rng);
        bool emFuria = Vida < VidaMaxima / 2;
        return emFuria ? dano * 5 / 4 : dano;
    }

    protected override int CalcularPoderEspecial(Especial especial, Random rng)
    {
        return especial.Poder + AtaqueTotal / 2;
    }

    protected override void SubirNivel() => AumentarAtributos(vida: 14, mana: 2, ataque: 3, defesa: 1, velocidade: 1);
}
