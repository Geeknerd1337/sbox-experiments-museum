using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Sandbox;

namespace Editor.CodeEditors;

[Title( "Cursor" )]
public class Cursor : ICodeEditor
{
    public void OpenFile( string path, int? line = null, int? column = null )
    {
        var codeWorkspace = $"{Environment.CurrentDirectory}/s&box.code-workspace";
        CreateWorkspace( codeWorkspace );
        
        Launch( $"\"{codeWorkspace}\" -g \"{path}:{line}:{column}\"" );
    }

    public void OpenSolution()
    {
        var codeWorkspace = $"{Environment.CurrentDirectory}/s&box.code-workspace";
        CreateWorkspace( codeWorkspace );

        // Need to wrap the code workspace in quotes, but CreateWorkspace doesn't need that
        Launch( $"\"{codeWorkspace}\"" );
    }

    public void OpenAddon( Project addon )
    {
        var projectPath = (addon != null) ? addon.GetRootPath() : "";

        Launch( $"\"{projectPath}\"" );
    }

    public bool IsInstalled() => !string.IsNullOrEmpty( GetLocation() );
    
    private static void Launch( string arguments )
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = GetLocation(),
            Arguments = arguments,
            CreateNoWindow = true,
        };

        System.Diagnostics.Process.Start( startInfo );
    }
    
    private static void CreateWorkspace( string path )
    {
        StringBuilder builder = new();
        builder.AppendLine( "{" );
        builder.AppendLine( "    \"folders\": [" );

        foreach ( var addon in EditorUtility.Projects.GetAll() )
        {
            if ( !addon.Active ) continue;

            builder.AppendLine( "        {" );
            builder.AppendLine( $"            \"name\": \"{addon.Config.Ident}\"," );
            builder.AppendLine( $"            \"path\": \"{addon.GetRootPath().Replace( @"\", @"\\" )}\"," );
            builder.AppendLine( "        }," );
        }

        builder.AppendLine( "    ]," );

        // You need the C# extension to do anything
        builder.AppendLine( "    \"extensions\": {" );
        builder.AppendLine( "        \"recommendations\": [" );
        builder.AppendLine( "            \"ms-dotnettools.csharp\"" );
        builder.AppendLine( "        ]," );
        builder.AppendLine( "    }," );

        // Settings: make sure we're using .net 6 and that roslyn analyzers are on (they never fucking are)
        builder.AppendLine( "    \"settings\": {" );
        builder.AppendLine( "        \"omnisharp.useModernNet\": true," );
        builder.AppendLine( "        \"omnisharp.enableRoslynAnalyzers\": true" );
        builder.AppendLine( "    }" );

        builder.AppendLine( "}" );

        File.WriteAllText( path, builder.ToString() );
    }
    
    
    static string Location;

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>" )]
    private static string GetLocation()
    {
        if ( Location != null )
        {
            return Location;
        }

        try
        {
            // Check multiple possible registry locations
            string[] possiblePaths = new[]
            {
                @"Applications\Cursor.exe\shell\open\command",
                @"Cursor\shell\open\command"  // Alternative common path
            };

            foreach ( var path in possiblePaths )
            {
                using ( var key = Registry.ClassesRoot.OpenSubKey( path ) )
                {
                    string value = key?.GetValue( "" ) as string;
                    if ( !string.IsNullOrEmpty( value ) )
                    {
                        // More flexible regex that handles different command formats
                        var match = Regex.Match( value, "\"([^\"]+)\"", RegexOptions.IgnoreCase );
                        if ( match.Success )
                        {
                            Location = match.Groups[1].Value;
                            if ( File.Exists( Location ) )
                            {
                                return Location;
                            }
                        }
                    }
                }
            }

            Log.Warning( "Could not find Cursor installation in registry" );
            return null;
        }
        catch ( Exception ex )
        {
            Log.Warning( $"Error accessing registry: {ex.Message}" );
            return null;
        }
    }
}
