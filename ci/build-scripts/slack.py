import logging
import os
import glob
from slack_sdk import WebClient
from slack_sdk.errors import SlackApiError

def send(channel_id):
    client = WebClient(token=os.environ.get("SLACK_BOT_TOKEN"))
    logger = logging.getLogger(__name__)
    files = glob.glob("./Setup/Installers/*.exe")

    if len(files) > 0:
        installer_path = files[0]
        try:
            result = client.files_upload_v2(
                channel=channel_id,
                initial_comment=os.environ.get("CI_COMMIT_MESSAGE"),
                file=installer_path,
            )
            logger.info(result)

        except SlackApiError as e:
            logger.error("Error uploading file: {}".format(e))