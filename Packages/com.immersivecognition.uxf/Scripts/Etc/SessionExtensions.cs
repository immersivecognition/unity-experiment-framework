using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    public static class SessionExtensions
    {
        
        /// <summary>
        /// Build the experiment using a UXFDataTable. 
        /// The table is used to generate trials row-by-row, assigning a setting per column.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="table"></param>
        public static void BuildFromTable(this Session session, UXFDataTable table, bool addSettingsToLog = true)
        {
            if (table == null) throw new ArgumentNullException("table");
            if (session == null) throw new ArgumentNullException("session");

            // setting keys are the same as headers, except with block num removed
            var settingsKeys = table.Headers
                .Where(h => h != "block_num")
                .ToArray();

            // if table does not contain block_num, we will use value of 1 later
            bool specifiedBlockNum = table.Headers.Contains("block_num");

            // loop down rows, creating trial for each one
            int rowNum = 0;
            var rows = table.GetAsListOfDict();
            foreach (var row in rows)
            {
                int blockNum;
                if (specifiedBlockNum)
                {
                    bool blockNumOk = int.TryParse(row["block_num"].ToString(), out blockNum);
                    if (!blockNumOk) throw new InvalidOperationException($"Error on row {rowNum}: Block number must be an integer.");
                }
                else
                {
                    blockNum = 1;
                }

                // keep creating blocks until we have enough
                while (session.blocks.Count() < blockNum)
                {
                    session.CreateBlock();
                }

                // create trial for the row
                var block = session.blocks[blockNum - 1];
                var newTrial = block.CreateTrial();

                // add all the columns to the settings for the trial
                foreach (var kvp in row)
                {
                    // skip block_num
                    if (kvp.Key == "block_num") continue;

                    // empty values do not assign a setting
                    if (kvp.Value.ToString().Trim() == string.Empty) continue;

                    // add trial setting                    
                    newTrial.settings.SetValue(kvp.Key, kvp.Value);

                    // possibly mark the setting to be logged in the results output
                    if (addSettingsToLog && !session.settingsToLog.Contains(kvp.Key))
                    {
                        session.settingsToLog.Add(kvp.Key);                        
                    }
                }

                rowNum++;
            }
        }

    }

}
