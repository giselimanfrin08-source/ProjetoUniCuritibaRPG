using RpgTexto.Models.Personagens;

namespace RpgTexto.Models.Itens;

/// <summary>
/// Base abstrata para itens que podem ser equipados (armas e armaduras).
/// </summary>
public abstract class Equipamento : Item
{
    private readonly HashSet<string> classesPermitidas;

    protected Equipamento(string id, string nome, int preco, IEnumerable<string> classesPermitidas, string descricao)
        : base(id, nome, preco, descricao)
    {
        this.classesPermitidas = new HashSet<string>(classesPermitidas, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Classes que podem usar o equipamento. Vazio significa todas.</summary>
    public IReadOnlyCollection<string> ClassesPermitidas => classesPermitidas;

    public bool PodeSerUsadoPor(Personagem personagem)
    {
        return classesPermitidas.Count == 0 || classesPermitidas.Contains(personagem.ClasseId);
    }

    public override ResultadoAcao Usar(Personagem alvo, Inventario inventario, Textos textos)
    {
        if (!PodeSerUsadoPor(alvo))
        {
            return ResultadoAcao.Falha(textos.Obter("item.classe_invalida", alvo.Nome, alvo.NomeClasse, Nome));
        }

        if (!inventario.Remover(this))
        {
            return ResultadoAcao.Falha(textos.Obter("item.nao_encontrado", Nome));
        }

        Equipamento? anterior = EquiparEm(alvo);
        if (anterior is null)
        {
            return ResultadoAcao.Ok(textos.Obter("item.equipado", alvo.Nome, Nome));
        }

        inventario.Adicionar(anterior);
        return ResultadoAcao.Ok(textos.Obter("item.equipado_troca", alvo.Nome, Nome, anterior.Nome));
    }

    /// <summary>Coloca o equipamento no espaço correto e devolve o que estava equipado antes.</summary>
    protected abstract Equipamento? EquiparEm(Personagem alvo);
}
