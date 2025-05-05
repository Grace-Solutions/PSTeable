# Data Import Sample
# This sample demonstrates how to import data into Teable

# Import the module
Import-Module PSTeable

# Connect to Teable
Connect-Teable -Token "your-api-token" -BaseUrl "https://your-teable-instance.com/api"

# Get a table to import data into
$table = Get-TeableTable -BaseId "your-base-id" | Where-Object { $_.Name -eq "Customers" }

# Import data from a JSON file
Import-TeableData -TableId $table.Id -Path "customers.json"

# Import data from a CSV file
Import-TeableData -TableId $table.Id -Path "customers.csv" -Format Csv

# Import data from a CSV file with custom delimiter
Import-TeableData -TableId $table.Id -Path "customers.tsv" -Format Csv -Delimiter "`t"

# Import data from a CSV file with no header row
Import-TeableData -TableId $table.Id -Path "customers-no-header.csv" -Format Csv -NoHeader

# Import data from a CSV file with field mapping
$fieldMapping = @{
    "Customer Name" = "Name"
    "Customer Email" = "Email"
    "Customer Phone" = "Phone"
}
Import-TeableData -TableId $table.Id -Path "customers-custom-headers.csv" -Format Csv -FieldMapping $fieldMapping

# Import data with batch processing
Import-TeableData -TableId $table.Id -Path "large-customer-list.json" -BatchSize 50 -ContinueOnError

# Update existing records using a key field
Import-TeableData -TableId $table.Id -Path "updated-customers.json" -Update -KeyField "Email"

# Create records from pipeline
$newCustomers = @(
    [PSCustomObject]@{
        Name = "John Doe"
        Email = "john.doe@example.com"
        Phone = "555-1234"
    },
    [PSCustomObject]@{
        Name = "Jane Smith"
        Email = "jane.smith@example.com"
        Phone = "555-5678"
    }
) | ConvertTo-TeableRecord

$newCustomers | Import-TeableData -TableId $table.Id

# Disconnect from Teable
Disconnect-Teable

