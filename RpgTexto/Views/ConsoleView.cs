using RpgTexto.Models;
using RpgTexto.Models.Personagens;
using RpgTexto.Models.Sistema;

namespace RpgTexto.Views;

/// <summary>A entrada do console foi encerrada (ex.: Ctrl+Z / fim do arquivo de entrada).</summary>
public class EntradaEncerradaException : Exception
{
}

/// <summary>
/// View base: cuida apenas de mostrar informações no console e ler o que o jogador digita.
/// Não contém regras do jogo.
/// </summary>
public class ConsoleView
{
    private const int LarguraLinha = 60;

    protected readonly Textos textos;

    public ConsoleView(Textos textos)
    {
        this.textos = textos;
    }

    // ---------------------------------------------------------------- Saída

    public void Limpar()
    {
        if (Console.IsOutputRedirected)
        {
            Console.WriteLine();
            return;
        }

        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            Console.WriteLine();
        }
    }

    public void ExibirTitulo(string titulo)
    {
        string linha = new('=', LarguraLinha);
        Escrever(linha, ConsoleColor.DarkCyan);
        Escrever("  " + titulo.ToUpperInvariant(), ConsoleColor.Cyan);
        Escrever(linha, ConsoleColor.DarkCyan);
    }

    public void ExibirSubtitulo(string subtitulo) => Escrever($"-- {subtitulo} --", ConsoleColor.Cyan);

    public void ExibirTexto(string texto) => Console.WriteLine(texto);

    public void ExibirLinhaEmBranco() => Console.WriteLine();

    public void ExibirSucesso(string texto) => Escrever(texto, ConsoleColor.Green);

    public void ExibirAviso(string texto) => Escrever(texto, ConsoleColor.Yellow);

    public void ExibirErro(string texto) => Escrever(texto, ConsoleColor.Red);

    public void ExibirDestaque(string texto) => Escrever(texto, ConsoleColor.Magenta);

    public void ExibirResultado(ResultadoAcao resultado)
    {
        if (resultado.Sucesso)
        {
            ExibirSucesso(resultado.Mensagem);
        }
        else
        {
            ExibirErro(resultado.Mensagem);
        }
    }

    // ---------------------------------------------------------------- Entrada

    /// <summary>Mostra as opções numeradas e retorna o índice escolhido (começando em 0).</summary>
    public int Escolher(string titulo, IReadOnlyList<string> opcoes)
    {
        if (!string.IsNullOrWhiteSpace(titulo))
        {
            ExibirSubtitulo(titulo);
        }

        for (int i = 0; i < opcoes.Count; i++)
        {
            Console.WriteLine($"  {i + 1} - {opcoes[i]}");
        }

        return LerOpcao(1, opcoes.Count) - 1;
    }

    public int LerOpcao(int minimo, int maximo)
    {
        while (true)
        {
            Console.Write(textos.Obter("ui.prompt") + " ");
            string entrada = LerLinha().Trim();
            if (int.TryParse(entrada, out int valor) && valor >= minimo && valor <= maximo)
            {
                return valor;
            }

            ExibirErro(textos.Obter("ui.opcao_invalida", minimo, maximo));
        }
    }

    public string LerTexto(string pergunta, int tamanhoMaximo)
    {
        while (true)
        {
            Console.Write(pergunta + " ");
            string entrada = LerLinha().Trim();
            if (entrada.Length == 0)
            {
                ExibirErro(textos.Obter("ui.texto_vazio"));
            }
            else if (entrada.Length > tamanhoMaximo)
            {
                ExibirErro(textos.Obter("ui.texto_longo", tamanhoMaximo));
            }
            else
            {
                return entrada;
            }
        }
    }

    public bool Confirmar(string pergunta)
    {
        return Escolher(pergunta, new[] { textos.Obter("ui.sim"), textos.Obter("ui.nao") }) == 0;
    }

    public void Pausar()
    {
        Console.Write(textos.Obter("ui.pausa") + " ");
        LerLinha();
    }

    /// <summary>Lista os personagens e retorna o índice escolhido, ou -1 para voltar.</summary>
    public int EscolherPersonagem(string titulo, IReadOnlyList<Personagem> personagens)
    {
        var opcoes = personagens
            .Select(p => textos.Obter("personagem.opcao", p.Nome, p.NomeClasse, p.Vida, p.VidaMaxima, p.Mana, p.ManaMaxima))
            .Append(textos.Obter("ui.voltar"))
            .ToList();
        int escolha = Escolher(titulo, opcoes);
        return escolha == personagens.Count ? -1 : escolha;
    }

    // ---------------------------------------------------------------- Grupo e quests

    public void ExibirResumoGrupo(Grupo grupo)
    {
        foreach (Personagem p in grupo.Membros)
        {
            string linha = textos.Obter("status.resumo", p.Nome, p.NomeClasse, p.Nivel,
                Barra(p.Vida, p.VidaMaxima), p.Vida, p.VidaMaxima, p.Mana, p.ManaMaxima);
            Escrever(linha, p.EstaVivo ? ConsoleColor.Gray : ConsoleColor.DarkGray);
        }

        ExibirTexto(textos.Obter("status.dinheiro", grupo.DinheiroTotal));
    }

    public void ExibirStatusDetalhado(Grupo grupo)
    {
        string nenhum = textos.Obter("status.nenhum");
        foreach (Personagem p in grupo.Membros)
        {
            ExibirSubtitulo(textos.Obter("status.cabecalho", p.Nome, p.NomeClasse, p.Nivel));
            ExibirTexto(textos.Obter("status.xp", p.Xp, p.XpProximoNivel));
            ExibirTexto(textos.Obter("status.vida_mana", p.Vida, p.VidaMaxima, p.Mana, p.ManaMaxima));
            ExibirTexto(textos.Obter("status.atributos", p.AtaqueTotal, p.DefesaTotal, p.VelocidadeTotal));
            ExibirTexto(textos.Obter("status.dinheiro_personagem", p.Dinheiro));
            ExibirTexto(textos.Obter("status.equipamento", p.Arma?.Nome ?? nenhum, p.Armadura?.Nome ?? nenhum));
            string especiais = string.Join(", ", p.Especiais.Select(e => textos.Obter("status.especial", e.Nome, e.CustoMana)));
            ExibirTexto(textos.Obter("status.especiais", especiais));
            ExibirLinhaEmBranco();
        }
    }

    public void ExibirDiario(IReadOnlyList<Quest> quests)
    {
        var abertas = quests.Where(q => q.Status is StatusQuest.Ativa or StatusQuest.Concluida).ToList();
        if (abertas.Count == 0)
        {
            ExibirAviso(textos.Obter("diario.vazio"));
        }

        foreach (Quest quest in abertas)
        {
            ExibirSubtitulo(quest.Titulo);
            ExibirTexto(quest.Descricao);
            string status = textos.Obter($"quest.status.{quest.Status.ToString().ToLowerInvariant()}");
            ExibirDestaque(textos.Obter("diario.progresso", status, quest.Progresso, quest.Quantidade));
            ExibirLinhaEmBranco();
        }

        int entregues = quests.Count(q => q.Status == StatusQuest.Entregue);
        ExibirTexto(textos.Obter("diario.entregues", entregues, quests.Count));
    }

    // ---------------------------------------------------------------- Auxiliares

    protected static void Escrever(string texto, ConsoleColor cor)
    {
        ConsoleColor anterior = Console.ForegroundColor;
        Console.ForegroundColor = cor;
        Console.WriteLine(texto);
        Console.ForegroundColor = anterior;
    }

    protected static string Barra(int atual, int maximo, int largura = 10)
    {
        int cheio = maximo <= 0 ? 0 : (int)Math.Round((double)Math.Max(0, atual) / maximo * largura);
        return "[" + new string('#', cheio) + new string('-', largura - cheio) + "]";
    }

    private static string LerLinha()
    {
        return Console.ReadLine() ?? throw new EntradaEncerradaException();
    }
}
