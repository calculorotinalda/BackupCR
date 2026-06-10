# BackupCR - Agente Profissional de Backup & Recuperação

**BackupCR** é uma solução corporativa de backup e recuperação de dados para ambientes Windows, Linux e máquinas virtuais, inspirada diretamente nas consolas corporativas do software **Veeam Backup & Replication**. 

Desenvolvido utilizando as tecnologias mais modernas do ecossistema .NET 8, o aplicativo une a robustez do C# em segundo plano com a flexibilidade do **Blazor Hybrid** hospedado em um container **WPF**, oferecendo uma interface no estilo Windows 11 Fluent Design, responsiva, com temas claro/escuro e micro-animações, rodando diretamente na bandeja do sistema (System Tray).

---

## 🌟 Funcionalidades Principais

### 1. Painel Central (Dashboard)
- **Métricas em Tempo Real**: Total de jobs ativos, taxa de sucesso de execuções, volume de armazenamento consumido e contagem de alertas.
- **Gráficos Interativos**: Gráficos SVG nativos para monitorar o volume de dados transferidos diariamente e utilização de repositórios.
- **Auditoria Rápida**: Grid das últimas execuções com badges de status coloridos e console popup para visualizar logs detalhados do terminal em tempo real.

### 2. Gerenciamento de Backups (Jobs)
- **Escopo Flexível**: Suporta backup de Arquivos/Pastas Locais, Máquinas Virtuais (VMware/Hyper-V) e Bancos de Dados SQL.
- **Tipos de Execução**: Backup Completo (Full), Incremental (alterações desde o último backup), Diferencial e Sintético.
- **Compressão & Deduplicação**: Redução inteligente de tamanho de pacotes compactados e descarte de blocos redundantes através de hashing de dados.
- **Criptografia Forte**: Proteção de dados ponta a ponta (AES-256) com chaves personalizáveis.

### 3. Destinos de Armazenamento (Repositórios)
- **Disco Local e USB Externo**: Destinos diretos no sistema de arquivos local.
- **Compartilhamentos de Rede (SMB/NFS)**: Conexão direta com storages NAS corporativos.
- **Servidores SFTP/FTP**: Upload seguro com SSH.NET.
- **Armazenamento em Nuvem**: Conexão com Amazon S3, Microsoft Azure Blob Storage e Google Cloud Storage.
- **Validador de Conexão**: Ferramenta de teste integrado que valida as credenciais e permissões de escrita dos endpoints antes de ativar o job.

### 4. Políticas de Agendador & Retenção GFS
- **Agendamento Flexível**: Execuções Manuais, Diárias (hora marcada), Semanais (dia da semana e hora) e Contínuas (CDP - Proteção de Dados Contínua a cada 15 minutos).
- **Políticas GFS (Grandfather-Father-Son)**: Manutenção inteligente de pontos de restauração estruturados (retenções de longo prazo semanais, mensais e anuais).

### 5. Console de Recuperação (Restore)
- **Pontos de Restauração (Restore Points)**: Seleção por data/hora com base nas execuções bem-sucedidas.
- **Restauração Completa**: Extração total do pacote para o diretório de destino.
- **Recuperação Granular de Arquivos**: Explorador de arquivos virtual que permite navegar dentro do pacote criptografado e selecionar arquivos individuais para restauração rápida.
- **Restauração Instantânea de VM (Veeam Instant Recovery)**: Simulação de montagem instantânea (vPower NFS) do disco virtual para colocar a máquina virtual online instantaneamente sem copiar arquivos primeiro.

### 6. Segurança Avançada
- **Proteção contra Ransomware**: Bloqueio de Imutabilidade (Object Lock) que impede modificações ou exclusões de backups existentes por malware.
- **Active Directory**: Sincronização automática de operadores e administradores locais baseados em grupos de domínio do AD.
- **MFA (Autenticação Multifator)**: Confirmação em duas etapas para operações sensíveis e exclusões de logs.

---

## 🛠️ Tecnologias Utilizadas

- **Core & Backend**: C# .NET 8
- **Frontend**: Blazor Hybrid + WPF (HTML5, Vanilla CSS com Fluent Design, JavaScript nativo)
- **Banco de Dados**: Entity Framework Core com suporte automatizado a SQLite local e MySQL corporativo
- **Transferência Segura**: SSH.NET (SFTP)
- **Criptografia**: Cryptography AES-256

---

## 🚀 Como Compilar o Projeto

O projeto acompanha um script de automação em PowerShell (`build.ps1`) para gerar um executável independente de forma rápida.

1. Abra o **PowerShell** no diretório raiz do projeto.
2. Execute o script de compilação:
   ```powershell
   .\build.ps1
   ```
3. O script irá:
   - Validar a instalação do .NET SDK.
   - Executar o comando `dotnet publish` direcionando os logs de stdout/stderr.
   - Consolidar as informações de compilação e eventuais erros no arquivo **`log.txt`**.
   - Gerar o executável autossuficiente (Self-Contained) em: `bin\Release\net8.0-windows\win-x64\publish\BackupCR.exe`.

---

## 💻 Como Executar

Após compilar o executável:
1. Abra a pasta `bin\Release\net8.0-windows\win-x64\publish\`.
2. Dê duplo clique em `BackupCR.exe`.
3. O aplicativo será carregado na barra de tarefas (área de notificação/System Tray) com o ícone corporativo (`icon.ico`).
4. Clique com o botão direito no ícone da bandeja para exibir o menu de controle do agente:
   - **Abrir Dashboard**: Maximiza a interface de usuário moderna.
   - **Criar Backup**: Inicia a execução em lote de todas as tarefas ativas em segundo plano.
   - **Configurar Backup**: Abre a listagem e criação de jobs de backup.
   - **Agendamento**: Direciona para as políticas de agendamento e retenção GFS.
   - **Sair**: Fecha completamente o agente em background.
# BackupCR
