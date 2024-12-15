using System.CommandLine;
using Utilities.BulkDataUpload;

namespace Utilities;

/// <summary>
/// Main entry point for the Utilities project
/// </summary>
public static class Program
{
    /// <summary>
    /// Parses the command line arguments and runs the appriopriate utility.
    /// </summary>
    /// <param name="args">List of command line arguments</param>
    /// <returns>The return status code of the application</returns>
    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Financial Tracker Utilities");

        // Configure the Bulk Data Upload command
        var jsonFileOption = new Option<string>("--folder", "Path to the folder with JSON files to upload")
        {
            IsRequired = true
        };
        var uploadCommand = new Command("upload", "Uploads an entire accounting period or group of accounting periods to the REST API")
        {
            jsonFileOption
        };
        uploadCommand.SetHandler(async (jsonFilePath) => await new BulkDataUploadUtility(jsonFilePath).Run(), jsonFileOption);
        rootCommand.AddCommand(uploadCommand);

        return rootCommand.Invoke(args);
    }
}