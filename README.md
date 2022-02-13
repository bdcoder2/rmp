# Routemap pages (rmp) for .Net websites WITHOUT MVC or Razor

Illustrates defining routemaps for website page handlers
WITHOUT the need of MVC or Razor pages.

## The [routemap] Attribute

One or more **[routemap]** attributes are placed above any method in 
a static or instance class that can process an HTTP request.  Each
routemap pattern must be unique.  If a duplicate routemap pattern is
detected an exception will be thrown.

```    
[routemap( "{route-pattern}", [ http_methods ] )]
```

The **[route pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-5.0)** is 
required.

The **http_methods** is optional and is used to indicate the allowed HTTP methods for the given route-pattern.  If no 
methods are provided, then the HTTP GET and POST methods are used by default.  Multiple HTTP methods can be specified.

Valid HTTP method verb are:

- CONNECT
- DELETE
- GET
- HEAD
- OPTIONS
- POST
- PUT
- TRACE

Some examples of setting HTTP methods for various routemaps:
    
    ```    
    // Handle HTTP GET requests only ...
    [routemap( "/do_get", routemap.http_methods.GET )]
    
    // Handle HTTP DELETE requests only ...
    [routemap( "/do_delete", routemap.http_methods.DELETE )]
    
     // Handle HTTP GET, POST and HEAD requests ...
    [routemap( "/do_something", routemap.http_methods.GET | routemap_http_methods.POST | routemap.http_methods.HEAD )]
    ```
    
## USAGE

 1. Create an empty .Net website project and include
    the file rpm.cs from this repository in your project (or simply
    clone this respository).

 2. Include the following using statement in any 
    file that uses the **[routemap]** attribute:

    ```
    using rmp;
    ```

    For example, the class "my_pages" shown below contains
    two two methods "page1_render" and "page2_render", each
    of which handles HTTP requests.
    
    
    ```
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using rmp;

    public class my_pages
    {
      
       private int m_page1_render_count;
       private int m_page2_render_count;

       public my_pages()
       {

          int m_page1_render_count = 0;

          int m_page2_render_count = 0;

       }


       [routemap( "/" ) ]
       private async Task page1_render( HttpContext http_context )
       {

          m_page1_render_count++;

          await http_context.Response.WriteAsync( $@"Page 1, render count: {m_page1_render_count} );

          return;

       }


       [routemap( "/page2" ) ]
       private async Task page2_render( HttpContext http_context )
       {

          m_page2_render_count++;

          await http_context.Response.WriteAsync( $@"Page 2, render count: {m_page2_render_count} );

          return;

       }

    }
    ```

 3. In the **Startup** class, modify the **ConfigureServices** and
    **Configure** methods to include the routemap pages service, as
    shown below:

    ```
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using rmp;

    public void ConfigureServices( IServiceCollection services )
    {

       // Add routemap pages service ...

       services.add_rmp();

    }
    
    
    public void Configure( IApplicationBuilder app )
    {

       // Use Routing (required) ...

       app.UseRouting();


       // Use routemap pages ...

       app.use_rmp();

    }
    ```

4. Build and launch the website.  The [routemap] patterns
   defined will be mapped to the appropriate handler, for 
   example:

   - / => will invoke the my_pages.page1_render method

   - /page2 => will invoke the my_pages.page2_render method
