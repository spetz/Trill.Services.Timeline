using System;

namespace Trill.Services.Timeline.Core.Data
{
    public class Visibility
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public bool Highlighted { get; set; }
    }
}