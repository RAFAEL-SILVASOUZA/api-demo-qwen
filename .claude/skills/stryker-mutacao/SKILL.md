---
name: stryker-mutacao
description:
  Instala e executa o Stryker.NET (mutation testing) em soluções .NET. Cria a
  configuração, roda os testes de mutação, analisa os mutants sobreviventes e
  sugere correções nos testes para aumentar o score.
---

# Skill: Testes de Mutação com Stryker.NET

Você é um agente especializado em **mutation testing** usando o **Stryker.NET**.
Sua tarefa é instalar, configurar, executar e analisar os resultados do Stryker
em soluções .NET (geralmente .NET 8+ com xUnit).

## Entrada esperada

O usuário pede para rodar testes de mutação com Stryker, por exemplo:
- "Rode o Stryker no projeto"
- "Faça mutation testing"
- "Instale e configure o Stryker.NET"

Se já houver um `stryker-config.json`, use-o. Se não, crie um novo.

## Procedimento

### 1. Instalar o Stryker.NET CLI

```powershell
dotnet tool install -g dotnet-stryker
```

Se a ferramenta já estiver instalada, o comando falha silenciosamente — continue.

### 2. Verificar se já existe configuração

Procure por `stryker-config.json` na raiz do projeto (diretório de trabalho).
Se existir, leia-o e decida se precisa ajustar. Se não existir, crie um novo.

### 3. Criar/ajustar o stryker-config.json

Crie ou sobrescreva `stryker-config.json` na raiz do projeto com:

```json
{
  "stryker-config": {
    "project-info": {
      "name": "<nome-do-projeto>",
      "version": ""
    },
    "mutation-level": "Standard",
    "mutate": [
      "**/Services/**/*.cs"
    ],
    "coverage-analysis": "perTest",
    "thresholds": {
      "high": 80,
      "low": 60,
      "break": 0
    },
    "verbosity": "info",
    "reporters": [
      "Progress",
      "Html"
    ],
    "since": {
      "enabled": false,
      "ignore-changes-in": [],
      "target": "master"
    },
    "break-on-initial-test-failure": false
  }
}
```

Regras para o campo `mutate`:
- Se o projeto tiver diretório `Services/`, use `**/Services/**/*.cs` (foco nos services — maior ROI).
- Se não houver Services, use `**/*.cs` para abranger tudo.
- Se já existir um `stryker-config.json` com configurações relevantes, preserve-as.

Regra: **NUNCA deixe o campo `module` vazio** — se não for usar, remova-o do JSON.

### 4. Executar o Stryker

```powershell
dotnet stryker
```

Timeout: aguarde até 5 minutos. O Stryker compila a solução, roda os testes, cria
mutantes e os testa um a um.

### 5. Analisar os resultados

O Stryker gera um relatório HTML em:
`StrykerOutput/<timestamp>/reports/mutation-report.html`

Extraia os mutants sobreviventes usando Node.js (o JSON embutido no HTML usa
unicode escapes que o PowerShell tem dificuldade):

```javascript
// Crie temporariamente: extract-survived.js
const fs = require('fs');
const html = fs.readFileSync('StrykerOutput/<timestamp>/reports/mutation-report.html', 'utf8');
const lines = html.split('\n');
let startLine = -1;
for (let i = 330; i < lines.length; i++) {
  if (lines[i].includes('app.report')) { startLine = i; break; }
}
if (startLine === -1) { console.log('No report found'); process.exit(1); }
let depth = 0; let json = '';
for (let i = startLine; i < lines.length; i++) {
  for (const ch of lines[i]) {
    if (ch === '{') depth++;
    else if (ch === '}') depth--;
    json += ch;
    if (depth === 0 && json.includes('}')) break;
  }
  if (depth === 0) break;
}
const firstBrace = json.indexOf('{');
if (firstBrace > 0) json = json.substring(firstBrace);
const report = JSON.parse(json);

// Detectar o prefixo do projeto a partir dos caminhos dos arquivos
// Remove tudo até "src/<pasta>/", deixando apenas o caminho relativo
const sampleFile = Object.keys(report.files).length > 0 ? Object.keys(report.files)[0] : '';
const srcMatch = sampleFile.match(/(.*[\\/])src[\\/][^\\/[\\/]+[\\/]/);
const projectRoot = srcMatch ? srcMatch[1] + 'src/' : '';

let survived = [];
for (const [file, fileData] of Object.entries(report.files)) {
  if (fileData.mutants) {
    fileData.mutants.forEach(m => {
      if (m.status === 'Survived') {
        const cleanFile = projectRoot ? file.replace(projectRoot, '') : file;
        survived.push({
          file: cleanFile,
          line: m.location.start.line,
          col: m.location.start.column,
          mutator: m.mutatorName,
          replacement: m.replacement
        });
      }
    });
  }
}
console.log(JSON.stringify(survived, null, 2));
```

Execute: `node extract-survived.js`

### 6. Interpretar cada mutant sobrevivente

Para cada mutant sobrevivente encontrado:

1. **Leia o arquivo** na linha indicada para ver o contexto.
2. **Identifique o método** onde a mutação ocorreu.
3. **Identifique o tipo de mutação**:
   - `Statement mutation` (replacement: `;`) — uma linha foi substituída por ponto e vírgula vazio. Isso significa que o teste não verifica se aquela operação realmente aconteceu no banco/estado.
   - `Conditional (true/false) mutation` — o teste não cobre um dos ramos condicionais.
   - `Equality mutation` — o teste não verifica o caso oposto da comparação.
   - `Boolean mutation` — o teste não cobre o valor invertido do booleano.
4. **Identifique o teste que cobre** (olhe o campo `coveredBy` no JSON do Stryker).
5. **Explique por que sobreviveu**: geralmente porque o teste verifica apenas o
   retorno do método, mas não a persistência/estado real no banco de dados.

### 7. Apresentar os resultados

Mostre ao usuário:

**Resumo:**
- Score de mutação (percentual)
- Total de mutants testados
- Killed / Survived / Timeout / Ignored

**Mutants sobreviventes (um por um):**

```
### Mutant #N — `<arquivo>:<linha>`
- **Método:** `<nomeDoMetodo>`
- **Mutação:** `<tipo>` — `<descrição>`
- **Teste que cobre:** `<nome do teste>`
- **Por que sobreviveu:** `<explicação>`
```

**Score final:**
- >= 80% (high): Excelente
- 60-79%: Aceitável, mas pode melhorar
- < 60% (low): Precisa de mais testes

### 8. Aplicar correções nos testes (ciclo iterativo)

Para CADA mutant sobrevivente, **leia o arquivo de teste correspondente**, identifique
o teste que cobre o mutante e **edite o arquivo de teste** para matá-lo.

#### Fluxo de mitigação (para CADA mutant sobrevivente):

1. **Mapeie o `coveredBy` ID para o nome do teste** usando a seção `testFiles`
   do JSON do relatório Stryker (chave `tests[].id` → `tests[].name`).
2. **Localize o arquivo de teste** que contém esse teste (use o namespace/prefixo
   do nome do teste para encontrar o arquivo `.cs` correspondente).
3. **Leia o método de teste completo** no arquivo.
4. **Identifique a correção** baseada no tipo de mutação (ver abaixo).
5. **Edite o arquivo de teste** com a correção usando a ferramenta `edit`.
6. **Rode os testes** para confirmar que passaram: `dotnet test`.
7. **Rode o Stryker novamente** (`dotnet stryker`) para verificar se o mutante foi morto.
8. Se o mutante ainda sobreviver, **reavalie a correção**:
   - Verifique se o EF Core InMemory está retornando objetos em memória (change tracking) ao invés do banco.
   - Use `AsNoTracking()` na consulta de verificação para forçar leitura do banco.
   - Ou crie um novo contexto/DbContext isolado para a verificação.
   - Edite e repita os passos 6-8.
9. Repita o fluxo completo para cada mutant sobrevivente até que **todos sejam mortos** ou o score atinja >= 80% sem sobreviventes.

#### Correções por tipo de mutação:

**Statement mutation em `SaveChangesAsync` / `Add` / `Remove`:**
O teste verifica o DTO retornado mas não a persistência no banco. Adicione verificação
direta no `DbContext`:

```csharp
// No teste X_DeveAtualizarCampos, após chamar o serviço:
var atualizadoNoBanco = await context.X.AsNoTracking().FirstAsync(c => c.Id == id);
Assert.Equal(valorEsperado, atualizadoNoBanco.Campo);
```

> **Importante com EF Core InMemory:** Use `AsNoTracking()` na consulta de verificação.
> Sem isso, o EF Core InMemory retorna o objeto já alterado em memória (change tracking),
> então a mutação que remove `SaveChangesAsync` ainda passa no teste porque o objeto
> rastreado pelo contexto já tem os valores atualizados.

**Statement mutation em método que retorna valor (ex.: `return true`):**
O teste não verifica o valor de retorno real. Adicione `Assert`:

```csharp
// No teste X_DeveRemover:
var resultado = await service.RemoverAsync(id);
Assert.True(resultado); // se foi removido com sucesso
```

**Conditional mutation (true/false):**
O teste cobre apenas um ramo. Adicione um teste para o caso oposto:

```csharp
[Fact]
public async Task Metodo_DeveRetornarNull_QuandoNaoEncontrado()
{
    // Arrange: contexto vazio ou sem o registro
    // Act: chama o método com ID inexistente
    // Assert: Assert.Null(resultado)
}
```

**Equality mutation:**
O teste não verifica o caso oposto da comparação. Adicione um teste para o caso
invertido (ex.: se testa `==`, adicione teste para `!=` ou valores diferentes).

**Boolean mutation:**
O teste não cobre o valor invertido do booleano. Adicione um teste para o caso
oposto.

**Null/Nothing mutation:**
O teste não verifica o comportamento quando o valor é null. Adicione um teste
para o caso null.

### 9. Limpeza

Remova os arquivos temporários `extract-survived.js` e `find-tests.js` após usar.

## Regras importantes

- **NUNCA altere o código fonte do projeto** (Services, Controllers, Models, etc.) —
  apenas edite os arquivos de teste (`*Tests.cs`).
- Se o score for >= 80% e **não houver mutants sobreviventes**, parabene o usuário e finalize.
- Se houver mutants sobreviventes, explique cada um e **aplique correções nos testes**
  (não apenas sugira). Rode `dotnet test` após cada alteração para validar.
- O campo `module` no config **nunca deve ficar vazio** — remova-o se não usar.
- Use sempre `dotnet stryker` (não `dotnet-stryker`) para rodar.
- Se o build falhar antes do Stryker rodar, corrija os erros de build primeiro.
- Se testes existentes falharem durante o Stryker, reporte e pare.
