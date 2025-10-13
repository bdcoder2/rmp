using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;

/*
==================================================

 Helper class for application settings.

==================================================
*/
namespace rmp
{
   public static class app_settings
   {

      static private Microsoft.Extensions.Configuration.IConfigurationRoot m_config = null;

      static private Microsoft.Extensions.Hosting.IHostEnvironment m_host_environment = null;

      static private String m_run_mode_config_file_spec = String.Empty;

      static public readonly System.Version app_version = null;

      static public readonly String content_root_path = String.Empty;

      static public readonly String web_root_path = String.Empty;


      /*
      --------------------------------------------------
      
      Constructor

      --------------------------------------------------
      */
      static app_settings()
      {

         System.Reflection.Assembly assembly;

         System.Reflection.AssemblyName assembly_name;


         // Get entry assembly (can return null if called from unmanaged code) ...

         assembly = System.Reflection.Assembly.GetExecutingAssembly();

         if ( assembly is null )

            throw new Exception( "Unable to get executing assembly?" );


         // Set app version and make sure it is not null ...

         assembly_name = assembly.GetName();

         if ( assembly_name.Version is null )
         {

            app_version = new Version( 0, 0, 0, 0 );

         }
         else
         {

            app_version = assembly_name.Version;

         }


         /*
         The <content-root> is the base path to any content (code, settings, etc) 
         used by the app.  By default the content root is the same as the application
         base path for the executable hosting the app; that is, the content root is: 

         String content_root = System.IO.Directory.GetCurrentDirectory();

         If need be, an alternative location can be specified with the following:

            config.UseContentRoot( "/some/other/directory" );

         NOTE:
         When using Visual Studio, if we build and ran this project using IIS Express,
         the content root used would be the PROJECT path, that is, the path returned
         by:

            app_settings.content_root_path = System.IO.Directory.GetCurrentDirectory();

         The above, would always return the PROJECT path rather than the path of the
         build target (output) directory.  To use the actual build target directory, 
         we instead use the path of the executing assembly location.
         
         REFERENCES:
         - https://learn.microsoft.com/en-us/aspnet/core/fundamentals/?tabs=windows#content-root
         */

         content_root_path = System.IO.Path.GetDirectoryName( assembly.Location );


         /*
         The <web-root> is the directory for public, static resources like css, js, 
         and image files.  The static files middleware will only serve files from 
         the web root directory and any sub-directories by default. The web root path
         defaults to <content root>/wwwroot, but you can specify a different web root
         name (other than "wwwroot").  Below, we set <web-root> to "www" ...

         IMPORTANT:
         Code files (including C# and Razor) should be placed OUTSIDE of the web root.
         This creates a clean separation between your client side content and server 
         side source code, which prevents server side code from being leaked.  For 
         example, someone could make a request for a settings file, ie: 
         https://<content-root>/<web-root>/appsettings.json ... or any other config file !!

         IMPORTANT:
         When changing the default <web-root> name (from "wwwroot" to something else), 
         the following entry must be included in the {projectname}.csproj file:

         1. In Visual Studio, right-click on project name -> Edit Project File

         2. Add the following entry to the project file:

           <ItemGroup>
            <None Update="www\**\*">
             <CopyToOutputDirectory>Always</CopyToOutputDirectory>
            </None>
           </ItemGroup>
 
         
         REFERENCES:
         - https://learn.microsoft.com/en-us/aspnet/core/fundamentals/
         */

         web_root_path = System.IO.Path.Combine( content_root_path, @"www" );

      }


      /*
      --------------------------------------------------

      Returns true if a configuration key exists, false 
      otherwise.

      Keys at different levels, are "flattened" down to a 
      single key in the format of "key1:key2:keyn".  For
      exmaple, if a configuration file contained the
      following JSON:

      {
         "appsettings": {
            "Level1": {
               "Level2": {
                  "level2_key": "prod_level2_value"
               },
               "level1_key": "prod_level1_value"
            }
         }
      }

      If we want the value for the key "level2_key", then 
      the key (path) would be: 
      
      "appsettings:level1:level2:level2_key".

      --------------------------------------------------
      */
      public static bool config_key_exists( String key )
      {

         Int32 idx;

         String key_name;
         String key_path;

         Microsoft.Extensions.Configuration.IConfigurationSection section;

         System.Collections.Generic.IEnumerable<IConfigurationSection> children;


         // Key name or path is required ...

         if ( String.IsNullOrWhiteSpace( key ) )

            return false;


         // config object must exist ...

         if ( config is null )

            return false;


         // Check if a key name or path was specified ...

         idx = key.LastIndexOf( ':' );

         if ( idx < 0 )
         {

            // Just a key name ...

            key_name = key;

            key_path = null;

         }
         else
         {

            // Seperate key name and path ...

            key_name = key.Substring( idx + 1 );

            key_path = key.Substring( 0, idx );

         }


         // Get children of key path if path was present ...

         children = null;

         if ( key_path is null )
         {

            children = config.GetChildren();

         }
         else
         {

            /*
            The GetSection method will never return null.  If no matching sub-section is 
            found with the specified key, an empty IConfigurationSection will be returned ...
            */

            section = config.GetSection( key_path );

            if ( section.Exists() )

               children = section.GetChildren();

         }

         if ( children is null )

            return false;


         // Loop to find the given key ...

         foreach ( Microsoft.Extensions.Configuration.IConfigurationSection child in children )
         {

            if ( String.Equals( key_name, child.Key, System.StringComparison.OrdinalIgnoreCase ) )

               return true;

         }

         return false;

      }



      /*
      --------------------------------------------------

      Returns true if a configuration section exists, 
      false otherwise.

      --------------------------------------------------
      */
      public static bool config_section_exists( String section_path )
      {

         Microsoft.Extensions.Configuration.IConfigurationSection section;


         // Section path to be found is required ...

         if ( String.IsNullOrEmpty( section_path ) )

            return false;


         // config object must exist ...

         if ( config is null )

            return false;


         /*
         The GetSection method will never return null.  If no matching sub-section is 
         found with the specified key, an empty IConfigurationSection will be returned ...
         */

         section = config.GetSection( section_path );

         return section.Exists();

      }


      /*
      --------------------------------------------------

      Returns a config key value if the key exists, null
      otherwise.

      --------------------------------------------------
      */
      public static String config_value( String key )
      {

         // If config key exists, return value ...

         if ( config_key_exists( key ) )

            return config[ key ];


         return null;

      }


      /*
      --------------------------------------------------

      Create a dictionary of configuration keys / values ...

      The most important thing to know about the configuration system 
      is that everything boils down to key/value pairs. You may have a 
      pseudo-hierarchy of these key/value pairs so you can walk it like
      a tree, but in the end it is still key/value pairs, like a dictionary. 
      No matter the input format, it all gets normalized.

      - Keys are case-insensitive.
      - Values are strings.
      - The hierarchy / path delimiter is a colon (:), when querying parsed configuration.
      - Every configuration provider flattens their structure down to the same normalized format.

      REFERENCES:
      - https://www.paraesthesia.com/archive/2018/06/20/microsoft-extensions-configuration-deep-dive/

      --------------------------------------------------
      */
      public static Dictionary<String, String> config_to_dictionary( String top_path = null, bool use_path_as_key = true )
      {

         String config_key;

         Microsoft.Extensions.Configuration.IConfigurationSection section;

         System.Collections.Generic.IEnumerable<IConfigurationSection> children;

         System.Collections.Generic.Stack<String> stack;

         System.Collections.Generic.Dictionary<String, String> dict;


         // Init dictionary to return ...

         dict = new();

         if ( config is not null )
         {

            // Init stack ...

            stack = new();

            // If no top path was specified, get children of the config root ....

            if ( top_path is null )
            {

               children = config.GetChildren();

            }
            else
            {

               /*
               A top path was specified, get children starting from the top path.
               The GetSection method will never return null.  If no matching sub-section is 
               found with the specified key, an empty IConfigurationSection will be returned ...
               */

               section = config.GetSection( top_path );

               if ( section.Exists() )
               {

                  children = section.GetChildren();

               }
               else
               {

                  children = null;

               }

            }


            // Loop to populate the dictionary until no more children are found ...

            while ( children != null )
            {

               foreach ( Microsoft.Extensions.Configuration.IConfigurationSection child in children )
               {

                  // If no value, push path onto stack ...

                  if ( child.Value is null )
                  {

                     stack.Push( child.Path );

                  }
                  else
                  {

                     // If using the path as the key, save path (as key) and value ...

                     if ( use_path_as_key )
                     {

                        dict[ child.Path ] = child.Value;

                     }
                     else
                     {

                        // Save key only and value ...

                        dict[ child.Key ] = child.Value;

                     }

                  }

               }

               // If more children exist, get next section ...

               if ( stack.Count > 0 )
               {

                  config_key = stack.Pop();

                  section = config.GetSection( config_key );

                  children = section.GetChildren();

               }
               else
               {

                  children = null;

               }

            }

         }

         return dict;

      }


      /*
      --------------------------------------------------
      
      Load application configuration values from various 
      sources.
      
      See: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments

      To determine the runtime environment, ASP.NET Core reads from the following environment 
      variables:

        1. DOTNET_ENVIRONMENT

        2. ASPNETCORE_ENVIRONMENT when the WebApplication.CreateBuilder method is called. The default 
           ASP.NET Core web app templates call WebApplication.CreateBuilder. The DOTNET_ENVIRONMENT 
           value overrides ASPNETCORE_ENVIRONMENT when WebApplicationBuilder is used. For other hosts, 
           such as ConfigureWebHostDefaults and WebHost.CreateDefaultBuilder, ASPNETCORE_ENVIRONMENT 
           has higher precedence.

      - Although the environment can be any String value, the following environment values are 
        provided by the framework: "Development", "Staging" and "Production".

      - If the DOTNET_ENVIRONMENT and ASPNETCORE_ENVIRONMENT environment variables are not set, the
        Production environment is the default environment.
      
      - Apps deployed to Azure are Production by default.

      - To set the "ASPNETCORE_ENVIRONMENT" environment variable in the Azure Portal for a Web App, use
        the Azure Portal or Azure CLI to set the environment variable.  For example, to set the
        the environment variable via the CLI:

        > az webapp config appsettings set --name mywebsite-dev --resource-group mywebsite-dev --settings ASPNETCORE_ENVIRONMENT=Development

        To List settings for the web app:

        > az webapp config appsettings list --name mywebsite-dev --resource-group mywebsite-dev

        To delete a setting / environment variable:

        > az webapp config appsettings delete --name mywebsite-dev --resource-group mywebsite-dev --setting-names ASPNETCORE_ENVIRONMENT


      - WHEN TESTING LOCALLY using Visual Studio, the "ASPNETCORE_ENVIRONMENT" environment variable is
        set in the "launchSettings.json" file, which is located in the "Properties" folder of the project.

        The launchSettings.json file in an ASP.NET Core project serves the purpose of configuring how the 
        application is launched and run during local development. The launchSettings.json file:
        - Is only used on the local development machine.
        - Is NOT deployed when the app is published.
        - May contain multiple profiles, each configuring a different environment.

        {
          ...
           },
           "profiles": {
             "IIS Express": {
               "commandName": "IISExpress",
               "launchBrowser": true,
               "environmentVariables": {
                 "ASPNETCORE_ENVIRONMENT": "Development"
               }
             },
             ...
             }
           }
        }
      
      - When the host is built, the last environment setting read by the app determines the app's environment. 
        The environment cannot be changed while the app is running.
      

      IMPORTANT:
      - Configuration values are loaded progressively, meaning that as providers are added the values from that 
        provider will be loaded in the same sequence in which they were added to the configuration builder and 
        each is given a chance to override entries previously set. If the same configuration key, say "timezone", 
        is contributed by multiple providers, then the last wins.

      RESOURCES:
      - https://www.red-gate.com/simple-talk/dotnet/net-development/asp-net-core-3-0-configuration-factsheet/

      --------------------------------------------------
      */
      public static void load( Microsoft.AspNetCore.Builder.WebApplicationBuilder web_app_builder )
      {

         String config_file_name;
         String config_file_spec;

         String[] cmd_line_args;


         // WebApplicationBuilder is required ...

         ArgumentNullException.ThrowIfNull( web_app_builder );


         // Make sure a host environment name has been set ...
         
         if ( String.IsNullOrWhiteSpace( web_app_builder.Environment.EnvironmentName ) )

            throw new Exception( "Environment.EnvironmentName is not set. Define the environment variable: ASPNETCORE_ENVIRONMENT" );


         // Save the host environment ...

         m_host_environment = web_app_builder.Environment;


         // Set base path for file-based configuration providers, ie: .json, XML, etc. ...

         web_app_builder.Configuration.SetBasePath( content_root_path );


         // Clear all configuration sources so we start with a clean slate ...

         web_app_builder.Configuration.Sources.Clear();


         /*
         Check for a COMMON settings file for ALL environments (test, production, etc).
         If found, add the keys/values from it ...
         */

         config_file_name = "appsettings_common.json";

         config_file_spec = System.IO.Path.Combine( content_root_path, config_file_name );

         if ( System.IO.File.Exists( config_file_spec ) )

				web_app_builder.Configuration.AddJsonFile( config_file_name, optional: false, reloadOnChange: false );

         
         /*
         Set the RUN MODE configuration file based on the the host environment name, 
         according to the table below:

         Host Environment Name  Configuration file loaded
         ---------------------  -------------------------------------------------
         Development            {content-root-path}\appsettings_development.json
         Production             {content-root-path}\appsettings_production.json
         ---------------------  -------------------------------------------------

         If no configuration file is found, no values are loaded ...
         */

         config_file_name = null;

         if ( web_app_builder.Environment.IsDevelopment() )
         {

				config_file_name = "appsettings_development.json";

			}
         else if ( web_app_builder.Environment.IsProduction()  )
         {

				config_file_name = "appsettings_production.json";

			}


         // If config file name set, build complete path and if the file exists,
         // add values to our configuration ...

         if ( !String.IsNullOrWhiteSpace( config_file_name ) )
         {

				m_run_mode_config_file_spec = Path.Combine( content_root_path, config_file_name );

				if ( System.IO.File.Exists( m_run_mode_config_file_spec ) )

					web_app_builder.Configuration.AddJsonFile( m_run_mode_config_file_spec, optional: false, reloadOnChange: false );

			}


         /*
         If in development mode (just to illustrate how), load keys / values from an 
         in-memory collection and from the local "user secrets" ...

         You may edit the user secret keys / values via Visual Studio.  In the
         Solution Explorer: Right-click on project name -> Manage User Secrets
         
         The values are stored in a JSON file in the local machine's user profile 
         folder: %APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json

         You may also use the developers console/prompt. To list any defined 
         user secrets. Navigate to the directory that contains the project
         file, as follows:

         > cd D:\path\to\yourproject.csproj
         > dotnet user-secrets list

         See: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets
         */

         if ( web_app_builder.Environment.IsDevelopment() )
         {

            System.Collections.Generic.Dictionary<String, String> in_memory_dict = new() {
               { "in_memory_key0", "in_memory_value0" },
               { "in_memory_key1", "in_memory_value1" }
            };

            // Add in-memory keys / values ...

            web_app_builder.Configuration.AddInMemoryCollection( in_memory_dict );


            // Add user secret keys / values ...

            System.Reflection.Assembly app_assembly = Assembly.GetEntryAssembly();

            if ( app_assembly is not null )

               web_app_builder.Configuration.AddUserSecrets( app_assembly );

         }


         /*
         Add keys / values from any environment variables defined ...

         NOTE: The AddEvironmentVariables() method will load ALL environment variables.  You may
               pass a "prefix" to the method so that only those environment variables that start
               with a given prefix will be loaded.  For example, the statement below will load only
               those environment variables that are prefixed with "ASPNETCORE_"

               config_builder.AddEnvironmentVariables( "ASPNETCORE_" );

               On the Windows platform, the prefix is not case-sensitive, and will be removed 
               from the environment variable names when loaded.  For example. if the environment
               variable name is "ASPNETCORE_WIDGET", the configuration key name will be "WIDGET".
         */

         web_app_builder.Configuration.AddEnvironmentVariables();


         // If command line args are present, add those keys / values ...

         cmd_line_args = System.Environment.GetCommandLineArgs();

         if ( cmd_line_args is not null )

            web_app_builder.Configuration.AddCommandLine( cmd_line_args );


         // Save configuration in our app settings ...

         m_config = web_app_builder.Configuration;

      }
      
      
      /*
      --------------------------------------------------
      Given a URL path RELATIVE to <web-root>, return a 
      physical path.
      
      ARGUMENTS:

         url_path

         A String containing the RELATIVE URL path to a
         resource.  For example: /about/index.html


      RETURNS:

         A String containing the physical path. For example, 
         if the <web-root> physical path is: 
      
            D:\website\www 
         
         and the url_path given is:
      
            /about/index.html
         
         the physical file path returned will be: 
      
            D:\website\www\about\index.html


      NOTES:
      
         This routine returns physical paths specific to Windows.
         Other operating systems may use different physical path
         delimiters.
      
         If the <web-root> has not been set, String.Empty
         will be returned.

         If the url_path is null or String.Empty, String.Empty
         will be returned.

      --------------------------------------------------
      */
      public static String physical_file_path( String url_path )
      {

         const Char k_physical_file_path_seperator_char = '\\';

         const Char k_url_path_seperator_char = '/';


         if ( String.IsNullOrEmpty( web_root_path ) )

            return String.Empty;


         if ( String.IsNullOrEmpty( url_path ) )

            return String.Empty;


         /*
         The web root path does not end with a slash, so make
         sure the url path does ...
         */

         String physical_file = web_root_path;

         if ( !url_path.StartsWith( k_url_path_seperator_char ) )

            physical_file += k_physical_file_path_seperator_char;


         physical_file += url_path.Replace( k_url_path_seperator_char, k_physical_file_path_seperator_char );

         return physical_file;

      }


      /*
      --------------------------------------------------

      Returns a String with all configuration setting 
      keys / values ...

      --------------------------------------------------
      */
      public static String to_string()
      {

         const String k_seperator_line = "--------------";

         String s;

         StringBuilder sb;

         System.Collections.Generic.Dictionary<String, String> config_dict;


         // Init ...

         sb = new StringBuilder();


         // Show configuration ...

         sb.AppendLine( k_seperator_line );

         sb.AppendLine( "CONFIGURATION:" );

         sb.AppendLine( "Application Name: " + app_name );

         sb.AppendLine( "Version: " + app_version.ToString() );

#if DEBUG
         sb.AppendLine( "Build: DEBUG" );
#elif RELEASE
         sb.AppendLine( "Build: RELEASE" );
#endif

         sb.Append( "Host Environment: " );

         if ( m_host_environment is null )
         {

            sb.AppendLine( "*** Host Environment has not been configured ***" );

         }
         else
         {

            sb.AppendLine( "Host Environment: " + m_host_environment.EnvironmentName );

         }


         sb.Append( "Run Mode Config File: " );

         if ( String.IsNullOrWhiteSpace( m_run_mode_config_file_spec ) )
         {

            sb.AppendLine( "*** Run mode configuration file not found ***" );

         }
         else
         {

            sb.AppendLine( m_run_mode_config_file_spec );

         }


         sb.AppendLine( "Content Root Path: " + content_root_path );

         sb.AppendLine( "Web Root Path: " + web_root_path );


         // Just for fun - show the process name (w3wp = IIS, iisexpres = IISExpress, etc.) ...
         
         sb.AppendLine( "Process Name: " + System.Diagnostics.Process.GetCurrentProcess().ProcessName );


         // Just for fun - show the configuration providers that were used and their load order ...
         
         sb.AppendLine( k_seperator_line );

         sb.AppendLine( "CONFIGURATION PROVIDERS:" );
         
         if ( m_config is null )
         {

            sb.AppendLine( "*** No configuration providers were loaded ***" );

         }
         else
         {

            Int32 load_order = 0;

            foreach ( Microsoft.Extensions.Configuration.IConfigurationProvider config_provider in m_config.Providers )
            {

               load_order++;

               s = $@"Load order: {load_order}, Provider: {config_provider.GetType().Name}";

               if ( config_provider is Microsoft.Extensions.Configuration.Json.JsonConfigurationProvider json_provider )

                  s += $@", File Path: " + Path.Combine( content_root_path, json_provider.Source.Path );

               sb.AppendLine( s );

            }

         }


         // Show ALL config keys / values ...

         sb.AppendLine( k_seperator_line );

         sb.AppendLine( "CONFIGURATION KEYS/VALUES:" );

         config_dict = config_to_dictionary();

         foreach ( KeyValuePair<String, String> kvp in config_dict )

            sb.AppendLine( "Key: " + kvp.Key + ", Value: " + kvp.Value );


         return sb.ToString();

      }


      /*
      --------------------------------------------------

      Returns the application name ...

      --------------------------------------------------
      */
      public static String app_name
      {

         get
         {

            if ( m_host_environment is null )

               return "Unknown";


            return m_host_environment.ApplicationName;

         }

      }


      /*
      --------------------------------------------------
      
      Returns an IConfigurationRoot object ...

      NOTE:
      
         Will be null if the load() method has not been
         called.

      --------------------------------------------------
      */
      public static Microsoft.Extensions.Configuration.IConfigurationRoot config
      {

         get
         {

            return m_config;

         }

      }


      /*
      --------------------------------------------------
      
      Returns an IHostEnvironment object.
      
      NOTE:
      
         Will be null if the load() method has not been
         called.

      --------------------------------------------------
      */
      public static IHostEnvironment host_environment
      {

         get
         {

            return m_host_environment;

         }

      }


      /*
      --------------------------------------------------
      
      Returns true if the host environment is PRODUCTION,
      false otherwise ...

      --------------------------------------------------
      */
      public static bool is_production
      {

         get
         {

            if ( m_host_environment is null )

               return false;


            return m_host_environment.IsProduction();

         }

      }


      /*
      --------------------------------------------------
      
      Returns true if the host environment is DEVELOPMENT,
      false otherwise ...

      --------------------------------------------------
      */
      public static bool is_development
      {

         get
         {

            if ( m_host_environment is null )

               return false;


            return m_host_environment.IsDevelopment();

         }

      }


      /*
      --------------------------------------------------

      Returns the run mode configuration fiel spec or
      null if no configuration file was found.

      --------------------------------------------------
      */
      public static String run_mode_config_file_spec
      {

         get
         {

            return m_run_mode_config_file_spec;

         }

      }

   }

}
