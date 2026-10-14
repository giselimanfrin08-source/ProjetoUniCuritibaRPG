using RpgTexto.Models;
using RpgTexto.Models.Cenarios;
using RpgTexto.Models.Itens;
using RpgTexto.Models.Persistencia;
using RpgTexto.Models.Personagens;
using RpgTexto.Models.Sistema;
using RpgTexto.Views;

namespace RpgTexto.Controllers;

/// <summary>
/// Controller principal: menu inicial, criação do grupo, carregamento do save
/// e o laço de exploração entre os cenários.
/// </summary>
public class JogoController
{
    private const int TamanhoMaximoNome = 15;

    private readonly Catalogo catalogo;
    private readonly Textos textos;
    private readonly GerenciadorSave gerenciadorSave;
    private readonly Random rng;
    private readonly ConsoleView view;
    private readonly BatalhaController batalhaController;
    private readonly CidadeController cidadeController;
    private readonly LojaController lojaController;
    private readonly InventarioController inventarioController;

    private Jogo? jogo;

    public JogoController(Catalogo catalogo, GerenciadorSave gerenciadorSave, Random rng)
    {
        this.catalogo = catalogo;
        this.gerenciadorSave = gerenciadorSave;
        this.rng = rng;
        textos = catalogo.Textos;

        view = new ConsoleView(textos);
        var itemView = new ItemView(textos, catalogo.NomeClasse);
        batalhaController = new BatalhaController(textos, new BatalhaView(textos), itemView, rng);
        cidadeController = new CidadeController(textos, view, gerenciadorSave);
        lojaController = new LojaController(textos, itemView);
        inventarioController = new InventarioController(textos, itemView);
    }

    public void Iniciar()
    {
        while (true)
        {
            view.Limpar();
            view.ExibirTitulo(textos.Obter("jogo.titulo"));
            view.ExibirTexto(textos.Obter("jogo.subtitulo"));
            view.ExibirLinhaEmBranco();

            int escolha = view.Escolher(textos.Obter("menu.titulo"), new[]
            {
                textos.Obter("menu.novo"),
                textos.Obter("menu.carregar"),
                textos.Obter("menu.sair")
            });

            switch (escolha)
            {
                case 0:
                    NovoJogo();
                    break;
                case 1:
                    CarregarJogo();
                    break;
                default:
                    view.ExibirTexto(textos.Obter("menu.ate_logo"));
                    return;
            }
        }
    }

    // ---------------------------------------------------------------- Início

    private void NovoJogo()
    {
        List<DadosClasse> classes = catalogo.Classes.Where(c => FabricaPersonagens.Suporta(c.Id)).ToList();
        if (classes.Count == 0)
        {
            view.ExibirErro(textos.Obter("novo.sem_classes"));
            view.Pausar();
            return;
        }

        view.Limpar();
        view.ExibirTitulo(textos.Obter("novo.titulo"));
        view.ExibirTexto(textos.Obter("novo.introducao"));

        var grupo = new Grupo();
        for (int i = 1; i <= Grupo.TamanhoMaximo; i++)
        {
            view.ExibirLinhaEmBranco();
            string nome = view.LerTexto(textos.Obter("novo.nome", i), TamanhoMaximoNome);
            var opcoes = classes
                .Select(c => textos.Obter("novo.classe_opcao", c.Nome, c.Vida, c.Mana, c.Ataque, c.Defesa, c.Velocidade, c.Descricao))
                .ToList();
            DadosClasse classe = classes[view.Escolher(textos.Obter("novo.classe_pergunta", nome), opcoes)];

            Personagem personagem = FabricaPersonagens.Criar(nome, classe);
            personagem.ReceberDinheiro(Jogo.DinheiroInicialPorPersonagem);
            grupo.AdicionarMembro(personagem);
            view.ExibirSucesso(textos.Obter("novo.adicionado", nome, classe.Nome));
        }

        jogo = Jogo.CriarNovo(catalogo, grupo);

        view.Limpar();
        view.ExibirTitulo(textos.Obter("historia.titulo"));
        view.ExibirTexto(textos.Obter("historia.texto"));
        view.ExibirLinhaEmBranco();
        view.Pausar();

        LoopExploracao();
    }

    private void CarregarJogo()
    {
        if (!gerenciadorSave.ExisteSave())
        {
            view.ExibirAviso(textos.Obter("save.inexistente"));
            view.Pausar();
            return;
        }

        try
        {
            jogo = ConversorSave.Restaurar(gerenciadorSave.Carregar(), catalogo);
        }
        catch (SaveInvalidoException)
        {
            view.ExibirErro(textos.Obter("save.invalido"));
            view.Pausar();
            return;
        }

        view.ExibirSucesso(textos.Obter("save.carregado", jogo.CenarioAtual.Nome));
        view.Pausar();
        LoopExploracao();
    }

    // ---------------------------------------------------------------- Exploração

    private void LoopExploracao()
    {
        while (jogo is not null)
        {
            Cenario cenario = jogo.CenarioAtual;
            view.Limpar();
            view.ExibirTitulo(cenario.Nome);
            view.ExibirTexto(cenario.Descricao);
            view.ExibirLinhaEmBranco();
            view.ExibirResumoGrupo(jogo.Grupo);
            view.ExibirLinhaEmBranco();

            IReadOnlyList<OpcaoCenario> opcoesCenario = cenario.ObterOpcoes(jogo.Mundo, textos);
            var opcoes = opcoesCenario.Select(o => o.Texto).ToList();
            opcoes.Add(textos.Obter("explorar.status"));
            opcoes.Add(textos.Obter("explorar.inventario"));
            opcoes.Add(textos.Obter("explorar.diario"));
            opcoes.Add(textos.Obter("explorar.menu"));

            int escolha = view.Escolher(textos.Obter("explorar.pergunta"), opcoes);
            int opcaoGeral = escolha - opcoesCenario.Count;

            bool continuar = opcaoGeral switch
            {
                < 0 => ExecutarOpcao(opcoesCenario[escolha]),
                0 => MostrarStatus(),
                1 => AbrirInventario(),
                2 => MostrarDiario(),
                _ => !view.Confirmar(textos.Obter("explorar.confirmar_saida"))
            };

            if (!continuar)
            {
                jogo = null;
            }
        }
    }

    /// <summary>Executa a opção do cenário. Retorna false quando o jogo termina (derrota).</summary>
    private bool ExecutarOpcao(OpcaoCenario opcao)
    {
        Jogo atual = jogo!;
        return opcao.Acao switch
        {
            TipoAcaoCenario.Taverna => NaCidade(c => cidadeController.AbrirTaverna(c.Taverna, atual)),
            TipoAcaoCenario.Inn => NaCidade(c => cidadeController.AbrirInn(c.Inn, atual)),
            TipoAcaoCenario.Loja => NaCidade(c => lojaController.Abrir(c.Loja, atual.Grupo)),
            TipoAcaoCenario.Viajar when opcao.Destino is not null => Viajar(opcao.Destino),
            TipoAcaoCenario.Explorar => Explorar(),
            TipoAcaoCenario.BuscarTesouro => BuscarTesouro(),
            _ => true
        };
    }

    private bool NaCidade(Action<Cidade> acao)
    {
        if (jogo?.CenarioAtual is Cidade cidade)
        {
            acao(cidade);
        }

        return true;
    }

    private bool Viajar(string destinoId)
    {
        Jogo atual = jogo!;
        IReadOnlyList<Quest> avancadas = atual.MoverPara(destinoId);
        if (avancadas.Count > 0)
        {
            view.Limpar();
            ControllerHelper.ExibirProgressoQuests(view, textos, avancadas);
            view.Pausar();
        }

        if (atual.CenarioAtual is AreaHostil area && area.GeraEncontroAoEntrar && area.SortearEncontro(rng))
        {
            return Batalhar(area, textos.Obter("viagem.emboscada", area.Nome));
        }

        return true;
    }

    private bool Explorar()
    {
        if (jogo!.CenarioAtual is not AreaHostil area)
        {
            return true;
        }

        if (!area.TemMonstros)
        {
            view.ExibirAviso(textos.Obter("explorar.sem_inimigos"));
            view.Pausar();
            return true;
        }

        return Batalhar(area, textos.Obter("explorar.encontro"));
    }

    private bool BuscarTesouro()
    {
        Jogo atual = jogo!;
        if (atual.CenarioAtual is not Caverna caverna)
        {
            return true;
        }

        if (!caverna.TemTesouroRestante(atual.TesourosColetados))
        {
            view.ExibirAviso(textos.Obter("tesouro.esgotado"));
            view.Pausar();
            return true;
        }

        if (caverna.SortearEncontro(rng))
        {
            var inimigos = caverna.GerarEncontro(rng);
            ResultadoBatalha resultado = batalhaController.Executar(atual, inimigos, textos.Obter("tesouro.guardioes"));
            if (resultado == ResultadoBatalha.Derrota)
            {
                return FimDeJogo();
            }

            if (resultado == ResultadoBatalha.Fuga)
            {
                return true;
            }
        }

        Item? item = atual.ColetarTesouro(caverna);
        view.ExibirSucesso(textos.Obter("tesouro.encontrado", item?.Nome ?? string.Empty));
        view.Pausar();
        return true;
    }

    private bool Batalhar(AreaHostil area, string mensagemInicial)
    {
        ResultadoBatalha resultado = batalhaController.Executar(jogo!, area.GerarEncontro(rng), mensagemInicial);
        return resultado != ResultadoBatalha.Derrota || FimDeJogo();
    }

    /// <summary>Mostra a tela de fim de jogo e retorna false para voltar ao menu principal.</summary>
    private bool FimDeJogo()
    {
        view.Limpar();
        view.ExibirTitulo(textos.Obter("fim.titulo"));
        view.ExibirErro(textos.Obter("fim.texto"));
        view.Pausar();
        return false;
    }

    // ---------------------------------------------------------------- Opções gerais

    private bool MostrarStatus()
    {
        view.Limpar();
        view.ExibirTitulo(textos.Obter("status.titulo"));
        view.ExibirStatusDetalhado(jogo!.Grupo);
        view.ExibirTexto(textos.Obter("status.dinheiro", jogo.Grupo.DinheiroTotal));
        view.Pausar();
        return true;
    }

    private bool AbrirInventario()
    {
        inventarioController.Abrir(jogo!.Grupo);
        return true;
    }

    private bool MostrarDiario()
    {
        view.Limpar();
        view.ExibirTitulo(textos.Obter("diario.titulo"));
        view.ExibirDiario(jogo!.Quests);
        view.Pausar();
        return true;
    }
}
