﻿using System;

namespace XDM.Core.Downloader
{
    public class ProbeResult
    {
        public bool Resumable { get; set; }
        public long? ResourceSize { get; set; }
        public Uri? FinalUri { get; set; }
        public string? AttachmentName { get; set; }

        public string? Referer { get; set; }

        public string? ContentType { get; set; }
        public DateTime LastModified { get; set; }
    }
}
