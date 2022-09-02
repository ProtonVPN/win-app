import os

def getInternalReleaseUpdateUrl():
    url=os.environ.get("INTERNAL_RELEASE_URL")
    if url:
      return "\"{url}\"".format(url=url)
    else:
      return "string.Empty"