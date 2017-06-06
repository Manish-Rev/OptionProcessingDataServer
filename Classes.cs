using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using CommonObjects;

namespace DataServer
{
    /// <summary>
    /// Class to display connections and datafeeds items in UI view
    /// </summary>
    public class DisplayItem
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public string State { get; set; }
        public string Error { get; set; }

        public DisplayItem()
        {
            Id = Guid.NewGuid().ToString();
            State = "Stopped";
            Enabled = true;
            Error = string.Empty;
        }

        /// <summary>
        /// saves parameters to file
        /// </summary>
        public void Save()
        {
            if (Parameters != null)
            {
                SavedSettings settings = new SavedSettings(Parameters, Enabled);
                string file = Path.Combine(Path.GetDirectoryName(FileName),
                                           Path.GetFileNameWithoutExtension(FileName) + ".set");
                settings.Save(file);
            }
        }

        /// <summary>
        /// loads parameters from file
        /// </summary>
        /// <param name="aDefaultParams">stores loaded parameters</param>
        public void Load(Dictionary<string, object> aDefaultParams)
        {
            string file = Path.Combine(Path.GetDirectoryName(FileName),
                                       Path.GetFileNameWithoutExtension(FileName) + ".set");
            SavedSettings settings = SavedSettings.Load(file);

            if (settings != null)
            {
                Enabled = settings.Enabled;
                Parameters = settings.Parameters;
            }
            else
                Parameters = aDefaultParams;
        }
    }

    public class DataFeedItem : DisplayItem
    {
        public IDataFeed DataFeed;
    }

    public class ConnServiceHostItem : DisplayItem
    {
        public IDataServerServiceHost Host;
    }

    /// <summary>
    /// serializable class to store/load objects in file
    /// </summary>
    [Serializable]
    public class SavedSettings
    {
        public bool Enabled = false;
        public Dictionary<string, object> Parameters;

        public SavedSettings(Dictionary<string, object> aParameters, bool aEnabled)
        {
            Parameters = aParameters;
            Enabled = aEnabled;
        }

        public static SavedSettings Load(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();

                        return (SavedSettings) formatter.Deserialize(fs);
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.Assert(false);
            }
            return null;
        }

        public void Save(string fileName)
        {
            
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(fs, this);
                }
            }
            catch
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }
    }
}
