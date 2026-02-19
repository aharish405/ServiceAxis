$ErrorActionPreference = "Stop"

# 1. Login
$loginUrl = "http://localhost:5170/api/Auth/login"
$loginBody = @{
    email = "admin@serviceaxis.io"
    password = "Admin@123!"
} | ConvertTo-Json

# Write-Host "Logging in..."
$loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.accessToken
if (-not $token) { $token = $loginResponse.data.AccessToken }

$headers = @{ Authorization = "Bearer $token" }

# 2. Create Incident (Priority 1 = High)
$createUrl = "http://localhost:5170/api/v1/records/incident"
$incidentBody = @{
    title = "Critical Server Outage"
    priority = "1" 
} | ConvertTo-Json

# Write-Host "Creating High Priority Incident..."
$createResponse = Invoke-RestMethod -Uri $createUrl -Method Post -Body $incidentBody -Headers $headers -ContentType "application/json"
$incidentId = $createResponse.id
if (-not $incidentId) { $incidentId = $createResponse.Id }
if (-not $incidentId) { $incidentId = $createResponse.data.id }

Write-Host "IncidentID: $incidentId"

# 3. Check SLA Status
$slaUrl = "http://localhost:5170/api/v1/records/incident/$incidentId/sla"

$slaResponse = Invoke-RestMethod -Uri $slaUrl -Method Get -Headers $headers
$slaData = $slaResponse.data

if ($slaData) {
    Write-Host "SLA_DATA_START"
    $slaData | ConvertTo-Json -Depth 5 | Write-Host
    Write-Host "SLA_DATA_END"
} else {
    Write-Warning "NO_SLA_DATA"
}
