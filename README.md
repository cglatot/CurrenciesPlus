# Umbra Plugin Development

Umbra supports custom plugins to extend its functionality and provide additional toolbar widgets and world marker types.
Plugins are written in C# and are loaded as DLL files at runtime by Umbra. Plugins can be loaded from the _Plugins
section_ in Umbra's Settings window.

## Prerequisites

In order to develop plugins for Umbra, you will need the following:

- .NET SDK 8.x or higher
- Visual Studio 2019 or later, or another C# IDE such as JetBrains Rider
- Umbra 2.1.0 or higher installed on your computer

Your plugin **MUST** have references to the following Umbra assemblies to function properly:

| Assembly           | Name           | Description                                                              |
|--------------------|----------------|--------------------------------------------------------------------------|
| `Umbra.dll`        | `Umbra`        | The main Umbra assembly.  Provided access to Widget & World Marker API's |
| `Umbra.Common.dll` | `Umbra.Common` | Umbra's framework assembly. Provides access to service API's             |

Your plugin **MAY** have references to the following assemblies to access additional functionality:

| Assembly                 | Name                 | Description                                             |
|--------------------------|----------------------|---------------------------------------------------------|
| `Umbra.Game.dll`         | `Umbra.Game`         | Provides access to FFXIV-specific API's that Umbra uses |
| `Una.Drawing.dll`        | `Una.Drawing`        | Provides access to drawing API's that Umbra uses        |
| `Dalamud.dll`            | `Dalamud`            | Provides access to Dalamud's API's.                     |
| `Lumina.dll`             | `Lumina`             | Provides access to Lumina's API's.                      |
| `Lumina.Excel.dll`       | `Lumina.Excel`       | Provides access to Lumina's Excel API's.                |
| `FFXIVClientStructs.dll` | `FFXIVClientStructs` | Provides access to FFXIV's client structs.              |

The assemblies listed above are forward-referenced in Umbra's plugin loader, so you can reference them in your plugin
project without needing to copy them to your project's output directory. If you need to reference other assemblies, you
will need to copy them to your project's output directory.

The physical locations of the Umbra assemblies can be found in Dalamud's `installedPlugins\Umbra\<version>\` directory.
You can find this in `%AppData%\XIVLauncher\addon\installedPlugins` on Windows. The Dalamud assemblies can be found in
`%AppData%\XIVLauncher\addon\Hooks\dev\` on Windows. Please refer to Dalamud's documentation for more information.

> [!NOTE]
> If you're running a local development version of Umbra, you must reference the assemblies from the local development
> directory instead by modifying the project file. (See example below)

## An example project file

Here is an example project file for a plugin that references all supported assemblies and builds for the .NET 8.0
to the "out" directory. This project file assumes that you have Umbra and Dalamud installed through XIVLauncher and
running on a Windows operating system. If you're running on Linux or macOS, you will need to adjust the paths
accordingly.

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <DalamudLibPath>$(appdata)\XIVLauncher\addon\Hooks\dev\</DalamudLibPath>
    </PropertyGroup>
    <PropertyGroup>
        <!-- Replace this with Umbra's directory is you have a local dev version installed. -->
        <!-- If you have Umbra installed through Dalamud, you can leave this as-is. -->
        <UmbraLibPath>C:\Path\To\Umbra</UmbraLibPath>
    </PropertyGroup>

    <Target Name="ResolveUmbraLibPath" BeforeTargets="PrepareForBuild">
        <PropertyGroup>
            <UmbraRootPath>$(appdata)\XIVLauncher\installedPlugins\Umbra</UmbraRootPath>
        </PropertyGroup>
        <ItemGroup>
            <UmbraDirectories Include="$(UmbraRootPath)\*" />
        </ItemGroup>
        <PropertyGroup Condition="@(UmbraDirectories->Count()) == 1">
            <UmbraLibPath>@(UmbraDirectories)\</UmbraLibPath>
        </PropertyGroup>
    </Target>

    <PropertyGroup>
        <TargetFramework>net8.0-windows</TargetFramework>
        <Platforms>x64</Platforms>
        <Nullable>enable</Nullable>
        <LangVersion>latest</LangVersion>
        <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
        <ProduceReferenceAssembly>false</ProduceReferenceAssembly>
        <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
        <OutputPath>out\$(Configuration)\</OutputPath>
    </PropertyGroup>

    <PropertyGroup>
        <Authors>Your Name</Authors>
        <Company>Sample Company</Company>
        <Version>1.0.0.0</Version>
        <Description>A description</Description>
        <Copyright>(C)2024</Copyright>
        <PackageProjectUrl>https://github.com/una-xiv/umbra</PackageProjectUrl>
        <PackageLicenseExpression>AGPL-3.0-or-later</PackageLicenseExpression>
        <IsPackable>false</IsPackable>
    </PropertyGroup>

    <ItemGroup>
        <Reference Include="Umbra">
            <HintPath>$(UmbraLibPath)Umbra.dll</HintPath>
            <Private>false</Private>
        </Reference>
        <Reference Include="Umbra.Common">
            <HintPath>$(UmbraLibPath)Umbra.Common.dll</HintPath>
            <Private>false</Private>
        </Reference>
        <Reference Include="Umbra.Game">
            <HintPath>$(UmbraLibPath)Umbra.Game.dll</HintPath>
            <Private>false</Private>
        </Reference>
        <Reference Include="Una.Drawing">
            <HintPath>$(UmbraLibPath)Una.Drawing.dll</HintPath>
            <Private>false</Private>
        </Reference>
        <Reference Include="Dalamud">
            <HintPath>$(DalamudLibPath)Dalamud.dll</HintPath>
            <Private>false</Private>
        </Reference>
        <Reference Include="FFXIVClientStructs">
            <HintPath>$(DalamudLibPath)FFXIVClientStructs.dll</HintPath>
            <Private>false</Private>
        </Reference>
        <Reference Include="Lumina">
            <HintPath>$(DalamudLibPath)Lumina.dll</HintPath>
            <Private>false</Private>
        </Reference>
        <Reference Include="Lumina.Excel">
            <HintPath>$(DalamudLibPath)Lumina.Excel.dll</HintPath>
            <Private>false</Private>
        </Reference>
    </ItemGroup>
</Project>
```
