using FirebirdSql.Data.FirebirdClient;
using Gcef.CustomProvider.Native;
using System.Data;
using System.Reflection;

namespace FirebirdProvider
{
    public class FirebirdProvider : Gcef.CustomProvider.Native.INativeQueryDataProvider
    {
        public static string ProviderName => "Firebird";

        public static void Configure(IFeatureCollection features)
        {
            features.Metadata().DisplayName = ProviderName;
            features.Metadata().Description = "This is Firebird provider.";

            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (Path.Combine(assemblyDirectory!, "UserGuide.md") is var userGuidePath && File.Exists(userGuidePath))
            {
                features.UserGuide().UserGuideMarkdown = File.ReadAllText(userGuidePath);
            }

            if (Path.Combine(assemblyDirectory!, "firebird_16x16.png") is var smallIconPath && File.Exists(smallIconPath))
            {
                features.Metadata().SmallIcon = GetDataURL(smallIconPath);
            }

            if (Path.Combine(assemblyDirectory!, "firebird_180x130.png") is var largeIconPath && File.Exists(largeIconPath))
            {
                features.Metadata().LargeIcon = GetDataURL(largeIconPath);
            }

            static string GetDataURL(string imgFilePath)
            {
                return "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(imgFilePath));
            }

            features.Get<IParameterParseFeature>().NeedFillParameters = false;
        }

        public static INativeQueryDataProvider CreateInstance() => new FirebirdProvider();

        public async Task ExecuteAsync(INativeQuery nativeQuery, Action<IDataReader> readerConsumer, params NativeParameter[] parameters)
        {
            using var connection = new FbConnection(nativeQuery.ConnectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = nativeQuery.QueryText;
            foreach (var parameter in parameters)
            {
                var para = command.CreateParameter();
                para.ParameterName = parameter.Name;
                para.Value = parameter.ParameterValue;
                command.Parameters.Add(para);
            }
            readerConsumer(await command.ExecuteReaderAsync());
        }

        public async Task TestConnectionAsync(string connectionString)
        {
            using var connection = new FbConnection(connectionString);
            await connection.OpenAsync();
        }
    }
}