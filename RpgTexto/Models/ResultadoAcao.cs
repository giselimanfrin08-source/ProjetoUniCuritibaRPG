namespace RpgTexto.Models;

/// <summary>
/// Resultado de uma ação do jogo: se ela foi executada e a mensagem a ser exibida.
/// </summary>
public sealed record ResultadoAcao(bool Sucesso, string Mensagem)
{
    public static ResultadoAcao Ok(string mensagem) => new(true, mensagem);

    public static ResultadoAcao Falha(string mensagem) => new(false, mensagem);
}
