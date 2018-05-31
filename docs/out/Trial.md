### `UXF.Trial`
*The base unit of experiments. A Trial is usually a singular attempt at a task by a participant after/during the presentation of a stimulus.*
---
## Fields
`status` *Status of the trial (enum)*
`block` *The block associated with this session*
`settings` *Trial settings. These will override block settings if set.*
`result` *Dictionary of results in a order.*
## Properties
`number` *Returns non-zero indexed trial number. This is generated based on its position in the block, and the ordering of the blocks within the session.*
`numberInBlock` *Returns non-zero indexed trial number for the current block.*
`session` *The session associated with this trial*
## Methods
### `#ctor(UXF.Block)`
*Manually create a trial. When doing this you need to add this trial to a block with block.trials.Add(trial)*
### `SetReferences(UXF.Block)`
*Set references for the trial.*
### `Begin`
*Begins the trial, updating the current trial and block number, setting the status to in progress, starting the timer for the trial, and beginning recording positions of every object with an attached tracker*
### `End`
*Ends the Trial, queues up saving results to output file, stops and saves tracked object data.*
---
*Note: This file was automatically generated*
