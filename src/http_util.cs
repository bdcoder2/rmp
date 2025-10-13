using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace rmp
{
   /*
   ==================================================

   HTTP utilities - various helper methods ...

   ==================================================
   */
   public static class http_util
   {

      /*
      --------------------------------------------------

      Constructor ...

      --------------------------------------------------
      */
      static http_util()
      {
      }


      /*
      --------------------------------------------------

      Get form data.

      ARGUMENTS:

         http_context

            An HttpContext object.

      RETURNS:

         An IFormCollection containing the parsed form
         values sent with the HTTP request or null if
         no form context exists.

      --------------------------------------------------
      */
      public static async Task<Microsoft.AspNetCore.Http.IFormCollection> form_data_get( Microsoft.AspNetCore.Http.HttpContext http_context )
      {

         if ( http_context is not null )
         {

            /*
            Check if any Form content type exists in the request, if not return null.
            NOTE: We MUST check if the request has any form content BEFORE calling the
            ReadFormAsync() method.  The ReadFormAsync() method will throw an exception 
            if no form content exists.  We avoid the exception by checking first ...
            */

            if ( http_context.Request.HasFormContentType )
            {

               /*
               Return the form data/collection ...
               Use HttpContext.Request.ReadFormAsync instead of HttpContext.Request.Form
               See: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices#prefer-readformasync-over-requestform
               */

               Microsoft.AspNetCore.Http.IFormCollection form_data = await http_context.Request.ReadFormAsync();

               return form_data;

            }

         }
         
         return null;

      }


      /*
      --------------------------------------------------

      Check if a form field exists.

      ARGUMENTS:

         form_data

            An IFormCollection interface.

         key

            A string containing the form field key (name)
            to check.

      RETURNS:

         True if the given key exists in the form_data
         collection, false otherwise.

      --------------------------------------------------
      */
      public static bool form_field_exists( Microsoft.AspNetCore.Http.IFormCollection form_data, String key )
      {

         // A form collection and key are required ...

         if ( form_data is null || String.IsNullOrWhiteSpace( key ) )

            return false;


         return form_data.ContainsKey( key );

      }


      /*
      --------------------------------------------------

      Get a form field value as a String.

      ARGUMENTS:

         form_data

            An IFormCollection interface.

         key

            A string containing the form key
            (field name).

      RETURNS:

         A String that is the value for the given
         key or null if the key does not exist in 
         the form collection.

      NOTE:
      
         Only the first value is returned if the
         key has multiple values.

      --------------------------------------------------
      */
      public static String form_field_string( Microsoft.AspNetCore.Http.IFormCollection form_data, String key )
      {

         Microsoft.Extensions.Primitives.StringValues field_values;

         
         // Get field values (if any) ...

         field_values = form_field_values( form_data, key );


         // If no field values exist, return null ...

         if ( StringValues.IsNullOrEmpty( field_values ) )

            return null;


         // Return the first field value ...

         return field_values[ 0 ];

      }


      /*
      --------------------------------------------------

      Get a form key (field name) value(s).

      ARGUMENTS:

         form_data

            An IFormCollection interface.

         key

            A string containing the form key (field name).

      RETURNS:

         A StringValues structure that contains the
         field values if the key exists in the form
         collection.
      
         A StringValues.Empty structure in all other 
         cases.

      NOTES:

         - Before calling this routine, you should call
           the form_field_exists() method to ensure the
           key actually exists in the form collection.

         - To check if a StringValues instance in .NET 
           Core is empty or contains no values, the 
           recommended and most robust method is to use 
           the static StringValues.IsNullOrEmpty method.

      --------------------------------------------------
      */
      public static Microsoft.Extensions.Primitives.StringValues form_field_values( Microsoft.AspNetCore.Http.IFormCollection form_data, String key )
      {

         // A form collection and field name (key) are required ...

         if ( form_data is not null && !String.IsNullOrWhiteSpace( key ) )
         {

            // Try to get the form field value(s) ...

            if ( form_data.TryGetValue( key, out Microsoft.Extensions.Primitives.StringValues field_values ) )

               return field_values;

         }


         return StringValues.Empty;

      }


      /*
      --------------------------------------------------

      Send an HTML response with a 200 status code and
      content type set to "text/html".

      ARGUMENTS:

         http_context

            An HttpContext object.

         sb

            A StringBuilder object.

      RETURNS:

         None.

      --------------------------------------------------
      */
      public static async Task html_response_send( Microsoft.AspNetCore.Http.HttpContext http_context, StringBuilder sb )
      {

         // HTTP context and StringBuilder are required ...

         if ( http_context is null || sb is null )

            return;


         // Send response ...

         http_context.Response.StatusCode = ( Int32 )HttpStatusCode.OK;

         http_context.Response.ContentType = "text/html";

         await http_context.Response.WriteAsync( sb.ToString() );

         return;

      }


      /*
      --------------------------------------------------

      Check if a querystring field exists.

      ARGUMENTS:

         querystring_data

            An IQueryCollection interface.

         key

            A string containing the querystring key 
            (field name) to check.

      RETURNS:

         True if the given key exists in the querystring
         collection, false otherwise.

      --------------------------------------------------
      */
      public static bool querystring_field_exists( Microsoft.AspNetCore.Http.IQueryCollection querystring_data, String key )
      {

         // A queryString collection and key (field name) are required ...

         if ( querystring_data is null || String.IsNullOrWhiteSpace( key ) )

            return false;
         

         return querystring_data.ContainsKey( key );

      }


      /*
      --------------------------------------------------

      Get a querystring field value as a String.

      ARGUMENTS:

         querystring_data

            An IQueryCollection interface.

         key

            A string containing the querystring key
            (field key).

      RETURNS:

         A String that is the value for the given key
         or null if the key does not exist in the 
         querystring collection.

      NOTE:
      
         Only the first value is returned if the key
         has multiple values.

      --------------------------------------------------
      */
      public static String querystring_field_string( Microsoft.AspNetCore.Http.IQueryCollection querystring_data, String key )
      {

         Microsoft.Extensions.Primitives.StringValues field_values;


         // Get field values ...

         field_values = querystring_field_values( querystring_data, key );


         // If no field values exist, return null ...

         if ( StringValues.IsNullOrEmpty( field_values ) )

            return null;


         // Return the first field value ...

         return field_values[ 0 ];

      }


      /*
      --------------------------------------------------

      Get a querystring key (field name) value(s).

      ARGUMENTS:

         querystring_data

            An IQueryCollection interface.

         key

            A string containing the querystring key
            (field name).

      RETURNS:

         A StringValues structure that contains the
         field values if the key exists in the 
         querystring collection.
      
         A StringValues.Empty structure in all other 
         cases.

      NOTES:

         - Before calling this routine, you should call
           the querystring_field_exists() method to ensure
           the key actually exists in the querystring
           collection.

         - To check if a StringValues instance in .NET 
           Core is empty or contains no values, the 
           recommended and most robust method is to use 
           the static StringValues.IsNullOrEmpty method.

      --------------------------------------------------
      */
      public static Microsoft.Extensions.Primitives.StringValues querystring_field_values( Microsoft.AspNetCore.Http.IQueryCollection querystring_data, String key )
      {

         // A querystring collection and field name (key) are required ...

         if ( querystring_data is not null && !String.IsNullOrWhiteSpace( key ) )
         {

            // Try to get the querystring field value(s) ...

            if ( querystring_data.TryGetValue( key, out Microsoft.Extensions.Primitives.StringValues field_values ) )

               return field_values;

         }


         return StringValues.Empty;

      }

   }

}
