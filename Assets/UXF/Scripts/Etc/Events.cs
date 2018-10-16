using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace UXF
{

    /// <summary>
    /// Event containing a Session as a parameter 
    /// </summary>
    [Serializable]
    public class SessionEvent : UnityEvent<Session>
    {

    }

    /// <summary>
    /// Event containing a Trial as a parameter
    /// </summary>
    [Serializable]
    public class TrialEvent : UnityEvent<Trial>
    {

    }

    /// <summary>
    /// Event containing a WriteFileInfo object as a parameter
    /// </summary>
    [Serializable]
    public class WriteFileEvent : UnityEvent<WriteFileInfo>
    {

    }


    public struct WriteFileInfo
    {
        public WriteFileType fileType;
        public string basePath;
        public string[] paths;
        public string RelativePath { get { return Extensions.CombinePaths("", paths); }}
        public string FullPath { get { return Extensions.CombinePaths(basePath, paths); } }
        public string FileName { get { return paths[paths.GetLength(0) - 1]; } }
        public WriteFileInfo(WriteFileType fileType, string basePath, params string[] paths)
        {
            this.fileType = fileType;
            this.basePath = basePath;
            this.paths = paths;
        }

    }

    public enum WriteFileType
    {
        Test, Trials, Tracker, Dictionary, ParticipantList, Log
    }

}