def createConfig(path, dsn):
    if dsn:
      dsn = "\"{dsn}\"".format(dsn=dsn)
    else:
      dsn = "string.Empty"

    content = """namespace ProtonVPN.Common.Configuration
{{
    public class GlobalConfig
    {{
        public static string SentryDsn => {dsn};
    }}
}}""".format(dsn=dsn)

    f = open(path, 'w')
    f.write(content)
    f.close()
