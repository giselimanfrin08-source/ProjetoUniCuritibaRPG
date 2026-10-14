# Crônicas do Vale Cinzento — RPG em modo texto

RPG em modo texto para console, desenvolvido em C# (.NET 8) como trabalho da disciplina de Programação Orientada a Objetos (Proposta 3).

**Equipe:** Giseli e Lua
**Versão:** 1ª versão (protótipo)

## Como executar

**Visual Studio 2022:** abra o arquivo `RpgTexto.sln` e pressione `F5`.

**Linha de comando** (requer o [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)):

```bash
dotnet run --project RpgTexto
```

Os arquivos de dados ficam na pasta `RpgTexto/Dados` e são copiados automaticamente para a pasta do executável na compilação. O `save.txt` é criado na pasta do executável.

## Funcionalidades

- Criação de um grupo de 4 personagens, escolhendo entre 5 classes (Mago, Paladino, Bárbaro, Necromante e Arqueiro), cada uma com 3 ataques especiais.
- Mapa com 3 cidades (cada uma com INN, loja e taverna), 5 campos abertos e 2 cavernas.
- Batalha por turnos, com ordem definida pela velocidade e ações de atacar, defender, especial, usar poção e fugir.
- Loja com poções, armas e armaduras. Equipamentos dão bônus nos atributos e são limitados por classe.
- Inventário compartilhado pelo grupo.
- 6 quests (batalhas em campos e cavernas e visitas a outras cidades), aceitas e entregues nas tavernas.
- Busca de tesouros nas cavernas.
- Experiência e subida de nível, com crescimento de atributos diferente para cada classe.
- Save criptografado (AES) gravado no INN e carregado pela tela inicial.

## Arquitetura (MVC)

```
RpgTexto/
├── Program.cs                 Ponto de entrada
├── Models/                    Regras e dados do jogo
│   ├── Cenarios/              Cenario (abstrata) → Cidade, AreaHostil (abstrata) → Campo, Caverna
│   │                          Inn, Loja, Taverna, Mundo
│   ├── Personagens/           Combatente (abstrata) → Personagem (abstrata) → Mago, Paladino, Barbaro,
│   │                          Necromante, Arqueiro; Combatente → Monstro; Grupo, Especial, DadosClasse
│   ├── Itens/                 Item (abstrata) → Pocao, Equipamento (abstrata) → Arma, Armadura; Inventario
│   ├── Sistema/               Jogo, Batalha, Quest
│   └── Persistencia/          LeitorArquivos (parse dos .txt), GerenciadorSave (criptografia), ConversorSave
├── Views/                     Somente entrada e saída no console
│   └── ConsoleView → BatalhaView, ItemView
├── Controllers/               Ligam as Views aos Models
│   └── JogoController, BatalhaController, CidadeController, LojaController, InventarioController
└── Dados/                     items.txt, monstros.txt, classes.txt, quests.txt, textos.txt
```

### Conceitos de orientação a objetos aplicados

- **Abstração:** `Cenario`, `AreaHostil`, `Combatente`, `Personagem`, `Item` e `Equipamento` são classes abstratas.
- **Herança:** os cenários, os personagens, os monstros e os itens são organizados em hierarquias.
- **Polimorfismo:** `ObterOpcoes()` é implementado por cada cenário; `CalcularPoderEspecial()` e `SubirNivel()` por cada classe jogável; `Usar()` por cada tipo de item (a poção recupera atributos e o equipamento é equipado); o `Barbaro` sobrescreve `CalcularDanoFisico()` (fúria).
- **Encapsulamento:** atributos privados com propriedades que validam os valores (a vida e a mana nunca ficam negativas nem passam do máximo, e não é possível gastar mais dinheiro do que o grupo tem).

## Arquivos de dados

Todos usam `;` como separador. Linhas iniciadas com `#` são comentários. Linhas com erro são ignoradas e listadas como aviso ao abrir o jogo.

| Arquivo | Conteúdo | Formato |
| --- | --- | --- |
| `items.txt` | Poções, armas e armaduras | `POCAO;id;nome;preco;vida;mana;descricao`<br>`ARMA;id;nome;preco;ataque;classes;descricao`<br>`ARMADURA;id;nome;preco;defesa;velocidade;classes;descricao` |
| `monstros.txt` | Inimigos | `id;nome;vida;ataque;defesa;velocidade;xp;ouro;areas` |
| `classes.txt` | Classes e especiais | `CLASSE;id;nome;vida;mana;ataque;defesa;velocidade;descricao`<br>`ESPECIAL;classe;nome;custoMana;poder;tipo;descricao` |
| `quests.txt` | Quests das tavernas | `id;taverna;tipo;alvo;quantidade;xp;ouro;itemRecompensa` |
| `textos.txt` | Diálogos, descrições e mensagens | `chave=texto` |

Para adicionar um item, um monstro, um golpe especial ou uma quest, basta incluir uma nova linha no arquivo correspondente, sem alterar o código.

## Save

O progresso é salvo no INN, no arquivo `save.txt`. O conteúdo é serializado em JSON e criptografado com AES (`System.Security.Cryptography`). A chave é derivada com PBKDF2 a partir de um salt aleatório. Se o arquivo for alterado manualmente, o jogo recusa o carregamento.
