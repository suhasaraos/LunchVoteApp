<#
.SYNOPSIS
    Test Bicep deployment without actually deploying resources
.DESCRIPTION
    Runs validation, linting, and what-if analysis on Bicep templates
.PARAMETER Environment
    Target environment (dev, staging, prod)
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('dev','staging','prod')]
    [string]$Environment = 'dev'
)

# Configuration
$ResourceGroup = "rg-lunchvote-$Environment"
$Location = "eastus"
$TemplateFile = ".\main.bicep"
$ParametersFile = ".\parameters.$Environment.json"

# Ensure we're in the bicep directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Bicep Testing Script" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Test 1: Check Azure CLI
Write-Host "`n[1/8] Checking Azure CLI..." -ForegroundColor Yellow
try {
    $azVersion = az version --query '\"azure-cli\"' -o tsv
    Write-Host "✓ Azure CLI version: $azVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Azure CLI not found. Please install it first." -ForegroundColor Red
    exit 1
}

# Test 2: Check Bicep CLI
Write-Host "`n[2/8] Checking Bicep CLI..." -ForegroundColor Yellow
try {
    $bicepVersion = az bicep version
    Write-Host "✓ $bicepVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Bicep CLI not found. Installing..." -ForegroundColor Yellow
    az bicep install
}

# Test 3: Check Authentication
Write-Host "`n[3/8] Checking Azure authentication..." -ForegroundColor Yellow
try {
    $account = az account show --query name -o tsv
    Write-Host "✓ Logged in to: $account" -ForegroundColor Green
} catch {
    Write-Host "✗ Not logged in. Running 'az login'..." -ForegroundColor Yellow
    az login
}

# Test 4: Check Parameter File
Write-Host "`n[4/8] Checking parameter file..." -ForegroundColor Yellow
if (Test-Path $ParametersFile) {
    Write-Host "✓ Parameter file found: $ParametersFile" -ForegroundColor Green
    
    # Check for placeholder values
    $content = Get-Content $ParametersFile -Raw
    if ($content -match "YOUR_ENTRA_ID_OBJECT_ID_HERE" -or $content -match "YOUR_EMAIL_HERE") {
        Write-Host "⚠ Warning: Parameter file contains placeholder values!" -ForegroundColor Yellow
        Write-Host "  Please update $ParametersFile with your actual values" -ForegroundColor Yellow
        
        # Get user info
        Write-Host "`n  Your credentials:" -ForegroundColor Cyan
        $objectId = az ad signed-in-user show --query id -o tsv
        $email = az ad signed-in-user show --query userPrincipalName -o tsv
        Write-Host "  Object ID: $objectId" -ForegroundColor White
        Write-Host "  Email: $email" -ForegroundColor White
    }
} else {
    Write-Host "✗ Parameter file not found: $ParametersFile" -ForegroundColor Red
    exit 1
}

# Test 5: Build Bicep (Syntax Check)
Write-Host "`n[5/8] Building Bicep (syntax check)..." -ForegroundColor Yellow
az bicep build --file $TemplateFile
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Bicep syntax is valid" -ForegroundColor Green
    # Clean up generated JSON
    Remove-Item ".\main.json" -ErrorAction SilentlyContinue
} else {
    Write-Host "✗ Bicep syntax errors found" -ForegroundColor Red
    exit 1
}

# Test 6: Lint Bicep
Write-Host "`n[6/8] Running Bicep linter..." -ForegroundColor Yellow
$lintOutput = az bicep lint --file $TemplateFile 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Linting passed" -ForegroundColor Green
} else {
    Write-Host "⚠ Linting warnings found:" -ForegroundColor Yellow
    Write-Host $lintOutput -ForegroundColor Gray
}

# Test 7: Validate Deployment
Write-Host "`n[7/8] Validating deployment..." -ForegroundColor Yellow

# Create resource group if it doesn't exist
$rgExists = az group exists --name $ResourceGroup
if ($rgExists -eq "false") {
    Write-Host "  Creating resource group: $ResourceGroup" -ForegroundColor Cyan
    az group create --name $ResourceGroup --location $Location --output none
}

az deployment group validate `
  --resource-group $ResourceGroup `
  --template-file $TemplateFile `
  --parameters $ParametersFile `
  --output none

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Deployment validation passed" -ForegroundColor Green
} else {
    Write-Host "✗ Deployment validation failed" -ForegroundColor Red
    exit 1
}

# Test 8: What-If Analysis
Write-Host "`n[8/8] Running what-if analysis..." -ForegroundColor Yellow
Write-Host "(This shows what resources will be created/modified)" -ForegroundColor Gray

az deployment group what-if `
  --resource-group $ResourceGroup `
  --template-file $TemplateFile `
  --parameters $ParametersFile

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Testing Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nTo deploy, run:" -ForegroundColor Cyan
Write-Host "  .\deploy.ps1 -Environment $Environment" -ForegroundColor White
Write-Host "`nTo clean up the test resource group:" -ForegroundColor Cyan
Write-Host "  az group delete --name $ResourceGroup --yes --no-wait" -ForegroundColor White
