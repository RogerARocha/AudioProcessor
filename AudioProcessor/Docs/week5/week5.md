# Week 5 - Audio Processor

## 1. Visão geral

O sistema utiliza um modelo de processamento assíncrono, para garantir performance e escalabilidade. Os componentes
tem responsabilidades bem definidas facilitando a manuntenção.

## 2. Threading

A técnica foi aplicada via `Task.Run` para desacoplar a resposta da API do processamento de IA da transcrição.

- Eficiência de Recursos: As tarefas de conversão e transcrição (CPU intensivas) são delegadas ao Thread Pool do .NET, permitindo que a aplicação continue atendendo a outras requisições simultaneamente.

- Sincronização de Dados: O sistema utiliza um status de "Processing..." no banco de dados, que é atualizado via Update assim que a thread de background conclui a análise.

A implementação pode ser encontrada em :
[`AudioService.cs:88-125`](https://gitlab.com/jala-university1/cohort-1/PT.CSPR-471.GA.T1.26.M1/SA/roger-alves-da-rocha/audioprocessor/-/blob/master/Services/AudioService.cs#L88-L125)

## 3. Extrator de Áudio

- Modelo de IA: Utiliza o modelo Whisper (GgmlType.Base) para uma transcrição rápida e leve.

- Limite de Caracteres: O resumo gerado é limitado aos primeiros 50 caracteres para visualização rápida na tabela.

- Normalização de Áudio: O FFmpeg converte o arquivo original para o formato exigido pelo Whisper: 16kHz, Mono, PCM 16-bit.

- Monitoramento: Logs de tempo granulares via `Stopwatch` medem a eficiência de cada etapa (Upload, Conversão e IA).

A implementação pode ser encontrada em:
[`AudioTranscription.cs`](https://gitlab.com/jala-university1/cohort-1/PT.CSPR-471.GA.T1.26.M1/SA/roger-alves-da-rocha/audioprocessor/-/blob/master/Services/AudioTranscription.cs)