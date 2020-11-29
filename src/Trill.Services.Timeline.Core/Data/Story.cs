using System;
using System.Collections.Generic;

namespace Trill.Services.Timeline.Core.Data
{
    public class Story
    {
        public long Id { get; set; }
        public Author Author { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public Visibility Visibility { get; set; }
        public int TotalRate { get; set; }
    }
}