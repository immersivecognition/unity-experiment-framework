from flask import Flask, request
from flask_httpauth import HTTPBasicAuth
from flask_cors import CORS
from werkzeug.security import generate_password_hash, check_password_hash
from pathlib import Path
import os

# where data will be stored
OUTPUT_DIR = Path(os.environ.get('UXF_OUTPUT_DIR', 'example_output')).resolve()

# The sample deliberately reads credentials from the environment. Never commit
# a real participant-data password to this file or expose the development server
# directly to the public internet.
USERNAME = os.environ.get('UXF_USERNAME', '')
PASSWORD = os.environ.get('UXF_PASSWORD', '')

# generate username/passwords
users = {USERNAME: generate_password_hash(PASSWORD)} if USERNAME and PASSWORD else {}

# create the flask application
app = Flask(__name__)

# for username/password support
auth = HTTPBasicAuth()

# for Cross Origin Resource Sharing (required for WebGL builds)
# read more here: https://docs.unity3d.com/Manual/webgl-networking.html
CORS(app, origins=os.environ.get('UXF_ALLOWED_ORIGINS', '*').split(','))


def safe_output_path(relative_path):
    """Resolve a submitted relative path while keeping it below OUTPUT_DIR."""
    if not relative_path or os.path.isabs(relative_path):
        raise ValueError('filepath must be a non-empty relative path')

    candidate = (OUTPUT_DIR / relative_path).resolve()
    try:
        candidate.relative_to(OUTPUT_DIR)
    except ValueError:
        raise ValueError('filepath escapes the configured output directory')
    return candidate

@app.route('/form', methods=['POST'])
@auth.login_required
def form():
    """
    POST request handler that accepts the data coming in and saves it to disk.
    """

    filepath = request.form.get("filepath", "")
    data = request.form.get("data")
    if data is None:
        return app.response_class('missing data field', status=400)

    try:
        fullpath = safe_output_path(filepath)
    except ValueError as error:
        return app.response_class(str(error), status=400)

    fullpath.parent.mkdir(parents=True, exist_ok=True)

    try:
        with open(fullpath, 'w', encoding='utf-8', newline='') as f:
            f.write(data)
            print(f"Wrote data to {fullpath}.")
        return app.response_class(status=200)
    except OSError:
        return app.response_class(status=500)


@auth.verify_password
def verify_password(username, password):
    if username in users and \
            check_password_hash(users.get(username), password):
        return username



@app.route('/')
@auth.login_required
def index():
    """
    Basic Hello World at the index.
    """
    return "Hello, {}!".format(auth.current_user())


if __name__ == '__main__':
    app.run()
