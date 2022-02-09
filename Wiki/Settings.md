# Settings
Mods can define settings that can be used to alter the mod installation process. Settings are bundled into 'groups' which will be shown together on the mod details page.

## Setting Category
| Property  | Type | Description |
| :--- | :--- | :--- |
| Category | String | The settings category group name |
| Settings | [Setting](https://github.com/dasorik/continuum-mod-manager/edit/main/Wiki/Settings.md#setting)[] | A list of settings that can be configured on the mod prior to installing |

## Setting
| Property  | Type | Description |
| :--- | :--- | :--- |
| SettingID | String | Defines the unique setting's ID (must be unique within the mod). eg. `"replace-icon"`. Can only contain alpha-numeric characters and '-' or '\_'. |
| SettingName | String | The name of the setting as shown on the UI |
| Description | String | Tool-tip text that is shown on hover (providing additional detail on what the setting does) |
| Type | SettingType | Defines which control is used for this setting. Must be either `Text`, `Checkbox` or `Dropdown` |
| DefaultValue | Object | Defines the default state of the settings that will be applied on first load (ie. for a checkbox -> `true`) |
| Validations | [SettingValidator](https://github.com/dasorik/continuum-mod-manager/edit/main/Wiki/Settings.md#setting-validators)[] | (Only applicable for 'Text' settings) A list of validators that will be executed against the value of the textbox. Mods cannot be installed if any of thier settings are triggering one or more of the validators assigned to a setting. Validators are detailed further below. |
| Options | [SettingOption](https://github.com/dasorik/continuum-mod-manager/edit/main/Wiki/Settings.md#setting-option)[] | (Only applicable for 'Dropdown' settings) A list of 'Text'/'Value' pairs that are used to define the list of available drop down options that can be selected from |

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

## Setting Validators
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

## Setting Option
| Property  | Type | Description |
| :--- | :--- | :--- |
| Text | String | The textual name of the option, shown in the UI |
| Value | String | The value of the setting (this is the value when referencing via `$MOD.setting-name`) |

Example
```json
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
```
