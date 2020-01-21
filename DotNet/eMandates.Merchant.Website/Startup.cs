using System;
using System.Diagnostics;
using System.Threading.Tasks;
using eMandates.Merchant.Library.AppConfig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eMandates.Merchant.Website
{
    public class Startup
    {
        private readonly string _contentRoot;

        public Startup(Microsoft.Extensions.Configuration.IConfiguration configuration, IHostingEnvironment env) : this(configuration)
        {
            _contentRoot = env.WebRootPath;
        }

        private Startup(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMvcCore()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Insert(0, new DecimalModelBinder());
                });

            services.AddOptions();
            services.Configure<ApplicationSettings>(Configuration.GetSection("eMandates.Merchant.Library.Settings"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware<IgnoreRouteMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            Trace.Listeners.Add(new CustomTraceListener(_contentRoot));
        }
    }

    public class IgnoreRouteMiddleware
    {

        private readonly RequestDelegate next;

        // You can inject a dependency here that gives you access
        // to your ignored route configuration.
        public IgnoreRouteMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue &&
                context.Request.Path.Value.Contains(".axd/"))
            {

                context.Response.StatusCode = 404;

                Console.WriteLine("Ignored!");

                return;
            }

            await next.Invoke(context);
        }
    }
}
