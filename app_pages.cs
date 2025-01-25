using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using rmp;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System;

namespace mywebsite
{

   /*
   ==================================================

    Illustrates using a instance class to render 
    website pages.

    Note the [routemap] attributes above those methods
    used to render pages.  See rpm.cs for details.

   ==================================================
   */
   public class app_pages
   {

      private Int32 m_product_page_render_count;


      /*
      --------------------------------------------------

      Constructor.

      --------------------------------------------------
      */
      public app_pages()
      {

         m_product_page_render_count = 0;

      }


      /*
      --------------------------------------------------

      Given a StringBuilder, add HTML code for the start
      of a page ...

      --------------------------------------------------
      */
      public void page_start( StringBuilder sb, String title = "Title", Boolean link_to_home = true )
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
      public void page_end( StringBuilder sb )
      {

         sb.AppendLine( $@"<p>{System.DateTime.UtcNow:F}</p>" );

         sb.AppendLine( @"</body></html>" );

      }


      /*
      --------------------------------------------------

      Render the home page ...

      --------------------------------------------------
      */
      [routemap( "/" )]
      public async Task home_page( HttpContext http_context )
      {

         StringBuilder sb;


         sb = new StringBuilder();


         // Build response ...

         page_start( sb, "Home", false );

         sb.AppendLine(
@"<ul>
<li><a href=""/product/22"">/product/22</a></li>
<li><a href=""/routemaps/"">/routemaps</a></li>
<li><a href=""/static1/"">/static1/</a></li>
<li><a href=""/static2/44"">/static2/44</a></li>
</ul>"
         );

         page_end( sb );
         
         
         // Send response ...

         http_context.Response.ContentType = "text/html";

         await http_context.Response.WriteAsync( sb.ToString() );

         return;

      }


      /*
      --------------------------------------------------

      Render a product page for a given product ID ...

      --------------------------------------------------
      */
      [routemap( "/product/{id:int}" )]
      public async Task product_page( HttpContext http_context )
      {

         const String k_product_id = "id";

         Int32 product_id;

         StringBuilder sb;

         RouteData route_data;


         sb = new StringBuilder();


         // Build response ...

         page_start( sb, "Product Page" );


         // Show number of times this page has been rendered ...

         m_product_page_render_count++;

         sb.Append( $@"<p>Render count: {m_product_page_render_count}" );


         // Get product ID ...

         route_data = http_context.GetRouteData();

         if ( route_data is not null )
         {

            // Get an individual data value ...

            if ( route_data.Values.ContainsKey( k_product_id ) )
            {

               product_id = Int32.Parse( ( String )route_data.Values[ k_product_id ] );

               // Build response ...

               sb.AppendLine( $@"<p>Product ID: {product_id}</p>" );

            }

         }

         page_end( sb );


         // Send response ...

         http_context.Response.ContentType = "text/html";

         await http_context.Response.WriteAsync( sb.ToString() );

         return;
      
      }


      /*
      --------------------------------------------------

      Render a page that shows endpoint information, and
      all defined routemaps ...

      --------------------------------------------------
      */
      [routemap( "/routemaps" )]
      public async Task routemaps_page( HttpContext http_context )
      {

         String s;

         StringBuilder sb;


         sb = new StringBuilder();

         
         // Build response ...

         page_start( sb, "Routemaps" );


         // Show endpoint info ...

         var endpoint = http_context.GetEndpoint();

         if ( endpoint is not null )
         {
 
            sb.AppendLine( $@"<b>This routemap endpoint properties:</b>
                           <ul>
                           <li>DisplayName: {endpoint.DisplayName}</li>
                           <li>RequestDelgate: {endpoint.RequestDelegate.Method.DeclaringType.FullName}.{endpoint.RequestDelegate.Method.Name}</li>
                           </ul>"
                         );

         }


         // Show all defined routemaps ...

         var routemap_endpoints = ( routemap_endpoints )http_context.RequestServices.GetService( typeof( routemap_endpoints ) );

         sb.AppendLine( $@"<b>Number of [routemap] endpoints defined: {routemap_endpoints.routemaps.Count}</b><ol>" );

         foreach ( KeyValuePair<String, routemap_data> kvp in routemap_endpoints.routemaps )
         {

            s = String.Join( ", ", kvp.Value.routemap_attribute.allowed_http_method_list );

            sb.AppendLine( $@"<li>Routemap pattern: {kvp.Value.routemap_attribute.route_pattern}
                           <ul>
                           <li>Allowed HTTP methods: {s}</li>
                           <li>Order: {kvp.Value.routemap_attribute.order}</li>
                           <li>Mapped to method: {kvp.Value.method_name}</li>
                           <li>Source file: {kvp.Value.routemap_attribute.file_path}, line: {kvp.Value.routemap_attribute.line_number}</li>
                           </ul>
                           <br></li>"
                         );

         }

         sb.AppendLine( "</ol>" );

         page_end( sb );


         // DEBUGGING - Log all routemaps to a text file
         // Uncomment and change path below as needed ...
         /*
         String routemap_file = @"D:\temp\routmap_pages.txt";

         try
         {

            routemap_endpoints.log_to_file( routemap_file );

            sb.AppendLine( $@"<b>Routemaps written to file</b>: {routemap_file}</p>" );

         }
         catch ( Exception ex )
         {

            sb.AppendLine( $@"<p>{ex}</p>" );

         }
         */


         // Send response ...

         http_context.Response.ContentType = "text/html";

         await http_context.Response.WriteAsync( sb.ToString() );

         return;

      }

   }

}
