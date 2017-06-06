using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonObjects;
using Modulus.TradeScript;

namespace DataServer
{
    public class TradeScript
    {
        public const string TradeScriptKey = "MNF746DFN8AS721KDK365";
        private static Validator scriptValidator = new Validator { License = TradeScriptKey };

        public static string Validate(string script)
        {
            return scriptValidator.Validate(script);
        }

        public static string ValidateBuySell(string buyScript, string sellScript)
        {
            var buyScriptValidateString = Validate(buyScript);
            var sellScriptValidateString = Validate(sellScript);
            var errorString = string.Empty;

            if (buyScriptValidateString != string.Empty)
                errorString += "Buy Script " + buyScriptValidateString;
            if (sellScriptValidateString != string.Empty)
            {
                if (errorString != string.Empty)
                    errorString += "; ";
                errorString += "Sell Script " + sellScriptValidateString;
            }
            return errorString;
        }

        public static IList<BacktestInformationItem> ExtractBacktestInformation(string backtestOutput)
        {
            int found = backtestOutput.IndexOf("Trade Log:");
            if (found <= 0) return null;

            var result = new List<BacktestInformationItem>();
            string[] report = backtestOutput.Substring(0, found - 1).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var dateInterval = report[0]
                .Replace("Back tested from ", "")
                .Split(new string[] { " to " }, StringSplitOptions.None);
            result.Add(new BacktestInformationItem("Start date", dateInterval[0]));
            result.Add(new BacktestInformationItem("End date", dateInterval[1]));

            for (var i = 1; i < report.Length; i++)
            {
                var entry = report[i].Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                if (entry.Length == 2)
                    result.Add(new BacktestInformationItem(entry[0].Replace("\r", ""), entry[1].Replace("\r", "")));
            }

            return result;
        }

        public static IList<BacktestTradeItem> ExtractBacktestTrades(string backtestOutput)
        {
            int found = backtestOutput.IndexOf("Trade Log:");
            if (found <= 0) return null;

            var result = new List<BacktestTradeItem>();
            string[] tradeLog = backtestOutput.Substring(found + "trade log:".Length + 1).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in tradeLog)
            {
                var entry = line.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (entry.Length == 3)
                    result.Add(new BacktestTradeItem(entry[0], entry[1], Double.Parse(entry[2])));
            }

            return result;
        }

        public static string Backtest(string buyScript, string sellScript, List<Bar> bars)
        {
            Backtest backtest = new Backtest();
            backtest.License = TradeScriptKey;
            backtest.OnScriptError += backtest_OnError;
            AppendData(backtest, bars);
            return backtest.RunBacktest(buyScript, sellScript, string.Empty, string.Empty, 0.00001f);
        }

        private static void backtest_OnError(object sender, ScriptErrorEventArgs e)
        {
        }

        public static void AppendData(IDataAppendAble dataAble, IList<Bar> bars)
        {
            foreach (var bar in bars)
            {
                dataAble.AppendRecord(bar.Date, bar.Open, bar.High, bar.Low, bar.Close, (long)bar.Volume);
            }
        }
    }

    public class TradescriptAlert
    {
        private Modulus.TradeScript.Alert _Alert;
        public bool _FireAlert = false;
        public bool _PrevFireAlert = false;

        public AlertCalculateType CalculateType;

        public TradescriptAlert(string name, SymbolItem symbol, string script, IList<Bar> history)
        {
            _Alert = new Modulus.TradeScript.Alert();
            _Alert.License = TradeScript.TradeScriptKey;
            _Alert.Symbol = symbol.Symbol;
            _Alert.AlertName = name;
            _Alert.AlertScript = script;
            TradeScript.AppendData(_Alert, history);
            _Alert.OnScriptError += alert_OnError;
            _Alert.OnAlert += alert_OnAlert;
        }

        private void alert_OnError(object sender, ScriptErrorEventArgs e)
        {

        }

        private void alert_OnAlert(object sender, AlertEventArgs e)
        {
            _FireAlert = true;
        }

        public bool AppendRecord(Bar bar, bool isNewBar)
        {
            _FireAlert = false;
            if (isNewBar)
            {
                int i = _Alert.RecordCount - 1;
                _Alert.AppendRecord(bar.Date, bar.Open, bar.High, bar.Low, bar.Close, (long)bar.Volume); 
            }
            else
            {
                _Alert.EditRecord(bar.Date, bar.Open, bar.High, bar.Low, bar.Close, (long)bar.Volume);
            }

            bool retVa = false;

            if (CalculateType == AlertCalculateType.OnTick)
            {
                retVa = _FireAlert;
            }
            else if (CalculateType == AlertCalculateType.OnBarClose)
            {
                if (isNewBar) retVa = _PrevFireAlert;
            }
            _PrevFireAlert = _FireAlert;
            return retVa;
        }

    }
}
