# Routemap pages (rmp) for .Net websites WITHOUT MVC OR RAZOR

 A [routemap] attribute is placed above any method
 in a static or instance class that can process an 
 HTTP request.

## USAGE

 1. Create an empty .Net website project and include
    the file rpm.cs from this repository in your project.

 2. Include the following using statement in any 
    file that uses the **[routemap]** attribute:

    ```
    using rmp;
    ```

    For example, the class "my_pages" shown below contains
    two two methods "page1_render" and "page2_render", each
    of which handles HTTP requests.
    
    A **[routemap]** attribute is placed above each method 
    and includes the [route pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-5.0) 
    used to invoke each method:
    
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


       [routemap( "/page1" ) ]
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

   - /page1 => will invoke the my_pages.page1_render method

   - /page2 => will invoke the my_pages.page2_render method
