namespace RpgTexto.Models.Personagens;

/// <summary>
/// Base abstrata para tudo que participa de uma batalha: personagens do grupo e monstros.
/// </summary>
public abstract class Combatente
{
    private int vida;
    private int vidaMaxima;

    protected Combatente(string nome, int vidaMaxima, int ataque, int defesa, int velocidade)
    {
        Nome = nome;
        VidaMaxima = vidaMaxima;
        Vida = vidaMaxima;
        AtaqueBase = ataque;
        DefesaBase = defesa;
        VelocidadeBase = velocidade;
    }

    public string Nome { get; protected set; }

    public int VidaMaxima
    {
        get => vidaMaxima;
        protected set => vidaMaxima = Math.Max(1, value);
    }

    /// <summary>A vida nunca fica abaixo de 0 nem acima da vida máxima.</summary>
    public int Vida
    {
        get => vida;
        protected set => vida = Math.Clamp(value, 0, VidaMaxima);
    }

    public int AtaqueBase { get; protected set; }

    public int DefesaBase { get; protected set; }

    public int VelocidadeBase { get; protected set; }

    public virtual int AtaqueTotal => AtaqueBase;

    public virtual int DefesaTotal => DefesaBase;

    public virtual int VelocidadeTotal => VelocidadeBase;

    public bool Defendendo { get; private set; }

    public bool EstaVivo => Vida > 0;

    /// <summary>Chamado no início do turno: a defesa do turno anterior deixa de valer.</summary>
    public void IniciarTurno() => Defendendo = false;

    public void Defender() => Defendendo = true;

    /// <summary>Aplica o dano (reduzido pela metade se estiver defendendo) e retorna o dano causado.</summary>
    public int ReceberDano(int valor)
    {
        int dano = Math.Max(1, valor);
        if (Defendendo)
        {
            dano = Math.Max(1, dano / 2);
        }

        Vida -= dano;
        return dano;
    }

    public virtual int CalcularDanoFisico(Combatente alvo, Random rng)
    {
        int variacao = rng.Next(0, AtaqueTotal / 4 + 1);
        return Math.Max(1, AtaqueTotal + variacao - alvo.DefesaTotal / 2);
    }

    public int Atacar(Combatente alvo, Random rng) => alvo.ReceberDano(CalcularDanoFisico(alvo, rng));
}
