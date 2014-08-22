﻿namespace Adaptive.ReactiveTrader.Shared
{
    public static class ServiceConstants
    {
        public static class Server
        {
            public const string UsernameHeader = "User";

            // pricing
            public const string PricingHub = "PricingHub";
            public const string SubscribePriceStream = "SubscribePriceStream";
            public const string UnsubscribePriceStream = "UnsubscribePriceStream";

            // blotter
            public const string BlotterHub = "BlotterHub";
            public const string SubscribeTrades = "SubscribeTrades";
            public const string UnsubscribeTrades = "UnsubscribeTrades";
            
            // reference data
            public const string ReferenceDataHub = "ReferenceDataHub";
            public const string GetCurrencyPairs = "GetCurrencyPairs";

            // executution
            public const string ExecutionHub = "ExecutionHub";
            public const string Execute = "Execute";

            // control
            public const string ControlHub = "ControlHub";
            public const string SetPriceFeedThroughput = "SetPriceFeedThroughput";
            public const string GetCurrencyPairStates = "GetCurrencyPairStates";
            public const string SetCurrencyPairState = "SetCurrencyPairState";
        }

        public static class Client
        {
            public const string OnNewPrice = "OnNewPrice";
            public const string OnNewTrade = "OnNewTrade";
            public const string OnCurrencyPairUpdate = "OnCurrencyPairUpdate"; 
        }
    }
}