namespace RpgTexto.Models.Personagens;

/// <summary>Especiais escalam com a velocidade e têm 20% de chance de acerto crítico.</summary>
public class Arqueiro : Personagem
{
    private const int ChanceCritico = 20;

    public Arqueiro(string nome, DadosClasse dados) : base(nome, dados)
    {
    }

    protected override int CalcularPoderEspecial(Especial especial, Random rng)
    {
        int poder = especial.Poder + VelocidadeTotal / 2;
        return rng.Next(100) < ChanceCritico ? poder * 3 / 2 : poder;
    }

    protected override void SubirNivel() => AumentarAtributos(vida: 8, mana: 4, ataque: 2, defesa: 1, velocidade: 2);
}
