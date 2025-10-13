using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace rmp
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

      private Int32 m_user_id_form_handler_count;

      private Int32 m_product_page_render_count;


      /*
      --------------------------------------------------

      Constructor.

      --------------------------------------------------
      */
      public app_pages()
      {

         m_user_id_form_handler_count = 0;

         m_product_page_render_count = 0;

      }


      /*
      --------------------------------------------------

      Handler for the User ID form ...

      --------------------------------------------------
      */
      [routemap( "/app_setting_key_form_handler", routemap.http_methods.POST )]
      public async Task app_setting_key_form_handler( HttpContext http_context )
      {

         String key;
         String s;

         StringBuilder sb;

         Microsoft.AspNetCore.Http.IFormCollection form_data;


         ArgumentNullException.ThrowIfNull( http_context );

         sb = new StringBuilder();

         m_user_id_form_handler_count++;


         // Page start HTML ...

         app_util.html_page_start( sb, "Setting Key Form Handler" );

         sb.AppendLine( $@"<p>Form handler count: {m_user_id_form_handler_count}</p>" );

         // Get the form data / collection ...

         form_data = await http_util.form_data_get( http_context );

         if ( form_data is not null )
         {

            key = http_util.form_field_string( form_data, "settings_key" );

            if ( String.IsNullOrWhiteSpace( key ) )
            {

               sb.Append( "<p><b>A Setting Key is required</b></p>" );

               // Show form again ...

               app_util.html_app_settings_key_form( sb, key );

            }
            else
            {

               // Show the key entered ...

               sb.AppendLine( @"<p>Key: " + HttpUtility.HtmlEncode( key ) + "</p>" );

               if ( app_settings.config_key_exists( key ) )
               {

                  s = app_settings.config_value( key );

                  sb.AppendLine( @"<p>Value: " + HttpUtility.HtmlEncode( s ) + "</p>" );

               }
               else
               {

                  sb.AppendLine( @"<p>The key <b>DOES NOT exist</b> in settings</p>" );

               }

            }

         }


         // Add page end HTML ...

         app_util.html_page_end( sb );


         // Send HTML response ...

         await http_util.html_response_send( http_context, sb );

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

               product_id = int.Parse( ( String )route_data.Values[ k_product_id ] );

               // Build response ...

               sb.AppendLine( $@"<p>Product ID: {product_id}</p>" );

            }

         }


         // Page end HTML ...

         app_util.html_page_end( sb );


         // Send HTML response ...

         await http_util.html_response_send( http_context, sb );

         return;
      
      }


      /*
      --------------------------------------------------

      Render a page that shows the query strings passed
      to a page ...

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


         sb.AppendLine( "<p>QueryString Data:</p>" );

         querystring_data = http_context.Request.Query;

         if ( querystring_data.Count == 0 )
         {

            sb.AppendLine( "<b>No queryString data was present in this request</b>" );

         }
         else
         {

            foreach ( KeyValuePair<String, StringValues> querystring_param in querystring_data )
            {

               querystring_key = querystring_param.Key;

               querystring_values = querystring_param.Value;

               sb.Append( "<p>Key: " + HttpUtility.HtmlEncode( querystring_key ) + ", Value count: " + querystring_values.Count.ToString() );

               if ( querystring_values.Count > 0 )
               {

                  sb.AppendLine( "<br>Values:<ul>" );

                  foreach ( String qs_val in querystring_values )
                  {

                     sb.Append( "<li>" );

                     if ( String.IsNullOrEmpty( qs_val ) )
                     {
                        sb.AppendLine( "NullOrEmpty value" );
                     }
                     else
                     {
                        sb.AppendLine( HttpUtility.HtmlEncode( qs_val ) );
                     }

                     sb.Append( "</li>" );

                  }

                  sb.AppendLine( "</ul>" );

               }

               sb.AppendLine( "</p>" );

            }

         }


         // Page end HTML ...

         app_util.html_page_end( sb );


         // Send HTML response ...

         await http_util.html_response_send( http_context, sb );

         return;

      }


      /*
      --------------------------------------------------

      Render a page that shows the current endpoint 
      information and all defined [routemap] pages ...

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

            foreach ( KeyValuePair<string, routemap_data> kvp in rm_endpoints.routemaps )
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

         await http_util.html_response_send( http_context, sb );

         return;

      }



      /*
      --------------------------------------------------

      Render a page that shows the current application
      settings ...

      --------------------------------------------------
      */
      [routemap( "/settings" )]
      public async Task settings_page( HttpContext http_context )
      {

         StringBuilder sb;

         ArgumentNullException.ThrowIfNull( http_context );

         sb = new StringBuilder();


         // Page start HTML ...

         app_util.html_page_start( sb, "Settings" );


         sb.AppendLine( "<pre>" );

         sb.Append( app_settings.to_string() );

         sb.AppendLine( "</pre>" );


         // Page end HTML ...

         app_util.html_page_end( sb );


         // Send HTML response ...

         await http_util.html_response_send( http_context, sb );

         return;

      }

   }

}
