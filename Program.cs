using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace mywebsite
{
   public class Program
   {

      /*
      --------------------------------------------------

      Create, build and run host ...

      --------------------------------------------------
      */
      public static void Main( string[] args )
      {

         CreateHostBuilder( args ).Build().Run();
      
      }


      /*
      --------------------------------------------------

      Configure host ...

      --------------------------------------------------
      */
      public static IHostBuilder CreateHostBuilder( string[] args ) =>

          Host.CreateDefaultBuilder( args )
              .ConfigureWebHostDefaults( webBuilder =>
               {
         
                  webBuilder.UseStartup<Startup>();
              
               } );
   }

}
