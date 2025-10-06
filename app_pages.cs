using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using rmp;

namespace mywebsite
{

   /*
   ==================================================

    Illustrates using a instance class to render 
    website pages.

    The HTML could also be built by using a text 
    templating engine such as:
    - https://github.com/scriban/scriban
    - https://github.com/sebastienros/fluid/

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

      Handler for the User ID form ...

      --------------------------------------------------
      */
      [routemap( "/user_id_form_handler", routemap.http_methods.POST )]
      public async Task user_id_form_handler( HttpContext http_context )
      {

         String s;
         String user_id;

         StringBuilder sb;

         Microsoft.AspNetCore.Http.IFormCollection form_data;


         ArgumentNullException.ThrowIfNull( http_context );

         sb = new StringBuilder();

         
         // Page start HTML ...

         app_util.html_page_start( sb, "User ID Form Handler" );


         // Get the form data / collection ...

         form_data = await app_util.form_data_get( http_context );

         if ( form_data is not null )
         {

            // Get the user ID field from the form ...

            user_id = app_util.form_field_string( form_data, "userid" );

            if ( String.IsNullOrWhiteSpace( user_id ) )
            {

               sb.Append( "<p><b>A User ID is required</b></p>" );

               // Show User ID form again ...

               app_util.html_user_id_form( sb );

            }
            else
            {

               // Show the user ID entered ...

               s = HttpUtility.HtmlEncode( user_id );

               sb.AppendLine( $@"<p>User ID entered: {s}</p>" );

            }

         }


         // Add page end HTML ...

         app_util.html_page_end( sb );


         // Send HTML response ...

         await app_util.html_send_response( http_context, sb );

         return;

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


         ArgumentNullException.ThrowIfNull( http_context );

         sb = new StringBuilder();


         // Page start HTML ...

         app_util.html_page_start( sb, "Home", false );


         // Add links to the page ...

         sb.AppendLine(
@"<ul>
<li><a href=""/api/double/100"">/api/double/100<a/></li>
<li><a href=""/product/22"">/product/22</a></li>
<li><a href=""/querystrings?a=1&b=2&b=3&c=4&c=5&c=6&d=dot.net"">/querystrings?a=1&b=2&b=3&c=4&c=5&c=6&d=dot.net</a></li>
<li><a href=""/routemaps/"">/routemaps</a></li>
<li><a href=""/static1/"">/static1/</a></li>
<li><a href=""/static2/44"">/static2/44</a></li>
</ul>"
         );


         // Add a User ID form ...

         app_util.html_user_id_form( sb, "Test" );


         // Page end HTML ...

         app_util.html_page_end( sb );


         // Send HTML response ...

         await app_util.html_send_response( http_context, sb );

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


         ArgumentNullException.ThrowIfNull( http_context );

         sb = new StringBuilder();


         // Page start HTML ...

         app_util.html_page_start( sb, "Product Page" );


         // Add number of times this page has been rendered ...

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


         // Page end HTML ...

         app_util.html_page_end( sb );


         // Send HTML response ...

         await app_util.html_send_response( http_context, sb );

         return;
      
      }

      /*
      --------------------------------------------------

      Render a page that shows the current endpoint 
      information and all defined routemaps ...

      --------------------------------------------------
      */
      [routemap( "/querystrings" )]
      public async Task querystrings_page( HttpContext http_context )
      {

         String querystring_key;

         StringValues querystring_values;

         StringBuilder sb;

         Microsoft.AspNetCore.Http.IQueryCollection querystring_data;


         ArgumentNullException.ThrowIfNull( http_context );

         sb = new StringBuilder();


         // Page start HTML ...

         app_util.html_page_start( sb, "Querystrings Page" );


         sb.AppendLine( "<p>Querystring Data:</p>" );

         querystring_data = http_context.Request.Query;

         if ( querystring_data.Count == 0 )
         {

            sb.AppendLine( "<b>No querystring data was present in this request</b>" );

         }
         else
         {

            foreach ( KeyValuePair<String, StringValues> querystring_param in querystring_data )
            {

               querystring_key = querystring_param.Key;

               querystring_values = querystring_param.Value;

               sb.Append( "<p>Key: " + HttpUtility.HtmlEncode( querystring_key ) );

               if ( querystring_values.Count > 0 )
               {

                  sb.AppendLine( "<br>Values:<ul>" );

                  foreach ( String qs_val in querystring_values )
                  {
                     sb.AppendLine( "<li>" + HttpUtility.HtmlEncode( qs_val ) + "</li>" );
                  }

                  sb.AppendLine( "</ul>" );

               }

               sb.AppendLine( "</p>" );

            }

         }


         // Page end HTML ...

         app_util.html_page_end( sb );


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


         // Send HTML response ...

         await app_util.html_send_response( http_context, sb );

         return;

      }


      /*
      --------------------------------------------------

      Render a page that shows the current endpoint 
      information and all defined routemaps ...

      --------------------------------------------------
      */
      [routemap( "/routemaps" )]
      public async Task routemaps_page( HttpContext http_context )
      {

         String s;

         StringBuilder sb;

         Microsoft.AspNetCore.Http.Endpoint endpoint;

         routemap_endpoints rm_endpoints;


         ArgumentNullException.ThrowIfNull( http_context );

         sb = new StringBuilder();

         
         // Page start HTML ...

         app_util.html_page_start( sb, "Routemaps" );


         // Add endpoint info ...

         endpoint = http_context.GetEndpoint();

         if ( endpoint is not null )
         {
 
            sb.AppendLine( $@"<b>This routemap endpoint properties:</b>
                           <ul>
                           <li>DisplayName: {endpoint.DisplayName}</li>
                           <li>RequestDelgate: {endpoint.RequestDelegate.Method.DeclaringType.FullName}.{endpoint.RequestDelegate.Method.Name}</li>
                           </ul>"
                         );

         }


         // Add all defined routemaps ...

         rm_endpoints = http_context.RequestServices.GetService( typeof( routemap_endpoints ) ) as routemap_endpoints;

         if ( rm_endpoints is null )
         {

            sb.AppendLine( $@"<b>No [routemap] endpoints are defined.</b>" );

         }
         else
         {

            sb.AppendLine( $@"<b>Number of [routemap] endpoints defined: {rm_endpoints.routemaps.Count}</b><ol>" );

            foreach ( KeyValuePair<String, routemap_data> kvp in rm_endpoints.routemaps )
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

         }

         
         // Page end HTML ...

         app_util.html_page_end( sb );


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


         // Send HTML response ...

         await app_util.html_send_response( http_context, sb );

         return;

      }

   }

}
