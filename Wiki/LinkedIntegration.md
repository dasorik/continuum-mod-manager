## Linked Integration
Some mods may need to support multiple games (eg. A game has multiple different versions, which may have slightly different modding requirements). In this scenario you can opt to make a mod compatible with one or more game integration.

| Property  | Type | Description |
| :--- | :--- | :--- |
| IntegrationID | String | The ID of the targeted game integration |
| TargetVersion | String | The targeted game integration version (in {major}.{minor} format). Can either be a specfic (exact) version (ie. "2.5"), or a wildcard (ie. "2.\*", to support all 2.X versions of the integration - wildcards can only be applied to minor versions). Integration versions higher than this version will only support the mod if their `ModCompatibilityVersion` is less than or equal to `TargetVersion` or the `MiniumumVersion` |
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
