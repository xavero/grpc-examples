
$grptools_path = '.\tmp';

$protoc_version = '3.14.0';
$protoc_grpcweb_version = '1.2.1';
$proto_import_location = '..\net5\GrpcDemo\GrpcServer\Protos';
$protofile = '..\net5\GrpcDemo\GrpcServer\Protos\greet.proto';

$output_dir = '.\src\grpc'

if((Test-Path $grptools_path) -eq $false)
{
    mkdir $grptools_path
}

if((Test-Path "$grptools_path\bin\protoc.exe") -eq $false) {

    Write-Output "$grptools_path\bin\protoc.exe does not exists"

    # protoc: gera arquivos boilerplate para o client grpc
    Invoke-WebRequest -Uri "https://github.com/protocolbuffers/protobuf/releases/download/v$protoc_version/protoc-$protoc_version-win64.zip" `
        -OutFile "$grptools_path\protoc-win64.zip"

    Expand-Archive -Path "$grptools_path\protoc-win64.zip" -DestinationPath "$grptools_path"

    Remove-Item "$grptools_path\protoc-win64.zip"
}

if((Test-Path "$grptools_path\bin\protoc-gen-grpc-web.exe") -eq $false) {

    Write-Output "$grptools_path\bin\protoc-gen-grpc-web.exe does not exists"

    # plugin do protoc para gerar arquivos javascript para o browser
    Invoke-WebRequest -Uri "https://github.com/grpc/grpc-web/releases/download/$protoc_grpcweb_version/protoc-gen-grpc-web-$protoc_grpcweb_version-windows-x86_64.exe" `
        -OutFile "$grptools_path\bin\protoc-gen-grpc-web.exe"
}

if((Test-Path $output_dir) -eq $false)
{
    mkdir $output_dir
}


Copy-Item "$grptools_path\bin\protoc-gen-grpc-web.exe" .\protoc-gen-grpc-web.exe

# gerar boilerplate do client gRPC javascript no browser
. "$grptools_path\bin\protoc.exe" -I="$proto_import_location" `
    "$protofile" `
    --js_out=import_style=commonjs:"$output_dir" `
    --grpc-web_out=import_style=commonjs+dts,mode=grpcweb:"$output_dir"

Remove-Item .\protoc-gen-grpc-web.exe
