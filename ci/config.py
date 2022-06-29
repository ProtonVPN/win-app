import os

def getSentryDsn():
    dsn=os.environ.get("SENTRY_DSN_V2")
    if dsn:
      return "\"{dsn}\"".format(dsn=dsn)
    else:
      return "string.Empty"

def getInternalReleaseUpdateUrl():
    url=os.environ.get("INTERNAL_RELEASE_URL")
    if url:
      return "\"{url}\"".format(url=url)
    else:
      return "string.Empty"

def createGlobalConfig(filePath):
    dsn = getSentryDsn()
    internalReleaseUrl = getInternalReleaseUpdateUrl()
    content = """namespace ProtonVPN.Common.Configuration
{{
    public class GlobalConfig
    {{
        public static string SentryDsn => {dsn};
        public static string InternalReleaseUpdateUrl => {internalReleaseUrl};
    }}
}}""".format(dsn=dsn, internalReleaseUrl=internalReleaseUrl)

    f = open(filePath, 'w')
    f.write(content)
    f.close()