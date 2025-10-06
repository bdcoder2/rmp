using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using rmp;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace mywebsite
{
   public class Program
   {

      private static Microsoft.AspNetCore.Builder.WebApplication web_app;

      /*
      ---------------------------------------------------------------------

      Create and run the web host ...

      ---------------------------------------------------------------------
      */
      public static async Task Main( String[] args )
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
         {

            await web_app.RunAsync();

         }

      }


      /*
      ---------------------------------------------------------------------

      Create and configure a web application / host ...

      ---------------------------------------------------------------------
      */
      private static Microsoft.AspNetCore.Builder.WebApplication web_app_create( String[] args )
      {

         Microsoft.AspNetCore.Builder.WebApplicationOptions web_app_options;

         Microsoft.AspNetCore.Builder.WebApplicationBuilder web_app_builder;

         Microsoft.AspNetCore.Builder.WebApplication web_app;


         // Set web app options and create web app builder ...

         web_app_options = new WebApplicationOptions()
         {

            Args = args,

            // ContentRootPath = Set content root path (contains the application content files; app settings, dlls, etc),

            // WebRootPath = Set web root path (contains the web-servable content files; static files such as html, css, javascript, etc.)

         };

         web_app_builder = WebApplication.CreateBuilder( web_app_options );



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
      ---------------------------------------------------------------------

      Configure middleware ...

      ---------------------------------------------------------------------
      */
      private static void middleware_configure( WebApplication app )
      {

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


         // Use routemap pages ...

         app.use_rmp();

      }


      /*
      ---------------------------------------------------------------------

      Configure services.

      Use this method to add services to the container.  

      A service is a reusable component that provides app 
      functionality. Services are registered in this method
      and consumed across the app via Dependency Injection (DI)
      or ApplicationServices.

      For example, a logging component is a service. 

      ---------------------------------------------------------------------
      */
      private static void services_configure( WebApplicationBuilder builder )
      {

         // Add routemap pages service ...

         builder.Services.add_rmp();


         // Add other services as required ...

      }

   }

}
