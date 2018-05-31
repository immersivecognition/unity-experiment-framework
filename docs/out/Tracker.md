#### `UXF.Tracker`
*Create a new class that inherits from this component to create custom tracking behaviour on a frame-by-frame basis.*
---
### Fields
`objectName` Name of the object used in saving
`measurementDescriptor` Description of the type of measurement this tracker will perform.
`customHeader` Custom column headers for tracked objects
### Properties
`pathHeader` The header used when saving the relative filename string within our behavioural data.
`header` The header that will go at the top of the output file associated with this tracker
### Methods
#### `StartRecording`
Begins recording.
#### `PauseRecording`
Pauses recording.
#### `StopRecording`
Stops recording.
#### `GetDataCopy`
Returns a copy of the data collected by this tracker.
#### `GetCurrentValues`
Acquire values for this frame and store them in an array.
#### `SetupDescriptorAndHeader`
Override this method and define your own descriptor and header.
---
*Note: This file was automatically generated*
