using System.Text;
using RpgTexto.Controllers;
using RpgTexto.Models;
using RpgTexto.Models.Persistencia;
using RpgTexto.Views;

Console.OutputEncoding = Encoding.UTF8;

// Os arquivos .txt ficam na mesma pasta do executável.
string pasta = AppContext.BaseDirectory;

try
{
    var leitor = new LeitorArquivos(pasta);
    Catalogo catalogo = leitor.CarregarCatalogo();

    if (leitor.Avisos.Count > 0)
    {
        var view = new ConsoleView(catalogo.Textos);
        view.ExibirTitulo(catalogo.Textos.Obter("dados.avisos"));
        foreach (string aviso in leitor.Avisos)
        {
            view.ExibirAviso(aviso);
        }

        view.Pausar();
    }

    // "--seed N" deixa os sorteios sempre iguais, o que ajuda nos testes.
    Random rng = LerSemente(args) is int semente ? new Random(semente) : new Random();
    var controller = new JogoController(catalogo, new GerenciadorSave(Path.Combine(pasta, "save.txt")), rng);
    controller.Iniciar();
}
catch (ArquivoDadosException ex)
{
    // O textos.txt pode ser justamente o arquivo que falhou, por isso esta mensagem fica no código.
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Não foi possível carregar os dados do jogo.");
    Console.WriteLine(ex.Message);
    Console.ResetColor();
}
catch (EntradaEncerradaException)
{
    // A entrada do console foi fechada: encerra o jogo sem erro.
}

static int? LerSemente(string[] args)
{
    int indice = Array.IndexOf(args, "--seed");
    return indice >= 0 && indice + 1 < args.Length && int.TryParse(args[indice + 1], out int semente)
        ? semente
        : null;
}
