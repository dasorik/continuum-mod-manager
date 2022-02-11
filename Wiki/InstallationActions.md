# Mod Install Actions
The below sections outline the different installation actions that are available for mod `InstallActions` or for integration `SetUpActions`/Automapping

## CopyFileAction
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

## CopyFilesAction
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

## DeleteFilesAction
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

## MoveFileAction
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

## MoveFilesAction
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

## ReplaceFileAction
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

## ReplaceFilesAction
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

## WriteToFileAction
This action writes content/data to different files
| Property | Type | Description |
| :--- | :--- | :--- |
| TargetFile | String | The file path of the file that will be written to. |
| Content | [WriteContent](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/InstallationActions.md#write-content)[] | An array of write commands to be applied to the file (See below for write command definitions) |

### Write Content
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

## UnzipFileAction
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
  "UseAutomapping": false,
  "DeleteWhenComplete": true
}
```

## UnzipFilesAction
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
  "UseAutomapping": false,
  "DeleteWhenComplete": true
}
```

## ZipDirectoryAction
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

## ZipFilesAction
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

## QuickBMSExtractAction
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

## UnluacDecompileAction
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
All mod installation actions also have an additional property called `Disabled`. This setting takes a predicate in the format:

`{setting-name} {operator} {value}`:

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
