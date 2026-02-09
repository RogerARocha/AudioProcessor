# AudioProcessor – Week 2 Compressão AAC

## Escopo

- Comprimir cada áudio carregado para AAC.
- Salvar o áudio comprimido no File Store (Supabase) em vez do original.
- Adicionar logs para medir o tempo de compressão.
- Analisar se é o momento certo para adicionar threading.

## Compressão para AAC

- A compressão está centralizada em `AudioProcessor.Services.AudioCompressorService`.
- O método responsável gera um arquivo de saída no formato `.aac`, usando Xabe.FFmpeg com os parâmetros `-c:a aac -b:a 128k`.
- O nome do arquivo de saída segue o padrão `{taskId}.aac`, armazenado em um diretório temporário (`_tempPath`).

## Armazenamento no File Store (Supabase)

- O upload para o File Store é feito em `Services/AudioService.cs`, método `Upload`. 
- Para cada arquivo recebido:
    - É gerado um `Guid` (`uuid`) para nomear o arquivo no bucket.
    - Se o arquivo original não estiver em AAC, ele é primeiro comprimido via `AudioCompressorService`, e os bytes resultantes são lidos do arquivo `.aac` gerado.
    - Em seguida, é chamado `_supabaseClient.Storage.From(bucket).Upload(fileBytes, nameInBucket);`, enviando os bytes já comprimidos em AAC diretamente para o Supabase.
    - A URL pública do arquivo é obtida com `GetPublicUrl(nameInBucket)` e armazenada em `AudioData.FileUrl` através do `AudioRepository`.

## Logs e medição de tempo

- A classe `AudioCompressorService` utiliza `System.Diagnostics.Stopwatch` para medir o tempo de compressão.
- O cronômetro é iniciado antes da chamada à conversão (`FFmpeg.Conversions.New()...Start()`), e interrompido após a conclusão.
- São escritos logs em `Console.WriteLine` com:
    - Mensagem de conclusão da conversão para o `taskId`.
    - Tempo total em milissegundos (`Time Elapsed: {stopwatch.ElapsedMilliseconds} ms`).
- Em caso de exceção, o cronômetro também é interrompido, e é logada mensagem com o tempo até o erro e o texto da exceção.

Requisito de logs de tempo de compressão em nível de console.  


## Threading – análise necessidade

### Situação atual

- O processamento é assíncrono, mas não há uso explícito de filas ou workers dedicados de background.
- O fluxo principal:
    - Recebe o arquivo via `AudioService.Upload`.
    - Se necessário, chama o `AudioCompressorService` para gerar o `.aac`.
    - Faz o upload para o Supabase e persiste metadados no repositório. 

### Considerando threading / background processing

Pontos a favor de adiar threading por enquanto:

- A funcionalidade crítica (compressão em AAC + upload para Supabase + logs de tempo) já está implementada de forma clara e sequencial.
- Antecipar filas e múltiplos workers pode aumentar a complexidade de debug enquanto a API ainda está consolidando contratos e integrações externas.

Pontos a favor de introduzir threading depois:

- Quando houver necessidade de:
    - Processar muitos áudios em paralelo sem bloquear requisições HTTP.
    - Escalar o processamento em segundo plano.
- A medição de tempo atual servirá de base para decidir o grau de paralelismo e dimensionamento de workers.

**Conclusão:** neste momento, threading avançado pode ser planejado como próximo passo, mas a decisão mais segura é consolidar a estabilidade da compressão, upload ao Supabase e observabilidade antes de introduzir filas/background workers.