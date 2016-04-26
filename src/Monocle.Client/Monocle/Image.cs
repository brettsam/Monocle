using System;

namespace Monocle
{
    public class Image
    {
        public string Id { get; set; }
        public bool IsProcessed { get; set; }
        public string Words { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
