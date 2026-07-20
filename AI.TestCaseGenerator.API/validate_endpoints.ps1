$base = 'http://127.0.0.1:5276'
$loginBody = @{ email = 'endpointtester@example.com'; password = 'TestPass123!' } | ConvertTo-Json

Write-Host '--- LOGIN ---'
$loginResp = Invoke-RestMethod -Uri "$base/api/Auth/login" -Method Post -ContentType 'application/json' -Body $loginBody
$token = $loginResp.token
Write-Host "Token length: $($token.Length)"
Write-Host (ConvertTo-Json $loginResp -Depth 5)

$headers = @{ Authorization = "Bearer $token" }

Write-Host '--- GET PROJECTS ---'
$projects = Invoke-RestMethod -Uri "$base/api/project" -Method Get -Headers $headers
Write-Host (ConvertTo-Json $projects -Depth 5)

if ($projects -eq $null -or $projects.Count -eq 0) {
    Write-Host 'No projects found. Creating one.'
    $createProjectBody = @{ name = 'Endpoint Test Project'; description = 'Project created for endpoint validation.' } | ConvertTo-Json
    $projectCreated = Invoke-RestMethod -Uri "$base/api/project" -Method Post -Headers $headers -ContentType 'application/json' -Body $createProjectBody
    $projectId = $projectCreated.id
    Write-Host 'Created project:'
    Write-Host (ConvertTo-Json $projectCreated -Depth 5)
} else {
    $projectId = $projects[0].id
    Write-Host "Using existing project id: $projectId"
}

Write-Host '--- UPLOAD DOCUMENT ---'
$filePath = 'D:\AI-TestCaseGenerator\AI_Test_Case_Generator_SRS.docx'
if (-Not (Test-Path $filePath)) {
    throw "Document file not found: $filePath"
}
$form = @{ ProjectId = $projectId; File = Get-Item $filePath }
$uploadResp = Invoke-RestMethod -Uri "$base/api/Document/upload" -Method Post -Headers $headers -Form $form
Write-Host 'Upload response:'
Write-Host (ConvertTo-Json $uploadResp -Depth 5)

$documentId = $uploadResp.id
Write-Host "Document ID: $documentId"

Write-Host '--- PROCESS DOCUMENT ---'
$processResp = Invoke-RestMethod -Uri "$base/api/Document/process/$documentId" -Method Post -Headers $headers
Write-Host 'Process response:'
Write-Host (ConvertTo-Json $processResp -Depth 5)

Write-Host '--- GENERATE TEST CASES ---'
$generateBody = @{ projectId = $projectId; moduleName = 'Login'; testType = 'All'; numberOfTestCases = 5; prompt = 'Generate test cases for this project.' } | ConvertTo-Json
$testResp = Invoke-RestMethod -Uri "$base/api/TestCase/generate" -Method Post -Headers $headers -ContentType 'application/json' -Body $generateBody
Write-Host 'Test case response:'
Write-Host (ConvertTo-Json $testResp -Depth 5)

Write-Host '--- AI CHAT ASK ---'
$chatBody = @{ projectId = $projectId; question = 'What is this project about?' } | ConvertTo-Json
$chatResp = Invoke-RestMethod -Uri "$base/api/AIChat/ask" -Method Post -Headers $headers -ContentType 'application/json' -Body $chatBody
Write-Host 'Chat response:'
Write-Host (ConvertTo-Json $chatResp -Depth 5)
