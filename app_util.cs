using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace rmp
{
   /*
   ==================================================

    App utilities - various helper methods ...

   ==================================================
   */
   public static class app_util
   {

      /*
      --------------------------------------------------

      Constructor ...

      --------------------------------------------------
      */
      static app_util()
      {
      }


      /*
      --------------------------------------------------

      Given an HttpContext, return the form collection
      for the current request, or null if none.

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

      Given an IFormCollection and a key, return the
      true if the key exists in the collection, false
      otherwise.

      --------------------------------------------------
      */
      public static Boolean form_field_exists( Microsoft.AspNetCore.Http.IFormCollection form_data, String key )
      {

         // A form collection and key are required ...

         if ( form_data is null || String.IsNullOrWhiteSpace( key ) )

            return false;


         return form_data.ContainsKey( key );

      }


      /*
      --------------------------------------------------

      Given an IFormCollection and a key, return the
      value for the key as a String or null if the
      key does not exist in the collection.

      Note: Only the first value is returned if the key 
            has multiple values.

      --------------------------------------------------
      */
      public static String form_field_string( Microsoft.AspNetCore.Http.IFormCollection form_data, String key )
      {

         // A form collection and form field name are required ...

         if ( form_data is null || String.IsNullOrWhiteSpace( key ) )

            return null;


         // Try to get the form field value(s) ...

         if ( form_data.TryGetValue( key, out Microsoft.Extensions.Primitives.StringValues field_values ) )
         {

            // If field value(s) exist, return the first ...

            if ( field_values.Count > 0 )

               return field_values[ 0 ];

         }


         return null;

      }


      /*
      --------------------------------------------------

      Given a StringBuilder, add HTML code for the end
      of a page ...

      --------------------------------------------------
      */
      public static void html_page_end( StringBuilder sb )
      {

         ArgumentNullException.ThrowIfNull( sb );

         sb.AppendLine( $@"<p>{System.DateTime.UtcNow:F}</p>" );

         sb.AppendLine( @"</body></html>" );

      }


      /*
      --------------------------------------------------

      Given a StringBuilder, add HTML code for the start
      of a page ...

      --------------------------------------------------
      */
      public static void html_page_start( StringBuilder sb, String title = "Title", Boolean link_to_home = true )
      {

         String s;

         ArgumentNullException.ThrowIfNull( sb );

         s = HttpUtility.HtmlEncode( title );

         sb.AppendLine( $@"<html><head><title>{s}</title></head><body>" );

         sb.AppendLine( @"<h1>" );

         if ( link_to_home )
         {

            sb.AppendLine( @"<a href=""/"">Home</a> : " );

         }

         sb.Append( s );

         sb.AppendLine( "</h1>" );

      }


      /*
      --------------------------------------------------

      Given an HttpContext and StringBuilder write the
      HTML response.  Sets the status code and content
      type as well ...

      --------------------------------------------------
      */
      public static async Task html_send_response( HttpContext http_context, StringBuilder sb )
      {

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

      Given a StringBuilder, add the HTML code for a
      User id form ...

      --------------------------------------------------
      */
      public static void html_user_id_form( StringBuilder sb, String user_id_default = "" )
      {

         ArgumentNullException.ThrowIfNull( sb );

         String user_id = System.Web.HttpUtility.HtmlEncode( user_id_default );

         sb.AppendLine(
@$"<form action=""/user_id_form_handler"" method=""POST"">
  <label for=""userid"">User ID:</label>
  <input type=""text"" value=""{user_id}"" id=""userid"" name=""userid"" placeholder=""Enter your User ID"">
  <p><input type=""submit"" value=""Submit""></p>
</form>"
         );

      }


      /*
      --------------------------------------------------

      Given an IQueryCollection and a key, return the
      true if the key exists in the collection, false
      otherwise.

      --------------------------------------------------
      */
      public static Boolean querystring_field_exists( Microsoft.AspNetCore.Http.IQueryCollection querystring_data, String key )
      {

         // A querystring collection and field name are required ...

         if ( querystring_data is null || String.IsNullOrWhiteSpace( key ) )
            return false;
         
         return querystring_data.ContainsKey( key );

      }


      /*
      --------------------------------------------------

      Given an IQueryCollection and a key, return the
      value for the key as a String or null if the
      key does not exist in the collection.

      Note: Only the first value is returned if the key 
            has multiple values.

      --------------------------------------------------
      */
      public static String querystring_field_string( Microsoft.AspNetCore.Http.IQueryCollection querystring_data, String key )
      {

         // A querystring collection and field name are required ...

         if ( querystring_data is null || String.IsNullOrWhiteSpace( key ) )

            return null;


         // Try to get the querystring field value(s) ...

         if ( querystring_data.TryGetValue( key, out Microsoft.Extensions.Primitives.StringValues field_values ) )
         {

            // If field value(s) exist, return the first ...

            if ( field_values.Count > 0 )

               return field_values[ 0 ];

         }

         return null;

      }

   }

}
