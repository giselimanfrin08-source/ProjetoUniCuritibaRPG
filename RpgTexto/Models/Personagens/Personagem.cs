using RpgTexto.Models.Itens;

namespace RpgTexto.Models.Personagens;

/// <summary>Personagem que subiu de nível ao ganhar experiência.</summary>
public sealed record EvolucaoPersonagem(Personagem Personagem, int NiveisGanhos);

/// <summary>
/// Base abstrata das classes jogáveis. Cada classe define como calcula
/// o poder dos especiais e como seus atributos crescem ao subir de nível.
/// </summary>
public abstract class Personagem : Combatente
{
    private readonly List<Especial> especiais;
    private int mana;
    private int manaMaxima;
    private int dinheiro;
    private int xp;

    protected Personagem(string nome, DadosClasse dados)
        : base(nome, dados.Vida, dados.Ataque, dados.Defesa, dados.Velocidade)
    {
        ClasseId = dados.Id;
        NomeClasse = dados.Nome;
        ManaMaxima = dados.Mana;
        Mana = ManaMaxima;
        Nivel = 1;
        especiais = new List<Especial>(dados.Especiais);
    }

    public string ClasseId { get; }

    public string NomeClasse { get; }

    public int Mana
    {
        get => mana;
        private set => mana = Math.Clamp(value, 0, ManaMaxima);
    }

    public int ManaMaxima
    {
        get => manaMaxima;
        private set => manaMaxima = Math.Max(0, value);
    }

    public int Dinheiro
    {
        get => dinheiro;
        private set => dinheiro = Math.Max(0, value);
    }

    public int Xp
    {
        get => xp;
        private set => xp = Math.Max(0, value);
    }

    public int Nivel { get; private set; }

    public int XpProximoNivel => Nivel * 100;

    public Arma? Arma { get; private set; }

    public Armadura? Armadura { get; private set; }

    public IReadOnlyList<Especial> Especiais => especiais;

    public override int AtaqueTotal => AtaqueBase + (Arma?.BonusAtaque ?? 0);

    public override int DefesaTotal => DefesaBase + (Armadura?.BonusDefesa ?? 0);

    public override int VelocidadeTotal => Math.Max(1, VelocidadeBase + (Armadura?.BonusVelocidade ?? 0));

    // ---------------------------------------------------------------- Métodos que cada classe implementa

    /// <summary>Calcula o poder final do especial de acordo com as características da classe.</summary>
    protected abstract int CalcularPoderEspecial(Especial especial, Random rng);

    /// <summary>Aumenta os atributos quando o personagem sobe de nível.</summary>
    protected abstract void SubirNivel();

    // ---------------------------------------------------------------- Equipamentos

    /// <summary>Equipa a arma e devolve a arma anterior (ou null).</summary>
    public Arma? EquiparArma(Arma arma)
    {
        Arma? anterior = Arma;
        Arma = arma;
        return anterior;
    }

    /// <summary>Equipa a armadura e devolve a armadura anterior (ou null).</summary>
    public Armadura? EquiparArmadura(Armadura armadura)
    {
        Armadura? anterior = Armadura;
        Armadura = armadura;
        return anterior;
    }

    // ---------------------------------------------------------------- Vida, mana e dinheiro

    /// <summary>Recupera vida de um personagem vivo e retorna quanto foi recuperado.</summary>
    public int Curar(int quantidade)
    {
        if (!EstaVivo || quantidade <= 0)
        {
            return 0;
        }

        int antes = Vida;
        Vida += quantidade;
        return Vida - antes;
    }

    public int RecuperarMana(int quantidade)
    {
        if (!EstaVivo || quantidade <= 0)
        {
            return 0;
        }

        int antes = Mana;
        Mana += quantidade;
        return Mana - antes;
    }

    /// <summary>Recupera toda a vida e mana, inclusive de personagens derrotados (usado no INN).</summary>
    public void RestaurarTudo()
    {
        Vida = VidaMaxima;
        Mana = ManaMaxima;
    }

    public void ReceberDinheiro(int valor)
    {
        if (valor > 0)
        {
            Dinheiro += valor;
        }
    }

    /// <summary>Retira até o valor pedido e retorna quanto foi retirado de fato.</summary>
    public int RetirarDinheiro(int valor)
    {
        int retirado = Math.Clamp(valor, 0, Dinheiro);
        Dinheiro -= retirado;
        return retirado;
    }

    // ---------------------------------------------------------------- Experiência

    /// <summary>Soma a experiência e retorna quantos níveis o personagem subiu.</summary>
    public int GanharXp(int valor)
    {
        if (valor <= 0)
        {
            return 0;
        }

        Xp += valor;
        int niveis = 0;
        while (Xp >= XpProximoNivel)
        {
            Xp -= XpProximoNivel;
            Nivel++;
            SubirNivel();
            niveis++;
        }

        return niveis;
    }

    protected void AumentarAtributos(int vida, int mana, int ataque, int defesa, int velocidade)
    {
        VidaMaxima += vida;
        ManaMaxima += mana;
        AtaqueBase += ataque;
        DefesaBase += defesa;
        VelocidadeBase += velocidade;
        Vida += vida;
        Mana += mana;
    }

    // ---------------------------------------------------------------- Especiais

    public bool PodeUsar(Especial especial) => Mana >= especial.CustoMana;

    public ResultadoAcao UsarEspecial(
        Especial especial,
        Combatente? alvo,
        IReadOnlyList<Combatente> inimigos,
        IReadOnlyList<Personagem> aliados,
        Random rng,
        Textos textos)
    {
        if (!especiais.Contains(especial))
        {
            return ResultadoAcao.Falha(textos.Obter("especial.desconhecido", Nome, especial.Nome));
        }

        if (!PodeUsar(especial))
        {
            return ResultadoAcao.Falha(textos.Obter("especial.sem_mana", Nome, especial.Nome));
        }

        bool alvoInvalido = especial.Alvo switch
        {
            AlvoEspecial.Inimigo => alvo is null || !alvo.EstaVivo,
            AlvoEspecial.Aliado => alvo is not Personagem || !alvo.EstaVivo,
            _ => false
        };
        if (alvoInvalido)
        {
            return ResultadoAcao.Falha(textos.Obter("especial.alvo_invalido"));
        }

        Mana -= especial.CustoMana;
        int poder = CalcularPoderEspecial(especial, rng);
        var linhas = new List<string> { textos.Obter("especial.usado", Nome, especial.Nome) };

        switch (especial.Tipo)
        {
            case TipoEspecial.DanoUnico:
                AplicarDanoEspecial(alvo!, poder, textos, linhas);
                break;

            case TipoEspecial.DanoTodos:
                foreach (Combatente inimigo in inimigos.Where(i => i.EstaVivo).ToList())
                {
                    AplicarDanoEspecial(inimigo, poder, textos, linhas);
                }

                break;

            case TipoEspecial.CuraAliado:
                Personagem aliado = (Personagem)alvo!;
                linhas.Add(textos.Obter("especial.cura", aliado.Nome, aliado.Curar(poder)));
                break;

            case TipoEspecial.CuraGrupo:
                foreach (Personagem membro in aliados.Where(a => a.EstaVivo))
                {
                    linhas.Add(textos.Obter("especial.cura", membro.Nome, membro.Curar(poder)));
                }

                break;

            case TipoEspecial.DrenarVida:
                int dano = AplicarDanoEspecial(alvo!, poder, textos, linhas);
                linhas.Add(textos.Obter("especial.dreno", Nome, Curar(dano / 2)));
                break;
        }

        return ResultadoAcao.Ok(string.Join(Environment.NewLine, linhas));
    }

    /// <summary>Aplica o dano do especial, registra as mensagens e retorna o dano causado.</summary>
    private static int AplicarDanoEspecial(Combatente alvo, int poder, Textos textos, List<string> linhas)
    {
        int dano = alvo.ReceberDano(Math.Max(1, poder - alvo.DefesaTotal / 3));
        linhas.Add(textos.Obter("especial.dano", alvo.Nome, dano));
        if (!alvo.EstaVivo)
        {
            linhas.Add(textos.Obter("batalha.derrotado", alvo.Nome));
        }

        return dano;
    }

    // ---------------------------------------------------------------- Save

    public EstadoPersonagem CriarEstado()
    {
        return new EstadoPersonagem
        {
            ClasseId = ClasseId,
            Nome = Nome,
            Nivel = Nivel,
            Xp = Xp,
            Vida = Vida,
            VidaMaxima = VidaMaxima,
            Mana = Mana,
            ManaMaxima = ManaMaxima,
            Ataque = AtaqueBase,
            Defesa = DefesaBase,
            Velocidade = VelocidadeBase,
            Dinheiro = Dinheiro,
            ArmaId = Arma?.Id,
            ArmaduraId = Armadura?.Id
        };
    }

    public void Restaurar(EstadoPersonagem estado, Arma? arma, Armadura? armadura)
    {
        Nivel = Math.Max(1, estado.Nivel);
        VidaMaxima = estado.VidaMaxima;
        ManaMaxima = estado.ManaMaxima;
        Vida = estado.Vida;
        Mana = estado.Mana;
        AtaqueBase = estado.Ataque;
        DefesaBase = estado.Defesa;
        VelocidadeBase = estado.Velocidade;
        Xp = estado.Xp;
        Dinheiro = estado.Dinheiro;
        Arma = arma;
        Armadura = armadura;
    }
}
