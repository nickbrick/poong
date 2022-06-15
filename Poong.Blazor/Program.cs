using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Poong.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Poong.Blazor
{
    public class Program
    {
        internal static Poong.Engine.Game Game;
        internal static readonly int PixelsPerGameUnit = 200;
        public static void Main(string[] args)
        {
            Configuration poongConfig = new Configuration();
            string jsonConfig;
            try
            {
                jsonConfig = System.Environment.GetEnvironmentVariable("POONG_CONFIG");
                poongConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<Poong.Engine.Configuration>(jsonConfig);
            }
            catch
            {
                jsonConfig = File.ReadAllText("poongConfig.json");
                poongConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<Poong.Engine.Configuration>(jsonConfig);
            }
                Game = new Game(poongConfig);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
