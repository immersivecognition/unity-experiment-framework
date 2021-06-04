from flask import Flask, request
from flask_httpauth import HTTPBasicAuth
from flask_cors import CORS
from werkzeug.security import generate_password_hash, check_password_hash
import os

# where data will be stored
OUTPUT_DIR = 'example_output'

# generate username/passwords
users = {
    "susan": generate_password_hash("hello"),
}

# create the flask application
app = Flask(__name__)

# for username/password support
auth = HTTPBasicAuth()

# for Cross Origin Resource Sharing (required for WebGL builds)
# read more here: https://docs.unity3d.com/Manual/webgl-networking.html
CORS(app)

@app.route('/form', methods=['POST'])
@auth.login_required
def form():
    """
    POST request handler that accepts the data coming in and saves it to disk.
    """

    filepath = request.form["filepath"]
    data = request.form["data"]

    fullpath = os.path.join(OUTPUT_DIR, filepath)
    directory, filename = os.path.split(fullpath)
    os.makedirs(directory, exist_ok=True)

    try:
        with open(fullpath, 'w+') as f:
            f.write(data)
            print(f"Wrote data to {fullpath}.")
        return app.response_class(status=200)
    except:
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
