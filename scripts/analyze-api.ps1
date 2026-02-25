param(
    [Parameter(Mandatory = $true)]
    [string]$BearerToken,
    [string]$OutputPath = "api-analysis-data.json",
    [int]$MaxDepth = 6
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

function Resolve-Schema {
    param([hashtable]$Doc, [object]$Schema)
    if ($null -eq $Schema) { return $null }
    if ($Schema -is [hashtable] -and $Schema.ContainsKey('$ref')) {
        $ref = [string]$Schema['$ref']
        if ($ref.StartsWith("#/components/schemas/")) {
            $name = $ref.Substring("#/components/schemas/".Length)
            if ($Doc.components.schemas.ContainsKey($name)) { return $Doc.components.schemas[$name] }
        }
    }
    return $Schema
}

function Get-SchemaTypeSummary {
    param([hashtable]$Doc, [object]$Schema, [int]$Depth = 0, [int]$MaxDepth = 6)
    if ($Depth -ge $MaxDepth) { return @{ type = "max-depth-reached" } }
    $resolved = Resolve-Schema -Doc $Doc -Schema $Schema
    if ($null -eq $resolved -or $resolved -isnot [hashtable]) { return @{ type = "unknown" } }

    if ($resolved.ContainsKey("oneOf")) {
        return @{
            type = "oneOf"
            variants = @($resolved.oneOf | ForEach-Object { Get-SchemaTypeSummary -Doc $Doc -Schema $_ -Depth ($Depth + 1) -MaxDepth $MaxDepth })
        }
    }

    if ($resolved.ContainsKey("allOf")) {
        return @{
            type = "allOf"
            variants = @($resolved.allOf | ForEach-Object { Get-SchemaTypeSummary -Doc $Doc -Schema $_ -Depth ($Depth + 1) -MaxDepth $MaxDepth })
        }
    }

    if ($resolved.ContainsKey("type") -and $resolved["type"] -eq "array") {
        return @{
            type = "array"
            items = Get-SchemaTypeSummary -Doc $Doc -Schema $resolved.items -Depth ($Depth + 1) -MaxDepth $MaxDepth
        }
    }

    if ($resolved.ContainsKey("properties") -or ($resolved.ContainsKey("type") -and $resolved["type"] -eq "object")) {
        $properties = @{}
        if ($resolved.ContainsKey("properties")) {
            foreach ($k in $resolved.properties.Keys) {
                $properties[$k] = Get-SchemaTypeSummary -Doc $Doc -Schema $resolved.properties[$k] -Depth ($Depth + 1) -MaxDepth $MaxDepth
            }
        }
        return @{
            type = "object"
            properties = $properties
            additionalProperties = if ($resolved.ContainsKey("additionalProperties")) { $resolved.additionalProperties } else { $false }
        }
    }

    if ($resolved.ContainsKey("type")) { return @{ type = [string]$resolved.type } }
    return @{ type = "unknown" }
}

function Get-JsonShapeSummary {
    param([object]$Node, [int]$Depth = 0, [int]$MaxDepth = 6)
    if ($Depth -ge $MaxDepth) { return @{ type = "max-depth-reached" } }
    if ($null -eq $Node) { return @{ type = "null" } }

    if ($Node -is [System.Collections.IDictionary]) {
        $props = @{}
        foreach ($k in $Node.Keys) {
            $props[[string]$k] = Get-JsonShapeSummary -Node $Node[$k] -Depth ($Depth + 1) -MaxDepth $MaxDepth
        }
        return @{ type = "object"; properties = $props }
    }

    if ($Node -is [System.Collections.IEnumerable] -and $Node -isnot [string]) {
        $items = @($Node)
        if ($items.Count -eq 0) { return @{ type = "array"; items = @{ type = "unknown-empty" } } }
        return @{ type = "array"; items = Get-JsonShapeSummary -Node $items[0] -Depth ($Depth + 1) -MaxDepth $MaxDepth }
    }

    if ($Node -is [bool]) { return @{ type = "boolean" } }
    if ($Node -is [int] -or $Node -is [long]) { return @{ type = "integer" } }
    if ($Node -is [double] -or $Node -is [decimal] -or $Node -is [float]) { return @{ type = "number" } }
    return @{ type = "string" }
}

function Get-SampleQueryValue {
    param([hashtable]$Param)
    $name = [string]$Param.name
    $schema = if ($Param.ContainsKey("schema")) { $Param.schema } else { $null }

    if ($name -eq "fields") {
        if ($schema -is [hashtable] -and $schema.ContainsKey("items") -and $schema.items.ContainsKey("enum")) {
            return (($schema.items.enum | ForEach-Object { [string]$_ }) -join ",")
        }
        return "id,name"
    }

    if ($schema -is [hashtable] -and $schema.ContainsKey("enum") -and $schema.enum.Count -gt 0) {
        return [string]$schema.enum[0]
    }

    switch ($name) {
        "limit" { "1" }
        "offset" { "0" }
        "status" { "approved" }
        "site_ids" { "1" }
        "currency" { "USD" }
        "keyword" { "test" }
        "since_id" { "1" }
        "max_id" { "1" }
        "created_at_min" { "2026-01-01T00:00:00Z" }
        "created_at_max" { "2026-12-31T23:59:59Z" }
        "start_time" { "2026-01-01T00:00:00Z" }
        "end_time" { "2026-12-31T23:59:59Z" }
        default { "sample" }
    }
}

function Build-QueryString {
    param([object[]]$Parameters)
    $parts = @()
    foreach ($param in $Parameters) {
        if ([string]$param.in -ne "query") { continue }
        $required = [bool]($param.required)
        $name = [string]$param.name
        if (-not $required -and $name -notin @("limit", "offset", "fields", "status", "site_ids", "currency", "keyword", "since_id", "max_id", "created_at_min", "created_at_max", "start_time", "end_time")) {
            continue
        }
        $value = Get-SampleQueryValue -Param $param
        $parts += ([System.Uri]::EscapeDataString($name) + "=" + [System.Uri]::EscapeDataString($value))
    }
    if ($parts.Count -eq 0) { return "" }
    return "?" + ($parts -join "&")
}

function Build-RequestBody {
    param([hashtable]$Operation)
    if (-not $Operation.ContainsKey("requestBody")) { return $null }
    $content = $Operation.requestBody.content
    if ($content -and $content.ContainsKey("application/x-www-form-urlencoded")) {
        $form = @{ email = "invalid@example.com"; password = "invalid-password" }
        return @{ Body = $form; ContentType = "application/x-www-form-urlencoded" }
    }
    return $null
}

function Invoke-Probe {
    param(
        [string]$Method,
        [string]$Url,
        [hashtable]$Headers,
        [hashtable]$RequestBody
    )

    $statusCode = -1
    $bodyText = ""
    $contentType = ""
    try {
        if ($null -eq $RequestBody) {
            $resp = Invoke-WebRequest -UseBasicParsing -Uri $Url -Method $Method -Headers $Headers -TimeoutSec 60 -ErrorAction Stop
        }
        else {
            $resp = Invoke-WebRequest -UseBasicParsing -Uri $Url -Method $Method -Headers $Headers -Body $RequestBody.Body -ContentType $RequestBody.ContentType -TimeoutSec 60 -ErrorAction Stop
        }
        $statusCode = [int]$resp.StatusCode
        $bodyText = [string]$resp.Content
        $contentType = [string]$resp.Headers["Content-Type"]
    }
    catch {
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode.value__
            try { $contentType = [string]$_.Exception.Response.Headers["Content-Type"] } catch {}
        }
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) {
            $bodyText = [string]$_.ErrorDetails.Message
        }
        elseif ($_.Exception.Message) {
            $bodyText = [string]$_.Exception.Message
        }
    }

    $actualShape = @{ type = "non-json" }
    $isLogicalError = $false
    $errorCode = $null
    if (-not [string]::IsNullOrWhiteSpace($bodyText)) {
        try {
            $parsed = $bodyText | ConvertFrom-Json -AsHashtable -Depth 100
            $actualShape = Get-JsonShapeSummary -Node $parsed -Depth 0 -MaxDepth $MaxDepth
            if ($parsed -is [System.Collections.IDictionary] -and $parsed.ContainsKey("error")) {
                $isLogicalError = $true
                if ($parsed.ContainsKey("code")) { $errorCode = [string]$parsed.code }
            }
        }
        catch {
            $actualShape = @{ type = "non-json" }
        }
    }

    return @{
        statusCode = $statusCode
        contentType = $contentType
        isLogicalError = $isLogicalError
        errorCode = $errorCode
        actualShape = $actualShape
        bodyPreview = if ($bodyText.Length -gt 2500) { $bodyText.Substring(0, 2500) } else { $bodyText }
    }
}

$docs = @(
    @{ Name = "user"; Path = "openapi/goaffpro-user.openapi.json"; Prefix = "/user" },
    @{ Name = "public"; Path = "openapi/goaffpro-public.openapi.json"; Prefix = "/public" }
)

$results = @()
foreach ($docInfo in $docs) {
    $doc = Get-Content -Raw $docInfo.Path | ConvertFrom-Json -AsHashtable -Depth 120
    foreach ($path in ($doc.paths.Keys | Sort-Object)) {
        if (-not $path.StartsWith($docInfo.Prefix)) { continue }
        $pathItem = $doc.paths[$path]
        foreach ($method in @("get", "post", "put", "patch", "delete")) {
            if (-not $pathItem.ContainsKey($method)) { continue }
            $operation = $pathItem[$method]
            $parameters = if ($operation.ContainsKey("parameters")) { @($operation.parameters) } else { @() }
            $query = Build-QueryString -Parameters $parameters
            $url = "https://api.goaffpro.com/v1$path$query"
            $specShape = $null
            if ($operation.responses -and $operation.responses["200"] -and $operation.responses["200"].content -and $operation.responses["200"].content["application/json"]) {
                $specShape = Get-SchemaTypeSummary -Doc $doc -Schema $operation.responses["200"].content["application/json"].schema -Depth 0 -MaxDepth $MaxDepth
            }
            if ($null -eq $specShape) { $specShape = @{ type = "unknown" } }
            $requestBody = Build-RequestBody -Operation $operation

            $scenarios = @()
            if ($docInfo.Name -eq "user") {
                $scenarios += @{ authMode = "auth"; headers = @{ Authorization = "Bearer $BearerToken" } }
                $scenarios += @{ authMode = "no_auth"; headers = @{} }
            }
            else {
                $scenarios += @{ authMode = "public"; headers = @{} }
            }

            foreach ($scenario in $scenarios) {
                $probe = Invoke-Probe -Method $method.ToUpperInvariant() -Url $url -Headers $scenario.headers -RequestBody $requestBody
                $results += @{
                    domain = $docInfo.Name
                    method = $method.ToUpperInvariant()
                    path = $path
                    summary = if ($operation.ContainsKey("summary")) { [string]$operation.summary } else { "" }
                    authMode = $scenario.authMode
                    requestParameters = $parameters
                    specShape = $specShape
                    probe = ($probe + @{ url = $url })
                }
            }
        }
    }
}

$results | ConvertTo-Json -Depth 100 | Set-Content -Encoding UTF8 $OutputPath
Write-Output "Wrote $OutputPath with $($results.Count) operation scenario records."
