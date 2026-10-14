using System.Globalization;
using System.Text;
using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;
using RpgTexto.Models.Sistema;

namespace RpgTexto.Models.Persistencia;

/// <summary>
/// Classe exclusiva de leitura dos arquivos de dados. Faz o parse das linhas
/// e atribui os valores aos objetos correspondentes. Linhas com erro são
/// ignoradas e registradas em <see cref="Avisos"/>, sem interromper o jogo.
/// </summary>
public class LeitorArquivos
{
    public const string ArquivoTextos = "textos.txt";
    public const string ArquivoItens = "items.txt";
    public const string ArquivoMonstros = "monstros.txt";
    public const string ArquivoClasses = "classes.txt";
    public const string ArquivoQuests = "quests.txt";

    private const char Separador = ';';
    private const char SeparadorLista = ',';
    private const char Comentario = '#';
    private const string TodasAsClasses = "TODAS";
    private const string SemItem = "-";

    private static readonly Dictionary<string, TipoEspecial> tiposEspecial = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DANO_UNICO"] = TipoEspecial.DanoUnico,
        ["DANO_TODOS"] = TipoEspecial.DanoTodos,
        ["CURA_ALIADO"] = TipoEspecial.CuraAliado,
        ["CURA_GRUPO"] = TipoEspecial.CuraGrupo,
        ["DRENAR_VIDA"] = TipoEspecial.DrenarVida
    };

    private static readonly Dictionary<string, TipoQuest> tiposQuest = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DERROTAR"] = TipoQuest.Derrotar,
        ["DERROTAR_EM"] = TipoQuest.DerrotarEm,
        ["VISITAR"] = TipoQuest.Visitar
    };

    private readonly string pasta;
    private readonly List<string> avisos = new();

    public LeitorArquivos(string pasta)
    {
        this.pasta = pasta;
    }

    public IReadOnlyList<string> Avisos => avisos;

    public Catalogo CarregarCatalogo()
    {
        Textos textos = CarregarTextos();
        List<Item> itens = CarregarItens();
        List<Monstro> monstros = CarregarMonstros();
        List<DadosClasse> classes = CarregarClasses();
        List<DefinicaoQuest> quests = CarregarQuests();

        VerificarMinimo(ArquivoItens, "poções", itens.OfType<Pocao>().Count(), 5);
        VerificarMinimo(ArquivoItens, "armas", itens.OfType<Arma>().Count(), 20);
        VerificarMinimo(ArquivoItens, "armaduras", itens.OfType<Armadura>().Count(), 20);
        VerificarMinimo(ArquivoMonstros, "monstros", monstros.Count, 20);
        VerificarMinimo(ArquivoClasses, "classes", classes.Count, 5);
        VerificarMinimo(ArquivoQuests, "quests", quests.Count, 5);

        return new Catalogo(textos, itens, monstros, classes, quests);
    }

    // ---------------------------------------------------------------- textos.txt  (chave=texto)

    public Textos CarregarTextos()
    {
        var textos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (numero, linha) in LerLinhas(ArquivoTextos))
        {
            int posicao = linha.IndexOf('=');
            if (posicao <= 0)
            {
                Avisar(ArquivoTextos, numero, "formato esperado: chave=texto");
                continue;
            }

            string chave = linha[..posicao].Trim();
            string valor = linha[(posicao + 1)..].Trim().Replace("\\n", Environment.NewLine);
            if (!textos.TryAdd(chave, valor))
            {
                Avisar(ArquivoTextos, numero, $"a chave '{chave}' está repetida");
            }
        }

        return new Textos(textos);
    }

    // ---------------------------------------------------------------- items.txt

    public List<Item> CarregarItens()
    {
        var itens = new List<Item>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (numero, linha) in LerLinhas(ArquivoItens))
        {
            string[] campos = ParseLinha(linha);
            Item? item = campos[0].ToUpperInvariant() switch
            {
                "POCAO" => CriarPocao(campos, numero),
                "ARMA" => CriarArma(campos, numero),
                "ARMADURA" => CriarArmadura(campos, numero),
                _ => Invalido<Item>(ArquivoItens, numero, $"tipo de item '{campos[0]}' desconhecido")
            };

            if (item is null)
            {
                continue;
            }

            if (!ids.Add(item.Id))
            {
                Avisar(ArquivoItens, numero, $"o id '{item.Id}' está repetido");
                continue;
            }

            itens.Add(item);
        }

        return itens;
    }

    // POCAO;id;nome;preco;recuperaVida;recuperaMana;descricao
    private Pocao? CriarPocao(string[] c, int numero)
    {
        if (!TemCampos(c, 7, ArquivoItens, numero)
            || !LerInteiro(c[3], "preço", ArquivoItens, numero, out int preco)
            || !LerInteiro(c[4], "vida", ArquivoItens, numero, out int vida)
            || !LerInteiro(c[5], "mana", ArquivoItens, numero, out int mana))
        {
            return null;
        }

        return new Pocao(c[1], c[2], preco, vida, mana, JuntarResto(c, 6));
    }

    // ARMA;id;nome;preco;bonusAtaque;classes;descricao
    private Arma? CriarArma(string[] c, int numero)
    {
        if (!TemCampos(c, 7, ArquivoItens, numero)
            || !LerInteiro(c[3], "preço", ArquivoItens, numero, out int preco)
            || !LerInteiro(c[4], "bônus de ataque", ArquivoItens, numero, out int ataque))
        {
            return null;
        }

        return new Arma(c[1], c[2], preco, ataque, ParseClasses(c[5]), JuntarResto(c, 6));
    }

    // ARMADURA;id;nome;preco;bonusDefesa;bonusVelocidade;classes;descricao
    private Armadura? CriarArmadura(string[] c, int numero)
    {
        if (!TemCampos(c, 8, ArquivoItens, numero)
            || !LerInteiro(c[3], "preço", ArquivoItens, numero, out int preco)
            || !LerInteiro(c[4], "bônus de defesa", ArquivoItens, numero, out int defesa)
            || !LerInteiro(c[5], "bônus de velocidade", ArquivoItens, numero, out int velocidade, permitirNegativo: true))
        {
            return null;
        }

        return new Armadura(c[1], c[2], preco, defesa, velocidade, ParseClasses(c[6]), JuntarResto(c, 7));
    }

    // ---------------------------------------------------------------- monstros.txt

    // id;nome;vida;ataque;defesa;velocidade;xp;ouro;areas
    public List<Monstro> CarregarMonstros()
    {
        var monstros = new List<Monstro>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (numero, linha) in LerLinhas(ArquivoMonstros))
        {
            string[] c = ParseLinha(linha);
            if (!TemCampos(c, 9, ArquivoMonstros, numero)
                || !LerInteiro(c[2], "vida", ArquivoMonstros, numero, out int vida)
                || !LerInteiro(c[3], "ataque", ArquivoMonstros, numero, out int ataque)
                || !LerInteiro(c[4], "defesa", ArquivoMonstros, numero, out int defesa)
                || !LerInteiro(c[5], "velocidade", ArquivoMonstros, numero, out int velocidade)
                || !LerInteiro(c[6], "xp", ArquivoMonstros, numero, out int xp)
                || !LerInteiro(c[7], "ouro", ArquivoMonstros, numero, out int ouro))
            {
                continue;
            }

            if (!ids.Add(c[0]))
            {
                Avisar(ArquivoMonstros, numero, $"o id '{c[0]}' está repetido");
                continue;
            }

            monstros.Add(new Monstro(c[0], c[1], vida, ataque, defesa, velocidade, xp, ouro, ParseLista(c[8])));
        }

        return monstros;
    }

    // ---------------------------------------------------------------- classes.txt

    // CLASSE;id;nome;vida;mana;ataque;defesa;velocidade;descricao
    // ESPECIAL;classeId;nome;custoMana;poder;tipo;descricao
    public List<DadosClasse> CarregarClasses()
    {
        var classes = new Dictionary<string, DadosClasse>(StringComparer.OrdinalIgnoreCase);
        var especiais = new List<(int Numero, string[] Campos)>();

        foreach (var (numero, linha) in LerLinhas(ArquivoClasses))
        {
            string[] c = ParseLinha(linha);
            switch (c[0].ToUpperInvariant())
            {
                case "CLASSE":
                    DadosClasse? classe = CriarClasse(c, numero);
                    if (classe is not null && !classes.TryAdd(classe.Id, classe))
                    {
                        Avisar(ArquivoClasses, numero, $"a classe '{classe.Id}' está repetida");
                    }

                    break;

                case "ESPECIAL":
                    especiais.Add((numero, c));
                    break;

                default:
                    Avisar(ArquivoClasses, numero, $"tipo de linha '{c[0]}' desconhecido");
                    break;
            }
        }

        // Os especiais são associados depois, então a ordem das linhas no arquivo não importa.
        foreach (var (numero, c) in especiais)
        {
            AdicionarEspecial(c, numero, classes);
        }

        foreach (DadosClasse classe in classes.Values)
        {
            if (!FabricaPersonagens.Suporta(classe.Id))
            {
                avisos.Add($"[{ArquivoClasses}] a classe '{classe.Id}' não possui implementação no código e não aparecerá no jogo.");
            }

            if (classe.Especiais.Count < 3)
            {
                avisos.Add($"[{ArquivoClasses}] a classe '{classe.Id}' possui {classe.Especiais.Count} especiais (mínimo 3).");
            }
        }

        return classes.Values.ToList();
    }

    private DadosClasse? CriarClasse(string[] c, int numero)
    {
        if (!TemCampos(c, 9, ArquivoClasses, numero)
            || !LerInteiro(c[3], "vida", ArquivoClasses, numero, out int vida)
            || !LerInteiro(c[4], "mana", ArquivoClasses, numero, out int mana)
            || !LerInteiro(c[5], "ataque", ArquivoClasses, numero, out int ataque)
            || !LerInteiro(c[6], "defesa", ArquivoClasses, numero, out int defesa)
            || !LerInteiro(c[7], "velocidade", ArquivoClasses, numero, out int velocidade))
        {
            return null;
        }

        return new DadosClasse(c[1], c[2], vida, mana, ataque, defesa, velocidade, JuntarResto(c, 8));
    }

    private void AdicionarEspecial(string[] c, int numero, Dictionary<string, DadosClasse> classes)
    {
        if (!TemCampos(c, 7, ArquivoClasses, numero)
            || !LerInteiro(c[3], "custo de mana", ArquivoClasses, numero, out int custo)
            || !LerInteiro(c[4], "poder", ArquivoClasses, numero, out int poder))
        {
            return;
        }

        if (!classes.TryGetValue(c[1], out DadosClasse? classe))
        {
            Avisar(ArquivoClasses, numero, $"a classe '{c[1]}' do especial não existe");
            return;
        }

        if (!tiposEspecial.TryGetValue(c[5], out TipoEspecial tipo))
        {
            Avisar(ArquivoClasses, numero, $"tipo de especial '{c[5]}' inválido");
            return;
        }

        classe.AdicionarEspecial(new Especial(c[2], custo, poder, tipo, JuntarResto(c, 6)));
    }

    // ---------------------------------------------------------------- quests.txt

    // id;tavernaId;tipo;alvo;quantidade;xp;ouro;itemRecompensa
    public List<DefinicaoQuest> CarregarQuests()
    {
        var quests = new List<DefinicaoQuest>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (numero, linha) in LerLinhas(ArquivoQuests))
        {
            string[] c = ParseLinha(linha);
            if (!TemCampos(c, 8, ArquivoQuests, numero)
                || !LerInteiro(c[4], "quantidade", ArquivoQuests, numero, out int quantidade)
                || !LerInteiro(c[5], "xp", ArquivoQuests, numero, out int xp)
                || !LerInteiro(c[6], "ouro", ArquivoQuests, numero, out int ouro))
            {
                continue;
            }

            if (!tiposQuest.TryGetValue(c[2], out TipoQuest tipo))
            {
                Avisar(ArquivoQuests, numero, $"tipo de quest '{c[2]}' inválido");
                continue;
            }

            if (!ids.Add(c[0]))
            {
                Avisar(ArquivoQuests, numero, $"o id '{c[0]}' está repetido");
                continue;
            }

            string? item = c[7] == SemItem || c[7].Length == 0 ? null : c[7];
            quests.Add(new DefinicaoQuest(c[0], c[1], tipo, c[3], quantidade, xp, ouro, item));
        }

        return quests;
    }

    // ---------------------------------------------------------------- Parse

    /// <summary>Lê o arquivo e retorna as linhas úteis (ignora linhas vazias e comentários).</summary>
    private IEnumerable<(int Numero, string Linha)> LerLinhas(string arquivo)
    {
        string caminho = Path.Combine(pasta, arquivo);
        string[] linhas;
        try
        {
            linhas = File.ReadAllLines(caminho, Encoding.UTF8);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new ArquivoDadosException($"Não foi possível ler o arquivo '{caminho}'.", ex);
        }

        var resultado = new List<(int, string)>();
        for (int i = 0; i < linhas.Length; i++)
        {
            string linha = linhas[i].Trim();
            if (linha.Length > 0 && linha[0] != Comentario)
            {
                resultado.Add((i + 1, linha));
            }
        }

        return resultado;
    }

    private static string[] ParseLinha(string linha)
    {
        return linha.Split(Separador).Select(campo => campo.Trim()).ToArray();
    }

    private static List<string> ParseLista(string valor)
    {
        return valor
            .Split(SeparadorLista, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    /// <summary>"TODAS" ou vazio significa que qualquer classe pode usar.</summary>
    private static List<string> ParseClasses(string valor)
    {
        return valor.Equals(TodasAsClasses, StringComparison.OrdinalIgnoreCase) ? new List<string>() : ParseLista(valor);
    }

    /// <summary>A descrição é o último campo; se ela tiver ';' as partes são juntadas de volta.</summary>
    private static string JuntarResto(string[] campos, int inicio)
    {
        return string.Join(Separador, campos.Skip(inicio)).Trim();
    }

    private bool LerInteiro(string valor, string campo, string arquivo, int numero, out int resultado, bool permitirNegativo = false)
    {
        if (int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out resultado)
            && (permitirNegativo || resultado >= 0))
        {
            return true;
        }

        Avisar(arquivo, numero, $"valor inválido para {campo}: '{valor}'");
        return false;
    }

    private bool TemCampos(string[] campos, int minimo, string arquivo, int numero)
    {
        if (campos.Length >= minimo)
        {
            return true;
        }

        Avisar(arquivo, numero, $"esperados {minimo} campos separados por '{Separador}', encontrados {campos.Length}");
        return false;
    }

    private T? Invalido<T>(string arquivo, int numero, string mensagem) where T : class
    {
        Avisar(arquivo, numero, mensagem);
        return null;
    }

    private void Avisar(string arquivo, int numero, string mensagem)
    {
        avisos.Add($"[{arquivo}, linha {numero}] {mensagem}. Linha ignorada.");
    }

    private void VerificarMinimo(string arquivo, string descricao, int quantidade, int minimo)
    {
        if (quantidade < minimo)
        {
            avisos.Add($"[{arquivo}] {quantidade} {descricao} carregados (mínimo exigido: {minimo}).");
        }
    }
}
