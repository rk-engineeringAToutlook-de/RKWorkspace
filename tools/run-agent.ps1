param(
    [switch] $Once,
    [switch] $Status,
    [switch] $Demo,
    [switch] $NoDemo,
    [string] $AgentId,
    [string] $WorkspaceName,
    [ValidateSet('Left', 'Right', 'Center')]
    [string] $Position,
    [string] $IpcServer,
    [string] $IpcClient,
    [string] $TargetAgentId,
    [switch] $IpcStopAfterTransfer
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$agentProject = Join-Path $root 'src\Agents\RKWorkspace.Agent\RKWorkspace.Agent.csproj'
$agentArgs = @()

if ($Once) {
    $agentArgs += '--once'
}

if ($Status) {
    $agentArgs += '--status'
}

if ($Demo) {
    $agentArgs += '--demo'
}

if ($NoDemo) {
    $agentArgs += '--no-demo'
}

if (-not [string]::IsNullOrWhiteSpace($AgentId)) {
    $agentArgs += '--agent-id'
    $agentArgs += $AgentId
}

if (-not [string]::IsNullOrWhiteSpace($WorkspaceName)) {
    $agentArgs += '--workspace-name'
    $agentArgs += $WorkspaceName
}

if (-not [string]::IsNullOrWhiteSpace($Position)) {
    $agentArgs += '--position'
    $agentArgs += $Position
}

if (-not [string]::IsNullOrWhiteSpace($IpcServer)) {
    $agentArgs += '--ipc-server'
    $agentArgs += $IpcServer
}

if (-not [string]::IsNullOrWhiteSpace($IpcClient)) {
    $agentArgs += '--ipc-client'
    $agentArgs += $IpcClient
}

if (-not [string]::IsNullOrWhiteSpace($TargetAgentId)) {
    $agentArgs += '--target-agent-id'
    $agentArgs += $TargetAgentId
}

if ($IpcStopAfterTransfer) {
    $agentArgs += '--ipc-stop-after-transfer'
}

dotnet build $agentProject -warnaserror
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet run --project $agentProject -- @agentArgs
exit $LASTEXITCODE
