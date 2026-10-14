using RpgTexto.Models.Cenarios;
using RpgTexto.Models.Itens;
using RpgTexto.Models.Personagens;
using RpgTexto.Models.Sistema;

namespace RpgTexto.Models.Persistencia;

/// <summary>
/// Converte o estado do jogo em <see cref="DadosSave"/> e reconstrói o jogo a partir dele.
/// </summary>
public static class ConversorSave
{
    public static DadosSave CriarSave(Jogo jogo)
    {
        return new DadosSave
        {
            DataHora = DateTime.Now,
            CenarioAtual = jogo.CenarioAtual.Id,
            Personagens = jogo.Grupo.Membros.Select(p => p.CriarEstado()).ToList(),
            Inventario = jogo.Grupo.Inventario.Itens.Select(i => i.Id).ToList(),
            Quests = jogo.Quests
                .Select(q => new EstadoQuest { Id = q.Id, Status = q.Status, Progresso = q.Progresso })
                .ToList(),
            TesourosColetados = jogo.TesourosColetados.ToList()
        };
    }

    public static Jogo Restaurar(DadosSave dados, Catalogo catalogo)
    {
        if (dados.Personagens.Count is 0 or > Grupo.TamanhoMaximo)
        {
            throw new SaveInvalidoException("Quantidade de personagens inválida no save.");
        }

        var grupo = new Grupo();
        foreach (EstadoPersonagem estado in dados.Personagens)
        {
            DadosClasse classe = catalogo.ObterClasse(estado.ClasseId)
                                 ?? throw new SaveInvalidoException($"A classe '{estado.ClasseId}' não existe mais.");
            if (!FabricaPersonagens.Suporta(classe.Id))
            {
                throw new SaveInvalidoException($"A classe '{classe.Id}' não possui implementação.");
            }

            Personagem personagem = FabricaPersonagens.Criar(estado.Nome, classe);
            personagem.Restaurar(
                estado,
                catalogo.ObterItem(estado.ArmaId) as Arma,
                catalogo.ObterItem(estado.ArmaduraId) as Armadura);
            grupo.AdicionarMembro(personagem);
        }

        // Itens que foram removidos do items.txt depois do save são ignorados.
        foreach (Item item in dados.Inventario.Select(catalogo.ObterItem).OfType<Item>())
        {
            grupo.Inventario.Adicionar(item);
        }

        List<Quest> quests = catalogo.CriarQuests();
        foreach (EstadoQuest estadoQuest in dados.Quests)
        {
            quests.FirstOrDefault(q => q.Id == estadoQuest.Id)?.Restaurar(estadoQuest.Status, estadoQuest.Progresso);
        }

        Mundo mundo = Mundo.Criar(catalogo, quests);
        string cenario = mundo.Existe(dados.CenarioAtual) ? dados.CenarioAtual : Mundo.CenarioInicialId;
        return new Jogo(grupo, mundo, quests, cenario, dados.TesourosColetados);
    }
}
