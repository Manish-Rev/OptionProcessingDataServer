
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonObjects;


namespace DataServer
{
    using System.Collections;

    public class AlertSubscription
    {
        private const int FiredAlertsHistoryMaxLenght = 50;

        public string Id { get; set; }
        public string AlertName { get; set; }
        public SymbolItem Symbol { get; set; }
        public Periodicity Periodicity { get; set; }
        public int Interval { get; set; }
        public DateTime NewBarDate { get; set; }
        public Bar LastBar { get; set; }

        public string UserSessionId;

        public string Login;
        public string Name { get; set; }
        public string Script { get; set; }

        public List<Alert> FiredAlertsHistory;
        private TradescriptAlert Alert { get; set; }

        public AlertCalculateType CalculationType { get; set; }
        
        public AlertSubscription()
        {
            Id = string.Empty;
            Symbol = new SymbolItem();
            FiredAlertsHistory = new List<Alert>(FiredAlertsHistoryMaxLenght);
        }

        public void AddFiredAlertToHistory(Alert aAlert)
        {
            if (this.FiredAlertsHistory.Count == FiredAlertsHistoryMaxLenght)
            {
                this.FiredAlertsHistory.RemoveAt(0);
            }
            this.FiredAlertsHistory.Add(aAlert);
        }
        
        public int NewBarSeconds
        {
            get
            {
                int periodSecond = 0;
                switch (Periodicity)
                {
                    case Periodicity.Second:
                        periodSecond = 1;
                        break;
                    case Periodicity.Minute:
                        periodSecond = 60;
                        break;
                    case Periodicity.Day:
                        periodSecond = 60 * 60 * 24;
                        break;
                    default:
                        break;
                }
                return periodSecond * Interval;
            }
        }

        public bool IsNewBar(DateTime barDate)
        {
            return barDate >= NewBarDate;
        }

        public bool AppendTick(Tick aTick)
        {
            bool newBar = IsNewBar(aTick.Date);
            if (newBar)
            {
                NewBarDate = aTick.Date.AddSeconds(NewBarSeconds);
                LastBar = new Bar
                {
                    Date = aTick.Date,
                    Open = aTick.Price,
                    High = aTick.Price,
                    Low = aTick.Price,
                    Close = aTick.Price,
                    Volume = aTick.Volume
                };
            }
            else
            {
                LastBar.Close = aTick.Price;
                LastBar.Volume = aTick.Volume;
                if (LastBar.High < LastBar.Close)
                    LastBar.High = LastBar.Close;
                if (LastBar.Low > LastBar.Close)
                    LastBar.Low = LastBar.Close;
            }
            return AppendRecord(LastBar, newBar);
        }

        public void InitAlert(IList<Bar> bars)
        {
            Alert = new TradescriptAlert(Name, Symbol, Script, bars);
            Alert.CalculateType = CalculationType;
            if (bars.Count > 0)
            {
                Bar lBar = bars.Last();
                LastBar = new Bar
                {
                     Date = lBar.Date,
                     Open = lBar.Open,
                     High = lBar.High,
                     Low = lBar.Low,
                     Close = lBar.Close,
                     Volume = lBar.Volume
                };
                NewBarDate = LastBar.Date.AddSeconds(NewBarSeconds);
            }
        }

        public bool AppendRecord(Bar bar, bool newBar)
        {
            return Alert.AppendRecord(bar, newBar);
        }
        
    }

    /// <summary>
    /// list of subscribers for Level1 quotes
    /// </summary>
    public class Level1Subscribers
    {
        /// <summary>
        /// latest tick
        /// </summary>
        public Tick Tick { get; set; }
        /// <summary>
        /// list of subscribers identified by unique ID, generally session ID
        /// </summary>
        public List<string> Subscribers { get; set; }

        public List<AlertSubscription> AlertSubscribers { get; set; }

    }

    /// <summary>
    /// list of subscribers for Level1 quotes
    /// </summary>
    public class Level2Subscribers
    {
        /// <summary>
        /// list of subscribers identified by unique ID, generally session ID
        /// </summary>
        public List<string> Subscribers { get; set; }
    }
}
