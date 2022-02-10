## Automapping
During extraction, certain files may need to be routed or modified by mods. To remove the need for every single mod to repeat the same extraction steps an integration can specify automappings that will automatically be applied to extracted files. Automappings can apply to a specific archive or a series of archives and can run any sequence of [install actions](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/InstallationActions.md) to the newly extracted files (other than other actions thay may cause cascading automapping).

| Property  | Type | Description |
| :--- | :--- | :--- |
| TargetPath | String | The file path that the archive (or archives) are located |
| FileFilter | String | A regex pattern, any files inside of `TargetPath` that match this filter will have the `Actions` (below) performed on the extracted files |
| Actions | [InstallAction](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/InstallationActions.md)[] | The sequence of actions to perform on files extracted from the archive) |

Example
```json
{
  "QuickBMSAutoMappings": [
  {
    "TargetPath": "[Game]\\assets\\icons\\",
    "FileFilter": "icons.zip",
    "Actions": [
      {
        "Action": "MoveFile",
        "TargetFile": "[Game]\\assets\\icons\\player\\player_icon.png",
        "DestinationPath": "[Game]\\assets\\levels\\player_icon.gfx"
      }
    ]
  },
  {
    "TargetPath": "[Game]\\assets\\textures\\",
    "FileFilter": "[a-gA-G0-9]+\\.zip",
    "Actions": [
      {
        "Action": "MoveFiles",
        "TargetPath": "[Game]\\assets\\textures\\textures",
        "FileFilter": ".+\\.(?:jpg|png)",
        "IncludeSubfolders": false,
        "DestinationPath": "[Game]\\assets\\textures\\"
      }
    ]
  }
}
```
