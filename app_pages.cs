using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using rmp;

/*
==================================================

 This class contains the methods used to render
 various website pages.

 Note the [routemap] attributes above those methods
 used to render pages.  See rpm.cs for details.

==================================================
*/

namespace mywebsite
{
   public class app_pages
   {

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

         sb.AppendLine( @"<b>Home</b>
                        <ul>
                        <li><a href=""/product/22"">/product/22</a></li>
                        <li><a href=""/routemaps/"">/routemaps</a></li>
                        </ul>"
                      );

         // Send response ...

         http_context.Response.ContentType = "text/html";

         await http_context.Response.WriteAsync( sb.ToString() );

         return;

      }


      /*
      --------------------------------------------------

      Rentder a product page for a given product ID ...

      --------------------------------------------------
      */
      [routemap( "/product/{id:int}" )]
      public async Task product_page( HttpContext http_context )
      {

         const String k_product_id = "id";

         int product_id;

         StringBuilder sb;


         sb = new StringBuilder();

         sb.AppendLine( "<b>Product Page</b>" );


         // Get product ID ...

         var route_data = http_context.GetRouteData();

         if ( route_data is not null )
         {

            // Get an individual data value ...

            if ( route_data.Values.ContainsKey( k_product_id ) )
            {

               product_id = Int32.Parse( (String)route_data.Values[ k_product_id ] );

               // Build response ...

               sb.AppendLine( $@"<ul><li>Product ID: {product_id}</li></ul>" );

            }

         }

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

            sb.AppendLine( $@"<li>Routemap pattern: {kvp.Key}
                           <ul>
                           <li>Allowed HTTP methods: {s}</li>
                           <li>Mapped to method: {kvp.Value.method_name}</li>
                           <li>Source file: {kvp.Value.routemap_attribute.file_path}, line: {kvp.Value.routemap_attribute.line_number}</li>
                           </ul>
                           <br></li>"
                         );

         }

         sb.AppendLine( "</ol>" );


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
