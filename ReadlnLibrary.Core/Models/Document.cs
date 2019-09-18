﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ReadlnLibrary.Core.Models
{
    public class RdlnDocument
    {
        public string DocumentId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Path { get; set; }
        public string Token { get; set; }
    }
}