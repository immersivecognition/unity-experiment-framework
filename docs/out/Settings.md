#### `UXF.Settings`
*Class which handles the cascading settings system. Wraps a Dictionary.*
---
### Fields
*None*### Properties
`empty`: Returns a new empty settings object.
`baseDict`: The underlying dictionary
`Keys`: The keys for the underlying dictionary
`Item(System.String)`: Get a setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. If one is never found, it will return null.
### Methods
`UXF.Settings(System.Collections.Generic.Dictionary{System.String,System.Object})`
Creates Settings instance from dictionary
`SetParent(UXF.Settings)`
Sets the parent setting object, which is accessed when a setting is not found in the dictionary.
`Add(System.String,System.Object)`
Add a new setting to the dictionary
---
*Note: This file was automatically generated*
