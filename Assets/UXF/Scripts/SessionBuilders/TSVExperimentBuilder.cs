using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UXF
{
    public class TSVExperimentBuilder : MonoBehaviour, IExperimentBuilder
    {

        [Tooltip("The name key in the settings that contains the name of the trial specification file.")]
        [SerializeField] private string tsvFileKey = "trial_specification_name";
        [Tooltip("Enable to copy all settings from each trial in the TSV file to the the trial results output.")]
        [SerializeField] private bool copyToResults = true;

        /// <summary>
        /// Reads a TSV from filepath as specified in tsvFileKey in the settings.
        /// The TSV file is used to generate trials row-by-row, assigning a setting per column.
        /// </summary>
        /// <param name="session"></param>
        public void BuildExperiment(Session session)
        {
            // check if settings contains the tsv file name
            if (!session.settings.ContainsKey(tsvFileKey))
            {
                throw new Exception($"TSV file name not specified in settings. Please specify a TSV file name in the settings with key \"{tsvFileKey}\".");
            }

            // get the tsv file name
            string tsvName = session.settings.GetString(tsvFileKey);

            // check if the file exists
            string tsvPath = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, tsvName));
            if (!File.Exists(tsvPath))
            {
                throw new Exception($"TSV file at \"{tsvPath}\" does not exist!");
            }

            // read the tsv file
            string[] tsvLines = File.ReadAllLines(tsvPath);

            // parse as table
            var table = UXFDataTable.FromTSV(tsvLines);

            // build the experiment.
            // this adds a new trial to the session for each row in the table
            // the trial will be created with the settings from the values from the table
            // if "block_num" is specified in the table, the trial will be added to the block with that number
            session.BuildFromTable(table, copyToResults);
        }
    }

}
