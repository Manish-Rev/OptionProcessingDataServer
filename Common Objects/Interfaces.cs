using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace CommonObjects
{
    /// <summary>
    /// Common interface for all data feeders
    /// </summary>
    public interface IDataFeed
    {
        /// <summary>
        /// feeder name, must be unique
        /// </summary>
        string Name { get; }
        /// <summary>
        /// list of supported exchanges
        /// </summary>
        List<ExchangeInfo> Markets { get; }
        /// <summary>
        /// default settings for data feeder
        /// </summary>
        Dictionary<string, object> DefaultSettings { get; }
        /// <summary>
        /// UI control to set/get data feeder parameters
        /// </summary>
        ILogonControl LogonControl { get; }
        /// <summary>
        /// start feeder
        /// </summary>
        /// <param name="aParams">data feeder parameters</param>
        void Start(Dictionary<string, object> aParams, string aPath);
        /// <summary>
        /// stop feeder
        /// </summary>
        void Stop();
        /// <summary>
        /// subscribe a symbol
        /// </summary>
        /// <param name="symbolItem">symbol info</param>
        void Subscribe(SymbolItem symbolItem);
        /// <summary>
        /// unsubscribe symbol
        /// </summary>
        /// <param name="symbolItem">symbol info</param>
        void UnSubscribe(SymbolItem symbolItem);
        /// <summary>
        /// subscribe symbol level2 data
        /// </summary>
        /// <param name="symbolItem">symbol info</param>
        void SubscribeLevel2(SymbolItem symbolItem);
        /// <summary>
        /// unsubscribe symbol level2 data
        /// </summary>
        /// <param name="symbolItem">symbol info</param>
        void UnsubscribeLevel2(SymbolItem symbolItem);
        /// <summary>
        /// subscribe symbol Times and Sales data
        /// </summary>
        /// <param name="symbolItem">symbol info</param>
        void GetHistory(GetHistoryCtx aCtx, HistoryAnswerHandler callback);
        /// <summary>
        /// time zone info
        /// </summary>
        TimeZoneInfo TimeZoneInfo { get; }
        /// <summary>
        /// builds name supported by feeder
        /// </summary>
        string BuildSymbolName(string aStandard, Instrument aType);
        /// <summary>
        /// parses symbol's name in feeder format
        /// </summary>
        /// <param name="aSymbol">symbol name in feeder format</param>
        /// <param name="aStandardName">parsed standard name</param>
        /// <param name="aType">parsed instrument type</param>
        /// <returns>true, if symbol name parsed correct</returns>
        bool TryParse(string aSymbol, out string aStandardName, out Instrument aType);

        event NewQuoteHandler NewQuote;
        event NewLevel2DataHandler NewLevel2;
    }

    /// <summary>
    /// Interface for UI control to manage data feeder parameters
    /// </summary>
    public interface ILogonControl
    {
        /// <summary>
        /// get/set data feeder parameters
        /// </summary>
        Dictionary<string, object> Settings { get; set; }
        /// <summary>
        /// validate data feeder parameters
        /// </summary>
        void ValidateSettings();
    }
}
