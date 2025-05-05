# Schema Management Sample
# This sample demonstrates how to manage schemas in Teable

# Import the module
Import-Module PSTeable

# Connect to Teable
Connect-Teable -Token "your-api-token" -BaseUrl "https://your-teable-instance.com/api"

# Get a base to work with
$base = Get-TeableBase -SpaceId "your-space-id" | Where-Object { $_.Name -eq "Inventory" }

# Export the base schema
Export-TeableSchema -BaseId $base.Id -Path "inventory-schema.json"

# Get a view to work with
$table = Get-TeableTable -BaseId $base.Id | Where-Object { $_.Name -eq "Products" }
$view = Get-TeableView -TableId $table.Id | Where-Object { $_.Name -eq "In Stock" }

# Export the view schema
Export-TeableSchema -ViewId $view.Id -Path "in-stock-view-schema.json"

# Create a new base from a schema
$newBase = Import-TeableSchema -SpaceId "your-space-id" -Path "inventory-schema.json" -Name "Inventory Copy"

# Update an existing base from a schema
Import-TeableSchema -BaseId $base.Id -Path "updated-inventory-schema.json" -Force

# Create a new view from a schema
$newView = Import-TeableSchema -TableId $table.Id -Path "in-stock-view-schema.json" -Name "In Stock Copy"

# Update an existing view from a schema
Import-TeableSchema -ViewId $view.Id -Path "updated-view-schema.json" -Force

# Compare two schemas
$comparison = Compare-TeableSchemas -Path1 "inventory-schema.json" -Path2 "updated-inventory-schema.json"
$comparison | Format-Table -Property Path, PropertyName, Value1, Value2

# Disconnect from Teable
Disconnect-Teable

