# Week 3 - Extrator de Resumo de Áudio

Esta documentação detalha a implementação do sistema de transcrição e resumo automatizado para o projeto de gerenciamento de áudios.

## Tecnologias Utilizadas
- **Whisper.net:** Wrapper C# para o modelo Whisper da OpenAI (baseado em C++ / ggml).
- **FFmpeg:** Responsável pela normalização do áudio antes da inferência.
- **Threading (Task Parallel Library):** Para processamento assíncrono em background.

## Fluxo de Processamento
O sistema segue um modelo de "Fogo e Esquecimento" (Fire and Forget) para otimizar a latência da API:

1. **Recepção:** O áudio é recebido via `IFormFile`.
2. **Persistência Inicial:** O registro é criado no Banco de Dados com status "Processando...".
3. **Background Task:** Uma nova Task é disparada para:
    - Converter o áudio para **WAV 16kHz Mono** (requisito da IA).
    - Executar a inferência local com o modelo `GgmlType.Base`.
    - Limitar o output aos primeiros **50 caracteres**.
    - Atualizar o registro no PostgreSQL via `Update`.
4. **Logs de Performance:** Cada etapa (Conversão, IA, DB) é cronometrada e logada no console.

