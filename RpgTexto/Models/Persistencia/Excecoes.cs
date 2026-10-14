namespace RpgTexto.Models.Persistencia;

/// <summary>Um arquivo de dados obrigatório não existe ou não pôde ser lido.</summary>
public class ArquivoDadosException : Exception
{
    public ArquivoDadosException(string mensagem, Exception? interna = null) : base(mensagem, interna)
    {
    }
}

/// <summary>O save.txt está corrompido, foi alterado manualmente ou é de outra versão.</summary>
public class SaveInvalidoException : Exception
{
    public SaveInvalidoException(string mensagem, Exception? interna = null) : base(mensagem, interna)
    {
    }
}
