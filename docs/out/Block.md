#### `UXF.Block`
*A set of trials, often used to group a number of consecutive Trial objects that share something in common.*
---
### Fields
`trials`: List of trials associated with this block
`settings`: Block settings. These will be overidden by trial settings if set.
### Properties
`firstTrial`: Return the first trial in this block
`lastTrial`: Return the last trial in this block
`number`: Returns the block number of this block, based on its position in the session.
`session`: The session associated with this block
### Methods
`UXF.Block(System.UInt32,UXF.Session)`
Create a block with a given number of trials under a given session
`CreateTrial`
Create a trial within this block
`GetRelativeTrial(System.Int32)`
Get a trial in this block by relative trial number (non-zero indexed)
---
*Note: This file was automatically generated*
