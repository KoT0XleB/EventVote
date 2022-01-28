using Qurre.API.Addons;
using System.Collections.Generic;
using System.ComponentModel;

namespace EventVote
{
    public class Config : IConfig
    {
        [Description("Plugin Name")]
        public string Name { get; set; } = "EventVote";

        [Description("Enable the plugin?")]
        public bool IsEnable { get; set; } = true;
        [Description("Time vote in seconds. Default: 60?")]
        public float Time { get; set; } = 60;
        [Description("Url webhook: https://discord.com/api/webhooks/webhook.id/webhook.token")]
        public string Token { get; set; } = string.Empty;
    }
}
