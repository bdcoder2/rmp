# Routemap pages (rmp) for .Net websites WITHOUT MVC or Razor

Illustrates using routemap attributes for website endpoints WITHOUT the need of MVC or Razor.

## The [routemap] attribute

One or more **[routemap]** attributes are placed above any method in 
a static or instance class that can process an HTTP request.  Each
routemap pattern must be unique.  If a duplicate routemap pattern is
detected an exception will be thrown.

### Syntax

```
    [routemap( "{route-pattern}", [http_methods], [order] )]
```

### Parameters

#### route-pattern

Required. The [route-pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0) defines an endpoint. An endpoint 
is something that can be selected, by matching the URL and HTTP method(s) provided.  A route-pattern may contain any number 
of [route-constraints](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0#route-constraints).  See the [route template reference](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0#route-template-reference) for more information.

#### http_methods
Optional.  Used to indicate the allowed HTTP methods for the given route-pattern.  Valid HTTP method verbs are:
- routemap.http_methods.CONNECT
- routemap.http_methods.DELETE
- routemap.http_methods.GET
- routemap.http_methods.HEAD
- routemap.http_methods.OPTIONS
- routemap.http_methods.POST
- routemap.http_methods.PUT
- routemap.http_methods.TRACE
- routemap.http_methods.PATCH

If no methods are provided, then the HTTP GET and POST methods are used by default.  Multiple HTTP methods can be specified by using the logical OR operator (|) between
method verbs, for example, to allow the GET or DELETE methods:
```   
routemap.http_methods.GET | routemap.http_methods.DELETE
```
#### order
Optional. The order for this routemap.  May be any value greater than or equal to zero.  Default is zero.
A lower value will have higher priority.


### Examples

- Match the URL **/do_get** for HTTP GET requests only:
        
```
   [routemap( "/do_get", routemap.http_methods.GET )]
```
        
- Match the URL **/do_delete** for HTTP DELETE requests only:

```
   [routemap( "/do_delete", routemap.http_methods.DELETE )]
```

- Match the URL **/do_something** for HTTP GET, POST and HEAD requests:

```
   [routemap( "/do_something", routemap.http_methods.GET | routemap.http_methods.POST | routemap.http_methods.HEAD )]
```

- Matches the URLs such as **/product/** or **/product/123** for HTTP GET and POST requests.  The **{id:int?}** segment, indicates an optional integer can be supplied after the /product/ segment:

```
   [routemap( "/product/{id:int?}/" )]
```
   
- Matches the URLs such as **/widget/** or **/widget/val1/val2/val3** for HTTP GET requests. The __{*queryvalues}__ segment, with the leading asterisk indicates that this is a wildcard parameter that can accept any additional segments in it:

```
   [routemap( "/widget/{*queryvalues}", routemap.http_methods.GET )]
```  

- Matches the URL **/api/v1/test** for HTTP GET and POST requests.  The **api_test_v2** method will be excuted because the order has a lower value than the **api_test** method.
```
   [routemap( "/api/v1/test", order: 1 )]
   public static async Task api_test_v2( HttpContext context ) { ... }

   [routemap( "/api/v1/test", order: 2 )]
   public static async Task api_test( HttpContext context ) { ... }
```  

### Usage

1. Create an empty .Net website project and include the file **rpm.cs** from this repository in your project (or simply
   clone this respository). For example, the class "my_pages" shown below contains two two methods "page1_render" and  
   "page2_render", each of which handles HTTP requests.

   ```
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


       [routemap( "/" ) ]
       [routemap( "/page1")]
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
   ```

2. Add the routemap pages service and middleware as shown in the example below:

    ```
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

    ```

3. Build and launch the website.  The [routemap] patterns defined will be mapped to the appropriate handler, for 
   example:
   - / => will invoke the my_pages.page1_render method</li>
   - /page1 => will invoke the my_pages.page1 render method</li>_
   - /page2 => will invoke the my_pages.page2_render method</li>
