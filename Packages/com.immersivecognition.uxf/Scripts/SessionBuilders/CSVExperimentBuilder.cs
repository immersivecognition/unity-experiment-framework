using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UXF
{
    public class CSVExperimentBuilder : MonoBehaviour, IExperimentBuilder
    {

        [Tooltip("The name key in the settings that contains the name of the trial specification file.")]
        [SerializeField] private string csvFileKey = "trial_specification_name";
        [Tooltip("Enable to copy all settings from each trial in the CSV file to the the trial results output.")]
        [SerializeField] private bool copyToResults = true;

        /// <summary>
        /// Reads a CSV from filepath as specified in csvFileKey in the settings.
        /// The CSV file is used to generate trials row-by-row, assigning a setting per column.
        /// </summary>
        /// <param name="session"></param>
        public void BuildExperiment(Session session)
        {
            // check if settings contains the csv file name
            if (!session.settings.ContainsKey(csvFileKey))
            {
                throw new Exception($"CSV file name not specified in settings. Please specify a CSV file name in the settings with key \"{csvFileKey}\".");
            }

            // get the csv file name
            string csvName = session.settings.GetString(csvFileKey);

            // check if the file exists
            string csvPath = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, csvName));
            if (!File.Exists(csvPath))
            {
                throw new Exception($"CSV file at \"{csvPath}\" does not exist!");
            }

            // read the csv file
            string[] csvLines = File.ReadAllLines(csvPath);

            // parse as table
            var table = UXFDataTable.FromCSV(csvLines);

            // build the experiment.
            // this adds a new trial to the session for each row in the table
            // the trial will be created with the settings from the values from the table
            // if "block_num" is specified in the table, the trial will be added to the block with that number
            session.BuildFromTable(table, copyToResults);
        }
    }

}
