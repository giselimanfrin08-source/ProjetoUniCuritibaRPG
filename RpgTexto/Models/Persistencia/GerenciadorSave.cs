using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RpgTexto.Models.Persistencia;

/// <summary>
/// Grava e lê o save.txt criptografado com AES (System.Security.Cryptography).
/// A chave é derivada de uma senha com PBKDF2 e um salt aleatório.
/// Formato do arquivo: Base64( salt[16] + IV[16] + dados criptografados ).
/// </summary>
public class GerenciadorSave
{
    private const string Senha = "RpgTexto-Proposta3-Save";
    private const int TamanhoSalt = 16;
    private const int TamanhoIv = 16;
    private const int TamanhoChave = 32;
    private const int Iteracoes = 100_000;

    private readonly string caminho;

    public GerenciadorSave(string caminho)
    {
        this.caminho = caminho;
    }

    public bool ExisteSave() => File.Exists(caminho);

    public void Salvar(DadosSave dados)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(dados);
        File.WriteAllText(caminho, Convert.ToBase64String(Criptografar(json)));
    }

    public DadosSave Carregar()
    {
        try
        {
            byte[] conteudo = Convert.FromBase64String(File.ReadAllText(caminho).Trim());
            byte[] json = Descriptografar(conteudo);
            return JsonSerializer.Deserialize<DadosSave>(json)
                   ?? throw new SaveInvalidoException("O save está vazio.");
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException or JsonException
                                       or IOException or UnauthorizedAccessException)
        {
            throw new SaveInvalidoException("Não foi possível ler o save.", ex);
        }
    }

    private static byte[] Criptografar(byte[] dados)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        using Aes aes = Aes.Create();
        aes.Key = DerivarChave(salt);
        aes.GenerateIV();
        byte[] cifrado = aes.EncryptCbc(dados, aes.IV);

        byte[] resultado = new byte[TamanhoSalt + TamanhoIv + cifrado.Length];
        Buffer.BlockCopy(salt, 0, resultado, 0, TamanhoSalt);
        Buffer.BlockCopy(aes.IV, 0, resultado, TamanhoSalt, TamanhoIv);
        Buffer.BlockCopy(cifrado, 0, resultado, TamanhoSalt + TamanhoIv, cifrado.Length);
        return resultado;
    }

    private static byte[] Descriptografar(byte[] conteudo)
    {
        if (conteudo.Length <= TamanhoSalt + TamanhoIv)
        {
            throw new CryptographicException("Conteúdo do save muito curto.");
        }

        byte[] salt = conteudo[..TamanhoSalt];
        byte[] iv = conteudo[TamanhoSalt..(TamanhoSalt + TamanhoIv)];
        byte[] cifrado = conteudo[(TamanhoSalt + TamanhoIv)..];

        using Aes aes = Aes.Create();
        aes.Key = DerivarChave(salt);
        return aes.DecryptCbc(cifrado, iv);
    }

    private static byte[] DerivarChave(byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(Senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoChave);
    }
}
