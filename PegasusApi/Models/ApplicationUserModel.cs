﻿using System.Collections.Generic;

namespace PegasusApi.Models
{
    public class ApplicationUserModel
    {
        public string Id { get; set; }
        public string Email  { get; set; }
        public Dictionary<string, string> Roles { get; set; }
    }
}
