# Creating Mods
A mod file structure typically looks like the following:

`{mod-name}.mod`<br>
┠─ `config.json` *(mandatory)*<br>
┠─ `portait.png`<br>
┠─ `background.png`<br>
└─ `resources\`<br>
┈ ┈ └─ `{any files placed here can be access via the [Mod] relative path}`

The `config.json` file must be present in the mods root folder, if this file is not present Continuum will reject adding the mod. The `portrait.png` and `background.png` files on the otherhand can be named anything (and can also be select other file formats), but it is good practice to stick to a consistent naming convention.

Files located within the `resources` can be referenced and used during the mod installation process. Use the path prefix '[Mod]' to reference any files that are placed inside the `resources` folder.

Once you have configured the mod files, simply archive (zip) the file and rename from `.zip` to `.mod`. From there the mod is ready to be added/installed via Continuum.

## Config File
The `config.json` file should contain the following properties
| Property | Type | Description |
| :--- | :--- | :--- |
| ModID | String | Defines the unique mod ID (must be globally unique). eg. `"Dasorik.ExampleMod"`. Can only contain alpha-numeric characters and '-', '.' or '\_'. |
| Version | String | Use this to internally track the current version of your mod, shown to users |
| DisplayName | String | The name of the mod, as shown in the UI |
| Author | ModContributor | This defines the primary author of the mod, only one primary author can be defined, other authors must be placed under 'Contributors' |
| Contributors | ModContributor[] | A list of additional contributors/authors of the mod. |
| DisplayImage | String | The file path (relative to the root of the mod files) of the image to display fof the mod. This image should be at least 512x512 pixels (or multiples of) in size |
| DisplayBackground | String[] | The file path (relative to the root of the mod files) of the large background image to display on the details page of the mod |
| Description | String | Outlines any information users need to know about the mod. Can use very limited markdown in this description (Use '#' at the start of a line for a header, and '\n' for a new-line) |
| LinkedIntegrations | LinkedIntegration[] | A list of integrations that the mod is linked to (and the versions of the integration it is available for). See 'Linked Integrations' below for details. |
| Settings | ModSettingCategory[] | A list of settings categories (used to group mod settings). See section 'Mod Settings' below for details. |
| InstallActions | ModInstallAction[] | A list of installation actions that the mod will execute when being installed. See section 'Mod Install Actions' below for details on valid action definitions |

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

### Mod Contributors
The below table outlines the property definition for the `ModContributors` object. Used for the `Author` and `Contributors` properties of the `config.json`
| Property  | Type | Description |
| :--- | :--- | :--- |
| Name | String | The name of the contributor |
| Role | String | Description text to outline the contribution of the user defined under `Name` |

Example
```json
{
  "Name": "Example Author",
  "Role": "Mod Author / Texture Artist"
}
```

### Linked Integrations
Some mods may need to support multiple games (eg. A game has multiple different versions, which may have slightly different modding requirements). In this scenario you can opt to make a mod compatible with one or more game integration.

| Property  | Type | Description |
| :--- | :--- | :--- |
| IntegrationID | String | The ID of the targeted game integration |
| TargetVersion | String | The targeted version game integration (in {major}.{minor} format). Can either be a specfic (exact) version (ie. "2.5"), or a wildcard (ie. "2.\*", to support all 2.X versions of the integration - wildcards can only be applied to minor versions). Integration versions higher than this version will only support the mod if their `ModCompatibilityVersion` is less than or equal to `TargetVersion` or the `MiniumumVersion` |
| MinimumVersion | String | The minimum compatible version of the game integration (in {major}.{minor} format) that this mod is supported in. Can either be a specfic (exact) version (ie. "2.5"), or a wildcard (ie. "2.\*", to target all 2.X versions of the integration - wildcards can only be applied to minor versions). If left blank, the minimum version will be assumed to the the `TargetVersion`. |
| ModCategory | String | This defines the category that the mod will be shown under in the UI (this can link to one of the categories defined on the respective integration) |

Example
```json
{
  "IntegrationID": "example_game_id",
  "TargetVersion": "2.*"
  "MinimumVersion": "1.2",
  "ModCategory": "Hats"
}
```

## Settings
Mods can define settings that can be used to alter the mod installation process. Settings are bundled into 'groups' which will be shown together on the mod details page.

### ModSettingCategory
| Property  | Type | Description |
| :--- | :--- | :--- |
| Category | String | The settings category group name |
| Settings | ModSetting[] | A list of settings that can be configured on the mod prior to installing |

### ModSetting
| Property  | Type | Description |
| :--- | :--- | :--- |
| SettingID | String | Defines the unique setting's ID (must be unique within the mod). eg. `"replace-icon"`. Can only contain alpha-numeric characters and '-' or '\_'. |
| SettingName | String | The name of the setting as shown on the UI |
| Description | String | Tool-tip text that is shown on hover (providing additional detail on what the setting does) |
| Type | ModSettingType | Defines which control is used for this setting. Must be either 'Text', 'Checkbox' or 'Dropdown' |
| DefaultValue | Object | Defines the default state of the settings that will be applied on first load (ie. for a checkbox -> `true`) |
| Validations | ModSettingValidator[] | (Only applicable for 'Text' settings) A list of validators that will be executed against the value of the textbox. Mods cannot be installed if any of thier settings are triggering one or more of the validators assigned to a setting. Validators are detailed further below. |
| Options | ModSettingValidator[] | (Only applicable for 'Dropdown' settings) A list of 'Text'/'Value' pairs that are used to define the list of available drop down options that can be selected from |

Example
```json
{
  "Category": "Icon Settings",
  "Settings": [
    {
      "SettingID": "replace-icon",
      "SettingName": "Replace Icon",
      "Description": "Replaces the character icon with a custom one",
      "Type": "Checkbox",
      "DefaultValue": true
    },
    {
      "SettingID": "replace-icon-type",
      "SettingName": "Replacement Icon Type",
      "Description": "Defines which icon will be used to replace the character icon",
      "Type": "Dropdown",
      "DefaultValue": "icon_2",
      "Options": [
        {
          "Text": "Icon #1",
          "Value": "icon_1"
        },
        {
          "Text": "Icon #2",
          "Value": "icon_2"
        }
      ]
    }
  ]
}
```

### Validations
Text settings can define a list of validators that will be executed against the value of the textbox. Mods cannot be installed if any of thier settings are triggering one or more of the validators assigned to a setting. There are currently three types of validators available:

* MandatoryField
  * Ensures that the text field has a non-null or empty value
* PathExists
  * The value of the textbox must map to a path with an optional check that looks for a specific file (`PathSuffix`) at the target destination
* Regex
  * The textbox value must match the pattern defined via the `Regex` property

| Property | Type | Description |
| :--- | :--- | :--- |
| ValidationType | String | Must be either "MandatoryField", "PathExists", or "Regex" |
| ErrorMessage | String | Text to show if the field fails validation |
| PathSuffix | String | (PathExists validator only - Optional) A path suffix that is appended on to the end of the textbox value that must also exist |
| Regex | String | (Regex validator only) A regex pattern that the textbox value must match in it's entirety |

Example
```json
{
  "SettingID": "replace-path",
  "SettingName": "Replacement Path",
  "Type": "Text",
  "Validations": [
    {
      "ValidationType": "MandatoryField",
      "ErrorMessage": "Replacement path must be defined"
    },
    {
      "ValidationType": "PathExists",
      "ErrorMessage": "Defined path is not a valid game directory",
      "PathSuffix": "game_launcher.exe"
    },
    {
      "ValidationType": "Regex",
      "ErrorMessage": "Defined path does not match expected value",
      "Regex": "[a-zA-Z]*"
    }
  ]
},
```

## Mod Install Actions
#### CopyFileAction
This action copies a single file from one location to another
| Property | Type | Description |
| :--- | :--- | :--- |
| TargetFile | String | File path of the file to copy. Can be relative to the [Mod], [Game], or [Integration] folders. |
| DestinationPath | String | The 'TargetFile' will be copied to this path. Must be relative to the [Game] folder. |

Example
```json
{
  "Action": "CopyFile",
  "TargetFile": "[Mod]\\test_asset.xml",
  "DestinationPath": "[Game]\\assets\\character_list.xml",
},
```

#### CopyFilesAction
This action copies multiple files from one location to another
| Property | Type | Description |
| :--- | :--- | :--- |
| TargetPath | String | File path of the folder to copy files from. Can be relative to the [Mod], [Game], or [Integration] folders. |
| FileFilter | String | Regex pattern used to filter included files from the target folder. Use .* to include all files. |
| IncludeSubfolders  | Boolean | If enabled, all files in the subfolders will be copied (If matching the 'FileFilter', this will retain file heirachy at the destination). |
| DestinationPath  | String | All filtered files in the 'TargetPath' will be copied to this path. Must be relative to the [Game] folder. |


Example
```json
{
  "Action": "CopyFiles",
  "TargetPath": "[Mod]\\config_data\\",
  "FileFilter": ".*",
  "IncludeSubfolders": true,
  "DestinationPath": "[Game]\\assets\\",
},
```

#### DeleteFilesAction
This action deletes the specified files
| Property | Type | Description |
| :--- | :--- | :--- |
| TargetFiles | String[] | A list of files to delete, must be relative to the [Game] folder |


Example
```json
{
  "Action": "DeleteFiles",
  "TargetFiles": [
    "[Game]\\config_data\\character.xml",
    "[Game]\\images\\icons\\icon.png",
  ]
},
```

#### MoveFileAction
This action moves a file from one location to another
| Property | Type | Description |
| :--- | :--- | :--- |
| TargetFile | String | File path of the file to move. Must be relative to the [Game] folder. |
| DestinationPath | String | The 'TargetFile' will be moved to this path. Must be relative to the [Game] folder. |

Example
```json
{
  "Action": "MoveFile",
  "TargetFile": "[Game]\\assets\\character_list.xml",
  "DestinationPath": "[Game]\\assets_new\\character_list.xml",
},
```

#### MoveFilesAction
This action moves multiple files from one location to another
| Property | Type | Description |
| :--- | :--- | :--- |
| TargetPath | String | File path of the folder to move files from. Must be relative to the [Game] folder. |
| FileFilter | String | Regex pattern used to filter included files from the target folder. Use .* to include all files. |
| IncludeSubfolders | Boolean | If enabled, all files in the subfolders will be moved (If matching the 'FileFilter', this will retain file heirachy at the destination). |
| DestinationPath | String | All filtered files in the 'TargetPath' will be moved to this path. Must be relative to the [Game] folder. |

Example
```json
{
  "Action": "MoveFiles",
  "TargetPath": "[Game]\\config_data\\",
  "FileFilter": ".*?\\.json",
  "IncludeSubfolders": true,
  "DestinationPath": "[Game]\\assets\\",
},
```

#### ReplaceFileAction
This action moves multiple files from one location to another

> 💬 NOTE: Try and avoid using file replace actions on commonly edited files, as this may cause mod conflicts. Consider using a 'WriteToFile' action instead

| Property | Type | Description |
| :--- | :--- | :--- |
| ReplacementFile | String  | The path to the file that will replace another. Must be relative to the [Mod] or [Integration] folders. |
| TargetFile | String | The path of the file that will be replaced with the contents of 'ReplacementFile'. Must be relative to the [Game] folder. |

Example
```json
{
  "Action": "ReplaceFile",
  "ReplacementFile": "[Mod]\\assets\\character_list.xml",
  "TargetFile": "[Game]\\assets\\character_list.xml",
},
```

#### ReplaceFilesAction
This action moves multiple files from one location to another

> 💬 NOTE: Try and avoid using file replace actions on commonly edited files, as this may cause mod conflicts. Consider using a 'WriteToFile' action instead

| Property | Type | Description |
| :--- | :--- | :--- |
| ReplacementPath | String | File path of the folder containing the files that will replace others. Must be relative to the [Mod] or [Integration] folders. |
| FileFilter | String | Regex pattern used to filter included files from the replacement folder. Use .* to include all files. |
| IncludeSubfolders | Boolean | If enabled, all files in the subfolders will be moved (If matching the 'FileFilter', this will replace based on file heirachy at the destination). |
| TargetPath | String | All filtered files in the 'ReplacementPath' will be moved to this path (and replace contents). Must be relative to the [Game] folder. |

Example
```json
{
  "Action": "ReplaceFiles",
  "ReplacementPath": "[Mod]\\config_data\\",
  "FileFilter": ".*?\\.json",
  "IncludeSubfolders": true,
  "DestinationPath": "[Game]\\config_data\\",
},
```

#### WriteToFileAction
This action writes content/data to different files
| Property | Type | Description |
| :--- | :--- | :--- |
| TargetFile | String | The file path of the file that will be written to. |
| Content | WriteContent[] | An array of write commands to be applied to the file (See below for write command definitions) |

##### WriteContent
| Property | Type | Description |
| :--- | :--- | :--- |
| StartOffset | Long | The starting byte offset of the file to start writing content |
| EndOffset | Long? | The end byte offset to stop writing at. if defined, all data between 'StartOffset' and 'EndOffset' will be replaced with the written text content (Even if larger/smaller than the range between the two offsets, this will increase or decrease the size of the file and shift the remaining bytes to accomodate). |
| DataFilePath | String | The file path of a file that contains the data to be written from 'StartOffset'. Cannot be used in conjunction with the 'Text' property |
| Text | String | The text to be written from 'StartOffset'. Cannot be used in conjunction with the 'DataFilePath' property |
| Replace | Long? | If enabled, and no 'EndOffset' is defined, this is used to instruct the file writer to overwrite the content being written to the file starting from 'StartOffset'. If set to 'false' (the default value), the size of the file will be increased and bytes inserted from the 'StartIndex' |

Example
```json
{
  "Action": "WriteToFile",
  "TargetFile": "[Game]\\assets\\main_config.xml",
  "Content": [
    {
      "DataFilePath": "[Mod]\\write_content.txt",
      "StartOffset": 42438,
      "Replace": false
    },
    {
      "Text": "__Character_ID=56",
      "StartOffset": 42438,
      "EndOffset": 42440
    }
  ]
}
```

#### UnzipFileAction
This action unzips a .zip file
| Property | Type | Description |
| :--- | :--- | :--- |
| TargetFile | String | The file path of the .zip file to extract. Must be relative to the [Game] folder. |
| DestinationPath | String | The file path where the .zip file will be extracted to. Must be relative to the [Game] folder. |
| UseAutoMapping | Boolean | With this option enabled, extracted files will be automatically mapped to the correct location, as defined via the integration's `UnzipAutomapping` settings (See 'Creating Integrations' for details about configuring automappings) |
| DeleteWhenComplete | Boolean | If enabled, the specified 'TargetFile' will be deleted once it has been extracted |

Example
```json
{
  "Action": "UnzipFile",
  "TargetFile": "[Game]\\assets\\main_config.zip",
  "DestinationPath": "[Game]\\assets\\main_config_extracted",
  "UnzipAutomapping": false,
  "DeleteWhenComplete": true
}
```

#### UnzipFilesAction
This action unzips multiple .zip files
| Property | Type | Description |
| :--- | :--- | :--- |
| TargetFiles | String[] | A list of file paths of .zip file to extract. Must be relative to the [Game] folder. |
| ExtractToSameDirectory | Boolean | By default extracted files will be placed at the same level of the .zip file in a folder with the same name. By turning this on, files will be extracted to the same level as the zip, without creating additional folders |
| UseAutoMapping | Boolean | With this option enabled, extracted files will be automatically mapped to the correct location, as defined via the integration's `UnzipAutomapping` settings (See 'Creating Integrations' for details about configuring automappings) |
| DeleteWhenComplete | Boolean | If enabled, the specified 'TargetFiles' will be deleted once they have been extracted |

Example
```json
{
  "Action": "UnzipFiles",
  "TargetFiles": [
    "[Game]\\assets\\main_config.zip",
    "[Game]\\assets\\secondary_config.zip"
  ]
  "ExtractToSameDirectory": false,
  "UnzipAutomapping": false,
  "DeleteWhenComplete": true
}
```

#### ZipDirectoryAction
This action archives (zips) files in a folder
| Property | Type | Description |
| :--- | :--- | :--- |
| DirectoryPath | String | The file path to the directory to be archived. Must be relative to the [Game] folder. |
| DestinationPath | String | The file path defining where the archive (.zip) will be placed once created. Must be relative to the [Game] folder. |
| DeleteDirectoryWhenComplete | Boolean | With this option enabled, the archived folder will be deleted once the archive process is complete |
| ReplaceFileAtDestination | Boolean | If enabled, any pre-existing file at 'DestinationPath' will be replaced. If not enabled and a file exists at the destination, this step will fail |
| IncludeBaseDirectory | Boolean | Defines if the root of the archive is the 'DirectoryPath' folder, or the files contained within the folder |

Example
```json
{
  "Action": "ZipDirectory",
  "DirectoryPath": "[Game]\\assets\\main_config\\",
  "DestinationPath": "[Game]\\assets\\main_config.zip"
  "DeleteDirectoryWhenComplete": true,
  "ReplaceFileAtDestination": true,
  "IncludeBaseDirectory": false
}
```

#### ZipFilesAction
This action archives (zips) files in a folder
| Property | Type | Description |
| :--- | :--- | :--- |
| FilesToInclude | String[] | The list of files to include in the archive. All files must be relative to the [Game] folder. |
| DestinationPath | String | The file path defining where the archive (.zip) will be placed once created. Must be relative to the [Game] folder. |
| DeleteFilesWhenComplete | Boolean | With this option enabled, the archived files will be deleted once the archive process is complete |
| ReplaceFileAtDestination | Boolean | If enabled, any pre-existing file at 'DestinationPath' will be replaced. If not enabled and a file exists at the destination, this step will fail |

Example
```json
{
  "Action": "ZipFiles",
  "FilesToInclude": [
    "[Game]\\assets\\resource_1.xml",
    "[Game]\\assets\\icon.png"
  ]
  "DestinationPath": "[Game]\\assets\\main_config.zip"
  "DeleteFilesWhenComplete": true,
  "ReplaceFileAtDestination": true
}
```

#### QuickBMSExtractAction
This action runs a QuickBMS extract on the target files

> 💬 NOTE: This action is only available for integrations that define a `QuickBMSScript`. If the current integration does not define this, these steps will fail

| Property  | Type | Description |
| :--- | :--- | :--- |
| TargetFiles  | String[]  | The list of files to QuickBMS extract. Must be relative to the [Game] folder. |
| UseAutoMapping  | Boolean | With this option enabled, extracted files will be automatically mapped to the correct location, as defined via the integration's `QuickBMSAutomapping` settings (See 'Creating Integrations' for details about configuring automappings) |
| DeleteWhenComplete  | Boolean | If enabled, the specified 'TargetFiles' will be deleted once they have been extracted |

Example
```json
{
  "Action": "QuickBMSExtract",
  "UseAutoMapping": true,
  "DeleteWhenComplete": true,
  "TargetFiles": [
    "[Game]\\assets\\sample_asset.zip",
    "[Game]\\assets\\icons\\icon_bundle.zip"
  ]
},
```

#### UnluacDecompileAction
This action decompiles a lua file using Unluac

| Property  | Type | Description |
| :--- | :--- | :--- |
| TargetFiles | String[] | The list of files to Unluac decompile. Must be relative to the [Game] folder. |

Example
```json
{
  "Action": "UnluacDecompile",
  "TargetFiles": [
    "[Game]\\assets\\player_movement.lua",
    "[Game]\\assets\\ai\\enemy_spawn.lua"
  ]
},
```

## Disabling Mod Actions
All mod installation actions also have an additional property called `Disabled`. This setting takes a predicate in the format `{setting-name} {operator} {value}`:

* Setting Name
  * Must reference one of the settings from either the mod or integration.
  * Mod settings are referenced via `$MOD.{setting-name}` (eg. `$MOD.replace-icon`)
  * Integration settings are referenced via `$INTEGRATION.{setting-name}` (eg. `$MOD.replace-icon`)
  * A third special value can be used in lieu of a mod/integration setting. `$INTEGRATION_ID` can be used to optionally enable/disable installation steps depending on the game integration the mod is being installed for (you can use this to create integration specific installation steps if targeting multiple integrations)
* Operator
  * Any of the normal arithmatic operators are available to use in this query ('=', '<', '>', '<=', '>=', '!=')
* Value
  * This can either be a constant value (ie. text, a number, or a boolean value), or can be a reference to another setting

```json
{
  "Action": "CopyFile",
  "TargetFile": "[Game]\\player_icon.xml",
  "DestinationPath": "[Game]\\player_icon_backup.xml",
  "Disabled": "$MOD.replace-icon = false"
}
```

In the example above, if the setting `replace-icon` is set to `false` the 'CopyFile' action will not be enabled during mod install, and `player_icon_backup.xml` will not be created.
