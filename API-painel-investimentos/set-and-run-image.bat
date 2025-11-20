@echo off

echo Criando volume e configurando banco de dados SQLite...

REM 1. Criar o volume Docker
echo Criando volume api_painel_investimentos_volume...
docker volume create api_painel_investimentos_volume

REM 2. Verificar se existe um arquivo de banco inicial para copiar
if exist "painel-investimentos.db" (
    echo Arquivo painel-investimentos.db encontrado. Copiando para o volume...
    docker run --rm -v %CD%:/origem -v api_painel_investimentos_volume:/destino alpine cp /origem/painel-investimentos.db /destino/
) else (
    echo Arquivo painel-investimentos.db nao encontrado. O banco sera criado automaticamente pela aplicacao.
)

REM 3. Verificar se a imagem Docker já existe
echo Verificando se a imagem apipainelinvestimentosimg:1.0 já existe...
docker images --filter reference="apipainelinvestimentosimg:1.0" | findstr "apipainelinvestimentosimg" > nul
if %errorlevel% equ 0 (
    echo Imagem apipainelinvestimentosimg:1.0 encontrada. Pulando construcao.
) else (
    echo Construindo a imagem Docker...
    docker build --no-cache -t apipainelinvestimentosimg:1.0 .
)

REM 4. Verificar se o container já existe
echo Verificando se o container api-painel-investimentos-ctn já existe...
docker ps -a --filter "name=api-painel-investimentos-ctn" | findstr "api-painel-investimentos-ctn" > nul
if %errorlevel% equ 0 (
    echo Container api-painel-investimentos-ctn encontrado. Reiniciando...
    
    REM Parar e remover o container existente
    docker stop api-painel-investimentos-ctn > nul 2>&1
    docker rm api-painel-investimentos-ctn > nul 2>&1
    echo Container antigo removido.
) else (
    echo Container api-painel-investimentos-ctn nao encontrado. Criando novo...
)

REM 5. Executar o container com o volume montado
echo Iniciando container com volume...
docker run -d ^
  -p 8080:8080 ^
  -p 8081:8081 ^
  -v api_painel_investimentos_volume:/app/data ^
  --name api-painel-investimentos-ctn ^
  -e ASPNETCORE_ENVIRONMENT=Development ^
  -e ConnectionStrings__SqliteConnection="Data Source=/app/data/painel-investimentos.db" ^
  apipainelinvestimentosimg:1.0

echo.
echo Container executado com sucesso!
echo Volume: api_painel_investimentos_volume
echo Banco de dados: /app/data/painel-investimentos.db
echo Aplicacao disponivel em: http://localhost:8080
echo.
echo Para ver os logs: docker logs simulador-app
echo Para parar: docker stop simulador-app
echo Para remover: docker rm simulador-app
pause