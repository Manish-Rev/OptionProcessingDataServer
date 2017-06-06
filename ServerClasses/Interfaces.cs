using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonObjects;

namespace DataServer
{
    /// <summary>
    /// common interface for all connection service hosts
    /// </summary>
    public interface IDataServerServiceHost
    {
        /// <summary>
        /// starts service
        /// </summary>
        /// <param name="aParameters">dictionary of parameters identified by a key name</param>
        void Start(Dictionary<string, object> aParameters);
        /// <summary>
        /// stops service
        /// </summary>
        void Stop();
        /// <summary>
        /// get dictionary of default parameters
        /// </summary>
        Dictionary<string, object> DefaultSettings { get; }
        /// <summary>
        /// name to identify service
        /// </summary>
        string Name { get; }
        /// <summary>
        /// returns object that implements ILogonControl interface. Generally, the object is UI control
        /// </summary>
        ILogonControl LogonControl { get; }
    }

    public interface IHistory_OneMinBars
    {
        void Start(Dictionary<string, object> aParams);
        void AddQuote(DateTime aTimestamp, SymbolItem aSymbol, double aOpen, double aHigh, double aLow, double aClose, long aVolume);
        //List<Bar> GetHistory(SymbolItem aSymbol, DateTime aFrom, DateTime aTo, int aMaxRecords);
        void Stop();
    }
}
