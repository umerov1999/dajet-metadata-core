using DaJet.Data;
using DaJet.Http.DataMappers;
using DaJet.Http.Model;
using DaJet.Metadata;
using Microsoft.Extensions.FileProviders;

namespace DaJet.Http.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationOptions options = new()
            {
                Args = args,
                ContentRootPath = AppContext.BaseDirectory
            };

            var builder = WebApplication.CreateBuilder(options);

            builder.Host.UseSystemd();
            builder.Host.UseWindowsService();

            ConfigureServices(builder.Services);
            ConfigureFileProvider(builder.Services);

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.MapControllers();

            //app.MapWhen(RouteToBlazor, builder =>
            //{
            //    builder.UseBlazorFrameworkFiles();
            //    builder.UseStaticFiles();

            //    builder.UseRouting();
            //    builder.UseEndpoints(endpoints =>
            //    {
            //        endpoints.MapFallbackToFile("{*path:nonfile}", "index.html");
            //    });
            //});

            //app.MapWhen(RouteToMetadataService, builder =>
            //{
            //    app.UseRouting();
            //    app.UseAuthorization();
            //    app.UseEndpoints(endpoints =>
            //    {
            //        endpoints.MapControllers();
            //    });
            //});

            app.Run();
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            MetadataService metadataService = new();

            InfoBaseDataMapper mapper = new();
            List<InfoBaseModel> list = mapper.Select();
            foreach (InfoBaseModel entity in list)
            {
                if (!Enum.TryParse(entity.DatabaseProvider, out DatabaseProvider provider))
                {
                    provider = DatabaseProvider.SqlServer;
                }

                metadataService.Add(new InfoBaseOptions()
                {
                    Key = entity.Name,
                    DatabaseProvider = provider,
                    ConnectionString = entity.ConnectionString
                });
            }

            services.AddSingleton<IMetadataService>(metadataService);
        }
        private static void ConfigureFileProvider(IServiceCollection services)
        {
            string catalogPath = AppContext.BaseDirectory;
            
            PhysicalFileProvider fileProvider = new(catalogPath);

            services.AddSingleton<IFileProvider>(fileProvider);
        }
    }
}