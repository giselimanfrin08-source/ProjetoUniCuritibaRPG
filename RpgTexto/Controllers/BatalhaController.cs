using RpgTexto.Models;
using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;
using RpgTexto.Models.Sistema;
using RpgTexto.Views;

namespace RpgTexto.Controllers;

/// <summary>
/// Conduz a batalha: pede a ação de cada personagem, executa os turnos dos monstros
/// e distribui as recompensas no final.
/// </summary>
public class BatalhaController
{
    private const int TamanhoRegistro = 8;

    private readonly Textos textos;
    private readonly BatalhaView view;
    private readonly ItemView itemView;
    private readonly Random rng;
    private readonly List<string> registro = new();

    public BatalhaController(Textos textos, BatalhaView view, ItemView itemView, Random rng)
    {
        this.textos = textos;
        this.view = view;
        this.itemView = itemView;
        this.rng = rng;
    }

    public ResultadoBatalha Executar(Jogo jogo, List<Monstro> inimigos, string? mensagemInicial = null)
    {
        if (inimigos.Count == 0)
        {
            return ResultadoBatalha.Vitoria;
        }

        var batalha = new Batalha(jogo.Grupo, inimigos, rng, textos);
        registro.Clear();
        if (mensagemInicial is not null)
        {
            Registrar(mensagemInicial);
        }

        Registrar(textos.Obter("batalha.inicio", string.Join(", ", inimigos.Select(i => i.Nome))));

        Combatente? atual;
        while ((atual = batalha.ProximoCombatente()) is not null)
        {
            if (atual is Personagem personagem)
            {
                TurnoPersonagem(batalha, personagem);
            }
            else if (atual is Monstro monstro)
            {
                Registrar(batalha.ExecutarTurnoMonstro(monstro).Mensagem);
            }
        }

        view.ExibirCampoBatalha(batalha, registro);

        if (batalha.GrupoVenceu)
        {
            Recompensar(jogo, batalha);
            return ResultadoBatalha.Vitoria;
        }

        if (batalha.Fugiu)
        {
            view.ExibirAviso(textos.Obter("batalha.fugiu"));
            view.Pausar();
            return ResultadoBatalha.Fuga;
        }

        view.ExibirErro(textos.Obter("batalha.derrota"));
        view.Pausar();
        return ResultadoBatalha.Derrota;
    }

    private void TurnoPersonagem(Batalha batalha, Personagem personagem)
    {
        string[] chavesAcoes =
        {
            "batalha.acao.atacar", "batalha.acao.defender", "batalha.acao.especial",
            "batalha.acao.item", "batalha.acao.fugir"
        };
        List<string> acoes = chavesAcoes.Select(chave => textos.Obter(chave)).ToList();

        while (true)
        {
            view.ExibirCampoBatalha(batalha, registro);
            view.ExibirTurno(personagem);
            var acao = (AcaoBatalha)view.Escolher(textos.Obter("batalha.pergunta"), acoes);

            ResultadoAcao? resultado = acao switch
            {
                AcaoBatalha.Atacar => Atacar(batalha, personagem),
                AcaoBatalha.Defender => batalha.Defender(personagem),
                AcaoBatalha.Especial => UsarEspecial(batalha, personagem),
                AcaoBatalha.UsarItem => UsarItem(batalha, personagem),
                AcaoBatalha.Fugir => batalha.TentarFugir(),
                _ => null
            };

            // null = o jogador escolheu "Voltar" em algum submenu
            if (resultado is null)
            {
                continue;
            }

            Registrar(resultado.Mensagem);
            if (resultado.Sucesso)
            {
                return;
            }
        }
    }

    private ResultadoAcao? Atacar(Batalha batalha, Personagem personagem)
    {
        Monstro? alvo = EscolherInimigo(batalha);
        return alvo is null ? null : batalha.Atacar(personagem, alvo);
    }

    private ResultadoAcao? UsarEspecial(Batalha batalha, Personagem personagem)
    {
        if (personagem.Especiais.Count == 0)
        {
            return ResultadoAcao.Falha(textos.Obter("especial.nenhum", personagem.Nome));
        }

        int indice = view.EscolherEspecial(personagem);
        if (indice < 0)
        {
            return null;
        }

        Especial especial = personagem.Especiais[indice];
        if (!personagem.PodeUsar(especial))
        {
            return ResultadoAcao.Falha(textos.Obter("especial.sem_mana", personagem.Nome, especial.Nome));
        }

        Combatente? alvo = null;
        if (especial.Alvo == AlvoEspecial.Inimigo)
        {
            alvo = EscolherInimigo(batalha);
            if (alvo is null)
            {
                return null;
            }
        }
        else if (especial.Alvo == AlvoEspecial.Aliado)
        {
            IReadOnlyList<Personagem> aliados = batalha.Grupo.MembrosVivos;
            int escolhido = view.EscolherPersonagem(textos.Obter("batalha.escolher_aliado"), aliados);
            if (escolhido < 0)
            {
                return null;
            }

            alvo = aliados[escolhido];
        }

        return batalha.UsarEspecial(personagem, especial, alvo);
    }

    private ResultadoAcao? UsarItem(Batalha batalha, Personagem personagem)
    {
        IReadOnlyList<ItemAgrupado> pocoes = batalha.Grupo.Inventario.ListarPocoes();
        if (pocoes.Count == 0)
        {
            return ResultadoAcao.Falha(textos.Obter("batalha.sem_pocoes"));
        }

        int indiceItem = itemView.EscolherItemAgrupado(textos.Obter("batalha.escolher_item"), pocoes, exibirPrecoVenda: false);
        if (indiceItem < 0)
        {
            return null;
        }

        IReadOnlyList<Personagem> aliados = batalha.Grupo.MembrosVivos;
        int indiceAlvo = view.EscolherPersonagem(textos.Obter("batalha.escolher_aliado"), aliados);
        if (indiceAlvo < 0)
        {
            return null;
        }

        return batalha.UsarItem(personagem, pocoes[indiceItem].Item, aliados[indiceAlvo]);
    }

    private Monstro? EscolherInimigo(Batalha batalha)
    {
        IReadOnlyList<Monstro> vivos = batalha.InimigosVivos;
        if (vivos.Count == 1)
        {
            return vivos[0];
        }

        int indice = view.EscolherAlvo(textos.Obter("batalha.escolher_alvo"), vivos);
        return indice < 0 ? null : vivos[indice];
    }

    private void Recompensar(Jogo jogo, Batalha batalha)
    {
        view.ExibirSucesso(textos.Obter("batalha.vitoria", batalha.XpTotal, batalha.OuroTotal));
        jogo.Grupo.ReceberDinheiro(batalha.OuroTotal);

        foreach (EvolucaoPersonagem evolucao in jogo.Grupo.DistribuirXp(batalha.XpTotal))
        {
            view.ExibirSucesso(textos.Obter("nivel.subiu", evolucao.Personagem.Nome, evolucao.Personagem.Nivel));
        }

        var questsAvancadas = new List<Quest>();
        foreach (Monstro inimigo in batalha.Inimigos)
        {
            questsAvancadas.AddRange(jogo.RegistrarEvento(TipoEvento.MonstroDerrotado, inimigo.Id));
        }

        ControllerHelper.ExibirProgressoQuests(view, textos, questsAvancadas);
        view.Pausar();
    }

    private void Registrar(string mensagem)
    {
        if (string.IsNullOrWhiteSpace(mensagem))
        {
            return;
        }

        registro.AddRange(mensagem.Split(Environment.NewLine));
        if (registro.Count > TamanhoRegistro)
        {
            registro.RemoveRange(0, registro.Count - TamanhoRegistro);
        }
    }
}
