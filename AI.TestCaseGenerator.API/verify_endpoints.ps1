$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5000'

function Invoke-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Path,
        [hashtable]$Headers = $null,
        [string]$Body = $null,
        [switch]$IsForm
    )

    try {
        $params = @{ Uri = "$base$Path"; Method = $Method; Headers = $Headers; UseBasicParsing = $true }
        if ($Body) { $params.Body = $Body }
        if ($IsForm) { $params.Form = $Body }
        $resp = Invoke-WebRequest @params
        [pscustomobject]@{ Name = $Name; Status = [string]$resp.StatusCode; Body = ($resp.Content.Trim().Replace("`r", ' ').Replace("`n", ' ')) }
    }
    catch {
        $status = 0
        if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode.value__ }
        $msg = if ($_.ErrorDetails) { $_.ErrorDetails.Message } else { $_.Exception.Message }
        [pscustomobject]@{ Name = $Name; Status = [string]$status; Body = $msg }
    }
}

$plainHeaders = @{ 'Content-Type' = 'application/json' }
$registerBody = '{"fullName":"Endpoint Tester","email":"endpointtester5@example.com","password":"TestPass123!","confirmPassword":"TestPass123!"}'
$registerResp = Invoke-WebRequest -UseBasicParsing -Uri "$base/api/Auth/register" -Method Post -Headers $plainHeaders -Body $registerBody
$token = ($registerResp.Content | ConvertFrom-Json).token
$authHeaders = @{ Authorization = "Bearer $token"; 'Content-Type' = 'application/json' }

$results = @(
    (Invoke-Endpoint -Name 'Health' -Method 'GET' -Path '/api/Health' -Headers $plainHeaders),
    (Invoke-Endpoint -Name 'Auth Login' -Method 'POST' -Path '/api/Auth/login' -Headers $plainHeaders -Body '{"email":"endpointtester5@example.com","password":"TestPass123!"}'),
    (Invoke-Endpoint -Name 'Auth Logout' -Method 'POST' -Path '/api/Auth/logout' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Auth Profile' -Method 'GET' -Path '/api/Auth/profile' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Auth Validate' -Method 'GET' -Path '/api/Auth/validate' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Project List' -Method 'GET' -Path '/api/Project' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Project Create' -Method 'POST' -Path '/api/Project' -Headers $authHeaders -Body '{"name":"API Test Project","description":"Created during validation"}'),
    (Invoke-Endpoint -Name 'Project Get' -Method 'GET' -Path '/api/Project/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Project Update' -Method 'PUT' -Path '/api/Project/1' -Headers $authHeaders -Body '{"name":"Updated Project","description":"Updated during validation"}'),
    (Invoke-Endpoint -Name 'Project Delete' -Method 'DELETE' -Path '/api/Project/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Document Project' -Method 'GET' -Path '/api/Document/project/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Document Upload' -Method 'POST' -Path '/api/Document/upload' -Headers @{ Authorization = "Bearer $token" } -Body @{ ProjectId = '1'; File = Get-Item -Path 'D:\temp\sample-upload.txt' } -IsForm),
    (Invoke-Endpoint -Name 'Document Detail' -Method 'GET' -Path '/api/Document/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Document Download' -Method 'GET' -Path '/api/Document/download/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Document Delete' -Method 'DELETE' -Path '/api/Document/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Document Process' -Method 'POST' -Path '/api/Document/process/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'TestCase Generate' -Method 'POST' -Path '/api/TestCase/generate' -Headers $authHeaders -Body '{"projectId":1,"moduleName":"Login","testType":"Positive","numberOfTestCases":2,"prompt":"Generate sample test cases"}'),
    (Invoke-Endpoint -Name 'TestCase Project' -Method 'GET' -Path '/api/TestCase/project/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'TestCase Detail' -Method 'GET' -Path '/api/TestCase/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'TestCase Search' -Method 'GET' -Path '/api/TestCase/search?projectId=1&keyword=login' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'TestCase Filter' -Method 'GET' -Path '/api/TestCase/filter?projectId=1&priority=High&testType=Positive' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'AIChat Ask' -Method 'POST' -Path '/api/AIChat/ask' -Headers $authHeaders -Body '{"projectId":1,"question":"Summarize this project"}'),
    (Invoke-Endpoint -Name 'AIChat History' -Method 'GET' -Path '/api/AIChat/history/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'AIChat Delete' -Method 'DELETE' -Path '/api/AIChat/history/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'AIChat Regenerate' -Method 'POST' -Path '/api/AIChat/regenerate' -Headers $authHeaders -Body '{"projectId":1,"question":"Summarize this project"}'),
    (Invoke-Endpoint -Name 'Export Excel' -Method 'GET' -Path '/api/Export/excel/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Export PDF' -Method 'GET' -Path '/api/Export/pdf/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'Export Single PDF' -Method 'GET' -Path '/api/Export/pdf/testcase/1' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'User Profile' -Method 'GET' -Path '/api/User/profile' -Headers $authHeaders),
    (Invoke-Endpoint -Name 'User Update' -Method 'PUT' -Path '/api/User/profile' -Headers $authHeaders -Body '{"fullName":"Updated Name"}'),
    (Invoke-Endpoint -Name 'User Change Password' -Method 'PUT' -Path '/api/User/change-password' -Headers $authHeaders -Body '{"currentPassword":"TestPass123!","newPassword":"NewPass123!"}'),
    (Invoke-Endpoint -Name 'User Delete' -Method 'DELETE' -Path '/api/User' -Headers $authHeaders)
)

$results | ForEach-Object { "{0}|{1}|{2}" -f $_.Name, $_.Status, ($_.Body -replace "`r?`n", ' ') } | Set-Content -Path 'D:\temp\endpoint_results.txt'
Get-Content 'D:\temp\endpoint_results.txt'
