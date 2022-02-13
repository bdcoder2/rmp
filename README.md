<h1>Routemap pages (rmp) for .Net websites WITHOUT MVC or Razor</h1>

Illustrates defining routemaps for website page handlers
WITHOUT the need of MVC or Razor pages.

<h2>The [routemap] attribute</h2>

One or more **[routemap]** attributes are placed above any method in 
a static or instance class that can process an HTTP request.  Each
routemap pattern must be unique.  If a duplicate routemap pattern is
detected an exception will be thrown.

```    
[routemap( "{route-pattern}", [ http_methods ] )]
```

<h3>Parameters</h3>

<dl>
    <dt>route-pattern</dt>
  <dd>Required. The <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-5.0" target="_blank">route-pattern</a> 
      defines an endpoint.  An endpoint is something that can be selected, by matching the URL and HTTP method(s).  
      See <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-5.0#route-template-reference" target="_blank">route template reference</a> for more information.  Examples of route-patterns:
      
  </dd>
  <dt>http_methods</dt>
  <dd>
      Optional.  Used to indicate the allowed HTTP methods for the given route-pattern.  
      Valid HTTP method verb are: CONNECT, DELETE, GET, HEAD, OPTIONS, POST, PUT and TRACE.      
      If no methods are provided, then the HTTP GET and POST methods are used by default.  
      Multiple HTTP methods can be specified.  Examples of HTTP methods for various routemaps:
  </dd>
</dl>

<h3>Examples</h3>

```    

// Match the URL /do_get for HTTP GET requests only ...
[routemap( "/do_get", routemap.http_methods.GET )]
    
// Match the URL /do_delete for HTTP DELETE requests only ...
[routemap( "/do_delete", routemap.http_methods.DELETE )]
    
// Match the URL /do_something for HTTP GET, POST and HEAD requests ...
[routemap( "/do_something", routemap.http_methods.GET | routemap_http_methods.POST | routemap.http_methods.HEAD )]
    
 ```

    
<h2>USAGE</h2>

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
