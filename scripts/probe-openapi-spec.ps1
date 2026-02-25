param(
    [Parameter(Mandatory = $true)]
    [string]$SpecPath,
    [Parameter(Mandatory = $true)]
    [string]$OutputPath,
    [string]$BearerToken,
    [string[]]$IncludePathPrefixes = @("/user", "/public"),
    [int]$MaxDepth = 6,
    [switch]$ProbeNoAuthForUser
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

function Read-OpenApiSpecAsHashtable {
    param([string]$Path)

    $ext = [System.IO.Path]::GetExtension($Path).ToLowerInvariant()
    if ($ext -in @(".yaml", ".yml")) {
        $tmpPath = [System.IO.Path]::GetTempFileName()
        try {
            $escapedInput = $Path.Replace("'", "''")
            $escapedOutput = $tmpPath.Replace("'", "''")
            $py = "import yaml,json,pathlib; p=pathlib.Path(r'$escapedInput'); data=yaml.safe_load(p.read_text(encoding='utf-8')); pathlib.Path(r'$escapedOutput').write_text(json.dumps(data), encoding='utf-8')"
            python -c $py | Out-Null
            return (Get-Content -Raw $tmpPath | ConvertFrom-Json -AsHashtable -Depth 120)
        }
        finally {
            if (Test-Path $tmpPath) { Remove-Item $tmpPath -Force }
        }
    }

    return (Get-Content -Raw $Path | ConvertFrom-Json -AsHashtable -Depth 120)
}

function Resolve-Schema {
    param(
        [hashtable]$Spec,
        [object]$Schema
    )

    if ($null -eq $Schema) {
        return $null
    }

    if ($Schema -is [hashtable] -and $Schema.ContainsKey('$ref')) {
        $ref = [string]$Schema['$ref']
        if ($ref.StartsWith("#/components/schemas/")) {
            $name = $ref.Substring("#/components/schemas/".Length)
            if ($Spec.components -and $Spec.components.schemas -and $Spec.components.schemas.ContainsKey($name)) {
                return $Spec.components.schemas[$name]
            }
        }
    }

    return $Schema
}

function Get-SpecSchemaShape {
    param(
        [hashtable]$Spec,
        [object]$Schema,
        [int]$Depth = 0,
        [int]$MaxDepth = 6
    )

    if ($Depth -ge $MaxDepth) {
        return @{ type = "max-depth-reached" }
    }

    $resolved = Resolve-Schema -Spec $Spec -Schema $Schema
    if ($null -eq $resolved -or $resolved -isnot [hashtable]) {
        return @{ type = "unknown" }
    }

    if ($resolved.ContainsKey("oneOf")) {
        return @{
            type = "oneOf"
            variants = @($resolved.oneOf | ForEach-Object {
                    Get-SpecSchemaShape -Spec $Spec -Schema $_ -Depth ($Depth + 1) -MaxDepth $MaxDepth
                })
        }
    }

    if ($resolved.ContainsKey("anyOf")) {
        return @{
            type = "anyOf"
            variants = @($resolved.anyOf | ForEach-Object {
                    Get-SpecSchemaShape -Spec $Spec -Schema $_ -Depth ($Depth + 1) -MaxDepth $MaxDepth
                })
        }
    }

    $schemaType = if ($resolved.ContainsKey("type")) { [string]$resolved.type } else { "" }

    if ($schemaType -eq "array") {
        return @{
            type = "array"
            items = Get-SpecSchemaShape -Spec $Spec -Schema $resolved.items -Depth ($Depth + 1) -MaxDepth $MaxDepth
        }
    }

    if ($schemaType -eq "object" -or $resolved.ContainsKey("properties")) {
        $properties = @{}
        if ($resolved.ContainsKey("properties")) {
            foreach ($key in $resolved.properties.Keys) {
                $properties[$key] = Get-SpecSchemaShape -Spec $Spec -Schema $resolved.properties[$key] -Depth ($Depth + 1) -MaxDepth $MaxDepth
            }
        }

        $additionalProperties = $false
        if ($resolved.ContainsKey("additionalProperties")) {
            $additionalProperties = $resolved.additionalProperties
        }

        return @{
            type = "object"
            properties = $properties
            additionalProperties = $additionalProperties
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($schemaType)) {
        return @{ type = $schemaType }
    }

    return @{ type = "unknown" }
}

function Get-JsonShape {
    param(
        [object]$Node,
        [int]$Depth = 0,
        [int]$MaxDepth = 6
    )

    if ($Depth -ge $MaxDepth) {
        return @{ type = "max-depth-reached" }
    }

    if ($null -eq $Node) {
        return @{ type = "null" }
    }

    if ($Node -is [System.Collections.IDictionary]) {
        $properties = @{}
        foreach ($key in $Node.Keys) {
            $properties[[string]$key] = Get-JsonShape -Node $Node[$key] -Depth ($Depth + 1) -MaxDepth $MaxDepth
        }

        return @{
            type = "object"
            properties = $properties
        }
    }

    if ($Node -is [System.Collections.IEnumerable] -and $Node -isnot [string]) {
        $items = @($Node)
        if ($items.Count -eq 0) {
            return @{
                type = "array"
                items = @{ type = "unknown-empty" }
            }
        }

        return @{
            type = "array"
            items = Get-JsonShape -Node $items[0] -Depth ($Depth + 1) -MaxDepth $MaxDepth
        }
    }

    if ($Node -is [bool]) { return @{ type = "boolean" } }
    if ($Node -is [int] -or $Node -is [long]) { return @{ type = "integer" } }
    if ($Node -is [double] -or $Node -is [decimal] -or $Node -is [float]) { return @{ type = "number" } }
    return @{ type = "string" }
}

function Get-SampleValueFromSchema {
    param(
        [hashtable]$Schema,
        [string]$Name
    )

    if ($Schema -and $Schema.ContainsKey("enum") -and $Schema.enum.Count -gt 0) {
        return [string]$Schema.enum[0]
    }

    if ($Name -eq "fields") {
        if ($Schema -and $Schema.ContainsKey("items") -and $Schema.items.ContainsKey("enum")) {
            return (($Schema.items.enum | ForEach-Object { [string]$_ }) -join ",")
        }
        return "id,name"
    }

    switch ($Name) {
        "limit" { return "1" }
        "offset" { return "0" }
        "status" { return "approved" }
        "site_ids" { return "1" }
        "currency" { return "USD" }
        "keyword" { return "test" }
        "since_id" { return "1" }
        "max_id" { return "1" }
        "created_at_min" { return "2026-01-01T00:00:00Z" }
        "created_at_max" { return "2026-12-31T23:59:59Z" }
        "start_time" { return "2026-01-01T00:00:00Z" }
        "end_time" { return "2026-12-31T23:59:59Z" }
        "email" { return "invalid@example.com" }
        "password" { return "invalid-password" }
        default {
            if ($Schema -and $Schema.ContainsKey("type")) {
                switch ([string]$Schema.type) {
                    "integer" { return "1" }
                    "number" { return "1" }
                    "boolean" { return "true" }
                    default { return "sample" }
                }
            }

            return "sample"
        }
    }
}

function Build-QueryString {
    param(
        [object[]]$Parameters
    )

    $parts = @()
    foreach ($parameter in $Parameters) {
        if (-not $parameter -or [string]$parameter.in -ne "query") {
            continue
        }

        $name = [string]$parameter.name
        $required = [bool]($parameter.required)

        # Include required parameters and known useful optional filters.
        if (-not $required -and $name -notin @("limit", "offset", "fields", "status", "site_ids", "currency", "keyword", "since_id", "max_id", "created_at_min", "created_at_max", "start_time", "end_time")) {
            continue
        }

        $schema = if ($parameter.schema -is [hashtable]) { $parameter.schema } else { @{} }
        $value = Get-SampleValueFromSchema -Schema $schema -Name $name
        $parts += ([System.Uri]::EscapeDataString($name) + "=" + [System.Uri]::EscapeDataString([string]$value))
    }

    if ($parts.Count -eq 0) {
        return ""
    }

    return "?" + ($parts -join "&")
}

function Build-RequestBody {
    param(
        [hashtable]$Operation
    )

    if (-not $Operation.ContainsKey("requestBody")) {
        return $null
    }

    $requestBody = $Operation.requestBody
    if (-not $requestBody -or -not $requestBody.ContainsKey("content")) {
        return $null
    }

    $content = $requestBody.content
    if ($content.ContainsKey("application/x-www-form-urlencoded")) {
        return @{
            Body = @{
                email = "invalid@example.com"
                password = "invalid-password"
            }
            ContentType = "application/x-www-form-urlencoded"
        }
    }

    if ($content.ContainsKey("application/json")) {
        $schema = $content["application/json"].schema
        if ($schema -and $schema.ContainsKey("properties")) {
            $obj = @{}
            foreach ($propName in $schema.properties.Keys) {
                $propSchema = if ($schema.properties[$propName] -is [hashtable]) { $schema.properties[$propName] } else { @{} }
                $obj[$propName] = Get-SampleValueFromSchema -Schema $propSchema -Name $propName
            }

            return @{
                Body = ($obj | ConvertTo-Json -Depth 20 -Compress)
                ContentType = "application/json"
            }
        }
    }

    return $null
}

$spec = Read-OpenApiSpecAsHashtable -Path $SpecPath
$baseUrl = [string]$spec.servers[0].url
if (-not $baseUrl.EndsWith("/")) {
    $baseUrl += "/"
}
$baseUrl = $baseUrl.TrimEnd("/")

$records = @()

foreach ($path in ($spec.paths.Keys | Sort-Object)) {
    $include = $false
    foreach ($prefix in $IncludePathPrefixes) {
        if ($path.StartsWith($prefix)) {
            $include = $true
            break
        }
    }

    if (-not $include) {
        continue
    }

    $pathItem = $spec.paths[$path]
    $pathParameters = @()
    if ($pathItem.ContainsKey("parameters")) {
        $pathParameters = @($pathItem.parameters)
    }

    foreach ($method in @("get", "post", "put", "patch", "delete")) {
        if (-not $pathItem.ContainsKey($method)) {
            continue
        }

        $operation = $pathItem[$method]
        $operationParameters = @()
        if ($operation.ContainsKey("parameters")) {
            $operationParameters = @($operation.parameters)
        }

        $allParameters = @($pathParameters + $operationParameters)
        $pathWithValues = $path
        foreach ($param in $allParameters) {
            if ([string]$param.in -eq "path") {
                $token = "{" + [string]$param.name + "}"
                $pathWithValues = $pathWithValues.Replace($token, "1")
            }
        }

        $query = Build-QueryString -Parameters $allParameters
        $url = "$baseUrl$pathWithValues$query"

        $requestBody = Build-RequestBody -Operation $operation

        $scenarios = @()
        if ($path.StartsWith("/user")) {
            if (-not [string]::IsNullOrWhiteSpace($BearerToken)) {
                $scenarios += @{ authMode = "auth"; headers = @{ Authorization = "Bearer $BearerToken" } }
            }
            if ($ProbeNoAuthForUser.IsPresent) {
                $scenarios += @{ authMode = "no_auth"; headers = @{} }
            }
        }
        else {
            $scenarios += @{ authMode = "public"; headers = @{} }
        }

        foreach ($scenario in $scenarios) {
            $statusCode = -1
            $responseText = ""
            $contentType = ""
            try {
                if ($null -eq $requestBody) {
                    $response = Invoke-WebRequest -UseBasicParsing -Uri $url -Method $method.ToUpperInvariant() -Headers $scenario.headers -TimeoutSec 60 -ErrorAction Stop
                }
                else {
                    $response = Invoke-WebRequest -UseBasicParsing -Uri $url -Method $method.ToUpperInvariant() -Headers $scenario.headers -Body $requestBody.Body -ContentType $requestBody.ContentType -TimeoutSec 60 -ErrorAction Stop
                }

                $statusCode = [int]$response.StatusCode
                $responseText = [string]$response.Content
                $contentType = [string]$response.Headers["Content-Type"]
            }
            catch {
                if ($_.Exception.Response) {
                    $statusCode = [int]$_.Exception.Response.StatusCode.value__
                    try { $contentType = [string]$_.Exception.Response.Headers["Content-Type"] } catch {}
                }

                if ($_.ErrorDetails -and $_.ErrorDetails.Message) {
                    $responseText = [string]$_.ErrorDetails.Message
                }
                elseif ($_.Exception.Message) {
                    $responseText = [string]$_.Exception.Message
                }
            }

            $actualShape = @{ type = "non-json" }
            $isLogicalError = $false
            $errorCode = $null
            if (-not [string]::IsNullOrWhiteSpace($responseText)) {
                try {
                    $parsed = $responseText | ConvertFrom-Json -AsHashtable -Depth 100
                    $actualShape = Get-JsonShape -Node $parsed -Depth 0 -MaxDepth $MaxDepth
                    if ($parsed -is [System.Collections.IDictionary] -and $parsed.ContainsKey("error")) {
                        $isLogicalError = $true
                        if ($parsed.ContainsKey("code")) {
                            $errorCode = [string]$parsed.code
                        }
                    }
                }
                catch {
                    $actualShape = @{ type = "non-json" }
                }
            }

            $responseSchema = $null
            if ($operation.ContainsKey("responses") -and $operation.responses.ContainsKey("200")) {
                $r200 = $operation.responses["200"]
                if ($r200 -is [hashtable] -and $r200.ContainsKey("content") -and $r200.content.ContainsKey("application/json")) {
                    $media = $r200.content["application/json"]
                    if ($media -is [hashtable] -and $media.ContainsKey("schema")) {
                        $responseSchema = $media.schema
                    }
                }
            }

            $records += @{
                method = $method.ToUpperInvariant()
                path = $path
                summary = if ($operation.ContainsKey("summary")) { [string]$operation.summary } else { "" }
                authMode = $scenario.authMode
                url = $url
                statusCode = $statusCode
                contentType = $contentType
                isLogicalError = $isLogicalError
                errorCode = $errorCode
                requestParameters = $allParameters
                specResponseShape = Get-SpecSchemaShape -Spec $spec -Schema $responseSchema -Depth 0 -MaxDepth $MaxDepth
                actualResponseShape = $actualShape
                responsePreview = if ($responseText.Length -gt 4000) { $responseText.Substring(0, 4000) } else { $responseText }
            }
        }
    }
}

$records | ConvertTo-Json -Depth 100 | Set-Content -Encoding UTF8 $OutputPath
Write-Output "Wrote $OutputPath with $($records.Count) records from $SpecPath."
