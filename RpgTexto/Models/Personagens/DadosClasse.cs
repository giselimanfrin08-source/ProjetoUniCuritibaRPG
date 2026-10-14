namespace RpgTexto.Models.Personagens;

/// <summary>
/// Atributos iniciais e golpes especiais de uma classe, lidos do arquivo classes.txt.
/// </summary>
public class DadosClasse
{
    private readonly List<Especial> especiais = new();

    public DadosClasse(string id, string nome, int vida, int mana, int ataque, int defesa, int velocidade, string descricao)
    {
        Id = id;
        Nome = nome;
        Vida = vida;
        Mana = mana;
        Ataque = ataque;
        Defesa = defesa;
        Velocidade = velocidade;
        Descricao = descricao;
    }

    public string Id { get; }

    public string Nome { get; }

    public int Vida { get; }

    public int Mana { get; }

    public int Ataque { get; }

    public int Defesa { get; }

    public int Velocidade { get; }

    public string Descricao { get; }

    public IReadOnlyList<Especial> Especiais => especiais;

    public void AdicionarEspecial(Especial especial) => especiais.Add(especial);
}
