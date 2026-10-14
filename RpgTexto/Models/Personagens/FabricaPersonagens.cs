namespace RpgTexto.Models.Personagens;

/// <summary>
/// Cria o objeto da classe jogável correta a partir do id lido do classes.txt.
/// </summary>
public static class FabricaPersonagens
{
    private static readonly Dictionary<string, Func<string, DadosClasse, Personagem>> construtores =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["mago"] = (nome, dados) => new Mago(nome, dados),
            ["paladino"] = (nome, dados) => new Paladino(nome, dados),
            ["barbaro"] = (nome, dados) => new Barbaro(nome, dados),
            ["necromante"] = (nome, dados) => new Necromante(nome, dados),
            ["arqueiro"] = (nome, dados) => new Arqueiro(nome, dados)
        };

    public static bool Suporta(string classeId) => construtores.ContainsKey(classeId);

    public static Personagem Criar(string nome, DadosClasse dados)
    {
        if (!construtores.TryGetValue(dados.Id, out var construtor))
        {
            throw new ArgumentException($"A classe '{dados.Id}' não possui implementação no código.", nameof(dados));
        }

        return construtor(nome, dados);
    }
}
