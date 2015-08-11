namespace EfEnumToLookup.LookupGenerator
{
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class ContextExtensions
    {
        public static string GetTableName<T>(this DbContext context) where T : class
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;

            return objectContext.GetTableName<T>();
        }

        public static string GetTableName<T>(this ObjectContext context) where T : class
        {
            string sql = context.CreateObjectSet<T>().ToTraceString();
            Regex regex = new Regex("FROM (?<table>.*) AS");
            Match match = regex.Match(sql);

            string table = match.Groups["table"].Value;
            return table;
        }

        public static string GetDefaultSchema(this DbContext context)
        {
            ////var table = (((IObjectContextAdapter)context).ObjectContext).MetadataWorkspace.GetItems(DataSpace.SSpace).FirstOrDefault();

            var table = (((IObjectContextAdapter)context).ObjectContext).MetadataWorkspace.GetItemCollection(DataSpace.SSpace)
                .GetItems<EntityContainer>()
                .Single()
                .BaseEntitySets
                .OfType<EntitySet>()
                .SingleOrDefault(s => !s.MetadataProperties.Contains("Type") || s.MetadataProperties["Type"].ToString() == "Tables");

            if(table == null)
            {
                return "dbo";
            }

          return table.MetadataProperties["Schema"].Value.ToString();
        }
    }
}
