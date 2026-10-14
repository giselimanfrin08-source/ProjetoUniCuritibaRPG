using RpgTexto.Models;
using RpgTexto.Models.Cenarios;
using RpgTexto.Models.Persistencia;
using RpgTexto.Models.Personagens;
using RpgTexto.Models.Sistema;
using RpgTexto.Views;

namespace RpgTexto.Controllers;

/// <summary>
/// Fluxo dos locais da cidade: taverna (quests) e INN (descanso e save).
/// </summary>
public class CidadeController
{
    private readonly Textos textos;
    private readonly ConsoleView view;
    private readonly GerenciadorSave gerenciadorSave;

    public CidadeController(Textos textos, ConsoleView view, GerenciadorSave gerenciadorSave)
    {
        this.textos = textos;
        this.view = view;
        this.gerenciadorSave = gerenciadorSave;
    }

    // ---------------------------------------------------------------- Taverna

    public void AbrirTaverna(Taverna taverna, Jogo jogo)
    {
        string cidadeId = jogo.CenarioAtual.Id;
        while (true)
        {
            view.Limpar();
            view.ExibirTitulo(textos.Obter("taverna.titulo", jogo.CenarioAtual.Nome));
            view.ExibirTexto(textos.Obter($"cenario.{cidadeId}.taverna"));
            view.ExibirLinhaEmBranco();

            int escolha = view.Escolher(textos.Obter("taverna.pergunta"), new[]
            {
                textos.Obter("taverna.opcao.quests", taverna.QuestsDisponiveis.Count),
                textos.Obter("taverna.opcao.entregar", taverna.QuestsParaEntregar.Count),
                textos.Obter("taverna.opcao.conversar"),
                textos.Obter("ui.voltar")
            });

            switch (escolha)
            {
                case 0:
                    EscolherQuest(taverna);
                    break;
                case 1:
                    EntregarQuests(taverna, jogo.Grupo);
                    break;
                case 2:
                    view.ExibirLinhaEmBranco();
                    view.ExibirTexto(textos.Obter($"cenario.{cidadeId}.taverneiro"));
                    view.Pausar();
                    break;
                default:
                    return;
            }
        }
    }

    private void EscolherQuest(Taverna taverna)
    {
        IReadOnlyList<Quest> disponiveis = taverna.QuestsDisponiveis;
        if (disponiveis.Count == 0)
        {
            view.ExibirAviso(textos.Obter("taverna.sem_quests"));
            view.Pausar();
            return;
        }

        var opcoes = disponiveis.Select(q => q.Titulo).Append(textos.Obter("ui.voltar")).ToList();
        int indice = view.Escolher(textos.Obter("taverna.quests_titulo"), opcoes);
        if (indice == disponiveis.Count)
        {
            return;
        }

        Quest quest = disponiveis[indice];
        view.ExibirLinhaEmBranco();
        view.ExibirSubtitulo(quest.Titulo);
        view.ExibirTexto(quest.Descricao);
        string item = quest.RecompensaItem?.Nome ?? textos.Obter("taverna.sem_item");
        view.ExibirDestaque(textos.Obter("taverna.recompensa", quest.RecompensaXp, quest.RecompensaOuro, item));

        if (view.Confirmar(textos.Obter("taverna.aceitar")) && quest.Aceitar())
        {
            view.ExibirSucesso(textos.Obter("taverna.aceita", quest.Titulo));
            view.Pausar();
        }
    }

    private void EntregarQuests(Taverna taverna, Grupo grupo)
    {
        IReadOnlyList<Quest> prontas = taverna.QuestsParaEntregar;
        if (prontas.Count == 0)
        {
            view.ExibirAviso(textos.Obter("taverna.nada_entregar"));
            view.Pausar();
            return;
        }

        foreach (Quest quest in prontas)
        {
            IReadOnlyList<EvolucaoPersonagem> evolucoes = quest.Entregar(grupo);
            view.ExibirSucesso(textos.Obter("taverna.entregue", quest.Titulo, quest.RecompensaXp, quest.RecompensaOuro));
            if (quest.RecompensaItem is not null)
            {
                view.ExibirSucesso(textos.Obter("taverna.item_recebido", quest.RecompensaItem.Nome));
            }

            foreach (EvolucaoPersonagem evolucao in evolucoes)
            {
                view.ExibirSucesso(textos.Obter("nivel.subiu", evolucao.Personagem.Nome, evolucao.Personagem.Nivel));
            }
        }

        view.Pausar();
    }

    // ---------------------------------------------------------------- INN

    public void AbrirInn(Inn inn, Jogo jogo)
    {
        while (true)
        {
            view.Limpar();
            view.ExibirTitulo(textos.Obter("inn.titulo", jogo.CenarioAtual.Nome));
            view.ExibirTexto(textos.Obter("inn.descricao"));
            view.ExibirLinhaEmBranco();
            view.ExibirResumoGrupo(jogo.Grupo);
            view.ExibirLinhaEmBranco();

            int escolha = view.Escolher(textos.Obter("inn.pergunta"), new[]
            {
                textos.Obter("inn.opcao.descansar", inn.CustoDescanso),
                textos.Obter("inn.opcao.salvar"),
                textos.Obter("ui.voltar")
            });

            switch (escolha)
            {
                case 0:
                    view.ExibirResultado(inn.Descansar(jogo.Grupo, textos));
                    view.Pausar();
                    break;
                case 1:
                    Salvar(jogo);
                    break;
                default:
                    return;
            }
        }
    }

    private void Salvar(Jogo jogo)
    {
        if (gerenciadorSave.ExisteSave() && !view.Confirmar(textos.Obter("save.sobrescrever")))
        {
            return;
        }

        try
        {
            gerenciadorSave.Salvar(ConversorSave.CriarSave(jogo));
            view.ExibirSucesso(textos.Obter("save.sucesso"));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            view.ExibirErro(textos.Obter("save.erro"));
        }

        view.Pausar();
    }
}
