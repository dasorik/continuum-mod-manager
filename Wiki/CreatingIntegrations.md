# Creating Integrations
Game integrations are an important part of Continuum, and no mods can be installed without first adding one. An integration represents a game (but this can also refresent any, even an arbitrary series of files), and mods are created and target a specific integration.

An integration's file structure typically looks like the following:

`{mod-name}.mod`<br>
┣ `config.json` *(mandatory)*<br>
┣ `portait.png`<br>
┣ `icon.png`<br>
┣ `icons\`<br>
┃ ┗ `{place category icons here - ideally .svg files}`<br>
┗ `resources\`<br>
&nbsp; ┗ `{any files placed here can be access via the [Integration] relative path}`

The `config.json` file must be present in the mod's root folder, if this file is not present Continuum will reject adding the integration. The `portrait.png` and `icon.png` files on the otherhand can be named anything (and can also be select other file formats), but it is good practice to stick to a consistent naming convention.

Files located within the `resources` can be referenced and used during the mod installation process. Use the path prefix '[Integration]' to reference any files that are placed inside the `resources` folder.

Once you have configured the integration's files, simply archive (zip) the file and rename from `.zip` to `.integration`. From there the integration is ready to be added to Continuum.

## Config File
The `config.json` file should contain the following properties
| Property | Type | Description |
| :--- | :--- | :--- |
| IntegrationID | String | Defines the unique integration ID (must be globally unique), and should reflect the game the integration is being build for. eg. `"example_game_name"`. Can only contain alpha-numeric characters and '-', '.' or '\_'. |
| Version | String | Use this to track the current version of the integration, If making new verions of an integration, ensure that any breaking changes increment the `{major}` version, while smaller non-breaking changes can just increment the `{minor}` version |
| DisplayName | String | The name of the integration, as shown in the UI |
| DisplayImage | String | The file path (relative to the root of the integration's files) of the image to display for the mod. This image should be at least 200x260 pixels (or multiples of) in size |
| DisplayIcon | String | The file path (relative to the root of the integration's files) of the icon image to display for this mod (as shown in the settings/navigation bar). This image should be at least 36x36 pixels (or multiples of) in size |
| Author | [Contributor](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/Contributor.md) | This defines the primary author of the integration, only one primary author can be defined, other authors must be placed under 'Contributors' |
| Contributors | [Contributor](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/Contributor.md)[] | A list of additional contributors/authors of the integration. |
| ModCompatibilityVersion | String | If an integration is updated (and it's major version increased), but it can still support mods created for an older version of the integration, then this should be set to the earliest version of the integration that supported those mods. <br><br> ie. The mod version is now at version `v3.0`, but mods created and targeting `v1.0` are still supported. The `ModCompatibilityVersion` should be set to `"1.0"` |
| TargetApplicationVersion | String | The targeted Continuum application version (in {major}.{minor} format). Can either be a specfic (exact) version (ie. `"2.5"`), or a wildcard (ie. `"2.\*"`, to support all `2.X` versions of the integration - wildcards can only be applied to minor versions). Applications versions higher than this version will only support the integration if the `ApplicationCompatibilityVersion` is less than or equal to `TargetApplicationVersion` or the `MinimumApplicationVersion` |
| MinimumApplicationVersion | String | The minimum compatible application (in {major}.{minor} format) that this integration is supported in. Can either be a specfic (exact) version (ie. `"2.5"`), or a wildcard (ie. `"2.\*"`, to target all `2.X` versions of the integration - wildcards can only be applied to minor versions). If left blank, the minimum version will be assumed to the the `TargetApplicationVersion`. |
| QuickBMSScript | String | The file path (relative to the root of the integration's files) where the QuickBMS script needed to extract the game's specific files is located. If needing to use QuickBMS for mods, ensure that this is defined |
| QuickBMSExtractMode | String | Must be either "RootFolder"/"NamedFolder"/"StaticFolder". Will default to "NamedFolder" if left blank (See section "QuickBMS Extract Modes" below for further details) |
| QuickBMSExtractPath | String | Only if using extract mode `StaticFolder` should property must be defined. Used to denote the relative file path of where extracted QuickBMS files (for a single file) should be copied from when moving into the game data folder |
| SetUpActions | [ModInstallAction](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/InstallationActions.md)[] | A list of installation actions that the integration will execute prior to allowing mods to be installed. Use this to set up the game files so that they are ready for modding (once uninstalling the integration, these changes will be reverted/uninstalled). See section 'Mod Install Actions' below for details on valid action definitions |
| QuickBMSAutomapping | [AutoMapping](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/Automapping.md)[] | The list of file automappings that should be applied to files after QuickBMS extract |
| UnzipAutoMappings | [AutoMapping](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/Automapping.md)[] | The list of file automappings that should be applied to files after zip extract |
| Categories | [ModCategory](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/CreatingIntegrations.md#mod-category)[] | Defines the list of categories that will be shown in the UI (allowing for filtering of mods by type). |
| Settings | [SettingCategory](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/Settings.md)[] | A list of settings categories (used to group mod settings). One of these settings must be named `install-path`, see below note for further details |

<br>

> ‼ Note: All game integrations must define the setting `install-path` (this can be placed under any setting category you wish). This directs the mod engine to where file modifications will take place. You can specify whatever validators you wish on this setting, but ideally this will include a `MandatoryField`, `PathExists` and `Regex` filter on the setting to ensure this is both a valid file path for the game in question.

### Mod Category
| Property | Type | Description |
| :--- | :--- | :--- |
| Category | String | Internally referenced name of the category |
| DisplayName | String | The name of the category as shown in the UI |
| IconPath | String | The file path, relative to the root integration folder that the icon for this category is located (this should be inside of the `icons\` folder) |

### QuickBMS Extract Modes
QuickBMS files can be extracted in different ways. Depending on the extract result, there may be a need to remove a root directory before copying files into the game directory. Extract modes allow you to define the extract root directory relative path (if applicable)
| Mode Name | Description |
| :--- | :--- |
| NamedFolder | Files extracted from the .zip file will be copied into the game directory relative to a folder of the same name as the .zip file (this is the default behaviour). |
| RootFolder | Files extracted from the .zip file will be copied into the game directory relative to the root folder of the extracted .zip |
| StaticFolder | Files extracted from the .zip file will be copied into the game directory relative to a statically named folder, defined by the `QuickBMSExtractPath` property on the game integration |

## Example
The end result of a config file will look similar to the below
```json
{
  "IntegrationID": "disney_infinity_3_0",
  "DisplayName": "Disney Infinity 3.0",
  "DisplayIcon": "icon.png",
  "DisplayImage": "portrait_3_0.png",
  "Author": "Dasorik",
  "Version": "1.0",
  "TargetApplicationVersion": "1.*",
  "MinimumApplicationVersion": "1.*",
  "QuickBMSScript": "disney_infinity.bms",
  "Categories": [
    {
      "Category": "Costumes",
      "DisplayName": "Costumes",
      "IconPath": "icons\\icon-costumes.svg"
    },
    {
      "Category": "Characters",
      "DisplayName": "Characters",
      "IconPath": "icons\\icon-characters.svg"
    }
  ],
  "Settings": [
    {
      "Category": "General",
      "Settings": [
        {
          "SettingName": "Installation Path",
          "SettingID": "install-path",
          "Type": "text",
          "DefaultValue": "",
          "Validations": [
            {
              "ValidationType": "Regex",
              "ErrorMessage": "Install path cannot contain the following symbols <>\"|?*",
              "Regex": "^[^<>\"|?*]+$"
            },
            {
              "ValidationType": "MandatoryField",
              "ErrorMessage": "Install path is a required field",
              "Required": true
            }
          ]
        }
      ]
    }
  ],
  "SetupActions": [
    {
      "Action": "QuickBMSExtract",
      "DeleteWhenComplete": true,
      "UseAutoMapping": true,
      "TargetFiles": [
        "[Game]\\assets\\textures\\textures.zip",
        "[Game]\\assets\\icons\\icons.zip"
      ]
    }
  ],
  "QuickBMSAutoMappings": [
    {
      "TargetPath": "[Game]\\assets\\textures\\",
      "FileFilter": "[a-gA-G0-9]+\\.zip",
      "Actions": [
        {
          "Action": "MoveFiles",
          "TargetPath": "[Game]\\assets\\textures\\textures",
          "FileFilter": ".+\\.tbody",
          "IncludeSubfolders": false,
          "DestinationPath": "[Game]\\assets\\textures\\"
        }
      ]
    }
  ]
}
```
