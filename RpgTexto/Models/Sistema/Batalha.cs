using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Sistema;

public enum AcaoBatalha
{
    Atacar,
    Defender,
    Especial,
    UsarItem,
    Fugir
}

public enum ResultadoBatalha
{
    Vitoria,
    Derrota,
    Fuga
}

/// <summary>
/// Regras da batalha por turnos. A ordem de cada rodada é definida pela velocidade
/// e todos os personagens vivos do grupo participam.
/// </summary>
public class Batalha
{
    private const int ChanceFugaBase = 50;

    private readonly List<Monstro> inimigos;
    private readonly Queue<Combatente> ordemTurnos = new();
    private readonly Random rng;
    private readonly Textos textos;

    public Batalha(Grupo grupo, IEnumerable<Monstro> inimigos, Random rng, Textos textos)
    {
        Grupo = grupo;
        this.inimigos = inimigos.ToList();
        this.rng = rng;
        this.textos = textos;
    }

    public Grupo Grupo { get; }

    public IReadOnlyList<Monstro> Inimigos => inimigos;

    public IReadOnlyList<Monstro> InimigosVivos => inimigos.Where(i => i.EstaVivo).ToList();

    public int Rodada { get; private set; }

    public bool Fugiu { get; private set; }

    public bool InimigosDerrotados => inimigos.All(i => !i.EstaVivo);

    public bool Terminou => Fugiu || InimigosDerrotados || Grupo.EstaDerrotado;

    public bool GrupoVenceu => !Fugiu && InimigosDerrotados && !Grupo.EstaDerrotado;

    public int XpTotal => inimigos.Sum(i => i.XpRecompensa);

    public int OuroTotal => inimigos.Sum(i => i.OuroRecompensa);

    /// <summary>Retorna quem age agora, ou null se a batalha terminou.</summary>
    public Combatente? ProximoCombatente()
    {
        while (!Terminou)
        {
            if (ordemTurnos.Count == 0)
            {
                IniciarRodada();
            }

            Combatente atual = ordemTurnos.Dequeue();
            if (atual.EstaVivo)
            {
                atual.IniciarTurno();
                return atual;
            }
        }

        return null;
    }

    public ResultadoAcao Atacar(Combatente atacante, Combatente alvo)
    {
        if (!alvo.EstaVivo)
        {
            return ResultadoAcao.Falha(textos.Obter("batalha.alvo_invalido"));
        }

        int dano = atacante.Atacar(alvo, rng);
        string mensagem = textos.Obter("batalha.ataque", atacante.Nome, alvo.Nome, dano);
        if (!alvo.EstaVivo)
        {
            mensagem += Environment.NewLine + textos.Obter("batalha.derrotado", alvo.Nome);
        }

        return ResultadoAcao.Ok(mensagem);
    }

    public ResultadoAcao Defender(Combatente combatente)
    {
        combatente.Defender();
        return ResultadoAcao.Ok(textos.Obter("batalha.defesa", combatente.Nome));
    }

    public ResultadoAcao UsarEspecial(Personagem personagem, Especial especial, Combatente? alvo)
    {
        return personagem.UsarEspecial(especial, alvo, InimigosVivos, Grupo.Membros, rng, textos);
    }

    public ResultadoAcao UsarItem(Personagem usuario, Item item, Personagem alvo)
    {
        if (!item.UsavelEmBatalha)
        {
            return ResultadoAcao.Falha(textos.Obter("batalha.item_invalido", item.Nome));
        }

        ResultadoAcao resultado = item.Usar(alvo, Grupo.Inventario, textos);
        return resultado.Sucesso
            ? ResultadoAcao.Ok(textos.Obter("batalha.item_usado", usuario.Nome) + Environment.NewLine + resultado.Mensagem)
            : resultado;
    }

    /// <summary>
    /// Tenta fugir. A chance depende da velocidade do grupo comparada à dos inimigos.
    /// Mesmo quando falha, a ação gasta o turno.
    /// </summary>
    public ResultadoAcao TentarFugir()
    {
        double velocidadeGrupo = Grupo.MembrosVivos.Average(m => m.VelocidadeTotal);
        double velocidadeInimigos = InimigosVivos.Average(i => i.VelocidadeTotal);
        int chance = Math.Clamp(ChanceFugaBase + (int)Math.Round((velocidadeGrupo - velocidadeInimigos) * 3), 10, 90);

        if (rng.Next(100) < chance)
        {
            Fugiu = true;
            return ResultadoAcao.Ok(textos.Obter("batalha.fuga_sucesso"));
        }

        return ResultadoAcao.Ok(textos.Obter("batalha.fuga_falhou"));
    }

    public ResultadoAcao ExecutarTurnoMonstro(Monstro monstro)
    {
        Personagem? alvo = monstro.EscolherAlvo(Grupo.Membros, rng);
        return alvo is null ? ResultadoAcao.Falha(string.Empty) : Atacar(monstro, alvo);
    }

    private void IniciarRodada()
    {
        Rodada++;
        IEnumerable<Combatente> ordem = Grupo.Membros
            .Cast<Combatente>()
            .Concat(inimigos)
            .Where(c => c.EstaVivo)
            .OrderByDescending(c => c.VelocidadeTotal)
            .ThenBy(_ => rng.Next());

        foreach (Combatente combatente in ordem)
        {
            ordemTurnos.Enqueue(combatente);
        }
    }
}
