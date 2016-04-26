using System;
using Microsoft.Azure.Mobile.Server;

namespace Monocle.Service.DataObjects
{
    public class Image : EntityData
    {
        public string UserId { get; set; }

        public string Words { get; set; }

        public bool IsProcessed { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}