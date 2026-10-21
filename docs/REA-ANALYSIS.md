# Levantamento do FH6 Drift Assist com REA

Data: 21/10/2026

## Material analisado

- Site: `https://l3g3nd2410.github.io/fh6-drift-assist/`
- Repositório público: `https://github.com/l3g3nd2410/fh6-drift-assist`
- Clone local analisado com `rea-agents analyze-javascript-application`.
- Evidence ID do REA: `ev_b3a803274024dd9ffb5f6706f2518012651ecd246c8cf85f01e9ee239bec2b41`.

## Resultado verificável

O repositório público contém apenas o site/preview. O REA encontrou um artefato relevante (`index.html`, 63.572 bytes) e nenhum módulo JavaScript separado, boundary Electron, IPC, preload, native add-on ou binário. O script inline do preview simula valores e interações visuais; ele não contém o processamento real do controle/telemetria.

O próprio resultado do REA marcou a cobertura como parcial porque o analisador não produziu IR semântico para o script inline. Por isso, este projeto **não afirma ser uma cópia byte a byte** do algoritmo fechado do FH6.

## Comportamentos públicos reproduzidos

- Entrada do controle físico.
- Telemetria UDP do Forza.
- Correção composta por yaw e slip.
- Fade depois do deadband, smoothing e rate limit.
- Controle Xbox 360 virtual por ViGEm.
- Desativação abaixo da velocidade mínima, fora da corrida e durante ré.
- Oito parâmetros e faixas fornecidos para este projeto.

## Adaptação específica para FH5

O parser usa o pacote Horizon Dash de 324 bytes: prefixo Sled de 232 bytes, lacuna de 12 bytes e bloco Dash começando no byte 244. O yaw rate vem do eixo Y da velocidade angular (offset 48), a velocidade local dos offsets 32/40, e a velocidade Dash do offset 256. O body slip é calculado por `atan2(velocityX, velocityZ)`.

Como o executável original não foi fornecido no repositório/site público, engenharia reversa nativa do algoritmo original não foi possível. Para uma comparação mais profunda seria necessário um instalador/binário obtido legitimamente pelo usuário e autorização para analisá-lo.
