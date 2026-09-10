using System;
using System.Collections.Generic;

namespace EJLive.Client.Service
{
    public class AgentConfiguration
    {
        public string AgentId { get; set; } = "default";
        public string ConfigSource { get; set; } = "built-in";
    }
}