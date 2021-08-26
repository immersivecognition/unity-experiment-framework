using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    public static class SessionExtensions
    {
        
        public static void TryBuildFromTable(this Session session, UXFDataTable table)
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
                    if (kvp.Key == "block_num") continue;
                    newTrial.settings.SetValue(kvp.Key, kvp.Value);
                }

                rowNum++;
            }
        }

    }

}
