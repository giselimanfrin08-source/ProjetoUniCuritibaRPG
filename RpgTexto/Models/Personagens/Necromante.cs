namespace RpgTexto.Models.Personagens;

/// <summary>Quanto mais ferido, mais poderosa fica a magia sombria.</summary>
public class Necromante : Personagem
{
    public Necromante(string nome, DadosClasse dados) : base(nome, dados)
    {
    }

    protected override int CalcularPoderEspecial(Especial especial, Random rng)
    {
        return especial.Poder + Nivel * 3 + (VidaMaxima - Vida) / 5;
    }

    protected override void SubirNivel() => AumentarAtributos(vida: 7, mana: 9, ataque: 1, defesa: 1, velocidade: 1);
}
