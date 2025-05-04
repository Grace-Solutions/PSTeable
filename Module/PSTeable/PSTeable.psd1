@{
    RootModule = 'lib\PSTeable.dll'
    ModuleVersion = '1.1.0'
    GUID = '8e4f8f5a-0f9a-4f5a-9f5a-0f9a4f5a9f5a'
    Author = 'PSTeable Contributors'
    CompanyName = 'Grace Solutions'
    Copyright = '(c) 2025 Grace Solutions. All rights reserved.'
    Description = 'PowerShell module for interacting with the Teable REST API'
    PowerShellVersion = '7.2'
    DotNetFrameworkVersion = '4.7.2'
    CompatiblePSEditions = @('Core')
    FunctionsToExport = @()
    CmdletsToExport = @(
        # Authentication
        'Connect-Teable',
        'Disconnect-Teable',

        # Spaces
        'Get-TeableSpace',

        # Bases
        'Get-TeableBase',
        'New-TeableBase',
        'Set-TeableBase',
        'Remove-TeableBase',

        # Tables
        'Get-TeableTable',
        'New-TeableTable',
        'Set-TeableTable',
        'Remove-TeableTable',

        # Fields
        'Get-TeableField',
        'New-TeableField',
        'Set-TeableField',
        'Remove-TeableField',

        # Records
        'Get-TeableRecord',
        'New-TeableRecord',
        'Set-TeableRecord',
        'Remove-TeableRecord',

        # Views
        'Get-TeableView',
        'New-TeableView',
        'Set-TeableView',
        'Remove-TeableView',

        # Automations
        'Get-TeableAutomation',
        'New-TeableAutomation',
        'Set-TeableAutomation',
        'Remove-TeableAutomation',

        # Dashboards
        'Get-TeableDashboard',
        'New-TeableDashboard',
        'Set-TeableDashboard',
        'Remove-TeableDashboard',

        # Attachments
        'Add-TeableAttachment',
        'Remove-TeableAttachment',

        # Plugins
        'Get-TeablePlugin',
        'Install-TeablePlugin',
        'Update-TeablePlugin',
        'Uninstall-TeablePlugin',

        # Roles & Security
        'Get-TeableRole',
        'Set-TeableUserRole',
        'Remove-TeableUserRole',

        # Data Conversion & Sync
        'ConvertTo-TeableRecord',
        'Export-TeableSchema',
        'Import-TeableSchema',
        'Compare-TeableSchemas',
        'Import-TeableData',
        'Export-TeableData',
        'Export-TeableRecords',
        'Backup-TeableData',
        'Restore-TeableData',
        'Sync-TeableData'
    )
    VariablesToExport = @()
    AliasesToExport = @()
    FormatsToProcess = @('types\PSTeable.Format.ps1xml')
    TypesToProcess = @('types\PSTeable.Types.ps1xml')
    PrivateData = @{
        PSData = @{
            Tags = @('Teable', 'API', 'REST')
            LicenseUri = 'https://opensource.org/licenses/MIT'
            ProjectUri = 'https://github.com/grace-solutions/PSTeable'
            ReleaseNotes = 'See RELEASE-NOTES.md for details'
        }
    }
}
