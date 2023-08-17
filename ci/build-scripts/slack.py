import logging
import os
import glob
from slack_sdk import WebClient
from slack_sdk.errors import SlackApiError

def send():
    client = WebClient(token=os.environ.get("SLACK_BOT_TOKEN"))
    logger = logging.getLogger(__name__)
    files = glob.glob("./Setup/Installers/*.exe")

    if len(files) > 0:
        installer_path = files[0]
        try:
            result = client.files_upload(
                channels=os.environ.get("SLACK_CHANNEL_ID"),
                initial_comment=os.environ.get("CI_COMMIT_MESSAGE"),
                file=installer_path,
            )
            logger.info(result)

        except SlackApiError as e:
            logger.error("Error uploading file: {}".format(e))