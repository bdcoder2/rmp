using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using rmp;

namespace mywebsite
{
   /*
   ==================================================

    Illustrates using a static class to handle API
    requests as well as using the Results class to
    build and return responses.

    Note the [routemap] attributes above methods in
    this class to handle requests. See rpm.cs for 
    details.

   ==================================================
   */
   public static class app_api
   {

      private static int m_api_invoked_count;


      /*
      --------------------------------------------------

      Constructor ...

      --------------------------------------------------
      */
      static app_api()
      {

         m_api_invoked_count = 0;

      }


      /*
      --------------------------------------------------

      API handler to double a value ...

      --------------------------------------------------
      */
      [routemap( "/api/double/{val:int?}" )]
      public static async Task api_echo_handler( HttpContext http_context )
      {

         Int32 n;

         String route_val;

         Microsoft.AspNetCore.Http.IResult result;


         ArgumentNullException.ThrowIfNull( http_context );


         // Keep count of how many times we have been invoked ...

         m_api_invoked_count++;


         // Get the required input route value and convert to an integer (just to show how) ...

         route_val = http_context.GetRouteValue( "val" ) as String;

         if ( !Int32.TryParse( route_val, out n ) )
         {

            result = Results.BadRequest( "Invalid route parameter in URL, i.e.: /api/double/{val}, supply a integer value, eg: /api/double/10" );

            await result.ExecuteAsync( http_context );

            return;

         }


         // Do whatever processing is required (DB lookups, etc.) ...



         // Create dictionary that holds results, again just to show how ...

         var dict = new Dictionary<String, Object>
            {
               { "api_invoked_count", m_api_invoked_count },
               { "input", n },
               { "output", n * 2 },
               { "utc", System.DateTime.UtcNow.ToString( "O" ) }
            };


         // Return results as JSON ...

         result = Results.Json( data: dict,
                                contentType: "application/json",
                                statusCode: ( Int32 )HttpStatusCode.OK
                              );

         await result.ExecuteAsync( http_context );


         // Can also return JSON this way ...

         // http_context.Response.StatusCode = ( Int32 )HttpStatusCode.OK;

         // await http_context.Response.WriteAsJsonAsync( dict );

         return;

      }

   }

}
