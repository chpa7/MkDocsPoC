// MkDocs requires a recent version of Python
// and the Python package manager, pip, to be installed on the system.
// Check https://www.mkdocs.org/user-guide/installation/ out for instructions. 

using System.Diagnostics;
using System.Reflection;
using YamlDotNet.RepresentationModel;

class Program
{
    static readonly string? currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    static readonly string allScenariosDir = currentDir + "\\..\\..\\..\\scenarios";
    static readonly string mkDocsDir = currentDir + "\\docs";

    enum PossibleTags
    {
        xm,
        xmcloud,
        dam,
        ops,
        cdp,
        personalize,
        send,
        ordercloud,
        discover,
        headlesscms,
        search
    }

    static void Main(string[] args)
    {
        // Create a dictionary with all the available tags and their locations
        Dictionary<PossibleTags, string> scenariosDict = new()
        {
            {PossibleTags.xm, allScenariosDir + "\\xm"},
            {PossibleTags.xmcloud, allScenariosDir + "\\xmcloud"},
            {PossibleTags.dam, allScenariosDir + "\\dam"},
            {PossibleTags.ops, allScenariosDir + "\\ops"},
            {PossibleTags.cdp, allScenariosDir + "\\cdp"},
            {PossibleTags.personalize, allScenariosDir + "\\personalize"},
            {PossibleTags.send, allScenariosDir + "\\send"},
            {PossibleTags.ordercloud, allScenariosDir + "\\ordercloud"},
            {PossibleTags.discover, allScenariosDir + "\\discover"},
            {PossibleTags.headlesscms, allScenariosDir + "\\headlesscms"},
            {PossibleTags.search, allScenariosDir + "\\search"}
        };

        // Create the mkdocs config file
        CreateMkDocsConfig();

        // Create the docs directory if it doesn't already exist
        Directory.CreateDirectory(mkDocsDir);

        // Copy the home page to the docs directory
        File.Copy(allScenariosDir + "\\index.md", mkDocsDir + "\\index.md");

        // If there are input arguments then copy only the respective scenarios,
        // otherwise copy all scenarios
        if (args.Length > 0)
        {
            foreach (string arg in args)
            {
                // If the argument is one of the possible tags and a directory exists for it in the scenarios dictionary
                // then copy the scenario over to the docs folder
                if (Enum.TryParse(arg, out PossibleTags tag) && scenariosDict.ContainsKey(tag) && scenariosDict.TryGetValue(tag, out string? scenarioDir))
                {
                    Console.WriteLine(tag);
                    CopyScenario(scenarioDir, mkDocsDir);
                }
            }
        } else
        {
            foreach (string scenarioDir in scenariosDict.Values)
            {
                CopyScenario(scenarioDir, mkDocsDir);
            }
        }

        RunMkDocs("mkdocs", "build");

        // Use ftp, ssh or scp to transfer the files to the server
        // eg. scp -r ./site user@host:/path/to/server/root

        // Keep console window open in debug mode.
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();

        // Delete the docs directory
        Directory.Delete(mkDocsDir, true);
    }

    static void CreateMkDocsConfig()
    {
        var stream = new YamlStream(
            new YamlDocument(
                new YamlMappingNode(
                    new YamlScalarNode("site_name"), new YamlScalarNode("MkDocsPoC"),
                    new YamlScalarNode("site_url"), new YamlScalarNode("https://docs.sitecore.com/demo/instanceID/"),
                    new YamlScalarNode("use_directory_urls"), new YamlScalarNode("false"),
                    new YamlScalarNode("theme"), new YamlMappingNode(
                        new YamlScalarNode("name"), new YamlScalarNode("readthedocs")
                    )
                )
            )
        );

        //stream.Save(Console.Out);
        using TextWriter writer = File.CreateText(currentDir + "\\mkdocs.yml");
        stream.Save(writer, false);
    }

    static void CopyScenario(string sourceDir, string destDir)
    {
        var allDirectories = Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories);

        foreach (string dir in allDirectories)
        {
            string dirToCreate = dir.Replace(sourceDir, destDir);
            Directory.CreateDirectory(dirToCreate);
        }

        var allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);

        foreach (string newPath in allFiles)
        {
            File.Copy(newPath, newPath.Replace(sourceDir, destDir), true);
        }
    }

    static void RunMkDocs(string cmd, string arg)
    {
        ProcessStartInfo start = new()
        {
            FileName = "C:\\Python310\\python.exe", // It should point to the python executable
            Arguments = string.Format("-m \"{0}\" \"{1}\"", cmd, arg), // -m is needed before specifying the module name
            UseShellExecute = false, // Do not use OS shell
            CreateNoWindow = true, // We don't need new window
            RedirectStandardOutput = true, // Any output, generated by the application will be redirected back
            RedirectStandardError = true // Any error in standard output will be redirected back (for example exceptions)
        };
        using Process? process = Process.Start(start);
        using StreamReader? reader = process?.StandardOutput;
        string? stderr = process?.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
        string? result = reader?.ReadToEnd(); // Here is the result of StdOut

        Console.WriteLine(stderr);
        Console.WriteLine(result);
    }
}
