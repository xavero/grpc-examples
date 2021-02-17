---
marp: true
theme: dracula
style: |
  table {
    width: 100%;
  }
headingDivider: 1
---

![bg left:40% 80%](https://grpc.io/img/logos/grpc-logo.png)

A high performance, open source universal RPC framework


# Introdução #

* Framework para comunicação entre aplicações
* Desenvolvido pelo Google, e agora parte da CNCF
* União de HTTP/2 e Protocol buffers
* Foco em peformance e produtividade
* Multiplas plataformas e linguagens

<!-- 
  Muito comum entre microserviços
	
  Aplicações que trabalham em cluster

  Desenvolvido em 2015, mas baseado em outra tecnologia amplamenta usada dentro do Google

 - Peformance: Mensagens binárias compactas, serialização rápida
 
 - Produtividade: Ferramentas que geram o código do client e server, esconde a complexidade da comunicação.

  Outros frameworks RPC: WCF, Thrift, JSON-RPC
-->


# gRPC vs REST #

| gRPC                        | REST                             |
| :--------------------------:|:--------------------------------:|
| "Contract First" (proto)    | Uri, Verbo, Headers, Status Code |
| Conteúdo para computadores  | Conteúdo para pessoas            |
| 4 tipos de operações        | 1 tipo de operação               |
| Suporte limitado no browser | Suporte nativo no browser        |

<!-- 
 
 - Tipos de operações:
    Unary
    Client Streaming
    Server Streaming
    Full duplex (Bidirecional Streaming)

  - Suporte no browser para Unary e Server Streaming apenas
-->


# Protocol buffer (.proto) #

```proto
syntax = "proto3";

package greet;

service Greeter {
  rpc SayHello (HelloRequest) returns (HelloReply);
}

message HelloRequest {
  string name = 1;
}

message HelloReply {
  string message = 1;
}
```

<!-- 
  Arquivo .proto precisa ser conhecido pelo client e server
-->


# Gerando Códigos Client e Server #

- Exemplo de geração de código com ferramentas Python
```bash
pip install grpcio-tools

python -m grpc_tools.protoc -I../../protos \
  --python_out=. \
  --grpc_python_out=. ../../protos/greet.proto
```

<!-- 
  Com exceção do .net (e node?), todas as linguagens usam o protoc para 
  gerar os códigos boilerplate (diretamente ou indiretamente como no Java/Maven)
-->

# <!-- fit --> Demo Time #

# Links Úteis #

- https://grpc.io/docs/what-is-grpc/core-concepts/
- https://aka.ms/grpcdocs
- https://aka.ms/grpcexamples
- https://docs.microsoft.com/en-gb/aspnet/core/grpc/protobuf
- https://github.com/grpc-ecosystem/awesome-grpc
- Demo com o James Newton-King: 
  https://www.youtube.com/watch?v=EJ8M2Em5Zzc
  http://grpcblazorperf.azurewebsites.net/
  