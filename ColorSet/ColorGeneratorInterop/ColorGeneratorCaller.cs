using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace ColorSet.ColorGeneratorInterop
{
    internal sealed class ColorGeneratorCaller
    {
        private readonly Process _p;

        public ColorGeneratorCaller()
        {
            _p = new Process
            {
                StartInfo =
                {
                    UseShellExecute        = false,
                    RedirectStandardInput  = true,
                    RedirectStandardOutput = true,
                    FileName               = "node.exe",
                    Arguments              = "./ColorGenerator/index.js"
                }
            };
            _p.Start();
        }

        public List<Color> Call(ColorRangeSpecs s)
        {
            _p.StandardInput.WriteLine(
                $"{s.Steps} {s.HueStart} {s.HueEnd} {s.HueCurve} {s.SatStart} {s.SatEnd} {s.SatCurve} {s.SatRate} {s.LumStart} {s.LumEnd} {s.LumCurve} {s.Modifier}");

            string json = _p.StandardOutput.ReadLine();

            List<Color> range = JsonConvert.DeserializeObject<List<Color>>(
                json,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}
            );

            return range;
        }
    }
}