using RpgTexto.Models;
using RpgTexto.Models.Personagens;
using RpgTexto.Models.Sistema;

namespace RpgTexto.Views;

/// <summary>
/// Tela de batalha: inimigos, grupo, registro dos últimos acontecimentos e menus de ação.
/// </summary>
public class BatalhaView : ConsoleView
{
    public BatalhaView(Textos textos) : base(textos)
    {
    }

    public void ExibirCampoBatalha(Batalha batalha, IReadOnlyList<string> registro)
    {
        Limpar();
        ExibirTitulo(textos.Obter("batalha.titulo", Math.Max(1, batalha.Rodada)));

        ExibirSubtitulo(textos.Obter("batalha.inimigos"));
        foreach (Monstro inimigo in batalha.Inimigos)
        {
            if (inimigo.EstaVivo)
            {
                Escrever(textos.Obter("batalha.linha_inimigo", inimigo.Nome, Barra(inimigo.Vida, inimigo.VidaMaxima),
                    inimigo.Vida, inimigo.VidaMaxima), ConsoleColor.Red);
            }
            else
            {
                Escrever(textos.Obter("batalha.linha_inimigo_derrotado", inimigo.Nome), ConsoleColor.DarkGray);
            }
        }

        ExibirSubtitulo(textos.Obter("batalha.grupo"));
        foreach (Personagem p in batalha.Grupo.Membros)
        {
            string linha = textos.Obter("batalha.linha_personagem", p.Nome, Barra(p.Vida, p.VidaMaxima),
                p.Vida, p.VidaMaxima, p.Mana, p.ManaMaxima);
            if (p.Defendendo)
            {
                linha += " " + textos.Obter("batalha.marcador_defesa");
            }

            Escrever(linha, p.EstaVivo ? ConsoleColor.Green : ConsoleColor.DarkGray);
        }

        if (registro.Count > 0)
        {
            ExibirSubtitulo(textos.Obter("batalha.registro"));
            foreach (string linha in registro)
            {
                Escrever(linha, ConsoleColor.DarkYellow);
            }
        }

        ExibirLinhaEmBranco();
    }

    public void ExibirTurno(Personagem personagem)
    {
        Escrever(textos.Obter("batalha.vez", personagem.Nome, personagem.NomeClasse), ConsoleColor.White);
    }

    /// <summary>Lista os inimigos vivos e retorna o índice escolhido, ou -1 para voltar.</summary>
    public int EscolherAlvo(string titulo, IReadOnlyList<Combatente> alvos)
    {
        var opcoes = alvos
            .Select(a => textos.Obter("batalha.opcao_alvo", a.Nome, a.Vida, a.VidaMaxima))
            .Append(textos.Obter("ui.voltar"))
            .ToList();
        int escolha = Escolher(titulo, opcoes);
        return escolha == alvos.Count ? -1 : escolha;
    }

    /// <summary>Lista os especiais do personagem e retorna o índice escolhido, ou -1 para voltar.</summary>
    public int EscolherEspecial(Personagem personagem)
    {
        var opcoes = personagem.Especiais
            .Select(e => textos.Obter(personagem.PodeUsar(e) ? "batalha.opcao_especial" : "batalha.opcao_especial_sem_mana",
                e.Nome, e.CustoMana, e.Descricao))
            .Append(textos.Obter("ui.voltar"))
            .ToList();
        int escolha = Escolher(textos.Obter("batalha.escolher_especial", personagem.Mana), opcoes);
        return escolha == personagem.Especiais.Count ? -1 : escolha;
    }
}
