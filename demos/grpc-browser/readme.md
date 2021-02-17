# Exemplo de Grpc no browser, usando proxy #

```powershell

npm install grpc-web google-protobuf webpack webpack-cli webpack-dev-server --save-dev

# protoc: gera arquivos boilerplate para o client e server grpc
Invoke-WebRequest -Uri https://github.com/protocolbuffers/protobuf/releases/download/v3.14.0/protoc-3.14.0-win64.zip `
    -OutFile protoc-win64.zip

Expand-Archive -Path protoc-win64.zip

# plugin do protoc para gerar arquivos javascript para o browser
Invoke-WebRequest -Uri https://github.com/grpc/grpc-web/releases/download/1.2.1/protoc-gen-grpc-web-1.2.1-windows-x86_64.exe `
    -OutFile protoc-gen-grpc-web.exe

# gerar boilerplate do client gRPC javascript no browser
.\protoc-win64\bin\protoc.exe -I="..\net5\GrpcDemo\GrpcServer\Protos" `
    "..\net5\GrpcDemo\GrpcServer\Protos\greet.proto" `
    --js_out=import_style=commonjs:.\src\grpc `
    --grpc-web_out=import_style=commonjs+dts,mode=grpcweb:.\src\grpc

npm start

```

### Referências ###

- https://github.com/grpc/grpc-web