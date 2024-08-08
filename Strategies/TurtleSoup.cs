using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using System.ComponentModel.DataAnnotations;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class TurtleSoup : Strategy
    {   private DateTime entryTime;
        private Swing SwingIndicator;
        private DynamicSRLines sRLines;
        //private int defaultQuantity = 5;
        //private double SupportDifferencePercent = 10;
        //private double stopLossPerncent = 0.25;
        private double PreviousSwingLow = 0;
        private double PreviousSwingHigh = 0;
        private double takeProfit = 0;
        private double stopLossLong = 0; 
        private double stopLossShort = 0;
        private int previousHourHigh = 0;
        private int previousHourLow = 0;
        private int countCandle = 0;
        private bool stopLossIsExist = false;
        private bool searchLongEnter = false;
        private bool searchShortEnter = false;




        private bool SupportPercent()
        {
            var SwingLow = SwingIndicator.SwingLow[0];
            var SwingHigh = SwingIndicator.SwingHigh[0];

            var onePercent = (SwingHigh - SwingLow) / 100;
            if (Math.Abs(SwingLow - Close[0]) < (SwingHigh - Close[0]))
            {
                //swingValue - swingLow
                return (SwingLow - Low[0]) / onePercent > SupportDifferencePercent;
            } else
            {
                //swingValue - swingHigh
                return (High[0] - SwingHigh) / onePercent > SupportDifferencePercent;
            }
        }

        private bool ReverseSupportPercent()
        {
            var SwingLow = SwingIndicator.SwingLow[0];
            var SwingHigh = SwingIndicator.SwingHigh[0];

            var onePercent = (SwingHigh - SwingLow) / 100;
            if ((SwingLow - Close[0]) < (SwingHigh - Close[0]))
            {
                //swingValue - swingLow
                return (Close[0] - SwingLow) / onePercent > ReverseBreakDownPercent;
            }
            else
            {
                //swingValue - swingHigh
                return (SwingHigh - Close[0]) / onePercent > ReverseBreakDownPercent;
            }
        }
        // private double StopLossLong()
        // {   
        //     if ( SwingIndicator.SwingLow[0] < Close[0] && SwingHigh > Close[0])
        //         Low[0];
        // }
        // private double StopLossShort()
        // {
        //     if ( SwingIndicator.SwingLow[0] > Close[0] && SwingHigh < Close[0])
        //          High[0];
        // }
 

        private void ActivatePositionEntry()
        {
            if (Position.MarketPosition == MarketPosition.Flat)
            {
                if (SwingIndicator.SwingLow[0] > Low[0] && SupportPercent())
                {
                    searchLongEnter = true;
                }
                if (High[0] > SwingIndicator.SwingHigh[0] && SupportPercent())
                {
                    searchShortEnter = true;
                }
            }
        }
        

            private void SearchEntry()
        {
            if (searchLongEnter && ReverseSupportPercent())
            {
                EnterLong(DefaultQuantity, "Long");
                searchShortEnter = false;
            }
            if (searchShortEnter && ReverseSupportPercent())
            {
                EnterShort(DefaultQuantity, "Short");
                searchShortEnter = false;
            }
        }

        private void CheckEntryState()
        {
            if (searchLongEnter || searchShortEnter)
            {
                if (PreviousSwingHigh != SwingIndicator.SwingHigh[0] || PreviousSwingLow != SwingIndicator.SwingLow[0])
                {
                    searchLongEnter = false;
                    searchShortEnter = false;
                }
            }

        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "TurtleSoupStrategy";
                TraceOrders = true;
                Calculate = Calculate.OnBarClose;
            }
            if (State == State.DataLoaded)
            {
                SwingIndicator = Swing(Close, SwingStrength);
                // sRLines = DynamicSRLines(5, 300, 10, 2000, 3, false, Brushes.Blue, Brushes.Red);
                SwingIndicator.Plots[0].Brush = Brushes.DarkCyan;
                SwingIndicator.Plots[1].Brush = Brushes.Goldenrod;
                AddChartIndicator(SwingIndicator);
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0)
                return;
            if (CurrentBars[0] < 1)
                return;

            Print("PreviousSwingLow" + PreviousSwingLow);
            Print("SwingIndicator.SwingLow[0]" + SwingIndicator.SwingLow[0]);

            Print("PreviousSwingHigh" + PreviousSwingHigh);
            Print("SwingIndicator.SwingHigh[0]" + SwingIndicator.SwingHigh[0]);


            Print("searchShortEnter"+ searchShortEnter);
            Print("searchLongEnter" + searchLongEnter);

           
            countCandle ++;
                ActivatePositionEntry();
                CheckEntryState();
                SearchEntry();

            // if (Position.MarketPosition == MarketPosition.Short  && Close[0] < takeProfit)
            // {
            //     ExitShort(DefaultQuantity, "Exit", "Short");
            // }
            // if (Position.MarketPosition == MarketPosition.Long && Close[0] > takeProfit)
            // {
            //     ExitLong(DefaultQuantity, "Exit", "Long");
            // }

             // Track the high and low of the previous 1-hour candle
            if (countCandle % 60 == 0 )
            {
                previousHourHigh = High[0];
                previousHourLow = Low[0];
            }

            // Place trades based on the conditions
            if (Close[0] > previousHourHigh)
            {
                EnterLong(DefaultQuantity, "Long");
            }
            else if (Close[0] < previousHourLow)
            {
                EnterShort(DefaultQuantity, "Short");
            }
            else if (High[0] > previousHourHigh && Close[0] <= previousHourHigh)
            {
                EnterShort(DefaultQuantity, "Short");
            }
            else if (Low[0] < previousHourLow && Close[0] >= previousHourLow)
            {
                EnterLong(DefaultQuantity, "Long");
            }

            if (Position.MarketPosition == MarketPosition.Short && Close[0] < takeProfit)
            {
                ExitShort(DefaultQuantity, "Exit", "Short");
            }
            if (Position.MarketPosition == MarketPosition.Long && Close[0] > takeProfit)
            {
                ExitLong(DefaultQuantity, "Exit", "Long");
            }
            if (Position.MarketPosition != MarketPosition.Flat && !stopLossIsExist)
            {
                    SetStopLoss();
                
            }
            }

            PreviousSwingLow = SwingIndicator.SwingLow[0];
            PreviousSwingHigh = SwingIndicator.SwingHigh[0];
        }

        private void SetStopLoss()
        {       double tick = GetTickSize();
            if (Position.MarketPosition == MarketPosition.Long  )
            {    
                 if ( SwingIndicator.SwingLow[0] < Close[0] && countCandle % 60 )
                    stopLossLong = Low[0];
                ExitLongStopMarket(0, true, DefaultQuantity, stopLossLong, "ExitByStopLoss", "Long");
                // takeProfit = Position.AveragePrice + (Position.AveragePrice - stopPrice) * takeProfitMultiplier;
                takeProfit = Open[0] + tick*takeProfitMultiplier ;
                Print("LongSrop" + StopLossLong());
            }

            if (Position.MarketPosition == MarketPosition.Short )
            {
               if ( SwingIndicator.SwingLow[0] > Close[0]  && countCandle % 60 )
                    stopLossShort = High[0];
                ExitShortStopMarket(0, true, DefaultQuantity, stopPrice, "ExitByStopLoss", "Short");
                // takeProfit = Position.AveragePrice - (stopPrice - Position.AveragePrice) * takeProfitMultiplier;
                takeProfit = Open[0] - tick*takeProfitMultiplier ;
                Print("ShortStop" + StopLossShort());
                Print("defaultQuantity"+ DefaultQuantity);
            }
            var isSetStopLoss = true ; 
        }
        private void GetTickSize()
            {
                return  TickSize;
            }
        // protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string comment)
        // {

        //     if (order.OrderState == OrderState.Filled && order.Name != "ExitByStopLoss")
        //     {
        //         Calculate = Calculate.OnEachTick;
        //         stopLossIsExist = false;

        //         if (order.IsLong)
        //         {
        //         }
        //     }
        //     if (order.Name == "ExitByStopLoss")
        //     {
        //         Calculate = Calculate.OnBarClose;
        //         stopLossIsExist = true;
        //     }
        // }

        #region Properties


        [NinjaScriptProperty]
        [Display(Name = "Support breakdownd percent", Order = 1, GroupName = "Parameters")]
        public double SupportDifferencePercent { get; set; } = 10;

        [NinjaScriptProperty]
        [Display(Name = "Reverse breakdownd percent", Order = 1, GroupName = "Parameters")]
        public double ReverseBreakDownPercent { get; set; } = 10;

        [NinjaScriptProperty]
        [Display(Name = "Order StopLoss multiplier ", Order = 2, GroupName = "Parameters")]
        public double stopLossMultiplier { get; set; } = 1;

        [NinjaScriptProperty]
        [Display(Name = "Order take profit multiplier ", Order = 3, GroupName = "Parameters")]
        public double takeProfitMultiplier { get; set; } = 2;

        [NinjaScriptProperty]
        [Display(Name = "Swing Strength", Order = 4, GroupName = "Parameters")]
        public int SwingStrength { get; set; } = 10;
        #endregion

    }
}
