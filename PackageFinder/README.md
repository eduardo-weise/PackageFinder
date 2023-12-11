# PackageFinder

PackageFinder é um projeto em C# que tem como objetivo buscar informações sobre pacotes NuGet e verificar se eles têm alguma vulnerabilidade conhecida.

## O que é PackageFinder?

PackageFinder é um aplicativo de console que lê uma lista de pacotes NuGet de um arquivo de texto e, para cada pacote, obtém informações do pacote e verifica se ele tem alguma vulnerabilidade conhecida. As informações do pacote são obtidas da API do NuGet e as vulnerabilidades são verificadas usando a API do OSS Index.

## Como usar o PackageFinder?

Para usar o PackageFinder, você precisa criar um arquivo de texto com uma lista de pacotes NuGet. Cada linha do arquivo deve conter um pacote no formato "NomeDoPacote@Versão". Por exemplo:

```
Microsoft.EntityFrameworkCore.Design@2.2.3
Microsoft.EntityFrameworkCore.Design@2.2.6
```

Depois de criar o arquivo de texto, você pode executar o PackageFinder e ele imprimirá informações sobre cada pacote e quaisquer vulnerabilidades conhecidas.

## O que PackageFinder faz?

PackageFinder faz o seguinte:

- Lê a lista de pacotes de um arquivo de texto.
- Para cada pacote, faz uma solicitação à API do NuGet para obter informações sobre o pacote.
- Para cada pacote, faz uma solicitação à API do OSS Index para verificar se o pacote tem alguma vulnerabilidade conhecida.
- Imprime informações sobre cada pacote e quaisquer vulnerabilidades conhecidas.

## Agradecimentos

Este projeto foi possível graças às seguintes bibliotecas e serviços:

- [NuGet API](https://docs.microsoft.com/en-us/nuget/api/overview)
- [OSS Index API](https://ossindex.sonatype.org/api)