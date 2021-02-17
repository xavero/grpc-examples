
# Exemplo de grpc com React

Dependências importantes:
- grpc-web
- google-protobuf

# Gerando o boilerplat do client no Windows

```powershell
.\generate-protobuf-files.ps1
```

Configuração do powershell:

- $proto_import_location
    Pasta que contém o arquivo .proto
- $protofile 
    Caminho completo do arquivo .proto

# Importando o client no React

```jsx
import { GreeterClient } from './grpc/greet_grpc_web_pb';
import { HelloRequest, HelloReply } from './grpc/greet_pb';

const grpcClient = new GreeterClient('https://localhost:5001', null, null);
```