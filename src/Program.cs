using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace rmp
{
   public class Program
   {

      private static WebApplication web_app;

      /*
      --------------------------------------------------

      Create and run the web host ...

      --------------------------------------------------
      */
      public static async Task Main( string[] args )
      {

         // Try to create the host ...

         try
         {

            web_app = web_app_create( args );

         }
         catch ( Exception ex )
         {

            web_app = null;

            // Log exception as needed ...

            Console.WriteLine( ex.ToString() );

         }


         // If host is not null, run and block the calling thread until the host is shut down ...

         if ( web_app is not null )

            await web_app.RunAsync();

      }


      /*
      --------------------------------------------------

      Create and configure a web application / host ...

      --------------------------------------------------
      */
      private static WebApplication web_app_create( String[] args )
      {

         Microsoft.AspNetCore.Builder.WebApplicationOptions web_app_options;

         Microsoft.AspNetCore.Builder.WebApplicationBuilder web_app_builder;

         Microsoft.AspNetCore.Builder.WebApplication web_app;


         // Set web app options and create a web app builder ...

         web_app_options = new()
         {

            // Save command line args, we pickup command line args in app_settings.load() ...

            Args = args,

            // Set content root path (contains the application content files; app settings, dlls, etc) ...

            ContentRootPath = app_settings.content_root_path,

            // Set web root path (contains the web-servable content files; static files such as html, css, javascript, etc.) ...

            WebRootPath = app_settings.web_root_path

         };


         // Init web app builder ...

         web_app_builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder( web_app_options );

         
         // Load application settings / configuration ...

         app_settings.load( web_app_builder );


         // Configure logging as needed ...


         // Configure the web host as needed ...


         // Configure services ...

         services_configure( web_app_builder );


         // Build the web application ...

         web_app = web_app_builder.Build();


         // Configure middleware ...

         middleware_configure( web_app );


         return web_app;

      }


      /*
      --------------------------------------------------

      Configure middleware ...

      --------------------------------------------------
      */
      private static void middleware_configure( WebApplication app )
      {

         Microsoft.AspNetCore.Builder.DefaultFilesOptions default_file_opts;


         // If in production, enforce HTTPS ...

         if ( app.Environment.IsProduction() )
         {

            app.UseHsts();

            app.UseHttpsRedirection();

         }
         else if ( app.Environment.IsDevelopment() )
         {

            app.UseDeveloperExceptionPage();

         }


         // Use Routing (required) ...

         app.UseRouting();


         // Use routemap pages (map endpoints) ...

         app.use_rmp();


         /*
         IMPORTANT:
         The UseDefaultFiles method must be called BEFORE the UseStaticFiles method
         to serve the default file. UseDefaultFiles is a URL rewriter that does not
         actually serve the file ...
         */

         default_file_opts = new();

         default_file_opts.DefaultFileNames.Clear();

         default_file_opts.DefaultFileNames.Add( "index.html" );

         app.UseDefaultFiles( default_file_opts );


         /*
         IMPORTANT:
         UseStaticFiles() will serve static files in the <web-root> folder only and
         from any sub-directories within the <web-root>. 
         
         Static files are typically located in the <web-root> folder, which is typically
         located outside of the <content-root> folder for security purposes.
         */

         app.UseStaticFiles();


         /*
         The MapStaticAssets() method in ASP.NET Core, introduced in .NET 9, is 
         designed to efficiently serve static web assets known at build and 
         publish time. However, it DOES NOT inherently serve default files like 
         index.html or default.html when a directory is requested without a 
         specific file name. This functionality is typically provided by the 
         UseDefaultFiles middleware, which works in conjunction with UseStaticFiles
         (as above).

         To serve default files in an ASP.NET Core application using MapStaticAssets, 
         it is necessary to combine MapStaticAssets with UseStaticFiles and UseDefaultFiles.

         To configure the application's request pipeline in Program.cs to serve
         default files:
         
         1. Enable Default Files: 
            Call app.UseDefaultFiles() before app.UseStaticFiles() to allow the 
            application to serve a default file (e.g.: index.html) when a 
            directory is requested.
         
         2. Enable Static Files: 
            Call app.UseStaticFiles() to enable serving static files from the web
            root directory.

         3. Map Static Assets: 
            Call app.MapStaticAssets() to leverage the performance benefits of 
            optimized static asset delivery for assets known at build/publish 
            time. 

         The following line can be commented out and static files will be served 
         just fine.  At the time of this writing, still waiting on more documentation 
         with regard to how to properly use MapStaticAssets, see:

         https://github.com/dotnet/AspNetCore.Docs/issues/36203
         */

         //app.MapStaticAssets();

      }


      /*
      --------------------------------------------------

      Configure services.

      Use this method to add services to the container.  

      A service is a reusable component that provides app 
      functionality. Services are registered in this method
      and consumed across the app via Dependency Injection (DI)
      or ApplicationServices.

      For example, a logging component is a service. 

      --------------------------------------------------
      */
      private static void services_configure( WebApplicationBuilder builder )
      {

         // Add routemap pages service ...

         builder.Services.add_rmp();


         // Add other services as required ...

      }

   }

}
