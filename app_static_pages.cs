using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using rmp;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System;

namespace mywebsite
{

   /*
   ==================================================

    Illustrates using a static class to render website
    pages.

    Note the [routemap] attributes above those methods
    used to render pages.  See rpm.cs for details.

   ==================================================
   */
   public static class app_static_pages
   {

      private static int m_static1_render_count;

      private static int m_static2_render_count;


      /*
      --------------------------------------------------

      Constructor ...

      --------------------------------------------------
      */
      static app_static_pages()
      {

         m_static1_render_count = 0;

         m_static2_render_count = 0;

      
      }


      /*
      --------------------------------------------------

      Given a StringBuilder, add HTML code for the start
      of a page ...

      --------------------------------------------------
      */
      public static void page_start( StringBuilder sb, String title = "Title", Boolean link_to_home = true )
      {

         String s;

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

      Given a StringBuilder, add HTML code for the end
      of a page ...

      --------------------------------------------------
      */
      public static void page_end( StringBuilder sb )
      {

         sb.AppendLine( $@"<p>{System.DateTime.UtcNow:F}</p>" );

         sb.AppendLine( @"</body></html>" );

      }


      /*
      --------------------------------------------------

      Render page ...

      --------------------------------------------------
      */
      [routemap( "/static1" )]
      public static async Task static1_render( HttpContext http_context )
      {

         StringBuilder sb;


         sb = new StringBuilder();

         // Build response ...

         page_start( sb, "Static1 Page" );

         m_static1_render_count++;

         sb.AppendLine( $@"<p>Render count: {m_static1_render_count}</p>" );

         page_end( sb );


         // Send response ...

         http_context.Response.ContentType = "text/html";

         await http_context.Response.WriteAsync( sb.ToString() );

         return;

      }


      /*
      --------------------------------------------------

      Render page with a required ID route parameter ...

      --------------------------------------------------
      */
      [routemap( "/static2/{id:int}" )]
      public static async Task static2_render( HttpContext http_context )
      {

         const String k_id = "id";

         int id;

         StringBuilder sb;


         sb = new StringBuilder();

         page_start( sb, "Static2 Page" );

         m_static2_render_count++;

         sb.AppendLine( $@"<p>Render count: {m_static2_render_count}</p>" );

         // Get ID ...

         var route_data = http_context.GetRouteData();

         if ( route_data is not null )
         {

            // Get an individual data value ...

            if ( route_data.Values.ContainsKey( k_id ) )
            {

               id = Int32.Parse( (String)route_data.Values[ k_id ] );

               // Build response ...

               sb.AppendLine( $@"<p> ID: {id}</p>" );

            }

         }

         page_end( sb );


         // Send response ...

         http_context.Response.ContentType = "text/html";

         await http_context.Response.WriteAsync( sb.ToString() );

         return;
      
      }

   }

}
