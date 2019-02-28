/*
 * 2006 - 2016 Ted Spence, http://tedspence.com
 * License: http://www.apache.org/licenses/LICENSE-2.0 
 * Home page: https://github.com/tspence/csharp-csv-reader
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;

namespace CSVFile
{
    public static class CSV
    {
        public const char DEFAULT_DELIMITER = ',';
        public const char DEFAULT_QUALIFIER = '"';
        public const char DEFAULT_TAB_DELIMITER = '\t';

        #region Reading CSV Formatted Data
        /// <summary>
        /// Parse a line whose values may include newline symbols or CR/LF
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public static string[] ParseMultiLine(StreamReader sr, char delimiter = DEFAULT_DELIMITER, char text_qualifier = DEFAULT_QUALIFIER)
        {
            StringBuilder sb = new StringBuilder();
            string[] array = null;
            while (!sr.EndOfStream) {

                // Read in a line
                sb.Append(sr.ReadLine());

                // Does it parse?
                string s = sb.ToString();
                if (TryParseLine(s, delimiter, text_qualifier, out array)) {
                    return array;
                }

                // We didn't succeed on the first try - our line must have an embedded newline in it
                sb.Append("\n");
            }

            // Fails to parse - return the best array we were able to get
            return array;
        }

        /// <summary>
        /// Parse the line and return the array if it succeeds, or as best as we can get
        /// </summary>
        /// <param name="s"></param>
        /// <param name="delimiter"></param>
        /// <param name="text_qualifier"></param>
        /// <returns></returns>
        public static string[] ParseLine(string s, char delimiter = DEFAULT_DELIMITER, char text_qualifier = DEFAULT_QUALIFIER)
        {
            string[] array = null;
            TryParseLine(s, delimiter, text_qualifier, out array);
            return array;
        }

        /// <summary>
        /// Read in a line of text, and use the Add() function to add these items to the current CSV structure
        /// </summary>
        /// <param name="s"></param>
        public static bool TryParseLine(string s, char delimiter, char text_qualifier, out string[] array)
        {
            bool success = true;
            List<string> list = new List<string>();
            StringBuilder work = new StringBuilder();
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];

                // If we are starting a new field, is this field text qualified?
                if ((c == text_qualifier) && (work.Length == 0)) {
                    int p2;
                    while (true) {
                        p2 = s.IndexOf(text_qualifier, i + 1);

                        // for some reason, this text qualifier is broken
                        if (p2 < 0) {
                            work.Append(s.Substring(i + 1));
                            i = s.Length;
                            success = false;
                            break;
                        }

                        // Append this qualified string
                        work.Append(s.Substring(i + 1, p2 - i - 1));
                        i = p2;

                        // If this is a double quote, keep going!
                        if (((p2 + 1) < s.Length) && (s[p2 + 1] == text_qualifier)) {
                            work.Append(text_qualifier);
                            i++;

                            // otherwise, this is a single qualifier, we're done
                        } else {
                            break;
                        }
                    }

                    // Does this start a new field?
                } else if (c == delimiter) {
                    list.Add(work.ToString());
                    work.Length = 0;

                    // Test for special case: when the user has written a casual comma, space, and text qualifier, skip the space
                    // Checks if the second parameter of the if statement will pass through successfully
                    // e.g. "bob", "mary", "bill"
                    if (i + 2 <= s.Length - 1) {
                        if (s[i + 1].Equals(' ') && s[i + 2].Equals(text_qualifier)) {
                            i++;
                        }
                    }
                } else {
                    work.Append(c);
                }
            }
            list.Add(work.ToString());

            // If we have nothing in the list, and it's possible that this might be a tab delimited list, try that before giving up
            if (list.Count == 1 && delimiter != DEFAULT_TAB_DELIMITER) {
                string[] tab_delimited_array = ParseLine(s, DEFAULT_TAB_DELIMITER, DEFAULT_QUALIFIER);
                if (tab_delimited_array.Length > list.Count) {
                    array = tab_delimited_array;
                    return success;
                }
            }

            // Return the array we parsed
            array = list.ToArray();
            return success;
        }
        #endregion

        #region DataTable related functions (not available on dot-net-portable)
#if !PORTABLE
        /// <summary>
        /// Read in a single CSV file into a datatable in memory
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="delim">The CSV field delimiter character.</param>
        /// <param name="qual">The CSV text qualifier character.</param>
        /// <returns>An data table of strings that were retrieved from the CSV file.</returns>
        public static DataTable LoadDataTable(string filename, bool first_row_are_headers = true, bool ignore_dimension_errors = true, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER)
        {
            return LoadDataTable(new StreamReader(filename), first_row_are_headers, ignore_dimension_errors, delim, qual);
        }

        /// <summary>
        /// Read in a single CSV file into a datatable in memory
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="delim">The CSV field delimiter character.</param>
        /// <param name="qual">The CSV text qualifier character.</param>
        /// <returns>An data table of strings that were retrieved from the CSV file.</returns>
        public static DataTable LoadDataTable(StreamReader stream, bool first_row_are_headers = true, bool ignore_dimension_errors = true, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER)
        {
            using (CSVReader cr = new CSVReader(stream, delim, qual)) {
                return cr.ReadAsDataTable(first_row_are_headers, ignore_dimension_errors, null);
            }
        }

        /// <summary>
        /// Read in a single CSV file into a datatable in memory
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="delim">The CSV field delimiter character.</param>
        /// <param name="qual">The CSV text qualifier character.</param>
        /// <returns>An data table of strings that were retrieved from the CSV file.</returns>
        public static DataTable LoadDataTable(string filename, string[] headers, bool ignore_dimension_errors = true, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER)
        {
            return LoadDataTable(new StreamReader(filename), headers, ignore_dimension_errors, delim, qual);
        }

        /// <summary>
        /// Read in a single CSV file into a datatable in memory
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="delim">The CSV field delimiter character.</param>
        /// <param name="qual">The CSV text qualifier character.</param>
        /// <returns>An data table of strings that were retrieved from the CSV file.</returns>
        public static DataTable LoadDataTable(StreamReader stream, string[] headers, bool ignore_dimension_errors = true, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER)
        {
            using (CSVReader cr = new CSVReader(stream, delim, qual)) {
                return cr.ReadAsDataTable(false, ignore_dimension_errors, headers);
            }
        }

        /// <summary>
        /// Convert a CSV file (in string form) into a data table
        /// </summary>
        /// <param name="source_string"></param>
        /// <param name="first_row_are_headers"></param>
        /// <param name="ignore_dimension_errors"></param>
        /// <returns></returns>
        public static DataTable LoadString(string source_string, bool first_row_are_headers, bool ignore_dimension_errors)
        {
            DataTable dt = null;
            byte[] byteArray = Encoding.ASCII.GetBytes(source_string);
            MemoryStream stream = new MemoryStream(byteArray);
            using (CSVReader cr = new CSVReader(new StreamReader(stream))) {
                dt = cr.ReadAsDataTable(first_row_are_headers, ignore_dimension_errors);
            }
            return dt;
        }

        /// <summary>
        /// Write a data table to disk at the designated file name in CSV format
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fn"></param>
#if DOTNET20
        public static void SaveAsCSV(DataTable dt, string filename, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#else
        public static void SaveAsCSV(this DataTable dt, string filename, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#endif
        {
            using (StreamWriter sw = new StreamWriter(filename)) {
                WriteToStream(dt, sw, save_column_names, delim, qual);
            }
        }

        /// <summary>
        /// Send this 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="from_address"></param>
        /// <param name="to_address"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="?"></param>
#if DOTNET20
        public static void SendCsvAttachment(DataTable dt, string from_address, string to_address, string subject, string body, string smtp_host, string attachment_filename)
#else
        public static void SendCsvAttachment(this DataTable dt, string from_address, string to_address, string subject, string body, string smtp_host, string attachment_filename)
#endif
        {
            // Save this CSV to a string
            string csv = WriteToString(dt, true);

            // Prepare the email message and attachment
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.To.Add(to_address);
            message.Subject = subject;
            message.From = new System.Net.Mail.MailAddress(from_address);
            message.Body = body;
            System.Net.Mail.Attachment a = System.Net.Mail.Attachment.CreateAttachmentFromString(csv, "text/csv");
            a.Name = attachment_filename;
            message.Attachments.Add(a);

            //            // Send the email
            //#if (DOTNET20 || DOTNET35)
            //            var smtp = new System.Net.Mail.SmtpClient(smtp_host);
            //            smtp.Send(message);
            //#else
            //            using (System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(smtp_host)) {
            //                smtp.Send(message);
            //            }
            //#endif

            throw new NotImplementedException();

        }

        /// <summary>
        /// Write the data table to a stream in CSV format
        /// </summary>
        /// <param name="dt">The data table to write</param>
        /// <param name="sw">The stream where the CSV text will be written</param>
        /// <param name="save_column_names">True if you wish the first line of the file to have column names</param>
        /// <param name="delim">The delimiter (comma, tab, pipe, etc) to separate fields</param>
        /// <param name="qual">The text qualifier (double-quote) that encapsulates fields that include delimiters</param>
#if DOTNET20
        public static void WriteToStream(DataTable dt, StreamWriter sw, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#else
        public static void WriteToStream(this DataTable dt, StreamWriter sw, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#endif
        {
            using (CSVWriter cw = new CSVWriter(sw, delim, qual)) {
                cw.Write(dt, save_column_names);
            }
        }
        
        /// <summary>
        /// Write a DataTable to a string in CSV format
        /// </summary>
        /// <param name="dt">The datatable to write</param>
        /// <param name="sw">The stream where the CSV text will be written</param>
        /// <param name="save_column_names">True if you wish the first line of the file to have column names</param>
        /// <param name="delim">The delimiter (comma, tab, pipe, etc) to separate fields</param>
        /// <param name="qual">The text qualifier (double-quote) that encapsulates fields that include delimiters</param>
        /// <returns>The CSV string representing the object array.</returns>
#if DOTNET20
        public static string WriteToString(DataTable dt, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#else
        public static string WriteToString(this DataTable dt, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#endif
        {
            using (var ms = new MemoryStream()) {
                var sw = new StreamWriter(ms);
                var cw = new CSVWriter(sw, delim, qual);
                cw.Write(dt, save_column_names);
                sw.Flush();
                ms.Position = 0;
                using (var sr = new StreamReader(ms)) {
                    return sr.ReadToEnd();
                }
            }
        }
#endif
        #endregion

        #region FileStream related functions (not available on dot-net-portable)
#if !PORTABLE
        /// <summary>
        /// Serialize an object array to a stream in CSV format
        /// </summary>
        /// <param name="list">The object array to write</param>
        /// <param name="sw">The stream where the CSV text will be written</param>
        /// <param name="save_column_names">True if you wish the first line of the file to have column names</param>
        /// <param name="delim">The delimiter (comma, tab, pipe, etc) to separate fields</param>
        /// <param name="qual">The text qualifier (double-quote) that encapsulates fields that include delimiters</param>
#if DOTNET20
        public static void WriteToStream<T>(IEnumerable<T> list, StreamWriter sw, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#else
        public static void WriteToStream<T>(this IEnumerable<T> list, StreamWriter sw, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#endif
        {
            using (CSVWriter cw = new CSVWriter(sw, delim, qual)) {
                cw.WriteObjects(list, save_column_names);
            }
        }

        /// <summary>
        /// Serialize an object array to a stream in CSV format
        /// </summary>
        /// <param name="list">The object array to write</param>
        /// <param name="sw">The stream where the CSV text will be written</param>
        /// <param name="save_column_names">True if you wish the first line of the file to have column names</param>
        /// <param name="delim">The delimiter (comma, tab, pipe, etc) to separate fields</param>
        /// <param name="qual">The text qualifier (double-quote) that encapsulates fields that include delimiters</param>
#if DOTNET20
        public static void WriteToStream<T>(IEnumerable<T> list, string filename, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#else
        public static void WriteToStream<T>(this IEnumerable<T> list, string filename, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#endif
        {
            using (StreamWriter sw = new StreamWriter(filename)) {
                WriteToStream<T>(list, sw, save_column_names, delim, qual);
            }
        }

        /// <summary>
        /// Serialize an object array to a string in CSV format
        /// </summary>
        /// <param name="list">The object array to write</param>
        /// <param name="sw">The stream where the CSV text will be written</param>
        /// <param name="save_column_names">True if you wish the first line of the file to have column names</param>
        /// <param name="delim">The delimiter (comma, tab, pipe, etc) to separate fields</param>
        /// <param name="qual">The text qualifier (double-quote) that encapsulates fields that include delimiters</param>
        /// <returns>The CSV string representing the object array.</returns>
#if DOTNET20
        public static string WriteToString<T>(IEnumerable<T> list, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#else
        public static string WriteToString<T>(this IEnumerable<T> list, bool save_column_names, char delim = DEFAULT_DELIMITER, char qual = DEFAULT_QUALIFIER)
#endif
        {
            using (var ms = new MemoryStream()) {
                var sw = new StreamWriter(ms);
                var cw = new CSVWriter(sw, delim, qual);
                cw.WriteObjects(list, save_column_names);
                sw.Flush();
                ms.Position = 0;
                using (var sr = new StreamReader(ms)) {
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Read in a single CSV file as an array of objects
        /// </summary>
        /// <typeparam name="T">The type of objects to deserialize from this CSV.</typeparam>
        /// <param name="stream">The stream to read.</param>
        /// <param name="ignore_dimension_errors">Set to true if you wish to ignore rows that have a different number of columns.</param>
        /// <param name="ignore_bad_columns">Set to true if you wish to ignore column headers that don't match up to object attributes.</param>
        /// <param name="ignore_type_conversion_errors">Set to true if you wish to overlook elements in the CSV array that can't be properly converted.</param>
        /// <param name="delim">The CSV field delimiter character.</param>
        /// <param name="qual">The CSV text qualifier character.</param>
        /// <returns>An array of objects that were retrieved from the CSV file.</returns>
        public static List<T> LoadArray<T>(string filename, bool ignore_dimension_errors = true, bool ignore_bad_columns = true, bool ignore_type_conversion_errors = true, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER) where T : class, new()
        {
            return LoadArray<T>(new StreamReader(filename), ignore_dimension_errors, ignore_bad_columns, ignore_type_conversion_errors, delim, qual);
        }
#endif
        #endregion

        #region Minimal portable functions
#if PORTABLE
        /// <summary>
        /// Convert a CSV file (in string form) into a list of string arrays 
        /// </summary>
        /// <param name="source_string"></param>
        /// <param name="first_row_are_headers"></param>
        /// <param name="ignore_dimension_errors"></param>
        /// <returns></returns>
        public static List<string[]> LoadString(string source_string, bool first_row_are_headers, bool ignore_dimension_errors)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(source_string);
            MemoryStream stream = new MemoryStream(byteArray);
            var results = new List<string[]>();
            using (CSVReader cr = new CSVReader(new StreamReader(stream))) {
                foreach (var line in cr) {
                    results.Add(line);
                }
            }
            return results;
        }
#endif
        #endregion

        #region Output Functions
        /// <summary>
        /// Output a single field value as appropriate
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Output(IEnumerable<object> line, char delimiter = DEFAULT_DELIMITER, char qualifier = DEFAULT_QUALIFIER, bool force_qualifiers = false)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object o in line) {

                // Null strings are just a delimiter
                if (o != null) {
                    string s = o.ToString();
                    if (s.Length > 0) {

                        // Does this string contain any risky characters?  Risky is defined as delim, qual, or newline
#if (DOTNET20 || DOTNET35 || DOTNET40 || DOTNET45 || PORTABLE)
                        if (force_qualifiers || (s.IndexOf(delimiter) >= 0) || (s.IndexOf(qualifier) >= 0) || s.Contains(Environment.NewLine)) {
#else
                        if (force_qualifiers || s.Contains(delimiter.ToString()) || s.Contains(qualifier.ToString()) || s.Contains(Environment.NewLine)) {
#endif
                            sb.Append(qualifier);

                            // Double up any qualifiers that may occur
                            sb.Append(s.Replace(qualifier.ToString(), qualifier.ToString() + qualifier.ToString()));
                            sb.Append(qualifier);
                        } else {
                            sb.Append(s);
                        }
                    }
                }

                // Move to the next cell
                sb.Append(delimiter);
            }

            // Subtract the trailing delimiter so we don't inadvertently add a column
            sb.Length -= 1;
            return sb.ToString();
        }
#endregion

        #region Shortcuts for static read calls
        /// <summary>
        /// Saves an array of objects to a CSV string in memory.
        /// </summary>
        /// <typeparam name="T">The type of objects to serialize from this CSV.</typeparam>
        /// <param name="list">The array of objects to serialize.</param>
        /// <param name="save_column_names">Set to true if you wish the first line of the CSV to contain the field names.</param>
        /// <param name="force_qualifiers">Set to true to force qualifier characters around each field.</param>
        /// <param name="delim">The CSV field delimiter character.</param>
        /// <param name="qual">The CSV text qualifier character.</param>
        /// <returns>The CSV string.</returns>
        public static string SaveArray<T>(IEnumerable<T> list, bool save_column_names = true, bool force_qualifiers = false, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER) where T : class, new()
        {
            using (var ms = new MemoryStream()) {
                var sw = new StreamWriter(ms);
                var cw = new CSVWriter(sw, delim, qual);
                cw.WriteObjects<T>(list, save_column_names, force_qualifiers);
                sw.Flush();
                ms.Position = 0;
                using (var sr = new StreamReader(ms)) {
                    return sr.ReadToEnd();
                }
            }
        }

#if !PORTABLE
        /// <summary>
        /// Read in a single CSV file as an array of objects
        /// </summary>
        /// <typeparam name="T">The type of objects to deserialize from this CSV.</typeparam>
        /// <param name="stream">The stream to read.</param>
        /// <param name="ignore_dimension_errors">Set to true if you wish to ignore rows that have a different number of columns.</param>
        /// <param name="ignore_bad_columns">Set to true if you wish to ignore column headers that don't match up to object attributes.</param>
        /// <param name="ignore_type_conversion_errors">Set to true if you wish to overlook elements in the CSV array that can't be properly converted.</param>
        /// <param name="delim">The CSV field delimiter character.</param>
        /// <param name="qual">The CSV text qualifier character.</param>
        /// <returns>An array of objects that were retrieved from the CSV file.</returns>
        public static List<T> LoadArray<T>(StreamReader stream, bool ignore_dimension_errors = true, bool ignore_bad_columns = true, bool ignore_type_conversion_errors = true, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER) where T : class, new()
        {
            using (CSVReader cr = new CSVReader(stream, delim, qual)) {
                return cr.Deserialize<T>(ignore_dimension_errors, ignore_bad_columns, ignore_type_conversion_errors);
            }
        }
#endif
        #endregion

        #region Chopping a CSV file into chunks
#if !PORTABLE
        /// <summary>
        /// Take a CSV file and chop it into multiple chunks of a specified maximum size.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="out_folder"></param>
        /// <param name="first_row_are_headers"></param>
        /// <param name="max_lines_per_file"></param>
        /// <returns>Number of files chopped</returns>
        public static int ChopFile(string filename, string out_folder, bool first_row_are_headers, int max_lines_per_file, char delim = CSV.DEFAULT_DELIMITER, char qual = CSV.DEFAULT_QUALIFIER)
        {
            int file_id = 1;
            int line_count = 0;
            string file_prefix = Path.GetFileNameWithoutExtension(filename);
            string ext = Path.GetExtension(filename);
            CSVWriter cw = null;

            // Read in lines from the file
            using (CSVReader cr = new CSVReader(filename, delim, qual, first_row_are_headers)) {

                // Okay, let's do the real work
                foreach (string[] line in cr.Lines()) {

                    // Do we need to create a file for writing?
                    if (cw == null) {
                        string fn = Path.Combine(out_folder, file_prefix + file_id.ToString() + ext);
                        cw = new CSVWriter(fn, delim, qual);
                        if (first_row_are_headers) {
                            cw.WriteLine(cr.Headers);
                        }
                    }

                    // Write one line
                    cw.WriteLine(line);

                    // Count lines - close the file if done
                    line_count++;
                    if (line_count >= max_lines_per_file) {
                        cw.Dispose();
                        cw = null;
                        file_id++;
                        line_count = 0;
                    }
                }
            }

            // Ensore the final CSVWriter is closed properly
            if (cw != null) {
                cw.Dispose();
                cw = null;
            }
            return file_id;
        }
#endif
        #endregion
    }
}
