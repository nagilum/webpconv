using System.Drawing.Imaging;
using WebPLib;

namespace WebPConv;

public static class Program
{
    /// <summary>
    /// Init all the things...
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private static void Main(string[] args)
    {
        if (args.Length is 0 ||
            args.Any(n => n is "-h" or "--help"))
        {
            ShowProgramUsage();
            return;
        }

        if (!TryParseCmdArgs(args, out var options))
        {
            return;
        }

        var files = new List<string>();

        foreach (var path in options.Paths)
        {
            try
            {
                files.AddRange(
                    Directory.GetFiles(
                        path,
                        "*.webp",
                        options.Recursive
                            ? SearchOption.AllDirectories
                            : SearchOption.TopDirectoryOnly));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error! Unable to get .webp files from {path}");
                Console.WriteLine(ex.Message);
            }
        }

        var webp = new WebP();

        foreach (var inputFile in files)
        {
            try
            {
                Console.WriteLine($"Input: {inputFile}");

                var bitmap = webp.Load(inputFile);
                var fileInfo = new FileInfo(inputFile);

                string ext;

                if (options.OutputFormat.Equals(ImageFormat.Jpeg))
                {
                    ext = ".jpeg";
                }
                else if (options.OutputFormat.Equals(ImageFormat.Png))
                {
                    ext = ".png";
                }
                else
                {
                    throw new Exception("Invalid output format.");
                }

                var outputFile = fileInfo.FullName.Replace(fileInfo.Extension, ext);
                var encoderParameters = new EncoderParameters(1);
                var qualityParameter = new EncoderParameter(Encoder.Quality, 100L);
                var codec = ImageCodecInfo.GetImageEncoders()
                    .FirstOrDefault(n => n.FormatID.Equals(options.OutputFormat.Guid));

                if (codec is null)
                {
                    throw new Exception($"No image codec for {options.OutputFormat}");
                }

                encoderParameters.Param[0] = qualityParameter;

                if (File.Exists(outputFile) &&
                    !options.Overwrite)
                {
                    Console.WriteLine($"Error: Output file {outputFile} already exists.");
                    Console.WriteLine();
                    
                    continue;
                }

                bitmap.Save(
                    outputFile,
                    codec,
                    encoderParameters);
                
                Console.WriteLine($"Output: {outputFile}");

                if (options.DeleteAfterConvert)
                {
                    File.Delete(inputFile);
                    Console.WriteLine($"Deleted: {inputFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Show program usage and options.
    /// </summary>
    private static void ShowProgramUsage()
    {
        var lines = new[]
        {
            "WebP Converter v0.2-beta",
            "Converts WebP files to either Jpeg or Png.",
            "",
            "Usage:",
            "  WebPConv <paths> [<options>]",
            "",
            "You can add as many paths as you want. The converted files will be created in the same folder as the original.",
            "",
            "Options:",
            "  -d|--delete            Delete the .webp file if it was successfully converted.",
            "  -f|--format <format>   Set output format. Can be either jpeg or png. Defaults to jpeg.",
            "  -o|--overwrite         Overwrite the output file if it already exists.",
            "  -r|--recursive         Get files recursively for each provided path.",
            "",
            "Source and documentation available at https://github.com/nagilum/webpconv",
            ""
        };

        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
    }

    /// <summary>
    /// Attempt to parse the command line arguments to options class.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <param name="options">Parsed options.</param>
    /// <returns>Success.</returns>
    private static bool TryParseCmdArgs(string[] args, out IOptions options)
    {
        options = new Options();

        var skip = false;

        for (var i = 0; i < args.Length; i++)
        {
            if (skip)
            {
                continue;
            }
            
            switch (args[i])
            {
                case "-d":
                case "--delete":
                    options.DeleteAfterConvert = true;
                    break;
                
                case "-f":
                case "--format":
                    if (i == args.Length - 1)
                    {
                        Console.WriteLine("Error! You have to specify an output format for the image files.");
                        return false;
                    }

                    if (args[i + 1].Equals("jpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        options.OutputFormat = ImageFormat.Jpeg;
                        skip = true;
                        break;
                    }
                    
                    if (args[i + 1].Equals("png", StringComparison.OrdinalIgnoreCase))
                    {
                        options.OutputFormat = ImageFormat.Png;
                        skip = true;
                        break;
                    }
                    
                    Console.WriteLine("Error! Invalid value for output format.");
                    return false;
                
                case "-o":
                case "--overwrite":
                    options.Overwrite = true;
                    break;
                
                case "-r":
                case "--recursive":
                    options.Recursive = true;
                    break;
                
                default:
                    if (Directory.Exists(args[i]))
                    {
                        options.Paths.Add(args[i]);
                        break;
                    }
                    
                    Console.WriteLine($"Error! Invalid path: {args[i]}");
                    return false;
            }
        }

        if (options.Paths.Count > 0)
        {
            return true;
        }

        Console.WriteLine("Error! You have to specify at least one path to read .webp files from.");
        return false;
    }
}