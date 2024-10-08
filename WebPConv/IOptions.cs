using System.Drawing.Imaging;

namespace WebPConv;

public interface IOptions
{
    /// <summary>
    /// Whether to delete the original .webp file after converting it.
    /// </summary>
    bool DeleteAfterConvert { get; set; }
    
    /// <summary>
    /// Image format of the output file.
    /// </summary>
    ImageFormat OutputFormat { get; set; }
    
    /// <summary>
    /// Overwrite output file if it exists.
    /// </summary>
    bool Overwrite { get; set; }
    
    /// <summary>
    /// Paths to read .webp files from.
    /// </summary>
    List<string> Paths { get; }
    
    /// <summary>
    /// Get files recursively for each provided path.
    /// </summary>
    bool Recursive { get; set; }
}