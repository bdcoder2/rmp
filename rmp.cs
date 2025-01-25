using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

/*
==================================================

 routemap pages (rmp)

 This namespace contains the classes that are used
 to implement website routemap pages.

 A [routemap] attribute is placed above any method
 in a static or instance class that can process an 
 HTTP request.

 USAGE

 1. Include the following using statement in any 
    file that defines a class that handles HTTP 
    requests, for example:

    using rmp;

    public class my_pages
    {
      
       private Int32 m_page1_render_count;
       private Int32 m_page2_render_count;

       public my_pages()
       {

          m_page1_render_count = 0;

          m_page2_render_count = 0;

       }


       [routemap( "/page1" ) ]
       private async Task page1_render( HttpContext http_context )
       {

          m_page1_render_count++;

          await http_context.Response.WriteAsync( $@"Page 1, render count: {m_page1_render_count}" );

          return;

       }

       [routemap( "/page2" ) ]
       private async Task page2_render( HttpContext http_context )
       {

          m_page2_render_count++;

          await http_context.Response.WriteAsync( $@"Page 2, render count: {m_page2_render_count}" );

          return;

       }

    }


 2. Add the routemap pages service and middleware, complete template below:

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using rmp;
    using System.Threading.Tasks;
    using System;

    namespace mywebsite
    {

       public class Program
       {

          private static Microsoft.AspNetCore.Builder.WebApplication web_app;

          public static async Task Main( String[] args )
          {

             try
             {

                web_app = web_app_create( args );

             }
             catch ( Exception ex )
             {

                web_app = null;

                Console.WriteLine( ex.ToString() );

             }

             if ( web_app is not null )
             {

                await web_app.RunAsync();

             }

          }

          private static Microsoft.AspNetCore.Builder.WebApplication web_app_create( String[] args )
          {

             Microsoft.AspNetCore.Builder.WebApplicationOptions web_app_options;

             Microsoft.AspNetCore.Builder.WebApplicationBuilder web_app_builder;

             Microsoft.AspNetCore.Builder.WebApplication web_app;


             // Set web app options and create web app builder ...

             web_app_options = new WebApplicationOptions()
             {

                Args = args,

             };

             web_app_builder = WebApplication.CreateBuilder( web_app_options );

             services_configure( web_app_builder );

             web_app = web_app_builder.Build();

             middleware_configure( web_app );


             return web_app;

          }

          private static void middleware_configure( WebApplication app )
          {

             app.UseRouting();

             app.use_rmp();

          }

          private static void services_configure( WebApplicationBuilder builder )
          {

             builder.Services.add_rmp();

          }

       }

    }


3. Build and launch the website.  The [routemap] patterns
   defined will be mapped to the appropriate handler, for 
   example:

    /page1 => will invoke the my_pages.page1_render method
    /page2 => will invoke the my_pages.page2_render method


 NOTES / REFERENCES

 - Routing in ASP.NET Core (specifically, Route template reference):
   https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-8.0

 - EndpointRouteBuilderExtensions.MapMethods Method:
   https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.endpointroutebuilderextensions.mapmethods?view=aspnetcore-8.0

 - Use Attributes in C# (specifically, How to create your own attribute):
   https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/attributes

 - Retrieving Information Stored in Attributes:
   https://docs.microsoft.com/en-us/dotnet/standard/attributes/retrieving-information-stored-in-attributes

 - How to: Hook Up a Delegate Using Reflection
   https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/how-to-hook-up-a-delegate-using-reflection

==================================================
*/

namespace rmp
{

   /*
   ==================================================
 
   The routemap (attribute) class is used to define
   the paths and HTTP methods that a RequestDelegate
   handles.

   ==================================================
   */
   [AttributeUsage( AttributeTargets.Method, AllowMultiple = true, Inherited = true )]
   public class routemap : Attribute
   {

      /*
      HTTP methods / verbs enumeration.  
      https://datatracker.ietf.org/doc/html/rfc7231#section-4.3

      Used to indicate the HTTP methods that a routemap
      supports.  Method verbs are additive, for example 
      to specify that GET and POST are supported use:

      routemap.http_methods.GET | routemap.http_methods.POST
      */

      // Public HTTP methods / verbs enumeration ...

      [System.Flags]
      public enum http_methods : Int32
      {

         GET = 1,
         HEAD = 2,
         POST = 4,
         PUT = 8,
         DELETE = 16,
         CONNECT = 32,
         OPTIONS = 64,
         TRACE = 128,
         PATCH = 256

      }


      // Private fields ...

      // The route pattern ...

      readonly private String m_route_pattern;

      // The HTTP method verbs this route will allow ...

      readonly private http_methods m_http_methods;

      // The route order.  Lower numeric value of order have higher priority ...

      readonly private Int32 m_order;

      // The source file path for this routemap ...

      readonly private String m_file_path;

      // The member name (handler) for this routemap ...

      readonly private String m_member_name;

      // The source file line number for this routemap ...

      readonly private int m_line_number;


      /*
      --------------------------------------------------

      Constructor with route pattern, HTTP methods, and
      order.

      PARAMETERS:

      route_pattern
      - The route template or pattern that provides a
        route to a route handler.

      http_methods
      - An enumeration of HTTP methods that this route
        will support.  By default, if none are specified, 
        then the GET and POST methods are supported.

      order
      - The order for this routemap.  May be any value
        greater than or equal to zero.  Default is zero.
        A lower value will have higher priority.


      EXAMPLE 1:

      The following "my_pages" class has a method called
      "page1_render".  To map the path "/page1/" to the 
      method, the following routemap attribute is included:

         public class my_pages
         {
            ...
            [routemap( "/page1" )]
            public static async Task page1_render( HttpContext context )
            { 

               await context.Response.WriteAsync( "Hello World!" );

            }

         }
         
      When a GET or POST request is made using the "/page1" path, the
      "page1_render" method will be invoked to return "Hello World!".

      
      EXAMPLE 2:
      
      The following "my_api" class has two methods with the same 
      routemap pattern, i.e.: /api/v1/test
      The "page1_render" method will be executed because the order
      is set to 1, which would take priority:

         public class my_api
         {
            ...
            [routemap( "/api/v1/test", order: 1 )]
            public static async Task page1_render( HttpContext context )
            { 

               // This routemap would take priority as order is lower

            }

            [routemap( "/api/v1/test", order: 2 )]
            public static async Task page2_render( HttpContext context )
            { 

               ...

            }

         }


      --------------------------------------------------
      */
      public routemap( String route_pattern,
                       http_methods allowed_http_methods = http_methods.GET | http_methods.POST,
                       Int32 order = 0,
                       [CallerFilePath] String file_path = "",
                       [CallerMemberName] String member_name = "",
                       [CallerLineNumber] Int32 line_number = 0 )
      {

         // A route pattern is required ...

         ArgumentNullException.ThrowIfNullOrWhiteSpace( route_pattern );

         m_route_pattern = route_pattern;

         m_http_methods = allowed_http_methods;

         if ( order >= 0 )
         {

            m_order = order;

         }
         else
         {

            m_order = 0;

         }

         m_file_path = file_path;

         m_member_name = member_name;

         m_line_number = line_number;

      }


      /*
      --------------------------------------------------

      Returns the route pattern for this routemap.

      --------------------------------------------------
      */
      public virtual String route_pattern
      {

         get
         {

            return m_route_pattern;

         }

      }


      /*
      --------------------------------------------------

      Returns the allowed HTTP methods enumeration for 
      this routemap.

      --------------------------------------------------
      */
      public virtual http_methods allowed_http_methods
      {

         get
         {

            return m_http_methods;

         }

      }


      /*
      --------------------------------------------------

      Returns a list of allowed HTTP method verb strings
      for this routemap.

      --------------------------------------------------
      */
      public virtual System.Collections.Generic.List<String> allowed_http_method_list
      {

         get
         {

            String http_verb;

            System.Collections.Generic.List<String> method_list;

            method_list = new System.Collections.Generic.List<String>();


            // Loop through http_methods enum to see which flags are set ...

            foreach ( Enum method_val in Enum.GetValues( m_http_methods.GetType() ) )
            {

               // If a flag is set, add HTTP verb name to list ...

               if ( m_http_methods.HasFlag( method_val ) )
               {

                  http_verb = method_val.ToString().ToUpper();

                  method_list.Add( http_verb );

               }

            }

            return method_list;

         }

      }


      /*
      --------------------------------------------------

      Returns the order for the routemap.  Endpoints 
      with a lower numeric value of order have higher 
      priority.

      --------------------------------------------------
      */
      public virtual Int32 order
      {

         get
         {

            return m_order;

         }

      }


      /*
      --------------------------------------------------

      Returns the source file path for this routemap.

      --------------------------------------------------
      */
      public virtual String file_path
      {

         get
         {

            return m_file_path;

         }

      }


      /*
      --------------------------------------------------

      Returns the member name (handler) for this routemap.

      --------------------------------------------------
      */
      public virtual String member_name
      {

         get
         {

            return m_member_name;

         }

      }


      /*
      --------------------------------------------------

      Returns the source line number for this routemap.

      --------------------------------------------------
      */
      public virtual Int32 line_number
      {

         get
         {

            return m_line_number;

         }

      }

   }


   /*
   ==================================================

   A helper class used to hold data associated with a
   routemap pattern.

   ==================================================
   */
   public class routemap_data
   {

      // The name of the method invoked for a routemap ...

      private readonly String m_method_name;

      // The routemap attribute object ...

      private readonly routemap m_routemap_attribute;


      /*
      --------------------------------------------------

      Constructor

      --------------------------------------------------
      */
      public routemap_data( String method_name, routemap routemap_attribute )
      {

         m_method_name = method_name;

         m_routemap_attribute = routemap_attribute;

      }


      /*
      --------------------------------------------------

      Returns a string containing the method name that 
      is invoked to handle a routemap pattern / path ...

      --------------------------------------------------
      */
      public String method_name
      {
         get 
         {

            return m_method_name;
         
         }

      }


      /*
      --------------------------------------------------

      Returns a [routemap] attribute object ...

      --------------------------------------------------
      */
      public routemap routemap_attribute
      {

         get
         {

            return m_routemap_attribute;

         }

      }

   }


   /*
   ==================================================

   Routemap endpoints class used to add endpoints for
   all methods that have one or more [routemap] 
   declarations associated with them.

   ==================================================
   */
   public class routemap_endpoints
   {

      /*
      A dictionary used to keep track of the routemap's
      created; key is the route pattern ...
      */

      private readonly System.Collections.Generic.Dictionary<String, routemap_data> m_routemap_dict;


      /*
      --------------------------------------------------

      Constructor.

      --------------------------------------------------
      */
      public routemap_endpoints()
      {

         m_routemap_dict = new System.Collections.Generic.Dictionary<String, routemap_data>();

      }


      /*
      --------------------------------------------------

      Writes all routemaps to the given output file.
      Useful when debugging.

      --------------------------------------------------
      */
      public void log_to_file( String output_file_spec )
      {

         StringBuilder sb;

         routemap_data routemap_data;

         routemap routemap_attribute;


         // Exit if no output file spec supplied ...

         if ( String.IsNullOrEmpty( output_file_spec ) )
         {

            return;

         }


         // Build list of routemap endpoints ...

         sb = new StringBuilder();

         sb.AppendLine( $@"Number of [routemap] endpoints defined: {m_routemap_dict.Count}" );

         foreach ( String route_pattern_key in m_routemap_dict.Keys )
         {

            routemap_data = m_routemap_dict[ route_pattern_key ];

            routemap_attribute = routemap_data.routemap_attribute;

            sb.AppendLine( "" );

            sb.AppendLine( $@"Route pattern: {routemap_attribute.route_pattern}" );

            foreach ( String http_method_verb in routemap_attribute.allowed_http_method_list )
            {

               sb.AppendLine( $@"- HTTP method: {http_method_verb}" );

            }

            sb.AppendLine( $@"- Order: {routemap_attribute.order}" );

            sb.AppendLine( $@"- Mapped to method: {routemap_data.method_name}" );

            sb.AppendLine( $@"- Source file: {routemap_attribute.file_path}, line number: {routemap_attribute.line_number}" );

         }


         // Write to output file ...

         try
         {

            System.IO.File.WriteAllText( output_file_spec, sb.ToString() );

         }
         catch ( Exception ex )
         {

            throw new Exception( $@"* Error writing to output file: {output_file_spec}, {ex}" );

         }

      }


      /*
      --------------------------------------------------

      Returns the defined routemaps as a dictionary of
      route patterns (the Key) and routemap_data objects.

      --------------------------------------------------
      */
      public System.Collections.Generic.Dictionary<String, routemap_data> routemaps
      {

         get
         {

            return m_routemap_dict;

         }

      }


      /*
      --------------------------------------------------

      Given a MethodInfo object, return a string that 
      contains the declaring type and method name, that
      is, the fully qualified method name ...

      NOTE: This will throw an exception if the given
            MethodInfo objects DeclaringType property
            is null.

      --------------------------------------------------
      */
      private static String qualified_method_name( MethodInfo method_info )
      {

         if ( method_info.DeclaringType is null )
         {

            throw new ArgumentException( $"{nameof( method_info )} does not have a declaring type." );

         }

         return method_info.DeclaringType.FullName + "." + method_info.Name;

      }


      /*
      --------------------------------------------------

      Returns a list of MethodInfo objects for all methods
      within the executing assembly that have one or more
      [routemap] declarations.

      --------------------------------------------------
      */
      private static System.Collections.Generic.List<MethodInfo> methods_with_routemap_attributes()
      {

         String method_name;

         ParameterInfo[] parameter_info_array;

         System.Reflection.MethodInfo[] method_info_array;

         System.Type[] assembly_types_array;

         System.Collections.Generic.List<MethodInfo> method_info_list;


         // Init ...

         method_info_list = new System.Collections.Generic.List<MethodInfo>();


         // Find all the methods that have [routemap] attribute(s) ...

         assembly_types_array = Assembly.GetExecutingAssembly().GetTypes();

         foreach ( Type t in assembly_types_array )
         {

            // If the type is a class or delegate, then get its methods ...

            if ( t.IsClass )
            {

               method_info_array = t.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );

               // Loop through the methods and save any methods with a [routemap] attribute in our list ...

               foreach ( MethodInfo method_info in method_info_array )
               {

                  if ( System.Attribute.GetCustomAttributes( method_info, typeof( routemap ) ).Length > 0 )
                  {

                     // Get qualified method name for error reporting ...

                     method_name = qualified_method_name( method_info );

                     // If the method has a single parameter of type HttpContext, save in our list ...

                     parameter_info_array = method_info.GetParameters();

                     if ( parameter_info_array.Length == 1 && parameter_info_array[ 0 ].ParameterType == typeof( Microsoft.AspNetCore.Http.HttpContext ) )
                     {

                        method_info_list.Add( method_info );

                     }
                     else
                     {

                        throw new InvalidOperationException( $@"Method: {method_name}, must have a single parameter of type: Microsoft.AspNetCore.Http.HttpContext" );

                     }

                  }

               }

            }

         }

         return method_info_list;

      }


      /*
      --------------------------------------------------

      Given a list of MethodInfo objects, return a 
      dictionary of fully qualified method names (key)
      and the associated route handler (value) for each.

      --------------------------------------------------
      */
      private static System.Collections.Generic.Dictionary<String, Microsoft.AspNetCore.Http.RequestDelegate> route_handlers_create( System.Collections.Generic.List<MethodInfo> method_info_list )
      {

         object class_object;

         String method_name;

         System.Collections.Generic.Dictionary<String, Microsoft.AspNetCore.Http.RequestDelegate> method_name_dict;

         /*
         The instance list is used to keep track of instances created when creating
         Microsoft.AspNetCore.Http.RequestDelegate objects for methods in non-static
         classes.  We need to keep track of the instances so that only a single instance 
         will be created in the case where an instance has multiple methods that have
         [routemap] attributes.  If we did not do this, a new instance would be created
         each time!
         */

         System.Collections.Generic.List<object> instance_list;

         Microsoft.AspNetCore.Http.RequestDelegate route_handler;


         // method_info_list is required ...

         ArgumentNullException.ThrowIfNull( method_info_list );


         // Init ...

         instance_list = new System.Collections.Generic.List<object>();

         method_name_dict = new System.Collections.Generic.Dictionary<String, Microsoft.AspNetCore.Http.RequestDelegate>();


         // Loop to create a route handler for each method in the list ...

         foreach ( MethodInfo method_info in method_info_list )
         {

            // Get fully qualified method name (for exception reporting and dictionary key) ...

            method_name = qualified_method_name( method_info );

            route_handler = null;

            /*
            Tried using RequestDelegateFactory.Create to create the RequestDelegate, for example:

              route_handler = RequestDelegateFactory.Create( method_info ).RequestDelegate;

            It works, BUT for methods of a non-static class, it will create a new instance
            of the class EACH TIME it is called (calls Activator.CreateInstance internally).
            Using the MethodInfo.CreateDelegate method works and allows us to create a single 
            instance instead.
            */

            // If a static method, create a delegate that can process an HTTP request ...

            if ( method_info.IsStatic )
            {

               try
               {

                  route_handler = method_info.CreateDelegate<Microsoft.AspNetCore.Http.RequestDelegate>();

               }
               catch ( Exception ex )
               {

                  throw new Exception( $"* Unable to create delegate for [routemap] static method: {method_name}, {ex}" );

               }

            }
            else
            {

               /*
               For a non-static method, we first check if an instance of the class
               that contains the method exists in our list.  If so, then we use 
               the instance to create the delegate ...
               */

               try
               {

                  class_object = null;

                  foreach ( object instance in instance_list )
                  {

                     if ( instance.GetType() == method_info.DeclaringType )
                     {

                        class_object = instance;

                        break;

                     }

                  }

                  /*
                  If a class object (instance) was not found above, it means we need to 
                  create an instance of the class that contains the given method, so 
                  attempt to do so ...
                  */

                  if ( class_object is null )
                  {

                     // Create an instance of the class; this will trigger the class constructor ...

                     class_object = System.Activator.CreateInstance( method_info.DeclaringType );

                     // Add the class instance in our list ...

                     instance_list.Add( class_object );

                  }

                  // Create the route handler ...

                  route_handler = method_info.CreateDelegate<Microsoft.AspNetCore.Http.RequestDelegate>( class_object );

               }
               catch ( Exception ex )
               {

                  throw new Exception( $"* Unable to create delegate for [routemap] method: {method_name}, {ex}" );

               }

            }

            // Save the method name and route handler in our dictionary ...

            method_name_dict.Add( method_name, route_handler );

         }

         return method_name_dict;

      }


      /*
      --------------------------------------------------

      Given a routemap attribute object, returns a string
      that contains the routemap order and route pattern,
      which we use as a key into our routemap dictionary
      (m_routemap_dict).

      --------------------------------------------------
      */
      private static String routemap_pattern_key( routemap routemap_attribute )
      {

         return $@"{routemap_attribute.order}_{routemap_attribute.route_pattern}";

      }


      /*
      --------------------------------------------------

      Given an IEndpointRouteBuilder, this method will
      add endpoints for all methods that have one or 
      more [routemap] declarations.

      --------------------------------------------------
      */
      public void add( Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints )
      {

         String key;
         String method_name;

         System.Attribute[] attribute_array;

         System.Collections.Generic.List<MethodInfo> method_info_list;

         System.Collections.Generic.Dictionary<String, Microsoft.AspNetCore.Http.RequestDelegate> method_name_dict;

         Microsoft.AspNetCore.Http.RequestDelegate route_handler;

         routemap_data routemap_data;


         // endpoints interface is required ...

         ArgumentNullException.ThrowIfNull( endpoints );


         // Find all methods with [routemap] attributes ...

         method_info_list = methods_with_routemap_attributes();


         // Create a route handler for each method ...

         method_name_dict = route_handlers_create( method_info_list );



         /*
         Using the [routemap] attribute(s) associated with each method,
         add an endpoint for each route pattern ...
         */

         foreach ( MethodInfo method_info in method_info_list )
         {

            // Get the fully qualified method name from the MethodInfo object,
            // this serves as the key into our method_name_dict dictionary ...

            method_name = qualified_method_name( method_info );


            // Get the route handler (RequestDelegate) for this method ...

            route_handler = method_name_dict[ method_name ];


            // Get all the [routemap] attributes associated with this method ...

            attribute_array = System.Attribute.GetCustomAttributes( method_info, typeof( routemap ), true );

            foreach ( routemap attribute in attribute_array )
            {

               // Internal consistency check for duplicate route patterns ...

               key = routemap_pattern_key( attribute );

               if ( m_routemap_dict.ContainsKey( key ) )
               {

                  throw new Exception( $"Method {method_name} contains a duplicate route pattern: {attribute.route_pattern} in {attribute.file_path} on line {attribute.line_number}" );

               }


               /*
               Map the route pattern, HTTP methods and route handler ...
               If the order is greater than zero, configure the EndpointBuilder 
               via a convention, see: https://github.com/dotnet/aspnetcore/issues/39241
               */

               if ( attribute.order > 0 )
               {

                  endpoints.MapMethods( attribute.route_pattern, attribute.allowed_http_method_list, route_handler ).Add( builder => ( ( RouteEndpointBuilder )builder ).Order = attribute.order );

               }
               else
               {

                  endpoints.MapMethods( attribute.route_pattern, attribute.allowed_http_method_list, route_handler );

               }

               // Save the method name and routemap data in our dictionary ...

               routemap_data = new routemap_data( method_name, attribute );

               m_routemap_dict.Add( key, routemap_data );

            }

         }

      }

   }

   /*
   Extension method used to add [routemap] endpoints - that is, 
   those methods that have one or more [routemap] atributes.

   USAGE:

   In the file Startup.cs, add the following lines to the 
   Configure() method in the following order:

   public void Configure( IApplicationBuilder app )
   {

      
      app.UseRouting();

      // other ...

      app.use_rmp();

   }

   */
   public static class routemap_extensions
   {
      public static void add_rmp( this IServiceCollection services )
      {

         ArgumentNullException.ThrowIfNull( services );

         services.AddSingleton<routemap_endpoints>();

      }

      // .Net 6.0+ ...
      public static void use_rmp( this WebApplication app )
      {

         ArgumentNullException.ThrowIfNull( app );

         map_endpoints( app );

      }

      // .Net 5 or 6 ...
      public static IApplicationBuilder use_rmp( this IApplicationBuilder builder )
      {

         return builder is null ? throw new ArgumentNullException( nameof( builder ) ) :  builder.UseEndpoints( map_endpoints );

      }

      private static void map_endpoints( this IEndpointRouteBuilder endpoints )
      {

         ArgumentNullException.ThrowIfNull( endpoints );

         // Get routemap_endpoints instance ...

         routemap_endpoints routemap_endpoints = endpoints.ServiceProvider.GetService<routemap_endpoints>();

         if ( routemap_endpoints is null )
         {

            throw new Exception( "Did you forget to call Services.add_rmp()?" );

         }

         // Add all [routemap] endpoints ...

         routemap_endpoints.add( endpoints );


         // DEBUGGING - log all routemap endpoints to a file ...
         //routemap_endpoints.log_to_file( @"D:\temp\website_endpoints.txt" );

      }

   }

}
