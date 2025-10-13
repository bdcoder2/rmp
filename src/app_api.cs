using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using rmp;

namespace rmp
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

      private static Int32 m_api_invoked_count;


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
      [routemap( "/api/double/{val:int?}", routemap.http_methods.GET )]
      public static async Task api_double_handler( HttpContext http_context )
      {

         Int32 err_code;
         Int32 input_val;
         Int32 output_val;
         Int32 status_code;

         String err_msg;
         String route_val;

         Microsoft.AspNetCore.Http.IResult result;


         ArgumentNullException.ThrowIfNull( http_context );


         // Keep count of how many times we have been invoked ...

         m_api_invoked_count++;


         // Get the required input route value and convert to an integer (just to show how) ...

         route_val = http_context.GetRouteValue( "val" ) as string;

         if ( !Int32.TryParse( route_val, out input_val) )
         {

            result = Results.BadRequest( "Invalid route parameter in URL, i.e.: /api/double/{val}, supply a integer value, eg: /api/double/10" );

            await result.ExecuteAsync( http_context );

            return;

         }


         // Double the input value ...

         status_code = ( Int32 )HttpStatusCode.OK;

         output_val = 0;

         err_code = 0;

         err_msg = null;

         try
         {

            output_val = checked( input_val * 2 );

         }
         catch ( Exception ex )
         {

            err_code = ex.HResult;

            err_msg = ex.Message;

            status_code = ( Int32 )HttpStatusCode.BadRequest;

         }


         // Create a dictionary that holds the results ...

         var dict = new Dictionary<String, Object>
            {
               { "api_invoked_count", m_api_invoked_count },
               { "input", input_val },
               { "output", output_val },
               { "utc", DateTime.UtcNow.ToString( "O" ) },
               { "err_code", err_code },
               { "err_msg", err_msg }
            };


         // Return results as JSON ...

         result = Results.Json( data: dict,
                                contentType: "application/json",
                                statusCode: status_code
                              );

         await result.ExecuteAsync( http_context );


         // Can also return JSON this way ...

         // http_context.Response.StatusCode = ( Int32 )HttpStatusCode.OK;

         // await http_context.Response.WriteAsJsonAsync( dict );

         return;

      }

   }

}
