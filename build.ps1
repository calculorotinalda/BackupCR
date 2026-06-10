$ErrorActionPreference = "Continue"
$logFile = "log.txt"
"=========================================" | Out-File $logFile -Encoding utf8
"Iniciando compilação do BackupCR..." | Out-File $logFile -Append -Encoding utf8
"Data/Hora: $(Get-Date)" | Out-File $logFile -Append -Encoding utf8
"=========================================" | Out-File $logFile -Append -Encoding utf8

try {
    # Verificar se o dotnet SDK está no PATH
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw "O SDK do .NET não foi localizado. Certifique-se de que o .NET 8 SDK está instalado e no seu PATH."
    }

    "Rodando comando: dotnet publish -c Release -r win-x64 --self-contained true" | Out-File $logFile -Append -Encoding utf8

    # Iniciar dotnet publish capturando stdout/stderr
    $p = Start-Process dotnet -ArgumentList "publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:PublishTrimmed=false -p:IncludeNativeLibrariesForSelfExtract=true" -NoNewWindow -PassThru -Wait -RedirectStandardOutput "stdout.txt" -RedirectStandardError "stderr.txt"

    $stdout = ""
    $stderr = ""
    if (Test-Path "stdout.txt") { $stdout = Get-Content "stdout.txt" -Raw }
    if (Test-Path "stderr.txt") { $stderr = Get-Content "stderr.txt" -Raw }

    "--- Retorno do compilador ---" | Out-File $logFile -Append -Encoding utf8
    $stdout | Out-File $logFile -Append -Encoding utf8

    if ($p.ExitCode -ne 0) {
        "--- ERROS DETECTADOS ---" | Out-File $logFile -Append -Encoding utf8
        $stderr | Out-File $logFile -Append -Encoding utf8
        "=========================================" | Out-File $logFile -Append -Encoding utf8
        "Compilação falhou com código de saída: $($p.ExitCode)" | Out-File $logFile -Append -Encoding utf8
        Write-Error "A compilação falhou. Veja o arquivo log.txt para obter detalhes."
        exit $p.ExitCode
    }

    # Criar pasta portable
    "Criando pasta de distribuição portátil (publish_portable)..." | Out-File $logFile -Append -Encoding utf8
    $portableDir = "publish_portable"
    if (-not (Test-Path $portableDir)) {
        New-Item -ItemType Directory -Path $portableDir | Out-Null
    }

    # Copiar o executável único e pastas de recursos
    Copy-Item "bin\Release\net8.0-windows\win-x64\publish\BackupCR.exe" -Destination "$portableDir\BackupCR.exe" -Force
    
    if (-not (Test-Path "$portableDir\icons")) { New-Item -ItemType Directory -Path "$portableDir\icons" | Out-Null }
    Copy-Item "icons\*" -Destination "$portableDir\icons" -Recurse -Force
    
    if (-not (Test-Path "$portableDir\wwwroot")) { New-Item -ItemType Directory -Path "$portableDir\wwwroot" | Out-Null }
    Copy-Item "wwwroot\*" -Destination "$portableDir\wwwroot" -Recurse -Force

    "=========================================" | Out-File $logFile -Append -Encoding utf8
    "Compilação concluída com SUCESSO!" | Out-File $logFile -Append -Encoding utf8
    "Executável Portable gerado em: $portableDir\BackupCR.exe" | Out-File $logFile -Append -Encoding utf8

    # Verificar e compilar com Inno Setup
    $iscc = Get-Command iscc -ErrorAction SilentlyContinue
    if (-not $iscc) {
        $isccPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
        if (Test-Path $isccPath) {
            $iscc = $isccPath
        }
    }

    if ($iscc) {
        "Inno Setup Compiler localizado ($iscc). Compilando instalador setup.iss..." | Out-File $logFile -Append -Encoding utf8
        $isccProcess = Start-Process $iscc -ArgumentList "setup.iss" -NoNewWindow -PassThru -Wait -RedirectStandardOutput "iscc_stdout.txt" -RedirectStandardError "iscc_stderr.txt"
        
        if ($isccProcess.ExitCode -eq 0) {
            "Instalador compilado com sucesso em: publish_installer\BackupCR_Setup.exe" | Out-File $logFile -Append -Encoding utf8
            Write-Output "Compilação e geração do instalador realizadas com sucesso!"
        } else {
            $isccError = Get-Content "iscc_stderr.txt" -Raw
            "Erro ao compilar instalador (Código $($isccProcess.ExitCode)): $isccError" | Out-File $logFile -Append -Encoding utf8
            Write-Warning "Executável compilado com sucesso, mas a geração do instalador falhou."
        }
    } else {
        "Inno Setup Compiler (ISCC.exe) não foi localizado. O instalador não foi gerado." | Out-File $logFile -Append -Encoding utf8
        Write-Output "Compilação concluída com sucesso! Executável portátil gerado em: $portableDir\BackupCR.exe"
    }

} catch {
    "--- ERRO EXCEPCIONAL ---" | Out-File $logFile -Append -Encoding utf8
    "$_" | Out-File $logFile -Append -Encoding utf8
    Write-Error "Ocorreu um erro no processo de compilação: $_"
} finally {
    # Remover arquivos temporários de redirecionamento
    if (Test-Path "stdout.txt") { Remove-Item "stdout.txt" -Force }
    if (Test-Path "stderr.txt") { Remove-Item "stderr.txt" -Force }
    if (Test-Path "iscc_stdout.txt") { Remove-Item "iscc_stdout.txt" -Force }
    if (Test-Path "iscc_stderr.txt") { Remove-Item "iscc_stderr.txt" -Force }
}
