using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorwegianCrawlerV2
{
    class Flight
    {
        public string Date { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalTime { get; set; }
        public string Duration { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public string Price { get; set; }
        public string Tax { get; set; }
    }
}
