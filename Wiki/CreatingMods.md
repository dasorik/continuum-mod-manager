# Creating Mods
A mod in continuum is represented as a configuration file containing a series of installation steps and other resources. During installation, the installation actions that are defined will be run in a specialized sequence (not always the order that they are defined in), altering the host game's files. As continuum tracks all file modifications, these mods can also be uninstalled by restoring file deltas that were captured during the installation process.

A mod file structure typically looks like the following:

`{mod-name}.mod`<br>
┣ `config.json` *(mandatory)*<br>
┣ `portait.png`<br>
┣ `icon.png`<br>
┗ `resources\`<br>
&nbsp; ┗ `{any files placed here can be access via the [Integration] relative path}`

The `config.json` file must be present in the mod's root folder, if this file is not present Continuum will reject adding the mod. The `portrait.png` and `background.png` files on the otherhand can be named anything (and can also be select other file formats), but it is good practice to stick to a consistent naming convention.

Files located within the `resources` can be referenced and used during the mod installation process. Use the path prefix '[Mod]' to reference any files that are placed inside the `resources` folder.

Once you have configured the mod files, simply archive (zip) the file and rename from `.zip` to `.mod`. From there the mod is ready to be added/installed via Continuum.

## Config File
The `config.json` file should contain the following properties
| Property | Type | Description |
| :--- | :--- | :--- |
| ModID | String | Defines the unique mod ID (must be globally unique). eg. `"Dasorik.ExampleMod"`. Can only contain alpha-numeric characters and '-', '.' or '\_'. |
| Version | String | Use this to internally track the current version of your mod, shown to users |
| DisplayName | String | The name of the mod, as shown in the UI |
| Author | [Contributor](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/Contributor.md) | This defines the primary author of the mod, only one primary author can be defined, other authors must be placed under 'Contributors' |
| Contributors | [Contributor](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/Contributor.md)[] | A list of additional contributors/authors of the mod. |
| DisplayImage | String | The file path (relative to the root of the mod files) of the image to display fof the mod. This image should be at least 512x512 pixels (or multiples of) in size |
| DisplayBackground | String | The file path (relative to the root of the mod files) of the large background image to display on the details page of the mod |
| Description | String | Outlines any information users need to know about the mod. Can use very limited markdown in this description (Use '#' at the start of a line for a header, and '\n' for a new-line) |
| LinkedIntegrations | [LinkedIntegration](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/LinkedIntegration.md)[] | A list of integrations that the mod is linked to (and the versions of the integration it is available for). |
| Settings | [SettingCategory](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/Settings.md)[] | A list of settings categories (used to group mod settings). |
| InstallActions | [InstallAction](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/InstallationActions.md)[] | A list of installation actions that the mod will execute when being installed. |

The end result of a config file will look similar to the example below:

```json
{
  "Version": "1.0",
  "ModID": "SampleAuthor.Sample",
  "Author": {
    "Name": "Sample Author",
    "Role": "Mod author"
  },
  "Contributors": [
    {
      "Name": "Sample Contributor #1",
      "Role": "Contributed to development"
    },
    {
      "Name": "Sample Contributor #1",
      "Role": "Moral support"
    }
  ],
  "DisplayName": "Sample Mod",
  "DisplayImage": "portrait.png",
  "DisplayBackground": "background.png",
  "Description": "#Info\nInclude details about your mod here!",
  "Settings": [
    {
      "Category": "Config",
      "Settings": [
        {
          "SettingID": "replace-icon",
          "SettingName": "Replace Icon",
          "Type": "Checkbox",
          "DefaultValue": true,
          "Description": "When enabled, this will replace the main player icon"
        }
      ]
    }
  ],
  "LinkedIntegrations": [
    {
      "IntegrationID": "example_game_id",
      "TargetVersion": "2.*",
      "MinimumVersion": "1.*",
      "ModCategory": "SampleCategory"
    }
  ],
  "InstallActions": [
    {
      "Action": "QuickBMSExtract",
      "UseAutoMapping": true,
      "DeleteWhenComplete": true,
      "TargetFiles": [
        "[Game]\\assets\\sample_asset.zip",
        "[Game]\\assets\\icons\\icon_bundle.zip"
      ]
    },
    {
      "Action": "WriteToFile",
      "TargetFile": "[Game]\\assets\\main_config.xml",
      "Content": [
        {
          "DataFilePath": "[Mod]\\write_content.txt",
          "StartOffset": 42438,
          "Replace": false
        }
      ]
    },
    {
      "Action": "ReplaceFile",
      "TargetFile": "[Game]\\player_icon.dds",
      "ReplacementFile": "[Mod]\\icon.dds",
      "Disabled": "$MOD.replace-icon = false"
    }
  ]
}
```
