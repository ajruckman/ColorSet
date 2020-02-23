using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColorSet.ColorGeneratorInterop;
using Newtonsoft.Json;

namespace ColorSet
{
    public static class Program
    {
        private static void Main()
        {
            ColorGeneratorCaller colorGeneratorCaller = new ColorGeneratorCaller();

            foreach (string directory in Directory.GetDirectories("./ThemeSpecs/").Select(Path.GetFileName))
            {
                List<string> result = new List<string>();

                Console.WriteLine(directory);
                Directory.CreateDirectory($"./Themes/{directory}");

                foreach (string file in Directory.GetFiles($"./ThemeSpecs/{directory}/"))
                {
                    string          profileName = Path.GetFileNameWithoutExtension(file);
                    string          profile     = File.ReadAllText(file);
                    ColorRangeSpecs s           = JsonConvert.DeserializeObject<ColorRangeSpecs>(profile);

                    string colorBoxURL =
                        $"https://www.colorbox.io/#steps={s.Steps}"                                               +
                        $"#hue_start={s.HueStart}#hue_end={s.HueEnd}#hue_curve={s.HueCurve}"                      +
                        $"#sat_start={s.SatStart}#sat_end={s.SatEnd}#sat_curve={s.SatCurve}#sat_rate={s.SatRate}" +
                        $"#lum_start={s.LumStart}#lum_end={s.LumEnd}#lum_curve={s.LumCurve}";

                    Console.WriteLine($"\t{file} -> {colorBoxURL}");

                    List<Color> profileColors = colorGeneratorCaller.Call(s);

                    List<Color> profileColorsStep1 = new List<Color>();
                    List<Color> profileColorsStep2 = new List<Color>();

                    if (s.HoverStepSize != 0)
                    {
                        s.LumStart         += s.HoverStepSize;
                        s.LumEnd           += s.HoverStepSize;
                        profileColorsStep1 =  colorGeneratorCaller.Call(s);
                        s.LumStart         -= s.HoverStepSize;
                        s.LumEnd           -= s.HoverStepSize;
                    }

                    if (s.FocusStepSize != 0)
                    {
                        s.LumStart         += s.FocusStepSize;
                        s.LumEnd           += s.FocusStepSize;
                        profileColorsStep2 =  colorGeneratorCaller.Call(s);
                        s.LumStart         -= s.FocusStepSize;
                        s.LumEnd           -= s.FocusStepSize;
                    }

                    for (var i = 0; i < profileColors.Count; i++)
                    {
                        Color  color = profileColors[i];
                        string line  = @$"$ColorSet_ThemeColor_{profileName}_{i * 10}: {color.Hex};";
                        result.Add(line);

                        if (s.HoverStepSize != 0)
                        {
                            Color  colorStep1 = profileColorsStep1[i];
                            string lineStep1  = @$"$ColorSet_ThemeColor_{profileName}_{i * 10}_Hover: {colorStep1.Hex};";
                            result.Add(lineStep1);
                        }

                        if (s.FocusStepSize != 0)
                        {
                            Color  colorStep2 = profileColorsStep2[i];
                            string lineStep2  = @$"$ColorSet_ThemeColor_{profileName}_{i * 10}_Focus: {colorStep2.Hex};";
                            result.Add(lineStep2);
                        }

                        Console.WriteLine(
                            $"\t\t{line} -> {Math.Round(color.Hue, 1)}, {Math.Round(100 * color.Sat, 1)}, {Math.Round(100 * color.HSV[2] ?? 0.0, 1)}");
                    }

                    Console.WriteLine();
                }

                File.WriteAllLines($"./Themes/{directory}/theme.scss", result);
            }
        }
    }
}