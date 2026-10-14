namespace RpgTexto.Models.Personagens;

/// <summary>Golpes e curas sagradas escalam com a defesa.</summary>
public class Paladino : Personagem
{
    public Paladino(string nome, DadosClasse dados) : base(nome, dados)
    {
    }

    protected override int CalcularPoderEspecial(Especial especial, Random rng)
    {
        return especial.Poder + DefesaTotal / 2;
    }

    protected override void SubirNivel() => AumentarAtributos(vida: 10, mana: 5, ataque: 2, defesa: 2, velocidade: 0);
}
