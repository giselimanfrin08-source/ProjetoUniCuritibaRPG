using System.Globalization;

namespace RpgTexto.Models;

/// <summary>
/// Repositório com todos os textos do jogo, carregados do arquivo textos.txt.
/// </summary>
public class Textos
{
    private readonly Dictionary<string, string> textos;

    public Textos(IDictionary<string, string> textos)
    {
        this.textos = new Dictionary<string, string>(textos, StringComparer.OrdinalIgnoreCase);
    }

    public int Quantidade => textos.Count;

    public bool Contem(string chave) => textos.ContainsKey(chave);

    /// <summary>
    /// Retorna o texto da chave, substituindo {0}, {1}... pelos argumentos.
    /// Se a chave não existir, retorna [chave] para facilitar a correção do arquivo.
    /// </summary>
    public string Obter(string chave, params object[] argumentos)
    {
        if (!textos.TryGetValue(chave, out string? texto))
        {
            return $"[{chave}]";
        }

        if (argumentos.Length == 0)
        {
            return texto;
        }

        try
        {
            return string.Format(CultureInfo.InvariantCulture, texto, argumentos);
        }
        catch (FormatException)
        {
            return texto;
        }
    }
}
