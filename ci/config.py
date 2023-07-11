import os

def getInternalReleaseUpdateUrl():
    url = os.environ.get("INTERNAL_RELEASE_URL")
    return "\"{url}\"".format(url = url) if url else "string.Empty"
