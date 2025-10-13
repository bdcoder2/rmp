using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System;
using System.Net;

namespace rmp
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

      private static Int32 m_static1_render_count;

      private static Int32 m_static2_render_count;


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

      Render page ...

      --------------------------------------------------
      */
      [routemap( "/static1" )]
      public static async Task static1_render( HttpContext http_context )
      {

         StringBuilder sb;


         ArgumentNullException.ThrowIfNull( http_context );

         sb = new StringBuilder();


         // Page start HTML ...

         app_util.html_page_start( sb, "Static1 Page" );


         // Show how many times this page has been rendered ...

         m_static1_render_count++;

         sb.AppendLine( $@"<p>Render count: {m_static1_render_count}</p>" );


         // Page end HTML ...

         app_util.html_page_end( sb );


         // Send HTML response ...

         await http_util.html_response_send( http_context, sb );

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

         Int32 id;

         StringBuilder sb;

         RouteData route_data;

         
         ArgumentNullException.ThrowIfNull( http_context );

         sb = new StringBuilder();

         
         // Page start HTML ...

         app_util.html_page_start( sb, "Static2 Page" );


         // Show how many times this page has been rendered ...

         m_static2_render_count++;

         sb.AppendLine( $@"<p>Render count: {m_static2_render_count}</p>" );


         // Get ID ...

         route_data = http_context.GetRouteData();

         if ( route_data is not null )
         {

            // Get an individual data value ...

            if ( route_data.Values.ContainsKey( k_id ) )
            {

               id = int.Parse( ( String )route_data.Values[ k_id ] );

               // Build response ...

               sb.AppendLine( $@"<p> ID: {id}</p>" );

            }

         }


         // Page end HTML ...

         app_util.html_page_end( sb );


         // Send HTML response ...

         await http_util.html_response_send( http_context, sb );

         return;
      
      }

   }

}
