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
        [Description("Время голосования. Default: 60?")]
        public float Time { get; set; } = 60;
        [Description("Список музыки, которая будет случайно исполняться после начала голосования.")]
        public List<string> ListMusic { get; set; } = new List<string>()
        {
            "FireSale.f32le"
        };
        [Description("Color for webhook")]
        public string Color { get; set; } = "65280";
        [Description("Url image for webhook")]
        public string Image { get; set; } = string.Empty;
        [Description("Url webhook (not worked): https://discord.com/api/webhooks/webhook.id/webhook.token")]
        public string Token { get; set; } = string.Empty;
    }
}
