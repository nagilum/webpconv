using System.Drawing.Imaging;
using WebPWrapper;

namespace webpconv
{
    internal static class Program
    {
        /// <summary>
        /// Whether to delete input files after convert.
        /// </summary>
        private static bool DeleteAfterConvert { get; set; } = false;

        /// <summary>
        /// List of input files to convert.
        /// </summary>
        private static List<string> InputFiles { get; set; } = new();

        /// <summary>
        /// Image format to save to.
        /// </summary>
        private static ImageFormat OutputFormat { get; set; } = ImageFormat.Jpeg;

        /// <summary>
        /// Init all the things..
        /// </summary>
        /// <param name="args">Program parameters.</param>
        private static void Main(string[] args)
        {
            try
            {
                if (!ParseProgramParameters(args))
                {
                    ShowProgramParameters();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return;
            }

            if (InputFiles.Count == 0)
            {
                return;
            }

            var webp = new WebP();

            foreach (var inputFile in InputFiles)
            {
                try
                {
                    Console.WriteLine($"Converting {inputFile}");

                    var bitmap = webp.Load(inputFile);
                    var fileInfo = new FileInfo(inputFile);

                    string ext;

                    if (OutputFormat == ImageFormat.Jpeg)
                    {
                        ext = ".jpeg";
                    }
                    else if (OutputFormat == ImageFormat.Png)
                    {
                        ext = ".png";
                    }
                    else
                    {
                        throw new Exception(
                            $"Invalid output format: {OutputFormat}");
                    }

                    var outputFile = fileInfo.FullName.Replace(fileInfo.Extension, ext);

                    var encoderParameters = new EncoderParameters(1);
                    var qualityParameter = new EncoderParameter(Encoder.Quality, 100L);

                    encoderParameters.Param[0] = qualityParameter;

                    var formatEncoder = GetEncoder(OutputFormat);

                    if (formatEncoder == null)
                    {
                        throw new Exception(
                            $"Unable to find codec for format: {OutputFormat}");
                    }

                    bitmap.Save(
                        outputFile,
                        formatEncoder,
                        encoderParameters);

                    Console.WriteLine($"Wrote {outputFile}");

                    if (DeleteAfterConvert)
                    {
                        File.Delete(inputFile);
                        Console.WriteLine($"Deleted {inputFile}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get the corresponding codec for the given format.
        /// </summary>
        /// <param name="format">Image format.</param>
        /// <returns>Image codec.</returns>
        private static ImageCodecInfo? GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// Parse program parameters into actionable data.
        /// </summary>
        /// <param name="args">Program parameters.</param>
        /// <returns>Success.</returns>
        /// <exception cref="Exception">Thrown if some parameters are incorrect.</exception>
        private static bool ParseProgramParameters(string[] args)
        {
            if (args.Length == 0 ||
                args.Any(n => n == "-h" ||
                              n == "--help"))
            {
                return false;
            }

            var skip = false;

            for (var i = 0; i < args.Length; i++)
            {
                if (skip)
                {
                    skip = false;
                    continue;
                }

                if (Directory.Exists(args[i]))
                {
                    try
                    {
                        InputFiles.AddRange(
                            Directory.GetFiles(
                                args[i],
                                "*.webp",
                                SearchOption.AllDirectories));
                    }
                    catch
                    {
                        //
                    }
                }
                else if (File.Exists(args[i]))
                {
                    InputFiles.Add(args[i]);
                }
                else
                {
                    switch (args[i])
                    {
                        case "-d":
                            DeleteAfterConvert = true;
                            break;

                        case "-of":
                            if (i == args.Length - 1)
                            {
                                throw new Exception(
                                    $"-of must be followed by a valid format.");
                            }

                            OutputFormat = args[i + 1] switch
                            {
                                "jpeg" => ImageFormat.Jpeg,
                                "png" => ImageFormat.Png,

                                _ => throw new Exception(
                                    $"Invalid format: {args[i + 1]}")
                            };

                            skip = true;
                            break;

                        default:
                            throw new Exception(
                                $"Unknown option: {args[i]}");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Show program parameters and usage.
        /// </summary>
        private static void ShowProgramParameters()
        {
            Console.WriteLine("webpconv v0.1-alpha");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  webpconv <paths> [<options>]");
            Console.WriteLine();
            Console.WriteLine("  You can add as many paths as you want, either folder or file path.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -d             Delete original .webp files after they've been successfully converted.");
            Console.WriteLine("  -of <format>   Set output format. Available formats are jpeg and png. Defaults to jpeg.");
            Console.WriteLine();
        }
    }
}