using RpgTexto.Models;
using RpgTexto.Models.Sistema;
using RpgTexto.Views;

namespace RpgTexto.Controllers;

/// <summary>
/// Rotinas de exibição usadas por mais de um controller.
/// </summary>
internal static class ControllerHelper
{
    /// <summary>Mostra o progresso das quests que avançaram (cada quest aparece uma vez).</summary>
    public static void ExibirProgressoQuests(ConsoleView view, Textos textos, IEnumerable<Quest> quests)
    {
        foreach (Quest quest in quests.Distinct())
        {
            if (quest.Status == StatusQuest.Concluida)
            {
                view.ExibirDestaque(textos.Obter("quest.concluida", quest.Titulo));
            }
            else
            {
                view.ExibirDestaque(textos.Obter("quest.progresso", quest.Titulo, quest.Progresso, quest.Quantidade));
            }
        }
    }
}
