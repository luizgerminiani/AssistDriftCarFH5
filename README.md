# AssistDriftCarFH5 (JSE)

Aplicativo Windows para assistência de drift no **Forza Horizon 5**. O JSE lê o controle físico por XInput e a telemetria `Data Out` do FH5, aplica correção por yaw/body slip e encaminha o resultado para um controle Xbox 360 virtual via ViGEmBus.

Não lê memória, não injeta DLL e não altera arquivos do jogo.

## Estado do projeto

- Receptor UDP FH5 nos formatos Sled (232 bytes) e Horizon Dash (324 bytes).
- Leitura e passagem completa de controle XInput.
- Saída Xbox 360 virtual via `Nefarius.ViGEm.Client`.
- Correção gyro + counter-steer, fade por slip, dois filtros e limitador de velocidade da direção.
- Gate automático fora da corrida, abaixo da velocidade mínima e em marcha à ré.
- Interface WPF e configurações persistidas em `%APPDATA%\JSE\AssistDriftCarFH5\settings.json`.

## Requisitos

- Windows 10/11 x64.
- Controle compatível com XInput.
- [ViGEmBus](https://github.com/nefarius/ViGEmBus/releases) instalado.
- [HidHide](https://github.com/nefarius/HidHide/releases) recomendado para esconder o controle físico somente do FH5 e impedir entrada dupla. Adicione o `JSE.exe` à lista de aplicativos permitidos do HidHide antes de ativar o cloaking.

## Configurar o FH5

Em **Configurações → HUD e Jogabilidade**:

1. Ative `Saída de dados`.
2. Configure IP `127.0.0.1`.
3. Configure porta `20777`.
4. Use o formato `Dash` quando a opção estiver disponível.

Conecte o controle físico antes de abrir/conectar o JSE. Depois clique em **Conectar**.

## Parâmetros

| Parâmetro | Mínimo | Máximo | Padrão |
|---|---:|---:|---:|
| Gyro strength | 0.000 | 2.000 | 1.500 |
| Counter gain | 0.000 | 2.000 | 1.500 |
| Slip deadband | 0.010 | 0.300 | 0.025 |
| Input smoothing | 0.000 | 0.950 | 0.450 |
| Output smoothing | 0.000 | 0.950 | 0.000 |
| Assist fade range | 0.020 | 0.200 | 0.080 |
| Max steer rate | 0.500 | 5.000 | 3.500 |
| Min speed km/h | 5 | 80 | 20 |

## Compilar e gerar pacote

No PowerShell:

```powershell
.\build.ps1
```

O pacote autocontido é criado em `artifacts\JSE`. Para um pacote menor que exige o runtime .NET Core 3.1 instalado:

```powershell
.\build.ps1 -FrameworkDependent
```

## Avisos

Faça os primeiros testes em baixa velocidade. O sinal de yaw/steering pode variar com ferramentas intermediárias ou layouts incomuns; se a correção puxar para dentro do drift em vez de contraesterçar, desligue a assistência imediatamente. Uso online e compatibilidade com regras/anticheat são responsabilidade do usuário.

Consulte [docs/REA-ANALYSIS.md](docs/REA-ANALYSIS.md) para o levantamento do projeto FH6 público e as limitações do material analisado.
