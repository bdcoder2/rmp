using System;
using System.Net;
using System.Runtime.CompilerServices;
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

      Given a StringBuilder, add HTML code for the end
      of a page ...

      --------------------------------------------------
      */
      public static void html_page_end( StringBuilder sb )
      {

         ArgumentNullException.ThrowIfNull( sb );

         sb.AppendLine( $@"<p>{DateTime.UtcNow:F}</p>" );

         sb.AppendLine( @"</body></html>" );

      }


      /*
      --------------------------------------------------

      Given a StringBuilder, add HTML code for the start
      of a page ...

      --------------------------------------------------
      */
      public static void html_page_start( StringBuilder sb, String title = "Title", bool link_to_home = true )
      {

         String page_title;


         ArgumentNullException.ThrowIfNull( sb );

         if ( String.IsNullOrWhiteSpace( title ) )
         {

            page_title = "Page Title";

         }
         else
         {

            page_title = HttpUtility.HtmlEncode( title );

         }


         sb.AppendLine( $@"<html><head><title>" + page_title + "</title></head><body>" );

         sb.AppendLine( @"<h1>" );

         if ( link_to_home )

            sb.AppendLine( @"<a href=""/"">Home</a> : " );

         sb.Append( page_title );

         sb.AppendLine( "</h1>" );

      }


      /*
      --------------------------------------------------

      Given a StringBuilder, add the HTML code for an
      app setting key form ...

      --------------------------------------------------
      */
      public static void html_app_settings_key_form( StringBuilder sb, String key_default = "" )
      {

         ArgumentNullException.ThrowIfNull( sb );


         String key = HttpUtility.HtmlEncode( key_default );

         sb.AppendLine(
@$"<form action=""/app_setting_key_form_handler"" method=""POST"">
      <label for=""settings_key"">Setting Key:</label>
      <input type=""text"" value=""{key}"" size=""40"" id=""settings_key"" name=""settings_key"" placeholder=""Settings Key""> (required)
      <p><input type=""submit"" value=""Submit""></p>
   </form>"
         );

      }

   }

}