namespace RpgTexto.Models.Personagens;

/// <summary>Especiais ficam mais fortes com o nível e com o cajado equipado.</summary>
public class Mago : Personagem
{
    public Mago(string nome, DadosClasse dados) : base(nome, dados)
    {
    }

    protected override int CalcularPoderEspecial(Especial especial, Random rng)
    {
        return especial.Poder + Nivel * 3 + AtaqueTotal / 3;
    }

    protected override void SubirNivel() => AumentarAtributos(vida: 6, mana: 10, ataque: 1, defesa: 1, velocidade: 1);
}
