using RpgTexto.Models.Personagens;
using RpgTexto.Models.Sistema;

namespace RpgTexto.Models.Persistencia;

/// <summary>
/// Tudo o que é gravado no save.txt (antes de ser criptografado).
/// </summary>
public class DadosSave
{
    public int Versao { get; set; } = 1;

    public DateTime DataHora { get; set; }

    public string CenarioAtual { get; set; } = string.Empty;

    public List<EstadoPersonagem> Personagens { get; set; } = new();

    public List<string> Inventario { get; set; } = new();

    public List<EstadoQuest> Quests { get; set; } = new();

    public List<string> TesourosColetados { get; set; } = new();
}

public class EstadoQuest
{
    public string Id { get; set; } = string.Empty;

    public StatusQuest Status { get; set; }

    public int Progresso { get; set; }
}
