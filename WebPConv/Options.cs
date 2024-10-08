using System.Drawing.Imaging;

namespace WebPConv;

public class Options : IOptions
{
    /// <summary>
    /// <inheritdoc cref="IOptions.DeleteAfterConvert"/>
    /// </summary>
    public bool DeleteAfterConvert { get; set; }
    
    /// <summary>
    /// <inheritdoc cref="IOptions.OutputFormat"/>
    /// </summary>
    public ImageFormat OutputFormat { get; set; } = ImageFormat.Jpeg;

    /// <summary>
    /// <inheritdoc cref="IOptions.Overwrite"/>
    /// </summary>
    public bool Overwrite { get; set; }

    /// <summary>
    /// <inheritdoc cref="IOptions.Paths"/>
    /// </summary>
    public List<string> Paths { get; } = [];

    /// <summary>
    /// <inheritdoc cref="IOptions.Recursive"/>
    /// </summary>
    public bool Recursive { get; set; }
}