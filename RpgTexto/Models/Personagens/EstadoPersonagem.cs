namespace RpgTexto.Models.Personagens;

/// <summary>
/// Dados de um personagem gravados no save.
/// </summary>
public class EstadoPersonagem
{
    public string ClasseId { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public int Nivel { get; set; }

    public int Xp { get; set; }

    public int Vida { get; set; }

    public int VidaMaxima { get; set; }

    public int Mana { get; set; }

    public int ManaMaxima { get; set; }

    public int Ataque { get; set; }

    public int Defesa { get; set; }

    public int Velocidade { get; set; }

    public int Dinheiro { get; set; }

    public string? ArmaId { get; set; }

    public string? ArmaduraId { get; set; }
}
